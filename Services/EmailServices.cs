using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace EduConnect.Services
{
    public class EmailSettings
    {
        public string Host { get; set; } = "smtp-relay.brevo.com";
        public int Port { get; set; } = 587;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty;
        public string FromName { get; set; } = "EduConnect";

        // Testing aid: when set, every email goes to this address instead
        public string? RedirectTo { get; set; }

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(Username) &&
            !string.IsNullOrWhiteSpace(Password) &&
            !string.IsNullOrWhiteSpace(FromAddress);
    }

    public interface IEmailService
    {
        Task SendAsync(string toAddress, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings settings;
        private readonly ILogger<EmailService> logger;

        public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> log)
        {
            settings = options.Value;
            logger = log;
        }

        public async Task SendAsync(string toAddress, string subject, string body)
        {
            // Without credentials the site still works: the message is written to the terminal
            if (!settings.IsConfigured)
            {
                logger.LogWarning(
                    "Email is not configured. Would have sent to {To} with subject '{Subject}':\n{Body}",
                    toAddress, subject, body);
                return;
            }

            string recipient = toAddress;
            if (!string.IsNullOrWhiteSpace(settings.RedirectTo))
            {
                recipient = settings.RedirectTo;
                subject = "[for " + toAddress + "] " + subject;
            }

            try
            {
                using MailMessage message = new MailMessage
                {
                    From = new MailAddress(settings.FromAddress, settings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };
                message.To.Add(recipient);

                using SmtpClient client = new SmtpClient(settings.Host, settings.Port)
                {
                    EnableSsl = true, // STARTTLS on port 587
                    Credentials = new NetworkCredential(settings.Username, settings.Password),
                    Timeout = 15000
                };

                await client.SendMailAsync(message);
                logger.LogInformation("Email sent to {To}", recipient);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send email to {To}", recipient);
            }
        }
    }
}