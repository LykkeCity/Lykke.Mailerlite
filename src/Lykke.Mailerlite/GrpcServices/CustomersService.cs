﻿using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Lykke.Mailerlite.ApiContract;
using Lykke.Mailerlite.Common.Commands;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Lykke.Mailerlite.GrpcServices
{
    public class CustomersService : Customers.CustomersBase
    {
        private readonly ISendEndpointProvider _commandsSender;
        private readonly ILogger<CustomersService> _logger;

        public CustomersService(ISendEndpointProvider commandsSender, ILogger<CustomersService> logger)
        {
            _commandsSender = commandsSender;
            _logger = logger;
        }

        public override async Task<Empty> Create(CreateCustomerRequest request, ServerCallContext context)
        {
            try
            {
                await _commandsSender.Send(new CreateCustomerCommand
                {
                    CustomerId = request.CustomerId,
                    RequestId = request.RequestId,
                    Email = request.Email,
                    Timestamp = request.Timestamp.ToDateTime(),
                    KycState = request.KycState,
                    FromRestrictedArea = request.FromRestrictedArea
                });

                return new Empty();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                throw;
            }
        }

        public override async Task<Empty> UpdateKyc(UpdateCustomerKycRequest request, ServerCallContext context)
        {
            try
            {
                await _commandsSender.Send(new UpdateCustomerKycCommand
                {
                    CustomerId = request.CustomerId,
                    RequestId = request.RequestId,
                    KycState = request.KycState,
                    Timestamp = request.Timestamp.ToDateTime()
                });
                
                return new Empty();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                throw;
            }
        }

        public override async Task<Empty> UpdateDeposit(UpdateCustomerDepositRequest request, ServerCallContext context)
        {
            try
            {
                await _commandsSender.Send(new UpdateCustomerDepositedCommand
                {
                    CustomerId = request.CustomerId,
                    RequestId = request.RequestId,
                });
                
                return new Empty();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, e);
                throw;
            }
        }
    }
}
