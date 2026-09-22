using MapMatrix.Entities;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace MapMatrix.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> users { get; set; } = null!;
        public DbSet<Trail> trails {  get; set; } = null!;
        public DbSet<Beacon> beacons { get; set; } = null!;
        public DbSet<TrailSchedule> trailSchedules { get; set; } = null!;
        public DbSet<TrailAssignment> trailAssignments { get; set; } = null!;
        public DbSet<GuidePosition> guidePositions { get; set; } = null!;
        public DbSet<GuideSession> guideSessions { get; set; } = null!;
        public DbSet<Emergency> emergencies { get; set; } = null!;
        public DbSet<Content> contents { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasPostgresEnum<UserRole>();
            modelBuilder.HasPostgresEnum<TrailStatus>();
            modelBuilder.HasPostgresEnum<TransportMode>();
            modelBuilder.HasPostgresEnum<SessionStatus>();
            modelBuilder.HasPostgresEnum<AssignmentStatus>();


            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Nom de la table en snake_case
                entity.SetTableName(ToSnakeCase(entity.GetTableName()));

                // Colonnes en snake_case
                foreach (var property in entity.GetProperties())
                {
                    // Ne pas renommer les propriétés de type enum
                    if (property.ClrType.IsEnum)
                        continue;

                    property.SetColumnName(ToSnakeCase(property.GetColumnName()));
                }

                // Clés étrangères
                foreach (var key in entity.GetKeys())
                {
                    key.SetName(ToSnakeCase(key.GetName()));
                }

                foreach (var foreignKey in entity.GetForeignKeys())
                {
                    foreignKey.SetConstraintName(ToSnakeCase(foreignKey.GetConstraintName()));
                }

                foreach (var index in entity.GetIndexes())
                {
                    index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()));
                }
            }

            // ======================
            // Configurations spécifiques
            // ======================

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasIndex(u => u.email).IsUnique();
                // role est maintenant un enum C# mappé sur l'enum PostgreSQL 'user_role'
                entity.Property(u => u.role).HasColumnName("role").HasColumnType("user_role");
            });

            modelBuilder.Entity<Trail>(entity =>
            {
                entity.ToTable("trails");

                entity.Property(t => t.transportMode)
                      .HasColumnName("transport_mode")
                      .HasColumnType("transport_mode");

                entity.Property(t => t.status)
                      .HasColumnName("status")
                      .HasColumnType("trail_status");

                entity.Property(t => t.approvalStatus)
                      .HasColumnName("approval_status")
                      .HasMaxLength(30);

                entity.Property(t => t.geometry)
                      .HasColumnType("geometry(LineString, 4326)");
            });

            modelBuilder.Entity<Beacon>(entity =>
            {
                entity.ToTable("beacons");
                entity.Property(b => b.position).HasColumnType("geometry(Point, 4326)");
            });

            modelBuilder.Entity<TrailSchedule>(entity =>
            {
                entity.ToTable("trail_schedules");
            });

            modelBuilder.Entity<TrailAssignment>(entity =>
            {
                entity.ToTable("trail_assignment");
                entity.Property(a => a.status).HasColumnName("status").HasColumnType("assignment_status");
            });

            modelBuilder.Entity<GuideSession>(entity =>
            {
                entity.ToTable("guide_sessions");
                entity.Property(s => s.status).HasColumnType("session_status");
                entity.Property(s => s.participantsCount).HasColumnName("participants_count");
            });

            modelBuilder.Entity<GuidePosition>(entity =>
            {
                entity.ToTable("guide_positions");
                entity.Property(p => p.position).HasColumnType("geometry(Point, 4326)");
            });

            modelBuilder.Entity<Emergency>(entity =>
            {
                entity.ToTable("emergencies");
                entity.Property(e => e.status).HasConversion<string>();
                entity.Property(e => e.position).HasColumnType("geometry(Point, 4326)");
            });

            modelBuilder.Entity<Content>(entity =>
            {
                entity.ToTable("contents");
                entity.Property(c => c.approvalStatus).HasConversion<string>();
                entity.Property(c => c.location).HasColumnType("geometry(Point, 4326)");
            });
        }

        // Helper pour convertir en snake_case
        private static string ToSnakeCase(string? input)
        {
            if (string.IsNullOrEmpty(input)) return input ?? "";

            return string.Concat(input.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? "_" + x : x.ToString()
            )).ToLower();
        }
    
    }
}
