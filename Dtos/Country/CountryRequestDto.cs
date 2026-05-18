using Pharmacy_API.Supports;

namespace Pharmacy_API.Dtos.Country
{
    public class CountryRequestDto : RequestDtoBase
    {
        public string? CountryName { get; set; }
        public string? Sort { get; set; }
    }
}
