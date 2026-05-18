using Microsoft.Extensions.Logging;
using AutoMapper;
using Pharmacy_API.Supports;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Repositories.Account;
using Pharmacy_API.Dtos.Account;
using Pharmacy_API.Filters.Account;

namespace Pharmacy_API.Services.Account
{
    public class PermissionService : IPermissionService
    {
        #region Fields
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        protected readonly IPermissionRepository _permissionRepository;
        #endregion

        #region Constructors
        public PermissionService(
            ILogger<PermissionService> logger,
            IMapper mapper,
            IPermissionRepository permissionRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _permissionRepository = permissionRepository;
        }
        #endregion

        #region Insert Permission
        public async Task<PermissionDto?> InsertPermissionAsync(PermissionRequestDto requestDto)
        {
            _logger.LogInformation("Insert Permission");

            Permission permission = new Permission();
            permission.Id = Guid.NewGuid().ToString();
            permission.Name = requestDto.Name;
            permission.Sort = requestDto.Sort;

            Permission? newPermission = await _permissionRepository.InsertAsync(permission);

            return newPermission == null ? null : _mapper.Map<Permission, PermissionDto>(newPermission);
        }
        #endregion

        #region Update Permission
        public async Task<int> UpdatePermissionAsync(PermissionRequestDto requestDto, string id)
        {
            _logger.LogInformation("Update Permission");


            Permission? permission = await _permissionRepository.GetByIdAsync(id);
            if (permission != null)
            {
                permission.Name = requestDto.Name;
                permission.Sort = requestDto.Sort;

                return await _permissionRepository.UpdateAsync(permission);
            }

            return 0;
        }
        #endregion

        #region Delete Permission
        public async Task<int> DeletePermissionAsync(string id)
        {
            _logger.LogInformation("Delete Permission");


            return await _permissionRepository.DeleteAsync(id);
        }
        #endregion

        #region Get Permission
        public async Task<PermissionDto?> GetPermissionAsync(string id, bool isDeep = false)
        {
            _logger.LogInformation("Get Permission");


            Permission? permission = await _permissionRepository.GetByIdAsync(id, isDeep);
            if (permission != null)
            {
                return _mapper.Map<Permission, PermissionDto>(permission);
            }

            return null;
        }
        #endregion

        #region Get List Permissions
        public async Task<PagedDto<PermissionDto>> GetListPermissionsAsync(PermissionFilterDto filterDto)
        {
            _logger.LogInformation("GetList Permissions");

            PagedDto<Permission> dt = await _permissionRepository.GetListAsync(_mapper.Map<PermissionFilterDto, PermissionFilter>(filterDto));

            List<PermissionDto> dtos = new List<PermissionDto>();
            foreach (Permission item in dt.Data)
            {
                dtos.Add(_mapper.Map<Permission, PermissionDto>(item));
            }

            return new PagedDto<PermissionDto>(dt.TotalRecords, dtos);
        }
        #endregion
    }
}