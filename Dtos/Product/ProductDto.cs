using Pharmacy_API.Models.Brand;
using Pharmacy_API.Models.Category;
using Pharmacy_API.Models.Country;
using Pharmacy_API.Models.Unit;
using System.ComponentModel.DataAnnotations;

namespace Pharmacy_API.Dtos.Product
{
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public double Price { get; set; }
        public string? Images { get; set; }
        public string? Description { get; set; }
        public string? NameAlias { get; set; }
        public DateTime ProductionDate { get; set; }
        public double Sale { get; set; }
        public string Category { get; set; } = string.Empty; // Ensure non-null default value
        public string Brand { get; set; } = string.Empty; // Ensure non-null default value
        public string Unit { get; set; } = string.Empty; // Ensure non-null default value
        public string? SortDescription { get; set; }
        public string? DosageForm { get; set; }
        public string? Packaging { get; set; }
        public string BrandOrigin { get; set; } = string.Empty; // Ensure non-null default value
        public string Manufacturer { get; set; } = string.Empty; // Ensure non-null default value
        public string? Ingredients { get; set; }
        public string? Usage { get; set; }
        public string DosageAndAdministration { get; set; } = string.Empty; // Ensure non-null default value
        public string SideEffects { get; set; } = string.Empty; // Ensure non-null default value
        public string Precautions { get; set; } = string.Empty; // Ensure non-null default value
        public string Storage { get; set; } = string.Empty; // Ensure non-null default value
        public string? Sort { get; set; }
        public bool IsActive { get; set; }
    }
}
