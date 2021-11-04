using System;
using System.Threading.Tasks;
using Lykke.Mailerlite.Common.Commands;
using Lykke.Mailerlite.Common.Domain.Mailerlite;
using Lykke.Mailerlite.Common.Persistence;
using MassTransit;
using Microsoft.Extensions.Logging;
using Swisschain.Extensions.Idempotency;
using Swisschain.Extensions.Idempotency.MassTransit;

namespace Lykke.Mailerlite.Worker.Messaging.Consumers
{
    public class UpdateCustomerDepositedCommandConsumer : IConsumer<UpdateCustomerDepositedCommand>
    {
        private readonly IUnitOfWorkManager<UnitOfWork> _unitOfWorkManager;
        private readonly IMailerliteClient _mailerlite;
        private readonly ILogger<UpdateCustomerDepositedCommandConsumer> _logger;

        public UpdateCustomerDepositedCommandConsumer(
            IUnitOfWorkManager<UnitOfWork> unitOfWorkManager,
            IMailerliteClient mailerlite,
            ILogger<UpdateCustomerDepositedCommandConsumer> logger)
        {
            _unitOfWorkManager = unitOfWorkManager;
            _mailerlite = mailerlite;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<UpdateCustomerDepositedCommand> context)
        {
            try
            {
                var command = context.Message;

                await using var unitOfWork = await _unitOfWorkManager.Begin($"UpdateCustomerDepositedCommand:{command.RequestId}");

                if (!unitOfWork.Outbox.IsClosed)
                {
                    var customer = await unitOfWork.Customers.GetOrDefault(command.CustomerId);

                    if (customer == null)
                        throw new InvalidOperationException($"Customer with Id {command.CustomerId} does not exist (at least yet).");

                    if (customer.Deposited)
                        return;
                    
                    customer.UpdateDeposited();

                    await _mailerlite.SetCustomerDepositedAsync(customer.Email, true);

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
