using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;
using MailKit.Security;
using API.ViewModels;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {

        private readonly IEmailService _emailService;
        public MailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost]
        [DynamicAuthorize]
        public IActionResult SystemSendEmail(EmailViewModel emailViewModel)
        {
            _emailService.SendEmail(emailViewModel);

            return Ok();
        }
    }
}
