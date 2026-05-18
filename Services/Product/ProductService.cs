using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Pharmacy_API.Context;
using Pharmacy_API.Dtos.Product;
using Pharmacy_API.Models.Product;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Services.Product
{
    public class ProductService : IProductService
    {
        private readonly AccountContext _context;
        private readonly IMapper _mapper;

        public ProductService(AccountContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PagedDto<Models.Product.Product>> GetListProductsAsync(ProductFilterDto filterDto)
        {
            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Country)
                .Include(p => p.Unit)
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var products = await query
                .Skip((filterDto.Page - 1) * filterDto.PageSize)
                .Take(filterDto.PageSize)
                .ToListAsync();

            return new PagedDto<Models.Product.Product>(totalCount, products);
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Country)
                .Include(p => p.Unit)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            return product == null ? null : _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> CreateProductAsync(ProductRequestDto requestDto)
        {
            var product = _mapper.Map<Models.Product.Product>(requestDto);
            // ✅ Convert thủ công string => enum
            product.ActiveFrom = requestDto.ActiveFrom;
            product.IsActive = requestDto.IsActive;
            product.StockStatus = Enum.TryParse<StockStatus>(requestDto.StockStatus, true, out var status)
                ? status
                : StockStatus.InStock; // fallback nếu sai
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> UpdateProductAsync(int id, ProductRequestDto requestDto)
        {
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null) return null;

            _mapper.Map(requestDto, existingProduct);
            existingProduct.IsActive = requestDto.IsActive;
            existingProduct.ActiveFrom = requestDto.ActiveFrom;
            // ✅ Gán lại StockStatus từ string
            existingProduct.StockStatus = Enum.TryParse<StockStatus>(requestDto.StockStatus, true, out var status)
                ? status
                : existingProduct.StockStatus;

            _context.Products.Update(existingProduct);
            await _context.SaveChangesAsync();

            return _mapper.Map<ProductDto>(existingProduct);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
