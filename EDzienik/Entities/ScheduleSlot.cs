namespace EDzienik.Entities;

public class ScheduleSlot
{
    public int Id { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public long StartUnix { get; set; }
    public long EndUnix { get; set; }

    public string Room { get; set; } = string.Empty;

    public int SchoolClassId { get; set; }
    public SchoolClass SchoolClass { get; set; } = null!;

    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;
}
