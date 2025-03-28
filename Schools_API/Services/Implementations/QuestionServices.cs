﻿using Schools_API.DTOs.Requests;
using Schools_API.DTOs.Response;
using Schools_API.DTOs.ServiceResponse;
using Schools_API.Repository.Interfaces;
using Schools_API.Services.Interfaces;

namespace Schools_API.Services.Implementations
{
    public class QuestionServices : IQuestionServices
    {
        private readonly IQuestionRepository _questionRepository;

        public QuestionServices(IQuestionRepository questionRepository)
        {
            _questionRepository = questionRepository;
        }

        public async Task<ServiceResponse<string>> AddMatchThePairQuestion(MatchThePairRequest request)
        {
            return await _questionRepository.AddMatchThePairQuestion(request);
        }

        public async Task<ServiceResponse<string>> AddOrUpdateMatchThePairType2(MatchThePair2Request request)
        {
            return await _questionRepository.AddOrUpdateMatchThePairType2(request);
        }

        public async Task<ServiceResponse<string>> AddUpdateComprehensiveQuestion(ComprehensiveQuestionRequest request)
        {
            return await _questionRepository.AddUpdateComprehensiveQuestion(request);
        }
        public async Task<ServiceResponse<string>> AddUpdateQuestion(QuestionDTO request)
        {
            try
            {
                return await _questionRepository.AddUpdateQuestion(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<string>> ApproveQuestion(QuestionApprovalRequestDTO request)
        {
            try
            {
                return await _questionRepository.ApproveQuestion(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<string>> AssignQuestionToProfiler(QuestionProfilerRequest request)
        {
            try
            {
                return await _questionRepository.AssignQuestionToProfiler(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionComparisonDTO>>> CompareQuestionAsync(QuestionCompareRequest newQuestion)
        {
            try
            {
                return await _questionRepository.CompareQuestionAsync(newQuestion);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionComparisonDTO>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<object>> CompareQuestionVersions(string questionCode)
        {
            try
            {
                return await _questionRepository.CompareQuestionVersions(questionCode);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<object>(false, ex.Message, null, 500);
            }
        }

        public async Task<ServiceResponse<bool>> DeleteQuestion(int QuestionId, string QuestionCode)
        {
            return await _questionRepository.DeleteQuestion(QuestionId, QuestionCode);

        }
        public async Task<ServiceResponse<byte[]>> GenerateExcelFile(DownExcelRequest request)
        {
            try
            {
                return await _questionRepository.GenerateExcelFile(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<byte[]>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetAllLiveQuestionsList(int SubjectId)
        {
            try
            {
                return await _questionRepository.GetAllLiveQuestionsList(SubjectId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetAllQuestionsList(GetAllQuestionListRequest request)
        {
            try
            {
                return await _questionRepository.GetAllQuestionsList(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetApprovedQuestionsList(GetAllQuestionListRequest request)
        {
            try
            {
                return await _questionRepository.GetApprovedQuestionsList(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<List<EmployeeListAssignedQuestionCount>>> GetAssignedQuestionsCount(int EmployeeId, int SubjectId)
        {
            try
            {
                return await _questionRepository.GetAssignedQuestionsCount(EmployeeId, SubjectId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<EmployeeListAssignedQuestionCount>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetAssignedQuestionsList(int employeeId)
        {
            try
            {
                return await _questionRepository.GetAssignedQuestionsList(employeeId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<QuestionResponseDTO>> GetQuestionByCode(string questionCode)
        {
            try
            {
                return await _questionRepository.GetQuestionByCode(questionCode);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<QuestionResponseDTO>(false, ex.Message, new QuestionResponseDTO(), 500);
            }
        }
        public async Task<ServiceResponse<QuestionProfilerResponse>> GetQuestionProfilerDetails(string QuestionCode)
        {
            try
            {
                return await _questionRepository.GetQuestionProfilerDetails(QuestionCode);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<QuestionProfilerResponse>(false, ex.Message, new QuestionProfilerResponse(), 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetRejectedQuestionsList(GetAllQuestionListRequest request)
        {
            try
            {
                return await _questionRepository.GetRejectedQuestionsList(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<List<ContentIndexResponses>>> GetSyllabusDetailsBySubject(SyllabusDetailsRequest request)
        {
            try
            {
                return await _questionRepository.GetSyllabusDetailsBySubject(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ContentIndexResponses>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<string>> MarkQuestionLive(string questionCode)
        {
            try
            {
                return await _questionRepository.MarkQuestionLive(questionCode);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<string>> RejectQuestion(QuestionRejectionRequestDTO request)
        {
            try
            {
                return await _questionRepository.RejectQuestion(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<int>> UpdateQIDCourseAsync(int qidCourseId, List<UpdateQIDCourseRequest> request)
        {
            return await _questionRepository.UpdateQIDCourseAsync(qidCourseId, request);
        }

        public async Task<ServiceResponse<string>> UploadQuestionsFromExcel(IFormFile file, int EmployeeId)
        {
            try
            {
                return await _questionRepository.UploadQuestionsFromExcel(file, EmployeeId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}
