using System.ComponentModel.DataAnnotations.Schema;

namespace EDzienik.Entities
{
    public class SubjectAssignment
    {
        public int Id { get; set; }

        public int TeacherId { get; set; }
        public virtual Teacher Teacher { get; set; } = null!;

        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; } = null!;

        public int ClassId { get; set; }

        [ForeignKey("ClassId")]
        public virtual SchoolClass Class { get; set; } = null!;
    }
}