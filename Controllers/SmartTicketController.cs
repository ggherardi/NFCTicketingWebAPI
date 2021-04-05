using CSharp.NFC.NDEF;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NFCTicketing;
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
    public class SmartTicketController : ControllerBase, IValidationStorage
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
            string role = _authManager.GetRole(_dbContext, credentials.Username, credentials.Password);
            if (!string.IsNullOrEmpty(role))
            {
                token = _authManager.GenerateToken(credentials.Username, role);
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
            Role userRole = _dbContext.Roles.FirstOrDefault(r => r.Name == RoleType.User);
            // Add password hashing!
            //registration.Credentials.Password
            _dbContext.SmartTicketUsers.Add(new SmartTicketUser() { Name = registration.Name, Surname = registration.Surname, Username = registration.Credentials.Username, Password = registration.Credentials.Password, Email = registration.Email, Role = userRole.Id});
            _dbContext.SaveChanges();
            return AuthenticateUser(registration.Credentials);
        }

        [HttpPost]
        [Route("associateticket")]
        public IActionResult AssociateTicket([FromBody] string cardId)
        {
            // I need to encrypt the cardId to avoid unallowed usage of the api
            SmartTicket ticket = _dbContext.SmartTickets.Find(cardId);
            if(ticket != null && !ticket.Deactivated && ticket.Username == null && !_dbContext.SmartTickets.Any(s => s.Username == User.Identity.Name && !s.Virtual))
            {
                ticket.Username = User.Identity.Name;
                _dbContext.SaveChanges();
                return Ok("The ticket has been succesfully associated to the user's account.");
            }
            else
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ticket == null ? "No tickets found with the provided id." : ticket.Username != null ? "The ticked has already an associated account." : "The user already has an associated physical ticket.");
            }            
        }

        [HttpPost]
        [Route("unbindticket")]
        public IActionResult UnbindTicket([FromBody] string cardId)
        {
            // I need to encrypt the cardId to avoid unallowed usage of the api
            SmartTicket ticket = _dbContext.SmartTickets.Find(cardId);
            if (ticket != null && !ticket.Deactivated && ticket.Username == User.Identity.Name)
            {
                ticket.Username = null;
                _dbContext.SaveChanges();
                return Ok("The ticket has been succesfully unbound from the user's account.");
            }
            else
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ticket == null ? "No tickets found with the provided id." : ticket.Username == null ? "The ticked is already unbound." : "The ticket is bound to another account.");
            }
        }

        [HttpPost]
        [Route("addcredit")]
        public IActionResult AddCredit([FromBody] CreditRecharge recharge)
        {
            byte[] encryptedTicketInNDEFMessage;
            SmartTicket ticket = _dbContext.SmartTickets.Find(recharge.TicketId);
            // Add online payments logic here to authorize the balance increase
            if(ticket != null)
            {
                try
                {
                    ticket.Credit += recharge.Amount;
                    EncryptableSmartTicket encryptableTicket = Utility.ConvertToEncryptableSmartTicket(ticket);
                    byte[] encryptedTicket = TicketEncryption.EncryptTicket(encryptableTicket, TicketEncryption.GetPaddedIV(encryptableTicket.CardID));
                    encryptedTicketInNDEFMessage = new NDEFMessage(encryptedTicket, NDEFRecordType.Types.Text).GetFormattedBlock();
                    _dbContext.SaveChanges();
                    _dbContext.CreditTransactions.Add(new CreditTransaction() { Amount = recharge.Amount, CardId = recharge.TicketId, Date = DateTime.Now, Location = "online" });
                    _dbContext.SaveChanges();

                }
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
                }
                return Ok(encryptedTicketInNDEFMessage);
            }
            else
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, "The ticket has not been found.");
            }
        }

        /// <summary>
        /// This endpoint creates a virtual card and returns a NDEFMessage containing the encrypted ticket, that the device will store and use when validating the ticket.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("createvirtualticket")]
        public IActionResult CreateVirtualTicket()
        {            
            byte[] encryptedTicketInNDEFMessage = new byte[] { }; 
            SmartTicket virtualTicket = null;
            if (!_dbContext.SmartTickets.Any(s => s.Username == User.Identity.Name && s.Virtual && !s.Deactivated))
            {
                try
                {
                    byte[] virtualTicketId = Guid.NewGuid().ToByteArray();
                    virtualTicket = new SmartTicket() { CardId = BitConverter.ToString(virtualTicketId), Credit = 0, TicketType = "BIT", Username = User.Identity.Name, Virtual = true, UsageTimestamp = DateTime.Now, Deactivated = false };
                    byte[] encryptedTicket = TicketEncryption.EncryptTicket(Utility.ConvertToEncryptableSmartTicket(virtualTicket), TicketEncryption.GetPaddedIV(virtualTicketId));
                    encryptedTicketInNDEFMessage = new NDEFMessage(encryptedTicket, NDEFRecordType.Types.Text).GetFormattedBlock();
                    _dbContext.SmartTickets.Add(virtualTicket);
                    _dbContext.SaveChanges();
                }                
                catch(Exception ex)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
                }
                return Ok(BitConverter.ToString(encryptedTicketInNDEFMessage));
            }
            return StatusCode((int)HttpStatusCode.InternalServerError, "The user already has an associated virtual ticket");
        }

        [HttpPost]
        [Route("deactivateticket")]
        public IActionResult DeactivateTicket([FromBody] string cardId)
        {
            SmartTicket ticket = _dbContext.SmartTickets.Find(cardId);
            if (ticket != null && !ticket.Deactivated && ticket.Username == User.Identity.Name)
            {
                ticket.Deactivated = true;
                _dbContext.SaveChanges();
                return Ok("The ticket has been deactivated");
            }
            else
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ticket == null ? "No tickets found with the provided id" : ticket.Deactivated ? "The ticket is already deactivated" : "The user is not the owner of the ticket");
            }
        }

        [HttpDelete]
        [Route("deleteticket/{cardid}")]
        public IActionResult DeleteVirtualTicket(string cardid)
        {            
            SmartTicket ticket = _dbContext.SmartTickets.Find(cardid);
            if (ticket != null && ticket.Username == User.Identity.Name && ticket.Virtual)
            {
                _dbContext.Remove(ticket);
                _dbContext.SaveChanges();
                return Ok("The ticket has been deleted.");
            }
            else
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ticket == null ? "No tickets found with the provided id" : ticket.Username != User.Identity.Name ? "The user is not the owner of the ticket" : "The ticket is not virtual" );
            }
        }

        [HttpPost]
        [Route("validate")]
        public IActionResult ValidateTicket([FromBody] ValidationRegistration registration)
        {
            SmartTicket ticket = _dbContext.SmartTickets.Find(registration.TicketId);
            if (ticket != null && ticket.Username == User.Identity.Name && ticket.Virtual)
            {
                try
                {
                    EncryptableSmartTicket encryptableTicket = Utility.ConvertToEncryptableSmartTicket(ticket);
                    ValidationManager manager = new ValidationManager(encryptableTicket, this, registration.Location);
                    manager.ValidateTicket();
                    return Ok("Ticket validated");
                }                
                catch(Exception ex)
                {
                    return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            else
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ticket == null ? "No tickets found with the provided id" : ticket.Username != User.Identity.Name ? "The user is not the owner of the ticket" : "The ticket is not virtual");
            }
        }

        [HttpGet]
        [Route("gettickets")]
        public IActionResult GetTickets()
        {
            string username = User.Identity.Name;
            IQueryable<SmartTicket> query = from tickets in _dbContext.SmartTickets
                        join users in _dbContext.SmartTicketUsers
                        on tickets.Username equals users.Username
                        where users.Username == User.Identity.Name && !tickets.Deactivated
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

        #region IValidationStorage interface
        [ApiExplorerSettings(IgnoreApi = true)]
        public void RegisterValidation(ValidationEntity validation)
        {
            _dbContext.Validations.Add(new Validation() { CardId = BitConverter.ToString(validation.CardId), Location = validation.Location, ValidationTime = validation.Time, EncryptedTicket = validation.EncryptedTicketHash });
            _dbContext.SaveChanges();
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public void RegisterTicketUpdate(EncryptableSmartTicket encryptableTicket)
        {
            SmartTicket ticket = _dbContext.SmartTickets.Find(BitConverter.ToString(encryptableTicket.CardID));
            if(ticket != null)
            {
                ticket.Credit = encryptableTicket.Credit;
                ticket.TicketType = encryptableTicket.TicketTypeName;
                ticket.CurrentValidation = encryptableTicket.CurrentValidation;
                ticket.SessionValidation= encryptableTicket.SessionValidation;
                ticket.SessionExpense = encryptableTicket.SessionExpense;
                _dbContext.SaveChanges();
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public void RegisterTransaction(NFCTicketing.CreditTransaction transaction)
        {
            _dbContext.CreditTransactions.Add(new CreditTransaction() { CardId = BitConverter.ToString(transaction.CardId), Location = transaction.Location, Amount = transaction.Amount, Date = transaction.Date });
            _dbContext.SaveChanges();
        }
        #endregion
    }
}
