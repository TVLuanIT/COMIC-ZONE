using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using COMICZONE.Data;
using COMICZONE.Models;
using COMICZONE.Models.Requests;
using Microsoft.AspNetCore.Authorization;

namespace COMICZONE.Controllers
{
    public class CartsController : Controller
    {
        private readonly ComiczoneContext _context;

        public CartsController(ComiczoneContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult UpdateQuantity([FromBody] UpdateCartRequest req)
        {
            var item = _context.CartItems
                .Include(c => c.Product)
                .FirstOrDefault(c => c.CartItemId == req.CartItemId);

            if (item == null) return Json(new { success = false });

            item.Quantity = req.Quantity;

            _context.SaveChanges();

            var itemTotalValue = item.Quantity * (item.Product?.Price ?? 0);

            var cartTotalValue = _context.CartItems
                .Where(c => c.CartId == item.CartId)
                .Include(c => c.Product)
                .Sum(c => c.Quantity * (c.Product!.Price ?? 0));

            var itemTotal = itemTotalValue.ToString("N0");
            var cartTotal = cartTotalValue.ToString("N0");

            return Json(new
            {
                success = true,
                itemTotal,
                cartTotal
            });
        }

        public IActionResult Remove(int cartItemId)
        {
            var item = _context.CartItems.Find(cartItemId);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Index()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr))
            {
                TempData["LoginRequired"] = "Vui lòng đăng nhập để xem giỏ hàng.";
                return RedirectToAction("Index", "Home");
            }

            int userId = int.Parse(userIdStr);

            var cart = _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                        .ThenInclude(p => p.Pictures)
                .FirstOrDefault(c => c.UserId == userId);

            // nếu chưa có cart thì tạo mới
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now,
                    CartItems = new List<CartItem>()
                };

                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            // lấy thông tin user
            var user = _context.Users
                .Include(u => u.Customer)
                .FirstOrDefault(u => u.Id == userId);

            if (user != null && user.Customer != null)
            {
                ViewBag.Customer = user.Customer;

                //ViewBag.FullName = user.Customer.Fullname;
                //ViewBag.Phone = user.Customer.Phone;
                //ViewBag.Address = user.Customer.Address;
            }

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int productId)
        {
            var referer = Request.Headers["Referer"].ToString();

            if (string.IsNullOrEmpty(referer))
            {
                referer = Url.Action("Index", "Home")!;
            }

            var userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr))
            {
                TempData["LoginRequired"] = "Vui lòng đăng nhập để thêm vào giỏ hàng.";
                return Redirect(referer);
            }

            int userId = int.Parse(userIdStr);

            var product = _context.Products.Find(productId);

            if (product == null)
            {
                return NotFound();
            }

            if (product.StockQuantity <= 0)
            {
                TempData["Error"] = "Sản phẩm đã hết hàng.";
                return Redirect(referer);
            }

            var cart = _context.Carts.FirstOrDefault(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.Now
                };

                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            var cartItem = _context.CartItems
                .FirstOrDefault(ci => ci.CartId == cart.CartId && ci.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity += 1;

                TempData["Success"] = "Sản phẩm đã có trong giỏ hàng.";
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    Quantity = 1
                };

                _context.CartItems.Add(cartItem);

                TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng.";
            }

            _context.SaveChanges();

            return Redirect(referer);
        }
    }
}
