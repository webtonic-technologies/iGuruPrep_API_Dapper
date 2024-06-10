using Config_API.DTOs.Requests;
using Config_API.DTOs.ServiceResponse;
using Config_API.Repository.Interfaces;
using Config_API.Services.Interfaces;
using iGuruPrep.Models;

namespace Config_API.Services.Implementations
{
    public class SubjectServices : ISubjectServices
    {
        private readonly ISubjectRepository  _subjectRepository;

        public SubjectServices(ISubjectRepository subjectRepository)
        {
            _subjectRepository = subjectRepository;
        }
        public async Task<ServiceResponse<string>> AddUpdateSubject(Subject request)
        {
            try
            {
                return await _subjectRepository.AddUpdateSubject(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<List<Subject>>> GetAllSubjects(GetAllSubjectsRequest request)
        {
            try
            {
                return await _subjectRepository.GetAllSubjects(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Subject>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<List<Subject>>> GetAllSubjectsMAsters()
        {
            try
            {
                return await _subjectRepository.GetAllSubjectsMAsters();
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Subject>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<Subject>> GetSubjectById(int id)
        {

            try
            {
                return await _subjectRepository.GetSubjectById(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Subject>(false, ex.Message, new Subject(), 500);
            }
        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                return await _subjectRepository.StatusActiveInactive(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }
    }
}
