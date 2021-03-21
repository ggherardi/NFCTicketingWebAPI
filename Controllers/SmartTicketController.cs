using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public IActionResult GetToken([FromBody] UserCredentials credentials)
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

        [HttpGet]
        public IActionResult GetTicket()
        {
            string username = User.Identity.Name;
            var query = from tickets in _dbContext.SmartTickets
                        join users in _dbContext.SmartTicketUsers
                        on tickets.UserId equals users.UserId
                        where users.Username == User.Identity.Name
                        select tickets;
            SmartTicket ticket = query.FirstOrDefault();
            if(ticket != null)
            {
                return Ok(ticket);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
