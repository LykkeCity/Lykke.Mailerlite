using System;
using System.Threading.Tasks;
using Lykke.Mailerlite.Common.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;
using Lykke.Mailerlite.Common.Domain.Customers;
using Lykke.Mailerlite.Common.Domain.Mailerlite;
using Lykke.Mailerlite.Common.Persistence;
using Swisschain.Extensions.Idempotency;
using Swisschain.Extensions.Idempotency.MassTransit;

namespace Lykke.Mailerlite.Worker.Messaging.Consumers
{
    public class CreateCustomerCommandConsumer : IConsumer<CreateCustomerCommand>
    {
        private readonly IUnitOfWorkManager<UnitOfWork> _unitOfWorkManager;
        private readonly IMailerliteClient _mailerlite;
        private readonly ILogger<CreateCustomerCommandConsumer> _logger;

        public CreateCustomerCommandConsumer(
            IUnitOfWorkManager<UnitOfWork> unitOfWorkManager,
            IMailerliteClient mailerlite,
            ILogger<CreateCustomerCommandConsumer> logger)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _mailerlite = mailerlite;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CreateCustomerCommand> context)
        {
            try
            {
                var command = context.Message;

                await using var unitOfWork = await _unitOfWorkManager.Begin($"CreateCustomerCommand:{command.RequestId}");

                if (!unitOfWork.Outbox.IsClosed)
                {
                    var customer = Customer.Create(command.CustomerId, command.Email);

                    customer.UpdateKycState(command.KycState, command.Timestamp);

                    await _mailerlite.CreateCustomerAsync(command.Email);

                    await _mailerlite.SetCustomerKycAsync(command.Email, command.KycState);

                    await _mailerlite.SetCustomerSubmittedDocumentsAsync(command.Email, false);

                    await _mailerlite.SetCustomerDepositedAsync(command.Email, false);

                    await _mailerlite.SetCustomerRegisteredAsync(command.Email, command.Timestamp);

                    await unitOfWork.Customers.AddOrIgnoreAsync(customer);

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
