using COMICZONE.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("Chatbot")]
public class ChatbotController : Controller
{
    private readonly IChatbotService _chatService;

    public ChatbotController(IChatbotService chatService)
    {
        _chatService = chatService;
    }

    public class ChatMessageDto
    {
        public string Message { get; set; }
    }

    [HttpPost("SendMessage")]
    public async Task<IActionResult> SendMessage([FromBody] ChatMessageDto dto)
    {
        try
        {
            var reply = await _chatService.GetReplyAsync(dto.Message);
            return Json(new { reply });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return Json(new { reply = "The chatbot is currently busy, please try again later." });
        }
    }
}