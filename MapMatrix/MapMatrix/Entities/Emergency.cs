using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace MapMatrix.Entities
{
    public class Emergency
    {
        [Key]
        public Guid identifiantEmergency { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SessionId { get; set; }

        [ForeignKey(nameof(SessionId))]
        public GuideSession Session { get; set; } = null!;

        [Required]
        public Guid GuideId { get; set; }
        public Point position { get; set; } = null!;

        public string? message { get; set; }

        [Required]
        public string status { get; set; } = "open"; // "open", "acknowledged", "resolved", "false_alarm"

        public Guid? acknowledgedBy { get; set; }
        public DateTime? acknowledgedAt { get; set; }
        public DateTime? resolvedAt { get; set; }

        public DateTime createdAt { get; set; } = DateTime.UtcNow;
    }
}
