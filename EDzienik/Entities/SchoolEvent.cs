namespace EDzienik.Entities
{
    public enum EventType
    {
        Announcement, 
        Exam,         
        Homework    
    }

    public class SchoolEvent
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public EventType Type { get; set; }

        public int SchoolClassId { get; set; }
        public virtual SchoolClass SchoolClass { get; set; } = null!;

        public int TeacherId { get; set; }
        public virtual Teacher Teacher { get; set; } = null!;
    }
}