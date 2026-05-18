using Pharmacy_API.Supports;

namespace Pharmacy_API.Dtos.Category
{
    public class CategoryRequestDto : RequestDtoBase
    {
        public int? ParentId { get; set; }
        //public Models.Category.Category? Parent { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryAlias { get; set; }
        public string? CategoryImage { get; set; }
        public string? Sort { get; set; }
    }
}
