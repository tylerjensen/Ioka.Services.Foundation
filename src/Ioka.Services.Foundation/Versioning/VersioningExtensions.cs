using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace Ioka.Services.Foundation.Versioning
{
    public static class VersioningExtensions
    {
        /// <summary>
        /// Add configuration for IVersionProvider. The VersionProvider should be very fast and minimize IO if any.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="provider">This can be left null and the DefaultVersionProvider will be returned.</param>
        /// <param name="combineWithDefault">Set to false to avoid having the DefaultVersionProvider data appended to the one provided in the provider parameter.</param>
        /// <returns></returns>
        public static IServiceCollection AddIokaVersionProvider(this IServiceCollection services, IVersionProvider provider = null, bool combineWithDefault = true)
        {
            var defaultProvider = new DefaultVersionProvider();
            if (null == provider)
            {
                provider = defaultProvider;
            }
            else
            {
                if (combineWithDefault)
                {
                    var combined = new Dictionary<string, VersionInfo>();
                    foreach (var item in defaultProvider.VersionData)
                    {
                        combined.Add(item.Key, item.Value);
                    }
                    foreach (var item in provider.VersionData)
                    {
                        combined.Add(item.Key, item.Value);
                    }
                    provider.VersionData = combined;
                }
            }
            services.AddSingleton<IVersionProvider>(provider);
            return services;
        }
    }
}
