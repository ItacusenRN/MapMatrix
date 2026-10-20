using System.Security.Claims;
using MapMatrix.DT0s;
using MapMatrix.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MapMatrix.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContentsController : ControllerBase
    {
        private readonly ContentService _contentService;
        public ContentsController(ContentService contentService)
        {
            _contentService = contentService;
        }

        // Public - vitrine (pas de login)
        [AllowAnonymous]
        [HttpGet("public")]
        public async Task<IActionResult> getAction([FromQuery] string? type = null)
        {
            var result = await _contentService.getPublicAsync(type);
            return Ok(result);
        }

        // Admin - tout voir
        [Authorize(Roles ="admin")]
        [HttpGet]
        public async Task<IActionResult> getAll()
        {
            var result = await _contentService.getAllAsync();
            return Ok(result);
        }

        // Admin ou guide - creer (guide -> pending)
        [Authorize(Roles ="admin,guide")]
        [HttpPost]
        public async Task<IActionResult> create([FromBody] CreateContentRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized();

            var result = await _contentService.createAsync(request, userId, role);
            return Ok(result);
        }

        // Admin - approuver / rejeter / publier
        [Authorize(Roles ="admin")]
        [HttpPut("{id}/approval")]
        public async Task<IActionResult> updateApproval(Guid id, [FromBody] UpdateContentApprovalRequest request)
        {
            var result = await _contentService.updateApprovalAsync(id, request);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // Update
        [Authorize(Roles = "admin,guide")]
        [HttpPut("{id}")]
        public async Task<IActionResult> update(Guid id, [FromBody] updateContentRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized();

            try
            {
                var result = await _contentService.updateAsync(id, request, userId, role);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // Delete
        [Authorize(Roles = "admin,guide")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> delete(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized();

            try
            {
                var ok = await _contentService.deleteAsync(id, userId, role);
                if (!ok) return NotFound();
                return Ok(new { message = "Contenu supprimé" });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

    }
}
