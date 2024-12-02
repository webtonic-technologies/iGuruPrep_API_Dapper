using Config_API.DTOs.Requests;
using Config_API.DTOs.Response;
using Config_API.DTOs.ServiceResponse;
using Config_API.Repository.Interfaces;
using Config_API.Services.Interfaces;

namespace Config_API.Services.Implementations
{
    public class PartialMarksRuleServices : IPartialMarksRuleServices
    {
        private readonly IPartialMarksRuleRepository _partialMarksRuleRepository;

        public PartialMarksRuleServices(IPartialMarksRuleRepository partialMarksRuleRepository)
        {
            _partialMarksRuleRepository = partialMarksRuleRepository;
        }
        public async Task<ServiceResponse<byte[]>> AddPartialMarksRule(PartialMarksRequest request)
        {
            return await _partialMarksRuleRepository.AddPartialMarksRule(request);
        }

        public async Task<byte[]> DownloadPartialMarksExcelSheet(int RuleId)
        {
            return await _partialMarksRuleRepository.DownloadPartialMarksExcelSheet(RuleId);
        }

        public async Task<ServiceResponse<List<PartialMarksResponse>>> GetAllPartialMarksRules()
        {
            return await _partialMarksRuleRepository.GetAllPartialMarksRules();
        }

        public async Task<ServiceResponse<List<PartialMarksResponse>>> GetAllPartialMarksRulesList(GetListRequest request)
        {
            return await _partialMarksRuleRepository.GetAllPartialMarksRulesList(request);
        }

        public async Task<ServiceResponse<PartialMarksResponse>> GetPartialMarksRuleyId(int RuleId)
        {
            return await _partialMarksRuleRepository.GetPartialMarksRuleyId(RuleId);
        }

        public async Task<ServiceResponse<string>> UploadPartialMarksSheet(IFormFile file, int RuleId)
        {
            return await _partialMarksRuleRepository.UploadPartialMarksSheet(file, RuleId);
        }
    }
}
