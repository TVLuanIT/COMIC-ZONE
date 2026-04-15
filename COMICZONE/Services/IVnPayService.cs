using COMICZONE.ViewModels;

namespace COMICZONE.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model, string? returnUrlOverride = null);
        VnPaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}
