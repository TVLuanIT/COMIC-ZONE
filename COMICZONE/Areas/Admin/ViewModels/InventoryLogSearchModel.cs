namespace COMICZONE.Areas.Admin.ViewModels
{
    public class InventoryLogSearchModel : AdminSearchModel
    {
        public int? Id { get; set; }

        // Product associations
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }

        // Transaction details
        public string? Type { get; set; }
        public List<string>? Types { get; set; }

        // Change amounts
        public int? ChangeAmountMin { get; set; }
        public int? ChangeAmountMax { get; set; }

        // Directional filters
        public bool? IncreaseOnly { get; set; }
        public bool? DecreaseOnly { get; set; }

        // Date range
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        // Magnitude filter
        public bool? LargeChangesOnly { get; set; } // abs(ChangeAmount) >= 50
    }
}
