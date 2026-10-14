using MapMatrix.Data;
using MapMatrix.DT0s;
using MapMatrix.Entities;
using Microsoft.EntityFrameworkCore;

namespace MapMatrix.Services
{
    public class AssignmentService
    {
        private readonly AppDbContext _context;

        public AssignmentService(AppDbContext context)
        {
            _context = context;
        }

        // === Le superviseur assigne un guide a un trajet ===

        public async Task<AssignmentResponse> assignBySupervisorAsync(
            CreateAssignmentRequest request, Guid supervisorId)
        {
            await validateTrailAndGuide(request.trailId, request.guideId);

            var trail = await _context.trails.FindAsync(request.trailId);
            if (trail == null)
                throw new ArgumentException("trajet introuvable.");

            // 2. Controle mode de transport <- ICI
            var guide = await _context.users.FindAsync(request.guideId);
            if (guide == null)
                throw new ArgumentException("Guide introuvable.");

            var modes = (guide.allowedTransportModes ?? "pied,velo,moto")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var trailMode = trail.transportMode.ToString();
            if (!modes.Any(m => string.Equals(m, trailMode, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(
                    $"Ce guide n'est pas habilite pour le mode <<  {trailMode} >>.");
            }

            await checkConflictAsync(request.guideId, request.scheduledDate, null, request.trailId);

            var assignment = new TrailAssignment
            {
                trailId = request.trailId,
                guideId = request.guideId,
                assignedBy = supervisorId,
                status = AssignmentStatus.approved,   // assignation directe = approved
                requestedByGuide = false,
                scheduledDate = request.scheduledDate,
                notes = request.notes
            };

            _context.trailAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            return await mapToResponse(assignment);
        }

        // === Le guide postule pour le trajet ===
        public async Task<AssignmentResponse> applyByGuideAsync(
            ApplyAssignmentRequest request, Guid guideId)
        {
            await validateTrailAndGuide(request.trailId, guideId);

            var trail = await _context.trails.FindAsync(request.trailId);
            if(trail == null)
                throw new ArgumentException("trajet introuvable.");

            // Controle mode <- ICI
            var guide = await _context.users.FindAsync(guideId);
            if(guide == null)
                throw new ArgumentException("Guide introuvable.");

            var modes = (guide.allowedTransportModes ?? "pied,velo,moto")
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var trailMode = trail.transportMode.ToString();
            if (!modes.Any(m => string.Equals(m, trailMode, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException(
                    $"Ce guide n'est pas habilite pour le mode <<  {trailMode} >>.");
            }

            await checkConflictAsync(guideId, request.scheduledDate, null, request.trailId);

            var assignment = new TrailAssignment
            {
                trailId = request.trailId,
                guideId = guideId,
                assignedBy = null,
                status = AssignmentStatus.pending,
                requestedByGuide = true,
                scheduledDate = request.scheduledDate,
                notes = request.notes
            };

            _context.trailAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            return await mapToResponse(assignment);
        }

        // === Reponse change le status (approve / reject / cancel) ===
        public async Task<AssignmentResponse?> updateStatusAsync(
            Guid assignmentId, string newStatus, Guid supervisorId)
        {
            if (!Enum.TryParse<AssignmentStatus>(newStatus, true, out var status))
                throw new ArgumentException("Status invalide. Valeurs possibles : approved, rejected, cancelled");

            var assignment = await _context.trailAssignments.FindAsync(assignmentId);
            if (assignment == null) return null;

            // Si on approuve, on revérifie les conflits
            if (status == AssignmentStatus.approved)
            {
                await checkConflictAsync(assignment.guideId, assignment.scheduledDate, assignment.id, assignment.trailId);
            }

            assignment.status = status;
            assignment.assignedBy = supervisorId;
            assignment.updatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return await mapToResponse(assignment);
        }

        // === Liste des assignations (filtrable) ===
        public async Task<List<AssignmentResponse>> getAllAsync(
            Guid? guideId = null, Guid? trailId = null, string? status = null)
        {
            var query = _context.trailAssignments.AsQueryable();

            if (guideId.HasValue)
                query = query.Where(a => a.guideId == guideId.Value);

            if (trailId.HasValue)
                query = query.Where(a => a.trailId == trailId.Value);

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<AssignmentStatus>(status, true, out var st))
                query = query.Where(a => a.status == st);

            var list = await query.OrderByDescending(a => a.createdAt).ToListAsync();

            var result = new List<AssignmentResponse>();
            foreach (var a in list)
                result.Add(await mapToResponse(a));

            return result;
        }

        // === Regle de conflit :
        // * Un guide ne peut pas avoir deux assignations approved
        // * Qui se chevauchent dans le temps ===
        private async Task checkConflictAsync(
    Guid guideId,
    DateTime? scheduledDate,
    Guid? excludeAssignmentId = null,
    Guid? trailId = null)
        {
            // Session déjà active
            var hasActiveSession = await _context.guideSessions.AnyAsync(s =>
                s.guideId == guideId &&
                (s.status == SessionStatus.planned
                 || s.status == SessionStatus.in_progress
                 || s.status == SessionStatus.emergency));

            if (hasActiveSession)
                throw new InvalidOperationException("Ce guide a déjà une session planifiée ou en cours.");

            var query = _context.trailAssignments
                .Where(a => a.guideId == guideId && a.status == AssignmentStatus.approved);

            if (excludeAssignmentId.HasValue)
                query = query.Where(a => a.id != excludeAssignmentId.Value);

            var existing = await query.ToListAsync();
            if (!existing.Any())
                return;

            if (scheduledDate == null)
                throw new InvalidOperationException(
                    "Ce guide a déjà une assignation active. Indiquez une date de départ ou annulez l'assignation existante.");

            TimeSpan newDuration = TimeSpan.FromHours(4);
            if (trailId.HasValue)
            {
                var trail = await _context.trails.FindAsync(trailId.Value);
                if (trail?.estimatedDuration != null)
                    newDuration = trail.estimatedDuration.Value;
            }

            var newStart = scheduledDate.Value;
            var newEnd = newStart.Add(newDuration);

            foreach (var a in existing)
            {
                if (!a.scheduledDate.HasValue)
                    throw new InvalidOperationException(
                        "Ce guide a une assignation approuvée sans date. Annulez-la ou renseignez la date.");

                TimeSpan otherDuration = TimeSpan.FromHours(4);
                var otherTrail = await _context.trails.FindAsync(a.trailId);
                if (otherTrail?.estimatedDuration != null)
                    otherDuration = otherTrail.estimatedDuration.Value;

                var otherStart = a.scheduledDate.Value;
                var otherEnd = otherStart.Add(otherDuration);

                bool overlap = newStart < otherEnd && otherStart < newEnd;
                if (overlap)
                    throw new InvalidOperationException(
                        $"Conflit horaire avec l'assignation du {otherStart:u} (trajet « {otherTrail?.name} »).");
            }
        }

        public async Task<List<AssignmentResponse>> getMyAssignmentsAsync(Guid guideId)
        {
            var list = await _context.trailAssignments
                .Where(a => a.guideId == guideId)
                .OrderByDescending(a => a.createdAt)
                .ToListAsync();

            var result = new List<AssignmentResponse>();
            foreach (var a in list)
            {
                result.Add(await mapToResponse(a));
            }
            return result;
        }

        private async Task validateTrailAndGuide(Guid trailId, Guid guideId)
        {
            var trail = await _context.trails.FindAsync(trailId);
            if (trail == null)
                throw new ArgumentException("Le trajet spécifié n'existe pas.");

            var guide = await _context.users.FindAsync(guideId);
            if (guide == null || guide.role != UserRole.guide)
                throw new ArgumentException("Le guide spécifié n'existe pas ou n'est pas un guide.");
        }

        public async Task<List<GuideListItemResponse>> getGuideAsync()
        {
            var guides = await _context.users
                .Where(u => u.role == UserRole.guide && u.isActive)
                .OrderBy(u => u.lastName)
                .ThenBy(u => u.firstName)
                .ToListAsync();

            var result = new List<GuideListItemResponse>();

            foreach(var g in guides)
            {
                // Session en cours ou planifiee
                var activeSession = await _context.guideSessions
                    .Where(s => s.guideId == g.id
                    && (s.status == SessionStatus.planned
                    || s.status == SessionStatus.in_progress
                    || s.status == SessionStatus.emergency))
                    .OrderByDescending(s => s.createdAt)
                    .FirstOrDefaultAsync();

                // Assignation approuvee (occupe meme sans session demarree)
                var activeAssignment = await _context.trailAssignments
                    .Where(a => a.guideId == g.id && 
                    a.status == AssignmentStatus.approved
                    && (a.scheduledDate == null
                    || a.scheduledDate.Value.Date == DateTime.UtcNow.Date
                    ||Math.Abs((a.scheduledDate.Value - DateTime.UtcNow).TotalDays) < 1))
                    .OrderByDescending(a => a.createdAt)
                    .FirstOrDefaultAsync();

                string? trailName = null;
                if(activeSession != null)
                {
                    var trail = await _context.trails.FindAsync(activeSession.trailId);
                    trailName = trail?.name;
                }
                else if(activeAssignment != null)
                {
                    var trail = await _context.trails.FindAsync(activeAssignment.trailId);
                    trailName = trail?.name;
                }

                var isAvailable = activeSession == null && activeAssignment == null;

                result.Add(new GuideListItemResponse
                {
                    id = g.id,
                    email = g.email,
                    firstName = g.firstName,
                    lastName = g.lastName,
                    phone = g.phone,
                    isActive = g.isActive,
                    isAvailable = isAvailable,
                    currentAssignmentId = activeAssignment?.id,
                    currentSessionId = activeSession?.id,
                    currentTrailName = trailName
                });
            }

            return result;
        }

        private async Task<AssignmentResponse> mapToResponse(TrailAssignment a)
        {
            var trail = await _context.trails.FindAsync(a.trailId);
            var guide = await _context.users.FindAsync(a.guideId);

            return new AssignmentResponse
            {
                id = a.id,
                trailId = a.trailId,
                trailName = trail?.name ?? "",
                guideId = a.guideId,
                guideName = guide != null ? $"{guide.firstName} {guide.lastName}" : "",
                assignedBy = a.assignedBy,
                status = a.status.ToString(),
                requestedByGuide = a.requestedByGuide,
                scheduledDate = a.scheduledDate,
                notes = a.notes,
                createdAt = a.createdAt
            };
        }
    }
}
