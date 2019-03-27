using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ioka.Services.Foundation.Security;

namespace Ioka.Services.Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecTestController : ControllerBase
    {
        [HttpGet]
        [Authorize("openid")]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            var id = User.GetClaimValue("sub");
            var username = User.GetClaimValue("username");
            var email = User.GetClaimValue("email");
            return new string[] { id, username, email };
        }
    }
}