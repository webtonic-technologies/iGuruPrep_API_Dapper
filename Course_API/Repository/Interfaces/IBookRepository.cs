using Course_API.DTOs;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;

namespace Course_API.Repository.Interfaces
{
    public interface IBookRepository
    {
        Task<ServiceResponse<BookDTO>> Get(int id);
        Task<ServiceResponse<List<BookDTO>>> GetAllBooks(BookListDTO request);
        Task<ServiceResponse<string>> Add(BookDTO request);
        Task<ServiceResponse<string>> Update(BookDTO request);
        Task<ServiceResponse<bool>> Delete(int id);
    }
}
