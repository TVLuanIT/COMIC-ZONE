namespace COMICZONE.Services
{
    public interface IProductSearchService
    {
        Task<string> GetStoreContextAsync(string message);
    }
}
