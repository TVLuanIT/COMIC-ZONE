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
    public class ProductsController : Controller
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
            ViewBag.Tags = await _context.Tags.ToListAsync();

            return View();
        }

        // POST: Admin/Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, int[] SelectedArtists, int[] SelectedTags)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Artists = await _context.Artists.ToListAsync();
                ViewBag.Tags = await _context.Tags.ToListAsync();
                return View(product);
            }

            if (SelectedArtists != null)
            {
                foreach (var artistId in SelectedArtists)
                {
                    var artist = await _context.Artists.FindAsync(artistId);
                    if (artist != null)
                        product.Artists.Add(artist);
                }
            }

            if (SelectedTags != null)
            {
                foreach (var tagId in SelectedTags)
                {
                    var tag = await _context.Tags.FindAsync(tagId);
                    if (tag != null)
                        product.Tags.Add(tag);
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
            ViewBag.Tags = await _context.Tags.ToListAsync();

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
                ViewBag.Tags = await _context.Tags.ToListAsync();
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
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Admin/Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
