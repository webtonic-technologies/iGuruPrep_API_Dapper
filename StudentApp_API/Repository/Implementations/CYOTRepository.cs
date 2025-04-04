using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Models;
using StudentApp_API.Repository.Interfaces;
using System.Data;

namespace StudentApp_API.Repository.Implementations
{
    public class CYOTRepository : ICYOTRepository
    {
        private readonly IDbConnection _connection; 
        public CYOTRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<List<SubjectDTO>>> GetSubjectsAsync(int registrationId)
        {
            try
            {
                // Step 1: Fetch Board, Class, and Course using RegistrationID
                var mapping = await _connection.QueryFirstOrDefaultAsync<dynamic>(
                    @"SELECT BoardId, ClassId, CourseId
              FROM tblStudentClassCourseMapping
              WHERE RegistrationID = @RegistrationID",
                    new { RegistrationID = registrationId }) ?? throw new Exception("No mapping found for the given RegistrationID.");

                // Step 2: Fetch SyllabusID using Board, Class, and Course
                var syllabusId = await _connection.QueryFirstOrDefaultAsync<int?>(
                    @"SELECT SyllabusId
              FROM tblSyllabus
              WHERE BoardID = @BoardId AND ClassID = @ClassId AND CourseID = @CourseId AND Status = 1",
                    new { mapping.BoardId, mapping.ClassId, mapping.CourseId });

                if (!syllabusId.HasValue)
                    throw new Exception("No syllabus found for the given mapping.");

                // Step 3: Fetch Subjects mapped to the SyllabusID
                var subjects = (await _connection.QueryAsync<SubjectDTO>(
                    @"SELECT SS.SubjectID, S.SubjectName
              FROM tblSyllabusSubjects SS
              INNER JOIN tblSubject S ON SS.SubjectID = S.SubjectId
              WHERE SS.SyllabusID = @SyllabusID AND SS.Status = 1 AND S.Status = 1",
                    new { SyllabusID = syllabusId.Value })).ToList();

                if (!subjects.Any())
                    return new ServiceResponse<List<SubjectDTO>>(false, "No subjects found for the given SyllabusID.", new List<SubjectDTO>(), 404);

                // Step 4: Fetch chapters for each subject and validate their mapping with the SyllabusID
                foreach (var subject in subjects)
                {
                    var chapters = await _connection.QueryAsync<string>(
                        @"SELECT CIC.ContentName_Chapter
                  FROM tblContentIndexChapters CIC
                  INNER JOIN tblSyllabusDetails SD ON CIC.ContentIndexId = SD.ContentIndexId
                  WHERE CIC.SubjectId = @SubjectId
                    AND CIC.Status = 1
                    AND CIC.IsActive = 1
                    AND SD.SyllabusID = @SyllabusID
                    AND SD.IndexTypeId = 1 -- Ensure it's a chapter",
                        new { SubjectId = subject.SubjectID, SyllabusID = syllabusId.Value });

                    subject.ChapterCount = chapters.Count();
                }

                return new ServiceResponse<List<SubjectDTO>>(true, "Records found", subjects, 200, subjects.Count);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<SubjectDTO>>(false, ex.Message, new List<SubjectDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<List<GetChaptersDTO>>> GetChaptersAsync(GetChaptersRequestCYOT request)
        {
            try
            {
                // Step 1: Fetch Board, Class, and Course using RegistrationID
                var mapping = await _connection.QueryFirstOrDefaultAsync<dynamic>(
                    @"SELECT BoardId, ClassId, CourseId
              FROM tblStudentClassCourseMapping
              WHERE RegistrationID = @RegistrationID",
                    new { RegistrationID = request.registrationId })
                    ?? throw new Exception("No mapping found for the given RegistrationID.");

                // Step 2: Fetch SyllabusID using Board, Class, and Course
                var actualSyllabusId = await _connection.QueryFirstOrDefaultAsync<int?>(
                    @"SELECT SyllabusId
              FROM tblSyllabus
              WHERE BoardID = @BoardId AND ClassID = @ClassId AND CourseID = @CourseId AND Status = 1",
                    new { mapping.BoardId, mapping.ClassId, mapping.CourseId });

                if (!actualSyllabusId.HasValue)
                    throw new Exception("Syllabus ID does not match or no syllabus found.");

                // Step 3: Fetch Chapters along with SubjectID
                var chapterResults = await _connection.QueryAsync(
                    @"SELECT S.SubjectID, 
                     C.ContentIndexId AS ChapterId, 
                     C.ContentName_Chapter AS ChapterName, 
                     C.ChapterCode, 
                     C.DisplayOrder
              FROM tblSyllabusDetails S
              INNER JOIN tblContentIndexChapters C ON S.ContentIndexId = C.ContentIndexId
              WHERE S.SyllabusID = @SyllabusID 
                AND S.SubjectID IN @SubjectIDs
                AND S.IndexTypeId = 1",
                    new { SyllabusID = actualSyllabusId, SubjectIDs = request.SubjectIds });

                if (!chapterResults.Any())
                    return new ServiceResponse<List<GetChaptersDTO>>(false, "No chapters found for the given SyllabusID and SubjectIDs.", new List<GetChaptersDTO>(), 404);

                int totalChapterCount = 0;

                // Step 4: Fetch Subject Names
                var subjects = (await _connection.QueryAsync<GetChaptersDTO>(
                    @"SELECT SubjectID AS SubjectId, SubjectName 
              FROM tblSubject 
              WHERE SubjectID IN @SubjectIDs",
                    new { SubjectIDs = request.SubjectIds })).ToList();

                // Step 5: Assign Chapters to Respective Subjects
                foreach (var subject in subjects)
                {
                    var subjectChapters = chapterResults
                        .Where(c => c.SubjectID == subject.SubjectId)
                        .Select(c => new ChapterDTO
                        {
                            ChapterId = c.ChapterId,
                            ChapterName = c.ChapterName,
                            ChapterCode = c.ChapterCode,
                            DisplayOrder = c.DisplayOrder,
                            ConceptCount = 0 // Initially set to 0
                        })
                        .ToList();

                    foreach (var chapter in subjectChapters)
                    {
                        // Count Topics
                        var topicCount = await _connection.QueryFirstOrDefaultAsync<int>(
                            @"SELECT COUNT(*)
                      FROM tblContentIndexTopics T
                      INNER JOIN tblSyllabusDetails S ON S.ContentIndexId = T.ContInIdTopic
                      WHERE T.ChapterCode = @ChapterCode 
                        AND T.Status = 1 
                        AND T.IsActive = 1
                        AND S.IndexTypeId = 2
                        AND S.Status = 1 
                        AND S.SyllabusID = @SyllabusID",
                            new { ChapterCode = chapter.ChapterCode, SyllabusID = actualSyllabusId });

                        // Count Subtopics
                        var subTopicCount = await _connection.QueryFirstOrDefaultAsync<int>(
                            @"SELECT COUNT(*)
                      FROM tblContentIndexSubTopics ST
                      INNER JOIN tblContentIndexTopics T ON ST.TopicCode = T.TopicCode
                      WHERE T.ChapterCode = @ChapterCode 
                        AND ST.Status = 1 
                        AND ST.IsActive = 1",
                            new { ChapterCode = chapter.ChapterCode });

                        chapter.ConceptCount = topicCount;//+ subTopicCount;
                        totalChapterCount += chapter.ConceptCount;
                    }

                    subject.Chapters = subjectChapters;
                }

                return new ServiceResponse<List<GetChaptersDTO>>(true, "Records found", subjects, 200, totalChapterCount);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<GetChaptersDTO>>(false, ex.Message, new List<GetChaptersDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<int>> InsertOrUpdateCYOTAsync(CYOTDTO cyot)
        {
            try
            {
                // Step 1: Validate CYOT Start Time
                //if (cyot.ChallengeStartTime <= DateTime.Now.AddMinutes(15))
                //{
                //    return new ServiceResponse<int>(false, "Challenge start time must be at least 15 minutes from the current time.", 0, 400);
                //}
                if (cyot.CYOTSyllabus == null || !cyot.CYOTSyllabus.Any() || cyot.CYOTSyllabus.Any(s => s.SubjectID <= 0 || s.ChapterID <= 0))
                {
                    throw new ArgumentException("At least one valid subject and chapter must be selected.");
                }
                if (cyot.MarksPerIncorrectAnswer > cyot.MarksPerCorrectAnswer)
                {
                    throw new ArgumentException("Marks per incorrect questions cannot be greater than marks per correct questions");
                }
                int cyotId;
                string queryMapping = @"
            SELECT TOP 1 [BoardId], [ClassID], [CourseID]
            FROM [tblStudentClassCourseMapping]
            WHERE [RegistrationID] = @RegistrationId";
                var studentMapping = await _connection.QueryFirstOrDefaultAsync<StudentData>(
               queryMapping, new { RegistrationId = cyot.CreatedBy });
                var cyotDto = new
                {
                    CYOTID = cyot.CYOTID,
                    ChallengeName = cyot.ChallengeName,
                    ChallengeDate = cyot.ChallengeDate,
                    ChallengeStartTime = cyot.ChallengeStartTime,
                    Duration = cyot.Duration,
                    NoOfQuestions = cyot.NoOfQuestions,
                    MarksPerCorrectAnswer = cyot.MarksPerCorrectAnswer,
                    MarksPerIncorrectAnswer = cyot.MarksPerIncorrectAnswer,
                    CreatedBy = cyot.CreatedBy,
                    ClassID = studentMapping.ClassId,
                    CourseID = studentMapping.CourseId,
                    BoardID = studentMapping.BoardId
                };
                // Step 2: Insert or Update `tblCYOT`
                if (cyot.CYOTID == 0)
                {
                    // Insert
                    var insertQuery = @"
            INSERT INTO tblCYOT (
                ChallengeName, ChallengeDate, ChallengeStartTime, Duration, 
                NoOfQuestions, MarksPerCorrectAnswer, MarksPerIncorrectAnswer, CreatedBy,
                ClassID, CourseID, BoardID,CreatedOn
            ) VALUES (
                @ChallengeName, @ChallengeDate, @ChallengeStartTime, @Duration, 
                @NoOfQuestions, @MarksPerCorrectAnswer, @MarksPerIncorrectAnswer, @CreatedBy,
                @ClassID, @CourseID, @BoardID,GETDATE()
            ); 
            SELECT CAST(SCOPE_IDENTITY() as int)";

                    cyotId = await _connection.ExecuteScalarAsync<int>(insertQuery, cyotDto);
                }
                else
                {
                    // Update
                    var updateQuery = @"
            UPDATE tblCYOT
            SET 
                ChallengeName = @ChallengeName, ChallengeDate = @ChallengeDate, 
                ChallengeStartTime = @ChallengeStartTime, Duration = @Duration, 
                NoOfQuestions = @NoOfQuestions, MarksPerCorrectAnswer = @MarksPerCorrectAnswer, 
                MarksPerIncorrectAnswer = @MarksPerIncorrectAnswer, CreatedBy = @CreatedBy,
                ClassID = @ClassID, CourseID = @CourseID, BoardID = @BoardID
            WHERE CYOTID = @CYOTID";

                    await _connection.ExecuteAsync(updateQuery, cyotDto);
                    cyotId = cyot.CYOTID;
                }
                // Step 3: Insert or Update `tblCYOTSyllabus`
                var deleteSyllabusQuery = "DELETE FROM tblCYOTSyllabus WHERE CYOTID = @CYOTID";
                await _connection.ExecuteAsync(deleteSyllabusQuery, new { CYOTID = cyotId });

                var insertSyllabusQuery = @"
        INSERT INTO tblCYOTSyllabus (CYOTID, SubjectID, ChapterID)
        VALUES (@CYOTID, @SubjectID, @ChapterID)";

                foreach (var syllabus in cyot.CYOTSyllabus)
                {
                    await _connection.ExecuteAsync(insertSyllabusQuery, new
                    {
                        CYOTID = cyotId,
                        SubjectID = syllabus.SubjectID,
                        ChapterID = syllabus.ChapterID
                    });
                }

                return new ServiceResponse<int>(true, "CYOT inserted/updated successfully.", cyotId, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<int>(false, $"Error: {ex.Message}", 0, 500);
            }
        }
        public async Task<ServiceResponse<bool>> UpdateCYOTSyllabusAsync(int cyotId, List<CYOTSyllabusDTO> syllabusList)
        {
            try
            {
                if (syllabusList == null || !syllabusList.Any() || syllabusList.Any(s => s.SubjectID <= 0 || s.ChapterID <= 0))
                {
                    throw new ArgumentException("At least one valid subject and chapter must be selected.");
                }
                // Step 1: Delete existing records for the CYOTID
                var deleteQuery = "DELETE FROM tblCYOTSyllabus WHERE CYOTID = @CYOTID";
                await _connection.ExecuteAsync(deleteQuery, new { CYOTID = cyotId });

                // Step 2: Insert new records
                var insertQuery = @"
            INSERT INTO tblCYOTSyllabus (CYOTID, SubjectID, ChapterID)
            VALUES (@CYOTID, @SubjectID, @ChapterID)";

                foreach (var syllabus in syllabusList)
                {
                    await _connection.ExecuteAsync(insertQuery, new
                    {
                        CYOTID = cyotId,
                        SubjectID = syllabus.SubjectID,
                        ChapterID = syllabus.ChapterID
                    });
                }

                return new ServiceResponse<bool>(true, "CYOT syllabus updated successfully.", true, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, $"Error: {ex.Message}", false, 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetCYOTQuestions(GetCYOTQuestionsRequest request)
        {
            // 1. Fetch Student Details
            var studentDetails = await _connection.QuerySingleOrDefaultAsync<StudentDetails>(
                "SELECT SCCMID, RegistrationID, CourseID, ClassID, BoardId " +
                "FROM tblStudentClassCourseMapping WHERE RegistrationID = @RegistrationID",
                new { RegistrationID = request.registrationId });

            if (studentDetails == null)
                return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);

            _connection.Open();
            // Step 2: Fetch CYOT Syllabus
            var cyotSyllabus = await _connection.QueryAsync<CYOTSubjectMapping>(
                "SELECT CYOTID as CYOTId, SubjectID, ChapterID FROM tblCYOTSyllabus WHERE CYOTID = @CYOTID",
                new { CYOTID = request.cyotId });

            if (!cyotSyllabus.Any())
                return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);

            // Step 8: Fetch CYOT details
            var cyotDetails = await _connection.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT NoOfQuestions, Duration FROM tblCYOT WHERE CYOTID = @CYOTID",
                new { CYOTID = request.cyotId });

            if (cyotDetails == null)
                return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);

            int limit = cyotDetails.NoOfQuestions;
            // Split the string and parse the numeric part
            string[] parts = cyotDetails.Duration.Split(' '); // Split by space
            int durationPerQuestion;
            if (parts.Length > 0 && int.TryParse(parts[0], out int duration))
            {
                durationPerQuestion = duration / limit;
                Console.WriteLine($"Duration per question: {durationPerQuestion} minutes");
            }
            else
            {
                throw new Exception("Invalid Duration format. Unable to parse numeric value.");
            }
            int remainingLimit = limit;
            // Step 9: Fetch questions
            var questions = await _connection.QueryAsync<QuestionResponseDTO>(
 "SELECT TOP(@Limit) q.*, qt.QuestionType AS QuestionTypeName, sub.SubjectName AS SubjectName " +
    "FROM tblQuestion q " +
    "INNER JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID " +
    "INNER JOIN tblSubject sub ON q.SubjectID = sub.SubjectId " +
    "WHERE q.ContentIndexId IN @ContentIndexIds " +
    "AND q.SubjectID IN @SubjectIds " +
    "AND q.IndexTypeId = 1 " +
    "AND q.IsConfigure = 1 " +
    "AND q.IsLive = 1 " +
    "AND q.QuestionTypeId IN @QuestionTypeIds " +
    "AND q.IsRejected = 0 " +
    "AND EXISTS (SELECT 1 FROM tblQIDCourse qc WHERE qc.QuestionCode = q.QuestionCode AND qc.LevelId IN (2,3,1) " +
    "AND qc.CourseID = @CourseID) " +
    "ORDER BY NEWID()",  // Random order
new
{
    ContentIndexIds = cyotSyllabus.Select(c => c.ChapterID),
    SubjectIds = cyotSyllabus.Select(c => c.SubjectID),
    QuestionTypeIds = new List<int> { 1, 2, 4, 5, 6, 9, 10, 11, 12, 13, 15 },
    Limit = limit,  // Use the NoOfQuestions limit from tblQuizoo
    CourseID = studentDetails.CourseID
});

     //new
     //{
     //    ContentIndexIds = filteredContent.Select(c => c.ContentIndexId),
     //    SubjectIds = filteredContent.Select(c => c.SubjectId),
     //    IndexTypeIds = filteredContent.Select(c => c.IndexTypeId),
     //    QuestionTypeIds = request.QuestionTypeId?.Any() == true ? request.QuestionTypeId : new List<int> { 1, 2, 10, 6 }, // Use provided list or default
     //    Limit = limit
     //}
    

            // Check if there are already mapped questions for the given CYOT
            string checkQuery = "SELECT COUNT(*) FROM tblCYOTQuestions WHERE CYOTID = @CYOTID";
            int existingCount = await _connection.ExecuteScalarAsync<int>(checkQuery, new { CYOTID = request.cyotId });

            if (existingCount > 0)
            {
                // Define the base query to fetch questions with their statuses
                string fetchExistingQuery = @"
    SELECT q.*,  sub.SubjectName AS SubjectName, qt.QuestionType AS QuestionTypeName,
           ISNULL(csqm.QuestionStatusId, 4) AS QuestionStatusId  -- Default to 'Not Visited' if no mapping exists
    FROM tblCYOTQuestions cyot
    INNER JOIN tblQuestion q ON cyot.QuestionID = q.QuestionId
    INNER JOIN tblSubject sub ON q.SubjectID = sub.SubjectId
    INNER JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
    LEFT JOIN tblCYOTStudentQuestionMapping csqm 
           ON cyot.CYOTID = csqm.CYOTId 
           AND q.QuestionId = csqm.QuestionId 
           AND csqm.StudentId = @RegistrationId
    WHERE cyot.CYOTID = @CYOTID";

                // Fetch the questions along with their statuses
                var questionsList = (await _connection.QueryAsync<QuestionResponseDTO>(fetchExistingQuery, new
                {
                    CYOTID = request.cyotId,
                    RegistrationId = request.registrationId
                })).ToList();

                // Apply filter on QuestionTypeId if provided
                if (request.QuestionTypeId != null && request.QuestionTypeId.Any(id => id != 0))
                {
                    questionsList = questionsList
                        .Where(q => request.QuestionTypeId.Contains(q.QuestionTypeId))
                        .ToList();
                }

                // Apply filter on QuestionStatusId if provided
                if (request.QuestionStatusId != null && request.QuestionStatusId.Any(id => id != 0))
                {
                    questionsList = questionsList
                        .Where(q => request.QuestionStatusId.Contains(q.QuestionStatusId))
                        .ToList();
                }
                // Convert the data to a list of DTOs
                var response = questionsList.Select(item =>
                {
                    if (item.QuestionTypeId == 11)
                    {
                        return new QuestionResponseDTO
                        {
                            QuestionId = item.QuestionId,
                            Paragraph = item.Paragraph,
                            SubjectName = item.SubjectName,
                            //  EmployeeName = item.EmpFirstName,
                            IndexTypeName = item.IndexTypeName,
                            ContentIndexName = item.ContentIndexName,
                            // QIDCourses = GetListOfQIDCourse(item.QuestionCode),
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
                            // EmployeeName = item.EmpFirstName,
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
                            //  QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                            //QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
                            //Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
                            //AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode)
                            MatchPairs = item.QuestionTypeId == 6 || item.QuestionTypeId == 12 ? GetMatchPairs(item.QuestionCode, item.QuestionId) : null,
                            MatchThePairType2Answers = item.QuestionTypeId == 12 ? GetMatchThePairType2Answers(item.QuestionCode, item.QuestionId) : null,
                            // Answersingleanswercategories = (item.QuestionTypeId != 6 && item.QuestionTypeId != 12) ? GetSingleAnswer(item.QuestionCode, item.QuestionId) : null,
                            AnswerMultipleChoiceCategories = (item.QuestionTypeId != 12) ? GetMultipleAnswers(item.QuestionCode) : null
                        };
                    }
                });
                return questionsList.Any()
                    ? new ServiceResponse<List<QuestionResponseDTO>>(true, "Operation Successful", questionsList.ToList(), 200, questionsList.Count())
                    : new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
                // Continue with your existing logic to process the filtered questionsList
            }
            else
            {
                var questionsResponse = new List<QuestionResponseDTO>();
                foreach (var questionData in questions)
                {
                    if (questionData.QuestionTypeId == 11)
                    {
                        var childQuestions = GetChildQuestions(questionData.QuestionCode);
                        questionData.ComprehensiveChildQuestions = childQuestions;

                        // Deduct child questions count from remaining limit
                        remainingLimit -= childQuestions.Count;
                    }
                    else
                    {
                        remainingLimit -= 1;  // Regular questions count as 1
                    }

                    questionsResponse.Add(questionData);

                    if (remainingLimit <= 0)
                        break;
                }
                // Step 10: Insert questions into tblCYOTQuestions
                var insertQuery = @"
        INSERT INTO tblCYOTQuestions (CYOTID, QuestionID, DisplayOrder)
        VALUES (@CYOTID, @QuestionID, @DisplayOrder)";
                await _connection.ExecuteAsync(insertQuery, questionsResponse.Select((q, index) => new
                {
                    CYOTID = request.cyotId,
                    QuestionID = q.QuestionId,
                    DisplayOrder = index + 1
                }));

                var studentQuestionMappingQuery = @"
        INSERT INTO tblCYOTStudentQuestionMapping (CYOTId, StudentId, QuestionId, QuestionStatusId, SubjectId)
        VALUES (@CYOTId, @StudentId, @QuestionId, @QuestionStatusId, @SubjectId)";
                await _connection.ExecuteAsync(studentQuestionMappingQuery, questionsResponse.Select((q, index) => new
                {
                    CYOTId = request.cyotId,
                    StudentId = request.registrationId,
                    QuestionId = q.QuestionId,
                    QuestionStatusId = 4,
                    SubjectId = q.subjectID
                }));
                // Convert the data to a list of DTOs
                var response = questionsResponse.Select(item =>
                {
                    if (item.QuestionTypeId == 11)
                    {
                        return new QuestionResponseDTO
                        {
                            QuestionId = item.QuestionId,
                            Paragraph = item.Paragraph,
                            SubjectName = item.SubjectName,
                            //  EmployeeName = item.EmpFirstName,
                            IndexTypeName = item.IndexTypeName,
                            ContentIndexName = item.ContentIndexName,
                            // QIDCourses = GetListOfQIDCourse(item.QuestionCode),
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
                            // EmployeeName = item.EmpFirstName,
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
                            //  QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                            //QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
                            //Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
                            //AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode)
                            MatchPairs = item.QuestionTypeId == 6 || item.QuestionTypeId == 12 ? GetMatchPairs(item.QuestionCode, item.QuestionId) : null,
                            MatchThePairType2Answers = item.QuestionTypeId == 12 ? GetMatchThePairType2Answers(item.QuestionCode, item.QuestionId) : null,
                            // Answersingleanswercategories = (item.QuestionTypeId != 6 && item.QuestionTypeId != 12) ? GetSingleAnswer(item.QuestionCode, item.QuestionId) : null,
                            AnswerMultipleChoiceCategories = (item.QuestionTypeId != 12) ? GetMultipleAnswers(item.QuestionCode) : null
                        };
                    }
                });
                return questionsResponse.Any()
                    ? new ServiceResponse<List<QuestionResponseDTO>>(true, "Operation Successful", response.ToList(), 200, questionsResponse.Count())
                    : new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
            }
        }
        public async Task<ServiceResponse<string>> UpdateQuestionNavigationAsync(CYOTQuestionNavigationRequest request)
        {
            try
            {
                string query = @"INSERT INTO tblCYOTQuestionNavigation 
                         (QuestionId, StartTime, EndTime, CYOTId, StudentId) 
                         VALUES (@QuestionId, @StartTime, @EndTime, @CYOTId, @StudentId);
                         SELECT CAST(SCOPE_IDENTITY() AS INT);";

                foreach (var subject in request.Subjects)
                {
                    foreach (var question in subject.Questions)
                    {
                        // Check if MultiOrSingleAnswerId is not null and contains at least one valid answer
                        if ((question.MultiOrSingleAnswerId != null && question.MultiOrSingleAnswerId.Any(id => id != 0)) ||
     (!string.IsNullOrEmpty(question.SubjectiveAnswers) && question.SubjectiveAnswers != "string"))
                        {
                            var data = new List<CYOTAnswerSubmissionRequest>
        {
            new CYOTAnswerSubmissionRequest
            {
                CYOTId = request.CYOTId,
                RegistrationId = request.StudentID,
                QuestionID = question.QuestionID,
                SubjectID = subject.SubjectId,
                QuestionTypeID = question.QuestionTypeID,
                SubjectiveAnswers = question.SubjectiveAnswers,
                MultiOrSingleAnswerId = question.MultiOrSingleAnswerId // Assuming question.AnswerID is a List<int>
            }
        };
                            // Submit the answer(s)
                            await SubmitCYOTAnswerAsync(data);
                        }

                        // Insert time logs regardless of whether an answer is submitted or not
                        foreach (var log in question.TimeLogs)
                        {
                            // Execute query for each time log
                            var navigationId = await _connection.ExecuteScalarAsync<int>(query, new
                            {
                                QuestionId = question.QuestionID,
                                StartTime = log.StartTime,
                                EndTime = log.EndTime,
                                CYOTId = request.CYOTId,
                                StudentId = request.StudentID
                            });
                        }
                        var updateQuestionStatusQuery = @"
    UPDATE [tblCYOTStudentQuestionMapping] 
    SET QuestionStatusId = @QuestionStatusId 
    WHERE QuestionId = @QuestionId 
      AND CYOTId = @CYOTId 
      AND StudentId = @RegistrationId 
      AND SubjectId = @SubjectID;";

                        await _connection.ExecuteAsync(updateQuestionStatusQuery, new
                        {
                            QuestionId = question.QuestionID,
                            CYOTId = request.CYOTId,
                            RegistrationId = request.StudentID,
                            SubjectId = subject.SubjectId,
                            QuestionStatusId = question.QuestionstatusId
                        });
                    }
                }

                // Step 4: Calculate marks gained by the student and total marks
                var marksQuery = @"
SELECT 
    SUM(CASE WHEN A.IsCorrect = 1 THEN CYOT.MarksPerCorrectAnswer 
             ELSE -CYOT.MarksPerIncorrectAnswer END) AS MarksObtained,
    COUNT(A.QuestionID) AS AttemptedQuestions,
    (SELECT COUNT(*) FROM tblCYOTQuestions WHERE CYOTID = @CYOTID) AS TotalQuestions,
    (SELECT COUNT(*) FROM tblCYOTQuestions WHERE CYOTID = @CYOTID) * MAX(CYOT.MarksPerCorrectAnswer) AS TotalMarks
FROM tblCYOTAnswers AS A
JOIN tblCYOT AS CYOT ON A.CYOTID = CYOT.CYOTID
WHERE A.CYOTID = @CYOTID AND A.StudentID = @StudentID
GROUP BY CYOT.MarksPerCorrectAnswer, CYOT.MarksPerIncorrectAnswer;";

                var marksResult = await _connection.QueryFirstOrDefaultAsync<MarksResult>(marksQuery, new
                {
                    CYOTID = request.CYOTId,
                    StudentID = request.StudentID
                });

                var marksObtained = marksResult.MarksObtained;
                var totalMarks = marksResult.TotalMarks;

                // Calculate the percentage
                var percentage = totalMarks > 0 ? (marksObtained * 100) / totalMarks : 0;
                var statusId = percentage >= 80 ? 3 : 2; // 3 = Challenge Open, 2 = Completed
                int owner = await _connection.QueryFirstOrDefaultAsync<int>(@"select CreatedBy from tblCYOT where CYOTID = @cyotid", new { cyotid = request.CYOTId });
                if(request.StudentID == owner)
                {
                    // Update the CYOT status
                    await _connection.ExecuteAsync(
                        @"UPDATE [tblCYOT] 
      SET CYOTStatusID = @StatusID 
      WHERE CYOTID = @CYOTID",
                        new
                        {
                            CYOTID = request.CYOTId,
                            StatusID = statusId
                        }
                    );
                }
                // Include marks in the response message
                return new ServiceResponse<string>(
                    true,
                    $"Navigation updated successfully. Marks Obtained: {marksObtained}, Total Marks: {totalMarks}, Percentage: {percentage}%",
                    "All records inserted successfully.",
                    200
                );

            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<CYOTDTO>> GetCYOTByIdAsync(int cyotId)
        {
            try
            {
                var query = @"
        SELECT 
            CYOTID,
            ChallengeName,
            ChallengeDate,
            ChallengeStartTime,
            Duration,
            NoOfQuestions,
            MarksPerCorrectAnswer,
            MarksPerIncorrectAnswer,
            CreatedBy,
            ClassID,
            CourseID,
            BoardID
        FROM tblCYOT
        WHERE CYOTID = @CYOTID";

                var cyot = await _connection.QueryFirstOrDefaultAsync<CYOTDTO>(query, new { CYOTID = cyotId });

                if (cyot == null)
                {
                    return new ServiceResponse<CYOTDTO>(false, "CYOT record not found.", null, 404);
                }
                // Query to fetch associated syllabus details
                var syllabusQuery = @"
            SELECT 
                cs.CYOTID, cs.SubjectID, cs.ChapterID,
                s.SubjectName, c.ContentName_Chapter
            FROM tblCYOTSyllabus cs
            INNER JOIN tblSubject s ON cs.SubjectID = s.SubjectID
            INNER JOIN tblContentIndexChapters c ON cs.ChapterID = c.ContentIndexId
            WHERE cs.CYOTID = @CYOTID";

                var syllabus = await _connection.QueryAsync<CYOTSyllabusDTO>(syllabusQuery, new { CYOTID = cyotId });

                cyot.CYOTSyllabus = syllabus.ToList();
                return new ServiceResponse<CYOTDTO>(true, "CYOT record retrieved successfully.", cyot, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<CYOTDTO>(false, $"Error: {ex.Message}", null, 500);
            }
        }
        public async Task<ServiceResponse<string>> SubmitCYOTAnswerAsync(List<CYOTAnswerSubmissionRequest> request)
        {

            try
            {
                // Step 2: Fetch CYOT marks per correct and incorrect answer
                var cyotMarksQuery = @"
        SELECT MarksPerCorrectAnswer, MarksPerIncorrectAnswer
        FROM tblCYOT
        WHERE CYOTID = @CYOTID";

                var cyotMarks = await _connection.QueryFirstOrDefaultAsync<(int MarksPerCorrect, int MarksPerIncorrect)>(
                    cyotMarksQuery,
                    new { CYOTID = request.First().CYOTId }
                );
                foreach (var answer in request)
                {
                  
                    string answerStatus = "Incorrect";
                    bool isCorrect = false; // ✅ Initialize isCorrect
                    decimal marks = 0; // ✅ Initialize marks

                    // Fetch question and answer details
                    var query = @"
                SELECT 
                    q.QuestionId, 
                    q.QuestionTypeId, 
                    s.MarksPerQuestion, 
                    s.NegativeMarks
                FROM tblQuestion q
                WHERE q.QuestionId = @QuestionID";

                    var questionData = await _connection.QueryFirstOrDefaultAsync<QuestionAnswerData>(query, new
                    {
                        QuestionID = answer.QuestionID
                    });
                    // Handle single answer types
                    if (IsSingleAnswerType(questionData.QuestionTypeId))
                    {
                        if (answer.QuestionTypeID == 4 || answer.QuestionTypeID == 9)
                        {
                            var singleAnswerQuery = @"
    SELECT sac.Answer
    FROM tblAnswersingleanswercategory sac
    INNER JOIN tblAnswerMaster am ON sac.Answerid = am.Answerid
    WHERE am.Questionid = @QuestionID";

                            var correctAnswerText = await _connection.QueryFirstOrDefaultAsync<string>(singleAnswerQuery, new { QuestionID = answer.QuestionID });

                            if (answer.QuestionTypeID == 4) // Text comparison (case-insensitive)
                            {
                                 isCorrect = string.Equals(correctAnswerText, answer.SubjectiveAnswers?.Trim(), StringComparison.OrdinalIgnoreCase);

                                marks = isCorrect ? questionData.MarksPerQuestion : -questionData.NegativeMarks;
                                answerStatus = isCorrect ? "Correct" : "Incorrect";
                            }
                            else if (answer.QuestionTypeID == 9) // Numerical comparison
                            {
                                 isCorrect = decimal.TryParse(correctAnswerText, out var correctValue) &&
                                                 decimal.TryParse(answer.SubjectiveAnswers?.Trim(), out var studentValue) &&
                                                 correctValue == studentValue;

                                marks = isCorrect ? questionData.MarksPerQuestion : -questionData.NegativeMarks;
                                answerStatus = isCorrect ? "Correct" : "Incorrect";
                            }
                        }
                    }
                    else if (answer.QuestionTypeID == 13 || answer.QuestionTypeID == 15)
                    {
                        var multiAnswerQuery = @"
                    SELECT amc.Answer
                    FROM tblAnswerMultipleChoiceCategory amc
                    INNER JOIN tblAnswerMaster am ON amc.Answerid = am.Answerid
                    WHERE am.Questionid = @QuestionID AND amc.IsCorrect = 1";

                        var correctAnswerTexts = await _connection.QueryAsync<string>(multiAnswerQuery, new { QuestionID = answer.QuestionID });
                        var correctAnswers = correctAnswerTexts.ToList();

                        var studentAnswers = answer.SubjectiveAnswers?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                                    .Select(s => s.Trim())
                                                                    .ToList() ?? new List<string>();

                        int actualCorrectCount = correctAnswers.Count;
                        int studentCorrectCount = 0;
                        bool isNegative = false;
                        // Check correctness and order
                        for (int i = 0; i < Math.Min(correctAnswers.Count, studentAnswers.Count); i++)
                        {
                            if (string.Equals(correctAnswers[i], studentAnswers[i], StringComparison.OrdinalIgnoreCase))
                            {
                                studentCorrectCount++;
                            }
                            else
                            {
                                isNegative = true;
                                break;
                            }
                        }
                        answerStatus = actualCorrectCount == studentCorrectCount ? "Correct" : "Incorrect";
                    }
                    // Handle multiple-answer types
                    else
                    {
                        var multipleAnswersQuery = @"
                    SELECT amc.Answermultiplechoicecategoryid
                    FROM tblAnswerMultipleChoiceCategory amc
                    INNER JOIN tblAnswerMaster am ON amc.Answerid = am.Answerid
                    WHERE am.Questionid = @QuestionID AND amc.IsCorrect = 1";

                        var correctAnswers = await _connection.QueryAsync<int>(multipleAnswersQuery, new { QuestionID = answer.QuestionID });

                        var actualCorrectCount = correctAnswers.Count();

                        // Check if any of the submitted answers are incorrect
                        bool hasIncorrectAnswer = answer.MultiOrSingleAnswerId.Any(submittedAnswer => !correctAnswers.Contains(submittedAnswer));

                        // Calculate studentCorrectCount based on the correctness of the answers
                        int studentCorrectCount = hasIncorrectAnswer ? -1 : answer.MultiOrSingleAnswerId.Intersect(correctAnswers).Count();

                        // Determine if negative marking applies
                        bool isNegative = hasIncorrectAnswer;

                        if (actualCorrectCount == studentCorrectCount && answer.MultiOrSingleAnswerId.All(correctAnswers.Contains))
                        {
                            marks = questionData.MarksPerQuestion;
                            answerStatus = "Correct";
                        }
                        else
                        {
                            marks = -questionData.NegativeMarks;
                            answerStatus = "Incorrect";
                        }
                    }

                    var existingAnswerQuery = @"
            SELECT COUNT(1)
            FROM tblCYOTAnswers
            WHERE CYOTID = @CYOTID AND QuestionID = @QuestionID AND StudentID = @StudentID";

                    var hasExistingAnswer = await _connection.ExecuteScalarAsync<bool>(
                        existingAnswerQuery,
                        new
                        {
                            CYOTID = answer.CYOTId,
                            QuestionID = answer.QuestionID,
                            StudentID = answer.RegistrationId
                        }
                    );

                    if (hasExistingAnswer)
                    {
                        // Update existing answer
                        var updateQuery = @"
                UPDATE tblCYOTAnswers
                SET AnswerId = @AnswerId, IsCorrect = @IsCorrect, 
                    SubjectId = @SubjectId, QuestionTypeId = @QuestionTypeId, AnswerStatus = @AnswerStatus, 
                    Marks = @Marks, Answer = @Answer
                WHERE CYOTID = @CYOTID AND QuestionID = @QuestionID AND StudentID = @StudentID";

                        await _connection.ExecuteAsync(
                            updateQuery,
                            new
                            {
                                CYOTID = answer.CYOTId,
                                StudentID = answer.RegistrationId,
                                QuestionID = answer.QuestionID,
                                AnswerId = string.Join(",", answer.MultiOrSingleAnswerId),
                                IsCorrect = isCorrect,
                                SubjectId = answer.SubjectID,
                                QuestionTypeId = answer.QuestionTypeID,
                                AnswerStatus = answerStatus,
                                Marks = marks,
                                Answer = answer.SubjectiveAnswers
                            }
                        );
                    }
                    else
                    {
                        // Insert new answer
                        var insertQuery = @"
                INSERT INTO tblCYOTAnswers (CYOTID, StudentID, QuestionID, IsCorrect, SubjectId, QuestionTypeId, AnswerId, AnswerStatus, Marks, Answer)
                VALUES (@CYOTID, @StudentID, @QuestionID, @IsCorrect, @SubjectId, @QuestionTypeId, @AnswerId, @AnswerStatus, @Marks, @Answer)";

                        await _connection.ExecuteAsync(
                            insertQuery,
                            new
                            {
                                CYOTID = answer.CYOTId,
                                StudentID = answer.RegistrationId,
                                QuestionID = answer.QuestionID,
                                AnswerId = string.Join(",", answer.MultiOrSingleAnswerId),
                                IsCorrect = isCorrect,
                                SubjectId = answer.SubjectID,
                                QuestionTypeId = answer.QuestionTypeID,
                                AnswerStatus = answerStatus,
                                Marks = marks,
                                Answer = answer.SubjectiveAnswers
                            }
                        );
                    }
                }
                return new ServiceResponse<string>(
                    true,
                    "Operation successful",
                    string.Empty,
                    200
                );

            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(
                    false,
                    $"Error: {ex.Message}",
                    null,
                    500
                );
            }
        }
        public async Task<ServiceResponse<List<CYOTQuestionWithAnswersDTO>>> GetCYOTQuestionsWithOptionsAsync(GetCYOTQuestionsRequest request)
        {
            // SQL query to fetch questions with all answer options and student's given answers
            var query = @"
SELECT 
    cq.CYOTID,
    cq.QuestionID,
    q.QuestionCode,
    q.QuestionDescription,
    q.Explanation, 
    q.ExtraInformation,
    q.QuestionTypeId,
    mc.Answermultiplechoicecategoryid,
    mc.Answer,
    mc.IsCorrect,
    sqm.QuestionStatusId,
    COALESCE(a.AnswerId, 0) AS StudentAnswerId,   -- Student's given answer ID (if any)
    COALESCE(a.IsCorrect, 0) AS IsStudentAnswerCorrect -- Whether the student's answer was correct (if any)
FROM 
    tblCYOTQuestions cq
JOIN 
    tblQuestion q ON cq.QuestionID = q.QuestionId
JOIN 
    tblAnswerMaster am ON q.QuestionId = am.Questionid
JOIN 
    tblAnswerMultipleChoiceCategory mc ON am.Answerid = mc.Answerid
LEFT JOIN 
    [tblCYOTStudentQuestionMapping] sqm ON cq.QuestionID = sqm.QuestionID AND sqm.StudentID = @StudentID AND sqm.CYOTID = @CYOTID
LEFT JOIN 
    tblCYOTAnswers a ON cq.QuestionID = a.QuestionID AND a.StudentID = @StudentID AND a.CYOTID = @CYOTID
WHERE 
    cq.CYOTID = @CYOTID
ORDER BY 
    cq.DisplayOrder";

            // Execute the query and fetch the results
            var rawData = await _connection.QueryAsync<dynamic>(query, new { CYOTID = request.cyotId, StudentID = request.registrationId });

            // Group data by QuestionID to include answers in a nested structure
            var groupedData = rawData.GroupBy(
                item => new
                {
                    CYOTID = (int)item.CYOTID,
                    QuestionID = (int)item.QuestionID,
                    QuestionCode = (string)item.QuestionCode,
                    QuestionDescription = (string)item.QuestionDescription,
                    QuestionStatusId = (int)item.QuestionStatusId,
                    QuestionTypeId = (int)item.QuestionTypeId
                },
           (key, answers) => new CYOTQuestionWithAnswersDTO
           {
               CYOTID = key.CYOTID,
               QuestionID = key.QuestionID,
               QuestionCode = key.QuestionCode,
               QuestionDescription = key.QuestionDescription,
               QuestionStatusId = key.QuestionStatusId,
               QuestionTypeId = key.QuestionTypeId,
               Answers = answers.Select(answer => new AnswerOptionDTO
               {
                   AnswerMultipleChoiceCategoryID = (int)answer.Answermultiplechoicecategoryid,
                   Answer = (string)answer.Answer,
                   IsCorrect = (bool)answer.IsCorrect,
                   IsStudentAnswer = ((int)answer.Answermultiplechoicecategoryid == (int)answer.StudentAnswerId), // Corrected here
                   IsStudentAnswerCorrect = ((int)answer.Answermultiplechoicecategoryid == (int)answer.StudentAnswerId) && (bool)answer.IsCorrect // Fixed here
               }).ToList()
           }).ToList();
            if (request.QuestionTypeId != null && request.QuestionTypeId.Any(id => id != 0))
            {
                groupedData = groupedData
                    .Where(q => request.QuestionTypeId.Contains(q.QuestionTypeId))
                    .ToList();
            }

            // Apply filter on QuestionStatusId if provided
            if (request.QuestionStatusId != null && request.QuestionStatusId.Any(id => id != 0))
            {
                groupedData = groupedData
                    .Where(q => request.QuestionStatusId.Contains(q.QuestionStatusId))
                    .ToList();
            }
            // Return the grouped data
            return new ServiceResponse<List<CYOTQuestionWithAnswersDTO>>(
                true,
                "Operation successful",
                groupedData,
                200, groupedData.Count
            );
        }
        public async Task<ServiceResponse<string>> MarkQuestionAsSave(SaveQuestionCYOTRequest request)
        {
            try
            {
                // Check if the record already exists
                string checkQuery = @"
            SELECT COUNT(1)
            FROM tblCYOTQuestionSave
            WHERE StudentID = @RegistrationId
              AND QuestionID = @QuestionId
              AND QuestionCode = @QuestionCode AND CYOTId = @CYOTId";

                int recordExists = await _connection.ExecuteScalarAsync<int>(checkQuery, new
                {
                    request.RegistrationId,
                    request.QuestionId,
                    request.QuestionCode,
                    request.CYOTId
                });

                if (recordExists > 0)
                {
                    // If record exists, delete it
                    string deleteQuery = @"
                DELETE FROM tblCYOTQuestionSave
                WHERE StudentID = @RegistrationId
                  AND QuestionID = @QuestionId
                  AND QuestionCode = @QuestionCode AND CYOTId = @CYOTId";

                    await _connection.ExecuteAsync(deleteQuery, new
                    {
                        request.RegistrationId,
                        request.QuestionId,
                        request.QuestionCode, request.CYOTId
                    });

                    return new ServiceResponse<string>(true, "Question unsaved (deleted).", null, 200);
                }
                else
                {
                    // If record does not exist, insert it
                    string insertQuery = @"
                INSERT INTO tblCYOTQuestionSave (StudentID, QuestionID, QuestionCode,SubjectId, CYOTId)
                VALUES (@RegistrationId, @QuestionId, @QuestionCode,@SubjectId, @CYOTId)";

                    await _connection.ExecuteAsync(insertQuery, new
                    {
                        request.RegistrationId,
                        request.QuestionId,
                        request.QuestionCode,
                        request.SubjectId,
                        request.CYOTId
                    });

                    return new ServiceResponse<string>(true, "Question saved (inserted).", null, 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, $"An error occurred: {ex.Message}", null, 500);
            }
        }
        public async Task<ServiceResponse<string>> ShareQuestionAsync(int studentId, int questionId, int CYOTId)
        {
            try
            {
                // Step 1: Fetch board, class, and course details for the given student.
                string mappingQuery = @"
            SELECT BoardId, ClassID, CourseID
            FROM tblStudentClassCourseMapping
            WHERE RegistrationID = @StudentId";

                var studentMapping = await _connection.QueryFirstOrDefaultAsync<dynamic>(
                    mappingQuery, new { StudentId = studentId });

                if (studentMapping == null)
                {
                    return new ServiceResponse<string>(false, "Student mapping not found.", string.Empty, 404);
                }

                int boardId = studentMapping.BoardId;
                int classId = studentMapping.ClassID;
                int courseId = studentMapping.CourseID;

                // Step 2: Find classmates (students with the same board, class, and course, excluding the given student)
                string classmatesQuery = @"
            SELECT RegistrationID
            FROM tblStudentClassCourseMapping
            WHERE BoardId = @BoardId
              AND ClassID = @ClassID
              AND CourseID = @CourseID
              AND RegistrationID <> @StudentId";

                var classmates = (await _connection.QueryAsync<int>(classmatesQuery, new
                {
                    BoardId = boardId,
                    ClassID = classId,
                    CourseID = courseId,
                    StudentId = studentId
                })).ToList();

                if (classmates == null || !classmates.Any())
                {
                    return new ServiceResponse<string>(false, "No classmates found.", string.Empty, 404);
                }

                // Step 3: Insert a shared question record for each classmate
                string insertQuery = @"
            INSERT INTO tblCYOTSharedQuestions (CYOTId, QuestionId, SharedBy, SharedTo)
            VALUES (@CYOTId, @QuestionId, @SharedBy, @SharedTo)";

                int totalInserted = 0;
                foreach (var classmateId in classmates)
                {
                    int rows = await _connection.ExecuteAsync(insertQuery, new
                    {
                        CYOTId = CYOTId,
                        QuestionId = questionId,
                        SharedBy = studentId,
                        SharedTo = classmateId
                    });
                    totalInserted += rows;
                }

                var data = GetQuestionById(questionId);

                return new ServiceResponse<string>(true, "Question shared successfully.", $"Shared with {classmates.Count} classmates", 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }

        }
        public async Task<ServiceResponse<CYOTQestionReportResponse>> GetCYOTQestionReportAsync(int studentId, int cyotId)
        {
            var query = @"
    SELECT 
        CYOT.NoOfQuestions AS TotalQuestions,
        CAST(CYOT.Duration AS VARCHAR) + ' min' AS TotalDuration,
        CYOT.NoOfQuestions * CYOT.MarksPerCorrectAnswer AS TotalMarks,
        COUNT(CASE WHEN A.IsCorrect = 1 THEN 1 END) AS CorrectCount,
        COUNT(CASE WHEN A.IsCorrect = 0 THEN 1 END) AS IncorrectCount,
        (CYOT.NoOfQuestions - COUNT(A.QuestionID)) AS UnansweredCount
    FROM tblCYOT AS CYOT
    LEFT JOIN tblCYOTAnswers AS A ON CYOT.CYOTID = A.CYOTID AND A.StudentID = @StudentID
    WHERE CYOT.CYOTID = @CYOTID
    GROUP BY CYOT.NoOfQuestions, CYOT.Duration, CYOT.MarksPerCorrectAnswer;";

            var result = await _connection.QueryFirstOrDefaultAsync<CYOTQestionReportResponse>(query, new
            {
                CYOTID = cyotId,
                StudentID = studentId
            });

            if (result != null)
            {
                result.CorrectPercentage = result.TotalQuestions > 0 ? Math.Round((decimal)result.CorrectCount / result.TotalQuestions * 100, 2) : 0;
                result.IncorrectPercentage = result.TotalQuestions > 0 ? Math.Round((decimal)result.IncorrectCount / result.TotalQuestions * 100, 2) : 0;
                result.UnansweredPercentage = result.TotalQuestions > 0 ? Math.Round((decimal)result.UnansweredCount / result.TotalQuestions * 100, 2) : 0;
            }

            return new ServiceResponse<CYOTQestionReportResponse>(
                true,
                "Report fetched successfully",
                result,
                200
            );
        }
        public async Task<ServiceResponse<CYOTAnalyticsResponse>> GetCYOTAnalyticsAsync(int studentId, int cyotId)
        {
            try
            {
                var query = @"
    SELECT 
        SUM(CASE WHEN A.IsCorrect = 1 THEN CYOT.MarksPerCorrectAnswer ELSE 0 END) AS AchievedMarks,
        SUM(CASE WHEN A.IsCorrect = 0 THEN CYOT.MarksPerIncorrectAnswer ELSE 0 END) AS NegativeMarks,
        (SELECT COUNT(*) * MAX(CYOT.MarksPerCorrectAnswer) 
         FROM tblCYOTQuestions 
         WHERE CYOTID = @CYOTID) AS TotalMarks
    FROM tblCYOTAnswers AS A
    JOIN tblCYOT AS CYOT ON A.CYOTID = CYOT.CYOTID
    WHERE A.CYOTID = @CYOTID AND A.StudentID = @StudentID;";

                var result = await _connection.QueryFirstOrDefaultAsync<dynamic>(query, new
                {
                    CYOTID = cyotId,
                    StudentID = studentId
                });

                if (result != null)
                {
                    var response = new CYOTAnalyticsResponse
                    {
                        AchievedMarks = (decimal)(result.AchievedMarks ?? 0),
                        NegativeMarks = (decimal)(result.NegativeMarks ?? 0),
                        FinalMarks = (decimal)(result.AchievedMarks ?? 0) - (decimal)(result.NegativeMarks ?? 0),
                        //FinalPercentage = (decimal)result.TotalMarks > 0 ? Math.Round(((decimal)result.AchievedMarks - (decimal)result.NegativeMarks) / (decimal)result.TotalMarks * 100, 2) : 0
                        FinalPercentage = (decimal)(result.TotalMarks ?? 0) > 0
                        ? Math.Round(((decimal)(result.AchievedMarks ?? 0) - (decimal)(result.NegativeMarks ?? 0))/ (decimal)(result.TotalMarks ?? 1) * 100, 2): 0
                };

                    return new ServiceResponse<CYOTAnalyticsResponse>(
                        true,
                        "Analytics data fetched successfully",
                        response,
                        200
                    );
                }

                return new ServiceResponse<CYOTAnalyticsResponse>(
                    false,
                    "No analytics data found",
                    null,
                    404
                );
            }
            catch (Exception ex)
            {
                return new ServiceResponse<CYOTAnalyticsResponse>(
                    false,
                    ex.Message,
                    null,
                    500
                );
            }
        }
        public async Task<ServiceResponse<CYOTTimeAnalytics>> GetCYOTTimeAnalyticsAsync(int studentId, int cyotId)
        {
            try
            {
                var query = @"
SELECT 
    -- Total time spent (in seconds)
    SUM(DATEDIFF(SECOND, N.StartTime, N.EndTime)) / 60.0 AS TotalTimeSpent,
    -- Average time per question (in seconds)
    AVG(DATEDIFF(SECOND, N.StartTime, N.EndTime)) / 60.0 AS AvgTimePerQuestion,

    -- Time spent on correct answers
    SUM(CASE WHEN A.IsCorrect = 1 THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE 0 END) / 60.0 AS TotalTimeSpentCorrect,
    -- Average time for correct answers
    AVG(CASE WHEN A.IsCorrect = 1 THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE NULL END) / 60.0 AS AvgTimeSpentCorrect,

    -- Time spent on incorrect answers
    SUM(CASE WHEN A.IsCorrect = 0 THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE 0 END) / 60.0 AS TotalTimeSpentWrong,
    -- Average time for incorrect answers
    AVG(CASE WHEN A.IsCorrect = 0 THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE NULL END) / 60.0 AS AvgTimeSpentWrong,

    -- Time spent on unattempted questions (Status ID 2 and 4)
    SUM(CASE WHEN SQM.QuestionStatusId IN (2, 4) THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE 0 END) / 60.0 AS TotalTimeSpentUnattempted,
    -- Average time for unattempted questions
    AVG(CASE WHEN SQM.QuestionStatusId IN (2, 4) THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE NULL END) / 60.0 AS AvgTimeSpentUnattempted

FROM tblCYOTQuestionNavigation AS N
LEFT JOIN tblCYOTAnswers AS A ON N.QuestionId = A.QuestionID AND N.StudentId = A.StudentID AND N.CYOTId = A.CYOTID
LEFT JOIN tblCYOTStudentQuestionMapping AS SQM ON N.QuestionId = SQM.QuestionId AND N.StudentId = SQM.StudentId AND N.CYOTId = SQM.CYOTId

WHERE N.CYOTId = @CYOTId AND N.StudentId = @StudentId;";

                var result = await _connection.QueryFirstOrDefaultAsync<dynamic>(query, new
                {
                    CYOTID = cyotId,
                    StudentID = studentId
                });

                if (result != null)
                {
                    var response = new CYOTTimeAnalytics
                    {
                        TotalTimeSpent = ConvertSecondsToTimeFormat((int)Math.Floor(result.TotalTimeSpent ?? 0)),
                        AvgTimePerQuestion = ConvertSecondsToTimeFormat((int)Math.Floor(result.AvgTimePerQuestion ?? 0)),

                        TotalTimeSpentCorrect = ConvertSecondsToTimeFormat((int)Math.Floor(result.TotalTimeSpentCorrect ?? 0)),
                        AvgTimeSpentCorrect = ConvertSecondsToTimeFormat((int)Math.Floor(result.AvgTimeSpentCorrect ?? 0)),

                        TotalTimeSpentWrong = ConvertSecondsToTimeFormat((int)Math.Floor(result.TotalTimeSpentWrong ?? 0)),
                        AvgTimeSpentWrong = ConvertSecondsToTimeFormat((int)Math.Floor(result.AvgTimeSpentWrong ?? 0)),

                        TotalTimeSpentUnattempted = ConvertSecondsToTimeFormat((int)Math.Floor(result.TotalTimeSpentUnattempted ?? 0)),
                        AvgTimeSpentUnattempted = ConvertSecondsToTimeFormat((int)Math.Floor(result.AvgTimeSpentUnattempted ?? 0))
                    };

                    return new ServiceResponse<CYOTTimeAnalytics>(
                        true,
                        "Time analytics fetched successfully",
                        response,
                        200
                    );
                }


                return new ServiceResponse<CYOTTimeAnalytics>(
                    false,
                    "No analytics data found",
                    null,
                    404
                );
            }
            catch (Exception ex)
            {
                return new ServiceResponse<CYOTTimeAnalytics>(
                    false,
                    ex.Message,
                    null,
                    500
                );
            }
        }
        //analytics per subject
        public async Task<ServiceResponse<CYOTQestionReportResponse>> GetCYOTQestionReportBySubjectAsync(int cyotId, int studentId, int subjectId)
        {
            var query = @"
SELECT 
    S.SubjectName,
    CYOT.NoOfQuestions AS TotalQuestions,
    CYOT.Duration AS TotalDuration,
    CYOT.NoOfQuestions * CYOT.MarksPerCorrectAnswer AS TotalMarks,
    COUNT(CASE WHEN A.IsCorrect = 1 THEN 1 END) AS CorrectCount,
    COUNT(CASE WHEN A.IsCorrect = 0 THEN 1 END) AS IncorrectCount,
    (CYOT.NoOfQuestions - COUNT(A.QuestionID)) AS UnansweredCount
FROM tblCYOT AS CYOT
LEFT JOIN tblCYOTAnswers AS A ON CYOT.CYOTID = A.CYOTID AND A.StudentID = @StudentID
LEFT JOIN tblSubject AS S ON A.SubjectID = S.SubjectID
WHERE CYOT.CYOTID = @CYOTID AND S.SubjectID = @SubjectID
GROUP BY S.SubjectName, CYOT.NoOfQuestions, CYOT.Duration, CYOT.MarksPerCorrectAnswer;";

            var result = await _connection.QueryFirstOrDefaultAsync<CYOTQestionReportResponse>(query, new
            {
                CYOTID = cyotId,
                StudentID = studentId,
                SubjectID = subjectId
            });

            if (result != null)
            {
                result.CorrectPercentage = result.TotalQuestions > 0 ? Math.Round((decimal)result.CorrectCount / result.TotalQuestions * 100, 2) : 0;
                result.IncorrectPercentage = result.TotalQuestions > 0 ? Math.Round((decimal)result.IncorrectCount / result.TotalQuestions * 100, 2) : 0;
                result.UnansweredPercentage = result.TotalQuestions > 0 ? Math.Round((decimal)result.UnansweredCount / result.TotalQuestions * 100, 2) : 0;
            }

            return new ServiceResponse<CYOTQestionReportResponse>(
                true,
                "Subject-wise report fetched successfully",
                result,
                200
            );
        }
        public async Task<ServiceResponse<CYOTAnalyticsResponse>> GetCYOTAnalyticsBySubjectAsync(int cyotId, int studentId, int subjectId)
        {
            try
            {
                var query = @"
SELECT 
    S.SubjectName,
    SUM(CASE WHEN A.IsCorrect = 1 THEN CYOT.MarksPerCorrectAnswer ELSE 0 END) AS AchievedMarks,
    SUM(CASE WHEN A.IsCorrect = 0 THEN CYOT.MarksPerIncorrectAnswer ELSE 0 END) AS NegativeMarks,
    (SELECT COUNT(*) * MAX(CYOT.MarksPerCorrectAnswer) 
     FROM tblCYOTQuestions AS Q
     WHERE Q.CYOTID = @CYOTID) AS TotalMarks
FROM tblCYOTAnswers AS A
JOIN tblCYOT AS CYOT ON A.CYOTID = CYOT.CYOTID
JOIN tblSubject AS S ON A.SubjectID = S.SubjectID
WHERE A.CYOTID = @CYOTID AND A.StudentID = @StudentID AND A.SubjectID = @SubjectID
GROUP BY S.SubjectName;";

                var result = await _connection.QueryFirstOrDefaultAsync<dynamic>(query, new
                {
                    CYOTID = cyotId,
                    StudentID = studentId,
                    SubjectID = subjectId
                });

                if (result != null)
                {
                    var response = new CYOTAnalyticsResponse
                    {
                        AchievedMarks = (decimal)result.AchievedMarks,
                        NegativeMarks = (decimal)result.NegativeMarks,
                        FinalMarks = (decimal)result.AchievedMarks - (decimal)result.NegativeMarks,
                        FinalPercentage = (decimal)result.TotalMarks > 0 ? Math.Round(((decimal)result.AchievedMarks - (decimal)result.NegativeMarks) / (decimal)result.TotalMarks * 100, 2) : 0
                    };

                    return new ServiceResponse<CYOTAnalyticsResponse>(
                        true,
                        "Subject-wise analytics data fetched successfully",
                        response,
                        200
                    );
                }

                return new ServiceResponse<CYOTAnalyticsResponse>(
                    false,
                    "No analytics data found",
                    null,
                    404
                );
            }
            catch (Exception ex)
            {
                return new ServiceResponse<CYOTAnalyticsResponse>(
                    false,
                    ex.Message,
                    null,
                    500
                );
            }
        }
        public async Task<ServiceResponse<CYOTTimeAnalytics>> GetCYOTTimeAnalyticsBySubjectAsync(int cyotId, int studentId, int subjectId)
        {
            try
            {
                var query = @"
SELECT 
    -- Total time spent (in seconds)
    SUM(DATEDIFF(SECOND, N.StartTime, N.EndTime)) / 60.0 AS TotalTimeSpent,
    -- Average time per question (in seconds)
    AVG(DATEDIFF(SECOND, N.StartTime, N.EndTime)) / 60.0 AS AvgTimePerQuestion,

    -- Time spent on correct answers
    SUM(CASE WHEN A.IsCorrect = 1 THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE 0 END) / 60.0 AS TotalTimeSpentCorrect,
    -- Average time for correct answers
    AVG(CASE WHEN A.IsCorrect = 1 THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE NULL END) / 60.0 AS AvgTimeSpentCorrect,

    -- Time spent on incorrect answers
    SUM(CASE WHEN A.IsCorrect = 0 THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE 0 END) / 60.0 AS TotalTimeSpentWrong,
    -- Average time for incorrect answers
    AVG(CASE WHEN A.IsCorrect = 0 THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE NULL END) / 60.0 AS AvgTimeSpentWrong,

    -- Time spent on unattempted questions (Status ID 2 and 4)
    SUM(CASE WHEN SQM.QuestionStatusId IN (2, 4) THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE 0 END) / 60.0 AS TotalTimeSpentUnattempted,
    -- Average time for unattempted questions
    AVG(CASE WHEN SQM.QuestionStatusId IN (2, 4) THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE NULL END) / 60.0 AS AvgTimeSpentUnattempted

FROM tblCYOTQuestionNavigation AS N
LEFT JOIN tblCYOTAnswers AS A ON N.QuestionId = A.QuestionID AND N.StudentId = A.StudentID AND N.CYOTId = A.CYOTID
LEFT JOIN tblCYOTStudentQuestionMapping AS SQM ON N.QuestionId = SQM.QuestionId AND N.StudentId = SQM.StudentId AND N.CYOTId = SQM.CYOTId

WHERE N.CYOTId = @CYOTId AND N.StudentId = @StudentId  AND SQM.SubjectID = @SubjectID;";
                //                var query = @"
                //SELECT 
                //    -- Total time spent (in minutes)
                //    SUM(DATEDIFF(SECOND, N.StartTime, N.EndTime)) / 60.0 AS TotalTimeSpent,
                //    -- Average time per question (in minutes)
                //    AVG(DATEDIFF(SECOND, N.StartTime, N.EndTime)) / 60.0 AS AvgTimePerQuestion,

                //    -- Time spent on correct answers
                //    SUM(CASE WHEN A.IsCorrect = 1 THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE 0 END) / 60.0 AS TotalTimeSpentCorrect,
                //    -- Average time for correct answers
                //    AVG(CASE WHEN A.IsCorrect = 1 THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE NULL END) / 60.0 AS AvgTimeSpentCorrect,

                //    -- Time spent on incorrect answers
                //    SUM(CASE WHEN A.IsCorrect = 0 THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE 0 END) / 60.0 AS TotalTimeSpentWrong,
                //    -- Average time for incorrect answers
                //    AVG(CASE WHEN A.IsCorrect = 0 THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE NULL END) / 60.0 AS AvgTimeSpentWrong,

                //    -- Time spent on unattempted questions (Status ID 2 and 4)
                //    SUM(CASE WHEN SQM.QuestionStatusId IN (2, 4) THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE 0 END) / 60.0 AS TotalTimeSpentUnattempted,
                //    -- Average time for unattempted questions
                //    AVG(CASE WHEN SQM.QuestionStatusId IN (2, 4) THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE NULL END) / 60.0 AS AvgTimeSpentUnattempted

                //FROM tblCYOTQuestionNavigation AS N
                //LEFT JOIN tblCYOTAnswers AS A ON N.QuestionId = A.QuestionID AND N.StudentId = A.StudentID AND N.CYOTId = A.CYOTID
                //LEFT JOIN tblCYOTStudentQuestionMapping AS SQM ON N.QuestionId = SQM.QuestionId AND N.StudentId = SQM.StudentId AND N.CYOTId = SQM.CYOTId
                //LEFT JOIN tblCYOTQuestions AS Q ON N.QuestionId = Q.QuestionID
                //WHERE N.CYOTId = @CYOTId AND N.StudentId = @StudentId AND SQM.SubjectID = @SubjectID;";

                var result = await _connection.QueryFirstOrDefaultAsync<dynamic>(query, new
                {
                    CYOTID = cyotId,
                    StudentID = studentId,
                    SubjectID = subjectId
                });

                if (result != null)
                {
                    var response = new CYOTTimeAnalytics
                    {
                        TotalTimeSpent = ConvertSecondsToTimeFormat((int)Math.Floor(result.TotalTimeSpent ?? 0)),
                        AvgTimePerQuestion = ConvertSecondsToTimeFormat((int)Math.Floor(result.AvgTimePerQuestion ?? 0)),

                        TotalTimeSpentCorrect = ConvertSecondsToTimeFormat((int)Math.Floor(result.TotalTimeSpentCorrect ?? 0)),
                        AvgTimeSpentCorrect = ConvertSecondsToTimeFormat((int)Math.Floor(result.AvgTimeSpentCorrect ?? 0)),

                        TotalTimeSpentWrong = ConvertSecondsToTimeFormat((int)Math.Floor(result.TotalTimeSpentWrong ?? 0)),
                        AvgTimeSpentWrong = ConvertSecondsToTimeFormat((int)Math.Floor(result.AvgTimeSpentWrong ?? 0)),

                        TotalTimeSpentUnattempted = ConvertSecondsToTimeFormat((int)Math.Floor(result.TotalTimeSpentUnattempted ?? 0)),
                        AvgTimeSpentUnattempted = ConvertSecondsToTimeFormat((int)Math.Floor(result.AvgTimeSpentUnattempted ?? 0))
                    };


                    return new ServiceResponse<CYOTTimeAnalytics>(
                        true,
                        "Subject-wise time analytics fetched successfully",
                        response,
                        200
                    );
                }

                return new ServiceResponse<CYOTTimeAnalytics>(
                    false,
                    "No analytics data found",
                    null,
                    404
                );
            }
            catch (Exception ex)
            {
                return new ServiceResponse<CYOTTimeAnalytics>(
                    false,
                    ex.Message,
                    null,
                    500
                );
            }
        }
        public async Task<ServiceResponse<string>> UpdateQuestionStatusAsync(int cyotId, int studentId, int questionId, bool isAnswered)
        {
            // Define status IDs
            const int Answered = 1;
            const int NotVisited = 4;
            const int Review = 3;
            const int ReviewWithAnswer = 5;
            const int Unanswered = 2;
            // Check current status
            var currentStatusId = await _connection.QuerySingleOrDefaultAsync<int>(
                @"SELECT QuestionStatusId
                  FROM tblCYOTStudentQuestionMapping
                  WHERE CYOTId = @CYOTId AND StudentId = @StudentId AND QuestionId = @QuestionId",
                new { CYOTId = cyotId, StudentId = studentId, QuestionId = questionId });

            int newStatusId;
            if (currentStatusId == 0)
            {
                // New entry
                newStatusId = isAnswered ? Answered : Review;
            }
            else if (currentStatusId == NotVisited)
            {
                newStatusId = isAnswered ? Answered : Review;
            }
            else if (currentStatusId == Review)
            {
                newStatusId = isAnswered ? Answered : Review;
            }
            else if (currentStatusId == Answered)
            {
                newStatusId = isAnswered ? Answered : ReviewWithAnswer;
            }
            else if (currentStatusId == ReviewWithAnswer)
            {
                newStatusId = isAnswered ? Answered : ReviewWithAnswer;
            }
            else if (currentStatusId == Unanswered)
            {
                newStatusId = isAnswered ? Answered : Unanswered;
            }
            else
            {
                // Default case
                newStatusId = currentStatusId;
            }
            // Update status in the mapping table
            if (currentStatusId == 0)
            {
                // Insert new mapping
                await _connection.ExecuteAsync(
                    @"INSERT INTO tblCYOTStudentQuestionMapping (CYOTId, StudentId, QuestionId, QuestionStatusId)
                      VALUES (@CYOTId, @StudentId, @QuestionId, @QuestionStatusId)",
                    new { CYOTId = cyotId, StudentId = studentId, QuestionId = questionId, QuestionStatusId = newStatusId });
            }
            else
            {
                // Update existing mapping
                await _connection.ExecuteAsync(
                    @"UPDATE tblCYOTStudentQuestionMapping
                      SET QuestionStatusId = @QuestionStatusId
                      WHERE CYOTId = @CYOTId AND StudentId = @StudentId AND QuestionId = @QuestionId",
                    new { QuestionStatusId = newStatusId, CYOTId = cyotId, StudentId = studentId, QuestionId = questionId });
            }

            // Log the review
            await _connection.ExecuteAsync(
                @"INSERT INTO tblCYOTQuestionReviewed (QuestionId, StudentId, CYOTId)
                  VALUES (@QuestionId, @StudentId, @CYOTId)",
                new { QuestionId = questionId, StudentId = studentId, CYOTId = cyotId });

            return new ServiceResponse<string>(true, "status updated successfully", string.Empty, 200);
        }
        private List<MatchPair> GetMatchPairs(string questionCode, int questionId)
        {
            const string query = @"
        SELECT MatchThePairId, PairColumn, PairRow, PairValue
        FROM tblQuestionMatchThePair
        WHERE QuestionCode = @QuestionCode AND QuestionId = @QuestionId";


            return _connection.Query<MatchPair>(query, new { QuestionCode = questionCode, QuestionId = questionId }).ToList();

        }
        private List<AnswerMultipleChoiceCategory> GetMultipleAnswers(string QuestionCode)
        {
            var answerMaster = _connection.QueryFirstOrDefault<AnswerMaster>(@"
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
        private List<MatchThePairAnswer> GetMatchThePairType2Answers(string questionCode, int questionId)
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
                return new List<MatchThePairAnswer>();
            }

            return _connection.Query<MatchThePairAnswer>(getAnswersQuery, new { AnswerId = answerId }).ToList();

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
        private Answersingleanswercategory GetSingleAnswer(string QuestionCode, int QuestionId)
        {
            var answerMaster = _connection.QueryFirstOrDefault<AnswerMaster>(@"
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
        private QuestionResponseDTO GetQuestionById(int QuestionId)
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
                WHERE q.QuestionId = @QuestionId AND q.IsActive = 1";

            var parameters = new { QuestionId = QuestionId };

            var item = _connection.QueryFirstOrDefault<dynamic>(sql, parameters);

            if (item != null)
            {
                if (item.QuestionTypeId == 11)
                {
                    var questionResponse = new QuestionResponseDTO
                    {
                        QuestionId = item.QuestionId,
                        Paragraph = item.Paragraph,
                        SubjectName = item.SubjectName,
                        EmployeeName = item.EmpFirstName,
                        IndexTypeName = item.IndexTypeName,
                        ContentIndexName = item.ContentIndexName,
                        // Qid = GetListOfQIDCourse(item.QuestionCode),
                        //QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
                        //Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
                        //AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode),
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
                    return questionResponse;
                }
                else
                {
                    var questionResponse = new QuestionResponseDTO
                    {
                        QuestionId = item.QuestionId,
                        QuestionDescription = item.QuestionDescription,
                        SubjectName = item.SubjectName,
                        EmployeeName = item.EmpFirstName,
                        IndexTypeName = item.IndexTypeName,
                        ContentIndexName = item.ContentIndexName,
                        // QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                        //QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
                        //Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
                        //AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode),
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
                        MatchPairs = item.QuestionTypeId == 6 || item.QuestionTypeId == 12 ? GetMatchPairs(item.QuestionCode, item.QuestionId) : null,
                        MatchThePairType2Answers = item.QuestionTypeId == 12 ? GetMatchThePairType2Answers(item.QuestionCode, item.QuestionId) : null,
                        Answersingleanswercategories = (item.QuestionTypeId != 6 && item.QuestionTypeId != 12) ? GetSingleAnswer(item.QuestionCode, item.QuestionId) : null,
                        AnswerMultipleChoiceCategories = (item.QuestionTypeId != 12) ? GetMultipleAnswers(item.QuestionCode) : null

                    };
                    return questionResponse;
                }

            }
            else
            {
                return null;
            }
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
        private bool IsSingleAnswerType(int questionTypeId)
        {
            // Assuming the following are single answer type IDs based on your data
            return questionTypeId == 4 || questionTypeId == 9;
            //|| questionTypeId == 8 || questionTypeId == 10 || questionTypeId == 11;
        }
    }
}