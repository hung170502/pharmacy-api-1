namespace Pharmacy_API.Dtos.Account
{
    public class AssignRoleRequest
    {
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }
}