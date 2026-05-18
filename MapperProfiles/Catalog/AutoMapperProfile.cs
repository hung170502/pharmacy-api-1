using AutoMapper;
using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Dtos.Brand;
using Pharmacy_API.Dtos.Category;
using Pharmacy_API.Dtos.Country;
using Pharmacy_API.Dtos.Unit;
using Pharmacy_API.Filters.Account;
using Pharmacy_API.Filters.Brand;
using Pharmacy_API.Filters.Category;
using Pharmacy_API.Filters.Country;
using Pharmacy_API.Filters.Unit;
using Pharmacy_API.Models.Account;

namespace Pharmacy_API.MapperProfiles.Category
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            #region Category Mapper
            CreateMap<Pharmacy_API.Models.Category.Category, CategoryDto>().ReverseMap();
            CreateMap<CategoryFilterDto, CategoryFilter>().ReverseMap();
            #endregion

            #region Brand Mapper
            CreateMap<Pharmacy_API.Models.Brand.Brand, BrandDto>().ReverseMap();
            CreateMap<BrandFilterDto, BrandFilter>().ReverseMap();
            #endregion

            #region Country Mapper
            CreateMap<Pharmacy_API.Models.Country.Country, CountryDto>().ReverseMap();
            CreateMap<CountryFilterDto, CountryFilter>().ReverseMap();
            #endregion

            #region Unit Mapper
            CreateMap<Pharmacy_API.Models.Unit.Unit, UnitDto>().ReverseMap();
            CreateMap<UnitFilterDto, UnitFilter>().ReverseMap();
            #endregion
        }

    }
}
