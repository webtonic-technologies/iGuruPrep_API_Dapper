using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;

namespace Course_API.Repository.Interfaces
{
    public interface IBookRepository
    {
        Task<ServiceResponse<BookResponseDTO>> Get(int id);
        Task<ServiceResponse<List<BookResponseDTO>>> GetAllBooks(BookListDTO request);
        Task<ServiceResponse<string>> AddUpdate(BookDTO request);
        Task<ServiceResponse<bool>> Delete(int id);
    }
}
