namespace COMICZONE.Areas.Admin.ViewModels
{
    public class ArtistSearchModel : AdminSearchModel
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
        public bool? UnusedArtistsOnly { get; set; }

        // Status
        public bool? IsDeleted { get; set; }
    }
}
