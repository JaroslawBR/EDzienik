using System.ComponentModel.DataAnnotations.Schema;

namespace EDzienik.Entities
{
    public class Teacher
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public virtual List<SubjectAssignment> Assignments { get; set; } = new();
    }
}