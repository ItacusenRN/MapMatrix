using System.Security.Claims;
using MapMatrix.DT0s;
using MapMatrix.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace MapMatrix.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmergenciesController : ControllerBase
    {
        private readonly EmergencyService _emergencyService;

        public EmergenciesController(EmergencyService emergencyService)
        {
            _emergencyService = emergencyService;
        }

        // POST : api/emergencies (guide declenche)
        [Authorize(Roles ="guide")]
        [HttpPost]
        public async Task<IActionResult> trigger([FromBody] TriggerEmergencyRequest request)
        {
            var guideIdclaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (guideIdclaim == null || !Guid.TryParse(guideIdclaim, out Guid guideId))
                return Unauthorized();

            try
            {
                var result = await _emergencyService.triggerAsync(request, guideId);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT : api/emegencies/{id}/status (superviseur)
        [Authorize(Roles = "admin")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> updateStatus(Guid id, [FromBody] UpdateEmergencyStatusRequest request)
        {
            var supervisorIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (supervisorIdClaim == null || !Guid.TryParse(supervisorIdClaim, out Guid supervisorId))
                return Unauthorized();

            try
            {
                var result = await _emergencyService.updateStatusAsync(id, request.status, supervisorId);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //GET : api/emergencies (toutes)
        [Authorize(Roles ="admin")]
        [HttpGet]
        public async Task<IActionResult> getAll()
        {
            var result = await _emergencyService.getAllAsync();
            return Ok(result);
        }

        // GET : api/emergencies/open (ouvertes uniquement)
        [Authorize(Roles ="admin")]
        [HttpGet("open")]
        public async Task<IActionResult> getOpen()
        {
            var result = await _emergencyService.getOpenAsync();
            return Ok(result);
        }
    }
}
