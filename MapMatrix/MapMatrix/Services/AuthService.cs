using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MapMatrix.Data;
using MapMatrix.DT0s;
using MapMatrix.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.IdentityModel.Tokens;

namespace MapMatrix.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;

        public AuthService(AppDbContext context, IConfiguration configuration, EmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }


        // === CREATION DU PREMIER ADMIN ===
        public async Task<CreateGuideReponse?> createFirstAdminAsync(CreateGuideRequest request)
        {
            // Sécurité : on ne permet cette action que s'il n'existe aucun admin
            bool adminExists = await _context.users.AnyAsync(u => u.role == UserRole.admin);
            if (adminExists)
                return null;

            string plainPassword = generateRandomPassword(10);

            var admin = new User
            {
                email = request.email,
                firstName = request.firstName,
                lastName = request.lastName,
                phone = request.phone,
                passwordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword),
                role = UserRole.admin,
                isActive = true,
                createdBy = null // Aucun créateur pour le premier admin
            };

            _context.users.Add(admin);
            await _context.SaveChangesAsync();

            return new CreateGuideReponse
            {
                identifiantGuide = admin.id,
                email = admin.email,
                nom = admin.firstName,
                prenom = admin.lastName,
                generatePassword = plainPassword
            };
        }

        // === LOGIN ===
        public async Task<LoginResponse?> loginAsync(LoginRequest request)
        {
            var user = await _context.users.FirstOrDefaultAsync(u => u.email == request.email && u.isActive);
            if (user == null)
                return null; // Invalid credentials

            // Verify password
            if(!BCrypt.Net.BCrypt.Verify(request.password, user.passwordHash))
                return null; // Invalid credentials

            // Mise à jour du Last Login
            user.lastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Generate JWT token
            var token = generateJwtToken(user);

            return new LoginResponse
            {
                token = token,
                userId = user.id,
                email = user.email,
                nom = user.firstName,
                prenom = user.lastName,
                role = user.role.ToString()
            };
        }

        // === CREATION D'UN GUIDE (par un admin) ===
        public async Task<CreateGuideReponse?> createGuideAsync(CreateGuideRequest request, Guid IdAdmin)
        {
            //verification que l'email n'existe pas déjà
            if(await _context.users.AnyAsync(u => u.email == request.email))
                return null; 

            //Generation automatique du mot de passe
            string plainPassword = generateRandomPassword(10);

            var guide = new User
            {
                email = request.email,
                firstName = request.firstName,
                lastName = request.lastName,
                phone = request.phone,
                passwordHash = BCrypt.Net.BCrypt.HashPassword(plainPassword),
                role = UserRole.guide,
                isActive = true,
                createdBy = IdAdmin
            };

            _context.users.Add(guide);
            await _context.SaveChangesAsync();
            await _emailService.sendGuideCredentialsAsync(guide.email, guide.firstName, plainPassword);

            return new CreateGuideReponse
            {
                identifiantGuide = guide.id,
                email = guide.email,
                nom = guide.firstName,
                prenom = guide.lastName,
                generatePassword = plainPassword
            };
        }

        // === === Helper Methods ===
        private string generateJwtToken(User user)
        {
            // Vérifier que la clé JWT est présente (évite l'appel Encoding.GetBytes(null))
            var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Configuration 'Jwt:Key' manquante.");
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                new Claim(ClaimTypes.Email, user.email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.role.ToString()),
            };

            if (!string.IsNullOrEmpty(user.firstName))
                claims.Add(new Claim("nom", user.firstName));
            if (!string.IsNullOrEmpty(user.lastName))
                claims.Add(new Claim("prenom", user.lastName));

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string generateRandomPassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            using var rng = RandomNumberGenerator.Create();
            byte[] uintBuffer = new byte[sizeof(uint)];

            while (length-- > 0)
            {
                rng.GetBytes(uintBuffer);
                uint num = BitConverter.ToUInt32(uintBuffer, 0);
                res.Append(valid[(int)(num % (uint)valid.Length)]);
            }

            return res.ToString();
        }
    }
}
