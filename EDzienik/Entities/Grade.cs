using System.ComponentModel.DataAnnotations;

namespace EDzienik.Entities;

public class Grade
{
    public int Id { get; set; }

    [Range(1, 6)]
    public int Value { get; set; }

    public string Description { get; set; } = string.Empty;

    public long CreatedUnix { get; set; }

    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public int SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public int TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;
}
