using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Repository.Interfaces;
using ControlPanel_API.Services.Interfaces;

namespace ControlPanel_API.Services.Implementations
{
    public class MagazineServices : IMagazineServices
    {
        private readonly IMagazineRepository  _magazineRepository;

        public MagazineServices(IMagazineRepository magazineRepository)
        {
            _magazineRepository = magazineRepository;
        }
        public async Task<ServiceResponse<string>> AddNewMagazine(MagazineDTO request)
        {
            try
            {
                return await _magazineRepository.AddNewMagazine(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<bool>> DeleteMagazine(int id)
        {
            try
            {
                return await _magazineRepository.DeleteMagazine(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }

        public async Task<ServiceResponse<List<MagazineDTO>>> GetAllMagazines(MagazineListDTO request)
        {

            try
            {
                return await _magazineRepository.GetAllMagazines(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<MagazineDTO>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<MagazineDTO>> GetMagazineById(int id)
        {
            try
            {
                return await _magazineRepository.GetMagazineById(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<MagazineDTO>(false, ex.Message, new MagazineDTO(), 500);
            }
        }
        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                return await _magazineRepository.StatusActiveInactive(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }

        public async Task<ServiceResponse<string>> UpdateMagazine(MagazineDTO request)
        {
            try
            {
                return await _magazineRepository.UpdateMagazine(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}
