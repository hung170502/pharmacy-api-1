using AutoMapper;
using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Dtos.Product;
using Pharmacy_API.Filters.Account;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Models.Product;
using System.Security;

namespace Pharmacy_API.MapperProfiles.Product
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Ánh xạ từ Product entity sang ProductDto
            CreateMap<Pharmacy_API.Models.Product.Product, ProductDto>()
                .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand.BrandName))
                .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category.CategoryName))
                .ForMember(dest => dest.BrandOrigin, opt => opt.MapFrom(src => src.Country.CountryName))
                .ForMember(dest => dest.Unit, opt => opt.MapFrom(src => src.Unit.UnitName))
                .ForMember(dest => dest.Manufacturer, opt => opt.MapFrom(src => src.Manufacturer.CountryName));

            // Ánh xạ từ ProductRequestDto sang Product entity
            CreateMap<ProductRequestDto, Pharmacy_API.Models.Product.Product>();
        }
    }
}
