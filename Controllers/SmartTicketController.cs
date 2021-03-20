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
        private readonly ILogger<SmartTicketController> _logger;
        private readonly IAuthenticationManager _authManager;


        public SmartTicketController(ILogger<SmartTicketController> logger, IAuthenticationManager authManager)
        {
            _logger = logger;
            _authManager = authManager;
        }
        
        [AllowAnonymous]
        [HttpPost]
        public IActionResult GetToken([FromBody] UserCredentials credentials)
        {
            string token = string.Empty;
            if(_authManager.Authenticate(credentials.Username, credentials.Password))
            {
                token = _authManager.GenerateToken(credentials.Username);
            }
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized();
            }
            return Ok(token);
        }
    }
}
