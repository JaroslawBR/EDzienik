namespace EDzienik.Entities
{
    public class Teacher
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public virtual List<SubjectAssignment> Assignments { get; set; } = new();
    }
}