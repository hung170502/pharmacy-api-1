using System;

namespace Pharmacy_API.Dtos.Account
{
    public partial class PermissionDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Sort { get; set; }
    }
}