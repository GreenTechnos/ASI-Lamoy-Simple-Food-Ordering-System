using backend.Constants;
using System.Net;
using System.Net.Mail;

namespace backend.Services
{
    public class EmailService
    {
        private readonly string _smtpServer;
        private readonly int _port;
        private readonly string _senderName;
        private readonly string _senderEmail;
        private readonly string _username;
        private readonly string _password;

        public EmailService()
        {
            _smtpServer = Environment.GetEnvironmentVariable(AppConstants.EmailServiceEnvironmentVariables.SmtpServer)!;
            _port = int.Parse(Environment.GetEnvironmentVariable(AppConstants.EmailServiceEnvironmentVariables.SmtpPort)!);
            _senderName = Environment.GetEnvironmentVariable(AppConstants.EmailServiceEnvironmentVariables.SmtpSenderName)!;
            _senderEmail = Environment.GetEnvironmentVariable(AppConstants.EmailServiceEnvironmentVariables.SmtpEmail)!;
            _username = Environment.GetEnvironmentVariable(AppConstants.EmailServiceEnvironmentVariables.SmtpEmail)!;
            _password = Environment.GetEnvironmentVariable(AppConstants.EmailServiceEnvironmentVariables.SmtpPassword)!;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            using var smtp = new SmtpClient(_smtpServer, _port)
            {
                Credentials = new NetworkCredential(_username, _password),
                EnableSsl = AppConstants.EmailServiceSettings.EnableSsl
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_senderEmail, _senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = AppConstants.EmailServiceSettings.IsBodyHtml
            };
            mail.To.Add(toEmail);

            await smtp.SendMailAsync(mail);
        }
    }
}
