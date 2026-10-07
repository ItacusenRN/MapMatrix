using System.Security.Claims;
using MapMatrix.DT0s;
using MapMatrix.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MapMatrix.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BeaconsController : ControllerBase
    {
        private readonly BeaconService _beaconService;

        public BeaconsController(BeaconService beaconService)
        {
            _beaconService = beaconService;
        }

        // POST : api/beacons - guide ou admin
        [Authorize(Roles = "guide,admin")]
        [HttpPost]
        public async Task<IActionResult> create([FromBody] CreateBeaconRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized();

            try
            {
                var result = await _beaconService.createAsync(request, userId);
                return Ok(result);
            }
            catch(ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET : api/beacons/trail/{trailId}
        [Authorize]
        [HttpGet("trail/{trailId}")]
        public async Task<IActionResult> getBytrail(Guid trailId)
        {
            var result = await _beaconService.getByTrailAsync(trailId);
            return Ok(result);
        }

        // DELETE : api/beacons/{id}
        [Authorize(Roles = "guide,admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> delete(Guid id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ??"";
            if (role == null || !Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized();

            try
            {
                var ok = await _beaconService.deleteAsync(id, userId, role);
                if (!ok) return NotFound();
                return Ok(new { message = "Balise supprimee" });
            }
            catch(UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
