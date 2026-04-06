namespace COMICZONE.Areas.Admin.ViewModels
{
    public class TagSearchModel : AdminSearchModel
    {
        public int? Id { get; set; }
        public string? Name { get; set; }

        // Product associations
        public List<int>? ProductIds { get; set; }
        public string? ProductNames { get; set; }

        // Metrics
        public bool? HasProducts { get; set; }
        public int? ProductCountMin { get; set; }
        public int? ProductCountMax { get; set; }
        
        // Flags
        public bool? UnusedTagsOnly { get; set; }
        public bool? PopularTagsOnly { get; set; }

        // Status
        public bool? IsDeleted { get; set; }
    }
}
