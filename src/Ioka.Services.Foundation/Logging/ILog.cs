using System;

namespace Ioka.Services.Foundation.Logging
{
    public interface ILog : IDisposable
    {
        Serilog.ILogger Logger { get; }

        ILog With(LogContext context);

        #region from ILogger
        void Debug(string messageTemplate);
        void Debug(Exception exception, string messageTemplate);
        void Debug(Exception exception, string messageTemplate, params object[] propertyValues);
        void Debug(string messageTemplate, params object[] propertyValues);
        void Error(Exception exception, string messageTemplate, params object[] propertyValues);
        void Error(string messageTemplate);
        void Error(Exception exception, string messageTemplate);
        void Error(string messageTemplate, params object[] propertyValues);
        void Fatal(string messageTemplate);
        void Fatal(string messageTemplate, params object[] propertyValues);
        void Fatal(Exception exception, string messageTemplate);
        void Fatal(Exception exception, string messageTemplate, params object[] propertyValues);
        void Info(Exception exception, string messageTemplate, params object[] propertyValues);
        void Info(Exception exception, string messageTemplate);
        void Info(string messageTemplate, params object[] propertyValues);
        void Info(string messageTemplate);
        bool IsEnabled(LogLevel level);
        void Trace(string messageTemplate);
        void Trace(Exception exception, string messageTemplate, params object[] propertyValues);
        void Trace(Exception exception, string messageTemplate);
        void Trace(string messageTemplate, params object[] propertyValues);
        void Warn(string messageTemplate);
        void Warn(Exception exception, string messageTemplate);
        void Warn(Exception exception, string messageTemplate, params object[] propertyValues);
        void Warn(string messageTemplate, params object[] propertyValues);
        void Write(LogLevel level, Exception exception, string messageTemplate, params object[] propertyValues);
        void Write(LogLevel level, string messageTemplate);
        void Write(LogLevel level, string messageTemplate, params object[] propertyValues);
        void Write(LogLevel level, Exception exception, string messageTemplate);
        #endregion

        #region with exception only
        void Debug(Exception exception);
        void Error(Exception exception);
        void Fatal(Exception exception);
        void Info(Exception exception);
        void Trace(Exception exception);
        void Warn(Exception exception);
        void Write(LogLevel level, Exception exception);
        #endregion


        #region with LogMessage but no template
        void Debug(LogMessage message);
        void Debug(LogMessage message, Exception exception);
        void Error(LogMessage message);
        void Error(LogMessage message, Exception exception);
        void Fatal(LogMessage message);
        void Fatal(LogMessage message, Exception exception);
        void Info(LogMessage message, Exception exception);
        void Info(LogMessage message);
        void Trace(LogMessage message);
        void Trace(LogMessage message, Exception exception);
        void Warn(LogMessage message);
        void Warn(LogMessage message, Exception exception);
        void Write(LogLevel level, LogMessage message, Exception exception);
        void Write(LogLevel level, LogMessage message);
        #endregion

        #region with LogMessage and template
        void Debug(LogMessage message, string messageTemplate);
        void Debug(LogMessage message, Exception exception, string messageTemplate);
        void Debug(LogMessage message, Exception exception, string messageTemplate, params object[] propertyValues);
        void Debug(LogMessage message, string messageTemplate, params object[] propertyValues);
        void Error(LogMessage message, Exception exception, string messageTemplate, params object[] propertyValues);
        void Error(LogMessage message, string messageTemplate);
        void Error(LogMessage message, Exception exception, string messageTemplate);
        void Error(LogMessage message, string messageTemplate, params object[] propertyValues);
        void Fatal(LogMessage message, string messageTemplate);
        void Fatal(LogMessage message, string messageTemplate, params object[] propertyValues);
        void Fatal(LogMessage message, Exception exception, string messageTemplate);
        void Fatal(LogMessage message, Exception exception, string messageTemplate, params object[] propertyValues);
        void Info(LogMessage message, Exception exception, string messageTemplate, params object[] propertyValues);
        void Info(LogMessage message, Exception exception, string messageTemplate);
        void Info(LogMessage message, string messageTemplate, params object[] propertyValues);
        void Info(LogMessage message, string messageTemplate);
        void Trace(LogMessage message, string messageTemplate);
        void Trace(LogMessage message, Exception exception, string messageTemplate, params object[] propertyValues);
        void Trace(LogMessage message, Exception exception, string messageTemplate);
        void Trace(LogMessage message, string messageTemplate, params object[] propertyValues);
        void Warn(LogMessage message, string messageTemplate);
        void Warn(LogMessage message, Exception exception, string messageTemplate);
        void Warn(LogMessage message, Exception exception, string messageTemplate, params object[] propertyValues);
        void Warn(LogMessage message, string messageTemplate, params object[] propertyValues);
        void Write(LogLevel level, LogMessage message, Exception exception, string messageTemplate, params object[] propertyValues);
        void Write(LogLevel level, LogMessage message, string messageTemplate);
        void Write(LogLevel level, LogMessage message, string messageTemplate, params object[] propertyValues);
        void Write(LogLevel level, LogMessage message, Exception exception, string messageTemplate);
        #endregion

    }
}
