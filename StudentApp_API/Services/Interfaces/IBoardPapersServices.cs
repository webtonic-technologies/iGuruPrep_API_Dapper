﻿using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
namespace StudentApp_API.Services.Interfaces
{
    public interface IBoardPapersServices
    {
        Task<ServiceResponse<List<TestSeriesSubjectsResponse>>> GetAllTestSeriesSubjects(int RegistrationId);
        Task<ServiceResponse<List<TestSeriesResponse>>> GetTestSeriesBySubjectId(GetTestseriesSubjects request);
        Task<ServiceResponse<TestSeriesQuestionsListResponse>> GetTestSeriesDescriptiveQuestions(TestSeriesQuestionRequest request);
        Task<ServiceResponse<string>> MarkQuestionAsSave(SaveQuestionRequest request);
        Task<ServiceResponse<string>> MarkQuestionAsRead(SaveQuestionRequest request);
        Task<ServiceResponse<List<QuestionTypeResponse>>> GetQuestionTypesByTestSeriesIdAsync(int testSeriesId);
        Task<ServiceResponse<Dictionary<string, object>>> GetTestSeriesPercentageBySubject(int RegistrationId);
        Task<ServiceResponse<string>> ShareQuestionAsync(int studentId, int questionId, int TestSeriesId);
    }
}
