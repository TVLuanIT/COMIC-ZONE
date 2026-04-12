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

        public async Task<IActionResult> Index()
        {
            var posts = await _marketplaceService.GetAllPostsAsync("Approved");
            return View(posts);
        }

        public async Task<IActionResult> Details(int id)
        {
            var post = await _marketplaceService.GetPostByIdAsync(id);
            if (post == null) return NotFound();
            return View(post);
        }

        public IActionResult Create()
        {
            if (!IsLoggedIn())
            {
                TempData["LoginRequired"] = "Bạn cần đăng nhập để đăng bán.";
                return RedirectToAction("Index", "Home", new { area = "" });
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
    }
}
