using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Ioka.Services.IdentityServer.Common;

namespace Ioka.Services.IdentityServer
{
    //ref: https://damienbod.com/2017/04/14/asp-net-core-identityserver4-resource-owner-password-flow-with-custom-userrepository/
    public class ProfileService : IProfileService
    {
        protected readonly ILogger Logger;
        protected readonly IUserRepository _userRepository;

        public ProfileService(IUserRepository userRepository, ILogger<ProfileService> logger)
        {
            _userRepository = userRepository;
            Logger = logger;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var type = context.Subject.GetType();

            var sub = context.Subject.GetSubjectId();

            Logger.LogDebug("Get profile called for subject {subject} from client {client} with claim types {claimTypes} via {caller}",
                context.Subject.GetSubjectId(),
                context.Client.ClientName ?? context.Client.ClientId,
                context.RequestedClaimTypes,
                context.Caller);

            var subjectId = context.Subject.GetSubjectId();
            var user = _userRepository.FindBySubjectId(subjectId);

            var claims = new List<Claim>
            {
                new Claim("username", user?.Username ?? subjectId),
                new Claim("email", user?.Email ?? subjectId.Substring(subjectId.IndexOf("\\")+1)),
            };

            context.IssuedClaims = claims;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = _userRepository.FindBySubjectId(context.Subject.GetSubjectId());
            context.IsActive = user != null;
        }
    }
}
