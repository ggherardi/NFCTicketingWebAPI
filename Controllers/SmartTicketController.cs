using CSharp.NFC.NDEF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NFCTicketing.Encryption;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NFCTicketingWebAPI.Controllers
{    
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SmartTicketController : ControllerBase
    {
        private readonly NFCValidationStorageContext _dbContext;
        private readonly ILogger<SmartTicketController> _logger;
        private readonly IAuthenticationManager _authManager;
        private IDictionary<string, string> _authenticatedUsers = new Dictionary<string, string>();

        public SmartTicketController(ILogger<SmartTicketController> logger, IAuthenticationManager authManager, NFCValidationStorageContext dbContext)
        {
            _dbContext = dbContext;
            _logger = logger;
            _authManager = authManager;
        }
        
        [AllowAnonymous]
        [HttpPost]
        [Route("authenticate")]
        public IActionResult AuthenticateUser([FromBody] UserCredentials credentials)
        {
            string token = string.Empty;
            if(_authManager.Authenticate(_dbContext, credentials.Username, credentials.Password))
            {
                token = _authManager.GenerateToken(credentials.Username);
                _authenticatedUsers.Add(token, credentials.Username);
            }
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }
            return Ok(token);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public IActionResult RegisterUser([FromBody] UserRegistration registration)
        {
            _dbContext.SmartTicketUsers.Add(new SmartTicketUser() { Name = registration.Name, Surname = registration.Surname, Username = registration.Credentials.Username, Password = registration.Credentials.Password });
            _dbContext.SaveChanges();
            return AuthenticateUser(registration.Credentials);
        }

        [HttpPost]
        [Route("associateticket")]
        public IActionResult AssociateTicket([FromBody] string cardId)
        {
            // I need to encrypt the cardId to avoid unallowed usage of the api
            SmartTicket ticket = _dbContext.SmartTickets.Find(cardId);
            if(ticket != null && ticket.Username == null && !_dbContext.SmartTickets.Any(s => s.Username == User.Identity.Name))
            {
                ticket.Username = User.Identity.Name;
                _dbContext.SaveChanges();
                return Ok("The ticket has been succesfully associated to the user's account.");
            }
            else
            {
                return StatusCode((int)HttpStatusCode.NotAcceptable, ticket == null ? "No tickets found with the provided id." : ticket.Username != null ? "The ticked has already an associated account." : "The user already has an associated physical ticket.");
            }            
        }

        /// <summary>
        /// This endpoint creates a virtual card and returns a NDEFMessage containing the encrypted ticket, that the device will store and use when validating the ticket.
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("createvirtualticket")]
        public IActionResult CreateVirtualTicket()
        {
            byte[] encryptedTicketInNDEFMessage = new byte[] { }; 
            SmartTicket virtualTicket = null;
            if (_dbContext.SmartTickets.FirstOrDefault(s => s.Username == User.Identity.Name && s.Virtual) != null)
            {
                try
                {
                    byte[] virtualTicketId = Guid.NewGuid().ToByteArray();
                    virtualTicket = new SmartTicket() { CardId = BitConverter.ToString(virtualTicketId), Credit = 0, TicketType = "BIT", Username = User.Identity.Name, Virtual = true, UsageTimestamp = DateTime.Now };
                    byte[] encryptedTicket = TicketEncryption.EncryptTicket(Utility.ConvertToEncryptableSmartTicket(virtualTicket), TicketEncryption.GetPaddedIV(virtualTicketId));
                    encryptedTicketInNDEFMessage = new NDEFMessage(encryptedTicket, NDEFRecordType.Types.Text).GetFormattedBlock();
                    _dbContext.SmartTickets.Add(virtualTicket);
                    _dbContext.SaveChanges();                    
                }                
                catch(Exception ex)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
                }
                return Ok(encryptedTicketInNDEFMessage);
            }
            return StatusCode((int)HttpStatusCode.NotAcceptable, "The user already has an associated virtual ticket.");
        }

        [HttpGet]
        [Route("gettickets")]
        public IActionResult GetTickets()
        {
            string username = User.Identity.Name;
            IQueryable<SmartTicket> query = from tickets in _dbContext.SmartTickets
                        join users in _dbContext.SmartTicketUsers
                        on tickets.Username equals users.Username
                        where users.Username == User.Identity.Name
                        select tickets;
            List<SmartTicket> allUserTickets = query.ToList();
            if(allUserTickets != null)
            {
                return Ok(allUserTickets);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
