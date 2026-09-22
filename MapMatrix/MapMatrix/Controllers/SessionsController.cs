using System.Security.Claims;
using MapMatrix.DT0s;
using MapMatrix.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MapMatrix.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly SessionService _sessionService;

        public SessionsController(SessionService sessionService)
        {
            _sessionService = sessionService;
        }

        // POST: api/session
        //[Authorize(Roles = "guide")]
        [HttpPost]
        public async Task<IActionResult> create([FromBody] CreateSessionRequest request)
        {
            var guideIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (guideIdClaim == null || !Guid.TryParse(guideIdClaim, out Guid guideId))
                return Unauthorized();

            try
            {
                var result = await _sessionService.createSessionAsync(request, guideId);
                return Ok(result);
            }
            catch(ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT : api/session/{id}/start
        [Authorize(Roles = "guide")]
        [HttpPut("{id}/start")]
        public async Task<IActionResult> start(Guid id)
        {
            var guideIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(guideIdClaim == null || !Guid.TryParse(guideIdClaim, out Guid guideId))
                return Unauthorized();

            try
            {
                var result = await _sessionService.startSessionAsync(id, guideId);
                if(result == null) return NotFound();
                return Ok(result);
            }
            catch(InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT : api/session/{id}/end
        [Authorize(Roles = "guide")]
        [HttpPut("{id}/end")]
        public async Task<IActionResult> end(Guid id)
        {
            var guideIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(guideIdClaim == null || !Guid.TryParse(guideIdClaim, out Guid guideId))
                return Unauthorized();

            var result = await _sessionService.endSessionAsync(id, guideId);
            if(result == null) return NotFound();
            return Ok(result);
        }

        // GET : api/session/my
        [Authorize(Roles = "guide")]
        [HttpGet("my")]
        public async Task<IActionResult> getMySessions()
        {
            var guideIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (guideIdClaim == null || !Guid.TryParse(guideIdClaim, out Guid guideId))
                return Unauthorized();

            var result = await _sessionService.getMySessionsAsync(guideId);
            return Ok(result);
        }

        // POST : api/session/position
        [Authorize(Roles = "guide")]
        [HttpPost("position")]
        public async Task<IActionResult> sendPosition([FromBody] SendPositionRequest request)
        {
            var guideIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(guideIdClaim == null || !Guid.TryParse(guideIdClaim, out Guid guideId))
                return Unauthorized();

            try
            {
                var result = await _sessionService.sendPositionAsync(request, guideId);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET : api/sessions/live-positions (pour le superviseur)
        [Authorize(Roles = "admin")]
        [HttpGet("live-positions")]
        public async Task<IActionResult> getLivePosition()
        {
            var result = await _sessionService.getLivePositionAsync();
            return Ok(result);
        }
    }
}
