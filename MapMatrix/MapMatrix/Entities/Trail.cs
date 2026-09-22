using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace MapMatrix.Entities
{
    public class Trail
    {
        [Key]
        public Guid id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(255)]
        public string name { get; set; } = null!;

        public string? description { get; set; }

        [Required]
        public TransportMode transportMode { get; set; } = TransportMode.pied;

        public short? difficulty { get; set; }

        [Column(TypeName = "numeric(8,2)")]
        public decimal? distanceKm { get; set; }

        public TimeSpan? estimatedDuration { get; set; }

        public LineString? geometry { get; set; } = null!;

        [Required]
        public TrailStatus status { get; set; } = TrailStatus.draft;

        [Required]
        public string approvalStatus { get; set; } = "pending";

        [Required]
        public Guid createdBy { get; set; }

        [ForeignKey(nameof(createdBy))]
        public User creator { get; set; } = null!;

        public DateTime? publishedAt { get; set; }

        public DateTime createdAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Beacon> beacons { get; set; } = new List<Beacon>();
        public ICollection<TrailSchedule> schedules { get; set; } = new List<TrailSchedule>();
        public ICollection<Content> contents { get; set; } = new List<Content>();
    }
}