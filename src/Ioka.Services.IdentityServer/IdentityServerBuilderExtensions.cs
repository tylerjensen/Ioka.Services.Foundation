using Microsoft.Extensions.DependencyInjection;
using Ioka.Services.IdentityServer.Common;
using Microsoft.Extensions.DependencyInjection.Extensions;
using IdentityServer4.Services;
using IdentityServer4.Stores.Serialization;
using IdentityServer4.Stores;

namespace Ioka.Services.IdentityServer
{
    public class Connections
    {
        public string IokaConnection { get; set; }
    }


    //ref: https://damienbod.com/2017/04/14/asp-net-core-identityserver4-resource-owner-password-flow-with-custom-userrepository/
    public static class IdentityServerBuilderExtensions
    {
        public static IIdentityServerBuilder AddIokaUserStore(this IIdentityServerBuilder builder, IUserRepository userRepository)
        {
            //builder.AddDefaultEndpoints();
            //builder.AddPluggableServices();

            if (null != userRepository)
                builder.Services.AddSingleton<IUserRepository>(userRepository);
            else
                builder.Services.AddSingleton<IUserRepository, DefaultUserRepository>();

            //builder.Services.AddTransient<IHandleGenerationService, DefaultHandleGenerationService>();
            //builder.Services.AddTransient<IPersistentGrantSerializer, PersistentGrantSerializer>();
            //builder.Services.AddTransient<IPersistedGrantStore, InMemoryPersistedGrantStore>();
            builder.Services.AddTransient<IRefreshTokenService, DefaultRefreshTokenService>();
            builder.Services.AddTransient<IRefreshTokenStore, DefaultRefreshTokenStore>();

            builder.AddProfileService<ProfileService>();

            builder.AddResourceOwnerValidator<ResourceOwnerPasswordValidator>();
            return builder;
        }
    }
}
