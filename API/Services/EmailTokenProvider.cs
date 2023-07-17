using API.Model;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace API.Services
{
    public class EmailTokenProvider : DataProtectorTokenProvider<User>
    {
        public EmailTokenProvider(IDataProtectionProvider dataProtectionProvider, IOptions<DataProtectionTokenProviderOptions> options, ILogger<DataProtectorTokenProvider<User>> logger) : base(dataProtectionProvider, options, logger)
        {
        }
    }
}
