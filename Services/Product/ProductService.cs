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
        private readonly CloudinaryService _cloudinaryService; // THÊM

        public ProductService(AccountContext context, IMapper mapper, CloudinaryService cloudinaryService)
        {
            _context = context;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<PagedDto<ProductDto>> GetListProductsAsync(ProductFilterDto filterDto)
        {
            var query = _context.Products.AsNoTracking().AsQueryable();

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
                .OrderByDescending(p => p.ProductId)
                .Skip((filterDto.Page - 1) * filterDto.PageSize)
                .Take(filterDto.PageSize)
                .Select(p => new ProductDto
                {
                    ProductId = p.ProductId,
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    Sale = p.Sale,
                    Images = p.Images,
                    Description = p.Description,
                    NameAlias = p.NameAlias,
                    ProductionDate = p.ProductionDate,
                    SortDescription = p.SortDescription,
                    DosageForm = p.DosageForm,
                    Packaging = p.Packaging,
                    Ingredients = p.Ingredients,
                    Usage = p.Usage,
                    DosageAndAdministration = p.DosageAndAdministration,
                    SideEffects = p.SideEffects,
                    Precautions = p.Precautions,
                    Storage = p.Storage,
                    StockStatus = p.StockStatus.ToString(),
                    IsActive = p.IsActive,
                    ActiveFrom = p.ActiveFrom,
                    CategoryId = p.CategoryId,
                    BrandId = p.BrandId,
                    UnitId = p.UnitId,
                    BrandOriginId = p.BrandOriginId,
                    ManufacturerId = p.ManufacturerId,
                    Category = p.Category.CategoryName ?? "",
                    CategoryAlias = p.Category.CategoryAlias ?? "",
                    Brand = p.Brand.BrandName ?? "",
                    Unit = p.Unit.UnitName ?? "",
                    BrandOrigin = p.Country.CountryName ?? "",
                    Manufacturer = p.Manufacturer.CountryName ?? "",
                })
                .ToListAsync();

            return new PagedDto<ProductDto>(totalCount, products);
        }

        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Country)
                .Include(p => p.Unit)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return null;

            return new ProductDto
            {
                ProductId = product.ProductId,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                Price = product.Price,
                Sale = product.Sale,
                Images = product.Images,
                Description = product.Description,
                NameAlias = product.NameAlias,
                ProductionDate = product.ProductionDate,
                SortDescription = product.SortDescription,
                DosageForm = product.DosageForm,
                Packaging = product.Packaging,
                Ingredients = product.Ingredients,
                Usage = product.Usage,
                DosageAndAdministration = product.DosageAndAdministration,
                SideEffects = product.SideEffects,
                Precautions = product.Precautions,
                Storage = product.Storage,
                Sort = product.Sort,
                IsActive = product.IsActive,
                ActiveFrom = product.ActiveFrom,
                StockStatus = product.StockStatus.ToString(),
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                UnitId = product.UnitId,
                BrandOriginId = product.BrandOriginId,
                ManufacturerId = product.ManufacturerId,
                Category = product.Category?.CategoryName ?? "",
                CategoryAlias = product.Category?.CategoryAlias ?? "",
                Brand = product.Brand?.BrandName ?? "",
                Unit = product.Unit?.UnitName ?? "",
                BrandOrigin = product.Country?.CountryName ?? "",
                Manufacturer = product.Manufacturer?.CountryName ?? ""
            };
        }

        // THÊM: Helper upload nhiều ảnh
        private async Task<(string? urls, string? publicIds)> UploadImagesAsync(List<IFormFile>? files)
        {
            if (files == null || files.Count == 0)
                return (null, null);

            var urls = new List<string>();
            var publicIds = new List<string>();

            foreach (var file in files)
            {
                var result = await _cloudinaryService.UploadImageAsync(file, "products");
                if (result?.Error == null)
                {
                    urls.Add(result.SecureUrl.ToString());
                    publicIds.Add(result.PublicId);
                }
            }

            return (string.Join(";", urls), string.Join(";", publicIds));
        }

        // THÊM: Helper xóa nhiều ảnh
        private async Task DeleteImagesAsync(string? publicIds)
        {
            if (string.IsNullOrEmpty(publicIds)) return;

            var ids = publicIds.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var id in ids)
            {
                await _cloudinaryService.DeleteImageAsync(id.Trim());
            }
        }

        public async Task<ProductDto> CreateProductAsync(ProductRequestDto requestDto)
        {
            // Upload ảnh lên Cloudinary
            var (urls, publicIds) = await UploadImagesAsync(requestDto.ImageFiles);

            var product = _mapper.Map<Models.Product.Product>(requestDto);
            product.Images = urls ?? requestDto.Images;
            product.ImagePublicIds = publicIds;

            if (string.IsNullOrEmpty(product.ProductCode))
                product.ProductCode = await GenerateProductCodeAsync();

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

            // Nếu có ảnh mới, xóa ảnh cũ rồi upload ảnh mới
            if (requestDto.ImageFiles != null && requestDto.ImageFiles.Count > 0)
            {
                await DeleteImagesAsync(existingProduct.ImagePublicIds);
                var (urls, publicIds) = await UploadImagesAsync(requestDto.ImageFiles);
                existingProduct.Images = urls;
                existingProduct.ImagePublicIds = publicIds;
            }

            if (!string.IsNullOrEmpty(requestDto.ProductName))
                existingProduct.ProductName = requestDto.ProductName;

            existingProduct.NameAlias = requestDto.NameAlias;
            existingProduct.Price = requestDto.Price;
            existingProduct.Sale = requestDto.Sale;
            existingProduct.Description = requestDto.Description;
            existingProduct.SortDescription = requestDto.SortDescription;
            existingProduct.DosageForm = requestDto.DosageForm;
            existingProduct.Packaging = requestDto.Packaging;
            existingProduct.Ingredients = requestDto.Ingredients;
            existingProduct.Usage = requestDto.Usage;
            existingProduct.DosageAndAdministration = requestDto.DosageAndAdministration;
            existingProduct.SideEffects = requestDto.SideEffects;
            existingProduct.Precautions = requestDto.Precautions;
            existingProduct.Storage = requestDto.Storage;
            existingProduct.Sort = requestDto.Sort;
            existingProduct.StockStatus = requestDto.StockStatus;
            existingProduct.IsActive = requestDto.IsActive;
            existingProduct.ActiveFrom = requestDto.ActiveFrom;
            existingProduct.ProductionDate = requestDto.ProductionDate;

            if (requestDto.CategoryId > 0)
                existingProduct.CategoryId = requestDto.CategoryId;
            if (requestDto.BrandId > 0)
                existingProduct.BrandId = requestDto.BrandId;
            if (requestDto.UnitId > 0)
                existingProduct.UnitId = requestDto.UnitId;
            if (requestDto.BrandOriginId > 0)
                existingProduct.BrandOriginId = requestDto.BrandOriginId;
            if (requestDto.ManufacturerId > 0)
                existingProduct.ManufacturerId = requestDto.ManufacturerId;

            await _context.SaveChangesAsync();
            return _mapper.Map<ProductDto>(existingProduct);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return false;

            // Xóa ảnh trên Cloudinary
            await DeleteImagesAsync(product.ImagePublicIds);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<string> GenerateProductCodeAsync()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string code;
            do
            {
                code = new string(Enumerable.Repeat(chars, 8)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
            while (await _context.Products.AnyAsync(p => p.ProductCode == code));
            return code;
        }

        public async Task<ProductDto?> GetProductByAliasAsync(string nameAlias)
        {
            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Country)
                .Include(p => p.Unit)
                .FirstOrDefaultAsync(p => p.NameAlias == nameAlias);

            if (product == null) return null;

            return new ProductDto
            {
                ProductId = product.ProductId,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                Price = product.Price,
                Sale = product.Sale,
                Images = product.Images,
                Description = product.Description,
                NameAlias = product.NameAlias,
                ProductionDate = product.ProductionDate,
                SortDescription = product.SortDescription,
                DosageForm = product.DosageForm,
                Packaging = product.Packaging,
                Ingredients = product.Ingredients,
                Usage = product.Usage,
                DosageAndAdministration = product.DosageAndAdministration,
                SideEffects = product.SideEffects,
                Precautions = product.Precautions,
                Storage = product.Storage,
                StockStatus = product.StockStatus.ToString(),
                IsActive = product.IsActive,
                ActiveFrom = product.ActiveFrom,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                UnitId = product.UnitId,
                BrandOriginId = product.BrandOriginId,
                ManufacturerId = product.ManufacturerId,
                Category = product.Category?.CategoryName ?? "",
                CategoryAlias = product.Category?.CategoryAlias ?? "",
                Brand = product.Brand?.BrandName ?? "",
                Unit = product.Unit?.UnitName ?? "",
                BrandOrigin = product.Country?.CountryName ?? "",
                Manufacturer = product.Manufacturer?.CountryName ?? ""
            };
        }
    }
}