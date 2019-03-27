using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Ioka.Services.Foundation.Security
{
    public static class SecurityExtensions
    {
        public static string GetClaimValue(this ClaimsPrincipal user, string claimType)
        {
            try
            {
                var val = user.Claims
                    .Where(x => x.Type == claimType)
                    .Select(x => x.Value)
                    .FirstOrDefault();
                return val;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// Add security to the API with Ioka Identity Server
        /// </summary>
        /// <param name="services"></param>
        /// <param name="signingCertIssuer">The issue URL from the identity server.</param>
        /// <param name="certPassword"></param>
        /// <param name="jwtAudience"></param>
        /// <param name="certLocation"></param>
        /// <param name="certBytes"></param>
        /// <param name="scopes">If null, then default scopes are only openid and ready.</param>
        /// <returns></returns>
        public static IServiceCollection AddIokaSecurity(this IServiceCollection services, string signingCertIssuer, string jwtAudience,
            string certLocation, string certPassword, string[] scopes = null)
        {
            if (null == certLocation) throw new ArgumentNullException(nameof(certLocation));
            return AddIokaSecurity(services, signingCertIssuer, jwtAudience, certLocation, null, certPassword, scopes);
        }

        public static IServiceCollection AddIokaSecurity(this IServiceCollection services, string signingCertIssuer, string jwtAudience,
            byte[] certBytes, string certPassword, string[] scopes = null)
        {
            if (null == certBytes) throw new ArgumentNullException(nameof(certBytes));
            return AddIokaSecurity(services, signingCertIssuer, jwtAudience, null, certBytes, certPassword, scopes);
        }

        private static IServiceCollection AddIokaSecurity(IServiceCollection services, string signingCertIssuer, string jwtAudience,
            string certLocation = null, byte[] certBytes = null, string certPassword = null, string[] scopes = null)
        { 
            if (string.IsNullOrWhiteSpace(signingCertIssuer)) throw new ArgumentNullException(nameof(signingCertIssuer));
            if (null == certPassword) certPassword = string.Empty;
            if (null == jwtAudience) jwtAudience = string.Empty;

            if (null == scopes || scopes.Length == 0) scopes = new[] { "openid" };

            try
            {
                var cert = string.IsNullOrWhiteSpace(certLocation)
                    ? new X509Certificate2(certBytes, certPassword, X509KeyStorageFlags.MachineKeySet)
                    : new X509Certificate2(certLocation, certPassword, X509KeyStorageFlags.MachineKeySet);
                var key = new X509SecurityKey(cert);
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidIssuer = signingCertIssuer,
                    IssuerSigningKey = key,
                    RequireSignedTokens = true
                };

                // MS mapping removes the "sub" claim from ClaimsPrincipal User. 
                // This line prevents that from happening. Go figure.
                // see https://github.com/IdentityServer/IdentityServer4/issues/1707

                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("sub");
                JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove("email");

                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // base-address of your identityserver
                    options.Audience = jwtAudience;
                    options.ClaimsIssuer = signingCertIssuer;
                    options.TokenValidationParameters = tokenValidationParameters;
                    options.RequireHttpsMetadata = false;
                });

                services.AddAuthorization(options =>
                {
                    foreach (var scope in scopes)
                    {
                        options.AddPolicy(scope, policy => policy.Requirements.Add(new ScopeRequirement(scope, signingCertIssuer)));
                    }
                });

                // register the scope authorization handler
                services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
            }
            catch (Exception ex)
            {
                //catch file not found error
                if (ex.HResult == 0x2006D080)
                {
                    throw new FileNotFoundException($"The file does not exist");
                }
                throw;
            }
            return services;
        }
    }
}
