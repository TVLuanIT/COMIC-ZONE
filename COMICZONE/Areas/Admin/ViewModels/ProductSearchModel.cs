namespace COMICZONE.Areas.Admin.ViewModels
{
    public class ProductSearchModel : AdminSearchModel
    {
        // Basic Info
        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Author { get; set; }
        public string? Translator { get; set; }
        public string? Series { get; set; }
        public string? Publisher { get; set; }
        public string? Distributor { get; set; }
        public string? Description { get; set; }

        // Format & Specs
        public string? Format { get; set; }
        public string? Size { get; set; }
        public string? Weight { get; set; }
        public int? Pages { get; set; }
        public string? IllustrationType { get; set; }
        public string? AgeGroup { get; set; }

        // Price Range
        public int? PriceMin { get; set; }
        public int? PriceMax { get; set; }

        // Stock Info
        public int? StockQuantityMin { get; set; }
        public int? StockQuantityMax { get; set; }
        public string? StockStatus { get; set; } // Available, OutOfStock, LowStock

        // Release Date Range
        public DateTime? ReleaseDateFrom { get; set; }
        public DateTime? ReleaseDateTo { get; set; }

        // Tags
        public List<int>? TagIds { get; set; }
        public string? TagNames { get; set; } // Comma separated or single keyword search

        // Artists
        public List<int>? ArtistIds { get; set; }
        public string? ArtistNames { get; set; } // Comma separated or single keyword search

        // Existence
        public bool? HasReviews { get; set; }
        public bool? HasPictures { get; set; }
        public bool? HasOrders { get; set; }

        // Status
        public bool? IsDeleted { get; set; }
    }
}
