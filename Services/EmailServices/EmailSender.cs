using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using OnlineCoursePlatform.Constants;

namespace OnlineCoursePlatform.Services.EmailServices
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Create a new SmtpClient
            SmtpClient client = new SmtpClient
            {
                // Set the port to 587
                Port = 587,
                // Set the host to smtp.gmail.com
                Host = "smtp.gmail.com", // or another sending email provider
                                         // Enable SSL
                EnableSsl = true,
                // Set the delivery method to Network
                DeliveryMethod = SmtpDeliveryMethod.Network,
                // Do not use default credentials
                UseDefaultCredentials = false,
                // Set the credentials to the email sender's email and password
                Credentials = new NetworkCredential(EmailConstant.EmailSender, EmailConstant.Password)
            };

            // Create a new MailMessage
            MailMessage mailMessage = new MailMessage
            {
                // Set the sender's email
                From = new MailAddress(EmailConstant.EmailSender),
                // Set the email subject
                Subject = subject,
                // Set the email body
                Body = htmlMessage,
                // Enable HTML in the email body
                IsBodyHtml = true
            };
            // Add the recipient's email
            mailMessage.To.Add(email);

            // Send the email asynchronously
            return client.SendMailAsync(mailMessage);
        }
    }
}