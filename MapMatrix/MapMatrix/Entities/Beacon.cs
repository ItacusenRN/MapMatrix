using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace MapMatrix.Entities
{
    public class Beacon
    {
        [Key]
        public Guid id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid trailId { get; set; }

        [ForeignKey(nameof(trailId))]
        public Trail trail { get; set; } = null!;

        [Required, MaxLength(200)]
        public string name { get; set; } = null!;

        public string? description { get; set; }
        
        public Point position { get; set; } = null!;

        public int sequenceOrder { get; set; } = 0;

        public bool estReference { get; set; } = false;

        [Required]
        public Guid? createdBy { get; set; }

        public DateTime createdAt { get; set; } = DateTime.UtcNow;
    }
}
