using System.ComponentModel.DataAnnotations;

namespace EDzienik.Entities;

public class Teacher
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public List<SubjectAssignment> Assignments { get; set; } = new();
    public List<ScheduleSlot> ScheduleSlots { get; set; } = new();
    public List<Grade> Grades { get; set; } = new();
    public List<SchoolEvent> SchoolEvents { get; set; } = new();
}
