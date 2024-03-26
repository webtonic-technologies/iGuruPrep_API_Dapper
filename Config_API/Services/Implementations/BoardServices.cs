using Config_API.DTOs.ServiceResponse;
using Config_API.Repository.Interfaces;
using Config_API.Services.Interfaces;

namespace Config_API.Services.Implementations
{
    public class BoardServices : IBoardServices
    {
        private readonly IBoardRepository _boardRepository;

        public BoardServices(IBoardRepository boardRepository)
        {
            _boardRepository = boardRepository;
        }
        public async Task<ServiceResponse<string>> AddUpdateBoard(Board request)
        {
            try
            {
               return await _boardRepository.AddUpdateBoard(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<List<Board>>> GetAllBoards()
        {

            try
            {
                return await _boardRepository.GetAllBoards();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Board>>(false, ex.Message, new List<Board>(), 500);
            }
        }

        public async Task<ServiceResponse<Board>> GetBoardById(int id)
        {
            try
            {
                return await _boardRepository.GetBoardById(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Board>(false, ex.Message, new Board(), 500);
            }
        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                return await _boardRepository.StatusActiveInactive(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }
    }
}
