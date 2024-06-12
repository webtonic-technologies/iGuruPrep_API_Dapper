using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.Response;
using ControlPanel_API.DTOs.ServiceResponse;

namespace ControlPanel_API.Services.Interfaces
{
    public interface IMagazineServices
    {
        Task<ServiceResponse<string>> AddNewMagazine(MagazineDTO magazineDTO);
        Task<ServiceResponse<string>> UpdateMagazine(MagazineDTO magazineDTO);
        Task<ServiceResponse<List<MagazineResponseDTO>>> GetAllMagazines(MagazineListDTO request);
        Task<ServiceResponse<MagazineResponseDTO>> GetMagazineById(int id);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
