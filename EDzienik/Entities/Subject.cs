using System.ComponentModel.DataAnnotations;

namespace EDzienik.Entities
{
    public class Subject
    {
        public int Id { get; set; }

        [Required, MaxLength(80)]
        public string Name { get; set; } = string.Empty; // np. "Matematyka"

        public virtual List<SubjectAssignment> Assignments { get; set; } = new();
        public virtual List<ScheduleSlot> ScheduleSlots { get; set; } = new();
        public virtual List<Grade> Grades { get; set; } = new();
    }
}
