using Microsoft.AspNetCore.Identity;

namespace EDzienik.Entities
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        public virtual Student? Student { get; set; }
        public virtual Teacher? Teacher { get; set; }
    }
}