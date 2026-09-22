using MapMatrix.DT0s;
using MapMatrix.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MapMatrix.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // ======================
        // POST : api/auth/create-first-admin
        // A utiliser UNE SEULE FOIS pour creer le premier superviseur
        // ======================
        [HttpPost("create-first-admin")]
        public async Task<IActionResult> createFirstAdmin([FromBody] CreateGuideRequest request)
        {
            if(string.IsNullOrWhiteSpace(request.email) ||
                string.IsNullOrWhiteSpace(request.lastName) ||
                string.IsNullOrWhiteSpace(request.firstName))
            {
                return BadRequest(new { message = "Email, prenom et nom sont obligatoires." });
            }

            var result = await _authService.createFirstAdminAsync(request);

            if (result == null)
                return Conflict(new { message = "Un adminastreur existe deja. Cet endpoint est desactive." });

            return Ok(result);
        }

        // ======================
        // POST : api/auth/login
        // Accessible à tout le monde (Admin + Guide)
        // ======================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.email) || string.IsNullOrWhiteSpace(request.password))
                return BadRequest(new { message = "Email et mot de passe sont obligatoires." });

            var result = await _authService.loginAsync(request);

            if (result == null)
                return Unauthorized(new { message = "Email ou mot de passe incorrect." });

            return Ok(result);
        }

        // ======================
        // POST : api/auth/create-guide
        // Réservé UNIQUEMENT à l'Admin
        // ======================
        [Authorize(Roles = "admin")]
        [HttpPost("create-guide")]
        public async Task<IActionResult> CreateGuide([FromBody] CreateGuideRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.email) ||
                string.IsNullOrWhiteSpace(request.firstName) ||
                string.IsNullOrWhiteSpace(request.lastName))
            {
                return BadRequest(new { message = "Email, prénom et nom sont obligatoires." });
            }

            // Récupérer l'ID de l'admin connecté depuis le token JWT
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (adminIdClaim == null || !Guid.TryParse(adminIdClaim, out Guid adminId))
                return Unauthorized(new { message = "Token invalide." });

            var result = await _authService.createGuideAsync(request, adminId);

            if (result == null)
                return Conflict(new { message = "Un utilisateur avec cet email existe déjà." });

            // On retourne le mot de passe généré pour que l'admin puisse le communiquer au guide
            return Ok(result);
        }
    }
}