using Course_API.DTOs;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;

namespace Course_API.Services.Interfaces
{
    public interface IBookServices
    {
        Task<ServiceResponse<Book>> Get(int id);
        Task<ServiceResponse<IEnumerable<Book>>> GetAll();
        Task<ServiceResponse<string>> Add(BookDTO request);
        Task<ServiceResponse<string>> Update(BookDTO request);
        Task<ServiceResponse<bool>> Delete(int id);
    }
}
