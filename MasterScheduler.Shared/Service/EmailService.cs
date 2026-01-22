using MasterScheduler.Shared.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.Service
{
    public class EmailService : IEmailService
    {        
        public async Task SendEmailAsync(string to, string subject, string body, CancellationToken token)
        {
            // Use the 16-character App Password here, NOT your gmail password
            var appPassword = "afgl bktr asqg buxj";

            using var client = new System.Net.Mail.SmtpClient("smtp.gmail.com")
            {
                Port = 587, // No need to parse a string here
                Credentials = new System.Net.NetworkCredential("roshanraj6824@gmail.com", appPassword),
                EnableSsl = true,
            };

            var mailMessage = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress("roshanraj6824@gmail.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage, token);
        }
    }
}
