using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MapMatrix.Entities
{
    public class User
    {
        [Key]
        [Column("id")]
        public Guid id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(255)]
        [Column("email")]
        public string email { get; set; } = null!;

        [Required]
        [Column("password_hash")]
        public string passwordHash { get; set; } = null!;

        [Required, MaxLength(100)]
        [Column("first_name")]
        public string firstName { get; set; } = null!;

        [Required, MaxLength(100)]
        [Column("last_name")]
        public string lastName { get; set; } = null!;

        [Required]
        [Column("role")]
        public UserRole role { get; set; } = UserRole.guide;

        [MaxLength(30)]
        [Column("phone")]
        public string? phone { get; set; }

        [Column("is_active")]
        public bool isActive { get; set; } = true;

        [Column("created_by")]
        public Guid? createdBy { get; set; }

        [ForeignKey(nameof(createdBy))]
        public User? creator { get; set; }

        [Column("last_login_at")]
        public DateTime? lastLoginAt { get; set; }

        [Column("created_at")]
        public DateTime createdAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;
    }
}