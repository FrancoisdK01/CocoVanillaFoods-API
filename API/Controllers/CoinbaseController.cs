using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using API.Model;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoinbaseController : ControllerBase
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;

        public CoinbaseController(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;
        }

        [HttpPost("createcharge")]
        public async Task<IActionResult> CreateCharge([FromBody] ChargeModel charge)
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("X-CC-Api-Key", _configuration["Coinbase:ApiKey"]);
            client.DefaultRequestHeaders.Add("X-CC-Version", "2018-03-22");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.commerce.coinbase.com/charges/");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(JsonConvert.SerializeObject(charge), Encoding.UTF8, "application/json");


            var requestContent = JsonConvert.SerializeObject(charge);
            request.Content = new StringContent(requestContent, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);

            Console.WriteLine(JsonConvert.SerializeObject(charge));

            // Check if the request is successful
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Full Response: " + responseContent);
                var result = JsonConvert.DeserializeObject<ChargeResponseModel>(responseContent);
                return Ok(new { hostedUrl = result.Data.HostedUrl });
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Error content: " + errorContent);  // <-- Add this line for debugging
                return BadRequest(errorContent);
            }
        }

        [HttpGet("getcharge/{chargeId}")]
        public async Task<IActionResult> GetCharge(string chargeId)
        {
            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("X-CC-Api-Key", _configuration["Coinbase:ApiKey"]);
            client.DefaultRequestHeaders.Add("X-CC-Version", "2018-03-22");

            var response = await client.GetAsync($"https://api.commerce.coinbase.com/charges/{chargeId}");

            if (response.IsSuccessStatusCode)
            {
                // Read the response content as string
                var responseContent = await response.Content.ReadAsStringAsync();

                // Log the raw response content
                Console.WriteLine(responseContent);

                // Deserialize the response into a dynamic object
                dynamic data = JsonConvert.DeserializeObject(responseContent);

                // Print out some properties
                Console.WriteLine("ID: " + data.id);
                Console.WriteLine("Resource: " + data.resource);
                Console.WriteLine("Code: " + data.code);

                var rawResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Raw Response: " + rawResponse);
                var result = JsonConvert.DeserializeObject<ChargeResponseModel>(rawResponse);

                return Ok(result);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return BadRequest(errorContent);
            }
        }

        [HttpPost("webhook")]
        public IActionResult HandleWebhook([FromBody] JObject payload, [FromHeader(Name = "X-CC-Webhook-Signature")] string signature)
        {
            var secret = _configuration["Coinbase:WebhookSecret"];

            // Compute a HMAC-SHA256 hash from the payload using the secret
            var computedSignature = ComputeHmacSha256Hash(payload.ToString(), secret);

            // Compare the received signature with the computed signature
            if (string.Equals(signature, computedSignature, StringComparison.OrdinalIgnoreCase))
            {
                // The signature is correct. The payload has not been tampered with.
                // TODO: process the event
                return Ok();
            }
            else
            {
                // The signature is incorrect. The payload could have been tampered with.
                return Unauthorized();
            }
        }

        private static string ComputeHmacSha256Hash(string input, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }

    }
}
