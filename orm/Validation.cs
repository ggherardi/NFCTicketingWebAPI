using System;
using System.Collections.Generic;

#nullable disable

namespace NFCTicketingWebAPI
{
    public partial class Validation
    {
        public string CardId { get; set; }
        public string Location { get; set; }
        public DateTime? ValidationTime { get; set; }
        public string EncryptedTicket { get; set; }

        public virtual SmartTicket Card { get; set; }
    }
}
