using System.ComponentModel.DataAnnotations;
using EDzienik.Entities.Enums;

namespace EDzienik.DTOs.Auth
{
    public class RegisterRequestDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        [MaxLength(128)]
        public string Password { get; set; }

        [Required]
        [MaxLength(64)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(64)]
        public string LastName { get; set; }

        [Required]
        public UserRoles Role { get; set; }

        
    }
}
