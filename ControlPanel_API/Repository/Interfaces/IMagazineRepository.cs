using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.Response;
using ControlPanel_API.DTOs.ServiceResponse;

namespace ControlPanel_API.Repository.Interfaces
{
    public interface IMagazineRepository
    {
        Task<ServiceResponse<string>> AddUpdateMagazine(MagazineDTO magazineDTO);
        Task<ServiceResponse<List<MagazineResponseDTO>>> GetAllMagazines(MagazineListDTO request);
        Task<ServiceResponse<MagazineResponseDTO>> GetMagazineById(int id);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
