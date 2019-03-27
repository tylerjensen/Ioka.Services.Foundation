using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ioka.Services.Foundation.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;

namespace Ioka.Services.Foundation.Versioning
{
    /// <summary>
    /// This is the documentation at the class level.
    /// </summary>
    [Route("api-meta/[controller]")]
    public class VersionController : Controller
    {
        private ILog _logger;
        private IVersionProvider _versionProvider;

        public VersionController(IVersionProvider versionProvider, ILog logger)
        {
            _versionProvider = versionProvider;
            _logger = logger;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get Version Info",
            Description = "Get version data for dependencies of this service.")]
        [SwaggerResponse(200, "The test was successful.", typeof(IDictionary<string, VersionInfo>))]
        public async Task<ActionResult> Get()
        {
            _logger.Info("Version endpoint called.");
            return Ok(_versionProvider?.VersionData);
        }
    }
}
