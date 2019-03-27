using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ioka.Services.IdentityServer
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IHostingEnvironment _environment;

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddMemoryCache();
            services.AddMvc();

            services.AddIdentityServer()
                .AddSigningCredential(LoadCert())
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients())
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddIokaUserStore(null);
        }

        private X509Certificate2 LoadCert()
        {
            var certLocation = _environment.ContentRootPath + "/iokasecuritydemo.pfx";
            var cert = new X509Certificate2(certLocation, "ioka");
            return cert;
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();
            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();
        }
    }
}
/*
 * Create PFX with IIS "Create Self-Signed Certificate" with private and public key.
 * Then create a public key cert for the client app (demo) using Powershell: 
    Get-PfxCertificate -FilePath InputBundle.pfx | Export-Certificate -FilePath OutputCert.cer -Type CERT
 */
