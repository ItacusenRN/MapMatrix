using MapMatrix.Data;
using MapMatrix.DT0s;
using MapMatrix.Entities;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace MapMatrix.Services
{
    public class ContentService
    {
        private readonly AppDbContext _context;
         public ContentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ContentResponse> createAsync(CreateContentRequest request, Guid authorId, string role)
        {
            Point? location = null;
            if(request.longitude.HasValue && request.latitude.HasValue)
            {
                var factory = new GeometryFactory(new PrecisionModel(), 4326);
                location = factory.CreatePoint(new Coordinate(request.longitude.Value, request.latitude.Value));
            }

            var isAdmin = role == "admin";
            var content = new Content
            {
                type = request.type,
                title = request.title,
                slug = request.slug,
                summary = request.summary,
                body = request.body,
                imageUrl = request.imageUrl,
                location = location,
                startDate = request.startDate,
                endDate = request.endDate,
                trailId = request.trailId,
                authorId = authorId,
                approvalStatus = isAdmin ? "approved" : "pending",
                isPublished = isAdmin && request.publishNow,
                publishedAt = (isAdmin && request.publishNow) ? DateTime.UtcNow : null,
            };

            _context.contents.Add(content);
            await _context.SaveChangesAsync();
            return mapToResponse(content);
        }

        // Vitrine publique : publies et approuvés uniquement
        public async Task<List<ContentResponse>> getPublicAsync(string? type = null)
        {
            var query = _context.contents
                .Where(c => c.isPublished && c.approvalStatus == "approved");

            if(!string.IsNullOrEmpty(type))
                query = query.Where(c => c.type == type);

            var list = await query
                .OrderByDescending(c => c.publishedAt)
                .ToListAsync();

            return list.Select(mapToResponse).ToList();
        }

        public async Task<List<ContentResponse>> getAllAsync()
        {
            var list = await _context.contents
                .OrderByDescending(c => c.createdAt)
                .ToListAsync();
            return list.Select(mapToResponse).ToList();
        }

        public async Task<ContentResponse?> updateApprovalAsync(Guid id, UpdateContentApprovalRequest request)
        {
            var content = await _context.contents.FindAsync(id);
            if(content == null)
                return null;

            content.approvalStatus = request.approvalStatus;
            content.updatedAt = DateTime.UtcNow;

            if(request.approvalStatus == "approved" && request.publish)
            {
                content.isPublished = true;
                content.publishedAt = DateTime.UtcNow;
            }

            if(request.approvalStatus == "rejected")
            {
                content.isPublished = false;
            }

            await _context.SaveChangesAsync();
            return mapToResponse(content);
        }

        private static ContentResponse mapToResponse(Content c)
        {
            return new ContentResponse
            {
                id = c.id,
                type = c.type,
                title = c.title,
                slug = c.slug,
                summary = c.summary,
                body = c.body,
                imageUrl = c.imageUrl,
                longitude = c.location?.X,
                latitude = c.location?.Y,
                startDate = c.startDate,
                endDate = c.endDate,
                isPublished = c.isPublished,
                publishedAt = c.publishedAt,
                trailId = c.trailId,
                approvalStatus = c.approvalStatus,
                authorId = c.authorId,
                createdAt = c.createdAt
            };
        }

        public async Task<ContentResponse?> updateAsync(Guid id, updateContentRequest request, Guid userId, string role)
        {
            var content = await _context.contents.FindAsync(id);
            if (content == null) return null;

            if (role != "admin" && content.authorId != userId)
                throw new UnauthorizedAccessException("Non autorisé.");

            if (request.type != null) content.type = request.type;
            if (request.title != null) content.title = request.title;
            if (request.slug != null) content.slug = request.slug;
            if (request.summary != null) content.summary = request.summary;
            if (request.body != null) content.body = request.body;
            if (request.imageUrl != null) content.imageUrl = request.imageUrl;
            if (request.startDate.HasValue) content.startDate = request.startDate;
            if (request.endDate.HasValue) content.endDate = request.endDate;
            if (request.trailId.HasValue) content.trailId = request.trailId;

            if (request.longitude.HasValue && request.latitude.HasValue)
            {
                var factory = new GeometryFactory(new PrecisionModel(), 4326);
                content.location = factory.CreatePoint(
                    new Coordinate(request.longitude.Value, request.latitude.Value));
            }

            // Si un guide modifie → repasse en pending
            if (role == "guide")
            {
                content.approvalStatus = "pending";
                content.isPublished = false;
            }

            content.updatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return mapToResponse(content);
        }

        public async Task<bool> deleteAsync(Guid id, Guid userId, string role)
        {
            var content = await _context.contents.FindAsync(id);
            if (content == null) return false;

            if (role != "admin" && content.authorId != userId)
                throw new UnauthorizedAccessException("Non autorisé.");

            _context.contents.Remove(content);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
