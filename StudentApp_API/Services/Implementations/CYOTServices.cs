﻿using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Models;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Services.Implementations
{
    public class CYOTServices : ICYOTServices
    {
        private readonly ICYOTRepository _cYOTRepository;

        public CYOTServices(ICYOTRepository cYOTRepository)
        {
            _cYOTRepository = cYOTRepository;
        }
        public async Task<ServiceResponse<List<ChapterDTO>>> GetChaptersAsync(int registrationId, int subjectId)
        {
            return await _cYOTRepository.GetChaptersAsync(registrationId, subjectId);
        }

        public async Task<ServiceResponse<CYOTDTO>> GetCYOTByIdAsync(int cyotId)
        {
            return await _cYOTRepository.GetCYOTByIdAsync(cyotId);
        }

        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetCYOTQuestions(int cyotId, int registrationId)
        {
            return await _cYOTRepository.GetCYOTQuestions(cyotId, registrationId);
        }

        public async Task<ServiceResponse<List<CYOTQuestionWithAnswersDTO>>> GetCYOTQuestionsWithOptionsAsync(int cyotId)
        {
            return await _cYOTRepository.GetCYOTQuestionsWithOptionsAsync(cyotId);
        }

        public async Task<ServiceResponse<List<SubjectDTO>>> GetSubjectsAsync(int registrationId)
        {
            return await _cYOTRepository.GetSubjectsAsync(registrationId);
        }

        public async Task<ServiceResponse<int>> InsertOrUpdateCYOTAsync(CYOTDTO cyot)
        {
            return await _cYOTRepository.InsertOrUpdateCYOTAsync(cyot);
        }

        public async Task<ServiceResponse<IEnumerable<AnswerPercentageResponse>>> SubmitCYOTAnswerAsync(SubmitAnswerRequest request)
        {
            return await _cYOTRepository.SubmitCYOTAnswerAsync(request);
        }

        public async Task<ServiceResponse<bool>> UpdateCYOTSyllabusAsync(int cyotId, List<CYOTSyllabusDTO> syllabusList)
        {
            return await _cYOTRepository.UpdateCYOTSyllabusAsync(cyotId, syllabusList);
        }
    }
}