using System;
using System.Collections.Generic;

#nullable disable

namespace NFCTicketingWebAPI
{
    public partial class Role
    {
        public Role()
        {
            SmartTicketUsers = new HashSet<SmartTicketUser>();
        }

        public short Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<SmartTicketUser> SmartTicketUsers { get; set; }
    }
}
