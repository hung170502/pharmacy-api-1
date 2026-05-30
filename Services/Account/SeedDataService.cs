using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pharmacy_API.Context;
using Pharmacy_API.Models.Account;

public class SeedDataService
{
    private readonly AccountContext _context;
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public SeedDataService(
        AccountContext context,
        RoleManager<Role> roleManager,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task SeedAsync()
    {
        // 1. Tạo Permissions
        var permissions = new List<Permission>
        {
            // Products
            new() { Name = "Products.View", DisplayName = "Xem sản phẩm", Group = "Sản phẩm", Sort = 1 },
            new() { Name = "Products.Create", DisplayName = "Thêm sản phẩm", Group = "Sản phẩm", Sort = 2 },
            new() { Name = "Products.Edit", DisplayName = "Sửa sản phẩm", Group = "Sản phẩm", Sort = 3 },
            new() { Name = "Products.Delete", DisplayName = "Xóa sản phẩm", Group = "Sản phẩm", Sort = 4 },
            
            // Orders
            new() { Name = "Orders.View", DisplayName = "Xem đơn hàng", Group = "Đơn hàng", Sort = 1 },
            new() { Name = "Orders.Create", DisplayName = "Tạo đơn hàng", Group = "Đơn hàng", Sort = 2 },
            new() { Name = "Orders.Edit", DisplayName = "Sửa đơn hàng", Group = "Đơn hàng", Sort = 3 },
            new() { Name = "Orders.Delete", DisplayName = "Xóa đơn hàng", Group = "Đơn hàng", Sort = 4 },
            
            // Customers
            new() { Name = "Customers.View", DisplayName = "Xem khách hàng", Group = "Khách hàng", Sort = 1 },
            new() { Name = "Customers.Create", DisplayName = "Thêm khách hàng", Group = "Khách hàng", Sort = 2 },
            new() { Name = "Customers.Edit", DisplayName = "Sửa khách hàng", Group = "Khách hàng", Sort = 3 },
            
            // Users
            new() { Name = "Users.View", DisplayName = "Xem nhân viên", Group = "Nhân viên", Sort = 1 },
            new() { Name = "Users.Create", DisplayName = "Thêm nhân viên", Group = "Nhân viên", Sort = 2 },
            new() { Name = "Users.Edit", DisplayName = "Sửa nhân viên", Group = "Nhân viên", Sort = 3 },
            new() { Name = "Users.Delete", DisplayName = "Xóa nhân viên", Group = "Nhân viên", Sort = 4 },
            
            // Reports
            new() { Name = "Reports.View", DisplayName = "Xem báo cáo", Group = "Báo cáo", Sort = 1 },
            new() { Name = "Reports.Export", DisplayName = "Xuất báo cáo", Group = "Báo cáo", Sort = 2 },
            
            // Settings
            new() { Name = "Settings.View", DisplayName = "Xem cài đặt", Group = "Cài đặt", Sort = 1 },
            new() { Name = "Settings.Edit", DisplayName = "Sửa cài đặt", Group = "Cài đặt", Sort = 2 },
        };

        foreach (var permission in permissions)
        {
            if (!await _context.Permissions.AnyAsync(p => p.Name == permission.Name))
            {
                _context.Permissions.Add(permission);
            }
        }
        await _context.SaveChangesAsync();

        // 2. Tạo Policies
        var policies = new List<(string Name, string DisplayName, string Group, int Sort)>
        {
            ("ProductManagement", "Quản lý sản phẩm", "Sản phẩm", 1),
            ("OrderManagement", "Quản lý đơn hàng", "Đơn hàng", 2),
            ("CustomerManagement", "Quản lý khách hàng", "Khách hàng", 3),
            ("UserManagement", "Quản lý nhân viên", "Nhân viên", 4),
            ("ReportManagement", "Quản lý báo cáo", "Báo cáo", 5),
            ("SettingsManagement", "Quản lý cài đặt", "Cài đặt", 6),
        };

        foreach (var (name, displayName, group, sort) in policies)
        {
            if (!await _context.Policies.AnyAsync(p => p.Name == name))
            {
                var policy = new Policy
                {
                    Name = name,
                    DisplayName = displayName,
                    Sort = sort
                };
                _context.Policies.Add(policy);
                await _context.SaveChangesAsync();

                // Gán permissions cho policy theo Group
                var policyPermissions = await _context.Permissions
                    .Where(p => p.Group == group)
                    .ToListAsync();

                foreach (var perm in policyPermissions)
                {
                    _context.PolicyPermissions.Add(new PolicyPermission
                    {
                        PolicyId = policy.Id,
                        PermissionId = perm.Id
                    });
                }
                await _context.SaveChangesAsync();
            }
        }

        // 3. Tạo Roles
        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            var adminRole = new Role
            {
                Name = "Admin",
                DisplayName = "Quản trị viên",
                Description = "Toàn quyền hệ thống",
                Sort = 1
            };
            await _roleManager.CreateAsync(adminRole);

            // Gán tất cả policies cho Admin
            var allPolicies = await _context.Policies.ToListAsync();
            foreach (var policy in allPolicies)
            {
                _context.RolePolicies.Add(new RolePolicy
                {
                    RoleId = adminRole.Id,
                    PolicyId = policy.Id
                });
            }
            await _context.SaveChangesAsync();
        }

        if (!await _roleManager.RoleExistsAsync("Staff"))
        {
            var staffRole = new Role
            {
                Name = "Staff",
                DisplayName = "Nhân viên",
                Description = "Quyền hạn cơ bản",
                Sort = 2
            };
            await _roleManager.CreateAsync(staffRole);

            // Gán 1 số policies cơ bản cho Staff
            var basicPolicies = await _context.Policies
                .Where(p => p.Name == "ProductManagement" || p.Name == "OrderManagement")
                .ToListAsync();

            foreach (var policy in basicPolicies)
            {
                _context.RolePolicies.Add(new RolePolicy
                {
                    RoleId = staffRole.Id,
                    PolicyId = policy.Id
                });
            }
            await _context.SaveChangesAsync();
        }

        if (!await _roleManager.RoleExistsAsync("Customer"))
        {
            var customerRole = new Role
            {
                Name = "Customer",
                DisplayName = "Khách hàng",
                Description = "Quyền cơ bản",
                Sort = 3
            };
            await _roleManager.CreateAsync(customerRole);
        }

        // ❌ KHÔNG tạo Admin user mặc định
        // Admin sẽ được tạo thủ công sau khi deploy
    }
}