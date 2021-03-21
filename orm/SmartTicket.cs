using System;
using System.Collections.Generic;

#nullable disable

namespace NFCTicketingWebAPI
{
    public partial class SmartTicket
    {
        public SmartTicket()
        {
            Validations = new HashSet<Validation>();
        }

        public string CardId { get; set; }
        public decimal Credit { get; set; }
        public string TicketType { get; set; }
        public DateTime? CurrentValidation { get; set; }
        public DateTime? SessionValidation { get; set; }
        public decimal? SessionExpense { get; set; }
        public int? UserId { get; set; }

        public virtual SmartTicketUser User { get; set; }
        public virtual ICollection<Validation> Validations { get; set; }
    }
}
