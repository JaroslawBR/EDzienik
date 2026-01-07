namespace EDzienik.Entities
{
    public class Grade
    {
        public int Id { get; set; }
        public int Value { get; set; } 
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