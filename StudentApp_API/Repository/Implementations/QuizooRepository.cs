using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using System.Data;

namespace StudentApp_API.Repository.Implementations
{
    public class QuizooRepository : IQuizooRepository
    {
        private readonly IDbConnection _connection;

        public QuizooRepository(IDbConnection connection)
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
        public async Task<ServiceResponse<int>> InsertOrUpdateQuizooAsync(QuizooDTO quizoo)
        {
            try
            {
                // Step 1: Validate Quiz Start Time
                if (quizoo.QuizooStartTime <= DateTime.Now.AddMinutes(15))
                {
                    return new ServiceResponse<int>(false, "Quiz start time must be at least 15 minutes from the current time.", 0, 400);
                }

                // Step 2: Validate against existing quizzes for the same user
                var conflictingQuiz = await _connection.QueryFirstOrDefaultAsync<DateTime?>(
                    @"SELECT TOP 1 QuizooStartTime
              FROM tblQuizoo
              WHERE CreatedBy = @CreatedBy 
                AND ABS(DATEDIFF(MINUTE, QuizooStartTime, @QuizooStartTime)) < 15",
                    new { quizoo.CreatedBy, quizoo.QuizooStartTime });

                if (conflictingQuiz.HasValue)
                {
                    return new ServiceResponse<int>(false,
                        $"A quiz is already scheduled at {conflictingQuiz.Value}. Ensure at least a 15-minute gap between quizzes.",
                        0, 400);
                }

                int quizooId;
                quizoo.IsSystemGenerated = false;

                // Step 3: Insert or Update `tblQuizoo`
                if (quizoo.QuizooID == 0)
                {
                    // Insert
                    var insertQuery = @"
                INSERT INTO tblQuizoo (
                    QuizooName, QuizooDate, QuizooStartTime, Duration, NoOfQuestions, 
                    NoOfPlayers, QuizooLink, CreatedBy, QuizooDuration, IsSystemGenerated, CreatedOn,
                    ClassID, CourseID, BoardID
                ) VALUES (
                    @QuizooName, @QuizooDate, @QuizooStartTime, @Duration, @NoOfQuestions, 
                    @NoOfPlayers, @QuizooLink, @CreatedBy, @QuizooDuration, @IsSystemGenerated, GETDATE()
                    @ClassID, @CourseID, @BoardID
                ); 
                SELECT CAST(SCOPE_IDENTITY() as int)";

                    quizooId = await _connection.ExecuteScalarAsync<int>(insertQuery, quizoo);
                    quizoo.QuizooLink = $"iGuruQuizooLink/{quizooId}";
                    await _connection.ExecuteAsync(@"update tblQuizoo set QuizooLink = @QuizooLink", new { QuizooLink  = quizoo.QuizooLink});
                }
                else
                {
                    // Update
                    quizoo.QuizooLink = $"iGuruQuizooLink/{quizoo.QuizooID}";
                    var updateQuery = @"
                UPDATE tblQuizoo
                SET 
                    QuizooName = @QuizooName, QuizooDate = @QuizooDate, QuizooStartTime = @QuizooStartTime, 
                    Duration = @Duration, NoOfQuestions = @NoOfQuestions, NoOfPlayers = @NoOfPlayers, 
                    QuizooLink = @QuizooLink, CreatedBy = @CreatedBy, QuizooDuration = @QuizooDuration, 
                    IsSystemGenerated = @IsSystemGenerated, ClassID = @ClassID, CourseID = @CourseID, 
                    BoardID = @BoardID
                WHERE QuizooID = @QuizooID";

                    await _connection.ExecuteAsync(updateQuery, quizoo);
                    quizooId = quizoo.QuizooID;
                }

                // Step 4: Insert or Update `tblQuizooSyllabus`
                var insertSyllabusQuery = @"
            INSERT INTO tblQuizooSyllabus (QuizooID, SubjectID, ChapterID)
            VALUES (@QuizooID, @SubjectID, @ChapterID)";

                foreach (var syllabus in quizoo.QuizooSyllabus)
                {
                    await _connection.ExecuteAsync(insertSyllabusQuery, new
                    {
                        QuizooID = quizooId,
                        SubjectID = syllabus.SubjectID,
                        ChapterID = syllabus.ChapterID
                    });
                }

                return new ServiceResponse<int>(true, "Quizoo inserted/updated successfully.", quizooId, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<int>(false, $"Error: {ex.Message}", 0, 500);
            }
        }
        public async Task<ServiceResponse<bool>> UpdateQuizooSyllabusAsync(int quizooId, List<QuizooSyllabusDTO> syllabusList)
        {

            try
            {
                // Step 1: Delete existing records for the QuizooID
                var deleteQuery = "DELETE FROM tblQuizooSyllabus WHERE QuizooID = @QuizooID";
                await _connection.ExecuteAsync(deleteQuery, new { QuizooID = quizooId });

                // Step 2: Insert new records
                var insertQuery = @"
                    INSERT INTO tblQuizooSyllabus (QuizooID, SubjectID, ChapterID)
                    VALUES (@QuizooID, @SubjectID, @ChapterID)";

                foreach (var syllabus in syllabusList)
                {
                    await _connection.ExecuteAsync(insertQuery, new
                    {
                        QuizooID = quizooId,
                        SubjectID = syllabus.SubjectID,
                        ChapterID = syllabus.ChapterID
                    });
                }

                return new ServiceResponse<bool>(true, "Syllabus updated successfully.", true, 200);
            }
            catch (Exception ex)
            {

                return new ServiceResponse<bool>(false, $"Error: {ex.Message}", false, 500);
            }

        }
        public async Task<ServiceResponse<List<QuizooDTOResponse>>> GetQuizoosByRegistrationIdAsync(int registrationId)
        {
            try
            {
                var query = @"
            SELECT 
                QuizooID,
                QuizooName,
                QuizooDate,
                QuizooStartTime,
                Duration,
                NoOfQuestions,
                NoOfPlayers,
                QuizooLink,
                CreatedBy,
                QuizooDuration,
                IsSystemGenerated,
                ClassID,
                CourseID,
                BoardID,
                CreatedOn
            FROM [tblQuizoo]
            WHERE CreatedBy = @RegistrationId
            ORDER BY CreatedOn DESC";

                // Fetch the quizzes
                var result = await _connection.QueryAsync<QuizooDTOResponse>(query, new { RegistrationId = registrationId });

                // Loop through each quizoo to determine the status
                foreach (var quizoo in result)
                {
                    // Extract numeric value from the Duration string
                    if (!string.IsNullOrWhiteSpace(quizoo.Duration) && quizoo.Duration.Contains("min"))
                    {
                        var durationNumericPart = quizoo.Duration.Split(' ')[0];
                        if (double.TryParse(durationNumericPart, out double durationInMinutes))
                        {
                            DateTime quizooEndTime = quizoo.QuizooStartTime.AddMinutes(durationInMinutes);
                            DateTime currentTime = DateTime.Now;

                            // Determine the quizoo status
                            if (currentTime < quizoo.QuizooStartTime)
                            {
                                quizoo.QuizooStatus = "Upcoming"; // Quiz is in the future
                            }
                            else if (currentTime >= quizoo.QuizooStartTime && currentTime <= quizooEndTime)
                            {
                                quizoo.QuizooStatus = "Ongoing"; // Quiz is currently ongoing
                            }
                            else
                            {
                                quizoo.QuizooStatus = "Completed"; // Quiz has ended
                            }

                            // Check if the user has joined and attempted the quiz
                            var playerAnswersQuery = @"
                        SELECT COUNT(*) 
                        FROM [tblQuizooPlayersAnswers] 
                        WHERE QuizooID = @QuizooID AND StudentID = @StudentID";

                            var playerAttempts = await _connection.ExecuteScalarAsync<int>(playerAnswersQuery, new { QuizooID = quizoo.QuizooID, StudentID = registrationId });

                            if (playerAttempts == 0)
                            {
                                if (currentTime < quizoo.QuizooStartTime)
                                {
                                    quizoo.QuizooStatus = "Not joined"; // The quiz hasn't started yet, and no attempts have been made
                                }
                                else
                                {
                                    quizoo.QuizooStatus = "Missed"; // The quiz has ended, and no attempts were made
                                }
                            }
                        }
                        else
                        {
                            throw new Exception($"Invalid Duration format: {quizoo.Duration}");
                        }
                    }
                    else
                    {
                        throw new Exception("Duration is null or in an unexpected format.");
                    }
                }

                return new ServiceResponse<List<QuizooDTOResponse>>(true, "Quizoos fetched successfully.", result.AsList(), 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuizooDTOResponse>>(false, $"Error: {ex.Message}", new List<QuizooDTOResponse>(), 500);
            }
        }
        public async Task<ServiceResponse<List<QuizooDTOResponse>>> GetInvitedQuizoosByRegistrationId(int registrationId)
        {
            try
            {
                var query = @"
            SELECT 
                q.QuizooID,
                q.QuizooName,
                q.QuizooDate,
                q.QuizooStartTime,
                q.Duration,
                q.NoOfQuestions,
                q.NoOfPlayers,
                q.QuizooLink,
                q.CreatedBy,
                CONCAT(r.FirstName, ' ', r.LastName) AS CreatedByName, -- Fetching the full name
                q.QuizooDuration,
                q.IsSystemGenerated,
                q.ClassID,
                q.CourseID,
                q.BoardID,
                q.CreatedOn
            FROM tblQuizooInvitation qi
            INNER JOIN tblQuizoo q ON qi.QuizooID = q.QuizooID
            LEFT JOIN tblRegistration r ON q.CreatedBy = r.RegistrationID -- Join to fetch inviter name
            WHERE qi.QInvitee = @RegistrationId
            ORDER BY q.CreatedOn DESC";

                // Fetch the list of quizzes the user is invited to
                var result = await _connection.QueryAsync<QuizooDTOResponse>(query, new { RegistrationId = registrationId });

                // Loop through each quizoo to determine the status
                foreach (var quizoo in result)
                {
                    // Extract numeric value from the Duration string
                    if (!string.IsNullOrWhiteSpace(quizoo.Duration) && quizoo.Duration.Contains("min"))
                    {
                        var durationNumericPart = quizoo.Duration.Split(' ')[0];
                        if (double.TryParse(durationNumericPart, out double durationInMinutes))
                        {
                            DateTime quizooEndTime = quizoo.QuizooStartTime.AddMinutes(durationInMinutes);
                            DateTime currentTime = DateTime.Now;

                            // Determine the quizoo status
                            if (currentTime < quizoo.QuizooStartTime)
                            {
                                quizoo.QuizooStatus = "Upcoming"; // Quiz is in the future
                            }
                            else if (currentTime >= quizoo.QuizooStartTime && currentTime <= quizooEndTime)
                            {
                                quizoo.QuizooStatus = "Ongoing"; // Quiz is currently ongoing
                            }
                            else
                            {
                                quizoo.QuizooStatus = "Completed"; // Quiz has ended
                            }

                            // Check if the user has joined and attempted the quiz
                            var playerAnswersQuery = @"
                        SELECT COUNT(*) 
                        FROM [tblQuizooPlayersAnswers] 
                        WHERE QuizooID = @QuizooID AND StudentID = @StudentID";

                            var playerAttempts = await _connection.ExecuteScalarAsync<int>(playerAnswersQuery, new { QuizooID = quizoo.QuizooID, StudentID = registrationId });

                            if (playerAttempts == 0)
                            {
                                if (currentTime < quizoo.QuizooStartTime)
                                {
                                    quizoo.QuizooStatus = "Not joined"; // The quiz hasn't started yet, and no attempts have been made
                                }
                                else
                                {
                                    quizoo.QuizooStatus = "Missed"; // The quiz has ended, and no attempts were made
                                }
                            }
                        }
                        else
                        {
                            throw new Exception($"Invalid Duration format: {quizoo.Duration}");
                        }
                    }
                    else
                    {
                        throw new Exception("Duration is null or in an unexpected format.");
                    }
                }

                return new ServiceResponse<List<QuizooDTOResponse>>(true, "Quizoos invited successfully fetched.", result.AsList(), 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuizooDTOResponse>>(false, $"Error: {ex.Message}", new List<QuizooDTOResponse>(), 500);
            }
        }
        public async Task<ServiceResponse<QuizooDTOResponse>> GetQuizooByIdAsync(int quizooId)
        {
            try
            {
                // Query to fetch quiz details
                var quizQuery = @"
            SELECT 
                QuizooID, QuizooName, QuizooDate, QuizooStartTime, Duration, NoOfQuestions, 
                NoOfPlayers, QuizooLink, CreatedBy, QuizooDuration, IsSystemGenerated, 
                ClassID, CourseID, BoardID
            FROM tblQuizoo
            WHERE QuizooID = @QuizooID";

                var quizoo = await _connection.QueryFirstOrDefaultAsync<QuizooDTOResponse>(quizQuery, new { QuizooID = quizooId });

                if (quizoo == null)
                {
                    return new ServiceResponse<QuizooDTOResponse>(false, "Quizoo not found.", null, 404);
                }

                // Query to fetch associated syllabus details
                var syllabusQuery = @"
            SELECT 
                qs.QuizooID, qs.SubjectID, qs.ChapterID, 
                s.SubjectName, c.ContentName_Chapter
            FROM tblQuizooSyllabus qs
            INNER JOIN tblSubject s ON qs.SubjectID = s.SubjectID
            INNER JOIN tblContentIndexChapters c ON qs.ChapterID = c.ContentIndexId
            WHERE qs.QuizooID = @QuizooID";

                var syllabus = await _connection.QueryAsync<QuizooSyllabusDTO>(syllabusQuery, new { QuizooID = quizooId });

                quizoo.QuizooSyllabus = syllabus.ToList();

                return new ServiceResponse<QuizooDTOResponse>(true, "Quizoo retrieved successfully.", quizoo, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<QuizooDTOResponse>(false, $"Error: {ex.Message}", null, 500);
            }
        }
    }
}