using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using System.Data;
using System.Linq;
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
        public async Task<ServiceResponse<List<ChapterTreeResponse>>> GetSyllabusContentDetailsWebView(SyllabusDetailsRequestWebView request)
        {
            List<ChapterTreeResponse> chapterList = new();

            if (_connection.State != ConnectionState.Open)
                _connection.Open();

            try
            {
                string queryChapters = @"
            SELECT DISTINCT
                c.ContentIndexId AS ContentId,
                c.ContentName_Chapter AS ContentName,
                c.SubjectId,
                s.SyllabusID,
                c.IndexTypeId
            FROM tblContentIndexChapters c
            INNER JOIN tblSyllabusDetails s ON c.ContentIndexId = s.ContentIndexId
            WHERE s.SyllabusID = @SyllabusId AND c.SubjectId = @SubjectId AND c.IndexTypeId = 1 AND c.IsActive = 1";

                var chapters = await _connection.QueryAsync<ChapterTreeResponse>(queryChapters, new
                {
                    SyllabusId = request.SyllabusId,
                    SubjectId = request.SubjectId
                });

                foreach (var chapter in chapters)
                {
                    chapter.RegistrationId = request.RegistrationId;
                    chapter.Percentage =  PercentageCalculation(chapter.IndexTypeId, chapter.ContentId, request.RegistrationId, chapter.SubjectId, request.SyllabusId ?? 0);
                    chapter.Question = await GetAttemptCountAsync(chapter.IndexTypeId, chapter.ContentId, chapter.SubjectId, request.SyllabusId ?? 0, request.RegistrationId);
                    chapter.TopicCount = 0;
                    chapter.Topics = new List<TopicResponse>();

                    string queryTopics = @"
                SELECT DISTINCT
                    t.ContInIdTopic AS ContentId,
                    t.ContentName_Topic AS TopicName,
                    t.IndexTypeId
                FROM tblContentIndexTopics t
                INNER JOIN tblSyllabusDetails s ON t.ContentIndexId = s.ContentIndexId
                WHERE t.ContentIndexId = @ContentIndexId AND t.IndexTypeId = 2 AND t.IsActive = 1 AND s.SyllabusID = @SyllabusId";

                    var topics = await _connection.QueryAsync<TopicResponse>(queryTopics, new
                    {
                        ContentIndexId = chapter.ContentId,
                        SyllabusId = request.SyllabusId
                    });

                    chapter.TopicCount = topics.Count();

                    foreach (var topic in topics)
                    {
                        topic.Percentage =  PercentageCalculation(topic.IndexTypeId, topic.ContentId, request.RegistrationId, chapter.SubjectId, request.SyllabusId ?? 0);
                        topic.Question = await GetAttemptCountAsync(topic.IndexTypeId, topic.ContentId, chapter.SubjectId, request.SyllabusId ?? 0, request.RegistrationId);
                        topic.SubTopics = new List<SubTopicResponse>();
                        topic.SubTopicCount = 0;

                        string querySubTopics = @"
                    SELECT
                        s.ContInIdSubTopic AS ContentId,
                        s.ContentName_SubTopic AS SubTopicName
                    FROM tblContentIndexSubTopics s
                    INNER JOIN tblSyllabusDetails d ON s.ContInIdTopic = d.ContentIndexId
                    WHERE s.ContInIdTopic = @ContentIndexId AND s.IndexTypeId = 3 AND s.IsActive = 1 AND d.SyllabusID = @SyllabusId";

                        var subTopics = await _connection.QueryAsync<SubTopicResponse>(querySubTopics, new
                        {
                            ContentIndexId = topic.ContentId,
                            SyllabusId = request.SyllabusId
                        });

                        foreach (var subTopic in subTopics)
                        {
                            subTopic.Percentage =  PercentageCalculation(3, subTopic.ContentId, request.RegistrationId, chapter.SubjectId, request.SyllabusId ?? 0);
                            subTopic.Question = await GetAttemptCountAsync(3, subTopic.ContentId, chapter.SubjectId, request.SyllabusId ?? 0, request.RegistrationId);
                        }

                        topic.SubTopics = subTopics.ToList();
                        topic.SubTopicCount = subTopics.Count();
                    }

                    chapter.Topics = topics.ToList();
                }
                chapterList = chapters.ToList();
                return new ServiceResponse<List<ChapterTreeResponse>>(true, "Success", chapterList, 200, chapterList.Count);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ChapterTreeResponse>>(false, ex.Message, null, 500);
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
                s.SyllabusID, s.Synopsis,
                c.IndexTypeId,
                (
                    SELECT COUNT(*) 
                    FROM tblContentIndexTopics t
                    INNER JOIN tblSyllabusDetails sd ON t.ContInIdTopic = sd.ContentIndexId
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
                s.IndexTypeId = 1 AND 
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
                s.SyllabusID, s.Synopsis,
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
            LEFT JOIN tblSyllabusDetails s ON t.ContInIdTopic = s.ContentIndexId
            WHERE 
                t.ContentIndexId = @ContentIndexId AND 
                s.IndexTypeId = 2 AND s.SyllabusID = @SyllabusID AND
                t.IsActive = 1";

                    contentResponse = (await _connection.QueryAsync<ConceptwisePracticeContentResponse>(queryTopics, new
                    {
                        ContentIndexId = request.ContentIndexId,
                        RegistrationId = request.RegistrationId,
                        SyllabusID = request.SyllabusId
                    })).ToList();
                }
                // Fetch subtopics (children of topics)
                else if (request.IndexTypeId == 2 && request.ContentIndexId.HasValue)
                {
                    string querySubTopics = @"
            SELECT 
                s.ContInIdSubTopic AS ContentId, 
                s.ContentName_SubTopic AS ContentName, 
                d.SyllabusID, d.Synopsis,
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
            LEFT JOIN tblSyllabusDetails d ON s.ContInIdSubTopic = d.ContentIndexId
            WHERE 
                s.ContInIdTopic = @ContentIndexId AND 
                d.IndexTypeId = 3 AND d.SyllabusID = @SyllabusID AND
                s.IsActive = 1";

                    contentResponse = (await _connection.QueryAsync<ConceptwisePracticeContentResponse>(querySubTopics, new
                    {
                        ContentIndexId = request.ContentIndexId,
                        RegistrationId = request.RegistrationId,
                        SyllabusID = request.SyllabusId
                    })).ToList();

                }

                // Add registration ID and calculate percentage
                foreach (var data in contentResponse)
                {
                    data.SubjectId = request.SubjectId;
                    data.RegistrationId = request.RegistrationId;
                    data.Percentage = PercentageCalculation(data.IndexTypeId, data.ContentId, request.RegistrationId, request.SubjectId, request.SyllabusId);
                    data.AttemptCount = await GetAttemptCountAsync(data.IndexTypeId, data.ContentId, request.SubjectId ?? 0, request.SyllabusId ?? 0, request.RegistrationId);
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

                    data.IsAnalytics = await GetIsAnalyticsAsync(data.IndexTypeId, data.ContentId, request.SubjectId ?? 0, request.SyllabusId ?? 0, request.RegistrationId);
                    data.IsQuestionAnalytics = data.IndexTypeId == 1 ? questionCount > 0 : false;

                    // Enable synopsis button only if synopsis is present
                    data.IsSynopsis = !string.IsNullOrWhiteSpace(data.Synopsis);
                }
                var response = (contentResponse == null || !contentResponse.Any())
    ? new ServiceResponse<List<ConceptwisePracticeContentResponse>>(false, "No records found", [], 404, 0)
    : new ServiceResponse<List<ConceptwisePracticeContentResponse>>(true, "Success", contentResponse, 200, contentResponse.Count);

                return response;

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
                    qt.QuestionType AS QuestionTypeName,
   CASE 
                   WHEN q.IndexTypeId = 1 THEN ci.ContentName_Chapter
                   WHEN q.IndexTypeId = 2 THEN ct.ContentName_Topic
                   WHEN q.IndexTypeId = 3 THEN cst.ContentName_SubTopic
               END AS ContentIndexName
                FROM tblQuestion q
                INNER JOIN tblQIDCourse qc ON q.QuestionId = qc.QID
                LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
                LEFT JOIN tbldifficultylevel dl ON qc.LevelId = dl.LevelId
  LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
        LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
        LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
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

                   // var questionsResponse = new List<QuestionResponseDTO>();
                    var questionsToInsert = new List<(int? QuestionId, int DisplayOrder)>();
                    var studentMappingsToInsert = new List<(int? QuestionId, int? SubjectId)>();
                    int displayOrder = 1, remainingLimit = 0;
                    
                    foreach (var questionData in questions)
                    {
                        if (remainingLimit <= 0)
                            break;

                        if (questionData.QuestionTypeId == 11)
                        {
                            var childQuestions = GetChildQuestions(questionData.QuestionCode);
                            int totalRequiredForThis = 1 + childQuestions.Count;

                            if (remainingLimit < totalRequiredForThis)
                                continue; // Skip this comprehensive block if not enough space left

                            questionData.ComprehensiveChildQuestions = childQuestions;

                            // Add parent question
                            questionsToReturn.Add(questionData);
                            questionsToInsert.Add((questionData.QuestionId, displayOrder));
                            studentMappingsToInsert.Add((questionData.QuestionId, questionData.subjectID));
                            displayOrder++;
                            remainingLimit--;

                            // Add child questions
                            foreach (var child in childQuestions)
                            {
                                var childAsResponse = new QuestionResponseDTO
                                {
                                    QuestionId = child.QuestionId ?? 0,
                                    QuestionDescription = child.QuestionDescription,
                                    QuestionTypeId = child.QuestionTypeId ?? 0,
                                    Status = child.Status,
                                    CreatedBy = child.CreatedBy,
                                    CreatedOn = child.CreatedOn,
                                    ModifiedBy = child.ModifiedBy,
                                    ModifiedOn = child.ModifiedOn,
                                    subjectID = child.subjectID,
                                    EmployeeId = child.EmployeeId,
                                    IndexTypeId = child.IndexTypeId,
                                    ContentIndexId = child.ContentIndexId,
                                    IsRejected = child.IsRejected,
                                    IsApproved = child.IsApproved,
                                    QuestionTypeName = "", // Optional
                                    QuestionCode = child.QuestionCode,
                                    Explanation = child.Explanation,
                                    ExtraInformation = child.ExtraInformation,
                                    IsActive = child.IsActive,
                                    ParentQId = child.ParentQId,
                                    ParentQCode = child.ParentQCode,
                                    Answersingleanswercategories = child.Answersingleanswercategories,
                                    AnswerMultipleChoiceCategories = child.AnswerMultipleChoiceCategories,
                                    ComprehensiveChildQuestions = null // child won't have nested children
                                };

                                questionsToReturn.Add(childAsResponse);
                                questionsToInsert.Add((child.QuestionId, displayOrder));
                                studentMappingsToInsert.Add((child.QuestionId, child.subjectID));
                                displayOrder++;
                                remainingLimit--;
                            }
                        }
                        else
                        {
                            questionsToReturn.Add(questionData);
                            questionsToInsert.Add((questionData.QuestionId, displayOrder));
                            studentMappingsToInsert.Add((questionData.QuestionId, questionData.subjectID));
                            displayOrder++;
                            remainingLimit--;
                        }
                    }
                }
            }
            else
            {
                string existingSetQuery = @"
SELECT q.*, cpq.*, qt.QuestionType AS QuestionTypeName,    CASE 
                   WHEN q.IndexTypeId = 1 THEN ci.ContentName_Chapter
                   WHEN q.IndexTypeId = 2 THEN ct.ContentName_Topic
                   WHEN q.IndexTypeId = 3 THEN cst.ContentName_SubTopic
               END AS ContentIndexName
FROM tblConceptwisePracticeQuestions cpq
INNER JOIN tblQuestion q ON q.QuestionId = cpq.QuestionID
LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
  LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
        LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
        LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
WHERE cpq.StudentId = @StudentId
AND cpq.ContentID = @ContentId
AND cpq.IndexTypeID = @IndexTypeId
AND cpq.SyllabusID = @SyllabusId
AND cpq.SetID = (
    SELECT MAX(SetID)
    FROM tblConceptwisePracticeQuestions
    WHERE StudentId = @StudentId
    AND ContentID = @ContentId
    AND IndexTypeID = @IndexTypeId
    AND SyllabusID = @SyllabusId
)
AND (cpq.QuestionStatusId = 4 OR cpq.Iscorrect = 0)";

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
                        qt.QuestionType AS QuestionTypeName,
   CASE 
                   WHEN q.IndexTypeId = 1 THEN ci.ContentName_Chapter
                   WHEN q.IndexTypeId = 2 THEN ct.ContentName_Topic
                   WHEN q.IndexTypeId = 3 THEN cst.ContentName_SubTopic
               END AS ContentIndexName
                    FROM tblQuestion q
                    INNER JOIN tblQIDCourse qc ON q.QuestionId = qc.QID
                    LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
                    LEFT JOIN tbldifficultylevel dl ON qc.LevelId = dl.LevelId
  LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
        LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
        LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
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
                        IsActive = item.IsActive
                    };
                }
            }).ToList();
            foreach (var item in questionsToReturn)
            {
                item.MatchPairs = item.QuestionTypeId == 6 || item.QuestionTypeId == 12 ? GetMatchPairs(item.QuestionCode, item.QuestionId) : null;
                item.MatchThePairType2Answers = item.QuestionTypeId == 12 ? GetMatchThePairType2Answers(item.QuestionCode, item.QuestionId) : null;
                item.Answersingleanswercategories = (item.QuestionTypeId != 6 && item.QuestionTypeId != 12) ? GetSingleAnswer(item.QuestionCode, item.QuestionId) : null;
                item.AnswerMultipleChoiceCategories = (item.QuestionTypeId != 12) ? GetMultipleAnswers(item.QuestionCode) : null;
            }
      
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

            // Step 1: Fetch all attempted questions for the student from tblConceptwisePracticeQuestions
            var allMappedQuestions = await _connection.QueryAsync<ConceptwisePracticeQuestion>(
                @"SELECT * FROM tblConceptwisePracticeQuestions 
      WHERE StudentId = @StudentId",
                new { StudentId = request.StudentId });

            // Step 2: Get difficulty levels from tbldifficultylevel
            var allLevels = await _connection.QueryAsync<DifficultyLevel>(
                @"SELECT * FROM tbldifficultylevel");

            // Step 3: Check current level status
            bool canProceedToNextLevel = true;
            int allowedLevelId = 0;
            foreach (var level in allLevels.OrderBy(l => l.LevelId))
            {
                var questionsInLevel = allMappedQuestions
                    .Where(q => q.LevelId == level.LevelId)
                    .ToList();

                if (!questionsInLevel.Any())
                    continue;

                int correctAnswers = questionsInLevel
                    .Count(q => q.IsCorrect == 1 && q.QuestionStatusId == 1); // 1 = answered

                int requiredCorrectAnswers = (int)Math.Ceiling(level.SuccessRate ?? 0);

                if (correctAnswers < requiredCorrectAnswers)
                {
                    canProceedToNextLevel = false;
                    allowedLevelId = level.LevelId;
                    break;
                }
            }

            // Only allow fetching of levels already unlocked
            questionsToReturn = questionsToReturn
  .Where(q => q.LevelId <= allowedLevelId)
  .ToList();


            // Step 4: Apply DifficultyLevel filter ONLY if explicitly provided (non-null and not only 0)
            bool shouldApplyLevelFilter = request.DifficultyLevel != null &&
                                          request.DifficultyLevel.Any(id => id != 0);

            if (shouldApplyLevelFilter)
            {
                // Allow all levels in request
                questionsToReturn = questionsToReturn.Where(q => request.DifficultyLevel.Contains(q.LevelId)).ToList();
            }
            var levelOrder = new List<int> { 1, 2, 3 };
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
                var questionData = await _connection.QueryFirstOrDefaultAsync<(int QuestionId, int QuestionTypeId, string Explanation)>(
                    @"SELECT QuestionId, QuestionTypeId, Explanation FROM tblQuestion WHERE QuestionId = @QuestionID",
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
                        AnswerStatus = isCorrect ? "Correct" : "Incorrect",
                        Answer = request.SubjectiveAnswers ?? string.Empty,
                        Marks = 0,
                        SubjectId = request.SubjectID,
                        QuestionTypeId = request.QuestionTypeID,
                        StaTime = request.StaTime,
                        EndTime = request.EndTime,
                        TimeTaken = timeTaken?.TotalSeconds
                    });
                response.IsAnswerCorrect = isCorrect;
                response.Explanation = questionData.Explanation;
                var latestSetId = await _connection.ExecuteScalarAsync<int?>(
    @"SELECT TOP 1 SetID 
      FROM tblConceptwisePracticeQuestions 
      WHERE StudentId = @StudentId 
      ORDER BY SetID DESC",
    new { StudentId = request.StudentID });

                await _connection.ExecuteAsync(@"update [tblConceptwisePracticeQuestions] set Iscorrect = @IsCorrect , QuestionStatusId = @QuestionStatusId 
where StudentId = @StudentId and SetID = @setid and QuestionID = @QuestionId", new
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
                    AverageTimePerUnansweredQuestion = ConvertSecondsToTimeFormat(SafeAverage(unansweredTimes))
                };

                return new ServiceResponse<StudentTimeAnalysisDto>(true, "Time analysis retrieved successfully", dto, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<StudentTimeAnalysisDto>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<ChapterAccuracyReportResponse>> GetChapterAccuracyReportAsync(ChapterAnalyticsRequest request)
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                // 1. Latest SetID for the current student and chapter
                string latestSetQuery = @"
SELECT MAX(SetID)
FROM tblConceptwisePracticeQuestions
WHERE StudentId = @StudentId
  AND SyllabusID = @SyllabusId
  AND SubjectId = @SubjectId
  AND ContentID = @ChapterId
  AND IndexTypeID = 1";

                int latestSetId = await _connection.ExecuteScalarAsync<int>(latestSetQuery, request);

                if (latestSetId == 0)
                {
                    return new ServiceResponse<ChapterAccuracyReportResponse>(true, "No data found", new ChapterAccuracyReportResponse(), 200);
                }

                // 2. My stats
                string myStatsQuery = @"
SELECT COUNT(*) AS Total, 
       SUM(CASE WHEN IsCorrect = 1 THEN 1 ELSE 0 END) AS Correct
FROM tblConceptwisePracticeQuestions
WHERE StudentId = @StudentId
  AND SyllabusID = @SyllabusId
  AND SubjectId = @SubjectId
  AND ContentID = @ChapterId
  AND IndexTypeID = 1
  AND SetID = @SetId";

                var myStats = await _connection.QueryFirstOrDefaultAsync<(int Total, int Correct)>(myStatsQuery,
                    new { request.StudentId, request.SyllabusId, request.SubjectId, request.ChapterId, SetId = latestSetId });

                decimal myAccuracy = myStats.Total > 0
                    ? Math.Round((decimal)myStats.Correct * 100 / myStats.Total, 2)
                    : 0;

                // 3. Classmates accuracy (average of all students except me)
                string classmatesStatsQuery = @"
SELECT StudentId,
       COUNT(*) AS Total,
       SUM(CASE WHEN IsCorrect = 1 THEN 1 ELSE 0 END) AS Correct
FROM tblConceptwisePracticeQuestions
WHERE SyllabusID = @SyllabusId
  AND SubjectId = @SubjectId
  AND ContentID = @ChapterId
  AND IndexTypeID = 1
  AND StudentId != @StudentId
GROUP BY StudentId";

                var classmates = (await _connection.QueryAsync<(int StudentId, int Total, int Correct)>(classmatesStatsQuery, request)).ToList();

                decimal classmatesAccuracy = 0;
                if (classmates.Count > 0)
                {
                    var average = classmates
                        .Where(c => c.Total > 0)
                        .Select(c => (decimal)c.Correct * 100 / c.Total)
                        .DefaultIfEmpty(0)
                        .Average();
                    classmatesAccuracy = Math.Round(average, 2);
                }

                // 4. Students with better accuracy
                int studentsWithBetterAccuracy = classmates
                    .Count(c => c.Total > 0 && ((decimal)c.Correct * 100 / c.Total) > myAccuracy);

                // 5. Total unique students attempted
                string totalStudentsQuery = @"
SELECT COUNT(DISTINCT StudentId)
FROM tblConceptwisePracticeQuestions
WHERE SyllabusID = @SyllabusId
  AND SubjectId = @SubjectId
  AND ContentID = @ChapterId
  AND IndexTypeID = 1";

                int totalStudents = await _connection.ExecuteScalarAsync<int>(totalStudentsQuery, request);

                // 6. Total attempts of the chapter
                string totalAttemptsQuery = @"
SELECT SetID
FROM tblConceptwisePracticeQuestions
WHERE SyllabusID = @SyllabusId
  AND SubjectId = @SubjectId
  AND ContentID = @ChapterId
  AND IndexTypeID = 1";

                int totalAttempts = await _connection.ExecuteScalarAsync<int>(totalAttemptsQuery, request);

                var response = new ChapterAccuracyReportResponse
                {
                    MyAccuracyPercentage = myAccuracy,
                    ClassmatesAccuracyPercentage = classmatesAccuracy,
                    StudentsWithBetterAccuracy = studentsWithBetterAccuracy,
                    TotalStudentsAttempted = totalStudents,
                    TotalAttemptsOfChapter = totalAttempts
                };

                return new ServiceResponse<ChapterAccuracyReportResponse>(true, "Success", response, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ChapterAccuracyReportResponse>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<ChapterAnalyticsResponse>> GetChapterAnalyticsAsync(ChapterAnalyticsRequest request)
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();

                // Step 1: Get Latest SetID
                string latestSetIdQuery = @"
SELECT MAX(SetID) 
FROM tblConceptwisePracticeQuestions
WHERE StudentId = @StudentId
  AND SyllabusID = @SyllabusId
  AND SubjectId = @SubjectId
  AND ContentID = @ChapterId
  AND IndexTypeID = 1";

                int setId = await _connection.ExecuteScalarAsync<int>(latestSetIdQuery, new
                {
                    StudentId = request.StudentId,
                    SyllabusId = request.SyllabusId,
                    SubjectId = request.SubjectId,
                    ChapterId = request.ChapterId
                });

                if (setId == 0)
                {
                    return new ServiceResponse<ChapterAnalyticsResponse>(true, "No attempts found", new ChapterAnalyticsResponse(), 200);
                }

                // Step 2: Fetch all related questions for Chapter, Topics, and Subtopics
                string analyticsQuery = @"
SELECT cpq.*
FROM tblConceptwisePracticeQuestions cpq
WHERE cpq.StudentId = @StudentId
  AND cpq.SyllabusID = @SyllabusId
  AND cpq.SubjectId = @SubjectId
  AND cpq.SetID = @SetID
  AND (
        (cpq.IndexTypeID = 1 AND cpq.ContentID = @ChapterId)
     OR (cpq.IndexTypeID = 2 AND cpq.ContentID IN (
            SELECT t.ContInIdTopic
            FROM tblContentIndexTopics t
            WHERE t.ContentIndexId = @ChapterId AND t.IsActive = 1
        ))
     OR (cpq.IndexTypeID = 3 AND cpq.ContentID IN (
            SELECT st.ContInIdSubTopic
            FROM tblContentIndexTopics t
            INNER JOIN tblContentIndexSubTopics st ON st.ContInIdTopic = t.ContInIdTopic
            WHERE t.ContentIndexId = @ChapterId AND t.IsActive = 1 AND st.IsActive = 1
        ))
    )";

                var questions = (await _connection.QueryAsync<dynamic>(analyticsQuery, new
                {
                    StudentId = request.StudentId,
                    SyllabusId = request.SyllabusId,
                    SubjectId = request.SubjectId,
                    ChapterId = request.ChapterId,
                    SetID = setId
                })).ToList();

                int total = questions.Count;
                int correct = questions.Count(q => q.Iscorrect == true);
                int unattempted = questions.Count(q => q.QuestionStatusId == 4);
                int incorrect = total - correct - unattempted;

                var result = new ChapterAnalyticsResponse
                {
                    TotalQuestions = total,
                    CorrectCount = correct,
                    IncorrectCount = incorrect,
                    UnattemptedCount = unattempted,
                    CorrectPercentage = total > 0 ? Math.Round((decimal)correct * 100 / total, 2) : 0,
                    IncorrectPercentage = total > 0 ? Math.Round((decimal)incorrect * 100 / total, 2) : 0,
                    UnattemptedPercentage = total > 0 ? Math.Round((decimal)unattempted * 100 / total, 2) : 0
                };

                return new ServiceResponse<ChapterAnalyticsResponse>(true, "Success", result, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ChapterAnalyticsResponse>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<ChapterTimeReportResponse>> GetChapterTimeReportAsync(ChapterAnalyticsRequest request)
        {
            var latestSetId = await _connection.ExecuteScalarAsync<int>(
                "SELECT TOP 1 SetID FROM tblConceptwisePracticeQuestions WHERE ContentID = @ChapterId AND IndexTypeID = 1 AND StudentId = @StudentId ORDER BY CPCID DESC",
                new { request.ChapterId, request.StudentId });

            var query = @"SELECT
    -- Student Stats
    SUM(CASE WHEN q.StudentId = @StudentId THEN DATEDIFF(SECOND, a.StaTime, a.EndTime) ELSE 0 END) AS TotalTimeSpentByMe,
    COUNT(CASE WHEN q.StudentId = @StudentId THEN 1 ELSE NULL END) AS TotalAttemptedByMe,

    SUM(CASE WHEN q.StudentId = @StudentId AND a.IsCorrect = 1 THEN DATEDIFF(SECOND, a.StaTime, a.EndTime) ELSE 0 END) AS TotalTimeCorrect,
    COUNT(CASE WHEN q.StudentId = @StudentId AND a.IsCorrect = 1 THEN 1 ELSE NULL END) AS CountCorrect,

    SUM(CASE WHEN q.StudentId = @StudentId AND a.IsCorrect = 0 THEN DATEDIFF(SECOND, a.StaTime, a.EndTime) ELSE 0 END) AS TotalTimeIncorrect,
    COUNT(CASE WHEN q.StudentId = @StudentId AND a.IsCorrect = 0 THEN 1 ELSE NULL END) AS CountIncorrect,

    SUM(CASE WHEN q.StudentId = @StudentId AND a.IsCorrect IS NULL AND q.QuestionStatusId = 2 THEN DATEDIFF(SECOND, a.StaTime, a.EndTime) ELSE 0 END) AS TotalTimeUnattempted,
    COUNT(CASE WHEN q.StudentId = @StudentId AND a.IsCorrect IS NULL AND q.QuestionStatusId = 2 THEN 1 ELSE NULL END) AS CountUnattempted,

    -- Classmate Stats
    SUM(CASE WHEN q.StudentId != @StudentId THEN DATEDIFF(SECOND, a.StaTime, a.EndTime) ELSE 0 END) AS ClassmatesTotalTime,
    COUNT(CASE WHEN q.StudentId != @StudentId THEN 1 ELSE NULL END) AS ClassmatesTotalAttempt
FROM tblConceptwisePracticeAnswers a
INNER JOIN tblConceptwisePracticeQuestions q ON a.QuestionId = q.QuestionID AND a.StudentId = q.StudentId
WHERE q.ContentID = @ChapterId AND q.IndexTypeID = 1 AND q.SetID = @SetId;";

            var result = await _connection.QueryFirstOrDefaultAsync<dynamic>(query, new
            {
                request.ChapterId,
                SetId = latestSetId,
                request.StudentId
            });

            if (result == null)
                return new ServiceResponse<ChapterTimeReportResponse>(false, string.Empty, new ChapterTimeReportResponse(), 500);

            var response = new ChapterTimeReportResponse
            {
                TotalTimeSpentByMe = ConvertSecondsToTimeFormat(result.TotalTimeSpentByMe),
                AvgTimeSpentByMePerQuestion = ConvertSecondsToTimeFormat(SafeDivide(result.TotalTimeSpentByMe, result.TotalAttemptedByMe)),

                TotalTimeCorrect = ConvertSecondsToTimeFormat(result.TotalTimeCorrect),
                AvgTimeCorrect = ConvertSecondsToTimeFormat(SafeDivide(result.TotalTimeCorrect, result.CountCorrect)),

                TotalTimeIncorrect = ConvertSecondsToTimeFormat(result.TotalTimeIncorrect),
                AvgTimeIncorrect = ConvertSecondsToTimeFormat(SafeDivide(result.TotalTimeIncorrect, result.CountIncorrect)),

                TotalTimeUnattempted = ConvertSecondsToTimeFormat(result.TotalTimeUnattempted),
                AvgTimeUnattempted = ConvertSecondsToTimeFormat(SafeDivide(result.TotalTimeUnattempted, result.CountUnattempted)),

                AvgTimeSpentByClassmates = ConvertSecondsToTimeFormat(result.ClassmatesTotalTime),
                AvgTimeSpentByClassmatesPerQuestion = ConvertSecondsToTimeFormat(SafeDivide(result.ClassmatesTotalTime, result.ClassmatesTotalAttempt))
            };

            return new ServiceResponse<ChapterTimeReportResponse>(true, "Records found", response, 200);
        }
        private string ConvertSecondsToTimeFormat(int seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            return $"{time.Minutes} minutes {time.Seconds} seconds";
        }
        private int SafeDivide(int total, int count)
        {
            return count == 0 ? 0 : total / count;
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
         SELECT * FROM tblAnswerMaster WHERE QuestionCode = @QuestionCode ORDER BY AnswerId DESC", new { QuestionCode });

            if (answerMaster != null)
            {
                string getQuery = @"
            SELECT Answermultiplechoicecategoryid as Answermultiplechoicecategoryid,
Answerid as Answerid, Answer as Answer, Iscorrect as Iscorrect
FROM [tblAnswerMultipleChoiceCategory] WHERE [Answerid] = @Answerid";

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
        private async Task<bool> GetIsAnalyticsAsync(int indexTypeId, int contentId, int subjectId, int syllabusId, int registrationId)
        {
            string query = string.Empty;

            if (indexTypeId == 1) // Chapter → check both child Topics and SubTopics
            {
                query = @"
            SELECT COUNT(1)
            FROM tblConceptwisePracticeQuestions q
            WHERE 
                q.QuestionStatusId != 4 AND
                q.SubjectId = @SubjectId AND
                q.SyllabusID = @SyllabusId AND
                q.StudentId = @RegistrationId AND
                (
                    EXISTS (
                        SELECT 1
                        FROM tblContentIndexTopics t
                        WHERE t.ContInIdTopic = q.ContentID AND t.ContentIndexId = @ContentId
                    )
                    OR
                    EXISTS (
                        SELECT 1
                        FROM tblContentIndexSubTopics st
                        INNER JOIN tblContentIndexTopics t ON st.ContInIdTopic = t.ContInIdTopic
                        WHERE st.ContInIdSubTopic = q.ContentID AND t.ContentIndexId = @ContentId
                    )
                )";
            }
            else if (indexTypeId == 2) // Topic → check child SubTopics
            {
                query = @"
            SELECT COUNT(1)
            FROM tblConceptwisePracticeQuestions q
            INNER JOIN tblContentIndexSubTopics st ON q.ContentID = st.ContInIdSubTopic
            WHERE 
                st.ContInIdTopic = @ContentId AND
                q.IndexTypeID = 3 AND
                q.SubjectId = @SubjectId AND
                q.SyllabusID = @SyllabusId AND
                q.StudentId = @RegistrationId AND
                q.QuestionStatusId != 4";
            }
            else if (indexTypeId == 3) // SubTopic → self check
            {
                query = @"
            SELECT COUNT(1)
            FROM tblConceptwisePracticeQuestions q
            WHERE 
                q.ContentID = @ContentId AND
                q.IndexTypeID = 3 AND
                q.SubjectId = @SubjectId AND
                q.SyllabusID = @SyllabusId AND
                q.StudentId = @RegistrationId AND
                q.QuestionStatusId != 4";
            }

            int count = await _connection.ExecuteScalarAsync<int>(query, new
            {
                ContentId = contentId,
                SubjectId = subjectId,
                SyllabusId = syllabusId,
                RegistrationId = registrationId
            });

            return count > 0;
        }
        private async Task<int> GetAttemptCountAsync(int indexTypeId, int contentId, int subjectId, int syllabusId, int registrationId)
        {
            var setIdsQuery = @"
        SELECT DISTINCT SetID
        FROM tblConceptwisePracticeQuestions
        WHERE 
            SyllabusID = @SyllabusId AND
            SubjectId = @SubjectId AND
            StudentId = @RegistrationId AND
            SetID IS NOT NULL AND SetID > 0";

            var setIds = (await _connection.QueryAsync<int>(setIdsQuery, new
            {
                SyllabusId = syllabusId,
                SubjectId = subjectId,
                RegistrationId = registrationId
            })).ToList();

            int attemptCount = 0;

            foreach (int setId in setIds)
            {
                bool isAttemptComplete = false;

                if (indexTypeId == 1) // Chapter
                {
                    string chapterCheckQuery = @"
                ;WITH TopicIDs AS (
                    SELECT ContInIdTopic
                    FROM tblContentIndexTopics
                    WHERE ContentIndexId = @ContentId AND IsActive = 1
                ),
                SubTopicIDs AS (
                    SELECT st.ContInIdSubTopic
                    FROM tblContentIndexSubTopics st
                    INNER JOIN TopicIDs t ON st.ContInIdTopic = t.ContInIdTopic
                    WHERE st.IsActive = 1
                )
                SELECT COUNT(1)
                FROM tblConceptwisePracticeQuestions q
                WHERE 
                    (
                        (q.ContentID = @ContentId AND q.IndexTypeID = 1) OR
                        (q.ContentID IN (SELECT ContInIdTopic FROM TopicIDs) AND q.IndexTypeID = 2) OR
                        (q.ContentID IN (SELECT ContInIdSubTopic FROM SubTopicIDs) AND q.IndexTypeID = 3)
                    )
                    AND q.SubjectId = @SubjectId
                    AND q.SyllabusID = @SyllabusId
                    AND q.StudentId = @RegistrationId
                    AND q.SetID = @SetId
                    AND q.QuestionStatusId != 2";  // != Correct

                    int incorrectCount = await _connection.ExecuteScalarAsync<int>(chapterCheckQuery, new
                    {
                        ContentId = contentId,
                        SubjectId = subjectId,
                        SyllabusId = syllabusId,
                        RegistrationId = registrationId,
                        SetId = setId
                    });

                    isAttemptComplete = incorrectCount == 0;
                }
                else if (indexTypeId == 2) // Topic
                {
                    string topicCheckQuery = @"
                SELECT COUNT(1)
                FROM tblConceptwisePracticeQuestions q
                INNER JOIN tblContentIndexSubTopics st ON q.ContentID = st.ContInIdSubTopic
                WHERE 
                    st.ContInIdTopic = @ContentId AND
                    st.IsActive = 1 AND
                    q.IndexTypeID = 3 AND
                    q.SubjectId = @SubjectId AND
                    q.SyllabusID = @SyllabusId AND
                    q.StudentId = @RegistrationId AND
                    q.SetID = @SetId AND
                    q.QuestionStatusId != 2";

                    int incorrectCount = await _connection.ExecuteScalarAsync<int>(topicCheckQuery, new
                    {
                        ContentId = contentId,
                        SubjectId = subjectId,
                        SyllabusId = syllabusId,
                        RegistrationId = registrationId,
                        SetId = setId
                    });

                    isAttemptComplete = incorrectCount == 0;
                }
                else if (indexTypeId == 3) // SubTopic
                {
                    string subtopicCheckQuery = @"
                SELECT COUNT(1)
                FROM tblConceptwisePracticeQuestions
                WHERE 
                    ContentID = @ContentId AND
                    IndexTypeID = 3 AND
                    SubjectId = @SubjectId AND
                    SyllabusID = @SyllabusId AND
                    StudentId = @RegistrationId AND
                    SetID = @SetId AND
                    QuestionStatusId != 2";

                    int incorrectCount = await _connection.ExecuteScalarAsync<int>(subtopicCheckQuery, new
                    {
                        ContentId = contentId,
                        SubjectId = subjectId,
                        SyllabusId = syllabusId,
                        RegistrationId = registrationId,
                        SetId = setId
                    });

                    isAttemptComplete = incorrectCount == 0;
                }

                if (isAttemptComplete)
                    attemptCount++;
            }

            return attemptCount;
        }
    }
}
