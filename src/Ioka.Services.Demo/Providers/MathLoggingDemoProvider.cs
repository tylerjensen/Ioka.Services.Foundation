using Ioka.Services.Foundation.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ioka.Services.Demo.Providers
{
    public interface IMathLoggingDemoProvider
    {
        Task<long> DoMathWithLogging(LogContext logContext, ILog logger);
    }

    public class MathLoggingDemoProvider : IMathLoggingDemoProvider
    {
        public async Task<long> DoMathWithLogging(LogContext logContext, ILog logger)
        {
            long x = 0;
            try
            {
                var rand = new Random();
                for (int i = 0; i < 10; i++)
                {
                    x = 1000 * (long)rand.NextDouble();
                    Thread.Sleep(10);
                }
                Thread.Sleep(100);
                var c = 0;
                x = 77 / c;
            }
            catch (Exception e)
            {
                //uses new logger with saved context as this is not on the request background thread
                logger.With(logContext).Error(e, "Error: value of {LargeValue}", x);
            }
            return x;
        }
    }
}
