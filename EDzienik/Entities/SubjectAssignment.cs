namespace EDzienik.Entities
{
    public class SubjectAssignment
    {
        public int Id { get; set; }

        public int TeacherId { get; set; }
        public virtual Teacher Teacher { get; set; } = null!;

        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; } = null!;

        public int SchoolClassId { get; set; }
        public virtual SchoolClass SchoolClass { get; set; } = null!;
    }
}
