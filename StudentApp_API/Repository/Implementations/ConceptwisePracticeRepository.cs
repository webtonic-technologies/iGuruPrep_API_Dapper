using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using System.Data;
namespace StudentApp_API.Repository.Implementations
{
    public class ConceptwisePracticeRepository: IConceptwisePracticeRepository
    {
        private readonly IDbConnection _connection;
        public ConceptwisePracticeRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<ConceptwisePracticeResponse>> GetSyllabusSubjects(int RegistrationId)
        {
            try
            {
                var data = await _connection.QueryFirstOrDefaultAsync(@"SELECT * FROM tblStudentClassCourseMapping 
                                                                 WHERE RegistrationID = @RegistrationID",
                                                                         new { RegistrationID = RegistrationId });

                if (data == null)
                {
                    return new ServiceResponse<ConceptwisePracticeResponse>(false, "No mapping found for this registration ID.", null, 404);
                }

                // SQL to get Syllabus subjects
                string query = @"
            SELECT 
                ss.SyllabusID AS SyllabusId,
                ss.SubjectID AS SubjectId,
                s.SubjectName AS SubjectName 
            FROM tblSyllabusSubjects ss
            INNER JOIN tblSyllabus sy ON ss.SyllabusID = sy.SyllabusId
            INNER JOIN tblSubject s ON ss.SubjectID = s.SubjectID
            WHERE 
                sy.BoardID = @BoardId AND 
                sy.ClassId = @ClassId AND 
                sy.CourseId = @CourseId AND
                sy.Status = 1";

                var resposne = await _connection.QueryAsync<dynamic>(
                    query, new { BoardId = data.BoardId, ClassId = data.ClassID, CourseId = data.CourseID });

                var subjects = new List<ConceptwisePracticeSubjectsResposne>();

                foreach (var m in resposne)
                {
                    // Get chapter count for each subject + syllabus combo
                    var chapterCount = await _connection.ExecuteScalarAsync<int>(@"
                SELECT COUNT(*) 
                FROM tblSyllabusDetails 
                WHERE SyllabusID = @SyllabusId 
                  AND SubjectId = @SubjectId 
                  AND IndexTypeId = 1",
                        new { SyllabusId = m.SyllabusId, SubjectId = m.SubjectId });

                    subjects.Add(new ConceptwisePracticeSubjectsResposne
                    {
                        SubjectId = m.SubjectId,
                        SubjectName = m.SubjectName,
                        SyllabusId = m.SyllabusId,
                        RegistrationId = RegistrationId,
                        ChapterCount = chapterCount,
                        Percentage = PercentageCalculation(0, 0, RegistrationId, m.SubjectId, m.SyllabusId)
                    });
                }

                // Calculate average percentage
                decimal averagePercentage = subjects.Any()
                    ? subjects.Average(s => s.Percentage)
                    : 0;

                var response = new ConceptwisePracticeResponse
                {
                    conceptwisePracticeSubjectsResposnes = subjects,
                    Percentage = Math.Round(averagePercentage, 2)
                };

                if (subjects != null && subjects.Count > 0)
                {
                    return new ServiceResponse<ConceptwisePracticeResponse>(true, "Subjects retrieved successfully.", response, 200);
                }
                else
                {
                    return new ServiceResponse<ConceptwisePracticeResponse>(false, "No subjects found for the given criteria.", null, 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ConceptwisePracticeResponse>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<List<ConceptwisePracticeContentResponse>>> GetSyllabusContentDetails(SyllabusDetailsRequest request)
        {
            List<ConceptwisePracticeContentResponse> contentResponse = new List<ConceptwisePracticeContentResponse>();

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            try
            {
                // Fetch chapters and count of child topics mapped to the given syllabus
                if (request.SyllabusId.HasValue && request.SubjectId.HasValue && request.IndexTypeId == 0 && request.ContentIndexId == 0)
                {
                    string queryChaptersWithTopicCount = @"
            SELECT 
                c.ContentIndexId AS ContentId, 
                c.SubjectId, 
                c.ContentName_Chapter AS ContentName, 
                s.SyllabusID, 
                c.IndexTypeId,
                (
                    SELECT COUNT(*) 
                    FROM tblContentIndexTopics t
                    INNER JOIN tblSyllabusDetails sd ON t.ContentIndexId = sd.ContentIndexId
                    WHERE 
                        t.ContentIndexId = c.ContentIndexId AND 
                        sd.SyllabusID = s.SyllabusID AND 
                        t.IndexTypeId = 2 AND 
                        t.IsActive = 1
                ) AS ConceptOrSubConceptCount,
                (
                    SELECT ISNULL(MAX(q.SetID) - 1, 0)
                    FROM tblConceptwisePracticeQuestions q
                    WHERE 
                        q.ContentID = c.ContentIndexId AND
                        q.IndexTypeID = c.IndexTypeId AND
                        q.SyllabusID = s.SyllabusID AND
                        q.SubjectId = c.SubjectId AND
                        q.StudentId = @RegistrationId
                ) AS AttemptCount
            FROM tblContentIndexChapters c
            LEFT JOIN tblSyllabusDetails s ON c.ContentIndexId = s.ContentIndexId
            WHERE 
                s.SyllabusID = @SyllabusId AND 
                c.SubjectId = @SubjectId AND 
                c.IndexTypeId = 1 AND 
                c.IsActive = 1";

                    contentResponse = (await _connection.QueryAsync<ConceptwisePracticeContentResponse>(queryChaptersWithTopicCount, new
                    {
                        SyllabusId = request.SyllabusId,
                        SubjectId = request.SubjectId,
                        RegistrationId = request.RegistrationId
                    })).ToList();
                }
                // Fetch topics (children of chapters)
                else if (request.IndexTypeId == 1 && request.ContentIndexId.HasValue)
                {
                    string queryTopics = @"
            SELECT 
                t.ContInIdTopic AS ContentId, 
                t.ContentName_Topic AS ContentName, 
                s.SyllabusID, 
                t.IndexTypeId,
                (
                    SELECT COUNT(*) 
                    FROM tblContentIndexSubTopics st
                    INNER JOIN tblSyllabusDetails sd ON st.ContInIdTopic = sd.ContentIndexId
                    WHERE 
                        st.ContInIdTopic = t.ContInIdTopic AND 
                        sd.SyllabusID = s.SyllabusID AND 
                        st.IndexTypeId = 3 AND 
                        st.IsActive = 1
                ) AS ConceptOrSubConceptCount,
                (
                    SELECT ISNULL(MAX(q.SetID) - 1, 0)
                    FROM tblConceptwisePracticeQuestions q
                    WHERE 
                        q.ContentID = t.ContInIdTopic AND
                        q.IndexTypeID = t.IndexTypeId AND
                        q.SyllabusID = s.SyllabusID AND
                        q.StudentId = @RegistrationId
                ) AS AttemptCount
            FROM tblContentIndexTopics t
            LEFT JOIN tblSyllabusDetails s ON t.ContentIndexId = s.ContentIndexId
            WHERE 
                t.ContentIndexId = @ContentIndexId AND 
                t.IndexTypeId = 2 AND 
                t.IsActive = 1";

                    contentResponse = (await _connection.QueryAsync<ConceptwisePracticeContentResponse>(queryTopics, new
                    {
                        ContentIndexId = request.ContentIndexId,
                        RegistrationId = request.RegistrationId
                    })).ToList();
                }
                // Fetch subtopics (children of topics)
                else if (request.IndexTypeId == 2 && request.ContentIndexId.HasValue)
                {
                    string querySubTopics = @"
            SELECT 
                s.ContInIdSubTopic AS ContentId, 
                s.ContentName_SubTopic AS ContentName, 
                d.SyllabusID, 
                s.IndexTypeId,
                0 AS ConceptOrSubConceptCount,
                (
                    SELECT ISNULL(MAX(q.SetID) - 1, 0)
                    FROM tblConceptwisePracticeQuestions q
                    WHERE 
                        q.ContentID = s.ContInIdSubTopic AND
                        q.IndexTypeID = s.IndexTypeId AND
                        q.SyllabusID = d.SyllabusID AND
                        q.StudentId = @RegistrationId
                ) AS AttemptCount
            FROM tblContentIndexSubTopics s
            LEFT JOIN tblSyllabusDetails d ON s.ContInIdTopic = d.ContentIndexId
            WHERE 
                s.ContInIdTopic = @ContentIndexId AND 
                s.IndexTypeId = 3 AND 
                s.IsActive = 1";

                    contentResponse = (await _connection.QueryAsync<ConceptwisePracticeContentResponse>(querySubTopics, new
                    {
                        ContentIndexId = request.ContentIndexId,
                        RegistrationId = request.RegistrationId
                    })).ToList();

                }

                // Add registration ID and calculate percentage
                foreach (var data in contentResponse)
                {
                    data.RegistrationId = request.RegistrationId;
                    data.Percentage = PercentageCalculation(data.IndexTypeId, data.ContentId, request.RegistrationId, request.SubjectId, request.SyllabusId);

                    // Check if any valid question was attempted (excluding Not Visited)
                    string questionExistQuery = @"
        SELECT COUNT(1)
        FROM tblConceptwisePracticeQuestions
        WHERE 
            ContentID = @ContentID AND
            IndexTypeID = @IndexTypeID AND
            SyllabusID = @SyllabusID AND
            SubjectId = @SubjectId AND
            StudentId = @RegistrationId AND
            QuestionStatusId != 4";

                    int questionCount = await _connection.ExecuteScalarAsync<int>(questionExistQuery, new
                    {
                        ContentID = data.ContentId,
                        IndexTypeID = data.IndexTypeId,
                        SyllabusID = request.SyllabusId,
                        SubjectId = request.SubjectId,
                        RegistrationId = request.RegistrationId
                    });

                    data.IsAnalytics = questionCount > 0;
                    data.IsQuestionAnalytics = questionCount > 0;

                    // Enable synopsis button only if synopsis is present
                    data.IsSynopsis = !string.IsNullOrWhiteSpace(data.Synopsis);
                }

                return new ServiceResponse<List<ConceptwisePracticeContentResponse>>(true, "Success", contentResponse, 200, contentResponse.Count);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ConceptwisePracticeContentResponse>>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<QuestionsSetResponse>> GetQuestionsAsync(GetQuestionsList request)
        {
            string difficultyLevelQuery = @"SELECT * FROM tbldifficultylevel WHERE Status = 1";

            var difficultyLevels = await _connection.QueryAsync<DifficultyLevelDTO>(difficultyLevelQuery);
            var result = new List<QuestionResponseDTO>();
            var questionsToReturn = new List<QuestionResponseDTO>();

            // Step 1: Check if any question mappings exist for the student for the given combination
            string checkMappingQuery = @"
        SELECT COUNT(1) FROM tblConceptwisePracticeQuestions
        WHERE StudentId = @StudentId AND ContentID = @ContentId
        AND IndexTypeID = @IndexTypeId AND SyllabusID = @SyllabusId";

            bool mappingExists = await _connection.ExecuteScalarAsync<bool>(checkMappingQuery, new
            {
                StudentId = request.StudentId,
                ContentId = request.contentId,
                IndexTypeId = request.indexTypeId,
                SyllabusId = request.SyllabusId
            });

            int setId = 1;

            if (!mappingExists)
            {
                // Step 2: First time mapping - insert questions for SetID = 1
                foreach (var level in difficultyLevels)
                {
                    string fetchQuestionsQuery = @"
                SELECT TOP (@Count)
                    q.QuestionId,
                    q.QuestionCode,
                    q.QuestionDescription,
                    q.QuestionFormula,
                    q.QuestionImage,
                    qc.LevelId,
                    dl.LevelName as LevelName,
                    q.QuestionTypeId,
                    q.IndexTypeId,
                    q.Status,
                    q.Explanation,
                    q.IsActive,
                    q.IsLive,
                    q.ExtraInformation,
                    q.IsConfigure,
                    q.CategoryId,
                    qt.QuestionType AS QuestionType
                FROM tblQuestion q
                INNER JOIN tblQIDCourse qc ON q.QuestionId = qc.QID
                LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
                LEFT JOIN tbldifficultylevel dl ON qc.LevelId = dl.LevelId
                WHERE q.SubjectId = @SubjectId
                AND q.IndexTypeId = @IndexTypeId
                AND q.ContentIndexId = @ContentIndexId
                AND qc.LevelId = @LevelId AND qc.CourseID = @CourseID
                AND q.IsActive = 1 AND q.IsLive = 1 AND q.IsRejected = 0
                ORDER BY NEWID()";

                    var questions = (await _connection.QueryAsync<QuestionResponseDTO>(fetchQuestionsQuery, new
                    {
                        SubjectId = request.subjectId,
                        IndexTypeId = request.indexTypeId,
                        ContentIndexId = request.contentId,
                        LevelId = level.LevelId,
                        CourseID = request.CourseId,
                        Count = level.NoofQperLevel
                    })).ToList();

                    foreach (var question in questions)
                    {
                        string insertQuestionQuery = @"
                    INSERT INTO tblConceptwisePracticeQuestions
                    (ContentID, IndexTypeID, QuestionID, SyllabusID, SetID, StudentId, LevelId, QuestionStatusId, Iscorrect, SubjectId)
                    VALUES (@ContentID, @IndexTypeID, @QuestionID, @SyllabusID, @SetID, @StudentId, @LevelId, 4, 0, @SubjectId)";

                        await _connection.ExecuteAsync(insertQuestionQuery, new
                        {
                            ContentID = request.contentId,
                            IndexTypeID = request.indexTypeId,
                            QuestionID = question.QuestionId,
                            SyllabusID = request.SyllabusId,
                            SetID = setId,
                            StudentId = request.StudentId,
                            LevelId = level.LevelId,
                            SubjectId = request.subjectId
                        });

                        questionsToReturn.Add(question);
                    }
                }
            }
            else
            {
                // Step 3: Questions already exist
                string existingSetQuery = @"
            SELECT q.*, cpq.*
            FROM tblConceptwisePracticeQuestions cpq
            INNER JOIN tblQuestion q ON q.QuestionId = cpq.QuestionID
            WHERE cpq.StudentId = @StudentId
            AND cpq.ContentID = @ContentId
            AND cpq.IndexTypeID = @IndexTypeId
            AND cpq.SyllabusID = @SyllabusId
            AND cpq.SetID = 1
            AND (cpq.QuestionStatusId = 4 OR (cpq.Iscorrect = 0))";

                questionsToReturn = (await _connection.QueryAsync<QuestionResponseDTO>(existingSetQuery, new
                {
                    StudentId = request.StudentId,
                    ContentId = request.contentId,
                    IndexTypeId = request.indexTypeId,
                    SyllabusId = request.SyllabusId
                })).ToList();

                if (!questionsToReturn.Any())
                {
                    // Step 4: All answered correctly in Set 1 → Create new Set
                    string maxSetQuery = @"
                SELECT ISNULL(MAX(SetID), 1) + 1
                FROM tblConceptwisePracticeQuestions
                WHERE StudentId = @StudentId
                AND ContentID = @ContentId
                AND IndexTypeID = @IndexTypeId
                AND SyllabusID = @SyllabusId";

                    setId = await _connection.ExecuteScalarAsync<int>(maxSetQuery, new
                    {
                        StudentId = request.StudentId,
                        ContentId = request.contentId,
                        IndexTypeId = request.indexTypeId,
                        SyllabusId = request.SyllabusId
                    });

                    foreach (var level in difficultyLevels)
                    {
                        string fetchQuestionsQuery = @"
                    SELECT TOP (@Count)
                        q.QuestionId,
                        q.QuestionCode,
                        q.QuestionDescription,
                        q.QuestionFormula,
                        q.QuestionImage,
                        qc.LevelId,
                        dl.LevelName as LevelName,
                        q.QuestionTypeId,
                        q.IndexTypeId,
                        q.Status,
                        q.Explanation,
                        q.IsActive,
                        q.IsLive,
                        q.ExtraInformation,
                        q.IsConfigure,
                        q.CategoryId,
                        qt.QuestionType AS QuestionType
                    FROM tblQuestion q
                    INNER JOIN tblQIDCourse qc ON q.QuestionId = qc.QID
                    LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
                    LEFT JOIN tbldifficultylevel dl ON qc.LevelId = dl.LevelId
                    WHERE q.SubjectId = @SubjectId
                    AND q.IndexTypeId = @IndexTypeId
                    AND q.ContentIndexId = @ContentIndexId
                    AND qc.LevelId = @LevelId AND qc.CourseID = @CourseID
                    AND q.IsActive = 1 AND q.IsLive = 1 AND q.IsRejected = 0
                    ORDER BY NEWID()";

                        var questions = (await _connection.QueryAsync<QuestionResponseDTO>(fetchQuestionsQuery, new
                        {
                            SubjectId = request.subjectId,
                            IndexTypeId = request.indexTypeId,
                            ContentIndexId = request.contentId,
                            LevelId = level.LevelId,
                            CourseID = request.CourseId,
                            Count = level.NoofQperLevel
                        })).ToList();

                        foreach (var question in questions)
                        {
                            string insertQuestionQuery = @"
                        INSERT INTO tblConceptwisePracticeQuestions
                        (ContentID, IndexTypeID, QuestionID, SyllabusID, SetID, StudentId, LevelId, QuestionStatusId, Iscorrect, SubjectId)
                        VALUES (@ContentID, @IndexTypeID, @QuestionID, @SyllabusID, @SetID, @StudentId, @LevelId, 4, 0, @SubjectId)";

                            await _connection.ExecuteAsync(insertQuestionQuery, new
                            {
                                ContentID = request.contentId,
                                IndexTypeID = request.indexTypeId,
                                QuestionID = question.QuestionId,
                                SyllabusID = request.SyllabusId,
                                SetID = setId,
                                StudentId = request.StudentId,
                                LevelId = level.LevelId,
                                SubjectId = request.subjectId
                            });

                            questionsToReturn.Add(question);
                        }
                    }
                }
            }

            // Step 5: Final mapping to DTOs (you can reuse your original DTO mapping code)
            var response = questionsToReturn.Select(item =>
            {
                if (item.QuestionTypeId == 11)
                {
                    return new QuestionResponseDTO
                    {
                        QuestionId = item.QuestionId,
                        Paragraph = item.Paragraph,
                        SubjectName = item.SubjectName,
                        IndexTypeName = item.IndexTypeName,
                        ContentIndexName = item.ContentIndexName,
                        ContentIndexId = item.ContentIndexId,
                        CreatedBy = item.CreatedBy,
                        CreatedOn = item.CreatedOn,
                        EmployeeId = item.EmployeeId,
                        IndexTypeId = item.IndexTypeId,
                        subjectID = item.subjectID,
                        ModifiedOn = item.ModifiedOn,
                        QuestionTypeId = item.QuestionTypeId,
                        QuestionTypeName = item.QuestionTypeName,
                        QuestionCode = item.QuestionCode,
                        Explanation = item.Explanation,
                        ExtraInformation = item.ExtraInformation,
                        IsActive = item.IsActive,
                        ComprehensiveChildQuestions = GetChildQuestions(item.QuestionCode)
                    };
                }
                else
                {
                    return new QuestionResponseDTO
                    {
                        QuestionId = item.QuestionId,
                        QuestionDescription = item.QuestionDescription,
                        QuestionTypeId = item.QuestionTypeId,
                        Status = item.Status,
                        CreatedBy = item.CreatedBy,
                        CreatedOn = item.CreatedOn,
                        ModifiedBy = item.ModifiedBy,
                        ModifiedOn = item.ModifiedOn,
                        subjectID = item.subjectID,
                        SubjectName = item.SubjectName,
                        EmployeeId = item.EmployeeId,
                        IndexTypeId = item.IndexTypeId,
                        IndexTypeName = item.IndexTypeName,
                        ContentIndexId = item.ContentIndexId,
                        ContentIndexName = item.ContentIndexName,
                        IsRejected = item.IsRejected,
                        IsApproved = item.IsApproved,
                        QuestionTypeName = item.QuestionTypeName,
                        QuestionCode = item.QuestionCode,
                        Explanation = item.Explanation,
                        ExtraInformation = item.ExtraInformation,
                        IsActive = item.IsActive,
                        MatchPairs = item.QuestionTypeId == 6 || item.QuestionTypeId == 12 ? GetMatchPairs(item.QuestionCode, item.QuestionId) : null,
                        MatchThePairType2Answers = item.QuestionTypeId == 12 ? GetMatchThePairType2Answers(item.QuestionCode, item.QuestionId) : null,
                        Answersingleanswercategories = (item.QuestionTypeId != 6 && item.QuestionTypeId != 12) ? GetSingleAnswer(item.QuestionCode, item.QuestionId) : null,
                        AnswerMultipleChoiceCategories = (item.QuestionTypeId != 12) ? GetMultipleAnswers(item.QuestionCode) : null
                    };
                }
            }).ToList();
            var levelOrder = new List<int> { 1, 2, 3 };
            if (request.QuestionStatus != null && request.QuestionStatus.Any(id => id != 0))
            {
                // Step 1: Get all QuestionIds from current list
                var questionIds = questionsToReturn.Select(q => q.QuestionId).ToList();

                // Step 2: Get the latest SetID for the student
              var latestSetId = await _connection.ExecuteScalarAsync<int?>(
                    @"SELECT TOP 1 SetID 
          FROM tblConceptwisePracticeQuestions 
          WHERE StudentId = @StudentId 
          ORDER BY SetID DESC",
                    new { StudentId = request.StudentId });

                if (latestSetId.HasValue)
                {
                    // Step 3: Fetch only questions that match latest SetID
                    var questionStatusDict = (await _connection.QueryAsync<(int QuestionId, int QuestionStatusId)>(
                        @"SELECT QuestionID, QuestionStatusId 
              FROM tblConceptwisePracticeQuestions 
              WHERE StudentId = @StudentId 
                AND SetID = @SetID 
                AND QuestionID IN @QuestionIds",
                        new { StudentId = request.StudentId, SetID = latestSetId.Value, QuestionIds = questionIds }))
                        .ToDictionary(q => q.QuestionId, q => q.QuestionStatusId);

                    // Step 4: Filter questions
                    questionsToReturn = questionsToReturn
                        .Where(q => questionStatusDict.ContainsKey(q.QuestionId) &&
                                    request.QuestionStatus.Contains(questionStatusDict[q.QuestionId]))
                        .ToList();
                }
            }

            // Apply filter on QuestionStatusId if provided
            if (request.DifficultyLevel != null && request.DifficultyLevel.Any(id => id != 0))
            {
                questionsToReturn = questionsToReturn
                    .Where(q => request.DifficultyLevel.Contains(q.LevelId))
                    .ToList();
            }
            questionsToReturn = questionsToReturn
                .OrderBy(q => levelOrder.IndexOf(q.LevelId))
                .ToList();
            var questionResponse = new QuestionsSetResponse
            {
                NumberOfSet = setId,
                QuestionResponseDTOs = questionsToReturn
            };
            return new ServiceResponse<QuestionsSetResponse>(true, "Records found successfully", questionResponse, 200, questionsToReturn.Count);
        }
        public async Task<ServiceResponse<string>> MarkQuestionAsSave(SaveQuestionConceptwisePracticeRequest request)
        {
            var response = new ServiceResponse<string>(true, string.Empty, string.Empty, 200);
            try
            {
                // Check if the question is already saved by this student
                var existingRecord = await _connection.QueryFirstOrDefaultAsync<int?>(
                    @"SELECT CPQSId 
              FROM [tblConceptwisePracticeQuestionSave] 
              WHERE StudentId = @StudentId AND QuestionId = @QuestionId",
                      new { StudentId = request.RegistrationId, request.QuestionId });

                if (existingRecord != null)
                {
                    // If record exists, delete it
                    var deleteQuery = @"DELETE FROM [tblConceptwisePracticeQuestionSave] 
                                WHERE StudentId = @RegistrationId AND QuestionId = @QuestionId";
                    var rowsDeleted = await _connection.ExecuteAsync(deleteQuery, new
                    {
                        request.RegistrationId,
                        request.QuestionId
                    });

                    if (rowsDeleted > 0)
                    {
                        response.Data = "Question unsaved successfully.";
                        response.Success = true;
                    }
                    else
                    {
                        response.Data = "Failed to unsave the question.";
                        response.Success = false;
                    }
                }
                else
                {
                    // If no record exists, insert a new saved question record
                    var insertQuery = @"INSERT INTO [tblConceptwisePracticeQuestionSave] (StudentId, QuestionId, QuestionCode) 
                                VALUES (@RegistrationId, @QuestionId, @QuestionCode)";

                    var rowsInserted = await _connection.ExecuteAsync(insertQuery, new
                    {
                        request.RegistrationId,
                        request.QuestionId,
                        request.QuestionCode
                    });

                    if (rowsInserted > 0)
                    {
                        response.Data = "Question saved successfully.";
                        response.Success = true;
                    }
                    else
                    {
                        response.Data = "Failed to save the question.";
                        response.Success = false;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                response.Data = $"An error occurred: {ex.Message}";
                response.Success = false;
            }

            return response;
        }
        public async Task<ServiceResponse<ConceptwiseAnswerResponse>> SubmitAnswerAsync(ConceptwisePracticeSubmitAnswerRequest request)
        {
            try
            {
                var response = new ConceptwiseAnswerResponse
                {
                    QuestionID = request.QuestionID
                };

                bool isCorrect = false;

                // Step 1: Get question type
                var questionData = await _connection.QueryFirstOrDefaultAsync<(int QuestionId, int QuestionTypeId)>(
                    @"SELECT QuestionId, QuestionTypeId FROM tblQuestion WHERE QuestionId = @QuestionID",
                    new { QuestionID = request.QuestionID });

                if (questionData.QuestionId == 0)
                    return new ServiceResponse<ConceptwiseAnswerResponse>(false, "Question not found", null, 404);

                // Step 2: Evaluate answer
                if (questionData.QuestionTypeId == 4 || questionData.QuestionTypeId == 9) // Subjective or Numeric
                {
                    string correctAnswer = await _connection.QueryFirstOrDefaultAsync<string>(
                        @"SELECT sac.Answer
                  FROM tblAnswersingleanswercategory sac
                  INNER JOIN tblAnswerMaster am ON sac.Answerid = am.Answerid
                  WHERE am.Questionid = @QuestionID",
                        new { request.QuestionID });

                    if (questionData.QuestionTypeId == 4) // Subjective
                    {
                        isCorrect = string.Equals(correctAnswer?.Trim(), request.SubjectiveAnswers?.Trim(), StringComparison.OrdinalIgnoreCase);
                    }
                    else if (questionData.QuestionTypeId == 9) // Numeric
                    {
                        isCorrect = decimal.TryParse(correctAnswer, out var correctVal) &&
                                    decimal.TryParse(request.SubjectiveAnswers, out var studentVal) &&
                                    correctVal == studentVal;
                    }
                }
                else if (questionData.QuestionTypeId == 13 || questionData.QuestionTypeId == 15) // Match/matrix
                {
                    var correctAnswers = (await _connection.QueryAsync<string>(
                        @"SELECT amc.Answer FROM tblAnswerMultipleChoiceCategory amc
                  INNER JOIN tblAnswerMaster am ON am.Answerid = amc.Answerid
                  WHERE am.Questionid = @QuestionID AND amc.IsCorrect = 1",
                        new { request.QuestionID })).ToList();

                    var studentAnswers = request.SubjectiveAnswers?.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList() ?? new();
                    isCorrect = correctAnswers.SequenceEqual(studentAnswers, StringComparer.OrdinalIgnoreCase);
                }
                else // MCQ
                {
                    var correctAnswerIds = (await _connection.QueryAsync<int>(
                        @"SELECT amc.Answermultiplechoicecategoryid FROM tblAnswerMultipleChoiceCategory amc
                  INNER JOIN tblAnswerMaster am ON am.Answerid = amc.Answerid
                  WHERE am.Questionid = @QuestionID AND amc.IsCorrect = 1",
                        new { request.QuestionID })).ToHashSet();

                    var studentAnswerIds = request.MultiOrSingleAnswerId?.ToHashSet() ?? new HashSet<int>();
                    isCorrect = correctAnswerIds.SetEquals(studentAnswerIds);
                }

                // Calculate Time Taken
                TimeSpan? timeTaken = null;
                if (request.StaTime.HasValue && request.EndTime.HasValue)
                    timeTaken = request.EndTime.Value - request.StaTime.Value;

                // Insert into tblConceptwisePracticeAnswers
                await _connection.ExecuteAsync(@"
            INSERT INTO tblConceptwisePracticeAnswers
                (StudentId, QuestionId, AnswerIds, IsCorrect, AnswerStatus, Answer, Marks, SubjectId, QuestionTypeId, StaTime, EndTime, TimeTaken)
            VALUES
                (@StudentId, @QuestionId, @AnswerIds, @IsCorrect, @AnswerStatus, @Answer, @Marks, @SubjectId, @QuestionTypeId, @StaTime, @EndTime, @TimeTaken)",
                    new
                    {
                        StudentId = request.StudentID,
                        QuestionId = request.QuestionID,
                        AnswerIds = request.MultiOrSingleAnswerId != null && request.MultiOrSingleAnswerId.Any()
                                        ? string.Join(",", request.MultiOrSingleAnswerId):string.Empty,
                        IsCorrect = isCorrect,
                        AnswerStatus = "string",
                        Answer = request.SubjectiveAnswers ?? string.Empty,
                        Marks = 0,
                        SubjectId = request.SubjectID,
                        QuestionTypeId = request.QuestionTypeID,
                        StaTime = request.StaTime,
                        EndTime = request.EndTime,
                        TimeTaken = timeTaken?.TotalSeconds
                    });
                response.IsAnswerCorrect = isCorrect;
                var latestSetId = await _connection.ExecuteScalarAsync<int?>(
    @"SELECT TOP 1 SetID 
      FROM tblConceptwisePracticeQuestions 
      WHERE StudentId = @StudentId 
      ORDER BY SetID DESC",
    new { StudentId = request.StudentID });

                await _connection.ExecuteAsync(@"update [tblConceptwisePracticeQuestions] set Iscorrect = @IsCorrect and QuestionStatusId = @QuestionStatusId 
where StudentId = @StudentId and SetID = @setid and QuestionID = @QuestionId and ", new
                {
                    IsCorrect = isCorrect,
                    QuestionStatusId = request.QuestionstatusId,
                    StudentId = request.StudentID,
                    setid = latestSetId,
                    QuestionId = request.QuestionID
                });
                return new ServiceResponse<ConceptwiseAnswerResponse>(true, "Answer submitted successfully", response, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ConceptwiseAnswerResponse>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<QuestionAnalyticsResponseDTO>> GetQuestionAnalyticsAsync(int studentId, int questionId, int setId)
        {
            try
            {
                string query = @"WITH StudentGroup AS (
    SELECT BoardId, ClassID, CourseID
    FROM tblStudentClassCourseMapping
    WHERE RegistrationID = @StudentId
),
Classmates AS (
    SELECT RegistrationID AS StudentID
    FROM tblStudentClassCourseMapping
    WHERE BoardId = (SELECT BoardId FROM StudentGroup)
      AND ClassID = (SELECT ClassID FROM StudentGroup)
      AND CourseID = (SELECT CourseID FROM StudentGroup)
      AND RegistrationID <> @StudentId
),
StudentStats AS (
    SELECT 
        CPA.StudentId,
        CPA.QuestionId,
        CPA.IsCorrect,
        DATEDIFF(SECOND, CPA.StaTime, CPA.EndTime) AS TimeTakenInSeconds
    FROM tblConceptwisePracticeAnswers CPA
    INNER JOIN tblConceptwisePracticeQuestions CPQ
        ON CPA.StudentId = CPQ.StudentId AND CPA.QuestionId = CPQ.QuestionId
    WHERE CPA.StudentId = @StudentId AND CPA.QuestionId = @QuestionId AND CPQ.SetID = @SetId
),
ClassmateStats AS (
    SELECT 
        CPA.StudentId,
        CPA.QuestionId,
        CPA.IsCorrect,
        DATEDIFF(SECOND, CPA.StaTime, CPA.EndTime) AS TimeTakenInSeconds
    FROM tblConceptwisePracticeAnswers CPA
    INNER JOIN tblConceptwisePracticeQuestions CPQ
        ON CPA.StudentId = CPQ.StudentId AND CPA.QuestionId = CPQ.QuestionId
    INNER JOIN Classmates C ON CPA.StudentId = C.StudentID
    WHERE CPA.QuestionId = @QuestionId AND CPQ.SetID = @SetId
),
YourAccuracy AS (
    SELECT 
        CAST(SUM(CASE WHEN IsCorrect = 1 THEN 1 ELSE 0 END) * 100.0 / COUNT(*) AS DECIMAL(10,2)) AS Accuracy,
        AVG(TimeTakenInSeconds) AS AvgTimeSpentInSeconds
    FROM StudentStats
),
ClassmateAccuracy AS (
    SELECT 
        CAST(SUM(CASE WHEN IsCorrect = 1 THEN 1 ELSE 0 END) * 100.0 / NULLIF(COUNT(*), 0) AS DECIMAL(10,2)) AS AvgAccuracy,
        AVG(TimeTakenInSeconds) AS AvgTimeSpentInSeconds,
        COUNT(DISTINCT StudentId) AS TotalAttempts,
        SUM(CASE WHEN IsCorrect = 1 THEN 1 ELSE 0 END) AS TotalCorrects
    FROM ClassmateStats
)
SELECT 
    YA.Accuracy AS YourAccuracy,
    YA.AvgTimeSpentInSeconds AS YourTimeSpent,
    CA.AvgAccuracy AS ClassmateAverageAccuracy,
    CA.AvgTimeSpentInSeconds AS ClassmateAverageTimeSpent,
    CA.TotalCorrects AS TotalCorrectClassmates,
    CA.TotalAttempts AS TotalClassmatesAttempted
FROM YourAccuracy YA, ClassmateAccuracy CA;";

                var result = await _connection.QueryFirstOrDefaultAsync<QuestionAnalyticsResponseDTO>(query, new
                {
                    StudentId = studentId,
                    QuestionId = questionId,
                    SetId = setId
                });

                return new ServiceResponse<QuestionAnalyticsResponseDTO>(true, "Question analytics fetched successfully", result ?? new QuestionAnalyticsResponseDTO(), 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<QuestionAnalyticsResponseDTO>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<PracticePerformanceStatsDto>> GetStudentPracticeStatsAsync(int studentId, int setId, int indexTypeId, int contentId)
        {
            try
            {
                // Step 1: Get all questions with their status
                string fetchQuestionsQuery = @"
        SELECT QuestionID, QuestionStatusId
        FROM tblConceptwisePracticeQuestions
        WHERE StudentId = @StudentId
          AND SetID = @SetId
          AND IndexTypeID = @IndexTypeId
          AND ContentID = @ContentId";

                var questions = (await _connection.QueryAsync<(int QuestionID, int QuestionStatusId)>(fetchQuestionsQuery,
                    new { StudentId = studentId, SetId = setId, IndexTypeId = indexTypeId, ContentId = contentId })).ToList();

                if (!questions.Any())
                    return new ServiceResponse<PracticePerformanceStatsDto>(false, "No questions found", null, 404);

                var questionIds = questions.Select(q => q.QuestionID).Distinct().ToList();

                // Step 2: Fetch student's answers and correctness
                string fetchAnswersQuery = @"
        SELECT QuestionId, IsCorrect
        FROM tblConceptwisePracticeAnswers
        WHERE StudentId = @StudentId
          AND QuestionId IN @QuestionIds";

                var answers = (await _connection.QueryAsync<(int QuestionId, bool IsCorrect)>(fetchAnswersQuery,
                    new { StudentId = studentId, QuestionIds = questionIds })).ToList();

                // Step 3: Process logic
                var attemptedStatuses = new HashSet<int> { 1, 3, 5 }; // Answered, Review, Review with Answer

                int total = questions.Count;
                int attempted = questions.Count(q => attemptedStatuses.Contains(q.QuestionStatusId));
                int unattempted = total - attempted;

                int correct = answers.Count(a => a.IsCorrect);
                int incorrect = attempted - correct;

                decimal accuracy = attempted > 0 ? Math.Round((decimal)correct * 100 / attempted, 2) : 0;

                // Percentages
                decimal correctPercentage = total > 0 ? Math.Round((decimal)correct * 100 / total, 2) : 0;
                decimal incorrectPercentage = total > 0 ? Math.Round((decimal)incorrect * 100 / total, 2) : 0;
                decimal unattemptedPercentage = total > 0 ? Math.Round((decimal)unattempted * 100 / total, 2) : 0;

                var dto = new PracticePerformanceStatsDto
                {
                    CorrectAnswers = correct,
                    IncorrectAnswers = incorrect,
                    UnattemptedQuestions = unattempted,
                    AverageAccuracyRate = accuracy,

                    CorrectPercentage = correctPercentage,
                    IncorrectPercentage = incorrectPercentage,
                    UnattemptedPercentage = unattemptedPercentage
                };

                return new ServiceResponse<PracticePerformanceStatsDto>(true, "Performance stats fetched successfully", dto, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<PracticePerformanceStatsDto>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<StudentTimeAnalysisDto>> GetStudentTimeAnalysisAsync(int studentId, int setId, int indexTypeId, int contentId)
        {
            try
            {
                string questionQuery = @"
            SELECT QuestionID, QuestionStatusId
            FROM tblConceptwisePracticeQuestions
            WHERE StudentId = @StudentId AND SetID = @SetId AND IndexTypeID = @IndexTypeId AND ContentID = @ContentId";

                var questions = (await _connection.QueryAsync<(int QuestionID, int QuestionStatusId)>(
                    questionQuery, new { StudentId = studentId, SetId = setId, IndexTypeId = indexTypeId, ContentId = contentId })).ToList();

                var questionIds = questions.Select(q => q.QuestionID).ToList();

                if (!questionIds.Any())
                    return new ServiceResponse<StudentTimeAnalysisDto>(false, "No questions found", null, 404);

                // Get time taken by the given student
                string answerQuery = @"
            SELECT QuestionId, IsCorrect, DATEDIFF(SECOND, StaTime, EndTime) AS TimeInSeconds
            FROM tblConceptwisePracticeAnswers
            WHERE StudentId = @StudentId AND QuestionId IN @QuestionIds";

                var answers = (await _connection.QueryAsync<(int QuestionId, bool? IsCorrect, int? TimeInSeconds)>(
                    answerQuery, new { StudentId = studentId, QuestionIds = questionIds })).ToList();

                var correctTimes = answers.Where(a => a.IsCorrect == true && a.TimeInSeconds.HasValue).Select(a => a.TimeInSeconds.Value).ToList();
                var incorrectTimes = answers.Where(a => a.IsCorrect == false && a.TimeInSeconds.HasValue).Select(a => a.TimeInSeconds.Value).ToList();

                var unansweredIds = questions.Where(q => q.QuestionStatusId == 2).Select(q => q.QuestionID).ToHashSet();
                var unansweredTimes = answers.Where(a => unansweredIds.Contains(a.QuestionId) && a.TimeInSeconds.HasValue).Select(a => a.TimeInSeconds.Value).ToList();

                var totalTimes = answers.Where(a => a.TimeInSeconds.HasValue).Select(a => a.TimeInSeconds.Value).ToList();

                // Step: Get classmates (same board, class, course)
                var studentGroup = await _connection.QueryFirstOrDefaultAsync<(int BoardId, int ClassId, int CourseId)>(
                    @"SELECT BoardId, ClassID, CourseID 
              FROM tblStudentClassCourseMapping 
              WHERE RegistrationID = @StudentId", new { StudentId = studentId });

                var classmates = (await _connection.QueryAsync<int>(
                    @"SELECT RegistrationID 
              FROM tblStudentClassCourseMapping 
              WHERE BoardId = @BoardId AND ClassID = @ClassId AND CourseID = @CourseId AND RegistrationID <> @StudentId",
                    new { StudentId = studentId, studentGroup.BoardId, studentGroup.ClassId, studentGroup.CourseId })).ToList();

                // Step: Get all classmates’ answers on same questionIds
                string classmatesTimeQuery = @"
            SELECT DATEDIFF(SECOND, StaTime, EndTime) AS TimeInSeconds
            FROM tblConceptwisePracticeAnswers
            WHERE StudentId IN @Classmates AND QuestionId IN @QuestionIds AND StaTime IS NOT NULL AND EndTime IS NOT NULL";

                var classmatesTimes = (await _connection.QueryAsync<int?>(
                    classmatesTimeQuery, new { Classmates = classmates, QuestionIds = questionIds }))
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .ToList();

                var dto = new StudentTimeAnalysisDto
                {
                    TotalTimeSpent = ConvertSecondsToTimeFormat(totalTimes.Sum()),
                    AverageTimePerQuestion = ConvertSecondsToTimeFormat(SafeAverage(totalTimes)),

                    TotalTimeOnCorrectAnswers = ConvertSecondsToTimeFormat(correctTimes.Sum()),
                    AverageTimePerCorrectAnswer = ConvertSecondsToTimeFormat(SafeAverage(correctTimes)),

                    TotalTimeOnIncorrectAnswers = ConvertSecondsToTimeFormat(incorrectTimes.Sum()),
                    AverageTimePerIncorrectAnswer = ConvertSecondsToTimeFormat(SafeAverage(incorrectTimes)),

                    TotalTimeOnUnansweredQuestions = ConvertSecondsToTimeFormat(unansweredTimes.Sum()),
                    AverageTimePerUnansweredQuestion = ConvertSecondsToTimeFormat(SafeAverage(unansweredTimes)),

                    AverageTimeByClassmates = ConvertSecondsToTimeFormat(classmatesTimes.Sum()),
                    AverageTimePerQuestionByClassmates = ConvertSecondsToTimeFormat(SafeAverage(classmatesTimes))
                };

                return new ServiceResponse<StudentTimeAnalysisDto>(true, "Time analysis retrieved successfully", dto, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<StudentTimeAnalysisDto>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<ChapterAccuracyReportResponse>> GetChapterAccuracyReportAsync(ChapterAccuracyReportRequest request)
        {
            try
            {
                // Step 1: Get all questions in the chapter for the given student, setId, indexTypeId, and contentId
                string fetchQuestionIdsQuery = @"
            SELECT QuestionID
            FROM tblConceptwisePracticeQuestions
            WHERE StudentId = @StudentId AND SetID = @SetId AND IndexTypeID = @IndexTypeId AND ContentID = @ContentId";

                var questionIds = (await _connection.QueryAsync<int>(fetchQuestionIdsQuery, request)).ToList();

                if (!questionIds.Any())
                    return new ServiceResponse<ChapterAccuracyReportResponse>(false, "No questions found for this chapter.", null, 404);

                // Step 2: Get student’s accuracy
                string studentAccuracyQuery = @"
            SELECT 
                COUNT(*) AS TotalAttempts,
                SUM(CASE WHEN IsCorrect = 1 THEN 1 ELSE 0 END) AS CorrectAttempts
            FROM tblConceptwisePracticeAnswers
            WHERE StudentId = @StudentId AND QuestionId IN @QuestionIds";

                var studentData = await _connection.QueryFirstOrDefaultAsync<(int TotalAttempts, int CorrectAttempts)>(
                    studentAccuracyQuery, new { request.StudentId, QuestionIds = questionIds });

                decimal yourAccuracy = studentData.TotalAttempts > 0
                    ? Math.Round((studentData.CorrectAttempts * 100.0m) / studentData.TotalAttempts, 2)
                    : 0;

                // Step 3: Get Board/Class/Course group
                var groupQuery = @"
            SELECT BoardId, ClassID, CourseID
            FROM tblStudentClassCourseMapping
            WHERE RegistrationID = @StudentId";

                var group = await _connection.QueryFirstOrDefaultAsync<(int BoardId, int ClassId, int CourseId)>(
                    groupQuery, new { request.StudentId });

                // Step 4: Get classmates
                var classmatesQuery = @"
            SELECT RegistrationID AS StudentID
            FROM tblStudentClassCourseMapping
            WHERE BoardId = @BoardId AND ClassID = @ClassId AND CourseID = @CourseId AND RegistrationID <> @StudentId";

                var classmates = (await _connection.QueryAsync<int>(
                    classmatesQuery, new { request.StudentId, group.BoardId, group.ClassId, group.CourseId })).ToList();

                if (!classmates.Any())
                {
                    return new ServiceResponse<ChapterAccuracyReportResponse>(true, "No classmates found.", new ChapterAccuracyReportResponse
                    {
                        YourAccuracy = yourAccuracy,
                        AverageClassAccuracy = 0,
                        StudentsOutperformingYou = 0,
                        TotalClassmatesAttempted = 0,
                        YourAttemptCount = request.SetId - 1
                    }, 200);
                }

                // Step 5: Get classmates' accuracy
                string classmateAccuracyQuery = @"
            SELECT StudentId,
                   COUNT(*) AS TotalAttempts,
                   SUM(CASE WHEN IsCorrect = 1 THEN 1 ELSE 0 END) AS CorrectAttempts
            FROM tblConceptwisePracticeAnswers
            WHERE StudentId IN @ClassmateIds AND QuestionId IN @QuestionIds
            GROUP BY StudentId";

                var classmateStats = (await _connection.QueryAsync<(int StudentId, int TotalAttempts, int CorrectAttempts)>(
                    classmateAccuracyQuery, new { ClassmateIds = classmates, QuestionIds = questionIds })).ToList();

                var classmateAccuracies = classmateStats
                    .Where(x => x.TotalAttempts > 0)
                    .Select(x => Math.Round((x.CorrectAttempts * 100.0m) / x.TotalAttempts, 2))
                    .ToList();

                decimal avgClassAccuracy = classmateAccuracies.Any() ? Math.Round(classmateAccuracies.Average(), 2) : 0;
                int outperforming = classmateAccuracies.Count(x => x > yourAccuracy);
                int totalAttempted = classmateStats.Count;

                var response = new ChapterAccuracyReportResponse
                {
                    YourAccuracy = yourAccuracy,
                    AverageClassAccuracy = avgClassAccuracy,
                    StudentsOutperformingYou = outperforming,
                    TotalClassmatesAttempted = totalAttempted,
                    YourAttemptCount = request.SetId > 1 ? request.SetId - 1 : 0
                };

                return new ServiceResponse<ChapterAccuracyReportResponse>(true, "Chapter accuracy report generated.", response, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ChapterAccuracyReportResponse>(false, ex.Message, null, 500);
            }
        }
        private decimal PercentageCalculation(int indexTypeId, int? contentIndexId, int studentId, int? subjectId, int? syllabusId)
        {
            if (indexTypeId == 0 && contentIndexId == 0 && subjectId.HasValue && syllabusId.HasValue)
            {
                // Get all active chapters for subject & syllabus
                var chapterIds = _connection.Query<int>(
                    @"SELECT ContentIndexId FROM tblContentIndexChapters 
              WHERE SubjectId = @SubjectId AND IsActive = 1",
                    new { SubjectId = subjectId, SyllabusId = syllabusId }).ToList();

                var chapterPercents = new List<decimal>();

                foreach (var chapterId in chapterIds)
                {
                    var chapterPercent = PercentageCalculation(1, chapterId, studentId, subjectId, syllabusId);
                    chapterPercents.Add(chapterPercent);
                }

                return chapterPercents.Any() ? Math.Round(chapterPercents.Average(), 2) : 0;
            }

            else if (indexTypeId == 1) // Chapter
            {
                var correct = _connection.QueryFirstOrDefault<int>(
                    @"SELECT COUNT(*) FROM tblConceptwisePracticeQuestions 
              WHERE ContentID = @ID AND IndexTypeID = 1 AND StudentId = @StudentId 
              AND SubjectId = @SubjectId AND SyllabusId = @SyllabusId AND IsCorrect = 1",
                    new { ID = contentIndexId, StudentId = studentId, SubjectId = subjectId, SyllabusId = syllabusId });

                var total = _connection.QueryFirstOrDefault<int>(
                    @"SELECT COUNT(*) FROM tblConceptwisePracticeQuestions 
              WHERE ContentID = @ID AND IndexTypeID = 1 AND StudentId = @StudentId 
              AND SubjectId = @SubjectId AND SyllabusId = @SyllabusId",
                    new { ID = contentIndexId, StudentId = studentId, SubjectId = subjectId, SyllabusId = syllabusId });

                decimal chapterPercent = total > 0 ? ((decimal)correct / total) * 100 : 0;

                var topicIds = _connection.Query<int>(
                    @"SELECT ContInIdTopic FROM tblContentIndexTopics 
              WHERE ContentIndexId = @ChapterId AND IsActive = 1",
                    new { ChapterId = contentIndexId }).ToList();

                var topicPercents = new List<decimal>();

                foreach (var topicId in topicIds)
                {
                    var topicPercent = PercentageCalculation(2, topicId, studentId, subjectId, syllabusId);
                    topicPercents.Add(topicPercent);
                }

                decimal avgTopic = topicPercents.Any() ? topicPercents.Average() : 0;
                return Math.Round((chapterPercent + avgTopic) / 2, 2);
            }

            else if (indexTypeId == 2) // Topic
            {
                var correct = _connection.QueryFirstOrDefault<int>(
                    @"SELECT COUNT(*) FROM tblConceptwisePracticeQuestions 
              WHERE ContentID = @ID AND IndexTypeID = 2 AND StudentId = @StudentId 
              AND SubjectId = @SubjectId AND SyllabusId = @SyllabusId AND IsCorrect = 1",
                    new { ID = contentIndexId, StudentId = studentId, SubjectId = subjectId, SyllabusId = syllabusId });

                var total = _connection.QueryFirstOrDefault<int>(
                    @"SELECT COUNT(*) FROM tblConceptwisePracticeQuestions 
              WHERE ContentID = @ID AND IndexTypeID = 2 AND StudentId = @StudentId 
              AND SubjectId = @SubjectId AND SyllabusId = @SyllabusId",
                    new { ID = contentIndexId, StudentId = studentId, SubjectId = subjectId, SyllabusId = syllabusId });

                decimal topicPercent = total > 0 ? ((decimal)correct / total) * 100 : 0;

                var subTopicIds = _connection.Query<int>(
                    @"SELECT ContInIdSubTopic FROM tblContentIndexSubTopics 
              WHERE ContInIdTopic = @TopicId AND IsActive = 1",
                    new { TopicId = contentIndexId }).ToList();

                var subPercents = new List<decimal>();

                foreach (var subId in subTopicIds)
                {
                    var subPercent = PercentageCalculation(3, subId, studentId, subjectId, syllabusId);
                    subPercents.Add(subPercent);
                }

                decimal avgSub = subPercents.Any() ? subPercents.Average() : 0;
                return Math.Round((topicPercent + avgSub) / 2, 2);
            }

            else if (indexTypeId == 3) // SubTopic
            {
                var correct = _connection.QueryFirstOrDefault<int>(
                    @"SELECT COUNT(*) FROM tblConceptwisePracticeQuestions 
              WHERE ContentID = @ID AND IndexTypeID = 3 AND StudentId = @StudentId 
              AND SubjectId = @SubjectId AND SyllabusId = @SyllabusId AND IsCorrect = 1",
                    new { ID = contentIndexId, StudentId = studentId, SubjectId = subjectId, SyllabusId = syllabusId });

                var total = _connection.QueryFirstOrDefault<int>(
                    @"SELECT COUNT(*) FROM tblConceptwisePracticeQuestions 
              WHERE ContentID = @ID AND IndexTypeID = 3 AND StudentId = @StudentId 
              AND SubjectId = @SubjectId AND SyllabusId = @SyllabusId",
                    new { ID = contentIndexId, StudentId = studentId, SubjectId = subjectId, SyllabusId = syllabusId });

                return total > 0 ? Math.Round(((decimal)correct / total) * 100, 2) : 0;
            }

            return 0;
        }
        private List<ParagraphQuestions> GetChildQuestions(string QuestionCode)
        {
            string sql = @"
                SELECT q.*, 
                       c.CourseName, 
                       b.BoardName, 
                       cl.ClassName, 
                       s.SubjectName,
                       et.ExamTypeName,
                       e.EmpFirstName,
                       qt.QuestionType as QuestionTypeName,
                       it.IndexType as IndexTypeName,
                       CASE 
                           WHEN q.IndexTypeId = 1 THEN ci.ContentName_Chapter
                           WHEN q.IndexTypeId = 2 THEN ct.ContentName_Topic
                           WHEN q.IndexTypeId = 3 THEN cst.ContentName_SubTopic
                       END AS ContentIndexName
                FROM tblQuestion q
                LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
                LEFT JOIN tblCourse c ON q.courseid = c.CourseID
                LEFT JOIN tblBoard b ON q.boardid = b.BoardID
                LEFT JOIN tblClass cl ON q.classid = cl.ClassID
                LEFT JOIN tblSubject s ON q.subjectID = s.SubjectID
                LEFT JOIN tblExamType et ON q.ExamTypeId = et.ExamTypeId
                LEFT JOIN tblEmployee e ON q.EmployeeId = e.EmployeeId
                LEFT JOIN tblQBIndexType it ON q.IndexTypeId = it.IndexId
                LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
                LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
                LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
                WHERE q.ParentQCode = @QuestionCode AND q.IsActive = 1 AND IsLive = 0 AND q.IsConfigure = 1";
            var parameters = new { QuestionCode = QuestionCode };
            var item = _connection.Query<dynamic>(sql, parameters);
            var response = item.Select(m => new ParagraphQuestions
            {
                QuestionId = m.QuestionId,
                QuestionDescription = m.QuestionDescription,
                ParentQId = m.ParentQId,
                ParentQCode = m.ParentQCode,
                QuestionTypeId = m.QuestionTypeId,
                Status = m.Status,
                CategoryId = m.CategoryId,
                CreatedBy = m.CreatedBy,
                CreatedOn = m.CreatedOn,
                ModifiedBy = m.ModifiedBy,
                ModifiedOn = m.ModifiedOn,
                subjectID = m.SubjectID,
                EmployeeId = m.EmployeeId,
                ModifierId = m.ModifierId,
                IndexTypeId = m.IndexTypeId,
                ContentIndexId = m.ContentIndexId,
                IsRejected = m.IsRejected,
                IsApproved = m.IsApproved,
                QuestionCode = m.QuestionCode,
                Explanation = m.Explanation,
                ExtraInformation = m.ExtraInformation,
                IsActive = m.IsActive,
                IsConfigure = m.IsConfigure,
                AnswerMultipleChoiceCategories = GetMultipleAnswers(m.QuestionCode),
                Answersingleanswercategories = GetSingleAnswer(m.QuestionCode, m.QuestionId)
            }).ToList();
            return response;
        }
        private List<MatchPair> GetMatchPairs(string questionCode, int questionId)
        {
            const string query = @"
        SELECT MatchThePairId, PairColumn, PairRow, PairValue
        FROM tblQuestionMatchThePair
        WHERE QuestionCode = @QuestionCode AND QuestionId = @QuestionId";


            return _connection.Query<MatchPair>(query, new { QuestionCode = questionCode, QuestionId = questionId }).ToList();

        }
        private List<DTOs.Response.MatchThePairAnswer> GetMatchThePairType2Answers(string questionCode, int questionId)
        {
            const string getAnswerIdQuery = @"
        SELECT AnswerId 
        FROM tblAnswerMaster
        WHERE QuestionCode = @QuestionCode AND QuestionId = @QuestionId";

            const string getAnswersQuery = @"
        SELECT MatchThePair2Id, PairColumn, PairRow
        FROM tblOptionsMatchThePair2
        WHERE AnswerId = @AnswerId";


            var answerId = _connection.QueryFirstOrDefault<int?>(getAnswerIdQuery, new { QuestionCode = questionCode, QuestionId = questionId });

            if (answerId == null)
            {
                return new List<DTOs.Response.MatchThePairAnswer>();
            }

            return _connection.Query<DTOs.Response.MatchThePairAnswer>(getAnswersQuery, new { AnswerId = answerId }).ToList();

        }
        private Answersingleanswercategory GetSingleAnswer(string QuestionCode, int QuestionId)
        {
            var answerMaster = _connection.QueryFirstOrDefault<AnswerMasters>(@"
        SELECT * FROM tblAnswerMaster WHERE QuestionCode = @QuestionCode and Questionid = @Questionid", new { QuestionCode, Questionid = QuestionId });

            if (answerMaster != null)
            {
                string getQuery = @"
            SELECT * FROM [tblAnswersingleanswercategory] WHERE [Answerid] = @Answerid";

                var response = _connection.QueryFirstOrDefault<Answersingleanswercategory>(getQuery, new { answerMaster.Answerid });
                return response ?? new Answersingleanswercategory();
            }
            else
            {
                return new Answersingleanswercategory();
            }
        }
        private List<AnswerMultipleChoiceCategory> GetMultipleAnswers(string QuestionCode)
        {
            var answerMaster = _connection.QueryFirstOrDefault<AnswerMasters>(@"
         SELECT TOP 1 * FROM tblAnswerMaster WHERE QuestionCode = @QuestionCode ORDER BY AnswerId DESC", new { QuestionCode });

            if (answerMaster != null)
            {
                string getQuery = @"
            SELECT * FROM [tblAnswerMultipleChoiceCategory] WHERE [Answerid] = @Answerid";

                var response = _connection.Query<AnswerMultipleChoiceCategory>(getQuery, new { answerMaster.Answerid });
                return response.AsList() ?? new List<AnswerMultipleChoiceCategory>();
            }
            else
            {
                return new List<AnswerMultipleChoiceCategory>();
            }
        }
        private bool IsSingleAnswerType(int questionTypeId)
        {
            // Assuming the following are single answer type IDs based on your data
            return questionTypeId == 4 || questionTypeId == 9;
            //|| questionTypeId == 8 || questionTypeId == 10 || questionTypeId == 11;
        }
        private int SafeAverage(List<int> values)
        {
            return (values?.Count ?? 0) > 0 ? (int)values.Average() : 0;
        }
        private string ConvertSecondsToTimeFormat(int seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            if (time.Hours > 0)
                return $"{time.Hours} hours {time.Minutes} minutes {time.Seconds} seconds";
            else if (time.Minutes > 0)
                return $"{time.Minutes} minutes {time.Seconds} seconds";
            else
                return $"{time.Seconds} seconds";
        }
    }
}
