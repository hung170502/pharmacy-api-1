using Pharmacy_API.Dtos.Product;
using Pharmacy_API.Models.Product;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Services.Product
{
    public interface IProductService
    {
        Task<PagedDto<ProductDto>> GetListProductsAsync(ProductFilterDto filterDto);  // ✅ Đổi từ Models.Product.Product sang ProductDto
        Task<ProductDto> GetProductByIdAsync(int id);
        Task<ProductDto> CreateProductAsync(ProductRequestDto requestDto);
        Task<ProductDto> UpdateProductAsync(int id, ProductRequestDto requestDto);
        Task<bool> DeleteProductAsync(int id);
    }
}