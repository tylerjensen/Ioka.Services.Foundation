using System.Threading.Tasks;
using Ioka.Services.IdentityServer.Common;

namespace Ioka.Services.IdentityServer
{
    //ref: https://damienbod.com/2017/04/14/asp-net-core-identityserver4-resource-owner-password-flow-with-custom-userrepository/
    public class DefaultUserRepository : IUserRepository
    {
        public bool ValidateCredentials(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            if (string.IsNullOrWhiteSpace(password)) return false;
            if ("sam.smith" == username) return true;
            return false;
        }

        public IdentityUser FindBySubjectId(string subjectId)
        {
            if (string.IsNullOrWhiteSpace(subjectId)) return null;
            if ("8426f6a9-9df2-4443-a5f3-43883b627b0a" == subjectId)
            {
                return new IdentityUser
                {
                    SubjectId = "8426f6a9-9df2-4443-a5f3-43883b627b0a",
                    Username = "sam.smith",
                    FullName = "Sam Smith",
                    FirstName = "Sam",
                    LastName = "Smith",
                    Email = "sam.smith@mailinator.com",
                    Phone = "201 555 1212",
                    Company = "Smith Co."
                };
            }
            return null;
        }

        public IdentityUser FindByUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return null;
            if ("sam.smith" == username)
            {
                return new IdentityUser
                {
                    SubjectId = "8426f6a9-9df2-4443-a5f3-43883b627b0a",
                    Username = "sam.smith",
                    FullName = "Sam Smith",
                    FirstName = "Sam",
                    LastName = "Smith",
                    Email = "sam.smith@mailinator.com",
                    Phone = "201 555 1212",
                    Company = "Smith Co."
                };
            }
            return null;
        }
    }
}
