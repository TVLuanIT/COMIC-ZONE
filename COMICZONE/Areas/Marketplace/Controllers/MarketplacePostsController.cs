using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using COMICZONE.Services;
using COMICZONE.Models;
using System.IO;
using System;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Areas.Marketplace.Controllers
{
    [Area("Marketplace")]
    public class MarketplacePostsController : COMICZONE.Controllers.BaseController
    {
        private readonly IMarketplaceService _marketplaceService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public MarketplacePostsController(IMarketplaceService marketplaceService, IWebHostEnvironment webHostEnvironment)
        {
            _marketplaceService = marketplaceService;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index(string sortOrder = "date_desc", string? searchTerm = null, string? category = null, string? condition = null, decimal? minPrice = null, decimal? maxPrice = null, int page = 1)
        {
            const int pageSize = 12;

            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentSearch"] = searchTerm;
            ViewData["CurrentCategory"] = category;
            ViewData["CurrentCondition"] = condition;
            ViewData["MinPrice"] = minPrice;
            ViewData["MaxPrice"] = maxPrice;

            var (posts, totalCount) = await _marketplaceService.GetAllPostsAsync("Approved", sortOrder, searchTerm, category, condition, minPrice, maxPrice, page, pageSize);

            var extraParams = new Dictionary<string, string>
            {
                { "sortOrder", sortOrder },
                { "searchTerm", searchTerm ?? "" },
                { "category", category ?? "" },
                { "condition", condition ?? "" },
                { "minPrice", minPrice?.ToString() ?? "" },
                { "maxPrice", maxPrice?.ToString() ?? "" }
            };

            var pagination = new PaginationModel
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Controller = "MarketplacePosts",
                Action = "Index",
                Area = "Marketplace",
                ExtraParams = extraParams
            };

            ViewBag.Pagination = pagination;
            return View(posts);
        }

        public async Task<IActionResult> Details(int id)
        {
            var post = await _marketplaceService.GetPostByIdAsync(id);
            if (post == null) return NotFound();

            if (IsLoggedIn())
            {
                var userId = int.Parse(CurrentUserId());
                ViewBag.IsFavorited = await _marketplaceService.IsFavoritedAsync(userId, id);
                ViewBag.CurrentUserId = userId;
            }
            else
            {
                ViewBag.IsFavorited = false;
                ViewBag.CurrentUserId = null;
            }

            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(int postId)
        {
            if (!IsLoggedIn())
                return Json(new { success = false, message = "login_required" });

            var userId = int.Parse(CurrentUserId());
            var isFavorited = await _marketplaceService.ToggleFavoriteAsync(userId, postId);

            return Json(new { success = true, isFavorited });
        }

        public async Task<IActionResult> Create()
        {
            if (!IsLoggedIn())
            {
                TempData["LoginRequired"] = "Bạn cần đăng nhập để đăng bán.";
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            var userId = int.Parse(CurrentUserId());
            var customer = await _marketplaceService.GetCustomerByUserIdAsync(userId);

            if (customer == null || string.IsNullOrWhiteSpace(customer.Phone) || string.IsNullOrWhiteSpace(customer.Address))
            {
                TempData["IncompleteProfile"] = "Bạn cần cập nhật số điện thoại và địa chỉ trong hồ sơ trước khi đăng bán.";
                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MarketplacePost post, List<IFormFile> uploadedImages)
        {
            if (!IsLoggedIn()) return RedirectToAction("Index", "Home", new { area = "" });

            ModelState.Remove("Seller");
            ModelState.Remove("Status");
            ModelState.Remove("Condition");
            ModelState.Remove("Category");
            ModelState.Remove("MarketplacePostImages");
            ModelState.Remove("MarketplaceFavorites");
            ModelState.Remove("MarketplaceMessages");
            ModelState.Remove("MarketplaceOrders");

            if (uploadedImages == null || uploadedImages.Count == 0 || !uploadedImages.Any(f => f.Length > 0))
            {
                ModelState.AddModelError("uploadedImages", "Bạn phải tải lên ít nhất một tấm ảnh minh họa cho sản phẩm.");
            }

            if (ModelState.IsValid)
            {
                post.Sellerid = int.Parse(CurrentUserId());
                var createdPost = await _marketplaceService.CreatePostAsync(post);

                if (uploadedImages != null && uploadedImages.Count > 0)
                {
                    var folderPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "marketplace");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    foreach (var file in uploadedImages)
                    {
                        if (file.Length > 0)
                        {
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            var filePath = Path.Combine(folderPath, fileName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                            
                            var postImage = new MarketplacePostImage
                            {
                                Postid = createdPost.Id,
                                Filename = fileName
                            };
                            await _marketplaceService.AddPostImageAsync(postImage);
                        }
                    }
                }

                TempData["Success"] = "Bài đăng của bạn đang chờ phê duyệt.";
                return RedirectToAction("Index");
            }
            return View(post);
        }
        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] MessageRequest request)
        {
            if (!IsLoggedIn())
                return Json(new { success = false, message = "login_required" });

            if (string.IsNullOrWhiteSpace(request.Message))
                return Json(new { success = false, message = "Message is empty" });

            var senderId = int.Parse(CurrentUserId());
            if (senderId == request.ReceiverId)
                return Json(new { success = false, message = "Cannot send message to yourself" });

            var message = new MarketplaceMessage
            {
                Postid = request.PostId,
                Senderid = senderId,
                Receiverid = request.ReceiverId,
                Message = request.Message,
                Createdat = DateTime.Now,
                Isread = false
            };

            var sentMessage = await _marketplaceService.SendMessageAsync(message);

            return Json(new
            {
                success = true,
                message = new
                {
                    id = sentMessage.Id,
                    senderId = sentMessage.Senderid,
                    receiverId = sentMessage.Receiverid,
                    text = sentMessage.Message,
                    createdAt = sentMessage.Createdat?.ToString("o"),
                    senderName = sentMessage.Sender?.Username,
                    senderAvatar = sentMessage.Sender?.Avatar
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(int postId, int otherUserId)
        {
            if (!IsLoggedIn())
                return Json(new { success = false, message = "login_required" });

            var currentUserId = int.Parse(CurrentUserId());

            // Mark unread messages as read
            await _marketplaceService.MarkMessagesAsReadAsync(currentUserId, otherUserId, postId);

            var messages = await _marketplaceService.GetConversationAsync(currentUserId, otherUserId, postId);

            var result = messages.Select(m => new
            {
                id = m.Id,
                senderId = m.Senderid,
                receiverId = m.Receiverid,
                text = m.Message,
                createdAt = m.Createdat?.ToString("o"),
                senderName = m.Sender?.Username,
                senderAvatar = m.Sender?.Avatar
            });

            return Json(new { success = true, messages = result });
        }

        [HttpGet]
        public async Task<IActionResult> CheckProfileStatus()
        {
            if (!IsLoggedIn())
                return Json(new { success = false, message = "login_required" });

            var userId = int.Parse(CurrentUserId());
            var customer = await _marketplaceService.GetCustomerByUserIdAsync(userId);

            bool isComplete = customer != null && 
                             !string.IsNullOrWhiteSpace(customer.Phone) && 
                             !string.IsNullOrWhiteSpace(customer.Address);

            return Json(new { success = true, isComplete });
        }

        [HttpGet]
        public async Task<IActionResult> MyPosts(int page = 1)
        {
            if (!IsLoggedIn())
            {
                return RedirectToAction("Login", "Authentication", new { area = "Account", returnUrl = Request.Path + Request.QueryString });
            }

            int userId = int.Parse(CurrentUserId());
            int pageSize = 10;

            var (posts, totalCount) = await _marketplaceService.GetPostsBySellerAsync(userId, page, pageSize);

            ViewBag.Pagination = new PaginationModel
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                Controller = "MarketplacePosts",
                Action = "MyPosts",
                Area = "Marketplace",
                PageParam = "page",
                ExtraParams = new Dictionary<string, string>()
            };

            return View(posts);
        }
    }

    public class MessageRequest
    {
        public int PostId { get; set; }
        public int ReceiverId { get; set; }
        public string Message { get; set; }
    }
}
