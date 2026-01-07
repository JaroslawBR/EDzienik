namespace EDzienik.Entities
{
    public class SchoolClass
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; 
        public int SchoolYear { get; set; }

        public virtual List<Student> Students { get; set; } = new();
    }
}