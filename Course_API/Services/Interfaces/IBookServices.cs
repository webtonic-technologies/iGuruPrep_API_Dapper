using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;

namespace Course_API.Services.Interfaces
{
    public interface IBookServices
    {
        Task<ServiceResponse<BookResponseDTO>> Get(int id);
        Task<ServiceResponse<List<BookResponseDTO>>> GetAll(BookListDTO request);
        Task<ServiceResponse<string>> Add(BookDTO request);
        Task<ServiceResponse<string>> Update(BookDTO request);
        Task<ServiceResponse<bool>> Delete(int id);
    }
}
