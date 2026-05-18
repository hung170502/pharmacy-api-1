using AutoMapper;
using Pharmacy_API.Dtos.Country;
using Pharmacy_API.Dtos.Unit;
using Pharmacy_API.Filters.Country;
using Pharmacy_API.Filters.Unit;
using Pharmacy_API.Repositories.Country;
using Pharmacy_API.Repositories.Unit;
using Pharmacy_API.Services.Country;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Services.Unit
{
    public class UnitService : IUnitService
    {
        private readonly ILogger<UnitService> _logger;
        private readonly IMapper _mapper;
        private readonly IUnitRepository _unitRepository;

        public UnitService(
            ILogger<UnitService> logger,
            IMapper mapper,
            IUnitRepository unitRepository)
        {
            _logger = logger;
            _mapper = mapper;
            _unitRepository = unitRepository;
        }

        #region Insert Unit
        public async Task<UnitDto?> InsertUnitAsync(UnitRequestDto requestDto)
        {
            _logger.LogInformation("Insert Unit");

            Pharmacy_API.Models.Unit.Unit unit = new Pharmacy_API.Models.Unit.Unit();
            unit.UnitName = requestDto.UnitName;
            unit.Sort = requestDto.Sort;

            Pharmacy_API.Models.Unit.Unit? newUnit = await _unitRepository.InsertAsync(unit);

            return newUnit == null ? null : _mapper.Map<Pharmacy_API.Models.Unit.Unit, UnitDto>(newUnit);
        }
        #endregion

        #region Update Unit
        public async Task<int> UpdateUnitAsync(UnitRequestDto requestDto, int id)
        {
            _logger.LogInformation("Update Unit");

            Pharmacy_API.Models.Unit.Unit? unit = await _unitRepository.GetByIdAsync(id);
            if (unit != null)
            {
                unit.UnitName = requestDto.UnitName;
                unit.Sort = requestDto.Sort;

                return await _unitRepository.UpdateAsync(unit);
            }

            return 0;
        }
        #endregion

        #region Delete Unit
        public async Task<int> DeleteUnitAsync(int id)
        {
            _logger.LogInformation("Delete Unit");

            return await _unitRepository.DeleteAsync(id);
        }
        #endregion

        #region Get Unit
        public async Task<UnitDto?> GetUnitAsync(int id, bool isDeep = false)
        {
            _logger.LogInformation("Get Unit");


            Pharmacy_API.Models.Unit.Unit? unit = await _unitRepository.GetByIdAsync(id, isDeep);
            if (unit != null)
            {
                return _mapper.Map<Pharmacy_API.Models.Unit.Unit, UnitDto>(unit);
            }

            return null;
        }
        #endregion

        #region Get List Units
        public async Task<PagedDto<UnitDto>> GetListUnitsAsync(UnitFilterDto filterDto)
        {
            _logger.LogInformation("GetList Units");

            PagedDto<Pharmacy_API.Models.Unit.Unit> dt = await _unitRepository.GetListAsync(_mapper.Map<UnitFilterDto, UnitFilter>(filterDto));

            List<UnitDto> dtos = new List<UnitDto>();
            foreach (Pharmacy_API.Models.Unit.Unit item in dt.Data)
            {
                dtos.Add(_mapper.Map<Pharmacy_API.Models.Unit.Unit, UnitDto>(item));
            }

            return new PagedDto<UnitDto>(dt.TotalRecords, dtos);
        }
        #endregion
    }
}
