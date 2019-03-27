namespace Ioka.Services.IdentityServer.Common
{
    public class IdentityUser
    {
        public string SubjectId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string FullName { get; set; }
        public string Company { get; set; }
    }
}
