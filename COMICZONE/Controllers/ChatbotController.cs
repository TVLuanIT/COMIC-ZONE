using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("Chatbot")]
public class ChatbotController : Controller
{
    private readonly GeminiChatService _chatService;

    public ChatbotController(GeminiChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost("SendMessage")]
    public async Task<IActionResult> SendMessage([FromBody] string message)
    {
        var response = await _chatService.SendMessageAsync(message);

        return Json(new { reply = response });
    }
}