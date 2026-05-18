namespace Pharmacy_API.Dtos.Product
{
    public class ProductRequestDto
    {
        public string ProductName { get; set; } = string.Empty;
        public double Price { get; set; }
        public string? Images { get; set; }
        public string? Description { get; set; }
        public string? NameAlias { get; set; }
        public DateTime ProductionDate { get; set; }
        public double Sale { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public int UnitId { get; set; }
        public string? SortDescription { get; set; }
        public string? DosageForm { get; set; }
        public string? Packaging { get; set; }
        public int CountryId { get; set; } // Quốc gia sản xuất
        public string Manufacturer { get; set; }
        public string? Ingredients { get; set; }
        public string? Usage { get; set; }
        public string DosageAndAdministration { get; set; }
        public string SideEffects { get; set; }
        public string Precautions { get; set; }
        public string Storage { get; set; }
        public string? Sort { get; set; }
        public string StockStatus { get; set; } = "InStock"; // "InStock" hoặc "OutOfStock"
        public bool IsActive { get; set; } = true;
        public DateTime? ActiveFrom { get; set; }


    }
}

