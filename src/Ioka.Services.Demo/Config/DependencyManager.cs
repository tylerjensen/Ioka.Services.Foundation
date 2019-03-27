using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ioka.Services.Demo.Config
{
    internal static class DependencyManager
    {
        public static IServiceCollection ConfigureDependencies(this IServiceCollection services, IConfiguration config)
        {
            Configure(services, config);
            return services;
        }

        public static void Configure(IServiceCollection services, IConfiguration config)
        {
            // configure simple service dependency
            // services.AddSingleton<IMyRepository>((serviceProvider) => new MyRepository());

            // configure dependency that relies on another
            // services.AddScoped<IMyHandler>((serviceProvider) => new MyHandler(serviceProvider.GetService<IMyRepository>()));

            // configure a dictionary factory configuration where MyHandlerFactory inherits from Dictionary<string, Func<IMyHandler>>
            // services.AddSingleton<IMyHandlerFactory>((x) => new MyHandlerFactory
            // {
            //     { "special", () => new SpecialHandler(x.GetService<IMyRepository>()) },
            //     { "my", () => new MyHandler(x.GetService<IMyRepository>()) }
            // });
        }

    }

    // example
    // public class MyHandlerFactory : Dictionary<string, Func<IMyHandler>>, IMyHandlerFactory
    // {
    //     public IMyHandler Create(string key)
    //     {
    //         return this[key]();
    //     }
    // }
}
