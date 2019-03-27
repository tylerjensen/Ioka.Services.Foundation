using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ioka.Services.Demo.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ioka.Services.Foundation.Versioning;
using Ioka.Services.Foundation.Logging;
using Ioka.Services.Foundation.MessageFormatters;
using Ioka.Services.Foundation.Security;
using Ioka.Services.Demo.Properties;
using Ioka.Services.Foundation.ApiDocs;
using Ioka.Services.Foundation.HighAvailability;

namespace Ioka.Services.Demo
{
    public class Startup
    {
        /// <summary>
        /// Startup constructor requires two additional injected dependencies use in ConfigureServices
        /// which does not support injection of these dependencies in that method.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="environment"></param>
        /// <param name="loggerFactory"></param>
        public Startup(IConfiguration configuration, IHostingEnvironment environment, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            Environment = environment;
            LoggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }
        public ILoggerFactory LoggerFactory { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //TODO get from appsettings
            var config = new ElasticConfig
            {
                Url = "http://elk:9200",
                ElasticUser = null,
                ElasticPassword = null
            };

            services
                .AddIokaVersionProvider(new VersionProvider())
                .AddIokaLogging(config, Foundation.Logging.LogLevel.Debug, () => "test")
                .AddMvc()
                .AddIokaFormatters()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services
                .AddIokaSecurity("https://localhost:44336", "https://localhost:44336/resources", GetEmbeddedCert(), "ioka", null)
                .AddIokaApiDocs(title: "Ioka.Services.Demo",
                    description: "This is the demo service reference app.");

            // Add dependencies to IoC container for each hosted component
            // which can be done like this DependencyManager.Configure(services, Configuration);
            // or as a fluent interface as in the line below.
            services.ConfigureDependencies(Configuration);

            // Configure any shared middleware such as a special request handler
            // services.AddSingleton<SpecialRequestHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //force HTTPS on all services
            app.UseHttpsRedirection();

            app.UseIokaApiDocsUI()
               .UseIokaForwardedHeaders()
               .UseAuthentication()
               .UseMvc();
        }

        private static byte[] GetEmbeddedCert()
        {
            var cert = Resource.iokasecurity;
            return cert;
        }

    }
}
