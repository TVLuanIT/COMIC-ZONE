using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using COMICZONE.Data;
using COMICZONE.Models;
using COMICZONE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using COMICZONE.ViewModels;

namespace COMICZONE.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ComiczoneContext _context;
        private readonly IVnPayService _vnPayservice;

        public OrdersController(ComiczoneContext context, IVnPayService vnPayservice)
        {
            _context = context;
            _vnPayservice = vnPayservice;
        }

        public IActionResult OrderDetails(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Pictures)
                .Include(o => o.User)
                    .ThenInclude(u => u.Customer)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            ViewBag.Page = "OrderDetails";
            ViewBag.Order = order;

            return View("~/Views/UserProfiles/MyOrders.cshtml");
        }

        public IActionResult Success()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Checkout(string fullname, string phone, string address, string note, string paymentMethod)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Authentication");

            int userId = int.Parse(userIdStr);

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToAction("Index", "Carts");
            }

            // Validate input
            var errors = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(fullname))
                errors["fullname"] = "Họ và tên không được để trống";
            if (string.IsNullOrWhiteSpace(phone))
                errors["phone"] = "Số điện thoại không được để trống";
            if (string.IsNullOrWhiteSpace(address))
                errors["address"] = "Địa chỉ không được để trống";
            if (string.IsNullOrWhiteSpace(paymentMethod))
                errors["paymentMethod"] = "Chọn phương thức thanh toán";

            if (errors.Any())
            {
                // Không return View("Index", cart") nữa
                TempData["CheckoutErrors"] = errors;
                TempData["CustomerFullname"] = fullname;
                TempData["CustomerPhone"] = phone;
                TempData["CustomerAddress"] = address;

                return RedirectToAction("Index", "Carts");
            }

            decimal totalAmount = cart.CartItems.Sum(i => i.Quantity * (i.Product?.Price ?? 0));

            // COD → xử lý luôn
            if (paymentMethod == "COD")
            {
                CreateOrder(userId, address, phone, note, "COD", true, null);
                return RedirectToAction("Success");
            }

            // VNPay → chuyển sang cổng thanh toán
            if (paymentMethod == "VnPay")
            {
                var vnPayModel = new VnPaymentRequestModel
                {
                    Amount = (double)(totalAmount * 100),
                    CreatedDate = DateTime.Now,
                    Description = $"Thanh toán đơn hàng của {fullname}",
                    FullName = fullname,
                    OrderId = new Random().Next(1000, 10000) // Tạo mã đơn hàng ngẫu nhiên
                };

                return Redirect(_vnPayservice.CreatePaymentUrl(HttpContext, vnPayModel));
            }

            // Nếu phương thức khác không hợp lệ
            TempData["Error"] = "Phương thức thanh toán không hợp lệ.";
            return RedirectToAction("Index", "Carts");
        }

        private void CreateOrder(int userId, string address, string phone, string note, string paymentMethod, bool isPaid, string? transactionId)
        {
            var cartItems = _context.CartItems
                .Include(x => x.Product)
                .Include(x => x.Cart)
                .Where(x => x.Cart.UserId == userId)
                .ToList();

            if (!cartItems.Any())
                throw new Exception("Cart is empty");

            var order = new Order
            {
                UserId = userId,
                ShippingAddress = address,
                PhoneNumber = phone,
                Note = note,
                CreatedAt = DateTime.Now,
                OrderDate = DateTime.Now,
                Status = "Pending",
                TotalAmount = cartItems.Sum(x => x.Product.Price * x.Quantity)
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            foreach (var item in cartItems)
            {
                var orderDetail = new OrderItem
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                };

                _context.OrderItems.Add(orderDetail);
            }

            _context.SaveChanges();

            _context.CartItems.RemoveRange(cartItems);
            _context.SaveChanges();
        }

        public IActionResult PaymentCallBack()
        {
            var reponse = _vnPayservice.PaymentExecute(Request.Query);
            if(reponse == null || reponse.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VN Pay: {reponse.VnPayResponseCode}";
                return RedirectToAction("PaymentFail");
            }

            // Tạo đơn hàng sau khi thanh toán thành công

            TempData["Message"] = "Thanh toán VN Pay thành công!";
            return RedirectToAction("Success");
        }

        public IActionResult PaymentFail()
        {
            return View();
        }
    }
}
