using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
namespace StudentApp_API.Services.Interfaces
{
    public interface IConceptwisePracticeServices
    {
        Task<ServiceResponse<ConceptwisePracticeResponse>> GetSyllabusSubjects(int RegistrationId);
        Task<ServiceResponse<List<ConceptwisePracticeContentResponse>>> GetSyllabusContentDetails(SyllabusDetailsRequest request);
        Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuestionsAsync(GetQuestionsList request);
        Task<ServiceResponse<string>> MarkQuestionAsSave(SaveQuestionConceptwisePracticeRequest request);
        Task<ServiceResponse<ConceptwiseAnswerResponse>> SubmitAnswerAsync(ConceptwisePracticeSubmitAnswerRequest request);
        Task<ServiceResponse<decimal>> GetStudentQuestionAccuracyAsync(int studentId, int questionId);
        Task<ServiceResponse<decimal>> GetStudentGroupAccuracyForQuestionAsync(int studentId, int questionId);
        Task<ServiceResponse<double>> GetAverageTimeSpentByOtherStudents(int studentId, int questionId);
        Task<ServiceResponse<QuestionAttemptStatsResponse>> GetQuestionAttemptStatsForGroupAsync(int studentId, int questionId);
        Task<ServiceResponse<double>> GetAverageTimeSpentOnQuestion(int studentId, int questionId);
        Task<ServiceResponse<AnswerTimeStatsDto>> GetAnswerTimeStats(int studentId, int indexTypeId, int contentId, int syllabusId);
        Task<ServiceResponse<TimeSpentDto>> GetTotalAndAverageTimeSpent(int studentId, int indexTypeId, int contentId, int syllabusId);
        Task<ServiceResponse<AccuracyRateDto>> GetStudentAndClassmatesAccuracyRate(int studentId, int indexTypeId, int contentId, int syllabusId);
        Task<ServiceResponse<PracticeStatsDto>> GetPracticeQuestionStats(int studentId, int indexTypeId, int contentId, int syllabusId);
        Task<ServiceResponse<AccuracyRateDtoComparison>> GetAccuracyRates(int studentId, int indexTypeId, int contentId, int syllabusId);
        Task<ServiceResponse<List<StudentAccuracyDto>>> GetClassmatesAccuracyRate(int studentId, int indexTypeId, int contentId, int syllabusId);
    }
}
