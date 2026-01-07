using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace EDzienik.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public int ClassId { get; set; }

        [ForeignKey("ClassId")]
        public virtual SchoolClass Class { get; set; } = null!;
    }
}