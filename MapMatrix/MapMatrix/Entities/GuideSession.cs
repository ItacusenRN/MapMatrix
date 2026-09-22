using System. ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MapMatrix.Entities
{
    public class GuideSession
    {
        [Key]
        public Guid id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid guideId { get; set; }

        [ForeignKey(nameof(guideId))]
        public User guide { get; set; } = null!;

        [Required]
        public Guid trailId { get; set; }

        [ForeignKey(nameof(trailId))]
        public Trail trail { get; set; } = null!;

        public Guid? scheduleId { get; set; }

        [Required]
        public SessionStatus status { get; set; } = SessionStatus.planned;

        public int participantsCount { get; set; } = 0;

        public string? notes { get; set; }

        public DateTime createdAt { get; set; } = DateTime.UtcNow;

        public DateTime updatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? endedAt { get; set; }

        public DateTime? startedAt {  get; set; }

        public DateTime? plannedStart { get; set; }

        //Navigation
        public ICollection<GuidePosition> positions { get; set; } = new List<GuidePosition>();
        public ICollection<Emergency> emergencies { get; set; } = new List<Emergency>();
    }
}
