﻿using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
namespace StudentApp_API.Repository.Interfaces
{
    public interface ICYOTRepository
    {
        Task<ServiceResponse<List<SubjectDTO>>> GetSubjectsAsync(int registrationId);
        Task<ServiceResponse<List<ChapterDTO>>> GetChaptersAsync(int registrationId, int subjectId);
        Task<ServiceResponse<int>> InsertOrUpdateCYOTAsync(CYOTDTO cyot);
        Task<ServiceResponse<CYOTDTO>> GetCYOTByIdAsync(int cyotId);
        Task<ServiceResponse<bool>> UpdateCYOTSyllabusAsync(int cyotId, List<CYOTSyllabusDTO> syllabusList);
        Task<ServiceResponse<List<QuestionResponseDTO>>> GetCYOTQuestions(GetCYOTQuestionsRequest request);
        Task<ServiceResponse<IEnumerable<AnswerPercentageResponse>>> SubmitCYOTAnswerAsync(List<SubmitAnswerRequest> request);
        Task<ServiceResponse<List<CYOTQuestionWithAnswersDTO>>> GetCYOTQuestionsWithOptionsAsync(int cyotId);
        Task<ServiceResponse<bool>> MakeCYOTOpenChallenge(int CYOTId);
        Task<ServiceResponse<List<CYOTResponse>>> GetCYOTListByStudent(CYOTListRequest request);
        Task<ServiceResponse<bool>> UpsertCYOTParticipantsAsync(List<CYOTParticipantRequest> requests);
    }
}
