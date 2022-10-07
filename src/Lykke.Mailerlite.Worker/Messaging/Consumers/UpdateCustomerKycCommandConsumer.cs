using System;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Mailerlite.Common.Commands;
using Lykke.Mailerlite.Common.Configuration;
using Lykke.Mailerlite.Common.Domain.Mailerlite;
using Lykke.Mailerlite.Common.Persistence;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Extensions.Idempotency;
using Swisschain.Extensions.Idempotency.MassTransit;

namespace Lykke.Mailerlite.Worker.Messaging.Consumers
{
    public class UpdateCustomerKycCommandConsumer : IConsumer<UpdateCustomerKycCommand>
    {
        private readonly MailerliteConfig _mailerliteConfig;
        private readonly IUnitOfWorkManager<UnitOfWork> _unitOfWorkManager;
        private readonly IMailerliteClient _mailerlite;
        private readonly ILogger<UpdateCustomerKycCommandConsumer> _logger;

        public UpdateCustomerKycCommandConsumer(
            MailerliteConfig mailerliteConfig,
            IUnitOfWorkManager<UnitOfWork> unitOfWorkManager,
            IMailerliteClient mailerlite,
            ILogger<UpdateCustomerKycCommandConsumer> logger)
        {
            _mailerliteConfig = mailerliteConfig;
            _unitOfWorkManager = unitOfWorkManager;
            _mailerlite = mailerlite;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UpdateCustomerKycCommand> context)
        {
            try
            {
                var command = context.Message;

                await using var unitOfWork = await _unitOfWorkManager.Begin($"UpdateCustomerKycCommand:{command.RequestId}");

                if (!unitOfWork.Outbox.IsClosed)
                {
                    var customer = await unitOfWork.Customers.GetOrDefault(command.CustomerId);

                    if (customer == null)
                    {
                        _logger.LogWarning($"Customer with Id {command.CustomerId} does not exist (at least yet).");
                        return;
                    }

                    if (!customer.HasEverSubmittedDocuments && command.KycState.ToLower().Contains("jumio"))
                    {
                        customer.UpdateHasEverSubmittedDocuments();
                        
                        await _mailerlite.SetCustomerSubmittedDocumentsAsync(customer.Email, true);
                    }

                    if (customer.KycStateTimestamp < command.Timestamp)
                    {
                        customer.UpdateKycState(command.KycState, command.Timestamp);

                        await _mailerlite.SetCustomerKycAsync(customer.Email, command.KycState);
                    }

                    if (_mailerliteConfig.StatusesToDeleteFromKycReminderGroup.Contains(customer.KycState))
                    {
                        var groupdId = await _mailerlite.FindGroupIdByNameAsync(_mailerliteConfig.KycReminderGroup);
                        if (groupdId.HasValue)
                        {
                            await _mailerlite.DeleteCustomerFromGroupAsync(customer.Email, groupdId.Value);   
                        }
                    }

                    await unitOfWork.Customers.Update(customer);

                    await unitOfWork.Commit();
                }

                await unitOfWork.EnsureOutboxDispatched(context);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                throw;
            }
        }
    }
}
