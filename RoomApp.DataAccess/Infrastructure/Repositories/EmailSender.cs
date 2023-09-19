using RoomApp.DataAccess.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace RoomApp.DataAccess.Infrastructure.Repositories
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var smtpServer = "smtp.gmail.com";
            var smtpPort = 587;
            var mail = "adhikarikshitiz12@gmail.com";
            var pw = _configuration["AppPassword"];

            using (var client = new SmtpClient(smtpServer, smtpPort))
            {
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(mail, pw);

                var mailMessage = new MailMessage(
                    from: mail,
                    to: email,
                    subject: subject,
                    body: message
                );

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}
