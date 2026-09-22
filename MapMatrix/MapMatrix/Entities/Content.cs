using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace MapMatrix.Entities
{
    public class Content
    {
        [Key]
        public Guid identifiantContent { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string type { get; set; } = null!; // "news", "event", "ad", "organization"

        [Required, MaxLength(255)]
        public string title { get; set; } = null!;

        [MaxLength(255)]
        public string? slug { get; set; }

        public string? summary { get; set; }
        public string? body { get; set; }
        public string? imageUrl { get; set; }

        public Point? location { get; set; }

        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }

        public bool isPublished { get; set; } = false;
        public DateTime? publishedAt { get; set; }

        // Lien avec un objet
        public Guid? identifiantTrail { get; set; }

        [ForeignKey(nameof(identifiantTrail))]
        public Trail? trail { get; set; }

        // Statut d'approbation du contenu
        [Required]
        public string approvalStatus { get; set; } = "pending"; // "pending", "approved", "rejected"

        public Guid? authorId { get; set; }

        public DateTime createdAt { get; set; } = DateTime.UtcNow;
        public DateTime? updatedAt { get; set; } = DateTime.UtcNow;
    }
}
