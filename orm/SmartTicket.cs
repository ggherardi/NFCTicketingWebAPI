using System;
using System.Collections.Generic;

#nullable disable

namespace NFCTicketingWebAPI
{
    public partial class SmartTicket
    {
        public SmartTicket()
        {
            CreditTransactions = new HashSet<CreditTransaction>();
            Validations = new HashSet<Validation>();
        }

        public string CardId { get; set; }
        public decimal Credit { get; set; }
        public string TicketType { get; set; }
        public DateTime? CurrentValidation { get; set; }
        public DateTime? SessionValidation { get; set; }
        public DateTime UsageTimestamp { get; set; }
        public decimal? SessionExpense { get; set; }
        public string Username { get; set; }
        public bool Virtual { get; set; }
        public bool Deactivated { get; set; }
        public DateTime CreationTime { get; set; }

        public virtual SmartTicketUser UsernameNavigation { get; set; }
        public virtual ICollection<CreditTransaction> CreditTransactions { get; set; }
        public virtual ICollection<Validation> Validations { get; set; }
    }
}
