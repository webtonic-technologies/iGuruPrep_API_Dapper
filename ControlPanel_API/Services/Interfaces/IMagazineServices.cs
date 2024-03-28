using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;

namespace ControlPanel_API.Services.Interfaces
{
    public interface IMagazineServices
    {
        Task<ServiceResponse<string>> AddNewMagazine(MagazineDTO magazineDTO);
        Task<ServiceResponse<string>> UpdateMagazine(UpdateMagazineDTO magazineDTO);
        Task<ServiceResponse<IEnumerable<MagazineDTO>>> GetAllMagazines();
        Task<ServiceResponse<MagazineDTO>> GetMagazineById(int id);
        Task<ServiceResponse<bool>> DeleteMagazine(int id);
        Task<ServiceResponse<string>> UpdateMagazineFile(MagazineDTO magazineDTO);
        Task<ServiceResponse<byte[]>> GetMagazineFileById(int id);
    }
}
