using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;
using Course_API.Repository.Interfaces;
using Course_API.Services.Interfaces;

namespace Course_API.Services.Implementations
{
    public class ScholarshipTestServices: IScholarshipTestServices
    {
        private readonly IScholarshipTestRepository _scholarshipTestRepository;

        public ScholarshipTestServices(IScholarshipTestRepository scholarshipTestRepository)
        {
            _scholarshipTestRepository = scholarshipTestRepository;
        }

        public async Task<ServiceResponse<int>> AddUpdateScholarshipTest(ScholarshipTestRequestDTO request)
        {
            try
            {
                return await _scholarshipTestRepository.AddUpdateScholarshipTest(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<int>(false, ex.Message, 0, 500);
            }
        }

        public async Task<ServiceResponse<ScholarshipTestResponseDTO>> GetScholarshipTestById(int ScholarshipTestId)
        {

            try
            {
                return await _scholarshipTestRepository.GetScholarshipTestById(ScholarshipTestId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ScholarshipTestResponseDTO>(false, ex.Message,  new ScholarshipTestResponseDTO(), 500);
            }
        }

        public async Task<ServiceResponse<List<ScholarshipTestResponseDTO>>> GetScholarshipTestList(ScholarshipGetListRequest request)
        {
            try
            {
                return await _scholarshipTestRepository.GetScholarshipTestList(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ScholarshipTestResponseDTO>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<string>> ScholarshipContentIndexMapping(ContentIndexRequest request, int ScholarshipTestId)
        {
            try
            {
                // List to store the mapped content index records
                List<ScholarshipContentIndex> contentIndexList = new List<ScholarshipContentIndex>();

                // Loop through each subject in the request
                if (request.Subjects != null)
                {
                    foreach (var subject in request.Subjects)
                    {
                        if (subject.Chapter != null)
                        {
                            foreach (var chapter in subject.Chapter)
                            {
                                // Add the chapter as TestSeriesContentIndex
                                contentIndexList.Add(new ScholarshipContentIndex
                                {
                                    SSTContIndId = chapter.TestseriesContentIndexId,
                                    IndexTypeId = chapter.IndexTypeId,
                                    ContentIndexId = chapter.ContentIndexId,
                                    ScholarshipTestId = ScholarshipTestId,
                                    SubjectId = subject.SubjectId
                                });
                                if (chapter.Concepts != null)
                                {
                                    // Loop through each concept (topic) in the chapter
                                    foreach (var concept in chapter.Concepts)
                                    {
                                        // Add the concept as TestSeriesContentIndex
                                        contentIndexList.Add(new ScholarshipContentIndex
                                        {
                                            SSTContIndId = concept.TestseriesConceptIndexId,
                                            IndexTypeId = concept.IndexTypeId,
                                            ContentIndexId = concept.ContInIdTopic,
                                            ScholarshipTestId = ScholarshipTestId,
                                            SubjectId = subject.SubjectId
                                        });
                                        if (concept.SubConcepts != null)
                                        {
                                            // Loop through each sub-concept in the concept (topic)
                                            foreach (var subConcept in concept.SubConcepts)
                                            {
                                                // Add the sub-concept as TestSeriesContentIndex
                                                contentIndexList.Add(new ScholarshipContentIndex
                                                {
                                                    SSTContIndId = subConcept.TestseriesConceptIndexId,
                                                    IndexTypeId = subConcept.IndexTypeId,
                                                    ContentIndexId = subConcept.ContInIdSubTopic,
                                                    ScholarshipTestId = ScholarshipTestId,
                                                    SubjectId = subject.SubjectId
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return await _scholarshipTestRepository.ScholarshipContentIndexMapping(contentIndexList, ScholarshipTestId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<string>> ScholarshipDiscountSchemeMapping(List<ScholarshipTestDiscountScheme>? request, int ScholarshipTestId)
        {
            try
            {
                return await _scholarshipTestRepository.ScholarshipDiscountSchemeMapping(request, ScholarshipTestId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<string>> ScholarshipInstructionsMapping(ScholarshipTestInstructions? request, int ScholarshipTestId)
        {
            try
            {
                return await _scholarshipTestRepository.ScholarshipInstructionsMapping(request, ScholarshipTestId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<string>> ScholarshipQuestionSectionMapping(List<QuestionSectionScholarship> request, int ScholarshipTestId)
        {
            try
            {
                var requestBody = new List<ScholarshipQuestionSection>();

                // Loop through the QuestionSection list
                foreach (var data in request)
                {
                    // For each QuestionSection, map its TestSeriesQuestionSections to new entries
                    if (data.ScholarshipQuestionSections != null)
                    {
                        var mappedSections = data.ScholarshipQuestionSections.Select(m => new ScholarshipQuestionSection
                        {
                            SSTSectionId = m.SSTSectionId,
                            ScholarshipTestId = ScholarshipTestId, // Use the TestSeriesId passed to the method
                            DisplayOrder = m.DisplayOrder,
                            SectionName = m.SectionName,
                            Status = m.Status,
                            LevelId1 = m.LevelId1,
                            QuesPerDifficulty1 = m.QuesPerDifficulty1,
                            LevelId2 = m.LevelId2,
                            QuesPerDifficulty2 = m.QuesPerDifficulty2,
                            LevelId3 = m.LevelId3,
                            QuesPerDifficulty3 = m.QuesPerDifficulty3,
                            QuestionTypeId = m.QuestionTypeId,
                            MarksPerQuestion = m.MarksPerQuestion,
                            NegativeMarks = m.NegativeMarks,
                            TotalNumberOfQuestions = m.TotalNumberOfQuestions,
                            NoOfQuestionsPerChoice = m.NoOfQuestionsPerChoice,
                            SubjectId = data.SubjectId // Map the SubjectId from the parent QuestionSection
                        }).ToList();

                        // Add the mapped sections to the requestBody
                        requestBody.AddRange(mappedSections);
                    }
                }
                return await _scholarshipTestRepository.ScholarshipQuestionSectionMapping(requestBody, ScholarshipTestId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<string>> ScholarshipQuestionsMapping(List<ScholarshipTestQuestion> request, int ScholarshipTestId, int SSTSectionId)
        {
            try
            {
                return await _scholarshipTestRepository.ScholarshipQuestionsMapping(request, ScholarshipTestId, SSTSectionId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}
