using Dapper;
using Microsoft.AspNetCore.Connections;
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

                    subject.ConceptCount = chapters.Count();
                }

                return new ServiceResponse<List<SubjectDTO>>(true, "Records found", subjects, 200, subjects.Count);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<SubjectDTO>>(false, ex.Message, new List<SubjectDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<bool>> MakeCYOTOpenChallenge(int CYOTId)
        {
            await _connection.ExecuteAsync(@"update [tblCYOT] set CYOTStatusID = 3 where CYOTID = @CYOTID", new { CYOTID = CYOTId });
            return new ServiceResponse<bool>(true, "marked as open challenge", true, 200);
        }
        public async Task<ServiceResponse<List<CYOTResponse>>> GetCYOTListByStudent(CYOTListRequest request)
        {
            try
            {
                // Query to get CYOT details filtered by StudentID
                var cyotQuery = @"
            SELECT 
                CYOTID,IsStarted,
                ChallengeName AS CYOTName,
                NoOfQuestions AS TotalQuestions,
                Duration,
                CYOTStatusID
            FROM tblCYOT
            WHERE CreatedBy = @StudentID";

                // Query to get total questions per CYOTID
                var totalQuestionsQuery = @"
            SELECT CYOTID, COUNT(*) AS TotalQuestions
            FROM tblCYOTQuestions
            GROUP BY CYOTID";

                // Query to get questions answered by the student
                var attemptedQuestionsQuery = @"
            SELECT CYOTID, COUNT(*) AS AttemptedQuestions
            FROM tblCYOTAnswers
            WHERE StudentID = @StudentID
            GROUP BY CYOTID";


                // Fetch CYOT data
                var cyotList = (await _connection.QueryAsync<CYOTResponse>(cyotQuery, new { StudentID = request.RegistrationId })).ToList();

                // Fetch total questions data
                var totalQuestions = await _connection.QueryAsync<dynamic>(totalQuestionsQuery);

                // Fetch attempted questions data
                var attemptedQuestions = await _connection.QueryAsync<dynamic>(attemptedQuestionsQuery, new { StudentID = request.RegistrationId });

                // Merge data
                foreach (var cyot in cyotList)
                {
                    bool isStarted = await _connection.QueryFirstOrDefaultAsync<bool>(@"select IsStarted from [tblCYOT] where CYOTID = @CYOTID", new { CYOTID = cyot.CYOTID });
                    var total = totalQuestions.FirstOrDefault(q => q.CYOTID == cyot.CYOTID)?.TotalQuestions ?? 0;
                    var attempted = attemptedQuestions.FirstOrDefault(q => q.CYOTID == cyot.CYOTID)?.AttemptedQuestions ?? 0;

                    cyot.Percentage = total > 0 ? (attempted * 100) / total : 0;
                    cyot.IsChallengeApplicable = cyot.Percentage >= 80;
                    if (cyot.IsChallengeApplicable)
                    {
                        cyot.CYOTStatus = "Challenged";
                    }
                    else if (isStarted)
                    {
                        cyot.CYOTStatus = "Complete";
                    }
                    else
                    {
                        cyot.CYOTStatus = "Pending";
                    }
                }
                var response = cyotList.Where(m => m.CYOTStatusID == request.StatusId).ToList();
                return new ServiceResponse<List<CYOTResponse>>(true, "CYOT list fetched successfully.", response, 200);

            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<CYOTResponse>>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<List<ChapterDTO>>> GetChaptersAsync(int registrationId, int subjectId)
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
                var actualSyllabusId = await _connection.QueryFirstOrDefaultAsync<int?>(
                    @"SELECT SyllabusId
              FROM tblSyllabus
              WHERE BoardID = @BoardId AND ClassID = @ClassId AND CourseID = @CourseId AND Status = 1",
                    new { mapping.BoardId, mapping.ClassId, mapping.CourseId });

                if (!actualSyllabusId.HasValue)
                    throw new Exception("Syllabus ID does not match or no syllabus found.");

                // Step 3: Fetch Chapters mapped to SyllabusID and SubjectID
                var chapters = (await _connection.QueryAsync<ChapterDTO>(
                    @"SELECT C.ContentIndexId AS ChapterId, C.ContentName_Chapter AS ChapterName, C.ChapterCode, C.DisplayOrder
              FROM tblSyllabusDetails S
              INNER JOIN tblContentIndexChapters C ON S.ContentIndexId = C.ContentIndexId
              WHERE S.SyllabusID = @SyllabusID AND S.SubjectID = @SubjectID AND S.IndexTypeId = 1",
                    new { SyllabusID = actualSyllabusId, SubjectID = subjectId })).ToList();

                if (!chapters.Any())
                    return new ServiceResponse<List<ChapterDTO>>(false, "No chapters found for the given SyllabusID and SubjectID.", new List<ChapterDTO>(), 404);

                // Step 4: Fetch and assign the count of topics and subtopics for each chapter
                foreach (var chapter in chapters)
                {
                    // Count Topics directly mapped to the chapter
                    var topicCount = await _connection.QueryFirstOrDefaultAsync<int>(
                        @"SELECT COUNT(*)
                  FROM tblContentIndexTopics T
                  WHERE T.ChapterCode = @ChapterCode AND T.Status = 1 AND T.IsActive = 1",
                        new { ChapterCode = chapter.ChapterCode });

                    // Count Subtopics mapped to the topics of the chapter
                    var subTopicCount = await _connection.QueryFirstOrDefaultAsync<int>(
                        @"SELECT COUNT(*)
                  FROM tblContentIndexSubTopics ST
                  INNER JOIN tblContentIndexTopics T ON ST.TopicCode = T.TopicCode
                  WHERE T.ChapterCode = @ChapterCode AND ST.Status = 1 AND ST.IsActive = 1",
                        new { ChapterCode = chapter.ChapterCode });

                    // Assign the total count of concepts (topics + subtopics) to the chapter
                    chapter.ConceptCount = topicCount + subTopicCount;
                }

                return new ServiceResponse<List<ChapterDTO>>(true, "Records found", chapters, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ChapterDTO>>(false, ex.Message, new List<ChapterDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<int>> InsertOrUpdateCYOTAsync(CYOTDTO cyot)
        {
            try
            {
                // Step 1: Validate CYOT Start Time
                if (cyot.ChallengeStartTime <= DateTime.Now.AddMinutes(15))
                {
                    return new ServiceResponse<int>(false, "Challenge start time must be at least 15 minutes from the current time.", 0, 400);
                }

                int cyotId;

                // Step 2: Insert or Update `tblCYOT`
                if (cyot.CYOTID == 0)
                {
                    // Insert
                    var insertQuery = @"
            INSERT INTO tblCYOT (
                ChallengeName, ChallengeDate, ChallengeStartTime, Duration, 
                NoOfQuestions, MarksPerCorrectAnswer, MarksPerIncorrectAnswer, CreatedBy,
                ClassID, CourseID, BoardID
            ) VALUES (
                @ChallengeName, @ChallengeDate, @ChallengeStartTime, @Duration, 
                @NoOfQuestions, @MarksPerCorrectAnswer, @MarksPerIncorrectAnswer, @CreatedBy,
                @ClassID, @CourseID, @BoardID
            ); 
            SELECT CAST(SCOPE_IDENTITY() as int)";

                    cyotId = await _connection.ExecuteScalarAsync<int>(insertQuery, cyot);
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

                    await _connection.ExecuteAsync(updateQuery, cyot);
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

                return new ServiceResponse<CYOTDTO>(true, "CYOT record retrieved successfully.", cyot, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<CYOTDTO>(false, $"Error: {ex.Message}", null, 500);
            }
        }
        public async Task<ServiceResponse<bool>> UpdateCYOTSyllabusAsync(int cyotId, List<CYOTSyllabusDTO> syllabusList)
        {
            try
            {
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
            _connection.Open();

            // Step 1: Fetch syllabus details
            var studentDetails = await _connection.QuerySingleOrDefaultAsync<StudentDetails>(
                "SELECT SCCMID, RegistrationID, CourseID, ClassID, BoardId " +
                "FROM tblStudentClassCourseMapping WHERE RegistrationID = @RegistrationID",
                new { RegistrationID = request.registrationId });

            if (studentDetails == null)
                return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);

            var syllabusDetails = await _connection.QueryAsync<SyllabusDetails>(
                "SELECT SyllabusId, BoardID, CourseId, ClassId, SyllabusName " +
                "FROM tblSyllabus WHERE BoardID = @BoardId AND CourseId = @CourseId AND ClassId = @ClassId",
                new { studentDetails.BoardId, studentDetails.CourseID, studentDetails.ClassID });

            if (!syllabusDetails.Any())
                return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);

            // Step 2: Fetch CYOT Syllabus
            var cyotSyllabus = await _connection.QueryAsync<QuizooSubjectMapping>(
                "SELECT CYOTID, SubjectID, ChapterID " +
                "FROM tblCYOTSyllabus WHERE CYOTID = @CYOTID",
                new { CYOTID = request.cyotId });

            if (!cyotSyllabus.Any())
                return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);

            // Step 3: Fetch Chapters (associated with SubjectId)
            var chapters = await _connection.QueryAsync<ContentDetails>(
                @"SELECT 
            CIC.ContentIndexId AS ChapterContentIndexId,
            CIC.SubjectId AS SubjectId,
            CIC.ContentName_Chapter AS ContentName,
            CIC.IndexTypeId AS IndexTypeId
        FROM tblContentIndexChapters CIC
        WHERE CIC.SubjectId IN @SubjectIds",
                new { SubjectIds = cyotSyllabus.Select(c => c.SubjectID) });

            // Step 4: Fetch Topics (associated with ChapterId)
            var topics = await _connection.QueryAsync<ContentDetails>(
                @"SELECT 
            CIT.ContInIdTopic AS TopicContentIndexId,
            CIT.ContentIndexId AS ChapterId,
            CIT.ContentName_Topic AS ContentName,
            CIT.IndexTypeId AS IndexTypeId
        FROM tblContentIndexTopics CIT
        WHERE CIT.ContentIndexId IN @ChapterIds",
                new { ChapterIds = chapters.Select(c => c.ChapterContentIndexId) });

            // Step 5: Fetch Subtopics (associated with TopicId)
            var subtopics = await _connection.QueryAsync<ContentDetails>(
                @"SELECT 
            CIS.ContInIdSubTopic AS SubTopicContentIndexId,
            CIS.ContInIdTopic AS TopicId,
            CIS.ContentName_SubTopic AS ContentName,
            CIS.IndexTypeId AS IndexTypeId
        FROM tblContentIndexSubTopics CIS
        WHERE CIS.ContInIdTopic IN @TopicIds",
                new { TopicIds = topics.Select(t => t.TopicContentIndexId) });

            // Step 6: Combine Chapters, Topics, and Subtopics
            var combinedContentDetails = chapters
                .Concat(topics)
                .Concat(subtopics)
                .Select(c => new ContentDetails
                {
                    ContentIndexId = c.ChapterContentIndexId != 0
                        ? c.ChapterContentIndexId
                        : c.TopicContentIndexId != 0
                            ? c.TopicContentIndexId
                            : c.SubTopicContentIndexId,
                    SubjectId = c.SubjectId,
                    ContentName = c.ContentName,
                    IndexTypeId = c.IndexTypeId
                })
                .Where(c => c.ContentIndexId != 0)
                .Distinct()
                .ToList();

            // Step 7: Filter based on tblSyllabusDetails mappings
            var syllabusContentMapping = await _connection.QueryAsync(
                @"SELECT DISTINCT ContentIndexId, IndexTypeId 
          FROM tblSyllabusDetails 
          WHERE SubjectId IN @SubjectIds 
          AND ContentIndexId IN @ContentIndexIds 
          AND IndexTypeId IN @IndexTypeIds",
                new
                {
                    SubjectIds = cyotSyllabus.Select(c => c.SubjectID),
                    ContentIndexIds = combinedContentDetails.Select(c => c.ContentIndexId),
                    IndexTypeIds = combinedContentDetails.Select(c => c.IndexTypeId)
                });

            var syllabusMappingList = syllabusContentMapping
                .Select(m => new { m.ContentIndexId, m.IndexTypeId })
                .ToList();

            var filteredContent = combinedContentDetails
                .Where(c => syllabusMappingList.Any(m => m.ContentIndexId == c.ContentIndexId && m.IndexTypeId == c.IndexTypeId))
                .ToList();

            if (!filteredContent.Any())
                return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);

            // Step 8: Fetch CYOT details
            var cyotDetails = await _connection.QuerySingleOrDefaultAsync<dynamic>(
                "SELECT NoOfQuestions, Duration FROM tblCYOT WHERE CYOTID = @CYOTID",
                new { CYOTID = request.cyotId });

            if (cyotDetails == null)
                return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);

            int limit = cyotDetails.NoOfQuestions;
            int durationPerQuestion = cyotDetails.Duration / limit;

            // Step 9: Fetch questions
            //var questions = await _connection.QueryAsync<QuestionResponseDTO>(
            //    "SELECT TOP(@Limit) * " +
            //    "FROM tblQuestion " +
            //    "WHERE ContentIndexId IN @ContentIndexIds " +
            //    "AND SubjectID IN @SubjectIds " +
            //    "AND IndexTypeId IN @IndexTypeIds " +
            //    "AND IsConfigure = 1 AND IsLive = 1 AND QuestionTypeId IN (1, 2, 10, 6) " +
            //    "ORDER BY NEWID()",
            //    new
            //    {
            //        ContentIndexIds = filteredContent.Select(c => c.ContentIndexId),
            //        SubjectIds = filteredContent.Select(c => c.SubjectId),
            //        IndexTypeIds = filteredContent.Select(c => c.IndexTypeId),
            //        Limit = limit
            //    });
            var questions = await _connection.QueryAsync<QuestionResponseDTO>(
    "SELECT TOP(@Limit) * " +
    "FROM tblQuestion " +
    "WHERE ContentIndexId IN @ContentIndexIds " +
    "AND SubjectID IN @SubjectIds " +
    "AND IndexTypeId IN @IndexTypeIds " +
    "AND IsConfigure = 1 AND IsLive = 1 " +
    "AND QuestionTypeId IN @QuestionTypeIds " + // Added filter
    "ORDER BY NEWID()",
    new
    {
        ContentIndexIds = filteredContent.Select(c => c.ContentIndexId),
        SubjectIds = filteredContent.Select(c => c.SubjectId),
        IndexTypeIds = filteredContent.Select(c => c.IndexTypeId),
        QuestionTypeIds = request.QuestionTypeId?.Any() == true ? request.QuestionTypeId : new List<int> { 1, 2, 10, 6 }, // Use provided list or default
        Limit = limit
    });
            // Convert the data to a list of DTOs
            var response = questions.Select(item =>
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
                //return new QuestionResponseDTO
                //{
                //    QuestionId = item.QuestionId,
                //    QuestionDescription = item.QuestionDescription,
                //    QuestionTypeId = item.QuestionTypeId,
                //    Status = item.Status,
                //    CreatedBy = item.CreatedBy,
                //    CreatedOn = item.CreatedOn,
                //    ModifiedBy = item.ModifiedBy,
                //    ModifiedOn = item.ModifiedOn,
                //    subjectID = item.subjectID,
                //    SubjectName = item.SubjectName,
                //    EmployeeId = item.EmployeeId,
                //    // EmployeeName = item.EmpFirstName,
                //    IndexTypeId = item.IndexTypeId,
                //    IndexTypeName = item.IndexTypeName,
                //    ContentIndexId = item.ContentIndexId,
                //    ContentIndexName = item.ContentIndexName,
                //    IsRejected = item.IsRejected,
                //    IsApproved = item.IsApproved,
                //    QuestionTypeName = item.QuestionTypeName,
                //    QuestionCode = item.QuestionCode,
                //    Explanation = item.Explanation,
                //    ExtraInformation = item.ExtraInformation,
                //    IsActive = item.IsActive,
                //    DurationperQuestion = durationPerQuestion,
                //    MatchPairs = item.QuestionTypeId == 6 || item.QuestionTypeId == 12 ? GetMatchPairs(item.QuestionCode, item.QuestionId) : null,
                //    AnswerMultipleChoiceCategories = (item.QuestionTypeId != 12) ? GetMultipleAnswers(item.QuestionCode) : null
                //};
            });
            // Step 10: Insert questions into tblCYOTQuestions
            var insertQuery = @"
        INSERT INTO tblCYOTQuestions (CYOTID, QuestionID, DisplayOrder)
        VALUES (@CYOTID, @QuestionID, @DisplayOrder)";
            await _connection.ExecuteAsync(insertQuery, questions.Select((q, index) => new
            {
                CYOTID = request.cyotId,
                QuestionID = q.QuestionId,
                DisplayOrder = index + 1
            }));

            return questions.Any()
                ? new ServiceResponse<List<QuestionResponseDTO>>(true, "Operation Successful", questions.ToList(), 200)
                : new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
        }
        public async Task<ServiceResponse<IEnumerable<AnswerPercentageResponse>>> SubmitCYOTAnswerAsync(List<SubmitAnswerRequest> requests)
        {
            try
            {
               
                    // Step 1: Fetch correct answers for all QuestionIDs and AnswerIDs
                    var correctAnswersQuery = @"
                SELECT AM.QuestionID, AMC.AnswerID, AMC.IsCorrect
                FROM tblAnswerMaster AM
                INNER JOIN tblAnswerMultipleChoiceCategory AMC ON AM.AnswerID = AMC.AnswerID
                WHERE AM.QuestionID IN @QuestionIDs AND AMC.AnswerID IN @AnswerIDs";

                    var correctAnswers = (await _connection.QueryAsync<(int QuestionID, int AnswerID, bool IsCorrect)>(
                        correctAnswersQuery,
                        new
                        {
                            QuestionIDs = requests.Select(r => r.QuestionID).Distinct(),
                            AnswerIDs = requests.Select(r => r.AnswerID).Distinct()
                        }
                    )).ToList();

                    // Step 2: Process each request to insert or update answers
                    foreach (var request in requests)
                    {
                        var isCorrect = correctAnswers
                            .FirstOrDefault(ca => ca.QuestionID == request.QuestionID && ca.AnswerID == request.AnswerID)
                            .IsCorrect;

                        var existingAnswerQuery = @"
                    SELECT COUNT(1)
                    FROM tblCYOTAnswers
                    WHERE CYOTID = @CYOTID AND QuestionID = @QuestionID AND StudentID = @StudentID";

                        var hasExistingAnswer = await _connection.ExecuteScalarAsync<bool>(
                            existingAnswerQuery,
                            new
                            {
                                CYOTID = request.QuizID,
                                QuestionID = request.QuestionID,
                                StudentID = request.StudentID
                            }
                        );

                        if (hasExistingAnswer)
                        {
                            // Update existing answer
                            var updateQuery = @"
                        UPDATE tblCYOTAnswers
                        SET AnswerID = @AnswerID, IsCorrect = @IsCorrect
                        WHERE CYOTID = @CYOTID AND QuestionID = @QuestionID AND StudentID = @StudentID";

                            await _connection.ExecuteAsync(
                                updateQuery,
                                new
                                {
                                    CYOTID = request.QuizID,
                                    StudentID = request.StudentID,
                                    QuestionID = request.QuestionID,
                                    AnswerID = request.AnswerID,
                                    IsCorrect = isCorrect
                                }
                            );
                        }
                        else
                        {
                            // Insert new answer
                            var insertQuery = @"
                        INSERT INTO tblCYOTAnswers (CYOTID, StudentID, QuestionID, AnswerID, IsCorrect)
                        VALUES (@CYOTID, @StudentID, @QuestionID, @AnswerID, @IsCorrect)";

                            await _connection.ExecuteAsync(
                                insertQuery,
                                new
                                {
                                    CYOTID = request.QuizID,
                                    StudentID = request.StudentID,
                                    QuestionID = request.QuestionID,
                                    AnswerID = request.AnswerID,
                                    IsCorrect = isCorrect
                                }
                            );
                        }
                    }

                    // Step 3: Fetch answer counts for all questions
                    var answerCountQuery = @"
                SELECT 
                    QuestionID,
                    AnswerID,
                    COUNT(AnswerID) AS AnswerCount
                FROM tblCYOTAnswers
                WHERE CYOTID = @CYOTID AND QuestionID IN @QuestionIDs
                GROUP BY QuestionID, AnswerID";

                    var answerCounts = await _connection.QueryAsync<AnswerPercentageResponse>(
                        answerCountQuery,
                        new
                        {
                            CYOTID = requests.First().QuizID,
                            QuestionIDs = requests.Select(r => r.QuestionID).Distinct()
                        }
                    );

                    // Step 4: Fetch total and attempted questions for the quiz
                    var totalQuestionsQuery = @"
                SELECT COUNT(*) AS TotalQuestions 
                FROM tblCYOTQuestions 
                WHERE CYOTID = @CYOTID";

                    var attemptedQuestionsQuery = @"
                SELECT COUNT(DISTINCT QuestionID) AS AttemptedQuestions
                FROM tblCYOTAnswers
                WHERE CYOTID = @CYOTID AND StudentID = @StudentID";

                    var totalQuestions = await _connection.QueryFirstOrDefaultAsync<int>(
                        totalQuestionsQuery,
                        new { CYOTID = requests.First().QuizID }
                    );

                    var attemptedQuestions = await _connection.QueryFirstOrDefaultAsync<int>(
                        attemptedQuestionsQuery,
                        new
                        {
                            CYOTID = requests.First().QuizID,
                            StudentID = requests.First().StudentID
                        }
                    );

                    // Step 5: Calculate percentage and update quiz status
                    var percentage = totalQuestions > 0 ? (attemptedQuestions * 100) / totalQuestions : 0;
                    var statusId = percentage >= 80 ? 3 : 2;

                    await _connection.ExecuteAsync(
                        @"UPDATE [tblCYOT] SET CYOTStatusID = @StatusID WHERE CYOTID = @CYOTID",
                        new
                        {
                            CYOTID = requests.First().QuizID,
                            StatusID = statusId
                        }
                    );

                    return new ServiceResponse<IEnumerable<AnswerPercentageResponse>>(
                        true,
                        "Operation successful",
                        answerCounts,
                        200
                    );
                
            }
            catch (Exception ex)
            {
                return new ServiceResponse<IEnumerable<AnswerPercentageResponse>>(
                    false,
                    $"Error: {ex.Message}",
                    null,
                    500
                );
            }
        }
        //    public async Task<ServiceResponse<IEnumerable<AnswerPercentageResponse>>> SubmitCYOTAnswerAsync(List<SubmitAnswerRequest> request)
        //    {

        //        string checkCorrectAnswerQuery = @"
        //SELECT AMC.IsCorrect
        //FROM tblAnswerMaster AM
        //INNER JOIN tblAnswerMultipleChoiceCategory AMC
        //ON AM.AnswerID = AMC.AnswerID
        //WHERE AM.QuestionID = @QuestionID AND AMC.AnswerID = @AnswerID";

        //        string checkExistingAnswerQuery = @"
        //SELECT COUNT(1)
        //FROM tblCYOTAnswers
        //WHERE CYOTID = @CYOTID AND QuestionID = @QuestionID AND StudentID = @StudentID";

        //        string updateQuery = @"
        //UPDATE tblCYOTAnswers
        //SET AnswerID = @AnswerID, IsCorrect = @IsCorrect
        //WHERE CYOTID = @CYOTID AND QuestionID = @QuestionID AND StudentID = @StudentID";

        //        string insertQuery = @"
        //INSERT INTO tblCYOTAnswers (CYOTID, StudentID, QuestionID, AnswerID, IsCorrect)
        //VALUES (@CYOTID, @StudentID, @QuestionID, @AnswerID, @IsCorrect)";

        //        string answerCountQuery = @"
        //SELECT 
        //    AnswerID,
        //    COUNT(AnswerID) AS AnswerCount
        //FROM tblCYOTAnswers
        //WHERE CYOTID = @CYOTID AND QuestionID = @QuestionID
        //GROUP BY AnswerID";

        //        // Check if the answer is correct
        //        bool isCorrect = await _connection.ExecuteScalarAsync<bool>(checkCorrectAnswerQuery, new
        //        {
        //            QuestionID = request.QuestionID,
        //            AnswerID = request.AnswerID
        //        });

        //        // Check if there is an existing answer for the student
        //        bool hasExistingAnswer = await _connection.ExecuteScalarAsync<bool>(checkExistingAnswerQuery, new
        //        {
        //            CYOTID = request.QuizID, // Assuming QuizID maps to CYOTID
        //            QuestionID = request.QuestionID,
        //            StudentID = request.StudentID
        //        });

        //        if (hasExistingAnswer)
        //        {
        //            // Update the existing answer
        //            await _connection.ExecuteAsync(updateQuery, new
        //            {
        //                CYOTID = request.QuizID,
        //                StudentID = request.StudentID,
        //                QuestionID = request.QuestionID,
        //                AnswerID = request.AnswerID,
        //                IsCorrect = isCorrect
        //            });
        //        }
        //        else
        //        {
        //            // Insert a new answer
        //            await _connection.ExecuteAsync(insertQuery, new
        //            {
        //                CYOTID = request.QuizID,
        //                StudentID = request.StudentID,
        //                QuestionID = request.QuestionID,
        //                AnswerID = request.AnswerID,
        //                IsCorrect = isCorrect
        //            });
        //        }

        //        // Fetch updated answer counts
        //        var answerCounts = await _connection.QueryAsync<AnswerPercentageResponse>(answerCountQuery, new
        //        {
        //            CYOTID = request.QuizID,
        //            QuestionID = request.QuestionID
        //        });
        //        // Query to get total questions per CYOTID
        //        var totalQuestionsQuery = @"SELECT COUNT(*) AS TotalQuestions FROM tblCYOTQuestions WHERE CYOTID = @CYOTID";

        //        // Query to get questions answered by the student
        //        var attemptedQuestionsQuery = @"SELECT COUNT(*) AS AttemptedQuestions FROM tblCYOTAnswers WHERE StudentID = @StudentID
        //        AND CYOTID = @CYOTID";

        //        // Fetch total questions count
        //        var totalQuestions = await _connection.QueryFirstOrDefaultAsync<int>(totalQuestionsQuery, new { CYOTID = request.QuizID });

        //        // Fetch attempted questions count
        //        var attemptedQuestions = await _connection.QueryFirstOrDefaultAsync<int>(attemptedQuestionsQuery, new { StudentID = request.StudentID, CYOTID = request.QuizID });

        //        // Calculate percentage
        //        int percentage = totalQuestions > 0 ? (attemptedQuestions * 100) / totalQuestions : 0;

        //        if (percentage >= 80)
        //        {
        //            await _connection.ExecuteAsync(@"update [tblCYOT] set CYOTStatusID = 3 where CYOTID = @CYOTID", new { CYOTID = request.QuizID });
        //        }
        //        else
        //        {
        //            await _connection.ExecuteAsync(@"update [tblCYOT] set CYOTStatusID = 2 where CYOTID = @CYOTID", new { CYOTID = request.QuizID });
        //        }
        //        return new ServiceResponse<IEnumerable<AnswerPercentageResponse>>(true, "operation successful", answerCounts, 200);
        //    }
        public async Task<ServiceResponse<List<CYOTQuestionWithAnswersDTO>>> GetCYOTQuestionsWithOptionsAsync(int cyotId)
        {
            // SQL query to fetch questions with all answer options
            var query = @"
SELECT 
    cq.CYOTID,
    cq.QuestionID,
    q.QuestionCode,
    q.QuestionDescription,
    q.Explanation, q.ExtraInformation,
    mc.Answermultiplechoicecategoryid,
    mc.Answer,
    mc.IsCorrect
FROM 
    tblCYOTQuestions cq
JOIN 
    tblQuestion q ON cq.QuestionID = q.QuestionId
JOIN 
    tblAnswerMaster am ON q.QuestionId = am.Questionid
JOIN 
    tblAnswerMultipleChoiceCategory mc ON am.Answerid = mc.Answerid
WHERE 
    cq.CYOTID = @CYOTID
ORDER BY 
    cq.DisplayOrder";

            // Execute the query and fetch the results
            var rawData = await _connection.QueryAsync<dynamic>(query, new { CYOTID = cyotId });

            // Group data by QuestionID to include answers in a nested structure
            var groupedData = rawData.GroupBy(
                item => new
                {
                    CYOTID = (int)item.CYOTID,
                    QuestionID = (int)item.QuestionID,
                    QuestionCode = (string)item.QuestionCode,
                    QuestionDescription = (string)item.QuestionDescription
                },
                (key, answers) => new CYOTQuestionWithAnswersDTO
                {
                    CYOTID = key.CYOTID,
                    QuestionID = key.QuestionID,
                    QuestionCode = key.QuestionCode,
                    QuestionDescription = key.QuestionDescription,
                    Answers = answers.Select(answer => new AnswerOptionDTO
                    {
                        AnswerMultipleChoiceCategoryID = (int)answer.Answermultiplechoicecategoryid,
                        Answer = (string)answer.Answer,
                        IsCorrect = (bool)answer.IsCorrect
                    }).ToList()
                }).ToList();

            // Return the grouped data
            return new ServiceResponse<List<CYOTQuestionWithAnswersDTO>>(
                true,
                "Operation successful",
                groupedData,
                200
            );
        }
        public async Task<ServiceResponse<bool>> UpsertCYOTParticipantsAsync(List<CYOTParticipantRequest> requests)
        {
            try
            {
                var query = @"
        IF EXISTS (SELECT 1 FROM tblCYOTParticipant WHERE StudentID = @StudentID AND CYOTID = @CYOTID)
        BEGIN
            -- Update existing record
            UPDATE tblCYOTParticipant
            SET 
                IsCompleted = @IsCompleted,
                IsStarted = @IsStarted,
                CYOTStatusID = @CYOTStatusID
            WHERE 
                StudentID = @StudentID AND CYOTID = @CYOTID;
        END
        ELSE
        BEGIN
            -- Insert new record
            INSERT INTO tblCYOTParticipant (StudentID, CYOTID, IsCompleted, IsStarted, CYOTStatusID)
            VALUES (@StudentID, @CYOTID, @IsCompleted, @IsStarted, @CYOTStatusID);
        END"
                ;
                foreach (var request in requests)
                {
                    // Execute the query for each request
                    await _connection.ExecuteAsync(query, request);
                }

                return new ServiceResponse<bool>(true, "All participant records processed successfully.", true, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, $"Error: {ex.Message}", false, 500);
            }
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
    }
}
