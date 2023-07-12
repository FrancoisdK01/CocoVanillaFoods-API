using API.ViewModels;

namespace API.Services.EmailService
{
    public interface IEmailService
    {
        void SendEmail(EmailViewModel evm) { }
    }
}
