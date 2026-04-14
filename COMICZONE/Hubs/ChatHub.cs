using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace COMICZONE.Hubs
{
    public class ChatHub : Hub
    {
        // Lưu trữ ConnectionId của từng UserId để có thể gửi tin nhắn đích danh
        // Trong môi trường thực tế nên dùng Redis hoặc một giải pháp scale được
        private static readonly ConcurrentDictionary<string, string> UserConnections = new ConcurrentDictionary<string, string>();

        public override Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext()?.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userId))
            {
                UserConnections[userId] = Context.ConnectionId;
            }
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.GetHttpContext()?.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userId))
            {
                string? connId;
                UserConnections.TryRemove(userId, out connId);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessageToUser(int receiverId, string message, int? postId = null, string? postTitle = null, decimal? postPrice = null, string? postImage = null, int? messageId = null)
        {
            var senderIdStr = Context.GetHttpContext()?.Session.GetString("UserId");
            var senderName = Context.GetHttpContext()?.Session.GetString("Username");
            var senderAvatar = Context.GetHttpContext()?.Session.GetString("Avatar");

            if (string.IsNullOrEmpty(senderIdStr)) return;

            int senderId = int.Parse(senderIdStr);
            var timestamp = DateTime.Now;

            // Gửi tin nhắn đến người nhận nếu họ online
            if (UserConnections.TryGetValue(receiverId.ToString(), out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", new
                {
                    senderId = senderId,
                    senderName = senderName,
                    senderAvatar = senderAvatar,
                    text = message,
                    createdAt = timestamp,
                    postId = postId,
                    postTitle = postTitle,
                    postPrice = postPrice,
                    postImage = postImage,
                    id = messageId
                });
            }

            // Đồng thời gửi ngược lại cho chính người gửi
            await Clients.Caller.SendAsync("MessageSent", new
            {
                receiverId = receiverId,
                text = message,
                createdAt = timestamp,
                postId = postId,
                postTitle = postTitle,
                postPrice = postPrice,
                postImage = postImage,
                id = messageId
            });
        }

        public async Task RecallMessage(int messageId, int otherUserId)
        {
            var senderIdStr = Context.GetHttpContext()?.Session.GetString("UserId");
            if (string.IsNullOrEmpty(senderIdStr)) return;

            // Thông báo cho người nhận
            if (UserConnections.TryGetValue(otherUserId.ToString(), out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("MessageRecalled", messageId);
            }

            // Đồng thời báo cho chính mình (các tab khác)
            await Clients.Caller.SendAsync("MessageRecalled", messageId);
        }
    }
}
