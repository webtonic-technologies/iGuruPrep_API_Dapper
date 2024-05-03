using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;

namespace ControlPanel_API.Services.Interfaces
{
    public interface IMagazineServices
    {
        Task<ServiceResponse<string>> AddNewMagazine(MagazineDTO magazineDTO);
        Task<ServiceResponse<string>> UpdateMagazine(MagazineDTO magazineDTO);
        Task<ServiceResponse<List<MagazineDTO>>> GetAllMagazines(MagazineListDTO request);
        Task<ServiceResponse<MagazineDTO>> GetMagazineById(int id);
        Task<ServiceResponse<bool>> DeleteMagazine(int id);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
