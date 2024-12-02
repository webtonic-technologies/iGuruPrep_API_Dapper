using Config_API.DTOs.Requests;
using Config_API.DTOs.Response;
using Config_API.DTOs.ServiceResponse;

namespace Config_API.Services.Interfaces
{
    public interface IPartialMarksRuleServices
    {
        Task<ServiceResponse<PartialMarksResponse>> GetPartialMarksRuleyId(int RuleId);
        Task<ServiceResponse<byte[]>> AddPartialMarksRule(PartialMarksRequest request);
        Task<ServiceResponse<List<PartialMarksResponse>>> GetAllPartialMarksRules();
        Task<ServiceResponse<string>> UploadPartialMarksSheet(IFormFile file, int RuleId);
        Task<byte[]> DownloadPartialMarksExcelSheet(int RuleId);
        Task<ServiceResponse<List<PartialMarksResponse>>> GetAllPartialMarksRulesList(GetListRequest request);
    }
}
