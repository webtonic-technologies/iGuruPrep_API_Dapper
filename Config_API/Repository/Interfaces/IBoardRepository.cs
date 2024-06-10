using Config_API.DTOs.Requests;
using Config_API.DTOs.ServiceResponse;

namespace Config_API.Repository.Interfaces
{
    public interface IBoardRepository
    {
        Task<ServiceResponse<List<Board>>> GetAllBoards(GetAllBoardsRequest request);
        Task<ServiceResponse<Board>> GetBoardById(int id);
        Task<ServiceResponse<string>> AddUpdateBoard(Board request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
        Task<ServiceResponse<List<Board>>> GetAllBoardsMaster();
    }
}
