using MapMatrix.Data;
using MapMatrix.DT0s;
using MapMatrix.Entities;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace MapMatrix.Services
{
    public class BeaconService
    {
        private readonly AppDbContext _context;

        public BeaconService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BeaconResponse> createAsync(CreateBeaconRequest request, Guid userId)
        {
            var trail = await _context.trails.FindAsync(request.trailId);
            if (trail == null)
                throw new ArgumentException("Le trajet n'existe pas.");

            var factory = new GeometryFactory(new PrecisionModel(), 4326);
            var point = factory.CreatePoint(new Coordinate(request.longitude, request.latitude));

            var beacon = new Beacon
            {
                trailId = request.trailId,
                name = request.name,
                description = request.description,
                position = point,
                sequenceOrder = request.sequenceOrder,
                estReference = request.estReference,
                createdBy = userId
            };

            _context.beacons.Add(beacon);
            await _context.SaveChangesAsync();

            return mapToResponse(beacon);
        }

        public async Task<List<BeaconResponse>> getByTrailAsync(Guid trailId)
        {
            var list = await _context.beacons
                .Where(b => b.trailId == trailId)
                .OrderBy(b => b.sequenceOrder)
                .ThenBy(b => b.createdAt)
                .ToListAsync();

            return list.Select(mapToResponse).ToList();
        }

        public async Task<bool> deleteAsync(Guid id, Guid userId, string role)
        {
            var beacon = await _context.beacons.FindAsync(id);
            if(beacon == null) return false;

            if (role != "admin" && beacon.createdBy != userId)
                throw new UnauthorizedAccessException("Non autorise a supprimer cette balise.");

            _context.beacons.Remove(beacon);
            await _context.SaveChangesAsync();
            return true;
        }

        private static BeaconResponse mapToResponse(Beacon b)
        {
            return new BeaconResponse
            {
                id = b.id,
                trailId = b.trailId,
                name = b.name,
                description = b.description,
                longitude = b.position.X,
                latitude = b.position.Y,
                sequenceOrder = b.sequenceOrder,
                estReference = b.estReference,
                createdAt = b.createdAt
            };
        }
    }
}
