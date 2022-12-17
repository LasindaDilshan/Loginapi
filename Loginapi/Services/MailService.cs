using Loginapi.Models;
using System.Net.Mail;

namespace Loginapi.Services
{
    public class MailService
    {

        private static readonly string smtpClient = "localhost";
        private static readonly int smtpPort = 1025;
        private static readonly string smtpEmail = "test@gmail.com";
        private static readonly string smtoName = "Testing";


        public static async void SendPasswordResetMailAsync(ResetToken token)
        {
            SmtpClient client = new SmtpClient(smtpClient)
            {
                Port = smtpPort,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = true,
                EnableSsl = false
            };

            MailMessage email = new()
            {
                From = new MailAddress(smtpEmail, smtoName),
                Subject = "Reset Your Password",
                Body = $"Click<a href = \"http://localhost:4200/reset/{token.Token}\">  here </a> to reset your passsword!",
                IsBodyHtml = true
                
            };
            email.To.Add(new MailAddress(token.Email));

            await client.SendMailAsync(email);
            email.Dispose();
        }

    }
}
