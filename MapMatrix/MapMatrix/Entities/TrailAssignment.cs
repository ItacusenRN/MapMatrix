using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MapMatrix.Entities
{
    public class TrailAssignment
    {
        [Key]
        public Guid id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid trailId { get; set; }

        [ForeignKey(nameof(trailId))]
        public Trail trail { get; set; } = null!;

        [Required]
        public Guid guideId { get; set; }

        [ForeignKey(nameof(guideId))]
        public User guide { get; set; } = null!;

        public Guid? assignedBy { get; set; }

        [ForeignKey(nameof(assignedBy))]
        public User? supervisor { get; set; }

        [Required]
        public AssignmentStatus status { get; set; } = AssignmentStatus.pending;

        public bool requestedByGuide { get; set; } = false;

        public DateTime? scheduledDate { get; set; } // Date prevue du trajet en question

        public string? notes { get; set; }

        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
    }
}
