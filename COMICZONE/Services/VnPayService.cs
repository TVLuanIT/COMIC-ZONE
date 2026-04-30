using COMICZONE.Helpers;
using COMICZONE.Models;
using COMICZONE.ViewModels;

namespace COMICZONE.Services
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _config;

        public VnPayService(IConfiguration config)
        {
            _config = config;
        }

        public string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model, string? returnUrlOverride = null)
        {
            var vnpay = new VnPayLibrary();
            var timeNow = DateTime.UtcNow.AddHours(7);
            var tick = DateTime.Now.ToString("yyyyMMddHHmmss");

            vnpay.AddRequestData("vnp_Version", _config["VnPay:Version"]);
            vnpay.AddRequestData("vnp_Command", _config["VnPay:Command"]);
            vnpay.AddRequestData("vnp_TmnCode", _config["VnPay:TmnCode"]);
            vnpay.AddRequestData("vnp_Amount", ((long)(model.Amount)).ToString());

            vnpay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", _config["VnPay:CurrCode"]);

            var ipAddr = Utils.GetIpAddress(context);
            if (string.IsNullOrEmpty(ipAddr) || ipAddr == "::1" || ipAddr == "127.0.0.1" || ipAddr.StartsWith("Invalid IP"))
            {
                ipAddr = "14.226.2.164"; // Dummy public IP for sandbox
            }

            vnpay.AddRequestData("vnp_IpAddr", ipAddr);
            vnpay.AddRequestData("vnp_Locale", _config["VnPay:Locale"]);
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang " + model.OrderId);
            vnpay.AddRequestData("vnp_OrderType", "other");
            
            var returnUrl = string.IsNullOrEmpty(returnUrlOverride) ? _config["VnPay:PaymentBackUrl"] : returnUrlOverride;
            vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
            
            vnpay.AddRequestData("vnp_TxnRef", tick);


            var paymentUrl = vnpay.CreateRequestUrl(_config["VnPay:BaseUrl"], _config["VnPay:HashSecret"]);

            Console.WriteLine(model.Amount);

            return paymentUrl;
        }

        public VnPaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var vnpay = new VnPayLibrary();

            foreach(var(key, value) in collections)
            {
                if(!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value.ToString());
                }
            }

            var vnp_orderId = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));
            var vnp_TransactionId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
            var vnp_SecureHash = collections.FirstOrDefault(x => x.Key == "vnp_SecureHash").Value;
            var vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
            var vnp_OrderInfo = vnpay.GetResponseData("vnp_OrderInfo");

            bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, _config["VnPay:HashSecret"]);
            if (!checkSignature)
            {
                return new VnPaymentResponseModel
                {
                    Success = false,
                };
            }

            return new VnPaymentResponseModel
            {
                Success = true,
                PaymentMethod = "VnPay",
                OrderId = vnp_orderId.ToString(),
                TransactionId = vnp_TransactionId.ToString(),
                VnPayResponseCode = vnp_ResponseCode,
                OrderDescription = vnp_OrderInfo,
                Token = vnp_SecureHash,
            };
        }
    }
}
