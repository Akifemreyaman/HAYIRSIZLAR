using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace HayirsizlarApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string bodyHtml)
        {
            var server = _configuration.GetValue<string>("SmtpSettings:Server");
            var port = _configuration.GetValue<int>("SmtpSettings:Port", 587);
            var username = _configuration.GetValue<string>("SmtpSettings:Username");
            var password = _configuration.GetValue<string>("SmtpSettings:Password");
            var senderEmail = _configuration.GetValue<string>("SmtpSettings:SenderEmail") ?? "noreply@hayirsizlar.com";
            var senderName = _configuration.GetValue<string>("SmtpSettings:SenderName") ?? "Hayırsızlar Log";
            var enableSsl = _configuration.GetValue<bool>("SmtpSettings:EnableSsl", true);

            bool smtpConfigured = !string.IsNullOrEmpty(server) && !string.IsNullOrEmpty(username);

            if (smtpConfigured)
            {
                try
                {
                    using (var client = new SmtpClient(server, port))
                    {
                        client.UseDefaultCredentials = false;
                        client.Credentials = new NetworkCredential(username, password);
                        client.EnableSsl = enableSsl;
                        
                        if (enableSsl)
                        {
                            // By-pass SSL certificate validation for mail servers using self-signed or domain mismatched SSLs (common in Plesk)
                            ServicePointManager.ServerCertificateValidationCallback = 
                                (sender, certificate, chain, sslPolicyErrors) => true;
                        }

                        var mailMessage = new MailMessage
                        {
                            From = new MailAddress(senderEmail, senderName),
                            Subject = subject,
                            Body = bodyHtml,
                            IsBodyHtml = true
                        };
                        mailMessage.To.Add(toEmail);

                        await client.SendMailAsync(mailMessage);
                        _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send SMTP email to {ToEmail}. Falling back to file logging.", toEmail);
                }
            }

            // Fallback: Write email to a local file
            try
            {
                var logDir = Path.Combine(Directory.GetCurrentDirectory(), "artifacts");
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }
                var logPath = Path.Combine(logDir, "email_logs.txt");

                var logContent = $"========================================\n" +
                                 $"TIME: {DateTime.Now}\n" +
                                 $"TO: {toEmail}\n" +
                                 $"SUBJECT: {subject}\n" +
                                 $"BODY:\n{bodyHtml}\n" +
                                 $"========================================\n\n";

                await File.AppendAllTextAsync(logPath, logContent);
                _logger.LogInformation("Email logged to file {LogPath} successfully.", logPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write email to log file.");
            }
        }
    }
}
