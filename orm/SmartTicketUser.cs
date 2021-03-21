using System;
using System.Collections.Generic;

#nullable disable

namespace NFCTicketingWebAPI
{
    public partial class SmartTicketUser
    {
        public SmartTicketUser()
        {
            SmartTickets = new HashSet<SmartTicket>();
        }

        public int UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public byte[] CreationTime { get; set; }

        public virtual ICollection<SmartTicket> SmartTickets { get; set; }
    }
}
