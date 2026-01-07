using System.ComponentModel.DataAnnotations;

namespace EDzienik.Entities
{
    public class Grade
    {
        public int Id { get; set; }

        [Range(1, 6)]
        public int Value { get; set; } 

        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public int StudentId { get; set; }
        public virtual Student Student { get; set; } = null!;

        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; } = null!;

        public int TeacherId { get; set; }
        public virtual Teacher Teacher { get; set; } = null!;
    }
}
