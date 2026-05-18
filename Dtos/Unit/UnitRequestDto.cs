using Pharmacy_API.Supports;

namespace Pharmacy_API.Dtos.Unit
{
    public class UnitRequestDto : RequestDtoBase
    {
        public string? UnitName { get; set; }
        public string? Sort { get; set; }
    }
}
