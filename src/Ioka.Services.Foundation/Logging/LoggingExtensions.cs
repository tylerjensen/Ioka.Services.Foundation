using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ioka.Services.Foundation.Logging
{
    public static class LoggingExtensions
    {
        /// <summary>
        /// Add custom Serilog logging to Elasticsearch endpoint.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="config">Connection configuration.</param>
        /// <param name="level">Log level</param>
        /// <param name="createIndexName">Function to create index name per request.</param>
        /// <param name="enrichers">Leave null to accept default enricher.</param>
        /// <returns></returns>
        public static IServiceCollection AddIokaLogging(this IServiceCollection services, ElasticConfig config,
            Func<string> createIndexName, ILogEventEnricher[] enrichers = null, ILogEventSink failureSink = null, Action<LogEvent> failureCallback = null)
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<ContextProvider>();
            services.AddSingleton<LogEnricher>((serviceProvider) => new LogEnricher(new LogContext(),
                LogContextFactory.CreateFactory(serviceProvider.GetService<ContextProvider>())));

            services.AddScoped<ILog>((serviceProvider) => new Log(config, createIndexName, failureSink, failureCallback, 
                (null == enrichers ? new[] { serviceProvider.GetService<LogEnricher>() } : enrichers)));

            var svcProvider = services.BuildServiceProvider();
            var log = svcProvider.GetService<ILog>();
            services.AddLogging(configure => configure.AddSerilog(log.Logger, dispose: true));

            // Configure ASP.NET Core extensions ILogger for default out of the box behavior.
            services.AddSingleton<Microsoft.Extensions.Logging.ILogger>((x) => x.GetRequiredService<ILogger<DefaultLogSource>>());

            // As we have already called BuildServiceProvider and resolved ILog, we register it again to
            // assure that it is provided as a singleton after the ConfigureServices completes.
            services.AddSingleton<ILog>((x) => log);
            return services;
        }
    }


}
