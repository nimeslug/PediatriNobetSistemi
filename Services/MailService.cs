using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace PediatriNobetSistemi.Services
{
    public class MailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public interface IMailService
    {
        Task<bool> SendAsync(string toEmail, string subject, string htmlBody);
        Task SendBulkAsync(IEnumerable<string> toEmails, string subject, string htmlBody);
    }

    public class MailService : IMailService
    {
        private readonly MailSettings _settings;
        private readonly ILogger<MailService> _logger;

        public MailService(IConfiguration config, ILogger<MailService> logger)
        {
            _settings = config.GetSection("MailSettings").Get<MailSettings>() ?? new MailSettings();
            _logger = logger;
        }

        public async Task<bool> SendAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_settings.SmtpServer) ||
                    string.IsNullOrWhiteSpace(_settings.SenderEmail))
                {
                    _logger.LogWarning("Mail ayarlari eksik, mail gonderilmedi: {Email}", toEmail);
                    return false;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;

                var builder = new BodyBuilder { HtmlBody = htmlBody };
                message.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort,
                    _settings.SmtpPort == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls);

                if (!string.IsNullOrEmpty(_settings.Password))
                    await smtp.AuthenticateAsync(_settings.SenderEmail, _settings.Password);

                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mail gonderim hatasi: {Email}", toEmail);
                return false;
            }
        }

        public async Task SendBulkAsync(IEnumerable<string> toEmails, string subject, string htmlBody)
        {
            foreach (var email in toEmails.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(email)) continue;
                await SendAsync(email, subject, htmlBody);
            }
        }
    }
}
