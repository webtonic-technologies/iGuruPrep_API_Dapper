using Config_API.DTOs;
using Config_API.DTOs.ServiceResponse;

namespace Config_API.Services.Interfaces
{
    public interface IBoardServices
    {
        Task<ServiceResponse<List<Board>>> GetAllBoards(GetAllBoardsRequest request);
        Task<ServiceResponse<Board>> GetBoardById(int id);
        Task<ServiceResponse<string>> AddUpdateBoard(Board request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
