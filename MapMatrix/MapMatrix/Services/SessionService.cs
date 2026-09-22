
using MapMatrix.Data;
using MapMatrix.DT0s;
using MapMatrix.Entities;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace MapMatrix.Services
{
    public class SessionService
    {
        private readonly AppDbContext _context;

        public SessionService(AppDbContext context)
        {
            _context = context;
        }

        // Creation d'une session de guide
        public async Task<SessionResponse> createSessionAsync(CreateSessionRequest request,Guid guideId)
        {
            var trail = await _context.trails.FindAsync(request.trailId);
            if(trail == null)
                throw new ArgumentException("Le trajet specifique n'existe pas.");

            var session = new GuideSession
            {
                guideId = guideId,
                trailId = request.trailId,
                scheduleId = request.scheduleId,
                status = SessionStatus.planned,
                participantsCount = request.participantsCount,
                plannedStart = request.plannedStart,
                notes = request.notes
            };

            _context.guideSessions.Add(session);
            await _context.SaveChangesAsync();

            return await mapToSessionResponse(session);
        }

        // Demarrage de la session
        public async Task<SessionResponse> startSessionAsync(Guid sessionId, Guid guideId)
        {
            var session = await _context.guideSessions
                .FirstOrDefaultAsync(s => s.id == sessionId && s.guideId == guideId);

            if (session == null) return null;

            if(session.status != SessionStatus.planned)
                throw new InvalidOperationException("La session ne peut pas être demarree.");

            session.status = SessionStatus.in_progress;
            session.startedAt = DateTime.UtcNow;
            session.updatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await mapToSessionResponse(session);
        }

        // Terminer une session
        public async Task<SessionResponse> endSessionAsync(Guid sessionId, Guid guideId)
        {
            var session = await _context.guideSessions
                .FirstOrDefaultAsync(s => s.id == sessionId && s.guideId == guideId);

            if (session == null) return null;

            session.status = SessionStatus.completed;
            session.endedAt = DateTime.UtcNow;
            session.updatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await mapToSessionResponse(session);
        }

        // Envoi de la position du guide
        public async Task<PositionResponse> sendPositionAsync(SendPositionRequest request, Guid guideId)
        {
            var session = await _context.guideSessions
                .FirstOrDefaultAsync(s => s.id == request.sessionId && s.guideId == guideId);

            if (session == null)
                throw new ArgumentException("Session introuvableou non autorisee.");

            if(session.status != SessionStatus.in_progress && session.status != SessionStatus.emergency)
                throw new InvalidOperationException("La session doit etre en cours pour envoyer la position.");

            var geomtryFactory = new GeometryFactory(new PrecisionModel(), 4326);
            var point = geomtryFactory.CreatePoint(new Coordinate(request.longitude, request.latitude));

            var position = new GuidePosition
            {
                sessionId = request.sessionId,
                guideId = guideId,
                position = point,
                altitude = request.altitude,
                accuracy = request.accuracy,
                speed = request.speed,
                heading = request.heading,
                recordedAt = DateTime.UtcNow
            };

            _context.guidePositions.Add(position);
            await _context.SaveChangesAsync();

            var guide = await _context.users.FindAsync(guideId);

            return new PositionResponse
            {
                id = position.id,
                sessionId = position.sessionId,
                guideId = position.guideId,
                guideName = guide != null ? $"{guide.firstName} {guide.lastName}" : "",
                longitude = position.position.X,
                latitude = position.position.Y,
                altitude = position.altitude,
                accuracy = position.accuracy,
                speed = position.speed,
                heading = position.heading,
                recordedAt = position.recordedAt
            };
        
        }

        // Position actuelle des guides en session (pour le superviseur seul !)
        public async Task<List<PositionResponse>> getLivePositionAsync()
        {
            var activeSessions = await _context.guideSessions
                .Where(s => s.status == SessionStatus.in_progress || s.status == SessionStatus.emergency)
                .Include(s => s.guide)
                .ToListAsync();

            var result = new List<PositionResponse>();

            foreach(var session in activeSessions)
            {
                var lastPosition = await _context.guidePositions
                    .Where(p => p.sessionId == session.id)
                    .OrderByDescending(p => p.recordedAt)
                    .FirstOrDefaultAsync();

                if(lastPosition != null)
                {
                    result.Add(new PositionResponse
                    {
                        id = lastPosition.id,
                        sessionId = lastPosition.sessionId,
                        guideId = lastPosition.guideId,
                        guideName = $"{session.guide.firstName} {session.guide.lastName}",
                        longitude = lastPosition.position.X,
                        latitude = lastPosition.position.Y,
                        altitude = lastPosition.altitude,
                        accuracy = lastPosition.accuracy,
                        speed = lastPosition.speed,
                        heading = lastPosition.heading,
                        recordedAt = lastPosition.recordedAt
                    });
                }
            }
            return result;
        }

        // Liste des sessions d'un guide
        public async Task<List<SessionResponse>> getMySessionsAsync(Guid guideId)
        {
            var sessions = await _context.guideSessions
                .Where(s => s.guideId == guideId)
                .OrderByDescending(s => s.createdAt)
                .ToListAsync();

            var result = new List<SessionResponse>();
            foreach(var s in sessions)
            {
                result.Add(await mapToSessionResponse(s));
            }
            return result;
        }

        // Helper
        private async Task<SessionResponse> mapToSessionResponse(GuideSession session)
        {
            var trail = await _context.trails.FindAsync(session.trailId);

            return new SessionResponse
            {
                id = session.id,
                guideId = session.guideId,
                trailId = session.trailId,
                trailName = trail?.name ?? "",
                status = session.status.ToString(),
                participantsCount = session.participantsCount,
                startedAt = session.startedAt,
                endedAt = session.endedAt,
                plannedStart = session.plannedStart,
                notes = session.notes,
                createdAt = session.createdAt
            };
        }
    }
}
