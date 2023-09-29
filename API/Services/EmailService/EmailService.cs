using API.ViewModels;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using System.Collections.Generic;

namespace API.Services.EmailService
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private const string BaseUrl = "https://api.mailgun.net/v3/sandboxb598764c79db4f869b21818d91f133e6.mailgun.org";

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public void SendEmail(EmailViewModel evm)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config.GetSection("EmailUserName").Value));
            email.To.Add(MailboxAddress.Parse(evm.To));
            email.Subject = evm.Subject;
            email.Body = new TextPart(TextFormat.Html) { Text = evm.Body };

            using var smtp = new SmtpClient();
            smtp.Connect(_config.GetSection("EmailHost").Value, 587, SecureSocketOptions.StartTls);
            smtp.Authenticate(_config.GetSection("EmailUserName").Value, _config.GetSection("EmailPassword").Value);
            smtp.Send(email);
            smtp.Disconnect(true);
        }

        public async Task<bool> SendSimpleMessage(EmailViewModel evm)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{_config.GetSection("EmailAPIKey").Value}")));

                var formContent = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("from", "b4i.promenade@gmail.com"),
                    new KeyValuePair<string, string>("to", evm.To),
                    new KeyValuePair<string, string>("subject", evm.Subject),
                    new KeyValuePair<string, string>("text", evm.Body)
                });

                var response = await client.PostAsync($"{BaseUrl}/messages", formContent);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                // Optionally, log the error message here
                var errorMessage = $"Failed to send email. Error: {response.Content.ReadAsStringAsync().Result}";

                return false;
            }
        }

        public async Task<bool> SendEmailWithAttachment(EmailViewModel evm, byte[] attachmentBytes, string attachmentFilename)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{_config.GetSection("EmailAPIKey").Value}")));

                // Create Multipart Form Data Content
                var formContent = new MultipartFormDataContent
                    {
                        { new StringContent("b4i.promenade@gmail.com"), "from" },
                        { new StringContent(evm.To), "to" },
                        { new StringContent(evm.Subject), "subject" },
                        { new StringContent(evm.Body), "text" },
                    };

                // Add attachment if provided
                if (attachmentBytes != null && attachmentBytes.Length > 0)
                {
                    var attachmentContent = new ByteArrayContent(attachmentBytes);
                    attachmentContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    formContent.Add(attachmentContent, "attachment", attachmentFilename);
                }

                var response = await client.PostAsync($"{BaseUrl}/messages", formContent);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                // Optionally, log the error message here
                var errorMessage = $"Failed to send email. Error: {response.Content.ReadAsStringAsync().Result}";

                return false;
            }
        }

    }
}
