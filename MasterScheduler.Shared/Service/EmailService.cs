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
        public async Task SendEmailAsync(string to,string subject, string body, CancellationToken token)
        {
            using var client = new System.Net.Mail.SmtpClient("smtp.gmail.com")
            {
                Port = int.Parse("587"),
                Credentials = new System.Net.NetworkCredential("lakshya2025gautam@gmail.com", "Liger@020223"),
                EnableSsl = true,
            };

            var mailMessage = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress("lakshya2025gautam@gmail.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage, token);
        }
    }
}
