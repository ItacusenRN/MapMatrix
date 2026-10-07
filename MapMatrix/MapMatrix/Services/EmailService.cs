using System.Net;
using System.Net.Mail;

namespace MapMatrix.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task sendGuideCredentialsAsync(string toEmail, string firstName, string password)
        {
            var host = _config["Smtp:Host"];
            var port = int.Parse(_config["Smtp:Port"] ?? "587");
            var enableSsl = bool.Parse(_config["Smtp:EnableSsl"] ?? "true");
            var userName = _config["Smtp:UserName"];
            var passwordSmtp = _config["Smtp:Password"];
            var from = _config["Smtp:From"] ?? userName;

            if(string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(userName))
            {
                _logger.LogWarning("configuration SMTP non faite - e-mail non envoye a {Email}", toEmail);
                return;
            }

            var body = $@"Bonjour {firstName},
Votre compte guide MapMatrix a ete cree.
Identifiants de connexion :
- E-mail : {toEmail}
- Mot de passe temporaire : {password}

Connectez-vous sur l'application mobile et changez ce mot de passe des que possible.

- L'equipe MapMatrix";

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(userName, passwordSmtp)
            };

            using var message = new MailMessage(from!, toEmail, "MapMatrix - vos identifiants guide", body);

            try
            {
                await client.SendMailAsync(message);
                _logger.LogInformation("E-mail identifiants envoye a {Email}", toEmail);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Echec de l'envoi a {Email}", toEmail);
                // Ne pas faire echouer la creation de compte
            }
        }
    }
}
