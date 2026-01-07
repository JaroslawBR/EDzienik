using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EDzienik.Entities
{
    public class User : IdentityUser
    {
        [MaxLength(64)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(64)]
        public string LastName { get; set; } = string.Empty;

        public virtual Student? Student { get; set; }
        public virtual Teacher? Teacher { get; set; }
    }
}
