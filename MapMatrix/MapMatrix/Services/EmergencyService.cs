using MapMatrix.Data;
using MapMatrix.DT0s;
using MapMatrix.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using NetTopologySuite.Geometries;

namespace MapMatrix.Services
{
    public class EmergencyService
    {
        private readonly AppDbContext _context;
        
        public EmergencyService (AppDbContext context)
        {
            _context = context;
        }

        // === Le guide declenche une urgence ===
        public async Task<EmergencyResponse> triggerAsync(TriggerEmergencyRequest request, Guid guideId)
        {
            var session = await _context.guideSessions
                .FirstOrDefaultAsync(s => s.id == request.sessionId && s.guideId == guideId);

            if (session == null)
                throw new ArgumentException("session introuvable ou non autorisee.");

            if (session.status != SessionStatus.in_progress && session.status != SessionStatus.emergency)
                throw new InvalidOperationException("La session doit etre en cours pour declencher une urgence.");

            // Passer la session en mode emergency
            session.status = SessionStatus.emergency;
            session.updatedAt = DateTime.UtcNow;

            var geometryFaectory = new GeometryFactory(new PrecisionModel(), 4326);
            var point = geometryFaectory.CreatePoint(new Coordinate(request.longitude, request.latitude));

            var emergency = new Emergency
            {
                sessionId = request.sessionId,
                guideId = guideId,
                position = point,
                message = request.message,
                status = EmergencyStatus.open,
                createdAt = DateTime.UtcNow
            };

            _context.emergencies.Add(emergency);
            await _context.SaveChangesAsync();

            return await mapToResponse(emergency);
        }

        // === changement du status par le superviseur ===
        public async Task<EmergencyResponse?> updateStatusAsync(Guid emergencyId, string newStatus, Guid supervisorId)
        {
            var emergency = await _context.emergencies.FindAsync(emergencyId);
            if (emergency == null) return null;

            if (!Enum.TryParse<EmergencyStatus>(newStatus, true, out var status))
                throw new ArgumentException("Status invalide, valeurs : acknowledged, resolved, cancelled");

            emergency.status = status;
            emergency.handledBy = supervisorId;

            if (status == EmergencyStatus.acknowledged)
                emergency.acknowledgedAt = DateTime.UtcNow;

            // Si resolu, la session peut etre remis en in_progress
            if(status == EmergencyStatus.resolved)
            {
                var session = await _context.guideSessions.FindAsync(emergency.sessionId);
                if(session != null && session.status == SessionStatus.emergency)
                {
                    session.status = SessionStatus.in_progress;
                    session.updatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
            return await mapToResponse(emergency);
        }

        // === Liste des urgences (Supervisur) ===
        public async Task<List<EmergencyResponse>> getAllAsync()
        {
            var list = await _context.emergencies
                .OrderByDescending(e => e.createdAt)
                .ToListAsync();

            var result = new List<EmergencyResponse>();
            foreach(var e in list)
            {
                result.Add(await mapToResponse(e));
            }
            return result;
        }

        // === Urgences ouvertes uniquement ===
        public async Task<List<EmergencyResponse>> getOpenAsync()
        {
            var list = await _context.emergencies
                .Where(e => e.status == EmergencyStatus.open || e.status == EmergencyStatus.acknowledged)
                .OrderByDescending(e => e.createdAt)
                .ToListAsync();

            var result = new List<EmergencyResponse>();
            foreach(var e in list)
            {
                result.Add(await mapToResponse(e));
            }
            return result;
        }

        // === Mapping ===
        private async Task<EmergencyResponse> mapToResponse(Emergency e)
        {
            var guide = await _context.users.FindAsync(e.guideId);
            var session = await _context.guideSessions.FindAsync(e.sessionId);
            var trail = session != null ? await _context.trails.FindAsync(session.trailId) : null;

            return new EmergencyResponse
            {
                id = e.id,
                sessionId = e.sessionId,
                guideId = e.guideId,
                guideName = guide != null ? $"{guide.firstName}{guide.lastName}" : "",
                trailName = trail?.name ?? "",
                longitude = e.position.X,
                latitude = e.position.Y,
                message = e.message,
                status = e.status.ToString(),
                handledBy = e.handledBy,
                triggeredAt = e.createdAt,
                resolvedAt = e.acknowledgedAt
            };
        }
    }
}
