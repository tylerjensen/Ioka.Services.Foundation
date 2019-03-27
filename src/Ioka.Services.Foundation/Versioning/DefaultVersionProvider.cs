using System.Collections.Generic;
using System.Reflection;

namespace Ioka.Services.Foundation.Versioning
{
    public class DefaultVersionProvider : IVersionProvider
    {
        private IDictionary<string, VersionInfo> _data = null;

        public DefaultVersionProvider()
        {
            _data = new Dictionary<string, VersionInfo>()
            {
                { "isf-Ioka.Services.Foundation", new VersionInfo { Version = Assembly.GetAssembly(typeof(DefaultVersionProvider)).GetName().Version.ToString() } },
                { "isf-Microsoft.AspNetCore.Authentication.JwtBearer", new VersionInfo { Version = Assembly.GetAssembly(typeof(Microsoft.AspNetCore.Authentication.JwtBearer.AuthenticationFailedContext)).GetName().Version.ToString() } },
                { "isf-Microsoft.AspNetCore.HttpOverrides", new VersionInfo { Version = Assembly.GetAssembly(typeof(Microsoft.AspNetCore.HttpOverrides.ForwardedHeadersDefaults)).GetName().Version.ToString() } },
                { "isf-Microsoft.AspNetCore.Mvc", new VersionInfo { Version = Assembly.GetAssembly(typeof(Microsoft.AspNetCore.Mvc.Controller)).GetName().Version.ToString() } },
                { "isf-Microsoft.Extensions.Logging.Abstractions", new VersionInfo { Version = Assembly.GetAssembly(typeof(Microsoft.Extensions.Logging.Abstractions.NullLogger)).GetName().Version.ToString() } },
                { "isf-Microsoft.Extensions.Logging.Console", new VersionInfo { Version = Assembly.GetAssembly(typeof(Microsoft.Extensions.Logging.Console.ConfigurationConsoleLoggerSettings)).GetName().Version.ToString() } },
                { "isf-Microsoft.Extensions.Logging.Debug", new VersionInfo { Version = Assembly.GetAssembly(typeof(Microsoft.Extensions.Logging.Debug.DebugLogger)).GetName().Version.ToString() } },
                { "isf-Microsoft.Extensions.PlatformAbstractions", new VersionInfo { Version = Assembly.GetAssembly(typeof(Microsoft.Extensions.PlatformAbstractions.ApplicationEnvironment)).GetName().Version.ToString() } },
                { "isf-Microsoft.IdentityModel.Tokens", new VersionInfo { Version = Assembly.GetAssembly(typeof(Microsoft.IdentityModel.Tokens.AsymmetricSecurityKey)).GetName().Version.ToString() } },
                { "isf-Serilog", new VersionInfo { Version = Assembly.GetAssembly(typeof(Serilog.ILogger)).GetName().Version.ToString() } },
                { "isf-Serilog.Extensions.Logging", new VersionInfo { Version = Assembly.GetAssembly(typeof(Serilog.Extensions.Logging.SerilogLoggerProvider)).GetName().Version.ToString() } },
                { "isf-Serilog.Sinks.Elasticsearch", new VersionInfo { Version = Assembly.GetAssembly(typeof(Serilog.Sinks.Elasticsearch.ElasticsearchJsonFormatter)).GetName().Version.ToString() } },
                { "isf-Swashbuckle.AspNetCore", new VersionInfo { Version = Assembly.GetAssembly(typeof(Swashbuckle.AspNetCore.Swagger.ISwaggerProvider)).GetName().Version.ToString() } },
                { "isf-WebApiContrib.Core.Formatter.Protobuf", new VersionInfo { Version = Assembly.GetAssembly(typeof(WebApiContrib.Core.Formatter.Protobuf.ProtobufFormatterOptions)).GetName().Version.ToString() } }
            };
        }

        public IDictionary<string, VersionInfo> VersionData {
            get => _data;
            set {
                _data = value;
            }
        }
    }
}
