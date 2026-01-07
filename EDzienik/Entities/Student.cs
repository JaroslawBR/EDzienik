using System.ComponentModel.DataAnnotations;

namespace EDzienik.Entities
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public virtual User User { get; set; } = null!;

        public int SchoolClassId { get; set; }
        public virtual SchoolClass SchoolClass { get; set; } = null!;

        public virtual List<Grade> Grades { get; set; } = new();
    }
}
