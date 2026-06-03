using AutoMapper;
using Pharmacy_API.Dtos.Product;
using Pharmacy_API.Models.Product;

namespace Pharmacy_API.MapperProfiles.Product
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // ✅ Product Entity → ProductDto
            CreateMap<Pharmacy_API.Models.Product.Product, ProductDto>()
                .ForMember(dest => dest.Category,
                    opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : ""))
                .ForMember(dest => dest.Brand,
                    opt => opt.MapFrom(src => src.Brand != null ? src.Brand.BrandName : ""))
                .ForMember(dest => dest.Unit,
                    opt => opt.MapFrom(src => src.Unit != null ? src.Unit.UnitName : ""))
                .ForMember(dest => dest.BrandOrigin,
                    opt => opt.MapFrom(src => src.Country != null ? src.Country.CountryName : ""))
                .ForMember(dest => dest.Manufacturer,
                    opt => opt.MapFrom(src => src.Manufacturer != null ? src.Manufacturer.CountryName : ""))
                .ForMember(dest => dest.StockStatus,
                    opt => opt.MapFrom(src => src.StockStatus.ToString()));

            // ProductRequestDto → Product Entity
            CreateMap<ProductRequestDto, Pharmacy_API.Models.Product.Product>()
                .ForMember(dest => dest.ProductId, opt => opt.Ignore())
                .ForMember(dest => dest.Brand, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Unit, opt => opt.Ignore())
                .ForMember(dest => dest.Country, opt => opt.Ignore())
                .ForMember(dest => dest.Manufacturer, opt => opt.Ignore())
                .ForMember(dest => dest.ImageFile, opt => opt.Ignore())
                .ForMember(dest => dest.ProductCode, opt => opt.Ignore())
                // ✅ Chỉ map các FK nếu > 0
                .ForMember(dest => dest.CategoryId, opt => opt.Condition(src => src.CategoryId > 0))
                .ForMember(dest => dest.BrandId, opt => opt.Condition(src => src.BrandId > 0))
                .ForMember(dest => dest.UnitId, opt => opt.Condition(src => src.UnitId > 0))
                .ForMember(dest => dest.BrandOriginId, opt => opt.Condition(src => src.BrandOriginId > 0))
                .ForMember(dest => dest.ManufacturerId, opt => opt.Condition(src => src.ManufacturerId > 0));
        }
    }
}