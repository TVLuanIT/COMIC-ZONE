using COMICZONE.Controllers;
using COMICZONE.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("Chatbot")]
public class ChatbotController : BaseController
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
            return Json(new { reply = "LỖI HỆ THỐNG C#: " + ex.Message + "\n" + ex.StackTrace });
        }
    }
}