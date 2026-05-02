using Microsoft.AspNetCore.Mvc;
using System.Threading;
using COMICZONE.ViewModels;
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
        private readonly IVnPayService _vnPayService;
        private readonly PaypalClient _paypalClient;

        public MarketplacePostsController(IMarketplaceService marketplaceService, IWebHostEnvironment webHostEnvironment, IVnPayService vnPayService, PaypalClient paypalClient)
        {
            _marketplaceService = marketplaceService;
            _webHostEnvironment = webHostEnvironment;
            _vnPayService = vnPayService;
            _paypalClient = paypalClient;
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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsLoggedIn())
            {
                TempData["LoginRequired"] = "Bạn cần đăng nhập để thực hiện chức năng này.";
                return RedirectToAction("Login", "Authentication", new { area = "Account" });
            }

            var post = await _marketplaceService.GetPostByIdAsync(id);
            if (post == null) return NotFound();

            if (post.Sellerid != int.Parse(CurrentUserId()))
            {
                TempData["Error"] = "Bạn không có quyền chỉnh sửa bài đăng này.";
                return RedirectToAction("MyPosts");
            }

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MarketplacePost post)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Authentication", new { area = "Account" });

            if (id != post.Id) return NotFound();

            var existingPost = await _marketplaceService.GetPostByIdAsync(id);
            if (existingPost == null || existingPost.Sellerid != int.Parse(CurrentUserId()))
            {
                return RedirectToAction("MyPosts");
            }

            ModelState.Remove("Seller");
            ModelState.Remove("Status");
            ModelState.Remove("Condition");
            ModelState.Remove("Category");
            ModelState.Remove("MarketplacePostImages");
            ModelState.Remove("MarketplaceFavorites");
            ModelState.Remove("MarketplaceMessages");

            if (ModelState.IsValid)
            {
                bool result = await _marketplaceService.UpdatePostAsync(post);
                if (result)
                {
                    TempData["Success"] = "Cập nhật thành công. Bài đăng đang chờ phê duyệt lại.";
                    return RedirectToAction("MyPosts");
                }
            }

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login", "Authentication", new { area = "Account" });

            var existingPost = await _marketplaceService.GetPostByIdAsync(id);
            if (existingPost == null || existingPost.Sellerid != int.Parse(CurrentUserId()))
            {
                return RedirectToAction("MyPosts");
            }

            bool result = await _marketplaceService.DeletePostAsync(id);
            if (result)
            {
                TempData["Success"] = "Tin đăng đã được xóa thành công.";
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa tin.";
            }

            return RedirectToAction("MyPosts");
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

        [HttpGet]
        public async Task<IActionResult> PromotionCheckout(int postId)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Authentication", new { area = "Account" });

            var post = await _marketplaceService.GetPostByIdAsync(postId);
            if (post == null || post.Sellerid != int.Parse(CurrentUserId()))
                return NotFound();

            if (post.MarketplacePostPromotions.Any(p => p.Status == "Active" && p.EndDate > DateTime.Now))
            {
                TempData["Message"] = "Bài đăng của bạn đang được quảng cáo rồi.";
                return RedirectToAction("MyPosts");
            }

            ViewBag.PaypalClientId = _paypalClient.ClientId;
            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePromotionPayment_VnPay(int postId, int days)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Authentication", new { area = "Account" });

            var post = await _marketplaceService.GetPostByIdAsync(postId);
            if (post == null || post.Sellerid != int.Parse(CurrentUserId()))
                return NotFound();

            if (days <= 0) days = 1;

            decimal totalAmount = days * 10000;
            var promotion = await _marketplaceService.PromotePostAsync(postId, post.Sellerid, days, totalAmount, "VNPAY");

            var vnPayModel = new VnPaymentRequestModel
            {
                Amount = (double)(totalAmount * 100),
                CreatedDate = DateTime.Now,
                Description = $"Thanh toan quang cao bai viet {postId}",
                FullName = CurrentUserId(),
                OrderId = promotion.Id
            };

            string returnUrl = Url.Action("PromotionPaymentCallback_VnPay", "MarketplacePosts", new { area = "Marketplace" }, Request.Scheme);
            return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel, returnUrl));
        }

        [HttpGet]
        public async Task<IActionResult> PromotionPaymentCallback_VnPay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Error"] = $"Lỗi thanh toán VN Pay: {response?.VnPayResponseCode}";
                return RedirectToAction("MyPosts");
            }

            if (!IsLoggedIn())
                return RedirectToAction("Login", "Authentication", new { area = "Account" });
            
            int promotionId = 0;

            if (!string.IsNullOrEmpty(response.OrderDescription) && response.OrderDescription.StartsWith("Thanh toan don hang "))
            {
                var idStr = response.OrderDescription.Replace("Thanh toan don hang ", "");
                int.TryParse(idStr, out promotionId);
            }
            
            if (promotionId == 0)
            {
                int.TryParse(response.OrderId, out promotionId);
            }

            if (promotionId > 0)
            {
                var activated = await _marketplaceService.ActivatePromotionAsync(promotionId);
                if (activated)
                {
                    TempData["Success"] = "Thanh toán thành công! Bài viết của bạn đã được quảng cáo nổi bật.";
                }
                else
                {
                    TempData["Error"] = "Thanh toán thành công nhưng có lỗi xảy ra khi kích hoạt khuyến mãi.";
                }
            }
            else
            {
                TempData["Error"] = "Thanh toán thành công nhưng không thể xác định mã khuyến mãi để kích hoạt.";
            }

            return RedirectToAction("MyPosts");
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaypalPromotion([FromBody] PromotionRequestModel model, CancellationToken cancellationToken)
        {
            if (!IsLoggedIn())
                return BadRequest("Bạn cần đăng nhập trước khi thanh toán.");

            var post = await _marketplaceService.GetPostByIdAsync(model.PostId);
            if (post == null || post.Sellerid != int.Parse(CurrentUserId()))
                return BadRequest("Bài đăng không khả dụng.");

            int days = model.Days <= 0 ? 1 : model.Days;
            decimal totalAmountVnd = days * 10000;
            decimal usdRate = 25400;
            var totalAmountUsd = Math.Round(totalAmountVnd / usdRate, 2);
            var stringUSD = totalAmountUsd.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);

            try
            {
                var promotion = await _marketplaceService.PromotePostAsync(model.PostId, post.Sellerid, days, totalAmountVnd, "PAYPAL");
                
                HttpContext.Session.SetInt32("PendingPaypalPromotionId", promotion.Id);

                var response = await _paypalClient.CreateOrder(stringUSD, "USD", "PR" + promotion.Id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new { message = ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CapturePaypalPromotion([FromQuery] string orderID, CancellationToken cancellationToken)
        {
            if (!IsLoggedIn())
                return BadRequest("Bạn cần đăng nhập trước khi bắt đầu.");

            try
            {
                var response = await _paypalClient.CaptureOrder(orderID);

                if (response.status != "COMPLETED")
                    return BadRequest("Thanh toán Paypal chưa hoàn tất.");

                var promotionId = HttpContext.Session.GetInt32("PendingPaypalPromotionId");
                if (promotionId.HasValue)
                {
                    await _marketplaceService.ActivatePromotionAsync(promotionId.Value);
                    HttpContext.Session.Remove("PendingPaypalPromotionId");
                    TempData["Success"] = "Thanh toán Paypal thành công! Bài viết đã được quảng cáo.";
                    return Ok(new { success = true });
                }

                return BadRequest("Không tìm thấy dữ liệu cấu hình quảng cáo.");
            }
            catch (Exception ex)
            {
                var error = new { message = ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }
    }

    public class MessageRequest
    {
        public int PostId { get; set; }
        public int ReceiverId { get; set; }
        public string Message { get; set; }
    }

    public class PromotionRequestModel
    {
        public int PostId { get; set; }
        public int Days { get; set; }
    }
}
