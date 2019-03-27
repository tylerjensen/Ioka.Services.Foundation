using System.Threading.Tasks;

namespace Ioka.Services.IdentityServer.Common
{
    //ref: https://damienbod.com/2017/04/14/asp-net-core-identityserver4-resource-owner-password-flow-with-custom-userrepository/
    public interface IUserRepository
    {
        bool ValidateCredentials(string username, string password);
        IdentityUser FindBySubjectId(string subjectId);
        IdentityUser FindByUsername(string username);
    }
}
