using System;

namespace COMICZONE.Areas.Admin.ViewModels
{
    public class CustomerSearchModel : AdminSearchModel
    {
        public int? CustomerId { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }
    }
}
