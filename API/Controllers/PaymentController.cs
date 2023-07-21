using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using API.Model;
using System.Text;
using System.Security.Cryptography;
using API.Data;
using System.Net;


namespace API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private readonly MyDbContext _context;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IHttpClientFactory clientFactory, IConfiguration configuration, MyDbContext context, ILogger<PaymentController> logger)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        [HttpPost("CreatePayment", Name = "CreatePayment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreatePayment([FromBody] PayFastRequest payment)
        {
            var client = _clientFactory.CreateClient();

            // Change this line to use the live PayFast URL
            var url = "https://www.payfast.co.za/eng/process";

            if (int.TryParse(_configuration["PayFast:MerchantId"], out int merchantId))
            {
                payment.merchant_id = merchantId;
            }
            else
            {
                // Log an error
                _logger.LogError($"MerchantId value '{_configuration["PayFast:MerchantId"]}' is not a valid integer.");
            }

            payment.merchant_key = _configuration["PayFast:MerchantKey"];

            var passphrase = _configuration["PayFast:MerchantPassphrase"];

            var propertyValues = payment.GetType().GetProperties()
                .Where(p => p.GetValue(payment) != null && p.Name != "signature")
                .OrderBy(p => p.Name)
                .Select(p => $"{p.Name}={UrlEncode(p.GetValue(payment).ToString())}");

            var rawData = string.Join("&", propertyValues) + $"&passphrase={passphrase}";

            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                payment.signature = BitConverter.ToString(hash).Replace("-", "").ToLower();
            }

            var keyValues = payment.GetType().GetProperties()
                .Select(p => new KeyValuePair<string, string>(p.Name, p.GetValue(payment)?.ToString() ?? ""));
            var formContent = new FormUrlEncodedContent(keyValues);

            var response = await client.PostAsync(url, formContent).ConfigureAwait(false);

            var responseContent = await response.Content.ReadAsStringAsync();

            PayFastResponse payFastResponse = null;
            try
            {
                payFastResponse = JsonConvert.DeserializeObject<PayFastResponse>(responseContent);
            }
            catch (JsonReaderException)
            {
                // The response content isn't valid JSON
                // Log the error and/or handle it appropriately
            }

            if (response.IsSuccessStatusCode)
            {
                // Instead of trying to read and process the response content, just return the URL.
                //return Ok(url);
                return Ok(payment);
            }
            else
            {
                // Handle the error...
                return BadRequest(responseContent);
            }
          
        }

        private string UrlEncode(string value)
        {
            return WebUtility.UrlEncode(value)?.Replace("%20", "+");
        }
    }

}


        
