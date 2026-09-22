using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MapMatrix.Entities
{
    public class TrailSchedule
    {
        [Key]
        public Guid identifiantTSchedule { get; set; } = Guid.NewGuid();

        [Required]
        public Guid trailId { get; set; }

        [ForeignKey(nameof(trailId))]
        public Trail trail { get; set; } = null!;

        public short? jourDeLaSemaine { get; set; } // 0 à 6 (dimanche à samedi)

        [Required]
        public TimeOnly depart { get; set; }

        public TimeOnly? arrivee { get; set; }

        public DateTime? DateSpecifique { get; set; }

        public int? participants { get; set; }

        public string? notes { get; set; }
        
        public DateTime creerA { get; set; } = DateTime.UtcNow;
    }
}
