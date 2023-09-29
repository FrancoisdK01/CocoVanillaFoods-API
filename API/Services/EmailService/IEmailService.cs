using API.ViewModels;

namespace API.Services.EmailService
{
    public interface IEmailService
    {
        void SendEmail(EmailViewModel evm);
        Task<bool> SendSimpleMessage(EmailViewModel evm);
        Task<bool> SendEmailWithAttachment(EmailViewModel evm, byte[] attachmentBytes, string attachmentFilename);
    }
}
