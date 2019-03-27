using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ioka.Services.IdentityServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VersionController : ControllerBase
    {
        // GET: api/Version
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "version1", "version2" };
        }
    }
}
