using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using COMICZONE.Services;
using COMICZONE.Models;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace COMICZONE.Areas.Chat.Controllers
{
    [Area("Chat")]
    public class ConversationsController : COMICZONE.Controllers.BaseController
    {
        private readonly IChatService _chatService;
        private readonly IMarketplaceService _marketplaceService;

        public ConversationsController(IChatService chatService, IMarketplaceService marketplaceService)
        {
            _chatService = chatService;
            _marketplaceService = marketplaceService;
        }

        public async Task<IActionResult> Index(int? otherUserId = null, int? postId = null)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Authentication", new { area = "Account", returnUrl = Request.Path + Request.QueryString });

            int currentUserId = int.Parse(CurrentUserId());
            var conversations = await _chatService.GetConversationsAsync(currentUserId);

            ViewBag.CurrentUserId = currentUserId;
            ViewBag.OtherUserId = otherUserId;
            ViewBag.PostId = postId;

            if (otherUserId.HasValue)
            {
                await _chatService.MarkAsReadAsync(currentUserId, otherUserId.Value);
            }

            if (postId.HasValue)
            {
                var post = await _marketplaceService.GetPostByIdAsync(postId.Value);
                ViewBag.ActivePost = post;
            }

            return View(conversations);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(int otherUserId)
        {
            if (!IsLoggedIn()) return Json(new { success = false });

            int currentUserId = int.Parse(CurrentUserId());
            var messages = await _chatService.GetMessagesAsync(currentUserId, otherUserId);
            
            await _chatService.MarkAsReadAsync(currentUserId, otherUserId);

            return Json(new { 
                success = true, 
                messages = messages.Select(m => new {
                    id = m.Id,
                    senderId = m.Senderid,
                    receiverId = m.Receiverid,
                    text = m.Message,
                    createdAt = m.Createdat,
                    postId = m.Postid,
                    postTitle = m.Post?.Title,
                    postPrice = m.Post?.Price,
                    postImage = m.Post?.MarketplacePostImages?.FirstOrDefault()?.Filename
                })
            });
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequest request)
        {
            if (!IsLoggedIn()) return Json(new { success = false });

            int currentUserId = int.Parse(CurrentUserId());
            var message = await _chatService.SaveMessageAsync(currentUserId, request.ReceiverId, request.Message, request.PostId);

            return Json(new { success = true, id = message.Id });
        }
        
        [HttpPost]
        public async Task<IActionResult> RecallMessage(int messageId)
        {
            if (!IsLoggedIn()) return Json(new { success = false });

            int currentUserId = int.Parse(CurrentUserId());
            bool success = await _chatService.RecallMessageAsync(messageId, currentUserId);

            return Json(new { success = success });
        }
    }

    public class ChatMessageRequest
    {
        public int ReceiverId { get; set; }
        public string Message { get; set; } = "";
        public int? PostId { get; set; }
    }
}
