using System.ComponentModel.DataAnnotations;

namespace EDzienik.Entities
{
    public class SchoolClass
    {
        public int Id { get; set; }

        [Required, MaxLength(16)]
        public string Name { get; set; } = string.Empty;

        public int SchoolYear { get; set; }

        public virtual List<Student> Students { get; set; } = new();
        public virtual List<SubjectAssignment> SubjectAssignments { get; set; } = new();
        public virtual List<ScheduleSlot> ScheduleSlots { get; set; } = new();
        public virtual List<SchoolEvent> SchoolEvents { get; set; } = new();
    }
}
