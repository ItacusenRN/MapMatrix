using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace MapMatrix.Entities
{
    public class GuidePosition
    {
        [Key]
        public Guid id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid sessionId { get; set; }

        [ForeignKey(nameof(sessionId))]
        public GuideSession session { get; set; } = null!;

        [Required]
        public Guid guideId { get; set; }

        public Point position { get; set; } = null!;

        [Column(TypeName = "numeric(8,2)")]
        public decimal? altitude { get; set; }

        [Column(TypeName = "numeric(6,2)")]
        public decimal? accuracy { get; set; }

        [Column(TypeName = "numeric(6,2)")]
        public decimal? speed { get; set; }

        [Column(TypeName = "numeric(5,2)")]
        public decimal? heading { get; set; }

        public DateTime recordedAt { get; set; } = DateTime.UtcNow;
    }
}
