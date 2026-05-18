using Pharmacy_API.Dtos.Unit;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Services.Unit
{
    public interface IUnitService
    {
        Task<UnitDto?> InsertUnitAsync(UnitRequestDto requestDto);
        Task<int> UpdateUnitAsync(UnitRequestDto requestDto, int id);
        Task<int> DeleteUnitAsync(int id);
        Task<UnitDto?> GetUnitAsync(int id, bool isDeep = false);
        Task<PagedDto<UnitDto>> GetListUnitsAsync(UnitFilterDto filterDto);
    }
}
