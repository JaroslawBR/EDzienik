using System.Security.Claims;

namespace EDzienik.Entities
{
    public class Student
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public int ClassId { get; set; }
        public virtual SchoolClass Class { get; set; } = null!;
    }
}