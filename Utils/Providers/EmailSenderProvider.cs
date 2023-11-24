using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;

namespace IsometricShooterWebApp.Utils.Providers
{
    public class EmailSenderProvider : IEmailSender
    {
        private readonly IConfiguration configuration;

        public bool Enabled => configuration.GetValue<bool>("smtp:enabled");
        private string mailAddress => configuration.GetValue<string>("smtp:mailAddress");
        private string userName => configuration.GetValue<string>("smtp:userName");
        private string password => configuration.GetValue<string>("smtp:password");
        private string serverAddress => configuration.GetValue<string>("smtp:serverAddress");
        private int serverPort => configuration.GetValue<int>("smtp:serverPort");
        private bool enableSSL => configuration.GetValue<bool>("smtp:enableSSL");

        public EmailSenderProvider(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (!Enabled)
                return;

            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(mailAddress));
            message.To.Add(MailboxAddress.Parse(email));

            message.Subject = subject;
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };


            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(serverAddress, serverPort, enableSSL ? SecureSocketOptions.StartTls : SecureSocketOptions.None);
            await smtp.AuthenticateAsync(userName, password);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }
    }
}
