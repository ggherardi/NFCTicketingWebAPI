using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NFCTicketingWebAPI
{
    public class UserCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UserRegistration
    {
        public UserCredentials Credentials { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
    }
}
