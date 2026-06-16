using AutoMapper;
using Pharmacy_API.Dtos.Brand;
using Pharmacy_API.Filters.Brand;
using Pharmacy_API.Models.Brand;

namespace Pharmacy_API.MapperProfiles.Brand
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // ✅ Map từ Entity sang DTO (response)
            CreateMap<Pharmacy_API.Models.Brand.Brand, BrandDto>()
                .ForMember(dest => dest.BrandId, opt => opt.MapFrom(src => src.BrandId))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.BrandName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.BrandImage, opt => opt.MapFrom(src => src.BrandImage))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Sort, opt => opt.MapFrom(src => src.Sort));
            // ✅ Nếu muốn trả về ImagePublicId, thêm dòng này:
            // .ForMember(dest => dest.ImagePublicId, opt => opt.MapFrom(src => src.ImagePublicId));

            // ✅ Map từ DTO request sang Entity (insert/update)
            CreateMap<BrandRequestDto, Pharmacy_API.Models.Brand.Brand>()
                .ForMember(dest => dest.BrandId, opt => opt.Ignore()) // Ignore vì tự sinh
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.BrandName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.BrandImage, opt => opt.MapFrom(src => src.BrandImage)) // ✅ Map BrandImage
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Sort, opt => opt.MapFrom(src => src.Sort))
                .ForMember(dest => dest.ImagePublicId, opt => opt.MapFrom(src => src.ImagePublicId)); // ✅ Map ImagePublicId

            // ✅ Map từ Filter DTO sang Filter Entity
            CreateMap<BrandFilterDto, BrandFilter>();
        }
    }
}