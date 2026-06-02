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

            // Áp dụng filter
            if (!string.IsNullOrEmpty(filterDto.Keyword))
            {
                var keyword = filterDto.Keyword.ToLower();
                query = query.Where(p =>
                    p.ProductName.ToLower().Contains(keyword) ||
                    (p.Description != null && p.Description.ToLower().Contains(keyword)));
            }

            if (filterDto.CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == filterDto.CategoryId.Value);

            if (filterDto.BrandId.HasValue)
                query = query.Where(p => p.BrandId == filterDto.BrandId.Value);

            if (filterDto.BrandOriginId.HasValue)
                query = query.Where(p => p.BrandOriginId == filterDto.BrandOriginId.Value);

            if (filterDto.UnitId.HasValue)
                query = query.Where(p => p.UnitId == filterDto.UnitId.Value);

            if (filterDto.MinPrice.HasValue)
                query = query.Where(p => p.Price >= filterDto.MinPrice.Value);

            if (filterDto.MaxPrice.HasValue)
                query = query.Where(p => p.Price <= filterDto.MaxPrice.Value);

            if (filterDto.StockStatus.HasValue)
                query = query.Where(p => p.StockStatus == filterDto.StockStatus.Value);

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

            product.ActiveFrom = requestDto.ActiveFrom;
            product.IsActive = requestDto.IsActive;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> UpdateProductAsync(int id, ProductRequestDto requestDto)
        {
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null) return null;

            _mapper.Map(requestDto, existingProduct);

            existingProduct.ActiveFrom = requestDto.ActiveFrom;
            existingProduct.IsActive = requestDto.IsActive;

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