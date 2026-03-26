using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using COMICZONE.Data;
using COMICZONE.Models;
using COMICZONE.Models.Enums;
using COMICZONE.Models.Requests;
using COMICZONE.Services;
using COMICZONE.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Controllers
{
    public class OrdersController : BaseController
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
        public IActionResult Checkout(CheckoutViewModel model)
        {
            var userIdStr = CurrentUserId();

            if (!IsLoggedIn())
                return RedirectToAction("Login", "Authentication");

            if (!int.TryParse(userIdStr, out int userId))
                return RedirectToAction("Login", "Authentication");

            if (!ModelState.IsValid)
            {
                TempData["CheckoutErrors"] = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .ToDictionary(
                        k => k.Key,
                        v => v.Value?.Errors.First().ErrorMessage
                    );

                TempData["CustomerFullname"] = model.Fullname;
                TempData["CustomerPhone"] = model.Phone;
                TempData["CustomerAddress"] = model.Address;

                return RedirectToAction("Index", "Carts");
            }

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng trống.";
                return RedirectToAction("Index", "Carts");
            }

            decimal totalAmount = cart.CartItems.Sum(i => i.Quantity * (i.Product?.Price ?? 0));

            // ================= COD =================
            if (model.PaymentMethod == PaymentMethod.COD)
            {
                CreateOrder(new CreateOrderRequest
                {
                    UserId = userId,
                    Address = model.Address,
                    Phone = model.Phone,
                    Note = model.Note,
                    PaymentMethod = "COD",
                    IsPaid = false,
                    TransactionId = null
                });

                return RedirectToAction("Success");
            }

            if (model.PaymentMethod == PaymentMethod.VNPAY)
            {
                HttpContext.Session.SetString("Checkout_Address", model.Address);
                HttpContext.Session.SetString("Checkout_Phone", model.Phone);
                HttpContext.Session.SetString("Checkout_Note", model.Note ?? "");
                
                var vnPayModel = new VnPaymentRequestModel
                {
                    Amount = (double)(totalAmount * 100),
                    CreatedDate = DateTime.Now,
                    Description = $"Thanh toán đơn hàng của {model.Fullname}",
                    FullName = model.Fullname,
                    OrderId = new Random().Next(1000, 10000)
                };

                return Redirect(_vnPayservice.CreatePaymentUrl(HttpContext, vnPayModel));
            }

            TempData["Error"] = "Phương thức thanh toán không hợp lệ.";
            return RedirectToAction("Index", "Carts");
        }

        private void CreateOrder(CreateOrderRequest request)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var cartItems = _context.CartItems
                    .Include(x => x.Product)
                    .Include(x => x.Cart)
                    .Where(x => x.Cart.UserId == request.UserId)
                    .ToList();

                if (!cartItems.Any())
                    throw new Exception("Cart is empty");

                decimal totalAmount = cartItems.Sum(x => (x.Product.Price ?? 0) * x.Quantity);

                var order = new Order
                {
                    UserId = request.UserId,
                    ShippingAddress = request.Address,
                    PhoneNumber = request.Phone,
                    Note = request.Note,
                    CreatedAt = DateTime.Now,
                    OrderDate = DateTime.Now,
                    Status = request.IsPaid ? "COMPLETED" : "PENDING",
                    TotalAmount = totalAmount
                };

                _context.Orders.Add(order);
                _context.SaveChanges();

                var payment = new Payment
                {
                    Orderid = order.OrderId,
                    Amount = totalAmount,
                    Paymentmethod = request.PaymentMethod,
                    Paymentstatus = request.IsPaid
                        ? PaymentStatus.SUCCESS.ToString()
                        : PaymentStatus.PENDING.ToString(),
                    Transactionid = request.TransactionId,
                    Createdat = DateTime.Now,
                    Paidat = request.IsPaid ? DateTime.Now : null
                };

                _context.Payments.Add(payment);

                foreach (var item in cartItems)
                {
                    _context.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price
                    });
                }

                _context.CartItems.RemoveRange(cartItems);

                _context.SaveChanges();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public IActionResult PaymentCallBack()
        {
            var response = _vnPayservice.PaymentExecute(Request.Query);

            if (response == null || response.VnPayResponseCode != "00")
            {
                TempData["Message"] = $"Lỗi thanh toán VN Pay: {response?.VnPayResponseCode}";

                return RedirectToAction("PaymentFail");
            }

            if (!IsLoggedIn())
                return RedirectToAction("Login", "Authentication");

            if (!int.TryParse(CurrentUserId(), out int userId))
                return RedirectToAction("Login", "Authentication");

            // tránh duplicate order nếu callback chạy lại
            var existedPayment = _context.Payments.FirstOrDefault(p => p.Transactionid == response.TransactionId);

            if (existedPayment != null)
                return RedirectToAction("Success");

            var address = HttpContext.Session.GetString("Checkout_Address");

            var phone = HttpContext.Session.GetString("Checkout_Phone");

            var note = HttpContext.Session.GetString("Checkout_Note");

            if (address == null || phone == null)
            {
                TempData["Message"] = "Phiên thanh toán đã hết hạn.";

                return RedirectToAction("Index", "Carts");
            }

            CreateOrder(new CreateOrderRequest
            {
                UserId = userId,
                Address = address,
                Phone = phone,
                Note = note,
                PaymentMethod = PaymentMethod.VNPAY.ToString(),
                IsPaid = true,
                TransactionId = response.TransactionId ?? "UNKNOWN"
            });

            TempData["Message"] = "Thanh toán VN Pay thành công!";

            HttpContext.Session.Remove("Checkout_Address");
            HttpContext.Session.Remove("Checkout_Phone");
            HttpContext.Session.Remove("Checkout_Note");

            return RedirectToAction("Success");
        }

        public IActionResult PaymentFail()
        {
            return View();
        }
    }
}
