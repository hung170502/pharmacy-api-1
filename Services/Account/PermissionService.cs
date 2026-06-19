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

            var permission = new Permission
            {
                Id = Guid.NewGuid().ToString(),
                Name = requestDto.Name,
                DisplayName = string.IsNullOrEmpty(requestDto.DisplayName)
                    ? GenerateDisplayName(requestDto.Name)  // Tự sinh nếu không có
                    : requestDto.DisplayName,
                Group = string.IsNullOrEmpty(requestDto.Group)
                    ? GenerateGroup(requestDto.Name)        // Tự sinh nếu không có
                    : requestDto.Group,
                Description = requestDto.Description ?? string.Empty,
                Sort = requestDto.Sort,
                CreatedAt = DateTime.UtcNow
            };

            var newPermission = await _permissionRepository.InsertAsync(permission);

            return newPermission == null ? null : _mapper.Map<Permission, PermissionDto>(newPermission);
        }
        #endregion
        // Helper methods
        private string GenerateDisplayName(string name)
        {
            var parts = name.Split('.');
            if (parts.Length != 2) return name;

            var module = parts[0];
            var action = parts[1];

            var groupName = GenerateGroup(name);
            var actionDisplay = action switch
            {
                "View" => "Xem",
                "Create" => "Thêm",
                "Edit" => "Sửa",
                "Delete" => "Xóa",
                "Export" => "Xuất",
                _ => action
            };

            return $"{actionDisplay} {groupName}".ToLower();
        }

        private string GenerateGroup(string name)
        {
            var module = name.Split('.')[0];
            return module switch
            {
                "Products" => "Sản phẩm",
                "Orders" => "Đơn hàng",
                "Customers" => "Khách hàng",
                "Users" => "Nhân viên",
                "Reports" => "Báo cáo",
                "Settings" => "Cài đặt",
                _ => module
            };
        }

        #region Update Permission
        // Services/Account/PermissionService.cs
        public async Task<int> UpdatePermissionAsync(PermissionRequestDto requestDto, string id)
        {
            _logger.LogInformation("Update Permission");

            var permission = await _permissionRepository.GetByIdAsync(id);
            if (permission == null) return 0;

            if (!string.IsNullOrEmpty(requestDto.Name) && requestDto.Name != permission.Name)
            {
                var exists = await _permissionRepository.AnyAsync(p => p.Name == requestDto.Name && p.Id != id);
                if (exists) throw new Exception($"Permission '{requestDto.Name}' already exists");
                permission.Name = requestDto.Name;
            }

            if (!string.IsNullOrEmpty(requestDto.DisplayName))
                permission.DisplayName = requestDto.DisplayName;

            if (!string.IsNullOrEmpty(requestDto.Group))
                permission.Group = requestDto.Group;

            if (!string.IsNullOrEmpty(requestDto.Description))
                permission.Description = requestDto.Description;

            permission.Sort = requestDto.Sort;
            permission.UpdatedAt = DateTime.UtcNow;

            return await _permissionRepository.UpdateAsync(permission);
            // ✅ UpdateAsync đã tự lưu, không cần SaveAsync
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