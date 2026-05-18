using AutoMapper;
using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Filters.Account;
using Pharmacy_API.Models.Account;
using System.Security;

namespace Pharmacy_API.MapperProfiles.Account
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            #region User Mapper
            CreateMap<ApplicationUser, UserDto>().ReverseMap();
            CreateMap<UserFilterDto, UserFilter>().ReverseMap();
            #endregion

            #region Policy Mapper  
            CreateMap<PolicyRequestDto, Policy>()
          .ForMember(dest => dest.PolicyPermissions, opt => opt.Ignore());
            CreateMap<Policy, PolicyDto>().ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.PolicyPermissions.Select(ur => ur.Permission)));

            CreateMap<PolicyFilterDto, PolicyFilter>();
            #endregion

            #region Permission Mapper
            CreateMap<Permission, PermissionDto>();
            CreateMap<PermissionFilterDto, PermissionFilter>();
            #endregion

            #region Role Mapper 
            CreateMap<Role, RoleDto>();
            CreateMap<RoleFilterDto, RoleFilter>();
            CreateMap<RoleRequestDto, Role>().ForMember(dest => dest.RolePolicies, opt => opt.Ignore());
            CreateMap<Role, RoleDto>().ForMember(dest => dest.Policies, opt => opt.MapFrom(src => src.RolePolicies.Select(ur => ur.Policy)));

            #endregion

        }
    }
}
