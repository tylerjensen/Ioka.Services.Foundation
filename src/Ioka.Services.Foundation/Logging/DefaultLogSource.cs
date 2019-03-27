using System;
using System.Collections.Generic;
using System.Text;

namespace Ioka.Services.Foundation.Logging
{
    /// <summary>
    /// Used only for default ILogger resolution since Serilog does not support ILogger but only ILogger<T>
    /// </summary>
    public class DefaultLogSource { }
}
