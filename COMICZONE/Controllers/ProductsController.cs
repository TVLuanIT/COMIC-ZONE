using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using COMICZONE.Data;
using COMICZONE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ComiczoneContext _context;

        public ProductsController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: /Products/Details/5
        public async Task<IActionResult> Detail(int id, string tab = "detail")
        {
            var product = await _context.Products
                .Include(p => p.Pictures)
                .Include(p => p.Artists)
                .Include(p => p.Tags)
                .Include(p => p.ProductReviews)
                    .ThenInclude(pr => pr.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.ActiveTab = tab; // Tab active

            // Khai báo ngoài if/else
            List<Product> RelatedByAuthor;

            if (!string.IsNullOrWhiteSpace(product.Author))
            {
                RelatedByAuthor = await _context.Products
                    .Include(p => p.Pictures)
                    .Include(p => p.Artists)
                    .Include(p => p.Tags)
                    .Where(p => p.Id != product.Id &&
                                !string.IsNullOrWhiteSpace(p.Author) &&
                                p.Author.Trim().ToLower() == product.Author.Trim().ToLower())
                    .OrderByDescending(p => p.ReleaseDate)
                    .Take(8)
                    .ToListAsync();
            }
            else
            {
                RelatedByAuthor = new List<Product>();
            }

            // Nếu chưa có summary trong DB → tạo object mặc định
            if (product.ProductReviewSummary == null)
            {
                product.ProductReviewSummary = new ProductReviewSummary
                {
                    Productid = product.Id,
                    Totalreview = 0,
                    Averagerating = 0,
                    Lastupdated = null
                };
            }
            ViewBag.RelatedByAuthor = RelatedByAuthor;
            return View(product);
        }

        // GET: /Products
        public async Task<IActionResult> Index(string sortOrder,
                                                string keyword,
                                                string activeGroup,

                                                string author,
                                                string artist,
                                                string translator,
                                                string series,
                                                string tag,
                                                string publisher,
                                                string distributor,

                                                int page = 1)
        {
            int pageSize = 9; // số sản phẩm mỗi trang

            var products = _context.Products
                                    .Include(p=>p.Artists)
                                    .Include(p=>p.Tags)
                                    .Include(p=>p.Pictures)
                                    .AsQueryable();
            // FILTER THEO (từ Detail page)
            // AUTHOR
            string? currentActiveGroup = activeGroup;
            string? currentSelectedValue = null;

            if (!string.IsNullOrWhiteSpace(author))
            {
                products = products.Where(p => p.Author == author);
                currentActiveGroup = "author";
                currentSelectedValue = author;
            }
            else if (!string.IsNullOrWhiteSpace(artist))
            {
                products = products.Where(p => p.Artists.Any(a => a.Name == artist));
                currentActiveGroup = "artist";
                currentSelectedValue = artist;
            }
            else if (!string.IsNullOrWhiteSpace(translator))
            {
                products = products.Where(p => p.Translator == translator);
                currentActiveGroup = "translator";
                currentSelectedValue = translator;
            }
            else if (!string.IsNullOrWhiteSpace(series))
            {
                products = products.Where(p => p.Series == series);
                currentActiveGroup = "series";
                currentSelectedValue = series;
            }
            else if (!string.IsNullOrWhiteSpace(tag))
            {
                products = products.Where(p => p.Tags.Any(t => t.Name == tag));
                currentActiveGroup = "tag";
                currentSelectedValue = tag;
            }
            else if (!string.IsNullOrWhiteSpace(publisher))
            {
                products = products.Where(p => p.Publisher == publisher);
                currentActiveGroup = "publisher";
                currentSelectedValue = publisher;
            }
            else if (!string.IsNullOrWhiteSpace(distributor))
            {
                products = products.Where(p => p.Distributor == distributor);
                currentActiveGroup = "distributor";
                currentSelectedValue = distributor;
            }
            // FILTER THEO KEYWORD (PHẢI ĐẶT TRƯỚC SORT & PAGING)
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var key = keyword.ToLower();

                products = products.Where(p =>
                    (p.Name ?? "").ToLower().Contains(key) ||
                    (p.Author ?? "").ToLower().Contains(key) ||
                    (p.Translator ?? "").ToLower().Contains(key) ||
                    (p.Distributor ?? "").ToLower().Contains(key) ||
                    (p.Series ?? "").ToLower().Contains(key) ||
                    (p.Publisher ?? "").ToLower().Contains(key) ||
                    p.Artists.Any(a => (a.Name ?? "").ToLower().Contains(key)) ||
                    p.Tags.Any(t => (t.Name ?? "").ToLower().Contains(key))
                );

                ViewBag.SectionTitle = $"Kết quả tìm kiếm cho \"{keyword}\"";
            }
            else
            {
                ViewBag.SectionTitle = "Danh sách sản phẩm";
            }
            //SORT
            switch (sortOrder)
            {
                case "name_asc":
                    products = products.OrderBy(p => p.Name);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "newest":
                    products = products.OrderByDescending(p => p.Id); // giả sử Id tăng theo thời gian
                    break;
                case "oldest":
                    products = products.OrderBy(p => p.Id); // giả sử Id giảm theo thời gian
                    break;
                default:
                    products = products.OrderBy(p => p.Name); // mặc định theo tên tăng dần
                    break;
            }

            int totalItems = await products.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (totalItems == 0 && !string.IsNullOrWhiteSpace(keyword))
            {
                TempData["SearchMessage"] = "Không tìm thấy sản phẩm phù hợp";
            }

            var pagedProducts = await products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Xử lý sắp xếp
            ViewBag.CurrentSort = sortOrder; // giữ giá trị hiện tại cho dropdown
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Keyword = keyword;
            // sau khi filter theo author/artist/...
            ViewBag.ActiveGroup = currentActiveGroup;
            ViewBag.SelectedValue = currentSelectedValue;
            // Chuẩn bị sidebar filters
            ViewBag.SidebarFilters = new Dictionary<string, List<string>>()
            {
                { "Tác giả", (await _context.Products
                            .Where(p => !string.IsNullOrEmpty(p.Author))
                            .Select(p => p.Author!)
                            .Distinct()
                            .ToListAsync()) },
                { "Họa sĩ", await _context.Artists
                            .Select(a => a.Name!)
                            .Distinct()
                            .ToListAsync() },
                { "Dịch giả", (await _context.Products
                            .Where(p => !string.IsNullOrEmpty(p.Translator))
                            .Select(p => p.Translator!)
                            .Distinct()
                            .ToListAsync()) },
                { "Series", (await _context.Products
                            .Where(p => !string.IsNullOrEmpty(p.Series))
                            .Select(p => p.Series!)
                            .Distinct()
                            .ToListAsync()) },
                { "Tag", await _context.Tags
                            .Select(a => a.Name!)
                            .Distinct()
                            .ToListAsync() },
                { "Nhà phát hành", (await _context.Products
                            .Where(p => !string.IsNullOrEmpty(p.Distributor))
                            .Select(p => p.Distributor!)
                            .Distinct()
                            .ToListAsync()) },
                { "NXB", (await _context.Products
                            .Where(p => !string.IsNullOrEmpty(p.Publisher))
                            .Select(p => p.Publisher!)
                            .Distinct()
                            .ToListAsync()) },
            };
            return View(pagedProducts);
        }

        // GET: /Products/Search?keyword=...
        public IActionResult Search(string keyword)
        {
            // Không nhập gì → quay lại Index, không làm gì cả
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return RedirectToAction("Index");
            }

            // Có nhập → điều hướng sang Index để xử lý
            return RedirectToAction("Index", new
            {
                keyword = keyword,
                page = 1
            });
        }
    }
}
