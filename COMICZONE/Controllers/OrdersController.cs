using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
        private readonly PaypalClient _paypalClient;

        public OrdersController(ComiczoneContext context, IVnPayService vnPayservice, PaypalClient paypalClient)
        {
            _context = context;
            _vnPayservice = vnPayservice;
            _paypalClient = paypalClient;
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

            string? errorMessage;

            // ================= COD =================
            if (model.PaymentMethod == PaymentMethod.COD)
            {
                var result = CreateOrder(new CreateOrderConRequest
                {
                    UserId = userId,
                    Address = model.Address,
                    Phone = model.Phone,
                    Note = model.Note,
                    PaymentMethod = "COD",
                    IsPaid = false,
                    TransactionId = null
                }, false, out errorMessage);

                if (!result)
                {
                    TempData["Error"] = errorMessage;
                    return RedirectToAction("Index", "Carts");
                }

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

            if (model.PaymentMethod == PaymentMethod.PAYPAL)
            {
                HttpContext.Session.SetString("Checkout_Address", model.Address);
                HttpContext.Session.SetString("Checkout_Phone", model.Phone);
                HttpContext.Session.SetString("Checkout_Note", model.Note ?? "");

                return RedirectToAction("Success");
            }

            TempData["Error"] = "Phương thức thanh toán không hợp lệ.";

            return RedirectToAction("Index", "Carts");
        }

        private bool CreateOrder(CreateOrderConRequest request, bool allowOutOfStockOrder, out string? errorMessage)
        {
            errorMessage = null;

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var cartItems = _context.CartItems
                    .Include(x => x.Product)
                    .Include(x => x.Cart)
                    .Where(x => x.Cart.UserId == request.UserId)
                    .ToList();

                if (!cartItems.Any())
                {
                    errorMessage = "Giỏ hàng đang trống.";
                    return false;
                }

                bool hasOutOfStockItem = false;

                // CHECK tồn kho trước khi tạo Order
                foreach (var item in cartItems)
                {
                    var product = item.Product;

                    if (product.StockQuantity < item.Quantity)
                    {
                        if (!allowOutOfStockOrder)
                        {
                            errorMessage =
                                $"Sản phẩm '{product.Name}' không đủ số lượng trong kho.";
                            return false;
                        }
                        hasOutOfStockItem = true;
                    }
                }

                decimal totalAmount = cartItems.Sum(x => (x.Product.Price ?? 0) * x.Quantity);

                var orderStatus = hasOutOfStockItem ? "OUT_OF_STOCK" : (request.IsPaid ? "COMPLETED" : "PENDING");

                var order = new Order
                {
                    UserId = request.UserId,
                    ShippingAddress = request.Address,
                    PhoneNumber = request.Phone,
                    Note = request.Note,
                    CreatedAt = DateTime.Now,
                    OrderDate = DateTime.Now,
                    Status = orderStatus,
                    TotalAmount = totalAmount
                };

                _context.Orders.Add(order);
                _context.SaveChanges();

                // ORDER STATUS HISTORY
                _context.OrderStatusHistories.Add(new OrderStatusHistory
                {
                    OrderId = order.OrderId,
                    Status = order.Status,
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = request.UserId
                });

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
                    var product = item.Product;

                    // chỉ trừ tồn kho nếu đủ hàng
                    if (product.StockQuantity >= item.Quantity)
                    {
                        product.StockQuantity -= item.Quantity;
                    }

                    // Tính subtotal
                    decimal price = product.Price ?? 0;
                    decimal subtotal = price * item.Quantity;

                    // Log inventory change
                    _context.InventoryLogs.Add(new InventoryLog
                    {
                        ProductId = product.Id,
                        ChangeAmount = -item.Quantity,
                        Type = "ORDER_CREATED",
                        CreatedAt = DateTime.Now
                    });

                    _context.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price,
                        Subtotal = subtotal
                    });
                }

                // CREATE INVOICE (nếu đã thanh toán)
                if (request.IsPaid)
                {
                    var customer = _context.Customers
                        .FirstOrDefault(x => x.Userid == request.UserId);

                    _context.Invoices.Add(new Invoice
                    {
                        OrderId = order.OrderId,
                        TotalAmount = totalAmount,
                        IssueDate = DateTime.Now,
                        CustomerName = customer?.Fullname ?? "Guest"
                    });
                }

                _context.CartItems.RemoveRange(cartItems);
                _context.SaveChanges();

                transaction.Commit();

                return true;
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

            string? errorMessage;

            var result = CreateOrder(new CreateOrderConRequest
            {
                UserId = userId,
                Address = address,
                Phone = phone,
                Note = note,
                PaymentMethod = PaymentMethod.VNPAY.ToString(),
                IsPaid = true,
                TransactionId = response.TransactionId ?? "UNKNOWN"
            }, true, out errorMessage);

            if (!result)
            {
                TempData["Error"] = errorMessage;
                return RedirectToAction("Index", "Carts");
            }

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

        #region Paypal payment
        [HttpPost("/Orders/create-paypal-order")]
        public async Task<IActionResult> CreatePaypalOrder(CancellationToken cancellationToken)
        {
            if (!IsLoggedIn())
                return BadRequest("Bạn cần đăng nhập trước khi thanh toán.");

            if (!int.TryParse(CurrentUserId(), out int userId))
                return BadRequest("Không xác định được danh tính người dùng.");

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return BadRequest("Giỏ hàng của bạn đang trống.");

            var tongTienVND = cart.CartItems.Sum(p =>
                p.Quantity * (p.Product?.Price ?? 0));

            decimal usdRate = 25400;
            var tongTienUSD = Math.Round(tongTienVND / usdRate, 2);

            var stringUSD = tongTienUSD.ToString(
                "0.00",
                System.Globalization.CultureInfo.InvariantCulture
            );

            try
            {
                var response = await _paypalClient.CreateOrder(
                    stringUSD,
                    "USD",
                    "DH" + DateTime.Now.Ticks.ToString()
                );

                return Ok(response);
            }
            catch (Exception ex)
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }

        [HttpPost("/Orders/capture-paypal-order")]
        public async Task<IActionResult> CapturePaypalOrder(string orderID, CancellationToken cancellationToken)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Authentication");

            if (!int.TryParse(CurrentUserId(), out int userId))
                return RedirectToAction("Login", "Authentication");

            try
            {
                var response = await _paypalClient.CaptureOrder(orderID);

                //lưu đơn hàng vào database
                if (response.status != "COMPLETED")
                    return BadRequest("Thanh toán chưa hoàn tất");

                var address = HttpContext.Session.GetString("Checkout_Address");
                var phone = HttpContext.Session.GetString("Checkout_Phone");
                var note = HttpContext.Session.GetString("Checkout_Note");

                string? errorMessage;

                var result = CreateOrder(new CreateOrderConRequest
                {
                    UserId = userId,
                    Address = address,
                    Phone = phone,
                    Note = note,
                    PaymentMethod = PaymentMethod.PAYPAL.ToString(),
                    IsPaid = true,
                    TransactionId = orderID
                }, true, out errorMessage);

                if (!result)
                {
                    TempData["Error"] = errorMessage;
                    return RedirectToAction("Index", "Carts");
                }

                HttpContext.Session.Remove("Checkout_Address");
                HttpContext.Session.Remove("Checkout_Phone");
                HttpContext.Session.Remove("Checkout_Note");

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                var error = new { ex.GetBaseException().Message };
                return BadRequest(error);
            }
        }
        #endregion
    }
}
