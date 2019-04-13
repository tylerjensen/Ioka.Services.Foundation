using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using Ioka.Services.Foundation.Logging;
using System.Threading;
using Ioka.Services.Demo.Providers;

namespace Ioka.Services.Demo
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILog _logger;
        private readonly IMathLoggingDemoProvider _mathDemoProvider;

        public ValuesController(ILog logger, IMathLoggingDemoProvider mathDemoProvider)
        {
            _logger = logger;
            _mathDemoProvider = mathDemoProvider;
        }

        // GET api/values
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get values a & b",
            Description = "A test of Get and Swagger docs")]
        [SwaggerResponse(200, "The test was successful.", typeof(string[]))]
        [SwaggerResponse(404, "The test was not found.", typeof(Exception))]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            var msg1 = "Another message";
            var msg3 = new CustomError { Name = "Second", Message = "Second other message" };
            _logger.Debug("This is a debug message. {msg1}, {@msg3}", msg1, msg3);

            var logContext = LogContextFactory.Create(this.HttpContext);
            var result = await _mathDemoProvider.DoMathWithLogging(logContext, _logger);

            return new string[] 
            {
                logContext["_UserId"],
                logContext["_RequestId"],
                result.ToString()
            };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public async Task Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
        }
    }

    public class CustomError
    {
        public string Name { get; set; }
        public string Message { get; set; }
    }
}
