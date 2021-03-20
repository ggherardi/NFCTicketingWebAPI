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
    [Route("[controller]")]
    public class SmartTicketController : ControllerBase
    {
        private readonly ILogger<SmartTicketController> _logger;

        public SmartTicketController(ILogger<SmartTicketController> logger)
        {
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public string Get()
        {                        
            return "Ciao";
        }
    }
}
