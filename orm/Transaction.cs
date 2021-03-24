using System;
using System.Collections.Generic;

#nullable disable

namespace NFCTicketingWebAPI
{
    public partial class Transaction
    {
        public string CardId { get; set; }
        public string Location { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
