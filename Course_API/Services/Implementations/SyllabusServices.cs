using Course_API.DTOs;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;
using Course_API.Repository.Interfaces;
using Course_API.Services.Interfaces;

namespace Course_API.Services.Implementations
{
    public class SyllabusServices : ISyllabusServices
    {
        private readonly ISyllabusRepository _syllabusRepository;

        public SyllabusServices(ISyllabusRepository syllabusRepository)
        {
            _syllabusRepository = syllabusRepository;
        }
        public async Task<ServiceResponse<string>> AddUpdateSyllabus(SyllabusDTO request)
        {
            try
            {
                return await _syllabusRepository.AddUpdateSyllabus(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<string>> AddUpdateSyllabusDetails(SyllabusDetailsDTO request)
        {

            try
            {
                return await _syllabusRepository.AddUpdateSyllabusDetails(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}
