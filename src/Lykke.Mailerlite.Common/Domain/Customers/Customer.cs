using System;
using System.Collections.Generic;
using Lykke.Mailerlite.Common.Commands;

namespace Lykke.Mailerlite.Common.Domain.Customers
{
    public class Customer
    {
        private Customer(string id, string email)
        {
            Id = id;
            Email = email;
        }
        
        private Customer(string id, string email, string kycState, DateTime? kycStateTimestamp, bool deposited, bool hasEverSubmittedDocuments)
            : this(id, email)
        {
            KycState = kycState;
            KycStateTimestamp = kycStateTimestamp;
            Deposited = deposited;
            HasEverSubmittedDocuments = hasEverSubmittedDocuments;
        }
        
        public string Id { get; }
        public string Email { get; }
        public string KycState { get; private set;  }
        public DateTime? KycStateTimestamp { get; private set; }
        public bool Deposited { get; private set; }
        public bool HasEverSubmittedDocuments { get; private set; }

        public static Customer Create(string id, string email)
        {
            var customer = new Customer(id, email);

            return customer;
        }

        public static Customer Restore(string id, string email, string kycState, DateTime? kycStateTimestamp, bool deposited, bool hasEverSubmittedDocuments)
        {
            var customer = new Customer(id, email, kycState, kycStateTimestamp, deposited, hasEverSubmittedDocuments);

            return customer;
        }

        public void UpdateKycState(string kycState, DateTime kycStateTimestamp)
        {
            KycState = kycState;
            KycStateTimestamp = kycStateTimestamp;
        }

        public void UpdateDeposited()
        {
            Deposited = true;
        }

        public void UpdateHasEverSubmittedDocuments()
        {
            HasEverSubmittedDocuments = true;
        }
    }
}
