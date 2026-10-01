using ECommerceAPI.Options;
using ECommerceAPI.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ECommerceAPI.Services.Implementations
{
    public sealed class EmailService : IEmailService
    {
        // Stores the Email configuration values such as SMTP host, port,
        // sender Email, username, and Gmail App Password.
        private readonly EmailOptions _emailOptions;

        // IOptions<EmailOptions> provides the strongly typed Email configuration
        // registered through ASP.NET Core Dependency Injection.
        public EmailService(IOptions<EmailOptions> emailOptions)
        {
            // Extract the actual EmailOptions object from IOptions<T>.
            _emailOptions = emailOptions.Value;
        }

        // Sends an Email asynchronously.
        // toEmail : Recipient's Email address.
        // subject : Subject of the Email.
        // body    : Content of the Email.
        // isHtml  : Indicates whether the body should be treated as HTML.
        //           By default, Emails are sent as HTML.
        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string body,
            bool isHtml = true)
        {
            // MimeMessage represents the complete Email message,
            // including sender, recipient, subject, and body.
            var message = new MimeMessage();

            // Set the sender information.
            // SenderName is the friendly name displayed to the recipient,
            // while FromEmail is the actual sender Email address.
            message.From.Add(new MailboxAddress(_emailOptions.SenderName, _emailOptions.FromEmail));

            // Add the recipient Email address.
            // MailboxAddress.Parse validates and converts the string
            // into a MailboxAddress understood by MimeKit.
            message.To.Add(MailboxAddress.Parse(toEmail));

            // Set the Email subject.
            message.Subject = subject;

            // BodyBuilder helps us create either an HTML or plain-text Email body.
            var bodyBuilder = new BodyBuilder();

            if (isHtml)
            {
                // Treat the provided content as HTML.
                // This allows formatting such as headings, links,
                // colors, tables, and other HTML elements.
                bodyBuilder.HtmlBody = body;
            }
            else
            {
                // Treat the provided content as plain text.
                bodyBuilder.TextBody = body;
            }

            // Convert the BodyBuilder content into a MIME body
            // and assign it to the Email message.
            message.Body = bodyBuilder.ToMessageBody();

            // Create the MailKit SMTP client that will communicate
            // with the configured SMTP server.
            using var smtpClient = new SmtpClient();

            // Connect to the SMTP server.
            // For Gmail:
            // Host = smtp.gmail.com
            // Port = 587
            // STARTTLS upgrades the connection to a secure TLS connection.
            await smtpClient.ConnectAsync(
                _emailOptions.Host,
                _emailOptions.Port,
                SecureSocketOptions.StartTls);

            // Authenticate with the SMTP server using the configured
            // Email account and Gmail App Password.
            await smtpClient.AuthenticateAsync(_emailOptions.Username, _emailOptions.AppPassword);

            // Send the Email through the authenticated SMTP connection.
            await smtpClient.SendAsync(message);

            // Close the SMTP connection properly.
            // true means MailKit should send the QUIT command
            // before disconnecting from the server.
            await smtpClient.DisconnectAsync(true);
        }
    }
}
