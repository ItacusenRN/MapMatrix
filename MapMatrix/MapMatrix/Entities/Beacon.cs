using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace MapMatrix.Entities
{
    public class Beacon
    {
        [Key]
        public Guid indentifiant { get; set; } = Guid.NewGuid();

        [Required]
        public Guid trailId { get; set; }

        [ForeignKey(nameof(trailId))]
        public Trail trail { get; set; } = null!;

        [Required, MaxLength(150)]
        public string nom { get; set; } = null!;

        public string? description { get; set; }
        
        public Point position { get; set; } = null!;

        public int ordreIndex { get; set; } = 0; // Ordre du beacon dans le trail

        public bool estReference { get; set; } = false; // Indique si le beacon est une référence pour le trail

        [Required]
        public Guid createurId { get; set; }

        public DateTime publierA { get; set; } = DateTime.UtcNow;
    }
}
