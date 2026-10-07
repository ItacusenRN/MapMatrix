using System.Security.Claims;
using MapMatrix.DT0s;
using MapMatrix.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MapMatrix.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssignmentsController : ControllerBase
    {
        private readonly AssignmentService _assignmentService;

        public AssignmentsController(AssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        // POST : api/assignments (assignement par le superviseur)
        [Authorize(Roles ="admin")]
        [HttpPost]
        public async Task<IActionResult> assign([FromBody] CreateAssignmentRequest request)
        {
            var supervisorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (supervisorIdClaim == null || !Guid.TryParse(supervisorIdClaim, out Guid supervisorId))
                return Unauthorized();

            try
            {
                var result = await _assignmentService.assignBySupervisorAsync(request, supervisorId);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST : api/assignments/apply (le guide postule)
        [Authorize(Roles ="guide")]
        [HttpPost("apply")]
        public async Task<IActionResult> apply([FromBody] ApplyAssignmentRequest request)
        {
            var guideIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (guideIdClaim == null || !Guid.TryParse(guideIdClaim, out Guid guideId))
                return Unauthorized();

            try
            {
                var result = await _assignmentService.applyByGuideAsync(request, guideId);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT : api/assignments/{id}/status (changement du statut par le superviseur)
        [Authorize(Roles ="admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> updateStatus(Guid id, [FromBody] UpdateAssignmentStatusRequest request)
        {
            var supervisorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (supervisorIdClaim == null || !Guid.TryParse(supervisorIdClaim, out Guid supervisorId))
                return Unauthorized();

            try
            {
                var result = await _assignmentService.updateStatusAsync(id, request.status, supervisorId);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET : api/assignments (le superviseur voit tout)
        [Authorize(Roles ="admin")]
        [HttpGet]
        public async Task<IActionResult> getAll()
        {
            var result = await _assignmentService.getAllAsync();
            return Ok(result);
        }

        //GET :api/assignments/my (le guide voit ses assignations)
        [Authorize(Roles ="guide")]
        [HttpGet("my")]
        public async Task<IActionResult> getMy()
        {
            var guideIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (guideIdClaim == null || !Guid.TryParse(guideIdClaim, out Guid guideId))
                return Unauthorized();

            var result = await _assignmentService.getMyAssignmentsAsync(guideId);
            return Ok(result);
        }
    }
}
