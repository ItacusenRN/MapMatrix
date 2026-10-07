using MapMatrix.Data;
using MapMatrix.DT0s;
using MapMatrix.Entities;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using System.Data.SqlTypes;

namespace MapMatrix.Services
{
    public class TrailService
    {
        private readonly AppDbContext _context;

        public TrailService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<TrailResponse> createAsync(CreateTrailRequest request, Guid userId, string userRole)
        {
            if (request.coordinates == null || request.coordinates.Count < 2)
                throw new ArgumentException("Le tracé doit contenir au moins 2 points.");

            var coords = new List<Coordinate>();

            foreach (var point in request.coordinates)
            {
                if (point == null || point.Count < 2)
                    throw new ArgumentException("Chaque point doit contenir longitude et latitude.");

                coords.Add(new Coordinate(point[0], point[1]));
            }

            var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
            var lineString = geometryFactory.CreateLineString(coords.ToArray());

            if(!Enum.TryParse<TransportMode>(request.transportMode, true, out var transportMode))
                transportMode = TransportMode.pied;

            var trail = new Trail
            {
                name = request.name,
                description = request.description,
                coverImageUrl = request.coverImageUrl,
                transportMode = Enum.TryParse<TransportMode>(request.transportMode, true, out var tm)
                        ? tm
                        : TransportMode.pied,
                difficulty = request.difficulty,
                distanceKm = request.distanceKm,
                geometry = lineString,
                status = TrailStatus.draft,
                approvalStatus = userRole == "admin" ? "approved" : "pending",
                createdBy = userId,
                publishedAt = userRole == "admin" ? DateTime.UtcNow : null
            };

            // Gestion de la durée estimée
            if(!string.IsNullOrEmpty(request.estimatedDuration) &&
                TimeSpan.TryParse(request.estimatedDuration, out var duration))
            {
                trail.estimatedDuration = duration;
            }

            _context.trails.Add(trail);
            await _context.SaveChangesAsync();

            return MapToResponse(trail);
        }

        public async Task<List<TrailResponse>> getAllAsync(string? userRole = null)
        {
            var query = _context.trails.AsQueryable();

            // Un guide ne voit que les trajets approuvés + les siens
            if(userRole == "guide")
            {
                // A laisser pour l'instant pour affiner plus tard
                // query = query.Where(t => t.approvalStatus == "approved" || t.createdBy == userRole);
            }

            var trails = await query.OrderByDescending(t => t.createdAt)
                .ToListAsync();

            return trails.Select(MapToResponse).ToList();
        }

        public async Task<TrailResponse?> getByIdAsync(Guid id)
        {
            var trail = await _context.trails.FindAsync(id);
            return trail == null ? null : MapToResponse(trail);
        }

        public async Task<TrailResponse?> updateApprovalAsync(Guid trailId, string approvalStatus, Guid admin)
        {
            var trail = await _context.trails.FindAsync(trailId);
            if (trail == null) return null;

            if (approvalStatus != "approved" && approvalStatus != "rejected")
                throw new ArgumentException("Invalid approval status.");

            trail.approvalStatus = approvalStatus;

            if (approvalStatus == "approved")
            {
                trail.status = TrailStatus.published;
                trail.publishedAt = DateTime.UtcNow;

            }
            else if (approvalStatus == "rejected")
            {
                trail.status = TrailStatus.draft;
                trail.publishedAt = DateTime.UtcNow;
            }

            trail.updatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return MapToResponse(trail);
        }

        private TrailResponse MapToResponse(Trail trail)
        {
            object? geometry = null;

            if(trail.geometry != null)
            {
                geometry = new
                {
                    type = "LineString",
                    coordinates = trail.geometry.Coordinates.Select(c => new[] { c.X, c.Y }).ToArray()
                };
            }

            return new TrailResponse
            {
                id = trail.id,
                name = trail.name,
                description = trail.description,
                coverImageUrl = trail.coverImageUrl,
                transportMode = trail.transportMode.ToString(),
                difficulty = trail.difficulty,
                distanceKm = trail.distanceKm,
                estimatedDuration = trail.estimatedDuration?.ToString(),
                status = trail.status.ToString(),
                approvalStatus = trail.approvalStatus,
                createdBy = trail.createdBy,
                publishedAt = trail.publishedAt,
                createdAt = trail.createdAt,
                geometry = geometry
            };
        }
    }
}
