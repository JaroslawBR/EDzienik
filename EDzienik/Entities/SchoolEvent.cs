using System.ComponentModel.DataAnnotations;

namespace EDzienik.Entities;

public enum EventType
{
    Announcement,
    Exam,
    Homework
}

public class SchoolEvent
{
    public int Id { get; set; }

    [Required, MaxLength(120)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public DateTime Date { get; set; }
    public EventType Type { get; set; }

    public int SchoolClassId { get; set; }
    public SchoolClass SchoolClass { get; set; } = null!;

    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;
}
