using Habit_Tracker_Backend.Configurations;
using Habit_Tracker_Backend.Services.Interfaces;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Habit_Tracker_Backend.Exceptions;
namespace Habit_Tracker_Backend.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailOptions _options;

        public EmailService(IOptions<EmailOptions> options)
        {
            _options = options.Value;
        }

        public async Task SendOtpAsync(string toEmail, string otp)
        {
            //var message = new MimeMessage();
            //message.From.Add(new MailboxAddress(
            //    _options.FromName, _options.FromEmail));

            //message.To.Add(MailboxAddress.Parse(toEmail));
            //message.Subject = "Your OTP Code";

            //message.Body = new TextPart("html")
            //{
            //    Text = $@"
            //    <h2>Habit Tracker</h2>
            //    <p>Your OTP is:</p>
            //    <h1>{otp}</h1>
            //    <p>Valid for <b>5 minutes</b></p>"
            //};

            //using var client = new SmtpClient();
            //await client.ConnectAsync(
            //    _options.SmtpServer,
            //    _options.Port,
            //    MailKit.Security.SecureSocketOptions.StartTls);

            //await client.AuthenticateAsync(
            //    _options.Username,
            //    _options.Password);

            //await client.SendAsync(message);
            //await client.DisconnectAsync(true);
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(
                    _options.FromName, _options.FromEmail));

                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = "Your OTP Code";

                message.Body = new TextPart("html")
                {
                    Text = $@"
                        <h2>Habit Tracker</h2>
                        <p>Your OTP is:</p>
                        <h1>{otp}</h1>
                        <p>Valid for <b>5 minutes</b></p>"
                };

                using var client = new SmtpClient();
                client.Timeout = 10_000;

                await client.ConnectAsync(
                    _options.SmtpServer,
                    _options.Port,
                    MailKit.Security.SecureSocketOptions.StartTls);

                await client.AuthenticateAsync(
                    _options.Username,
                    _options.Password);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception)
            {
                throw new EmailSendFailedException();
            }
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _options.FromName, _options.FromEmail));

            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = body
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(
                _options.SmtpServer,
                _options.Port,
                MailKit.Security.SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(
                _options.Username,
                _options.Password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}


