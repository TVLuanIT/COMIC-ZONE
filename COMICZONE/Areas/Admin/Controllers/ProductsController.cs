using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using COMICZONE.Data;
using COMICZONE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : AdminBaseController
    {
        private readonly ComiczoneContext _context;

        public ProductsController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: Admin/Products
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Pictures)
                .Include(p => p.Artists)
                .Include(p => p.Tags)
                .Include(p => p.ProductReviewSummary)
                .ToListAsync();

            return View(products);
        }

        // GET: Admin/Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Pictures)
                .Include(p => p.Artists)
                .Include(p => p.Tags)
                .Include(p => p.ProductReviewSummary)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Admin/Products/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Artists = await _context.Artists.ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(t => !t.Isdeleted).ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Product product,
            int[] SelectedArtists,
            int[] SelectedTags,
            List<IFormFile> Pictures)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Artists = await _context.Artists.ToListAsync();
                ViewBag.Tags = await _context.Tags.Where(t => !t.Isdeleted).ToListAsync();
                return View(product);
            }

            // ===============================
            // Artists
            // ===============================
            if (SelectedArtists != null)
            {
                foreach (var artistId in SelectedArtists)
                {
                    var artist = await _context.Artists.FindAsync(artistId);
                    if (artist != null)
                        product.Artists.Add(artist);
                }
            }

            // ===============================
            // Tags
            // ===============================
            if (SelectedTags != null)
            {
                foreach (var tagId in SelectedTags)
                {
                    var tag = await _context.Tags.FindAsync(tagId);
                    if (tag != null)
                        product.Tags.Add(tag);
                }
            }

            // ===============================
            // Upload pictures
            // ===============================
            if (Pictures != null && Pictures.Count > 0)
            {
                foreach (var file in Pictures)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

                        var path = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot/images/products",
                            fileName
                        );

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        product.Pictures.Add(new Picture
                        {
                            FileName = fileName
                        });
                    }
                }
            }

            _context.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.Artists)
                .Include(p => p.Tags)
                .Include(p => p.Pictures)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            ViewBag.Artists = await _context.Artists.ToListAsync();
            ViewBag.Tags = await _context.Tags.Where(t => !t.Isdeleted).ToListAsync();

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            Product model,
            int[] SelectedArtists,
            int[] SelectedTags,
            int[] DeletedPictures,
            List<IFormFile> NewPictures)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Artists = await _context.Artists.ToListAsync();
                ViewBag.Tags = await _context.Tags.Where(t => !t.Isdeleted).ToListAsync();
                return View(model);
            }

            var product = await _context.Products
                .Include(p => p.Artists)
                .Include(p => p.Tags)
                .Include(p => p.Pictures)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            // cập nhật field
            product.Name = model.Name;
            product.Price = model.Price;
            product.Distributor = model.Distributor;
            product.Author = model.Author;
            product.Translator = model.Translator;
            product.Series = model.Series;
            product.Description = model.Description;
            product.StockQuantity = model.StockQuantity;
            product.Format = model.Format;
            product.Size = model.Size;
            product.Weight = model.Weight;
            product.Pages = model.Pages;
            product.IllustrationType = model.IllustrationType;
            product.ReleaseDate = model.ReleaseDate;
            product.Publisher = model.Publisher;
            product.AgeGroup = model.AgeGroup;

            // ===== Artists =====
            product.Artists.Clear();

            if (SelectedArtists != null)
            {
                foreach (var artistId in SelectedArtists)
                {
                    var artist = await _context.Artists.FindAsync(artistId);
                    if (artist != null)
                    {
                        product.Artists.Add(artist);
                    }
                }
            }

            // ===== Tags =====
            product.Tags.Clear();

            if (SelectedTags != null)
            {
                foreach (var tagId in SelectedTags)
                {
                    var tag = await _context.Tags.FindAsync(tagId);
                    if (tag != null)
                    {
                        product.Tags.Add(tag);
                    }
                }
            }

            // ===== Xóa ảnh =====
            if (DeletedPictures != null)
            {
                var pics = product.Pictures
                    .Where(p => DeletedPictures.Contains(p.Id))
                    .ToList();

                foreach (var pic in pics)
                {
                    product.Pictures.Remove(pic);   // remove relation
                    _context.Pictures.Remove(pic);  // remove entity

                    if (!string.IsNullOrEmpty(pic.FileName))
                    {
                        var path = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot/images/products",
                            pic.FileName);

                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                    }
                }
            }
            }

            // ===== Upload ảnh mới =====
            if (NewPictures != null && NewPictures.Any())
            {
                foreach (var file in NewPictures)
                {
                    if (file.Length <= 0) continue;

                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

                    var path = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot/images/products",
                        fileName);

                    using var stream = new FileStream(path, FileMode.Create);
                    await file.CopyToAsync(stream);

                    product.Pictures.Add(new Picture
                    {
                        FileName = fileName
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Pictures)
                .Include(p => p.Artists)
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.CartItemsCount = await _context.CartItems.CountAsync(c => c.ProductId == id);
            ViewBag.InventoryLogsCount = await _context.InventoryLogs.CountAsync(i => i.ProductId == id);
            ViewBag.OrderItemsCount = await _context.OrderItems.CountAsync(o => o.ProductId == id);
            ViewBag.ReviewsCount = await _context.ProductReviews.CountAsync(r => r.Productid == id);
            ViewBag.ViewsCount = await _context.UserProductViews.CountAsync(v => v.ProductId == id);

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleDelete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.Isdeleted = !product.Isdeleted;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isDeleted = product.Isdeleted });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products
                .Include(p => p.Pictures)
                .Include(p => p.Artists)
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. UserProductViews
                var userProductViews = await _context.UserProductViews.Where(v => v.ProductId == id).ToListAsync();
                if (userProductViews.Any()) _context.UserProductViews.RemoveRange(userProductViews);

                // 2. InventoryLogs
                var inventoryLogs = await _context.InventoryLogs.Where(l => l.ProductId == id).ToListAsync();
                if (inventoryLogs.Any()) _context.InventoryLogs.RemoveRange(inventoryLogs);

                // 3. CartItems
                var cartItems = await _context.CartItems.Where(c => c.ProductId == id).ToListAsync();
                if (cartItems.Any()) _context.CartItems.RemoveRange(cartItems);

                // 4. OrderItems
                var orderItems = await _context.OrderItems.Where(o => o.ProductId == id).ToListAsync();
                if (orderItems.Any()) _context.OrderItems.RemoveRange(orderItems);

                // 5. ProductReviewSummaries
                var reviewSummary = await _context.ProductReviewSummaries.FirstOrDefaultAsync(s => s.Productid == id);
                if (reviewSummary != null) _context.ProductReviewSummaries.Remove(reviewSummary);

                // 6. ProductReviews (kèm Likes và Replies)
                var reviewIds = await _context.ProductReviews.Where(r => r.Productid == id).Select(r => r.Reviewid).ToListAsync();
                if (reviewIds.Any())
                {
                    var replies = await _context.ProductReviewReplies.Where(r => reviewIds.Contains(r.Reviewid)).ToListAsync();
                    if (replies.Any())
                    {
                        var replyIds = replies.Select(r => r.Replyid).ToList();
                        var replyLikes = await _context.ProductReviewReplyLikes.Where(rl => replyIds.Contains(rl.Replyid)).ToListAsync();
                        if (replyLikes.Any()) _context.ProductReviewReplyLikes.RemoveRange(replyLikes);

                        foreach(var rep in replies) rep.Parentreplyid = null; // Gỡ tự tham chiếu trước
                        await _context.SaveChangesAsync(); 
                        _context.ProductReviewReplies.RemoveRange(replies);
                    }

                    var reviewLikes = await _context.ProductReviewLikes.Where(rl => reviewIds.Contains(rl.Reviewid)).ToListAsync();
                    if (reviewLikes.Any()) _context.ProductReviewLikes.RemoveRange(reviewLikes);

                    var reviews = await _context.ProductReviews.Where(r => r.Productid == id).ToListAsync();
                    _context.ProductReviews.RemoveRange(reviews);
                }

                // 7. Xóa ảnh cứng và dữ liệu
                foreach (var pic in product.Pictures.ToList())
                {
                    if (!string.IsNullOrEmpty(pic.FileName))
                    {
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products", pic.FileName);
                        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                    }
                    product.Pictures.Remove(pic);
                    _context.Pictures.Remove(pic);
                }

                // 8. Xóa Artists và Tags
                product.Artists.Clear();
                product.Tags.Clear();

                // 9. Xóa Product
                _context.Products.Remove(product);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
