﻿using Schools_API.DTOs.Requests;
using Schools_API.DTOs.Response;
using Schools_API.DTOs.ServiceResponse;

namespace Schools_API.Services.Interfaces
{
    public interface IQuestionServices
    {
        Task<ServiceResponse<QuestionResponseDTO>> GetQuestionByCode(string questionCode);
        Task<ServiceResponse<List<QuestionResponseDTO>>> GetAllQuestionsList(GetAllQuestionListRequest request);
        Task<ServiceResponse<List<QuestionResponseDTO>>> GetApprovedQuestionsList(GetAllQuestionListRequest request);
        Task<ServiceResponse<List<QuestionResponseDTO>>> GetRejectedQuestionsList(GetAllQuestionListRequest request);
        Task<ServiceResponse<string>> AddUpdateQuestion(QuestionDTO request);
        Task<ServiceResponse<List<QuestionComparisonDTO>>> CompareQuestionAsync(QuestionCompareRequest newQuestion);
        Task<ServiceResponse<string>> RejectQuestion(QuestionRejectionRequestDTO request);
        Task<ServiceResponse<string>> ApproveQuestion(QuestionApprovalRequestDTO request);
        Task<ServiceResponse<string>> AssignQuestionToProfiler(QuestionProfilerRequest request);
        Task<ServiceResponse<QuestionProfilerResponse>> GetQuestionProfilerDetails(string QuestionCode);
        Task<ServiceResponse<object>> CompareQuestionVersions(string questionCode);
        Task<ServiceResponse<List<QuestionResponseDTO>>> GetAssignedQuestionsList(int employeeId);
        Task<ServiceResponse<List<EmployeeListAssignedQuestionCount>>> GetAssignedQuestionsCount(int EmployeeId, int SubjectId);
        Task<ServiceResponse<List<QuestionResponseDTO>>> GetAllLiveQuestionsList(int SubjectId);
        Task<ServiceResponse<string>> MarkQuestionLive(string questionCode);
        Task<ServiceResponse<List<ContentIndexResponses>>> GetSyllabusDetailsBySubject(SyllabusDetailsRequest request);
        Task<ServiceResponse<byte[]>> GenerateExcelFile(DownExcelRequest request);
        Task<ServiceResponse<string>> UploadQuestionsFromExcel(IFormFile file, int EmployeeId);
        Task<ServiceResponse<string>> AddUpdateComprehensiveQuestion(ComprehensiveQuestionRequest request);
        Task<ServiceResponse<int>> UpdateQIDCourseAsync(int qidCourseId, List<UpdateQIDCourseRequest> requests);
        Task<ServiceResponse<string>> AddOrUpdateMatchThePairType2(MatchThePair2Request request);
        Task<ServiceResponse<string>> AddMatchThePairQuestion(MatchThePairRequest request);
        Task<ServiceResponse<bool>> DeleteQuestion(int QuestionId, string QuestionCode);
    }
}
