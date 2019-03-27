using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using Ioka.Services.Foundation.Logging;
using System.Threading;

namespace Ioka.Services.Demo
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private ILog _logger;

        public ValuesController(ILog logger)
        {
            _logger = logger;
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
            var userId = logContext["_UserId"];
            var requestId = logContext["_RequestId"];

            await Task.Factory.StartNew(() =>
            {
                long x = 0;
                try
                {
                    var rand = new Random();
                    for (int i = 0; i < 100; i++)
                    {
                        x = (long)rand.NextDouble();
                        Thread.Sleep(10);
                    }
                    Thread.Sleep(1000);
                    var c = 0;
                    x = 77 / c;
                }
                catch (Exception e)
                {
                    //uses new logger with saved context as this is not on the request background thread
                    _logger.With(logContext).Error(e, "Error: value of {LargeValue}", x);
                }
            }, TaskCreationOptions.LongRunning);


            return new string[] { "value1a", "value2a" };
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
