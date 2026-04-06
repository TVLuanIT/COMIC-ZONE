namespace COMICZONE.Areas.Admin.ViewModels
{
    public class BlogSearchModel : AdminSearchModel
    {
        public int? CategoryId { get; set; }
        public bool? IsDeleted { get; set; }
    }
}
