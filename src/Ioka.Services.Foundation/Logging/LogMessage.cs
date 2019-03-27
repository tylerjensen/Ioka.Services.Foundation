using System;
using System.Collections.Generic;
using System.Text;

namespace Ioka.Services.Foundation.Logging
{
    public class LogMessage
    {
        public Guid CustomerId { get; set; }
        public int MemberId { get; set; }
        public string Message { get; set; }
        public string Component { get; set; }
        public Dictionary<string, string> Other { get; set; } = new Dictionary<string, string>();
    }
}
