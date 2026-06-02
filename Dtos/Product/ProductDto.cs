using System.ComponentModel.DataAnnotations;

namespace Pharmacy_API.Dtos.Product
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public double Price { get; set; }
        public string? Images { get; set; }
        public string? Description { get; set; }
        public string? NameAlias { get; set; }
        public DateTime ProductionDate { get; set; }
        public double Sale { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public string? SortDescription { get; set; }
        public string? DosageForm { get; set; }
        public string? Packaging { get; set; }
        public string BrandOrigin { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string? Ingredients { get; set; }
        public string? Usage { get; set; }
        public string DosageAndAdministration { get; set; } = string.Empty;
        public string SideEffects { get; set; } = string.Empty;
        public string Precautions { get; set; } = string.Empty;
        public string Storage { get; set; } = string.Empty;
        public string? Sort { get; set; }
        public string StockStatus { get; set; } = "InStock"; // Trả về string cho client
        public bool IsActive { get; set; }
        public DateTime? ActiveFrom { get; set; }
    }
}