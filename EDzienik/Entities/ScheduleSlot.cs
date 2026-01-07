using System.Security.Claims;

namespace EDzienik.Entities
{
    public class ScheduleSlot
    {
        public int Id { get; set; }

        public DayOfWeek DayOfWeek { get; set; } 
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Room { get; set; } = string.Empty;

        public int ClassId { get; set; }
        public virtual SchoolClass Class { get; set; } = null!;

        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; } = null!;

        public int TeacherId { get; set; }
        public virtual Teacher Teacher { get; set; } = null!;
    }
}