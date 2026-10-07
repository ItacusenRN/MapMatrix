using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace MapMatrix.Entities
{
    public class Emergency
    {
        [Key]
        public Guid id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid sessionId { get; set; }

        [ForeignKey(nameof(sessionId))]
        public GuideSession session { get; set; } = null!;

        [Required]
        public Guid guideId { get; set; }

        [ForeignKey(nameof(guideId))]
        public User guide { get; set; } = null!;

        public Point position { get; set; } = null!;

        public string? message { get; set; }

        [Required]
        public EmergencyStatus status { get; set; } = EmergencyStatus.open; // "open", "acknowledged", "resolved", "false_alarm"

        public Guid? handledBy { get; set; } //superviseur qui a pris en charge

        public DateTime acknowledgedAt { get; set; } = DateTime.UtcNow;
        public DateTime createdAt { get; set; } = DateTime.UtcNow;
    }
}
