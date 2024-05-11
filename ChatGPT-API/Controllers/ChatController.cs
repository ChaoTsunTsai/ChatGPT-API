using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RestSharp;
using ChatGPT_API.Models;
using System.Text.Json;
using ChatGPT_API.Extensions;

namespace YourNamespace.Controllers
{
    public class ChatController : Controller
    {
        private readonly OpenAISettings _openAISettings;

        public ChatController(IOptions<OpenAISettings> options)
        {
            _openAISettings = options.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        /*[HttpPost]
        public async Task<IActionResult> SendMessage(string userMessage) //userMessage is user's input string.
        {
            var client = new RestClient("https://api.openai.com/v1/chat/completions");
            var request = new RestRequest("/", Method.Post);

            request.AddHeader("Authorization", $"Bearer {_openAISettings.ApiKey}");
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
            new { role = "system", content = "You are ChatGPT, a large language model." },
            new { role = "user", content = userMessage }
        }
            });

            var response = await client.ExecuteAsync(request);

            // 使用 System.Text.Json 來解析響應
            
            var jsonResponse = JsonDocument.Parse(response.Content);
            var messageContent = jsonResponse.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return Content(messageContent); // 返回純文本回應
        }*/
        [HttpPost]
        public async Task<IActionResult> SendMessage(string userMessage)
        {
            List<object> messages = HttpContext.Session.GetObject<List<object>>("ChatHistory") ?? new List<object>();
            messages.Add(new { role = "user", content = userMessage });

            var client = new RestClient("https://api.openai.com/v1/chat/completions");
            var request = new RestRequest("/", Method.Post);
            request.AddHeader("Authorization", $"Bearer {_openAISettings.ApiKey}");
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(new
            {
                model = "gpt-3.5-turbo",
                messages = messages.ToArray()
            });

            var response = await client.ExecuteAsync(request);
            var jsonResponse = JsonDocument.Parse(response.Content);
            var messageContent = jsonResponse.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            // 保存返回的消息以保持对话历史
            messages.Add(new { role = "system", content = messageContent });

            HttpContext.Session.SetObject("ChatHistory", messages);

            ViewBag.Response = messageContent;            
            return Content(messageContent); // 返回純文本回應
        }
    }
}
