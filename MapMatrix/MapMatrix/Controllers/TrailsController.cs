using MapMatrix.DT0s;
using MapMatrix.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MapMatrix.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrailsController : ControllerBase
    {
        private readonly TrailService _trailService;

        public TrailsController(TrailService trailService)
        {
            _trailService = trailService;
        }

        // POST : api/trails
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> create([FromBody] CreateTrailRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out Guid userId) || roleClaim == null)
                return Unauthorized();

            try
            {
                var result = await _trailService.createAsync(request, userId, roleClaim);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET : api/trails
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> getAll()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = null;
            if (Guid.TryParse(userIdClaim, out var id))
                userId = id;

            var result = await _trailService.getAllAsync(role);
            return Ok(result);
        }

        // GET : api/trails/{id}
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> getById(Guid id)
        {
            var result = await _trailService.getByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // PUT : api/trails/{id}/approval
        [Authorize(Roles = "admin")]
        [HttpPut("{id}/approval")]
        public async Task<IActionResult> updateApproval(Guid id, [FromBody] UpdateApprovalRequest request)
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (adminIdClaim == null || !Guid.TryParse(adminIdClaim, out Guid adminId))
                return Unauthorized();

            try
            {
                var result = await _trailService.updateApprovalAsync(id, request.approvalStatus, adminId);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}