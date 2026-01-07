using System.ComponentModel.DataAnnotations;

namespace EDzienik.Entities
{
    public class Teacher
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public virtual User User { get; set; } = null!;

        public virtual List<SubjectAssignment> Assignments { get; set; } = new();
        public virtual List<ScheduleSlot> ScheduleSlots { get; set; } = new();
        public virtual List<Grade> Grades { get; set; } = new();
        public virtual List<SchoolEvent> SchoolEvents { get; set; } = new();
    }
}
