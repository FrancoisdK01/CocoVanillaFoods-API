using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NuGet.Common;
using API.Model;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatBotController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public ChatBotController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpPost]
        [DynamicAuthorize]
        public async Task<IActionResult> SendMessageToBot([FromBody] UserMessage message)
        {
            var response = await _httpClient.PostAsJsonAsync("http://localhost:5005/webhooks/rest/webhook", message);

            if (response.IsSuccessStatusCode)
            {
                var botMessage = await response.Content.ReadFromJsonAsync<BotMessage>();
                return Ok(botMessage);
            }

            return BadRequest();
        }
    }
}
