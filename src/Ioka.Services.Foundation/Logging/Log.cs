using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.File;

namespace Ioka.Services.Foundation.Logging
{
    public class ElasticConfig
    {
        public string Url { get; set; }
        public string ElasticUser { get; set; }
        public string ElasticPassword { get; set; }
    }

    public class Log : ILog, IDisposable
    {
        private object _syncRoot = new object();
        private Serilog.Core.Logger _logger;

        private string _index;
        private ElasticConfig _config;
        private LogLevel _level;
        private ILogEventEnricher[] _enrichers;
        private ILogEventSink _failureSink;
        private Action<LogEvent> _failureCallback;

        /// <summary>
        /// Create a new Log instance with default context.
        /// </summary>
        /// <param name="environment">The environment must be provided. This determines the ELK index, e.g. alog-local or alog-production.</param>
        /// <param name="config">Configuration for minimum log level and Elasticsearch connection details.</param>
        public Log(ElasticConfig config, LogLevel level, Func<string> createIndexName) 
            : this(config, level, createIndexName, null, null, null)
        {
        }

        /// <summary>
        /// Create a new Log instance with default context.
        /// </summary>
        /// <param name="environment">The environment must be provided. This determines the ELK index, e.g. alog-local or alog-production.</param>
        /// <param name="config">Configuration for minimum log level and Elasticsearch connection details.</param>
        /// <param name="enrichers">Serilog enrichers. If null, the default LogEnricher is used.</param>
        public Log(ElasticConfig config, LogLevel level, Func<string> createIndexName, ILogEventEnricher[] enrichers)
            : this(config, level, createIndexName, null, null, enrichers)
        {
        }

        /// <summary>
        /// Create a new Log instance with default context with a failure sink.
        /// </summary>
        /// <param name="environment">The environment must be provided. This determines the ELK index, e.g. alog-local or alog-production.</param>
        /// <param name="config">Configuration for minimum log level and Elasticsearch connection details.</param>
        /// <param name="failureSink">Serilog failure sink. Pass null if not used.</param>
        /// <param name="failureCallback">Serilog failure callback. Pass null if not used.</param>
        /// <param name="enrichers">Serilog enrichers. If null, the default LogEnricher is used.</param>
        public Log(ElasticConfig config, LogLevel level, Func<string> createIndexName, ILogEventSink failureSink, Action<LogEvent> failureCallback, ILogEventEnricher[] enrichers) 
        {
            _index = null == createIndexName ? "default" : createIndexName();
            _config = config;
            _level = level;
            _enrichers = enrichers;
            _failureSink = failureSink;
            _failureCallback = failureCallback;

            var elasticOptions = new ElasticsearchSinkOptions(new Uri(config.Url))
            {
                CustomFormatter = new LogExceptionFormatter(),
                MinimumLogEventLevel = LogEventLevel.Verbose,
                AutoRegisterTemplate = false,
                IndexDecider = (logEventProxy, dateTimeOffset) => _index,
                OverwriteTemplate = false,
                FailureCallback = null != _failureCallback
                        ? _failureCallback
                        : (e) =>
                        {
                            Console.WriteLine("Unable to submit event " + e.MessageTemplate);
                        },
                EmitEventFailure = null != _failureSink
                        ? EmitEventFailureHandling.WriteToSelfLog |
                          EmitEventFailureHandling.WriteToFailureSink |
                          EmitEventFailureHandling.RaiseCallback
                        : EmitEventFailureHandling.WriteToSelfLog |
                          EmitEventFailureHandling.RaiseCallback,
                FailureSink = _failureSink
            };

            if (!string.IsNullOrWhiteSpace(_config.ElasticUser))
                elasticOptions.ModifyConnectionSettings = (connConfig) => connConfig.BasicAuthentication(config.ElasticUser, config.ElasticPassword);

            Serilog.Debugging.SelfLog.Enable(msg => 
            {
                Console.WriteLine(msg);
            });

            var logConfig = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.Elasticsearch(elasticOptions);

            if (null == enrichers || enrichers.Length == 0)
            {
                enrichers = new[] { new LogEnricher() };
            }
            logConfig.Enrich.With(enrichers);

            switch (_level)
            {
                case LogLevel.Trace:
                    logConfig.MinimumLevel.Verbose();
                    break;
                case LogLevel.Debug:
                    logConfig.MinimumLevel.Debug();
                    break;
                case LogLevel.Info:
                    logConfig.MinimumLevel.Information();
                    break;
                case LogLevel.Warn:
                    logConfig.MinimumLevel.Warning();
                    break;
                case LogLevel.Error:
                    logConfig.MinimumLevel.Error();
                    break;
                default:
                    logConfig.MinimumLevel.Fatal();
                    break;
            }

            _logger = logConfig.CreateLogger();
            var ctx = new LogContext();
        }

        public ILog With(LogContext context)
        {
            context["_ThreadId-With"] = Environment.CurrentManagedThreadId.ToString(); //set to current thread
            var list = _enrichers.Where(x => x.GetType() != typeof(LogEnricher)).ToList();
            list.Insert(0, new LogEnricher(context, null));
            return new Log(_config, _level, () => _index, _failureSink, _failureCallback, list.ToArray());
        }

        public ILogger Logger
        {
            get => _logger;
        }


        #region From ILogger
        public void Debug(string messageTemplate)
        {
            Logger.Debug(messageTemplate);
        }

        public void Debug(Exception exception, string messageTemplate)
        {
            Logger.Debug(exception, messageTemplate);
        }

        public void Debug(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Logger.Debug(exception, messageTemplate, propertyValues);
        }

        public void Debug(string messageTemplate, params object[] propertyValues)
        {
            Logger.Debug(messageTemplate, propertyValues);
        }

        public void Error(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Logger.Error(exception, messageTemplate, propertyValues);
        }

        public void Error(string messageTemplate)
        {
            Logger.Error(messageTemplate);
        }

        public void Error(Exception exception, string messageTemplate)
        {
            Logger.Error(exception, messageTemplate);
        }

        public void Error(string messageTemplate, params object[] propertyValues)
        {
            Logger.Error(messageTemplate, propertyValues);
        }

        public void Fatal(string messageTemplate)
        {
            Logger.Fatal(messageTemplate);
        }

        public void Fatal(string messageTemplate, params object[] propertyValues)
        {
            Logger.Fatal(messageTemplate, propertyValues);
        }

        public void Fatal(Exception exception, string messageTemplate)
        {
            Logger.Fatal(exception, messageTemplate);
        }

        public void Fatal(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Logger.Fatal(exception, messageTemplate, propertyValues);
        }

        public void Info(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Logger.Information(exception, messageTemplate, propertyValues);
        }

        public void Info(Exception exception, string messageTemplate)
        {
            Logger.Information(exception, messageTemplate);
        }

        public void Info(string messageTemplate, params object[] propertyValues)
        {
            Logger.Information(messageTemplate, propertyValues);
        }

        public void Info(string messageTemplate)
        {
            Logger.Information(messageTemplate);
        }

        public bool IsEnabled(LogLevel level)
        {
            return Logger.IsEnabled(level.ToLogEventLevel());
        }

        public void Trace(string messageTemplate)
        {
            Logger.Verbose(messageTemplate);
        }

        public void Trace(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Logger.Verbose(exception, messageTemplate, propertyValues);
        }

        public void Trace(Exception exception, string messageTemplate)
        {
            Logger.Verbose(exception, messageTemplate);
        }

        public void Trace(string messageTemplate, params object[] propertyValues)
        {
            Logger.Verbose(messageTemplate, propertyValues);
        }

        public void Warn(string messageTemplate)
        {
            Logger.Warning(messageTemplate);
        }

        public void Warn(Exception exception, string messageTemplate)
        {
            Logger.Warning(exception, messageTemplate);
        }

        public void Warn(Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Logger.Warning(exception, messageTemplate, propertyValues);
        }

        public void Warn(string messageTemplate, params object[] propertyValues)
        {
            Logger.Warning(messageTemplate, propertyValues);
        }

        public void Write(LogLevel level, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            Logger.Write(level.ToLogEventLevel(), exception, messageTemplate, propertyValues);
        }

        public void Write(LogLevel level, string messageTemplate)
        {
            Logger.Write(level.ToLogEventLevel(), messageTemplate);
        }

        public void Write(LogLevel level, string messageTemplate, params object[] propertyValues)
        {
            Logger.Write(level.ToLogEventLevel(), messageTemplate, propertyValues);
        }

        public void Write(LogLevel level, Exception exception, string messageTemplate)
        {
            Logger.Write(level.ToLogEventLevel(), exception, messageTemplate);
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // dispose managed state (managed objects).
                    _logger.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Log() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        #region with LogMessage
        public void Debug(LogMessage message, string messageTemplate)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Debug(messageTemplate + "-{@_message}", message);
        }

        public void Debug(LogMessage message, Exception exception, string messageTemplate)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Debug(exception, messageTemplate + "-{@_message}", message);
        }

        public void Debug(LogMessage message, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Debug(exception, messageTemplate + "-{@_message}", 
                propertyValues == null 
                ? new[] { message } 
                : propertyValues.Concat(new[] { message }).ToArray());
        }

        public void Debug(LogMessage message, string messageTemplate, params object[] propertyValues)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Debug(messageTemplate + "-{@_message}", 
                propertyValues == null
                ? new[] { message }
                : propertyValues.Concat(new[] { message }).ToArray());
        }

        public void Error(LogMessage message, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Error(exception, messageTemplate + "-{@_message}",
                propertyValues == null
                ? new[] { message }
                : propertyValues.Concat(new[] { message }).ToArray());
        }

        public void Error(LogMessage message, string messageTemplate)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Error(messageTemplate + "-{@_message}", message);
        }

        public void Error(LogMessage message, Exception exception, string messageTemplate)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Error(exception, messageTemplate + "-{@_message}", message);
        }

        public void Error(LogMessage message, string messageTemplate, params object[] propertyValues)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Error(messageTemplate + "-{@_message}",
                propertyValues == null
                ? new[] { message }
                : propertyValues.Concat(new[] { message }).ToArray());
        }

        public void Fatal(LogMessage message, string messageTemplate)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Fatal(messageTemplate + "-{@_message}", message);
        }

        public void Fatal(LogMessage message, string messageTemplate, params object[] propertyValues)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Fatal(messageTemplate + "-{@_message}",
                propertyValues == null
                ? new[] { message }
                : propertyValues.Concat(new[] { message }).ToArray());
        }

        public void Fatal(LogMessage message, Exception exception, string messageTemplate)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Fatal(exception, messageTemplate + "-{@_message}", message);
        }

        public void Fatal(LogMessage message, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Fatal(exception, messageTemplate + "-{@_message}",
                propertyValues == null
                ? new[] { message }
                : propertyValues.Concat(new[] { message }).ToArray());
        }

        public void Info(LogMessage message, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Info(exception, messageTemplate + "-{@_message}",
                propertyValues == null
                ? new[] { message }
                : propertyValues.Concat(new[] { message }).ToArray());
        }

        public void Info(LogMessage message, Exception exception, string messageTemplate)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Info(exception, messageTemplate + "-{@_message}", message);
        }

        public void Info(LogMessage message, string messageTemplate, params object[] propertyValues)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Info(messageTemplate + "-{@_message}",
                propertyValues == null
                ? new[] { message }
                : propertyValues.Concat(new[] { message }).ToArray());
        }

        public void Info(LogMessage message, string messageTemplate)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Info(messageTemplate + "-{@_message}", message);
        }

        public void Trace(LogMessage message, string messageTemplate)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Trace(messageTemplate + "-{@_message}", message);
        }

        public void Trace(LogMessage message, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
        }

        public void Trace(LogMessage message, Exception exception, string messageTemplate)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
        }

        public void Trace(LogMessage message, string messageTemplate, params object[] propertyValues)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Trace(messageTemplate + "-{@_message}",
                propertyValues == null
                ? new[] { message }
                : propertyValues.Concat(new[] { message }).ToArray());
        }

        public void Warn(LogMessage message, string messageTemplate)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Warn(messageTemplate + "-{@_message}", message);
        }

        public void Warn(LogMessage message, Exception exception, string messageTemplate)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Warn(exception, messageTemplate + "-{@_message}", message);
        }

        public void Warn(LogMessage message, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Warn(exception, messageTemplate + "-{@_message}",
                propertyValues == null
                ? new[] { message }
                : propertyValues.Concat(new[] { message }).ToArray());
        }

        public void Warn(LogMessage message, string messageTemplate, params object[] propertyValues)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Warn(messageTemplate + "-{@_message}",
                propertyValues == null
                ? new[] { message }
                : propertyValues.Concat(new[] { message }).ToArray());
        }

        public void Write(LogLevel level, LogMessage message, Exception exception, string messageTemplate, params object[] propertyValues)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Write(level, exception, messageTemplate + "-{@_message}",
                propertyValues == null
                ? new[] { message }
                : propertyValues.Concat(new[] { message }).ToArray());
        }

        public void Write(LogLevel level, LogMessage message, string messageTemplate)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Write(level, messageTemplate + "-{@_message}", message);
        }

        public void Write(LogLevel level, LogMessage message, string messageTemplate, params object[] propertyValues)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Write(level, messageTemplate + "-{@_message}",
                propertyValues == null
                ? new[] { message }
                : propertyValues.Concat(new[] { message }).ToArray());
        }

        public void Write(LogLevel level, LogMessage message, Exception exception, string messageTemplate)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            Write(level, exception, messageTemplate + "-{@_message}", message);
        }

        public void Debug(LogMessage message)
        {
            Debug("{@_message}", message);
        }

        public void Debug(LogMessage message, Exception exception)
        {
            Debug(exception, "{@_message}", message);
        }

        public void Error(LogMessage message)
        {
            Error("{@_message}", message);
        }

        public void Error(LogMessage message, Exception exception)
        {
            Error(exception, "{@_message}", message);
        }

        public void Fatal(LogMessage message)
        {
            Fatal("{@_message}", message);
        }

        public void Fatal(LogMessage message, Exception exception)
        {
            Fatal(exception, "{@_message}", message);
        }

        public void Info(LogMessage message, Exception exception)
        {
            Info(exception, "{@_message}", message);
        }

        public void Info(LogMessage message)
        {
            Info("{@_message}", message);
        }

        public void Trace(LogMessage message)
        {
            Trace("{@_message}", message);
        }

        public void Trace(LogMessage message, Exception exception)
        {
            Trace(exception, "{@_message}", message);
        }

        public void Warn(LogMessage message)
        {
            Warn("{@_message}", message);
        }

        public void Warn(LogMessage message, Exception exception)
        {
            Warn(exception, "{@_message}", message);
        }

        public void Write(LogLevel level, LogMessage message, Exception exception)
        {
            Write(level, exception, "{@_message}", message);
        }

        public void Write(LogLevel level, LogMessage message)
        {
            Write(level, "{@_message}", message);
        }
        #endregion

        #region Exception only
        public void Debug(Exception exception)
        {
            Debug(exception, "(exception only)");
        }

        public void Error(Exception exception)
        {
            Error(exception, "(exception only)");
        }

        public void Fatal(Exception exception)
        {
            Fatal(exception, "(exception only)");
        }

        public void Info(Exception exception)
        {
            Info(exception, "(exception only)");
        }

        public void Trace(Exception exception)
        {
            Trace(exception, "(exception only)");
        }

        public void Warn(Exception exception)
        {
            Warn(exception, "(exception only)");
        }

        public void Write(LogLevel level, Exception exception)
        {
            Write(level, exception, "(exception only)");
        }
        #endregion
    }
}
