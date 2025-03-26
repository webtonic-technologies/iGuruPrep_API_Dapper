using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.Responses;
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

                    subject.ChapterCount = chapters.Count();
                }

                return new ServiceResponse<List<SubjectDTO>>(true, "Records found", subjects, 200, subjects.Count);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<SubjectDTO>>(false, ex.Message, new List<SubjectDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<List<ChapterDTO>>> GetChaptersAsync(int registrationId, List<int> subjectIds)
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

                // Step 3: Fetch Chapters mapped to SyllabusID and multiple SubjectIDs
                var chapters = (await _connection.QueryAsync<ChapterDTO>(
                    @"SELECT C.ContentIndexId AS ChapterId, C.ContentName_Chapter AS ChapterName, C.ChapterCode, C.DisplayOrder
              FROM tblSyllabusDetails S
              INNER JOIN tblContentIndexChapters C ON S.ContentIndexId = C.ContentIndexId
              WHERE S.SyllabusID = @SyllabusID 
                AND S.SubjectID IN @SubjectIDs
                AND S.IndexTypeId = 1",
                    new { SyllabusID = actualSyllabusId, SubjectIDs = subjectIds })).ToList();

                if (!chapters.Any())
                    return new ServiceResponse<List<ChapterDTO>>(false, "No chapters found for the given SyllabusID and SubjectIDs.", new List<ChapterDTO>(), 404);
                int totalChapterCount = 0;
                // Step 4: Fetch and assign the count of topics and subtopics for each chapter
                foreach (var chapter in chapters)
                {
                    // Count Topics directly mapped to the chapter and syllabus
                    var topicCount = await _connection.QueryFirstOrDefaultAsync<int>(
                        @"SELECT COUNT(*)
                  FROM tblContentIndexTopics T
                  INNER JOIN tblSyllabusDetails S ON S.ContentIndexId = T.ContInIdTopic
                  WHERE T.ChapterCode = @ChapterCode 
                    AND T.Status = 1 
                    AND T.IsActive = 1
                    AND S.IndexTypeId = 2
                    AND S.Status = 1",
                        new { ChapterCode = chapter.ChapterCode });

                    // Count Subtopics mapped to the topics of the chapter
                    var subTopicCount = await _connection.QueryFirstOrDefaultAsync<int>(
                        @"SELECT COUNT(*)
                  FROM tblContentIndexSubTopics ST
                  INNER JOIN tblContentIndexTopics T ON ST.TopicCode = T.TopicCode
                  WHERE T.ChapterCode = @ChapterCode 
                    AND ST.Status = 1 
                    AND ST.IsActive = 1",
                        new { ChapterCode = chapter.ChapterCode });

                    // Assign the total count of concepts (topics + subtopics) to the chapter
                    chapter.ConceptCount = topicCount;//+ subTopicCount;
                    totalChapterCount = totalChapterCount + chapter.ConceptCount;
                }
                return new ServiceResponse<List<ChapterDTO>>(true, "Records found", chapters, 200, totalChapterCount);
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
                if (quizoo.QuizooStartTime <= DateTime.Now.AddMinutes(15) && quizoo.QuizooID == 0)
                {
                    return new ServiceResponse<int>(false, "Quiz start time must be at least 15 minutes from the current time.", 0, 400);
                }
                if (quizoo.QuizooSyllabus == null || !quizoo.QuizooSyllabus.Any() || quizoo.QuizooSyllabus.Any(s => s.SubjectID <= 0 || s.ChapterID <= 0))
                {
                    throw new ArgumentException("At least one valid subject and chapter must be selected.");
                }
                var duration = int.Parse(quizoo.Duration.Split(' ')[0]);
                // Step 2: Validate against existing quizzes for the same user (considering duration)
                var conflictingQuiz = await _connection.QueryFirstOrDefaultAsync<DateTime?>(
                    @"SELECT TOP 1 QuizooStartTime 
      FROM tblQuizoo
      WHERE CreatedBy = @CreatedBy 
        AND (
              (@QuizooStartTime BETWEEN QuizooStartTime AND DATEADD(MINUTE, TRY_CAST(LEFT(Duration, CHARINDEX(' ', Duration) - 1) AS INT), QuizooStartTime)) OR
              (DATEADD(MINUTE, @Duration, @QuizooStartTime) BETWEEN QuizooStartTime AND DATEADD(MINUTE, TRY_CAST(LEFT(Duration, CHARINDEX(' ', Duration) - 1) AS INT), QuizooStartTime)) OR
              (QuizooStartTime BETWEEN @QuizooStartTime AND DATEADD(MINUTE, @Duration, @QuizooStartTime))
            )
      ORDER BY QuizooID DESC;",
                    new
                    {
                        quizoo.CreatedBy,
                        quizoo.QuizooStartTime,
                        Duration = duration    // Extracting duration in minutes
                    });

                if (conflictingQuiz.HasValue)
                {
                    return new ServiceResponse<int>(false,
                        $"A quiz is already scheduled at {conflictingQuiz.Value}. Ensure no overlap between quizzes.",
                        0, 400);
                }
                int quizooId;
                string queryMapping = @"
            SELECT TOP 1 [BoardId], [ClassID], [CourseID]
            FROM [tblStudentClassCourseMapping]
            WHERE [RegistrationID] = @RegistrationId";

                var studentMapping = await _connection.QueryFirstOrDefaultAsync<StudentClassCourseMappings>(
                    queryMapping, new { RegistrationId = quizoo.CreatedBy });

                var quizooDto = new
                {
                    QuizooID = quizoo.QuizooID,
                    QuizooName = quizoo.QuizooName,
                    QuizooDate = quizoo.QuizooDate,
                    QuizooStartTime = quizoo.QuizooStartTime,
                    Duration = quizoo.Duration,
                    NoOfQuestions = quizoo.NoOfQuestions,
                    NoOfPlayers = quizoo.NoOfPlayers,
                    CreatedBy = quizoo.CreatedBy,
                    IsSystemGenerated = false,
                    ClassID = studentMapping.ClassId,
                    CourseID = studentMapping.CourseId,
                    BoardID = studentMapping.BoardId
                };
                // Step 3: Insert or Update `tblQuizoo`
                if (quizoo.QuizooID == 0)
                {
                    // Insert
                    var insertQuery = @"
                INSERT INTO tblQuizoo (
                    QuizooName, QuizooDate, QuizooStartTime, Duration, NoOfQuestions, 
                    NoOfPlayers, CreatedBy, IsSystemGenerated, CreatedOn,
                    ClassID, CourseID, BoardID
                ) VALUES (
                    @QuizooName, @QuizooDate, @QuizooStartTime, @Duration, @NoOfQuestions, 
                    @NoOfPlayers, @CreatedBy, @IsSystemGenerated, GETDATE(),
                    @ClassID, @CourseID, @BoardID
                ); 
                SELECT CAST(SCOPE_IDENTITY() as int)";

                    quizooId = await _connection.ExecuteScalarAsync<int>(insertQuery, quizooDto);
                    string quizooByteCode = EncryptionHelper.EncryptString(quizooId.ToString());
                    string registrationByteCode = EncryptionHelper.EncryptString(quizoo.CreatedBy.ToString());
                    quizoo.QuizooLink = $"https://www.xmtopper.com/quizo/{quizooByteCode}/{registrationByteCode}";
                    await _connection.ExecuteAsync(@"update tblQuizoo set QuizooLink = @QuizooLink where QuizooID = @quizooId", new { QuizooLink = quizoo.QuizooLink, quizooId = quizooId });
                }
                else
                {
                    // Update
                    var updateQuery = @"
                UPDATE tblQuizoo
                SET 
                    QuizooName = @QuizooName, QuizooDate = @QuizooDate, QuizooStartTime = @QuizooStartTime, 
                    Duration = @Duration, NoOfQuestions = @NoOfQuestions, NoOfPlayers = @NoOfPlayers, CreatedBy = @CreatedBy, 
                    IsSystemGenerated = @IsSystemGenerated, ClassID = @ClassID, CourseID = @CourseID, 
                    BoardID = @BoardID
                WHERE QuizooID = @QuizooID";

                    await _connection.ExecuteAsync(updateQuery, quizooDto);
                    quizooId = quizoo.QuizooID;
                }
                if (quizoo.QuizooID != 0)
                {
                    await UpdateQuizooSyllabusAsync(quizoo.QuizooID, quizoo.QuizooSyllabus);
                }
                else
                {
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
                if (syllabusList == null || !syllabusList.Any() || syllabusList.Any(s => s.SubjectID <= 0 || s.ChapterID <= 0))
                {
                    throw new ArgumentException("At least one valid subject and chapter must be selected.");
                }
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
        public async Task<ServiceResponse<List<QuizooDTOResponse>>> GetQuizoosByRegistrationIdAsync(QuizooListFilters request)
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
        q.IsSystemGenerated,
        q.ClassID,
        q.CourseID,
        q.BoardID,
        q.CreatedOn,
        COUNT(p.StudentId) AS Players -- Count of players attended the quiz
    FROM 
        [tblQuizoo] q
    LEFT JOIN 
        [tblQuizooParticipants] p ON q.QuizooID = p.QuizooId
    WHERE 
        q.CreatedBy = @RegistrationId AND q.IsSystemGenerated = 0
    GROUP BY 
        q.QuizooID, q.QuizooName, q.QuizooDate, q.QuizooStartTime, q.Duration, 
        q.NoOfQuestions, q.NoOfPlayers, q.QuizooLink, q.CreatedBy, 
        q.IsSystemGenerated, q.ClassID, q.CourseID, q.BoardID, q.CreatedOn
    ORDER BY 
        q.CreatedOn DESC";

            //    var query = @"
            //SELECT 
            //    QuizooID,
            //    QuizooName,
            //    QuizooDate,
            //    QuizooStartTime,
            //    Duration,
            //    NoOfQuestions,
            //    NoOfPlayers,
            //    QuizooLink,
            //    CreatedBy,
            //    IsSystemGenerated,
            //    ClassID,
            //    CourseID,
            //    BoardID,
            //    CreatedOn
            //FROM [tblQuizoo]
            //WHERE CreatedBy = @RegistrationId AND IsSystemGenerated = 0
            //ORDER BY CreatedOn DESC";

                // Fetch the quizzes
                var result = await _connection.QueryAsync<QuizooDTOResponse>(query, new { RegistrationId = request.RegistrationId });
                // Map the filter integers to enum values
                var filters = request.Filters?.Select(f => (QuizooFilterType)f).ToList();

                // Loop through each quizoo to determine the status
                foreach (var quizoo in result)
                {
                    // Extract numeric value from the Duration string
                    if (!string.IsNullOrWhiteSpace(quizoo.Duration) && quizoo.Duration.Contains("min"))
                    {
                        var durationNumericPart = quizoo.Duration.Split(' ')[0];
                        if (double.TryParse(durationNumericPart, out double durationInMinutes))
                        {
                            DateTime currentTime = DateTime.Now;
                            DateTime quizooEndTime = quizoo.QuizooStartTime.AddMinutes(int.Parse(quizoo.Duration.Split(' ')[0]));

                            if ((quizoo.QuizooStartTime - currentTime).TotalMinutes > 5)
                            {
                                quizoo.QuizooStatus = "Upcoming"; // More than 5 minutes left
                            }
                            else if ((quizoo.QuizooStartTime - currentTime).TotalMinutes <= 5 && currentTime < quizoo.QuizooStartTime)
                            {
                                quizoo.QuizooStatus = "Not joined"; // Less than or equal to 5 minutes left, but quiz not started
                            }
                            else if (currentTime >= quizoo.QuizooStartTime && currentTime <= quizooEndTime)
                            {
                                quizoo.QuizooStatus = "Ongoing"; // Quiz is currently ongoing
                            }

                            // Check if the user has joined and attempted the quiz
                            var playerAnswersQuery = @"
                        SELECT COUNT(*) 
                        FROM [tblQuizooPlayersAnswers] 
                        WHERE QuizooID = @QuizooID AND StudentID = @StudentID";

                            var playerAttempts = await _connection.ExecuteScalarAsync<int>(playerAnswersQuery, new { QuizooID = quizoo.QuizooID, StudentID = request.RegistrationId });

                            DateTime quizooEndTime1 = quizoo.QuizooStartTime.AddMinutes(int.Parse(quizoo.Duration.Split(' ')[0]));

                            if (playerAttempts == 0)
                            {
                                if (currentTime > quizooEndTime1)
                                {
                                    quizoo.ShowCorrectAnswers = true;
                                    quizoo.ShowLeaderBoard = true;
                                    quizoo.QuizooStatus = "Missed"; // The quiz duration has ended, and no attempts were made
                                }
                            }
                            if (playerAttempts != 0)
                            {
                                if (currentTime > quizooEndTime1)
                                {
                                    quizoo.ShowCorrectAnswers = true;
                                    quizoo.ShowLeaderBoard = true;
                                    quizoo.QuizooStatus = "Completed"; // The quiz duration has ended, and no attempts were made
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
                // Apply filters to the result
                if (filters != null && filters.Count > 0 && !filters.Contains(0))
                {
                    result = result.Where(q => filters.Contains(Enum.Parse<QuizooFilterType>(q.QuizooStatus.Replace(" ", "")))).ToList();
                }
                if (!result.Any())
                {
                    return new ServiceResponse<List<QuizooDTOResponse>>(false, "No records found", [], 404);
                }
                return new ServiceResponse<List<QuizooDTOResponse>>(true, "Quizoos fetched successfully.", result.AsList(), 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuizooDTOResponse>>(false, $"Error: {ex.Message}", new List<QuizooDTOResponse>(), 500);
            }
        }
        public async Task<ServiceResponse<List<QuizooDTOResponse>>> GetInvitedQuizoosByRegistrationId(QuizooListFilters request)
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
        q.IsSystemGenerated,
        q.ClassID,
        q.CourseID,
        q.BoardID,
        q.CreatedOn,
        COUNT(p.StudentId) AS Players -- Count of players attended the quiz
    FROM tblQuizooInvitation qi
    INNER JOIN tblQuizoo q ON qi.QuizooID = q.QuizooID
    LEFT JOIN tblRegistration r ON q.CreatedBy = r.RegistrationID -- Join to fetch inviter name
    LEFT JOIN tblQuizooParticipants p ON q.QuizooID = p.QuizooId -- Join to get the player count
    WHERE qi.QInvitee = @RegistrationId
    GROUP BY 
        q.QuizooID, q.QuizooName, q.QuizooDate, q.QuizooStartTime, q.Duration, 
        q.NoOfQuestions, q.NoOfPlayers, q.QuizooLink, q.CreatedBy, 
        r.FirstName, r.LastName, q.IsSystemGenerated, q.ClassID, 
        q.CourseID, q.BoardID, q.CreatedOn
    ORDER BY q.CreatedOn DESC";

                //    var query = @"
                //SELECT 
                //    q.QuizooID,
                //    q.QuizooName,
                //    q.QuizooDate,
                //    q.QuizooStartTime,
                //    q.Duration,
                //    q.NoOfQuestions,
                //    q.NoOfPlayers,
                //    q.QuizooLink,
                //    q.CreatedBy,
                //    CONCAT(r.FirstName, ' ', r.LastName) AS CreatedByName, -- Fetching the full name
                //    q.IsSystemGenerated,
                //    q.ClassID,
                //    q.CourseID,
                //    q.BoardID,
                //    q.CreatedOn
                //FROM tblQuizooInvitation qi
                //INNER JOIN tblQuizoo q ON qi.QuizooID = q.QuizooID
                //LEFT JOIN tblRegistration r ON q.CreatedBy = r.RegistrationID -- Join to fetch inviter name
                //WHERE qi.QInvitee = @RegistrationId
                //ORDER BY q.CreatedOn DESC";

                // Fetch the list of quizzes the user is invited to
                var result = await _connection.QueryAsync<QuizooDTOResponse>(query, new { RegistrationId = request.RegistrationId });

                // Map the filter integers to enum values
                var filters = request.Filters?.Select(f => (QuizooFilterType)f).ToList();
                // Loop through each quizoo to determine the status
                foreach (var quizoo in result)
                {
                    // Extract numeric value from the Duration string
                    if (!string.IsNullOrWhiteSpace(quizoo.Duration) && quizoo.Duration.Contains("min"))
                    {
                        var durationNumericPart = quizoo.Duration.Split(' ')[0];
                        if (double.TryParse(durationNumericPart, out double durationInMinutes))
                        {
                            DateTime currentTime = DateTime.Now;
                            DateTime quizooEndTime = quizoo.QuizooStartTime.AddMinutes(int.Parse(quizoo.Duration.Split(' ')[0]));

                            if ((quizoo.QuizooStartTime - currentTime).TotalMinutes > 5)
                            {
                                quizoo.QuizooStatus = "Upcoming"; // More than 5 minutes left
                            }
                            else if ((quizoo.QuizooStartTime - currentTime).TotalMinutes <= 5 && currentTime < quizoo.QuizooStartTime)
                            {
                                quizoo.QuizooStatus = "Not joined"; // Less than or equal to 5 minutes left, but quiz not started
                            }
                            else if (currentTime >= quizoo.QuizooStartTime && currentTime <= quizooEndTime)
                            {
                                quizoo.QuizooStatus = "Ongoing"; // Quiz is currently ongoing
                            }

                            // Check if the user has joined and attempted the quiz
                            var playerAnswersQuery = @"
                        SELECT COUNT(*) 
                        FROM [tblQuizooPlayersAnswers] 
                        WHERE QuizooID = @QuizooID AND StudentID = @StudentID";

                            var playerAttempts = await _connection.ExecuteScalarAsync<int>(playerAnswersQuery, new { QuizooID = quizoo.QuizooID, StudentID = request.RegistrationId });

                            DateTime quizooEndTime1 = quizoo.QuizooStartTime.AddMinutes(int.Parse(quizoo.Duration.Split(' ')[0]));

                            if (playerAttempts == 0)
                            {
                                if (currentTime > quizooEndTime1)
                                {
                                    quizoo.ShowCorrectAnswers = true;
                                    quizoo.ShowLeaderBoard = true;
                                    quizoo.QuizooStatus = "Missed"; // The quiz duration has ended, and no attempts were made
                                }
                            }
                            if (playerAttempts != 0)
                            {
                                if (currentTime > quizooEndTime1)
                                {
                                    quizoo.ShowCorrectAnswers = true;
                                    quizoo.ShowLeaderBoard = true;
                                    quizoo.QuizooStatus = "Completed"; // The quiz duration has ended, and no attempts were made
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
                // Apply filters to the result
                if (filters != null && filters.Count > 0 && !filters.Contains(0))
                {
                    result = result.Where(q => filters.Contains(Enum.Parse<QuizooFilterType>(q.QuizooStatus.Replace(" ", "")))).ToList();
                }
                if (!result.Any())
                {
                    return new ServiceResponse<List<QuizooDTOResponse>>(false, "No records found", [], 404);
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
                NoOfPlayers, QuizooLink, CreatedBy, IsSystemGenerated, 
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
                qs.QuizooID, qs.SubjectID, qs.ChapterID, qs.QSID as QSID,
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
        public async Task<ServiceResponse<List<QuizooDTOResponse>>> GetOnlineQuizoosByRegistrationIdAsync(QuizooListFilters request)
        {
            try
            {
                var query = @"
WITH Classmates AS (
    SELECT DISTINCT sc.RegistrationID
    FROM tblStudentClassCourseMapping sc
    JOIN tblStudentClassCourseMapping sc2 ON 
        sc.CourseID = sc2.CourseID 
        AND sc.ClassID = sc2.ClassID 
        AND sc.BoardId = sc2.BoardId
    WHERE sc2.RegistrationID = @RegistrationId
)
SELECT 
    q.QuizooID,
    q.QuizooName,
    q.QuizooDate,
    q.QuizooStartTime,
    q.NoOfQuestions,
    q.NoOfPlayers,
    q.Duration,
    q.QuizooLink,
    q.CreatedBy,
    q.IsSystemGenerated,
    q.ClassID,
    q.CourseID,
    q.BoardID,
    q.CreatedOn,
    COUNT(op.StudentID) AS Players -- Count of online players who attended the quiz
FROM 
    tblQuizoo q
LEFT JOIN tblQuizooOnlinePlayers op ON q.QuizooID = op.QuizooID -- Join to get the online player count
WHERE 
    (q.CreatedBy = @RegistrationId OR q.CreatedBy IN (SELECT RegistrationID FROM Classmates))
    AND q.IsSystemGenerated = 1
GROUP BY 
    q.QuizooID, q.QuizooName, q.QuizooDate, q.QuizooStartTime, q.NoOfQuestions, 
    q.NoOfPlayers, q.Duration, q.QuizooLink, q.CreatedBy, q.IsSystemGenerated, 
    q.ClassID, q.CourseID, q.BoardID, q.CreatedOn
ORDER BY 
    q.CreatedOn DESC;";
                //                var query = @"
                //WITH Classmates AS (
                //    SELECT DISTINCT sc.RegistrationID
                //    FROM tblStudentClassCourseMapping sc
                //    JOIN tblStudentClassCourseMapping sc2 ON 
                //        sc.CourseID = sc2.CourseID 
                //        AND sc.ClassID = sc2.ClassID 
                //        AND sc.BoardId = sc2.BoardId
                //    WHERE sc2.RegistrationID = @RegistrationId
                //)
                //SELECT 
                //    q.QuizooID,
                //    q.QuizooName,
                //    q.QuizooDate,
                //    q.QuizooStartTime,
                //    q.NoOfQuestions,
                //    q.NoOfPlayers,
                //    q.Duration,
                //    q.QuizooLink,
                //    q.CreatedBy,
                //    q.IsSystemGenerated,
                //    q.ClassID,
                //    q.CourseID,
                //    q.BoardID,
                //    q.CreatedOn
                //FROM 
                //    tblQuizoo q
                //WHERE 
                //    (q.CreatedBy = @RegistrationId OR q.CreatedBy IN (SELECT RegistrationID FROM Classmates))
                //    AND q.IsSystemGenerated = 1
                //ORDER BY 
                //    q.CreatedOn DESC;";

                // Fetch the quizzes
                var result = await _connection.QueryAsync<QuizooDTOResponse>(query, new { RegistrationId = request.RegistrationId });


                // Map the filter integers to enum values
                var filters = request.Filters?.Select(f => (QuizooFilterType)f).ToList();
                // Loop through each quizoo to determine the status
                foreach (var quizoo in result)
                {
                    try
                    {
                        // Trim and check the Duration string
                        if (!string.IsNullOrWhiteSpace(quizoo.Duration))
                        {
                            quizoo.Duration = quizoo.Duration.Trim();

                            // Check if the duration contains "min" and extract the numeric value
                            if (quizoo.Duration.EndsWith("min", StringComparison.OrdinalIgnoreCase))
                            {
                                var durationNumericPart = quizoo.Duration.Replace("min", "").Trim();

                                if (double.TryParse(durationNumericPart, out double durationInMinutes))
                                {
                                    DateTime quizooEndTime = quizoo.QuizooStartTime.AddMinutes(durationInMinutes);
                                    DateTime currentTime = DateTime.Now;

                                    // Determine the quizoo status
                                    if (currentTime >= quizoo.QuizooStartTime && currentTime <= quizooEndTime)
                                    {
                                        quizoo.QuizooStatus = "Ongoing"; // Quiz is currently ongoing
                                    }
                                    else
                                    {
                                        quizoo.ShowCorrectAnswers = true;
                                        quizoo.ShowLeaderBoard = true;
                                        quizoo.QuizooStatus = "Completed"; // Quiz has ended
                                    }
                                }
                                else
                                {
                                    throw new Exception($"Invalid Duration format: {quizoo.Duration}");
                                }
                            }
                            else
                            {
                                throw new Exception($"Unexpected duration format (missing 'min'): {quizoo.Duration}");
                            }
                        }
                        else
                        {
                            throw new Exception("Duration is null or empty.");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle specific duration errors without breaking the entire loop
                        quizoo.QuizooStatus = $"Error: {ex.Message}";
                    }
                }
                if (filters != null && filters.Count > 0 && !filters.Contains(0))
                {
                    result = result.Where(q => filters.Contains(Enum.Parse<QuizooFilterType>(q.QuizooStatus.Replace(" ", "")))).ToList();
                }
                if (!result.Any())
                {
                    return new ServiceResponse<List<QuizooDTOResponse>>(false, "No records found", [], 404);
                }
                return new ServiceResponse<List<QuizooDTOResponse>>(true, "Quizoos fetched successfully.", result.AsList(), 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuizooDTOResponse>>(false, $"Error: {ex.Message}", new List<QuizooDTOResponse>(), 500);
            }
        }
        public async Task<ServiceResponse<string>> ShareQuizooAsync(int studentId, int quizooId)
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

                // Step 3: Insert a shared Quizoo record for each classmate
                string insertQuery = @"
        INSERT INTO tblQuizooInvitation (QuizooID, QInviter, QInvitee)
        VALUES (@QuizooId, @SharedBy, @SharedTo)";

                int totalInserted = 0;
                foreach (var classmateId in classmates)
                {
                    int rows = await _connection.ExecuteAsync(insertQuery, new
                    {
                        QuizooId = quizooId,
                        SharedBy = studentId,
                        SharedTo = classmateId
                    });
                    totalInserted += rows;
                }

                return new ServiceResponse<string>(true, $"Quizoo shared successfully with {classmates.Count} classmates.", $"Total Shared: {totalInserted}", 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<string>> ValidateQuizStartAsync(int quizooId, int studentId)
        {
            try
            {
                // Step 1: Check if the user is the creator or an invited participant
                var isAuthorized = await _connection.ExecuteScalarAsync<int>(@"
            SELECT 
                CASE 
                    WHEN q.CreatedBy = @StudentId THEN 1
                    WHEN EXISTS (
                        SELECT 1 FROM tblQuizooInvitation 
                        WHERE QuizooID = @QuizooID AND QInvitee = @StudentId
                    ) THEN 1
                    ELSE 0
                END AS IsAuthorized
            FROM tblQuizoo q
            WHERE q.QuizooID = @QuizooID;
        ", new { QuizooID = quizooId, StudentId = studentId });

                if (isAuthorized == 0)
                {
                    return new ServiceResponse<string>(false, "You are not authorized to start this quiz.", string.Empty, 403);
                }

                // Step 2: Fetch the quiz status and end time
                var result = await _connection.QueryFirstOrDefaultAsync<(bool IsDismissed, DateTime QuizooStartTime, string Duration)>(@"
            SELECT IsDismissed, QuizooStartTime, Duration
            FROM tblQuizoo
            WHERE QuizooID = @QuizooID;
        ", new { QuizooID = quizooId });

                if (result.IsDismissed)
                {
                    // Calculate the quiz end time
                    int durationMinutes = int.Parse(result.Duration.Split(' ')[0]); // Extracting duration as an integer
                    DateTime quizooEndTime = result.QuizooStartTime.AddMinutes(durationMinutes);

                    // Check if the quiz end time has also passed
                    if (DateTime.Now > quizooEndTime)
                    {
                        return new ServiceResponse<string>(false, "Quizoo is dismissed as well as ended.", "Dismissed and Ended", 400);
                    }

                    return new ServiceResponse<string>(false, "Quizoo is dismissed.", "Dismissed", 400);
                }

                // Step 3: Check if the student has already participated and not force exited
                var existingParticipant = await _connection.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*)
            FROM tblQuizooParticipants
            WHERE QuizooId = @QuizooID AND StudentId = @StudentId AND IsForceExit = 0;
        ", new { QuizooID = quizooId, StudentId = studentId });

                if (existingParticipant > 0)
                {
                    return new ServiceResponse<string>(false, "You have already participated in the quiz.", "Already Participated", 400);
                }

                // Step 4: Insert participant record after validation
                await _connection.ExecuteAsync(@"
            INSERT INTO tblQuizooParticipants (QuizooId, StudentId, IsForceExit)
            VALUES (@QuizooID, @StudentId, 0);
        ", new { QuizooID = quizooId, StudentId = studentId });

                return new ServiceResponse<string>(true, "Quiz can be started and participation recorded.", "Participated", 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, $"Error: {ex.Message}", string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<bool>> CheckAndDismissQuizAsync(int quizooId)
        {
            try
            {
                // Step 1: Fetch the quiz start time and duration
                var quizData = await _connection.QueryFirstOrDefaultAsync<(DateTime QuizooStartTime, string Duration)>(@"
            SELECT QuizooStartTime, Duration 
            FROM tblQuizoo
            WHERE QuizooID = @QuizooID;
        ", new { QuizooID = quizooId });

                if (quizData == default)
                {
                    return new ServiceResponse<bool>(false, "Quiz not found.", false, 404);
                }

                // Calculate the quiz end time
                int durationMinutes = int.Parse(quizData.Duration.Split(' ')[0]); // Extract duration as an integer
                DateTime quizooEndTime = quizData.QuizooStartTime.AddMinutes(durationMinutes);
                DateTime currentTime = DateTime.Now;

                // Step 2: Check the number of participants
                var participantCount = await _connection.ExecuteScalarAsync<int>(@"
            SELECT COUNT(*) 
            FROM tblQuizooParticipants 
            WHERE QuizooId = @QuizooID;
        ", new { QuizooID = quizooId });

                // Step 3: Dismiss the quiz if:
                // - The current time has passed the quiz end time
                // - There are fewer than 2 participants
                if (currentTime > quizooEndTime && participantCount < 2)
                {
                    await _connection.ExecuteAsync(@"
                UPDATE tblQuizoo 
                SET IsDismissed = 1 
                WHERE QuizooID = @QuizooID;
            ", new { QuizooID = quizooId });

                    return new ServiceResponse<bool>(false, "Quiz dismissed due to insufficient participants and end time passed.", false, 400);
                }

                return new ServiceResponse<bool>(true, "Quiz has sufficient participants or has not ended yet.", true, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, $"Error: {ex.Message}", false, 500);
            }
        }
        public async Task<ServiceResponse<List<ParticipantDto>>> GetParticipantsAsync(int quizooId, int studentId)
        {
            try
            {
                // Step 1: Get the current user first
                var currentUser = await _connection.QueryAsync<ParticipantDto>(@"
            SELECT p.StudentId, p.IsForceExit, 
                   CONCAT(r.FirstName, ' ', r.LastName) AS FullName, 
                   r.Photo
            FROM tblQuizooParticipants p
            INNER JOIN tblRegistration r ON p.StudentId = r.RegistrationID
            WHERE p.QuizooId = @QuizooID AND p.StudentId = @StudentID;
        ", new { QuizooID = quizooId, StudentID = studentId });

                // Step 2: Get remaining users excluding the current user
                var remainingUsers = await _connection.QueryAsync<ParticipantDto>(@"
            SELECT p.StudentId, p.IsForceExit, 
                   CONCAT(r.FirstName, ' ', r.LastName) AS FullName, 
                   r.Photo
            FROM tblQuizooParticipants p
            INNER JOIN tblRegistration r ON p.StudentId = r.RegistrationID
            WHERE p.QuizooId = @QuizooID AND p.StudentId <> @StudentID;
        ", new { QuizooID = quizooId, StudentID = studentId });

                // Combine the current user and remaining users
                var participants = currentUser.Concat(remainingUsers).ToList();

                return new ServiceResponse<List<ParticipantDto>>(true, "Participants list fetched successfully.", participants, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ParticipantDto>>(false, $"Error: {ex.Message}", null, 500);
            }
        }
        public async Task<ServiceResponse<int>> SetForceExitAsync(int QuizooID, int StudentID)
        {
            const string query = "UPDATE tblQuizooParticipants SET IsForceExit = 1 WHERE QuizooId = @QuizooID and StudentId = @StudentID";
            try
            {
                int rowsAffected = await _connection.ExecuteAsync(query, new { QuizooID, StudentID });

                if (rowsAffected == 0)
                {
                    return new ServiceResponse<int>(false, "Operation failed: No record updated. Check if QPID is valid.", 0, 404);
                }

                return new ServiceResponse<int>(true, "Operation successful", rowsAffected, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<int>(false, ex.Message, 0, 500);
            }
        }
    }
}