using Course_API.DTOs;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;
using Course_API.Repository.Interfaces;
using Course_API.Services.Interfaces;

namespace Course_API.Services.Implementations
{
    public class BookServices : IBookServices
    {

        private readonly IBookRepository _bookRepository;

        public BookServices(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }
        public async Task<ServiceResponse<string>> Add(BookDTO request)
        {
            try
            {
                return await _bookRepository.Add(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<bool>> Delete(int id)
        {
            try
            {
                return await _bookRepository.Delete(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }

        public async Task<ServiceResponse<BookDTO>> Get(int id)
        {
            try
            {
                return await _bookRepository.Get(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<BookDTO>(false, ex.Message, new BookDTO(), 500);
            }
        }

        public async Task<ServiceResponse<List<BookDTO>>> GetAll(BookListDTO request)
        {
            try
            {
                return await _bookRepository.GetAllBooks(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<BookDTO>>(false, ex.Message, new List<BookDTO>(), 500);
            }
        }

        public async Task<ServiceResponse<string>> Update(BookDTO request)
        {
            try
            {
                return await _bookRepository.Update(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}
