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
            await checkConflictAsync(request.guideId, request.scheduledDate);

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
            await checkConflictAsync(guideId, request.scheduledDate);

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
                await checkConflictAsync(assignment.guideId, assignment.scheduledDate, assignment.id);
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
            Guid guideId, DateTime? scheduledDate, Guid? excludeAssignmentId = null)
        {
            // Si aucune date n'est fournie, on considère qu'il ne doit
            // pas y avoir d'autre assignation approved "ouverte"
            var query = _context.trailAssignments
                .Where(a => a.guideId == guideId
                         && a.status == AssignmentStatus.approved);

            if (excludeAssignmentId.HasValue)
                query = query.Where(a => a.id != excludeAssignmentId.Value);

            var existing = await query.ToListAsync();

            if (!existing.Any()) return;

            // Si la nouvelle assignation n'a pas de date → conflit dès qu'il existe déjà une approved
            if (!scheduledDate.HasValue)
            {
                throw new InvalidOperationException(
                    "Ce guide a déjà une assignation active. Précisez une date éloignée ou attendez qu'il soit libre.");
            }

            // Sinon on vérifie un chevauchement simple (± 1 jour par défaut)
            // Tu pourras affiner plus tard avec la durée réelle du trajet
            foreach (var a in existing)
            {
                if (!a.scheduledDate.HasValue)
                {
                    throw new InvalidOperationException(
                        "Ce guide a déjà une assignation active sans date précise.");
                }

                var diff = (scheduledDate.Value - a.scheduledDate.Value).Duration();
                if (diff.TotalHours < 24) // moins de 24h d'écart → conflit
                {
                    throw new InvalidOperationException(
                        $"Conflit : le guide est déjà assigné le {a.scheduledDate:dd/MM/yyyy HH:mm}.");
                }
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
