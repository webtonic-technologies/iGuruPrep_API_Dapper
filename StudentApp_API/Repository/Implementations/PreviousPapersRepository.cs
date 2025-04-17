using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using System.Data;
using StudentApp_API.Models;

public class PreviousPapersRepository
{
    private readonly IDbConnection _connection;

    public PreviousPapersRepository(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<ServiceResponse<IEnumerable<StudentApp_API.DTOs.Response.Course>>> GetCoursesByStudentAsync(int registrationId)
    {
        const string query = @"
        SELECT 
            c.CourseId,
            c.CourseName,
            c.CourseCode
        FROM 
            tblStudentClassCourseMapping sccm
        INNER JOIN 
            tblCourse c ON sccm.CourseID = c.CourseId
        WHERE 
            sccm.RegistrationID = @RegistrationID;";

        // Ensure the connection is open
        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }

        try
        {
            // Execute the query and return the result
            var resposne = await _connection.QueryAsync<StudentApp_API.DTOs.Response.Course>(query, new { RegistrationID = registrationId });
            if (!resposne.Any())
            {
                return new ServiceResponse<IEnumerable<StudentApp_API.DTOs.Response.Course>>(false, "No records found", [], 404);
            }
            else
            {
                return new ServiceResponse<IEnumerable<StudentApp_API.DTOs.Response.Course>>(true, "Records found", resposne, 200);
            }
        }
        catch (Exception ex)
        {
            return new ServiceResponse<IEnumerable<StudentApp_API.DTOs.Response.Course>>(false, ex.Message, [], 500);
        }
        finally
        {
            // Optionally close the connection if you prefer to manage it manually
            _connection.Close();
        }
    }
    public async Task<ServiceResponse<IEnumerable<TestSeriesDto>>> GetTestSeriesByStudentAsync(GetTestSeriesList request)
    {
        try
        {
            // Query to fetch BoardID and ClassID for the given RegistrationID and CourseID
            const string mappingQuery = @"
        SELECT 
            BoardId,
            ClassID
        FROM 
            tblStudentClassCourseMapping
        WHERE 
            RegistrationID = @RegistrationID AND
            CourseID = @CourseID;";

            // Execute the mapping query
            var mapping = await _connection.QueryFirstOrDefaultAsync(mappingQuery, new { RegistrationID = request.registrationId, CourseID = request.courseId });

            if (mapping == null)
            {
                // Handle case where no mapping is found
                return new ServiceResponse<IEnumerable<TestSeriesDto>>(false, "No records found", Enumerable.Empty<TestSeriesDto>(), 404);
            }

            int boardId = mapping.BoardId;
            int classId = mapping.ClassID;

            // Base query to fetch Test Series matching the BoardID, ClassID, and CourseID
            var testSeriesQuery = @"
        SELECT 
            ts.TestSeriesId AS Id,
            ts.NameOfExam AS Name,
            ts.Duration,
            ts.TotalNoOfQuestions AS TotalQuestions,
            ts.StartDate,
            ts.StartTime
        FROM 
            tblTestSeries ts
        INNER JOIN 
            tblTestSeriesBoards tsb ON ts.TestSeriesId = tsb.TestSeriesId
        INNER JOIN 
            tblTestSeriesClass tsc ON ts.TestSeriesId = tsc.TestSeriesId
        INNER JOIN 
            tblTestSeriesCourse tscs ON ts.TestSeriesId = tscs.TestSeriesId
        WHERE 
            tsb.BoardId = @BoardId AND
            tsc.ClassId = @ClassId AND
            tscs.CourseId = @CourseId";

            // Modify the query based on the IsStarted filter
            if (request.IsStarted.HasValue)
            {
                if (request.IsStarted.Value)
                {
                    // Fetch test series that the student has started
                    testSeriesQuery += @"
                AND EXISTS (
                    SELECT 1
                    FROM tblPreviousPapers pp
                    WHERE pp.TestSeriesid = ts.TestSeriesId
                    AND pp.Studentid = @RegistrationID
                    AND pp.IsStarted = 1
                )";
                }
                else
                {
                    // Fetch test series that the student has not started
                    testSeriesQuery += @"
                AND NOT EXISTS (
                    SELECT 1
                    FROM tblPreviousPapers pp
                    WHERE pp.TestSeriesid = ts.TestSeriesId
                    AND pp.Studentid = @RegistrationID
                )";
                }
            }

            // Execute the test series query
            var testSeriesList = await _connection.QueryAsync<TestSeriesDto>(testSeriesQuery, new { BoardId = boardId, ClassId = classId, CourseId = request.courseId, RegistrationID = request.registrationId });

            if (!testSeriesList.Any())
            {
                return new ServiceResponse<IEnumerable<TestSeriesDto>>(false, "No records found", Enumerable.Empty<TestSeriesDto>(), 404);
            }
            else
            {
                return new ServiceResponse<IEnumerable<TestSeriesDto>>(true, "Records found", testSeriesList, 200);
            }
        }
        catch (Exception ex)
        {
            return new ServiceResponse<IEnumerable<TestSeriesDto>>(false, ex.Message, Enumerable.Empty<TestSeriesDto>(), 500);
        }
    }
    public async Task<ServiceResponse<string>> EnrollInTestSeriesAsync(int registrationId, int testSeriesId)
    {
        try
        {
            // Check if the user has already signed up for the test series
            const string checkQuery = @"
            SELECT COUNT(1)
            FROM tblPreviousPapers
            WHERE Studentid = @StudentId AND TestSeriesid = @TestSeriesId;";

            var exists = await _connection.ExecuteScalarAsync<bool>(checkQuery, new
            {
                StudentId = registrationId,
                TestSeriesId = testSeriesId
            });

            if (exists)
            {
                return new ServiceResponse<string>(false, "You have already signed up for this test series.", string.Empty, 500);
            }
            else
            {
                // Insert a new record into tblPreviousPapers
                const string insertQuery = @"
                INSERT INTO tblPreviousPapers (TestSeriesid, Studentid, IsStarted)
                VALUES (@TestSeriesId, @StudentId, @IsStarted);";

                await _connection.ExecuteAsync(insertQuery, new
                {
                    TestSeriesId = testSeriesId,
                    StudentId = registrationId,
                    IsStarted = true
                });

                const string questionQuery = @"
    SELECT tsq.Questionid AS QuestionId,
           tsq.QuestionCode
    FROM tbltestseriesQuestions tsq
    WHERE tsq.TestSeriesid = @TestSeriesId";
                var questions = (await _connection.QueryAsync<TestSeriesQuestions>(questionQuery, new { TestSeriesId = testSeriesId })).ToList();

                var studentQuestionMappingQuery = @"
        INSERT INTO tblPreviousPaperQuestions (TestSeriesid, Studentid, Questionid, QuestionStatusid)
        VALUES (@TestSeriesid, @Studentid, @Questionid, @QuestionStatusid)";
                await _connection.ExecuteAsync(studentQuestionMappingQuery, questions.Select((q, index) => new
                {
                    TestSeriesid = testSeriesId,
                    Studentid = registrationId,
                    Questionid = q.QuestionId,
                    QuestionStatusid = 4
                }));
                return new ServiceResponse<string>(true, "Operation Successful", "Enrollent done successfully", 200);
            }
        }
        catch (Exception ex)
        {
            return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
        }
    }
    public async Task<ServiceResponse<IEnumerable<TestSeriesSubjects>>> GetTestSeriesContentAsync(int testSeriesId)
    {
        try
        {


            var query = @"
        SELECT 
            s.SubjectID, s.SubjectName,
            c.ContentIndexId, c.ContentName_Chapter,
            t.ContInIdTopic, t.ContentName_Topic,
            st.ContInIdSubTopic, st.ContentName_SubTopic
        FROM tblTestSeriesSubjects ts
        INNER JOIN tblSubjects s ON ts.SubjectID = s.SubjectID
        LEFT JOIN tblContentIndexChapters c ON s.SubjectID = c.SubjectId
        LEFT JOIN tblContentIndexTopics t ON c.ContentIndexId = t.ContentIndexId
        LEFT JOIN tblContentIndexSubTopics st ON t.ContInIdTopic = st.ContInIdTopic
        WHERE ts.TestSeriesID = @TestSeriesID
        ORDER BY s.SubjectID, c.ContentIndexId, t.ContInIdTopic, st.ContInIdSubTopic;";

            var subjectDictionary = new Dictionary<int, TestSeriesSubjects>();

            var result = await _connection.QueryAsync<TestSeriesSubjects, ContentIndexResponse, ContentIndexTopics, ContentIndexSubTopic, TestSeriesSubjects>(
                query,
                (subject, chapter, topic, subtopic) =>
                {
                    if (!subjectDictionary.TryGetValue(subject.SubjectID, out var subjectEntry))
                    {
                        subjectEntry = subject;
                        subjectEntry.TestSeriesContentIndexes = new List<ContentIndexResponse>();
                        subjectDictionary.Add(subject.SubjectID, subjectEntry);
                    }

                    var chapterEntry = subjectEntry.TestSeriesContentIndexes
                        .FirstOrDefault(c => c.ContentIndexId == chapter.ContentIndexId);

                    if (chapterEntry == null && chapter.ContentIndexId != 0)
                    {
                        chapterEntry = chapter;
                        chapterEntry.ContentIndexTopics = new List<ContentIndexTopics>();
                        subjectEntry.TestSeriesContentIndexes.Add(chapterEntry);
                    }

                    var topicEntry = chapterEntry?.ContentIndexTopics
                        .FirstOrDefault(t => t.ContInIdTopic == topic.ContInIdTopic);

                    if (topicEntry == null && topic.ContInIdTopic != 0)
                    {
                        topicEntry = topic;
                        topicEntry.ContentIndexSubTopics = new List<ContentIndexSubTopic>();
                        chapterEntry?.ContentIndexTopics.Add(topicEntry);
                    }

                    if (subtopic.ContInIdSubTopic != 0)
                    {
                        topicEntry?.ContentIndexSubTopics.Add(subtopic);
                    }

                    return subjectEntry;
                },
                new { TestSeriesID = testSeriesId },
                splitOn: "ContentIndexId,ContInIdTopic,ContInIdSubTopic"
            );
            if (subjectDictionary.Values.Any())
            {
                return new ServiceResponse<IEnumerable<TestSeriesSubjects>>(true, "Records found", subjectDictionary.Values, 200);
            }
            else
            {
                return new ServiceResponse<IEnumerable<TestSeriesSubjects>>(false, "Records not found", [], 404);
            }
        }
        catch (Exception ex)
        {
            return new ServiceResponse<IEnumerable<TestSeriesSubjects>>(false, ex.Message, [], 500);
        }
    }
    public async Task<ServiceResponse<TestSeriesResponseDTO?>> GetTestSeriesInstructionsAsync(int testSeriesId)
    {
        try
        {
            const string query = @"
    SELECT 
        ts.TestSeriesId,
        ts.TestPatternName,
        ts.Duration,
        ts.TotalNoOfQuestions,
        ts.ManualQuestionSelect,
        ts.StartDate,
        ts.StartTime,
        ts.ResultDate,
        tsi.TestInstructionsId,
        tsi.Instructions,
        tsi.TestSeriesID,
        tsi.InstructionName,
        tsi.InstructionId
    FROM 
        tblTestSeries ts
    LEFT JOIN 
        tblTestSeriesInstructions tsi ON ts.TestSeriesId = tsi.TestSeriesID
    WHERE 
        ts.TestSeriesId = @TestSeriesId;";

            var testSeriesDictionary = new Dictionary<int, TestSeriesResponseDTO>();

            var result = await _connection.QueryAsync<TestSeriesResponseDTO, TestSeriesInstructions, TestSeriesResponseDTO>(
                query,
                (testSeries, instruction) =>
                {
                    if (!testSeriesDictionary.TryGetValue(testSeries.TestSeriesId, out var testSeriesEntry))
                    {
                        testSeriesEntry = testSeries;
                        testSeriesDictionary.Add(testSeriesEntry.TestSeriesId, testSeriesEntry);
                    }

                    if (instruction != null)
                    {
                        testSeriesEntry.TestSeriesInstruction = instruction;
                    }

                    return testSeriesEntry;
                },
                new { TestSeriesId = testSeriesId },
                splitOn: "TestInstructionsId"
            );
            if (result != null)
            {
                return new ServiceResponse<TestSeriesResponseDTO?>(true, "Record found", testSeriesDictionary.Values.FirstOrDefault(), 200);
            }
            else
            {
                return new ServiceResponse<TestSeriesResponseDTO?>(false, "no Record found", null, 404);
            }
        }
        catch (Exception ex)
        {
            return new ServiceResponse<TestSeriesResponseDTO?>(false, ex.Message, null, 500);
        }
    }
    public async Task<ServiceResponse<List<PreviousPaperTestSeriesSubjectDetails>>> GetTestSeriesQuestionsAsync(int testSeriesId)
    {
        const string subjectQuery = @"
    SELECT tss.SubjectID, s.SubjectName
    FROM tblTestSeriesSubjects tss
    JOIN tblSubject s ON tss.SubjectID = s.SubjectID
    WHERE tss.TestSeriesID = @TestSeriesId";

        const string sectionQuery = @"
    SELECT tsqs.testseriesQuestionSectionid AS SectionId,
           tsqs.SectionName,
           tsqs.QuestionTypeID,
           tsqs.SubjectId
    FROM tbltestseriesQuestionSection tsqs
    WHERE tsqs.TestSeriesID = @TestSeriesId";

        const string questionQuery = @"
    SELECT tsq.Questionid AS QuestionId,
           tsq.QuestionCode,
           q.QuestionDescription,
           q.QuestionTypeId,
           q.SubjectID,
           q.IndexTypeId,
           q.ContentIndexId,
           q.Explanation,
           q.ExtraInformation,
           q.Paragraph,
           q.ParentQId,
           q.ParentQCode
    FROM tbltestseriesQuestions tsq
    JOIN tblQuestions q ON tsq.Questionid = q.QuestionID
    WHERE tsq.TestSeriesid = @TestSeriesId";


        var subjects = (await _connection.QueryAsync<PreviousPaperTestSeriesSubjectDetails>(subjectQuery, new { TestSeriesId = testSeriesId })).ToList();
        var sections = (await _connection.QueryAsync<QuestionSection>(sectionQuery, new { TestSeriesId = testSeriesId })).ToList();
        var questions = (await _connection.QueryAsync<TestSeriesQuestions>(questionQuery, new { TestSeriesId = testSeriesId })).ToList();
        var response = questions.Select(item =>
        {
            if (item.QuestionTypeId == 11)
            {
                return new TestSeriesQuestions
                {
                    QuestionId = item.QuestionId,
                    QuestionDescription = item.QuestionDescription,
                    ParentQCode = item.ParentQCode,
                    ParentQId = item.ParentQId,
                    Paragraph = item.Paragraph,
                    SubjectName = item.SubjectName,
                    IndexTypeName = item.IndexTypeName,
                    ContentIndexName = item.ContentIndexName,
                    ContentIndexId = item.ContentIndexId,
                    IndexTypeId = item.IndexTypeId,
                    subjectID = item.subjectID,
                    QuestionTypeId = item.QuestionTypeId,
                    QuestionTypeName = item.QuestionTypeName,
                    QuestionCode = item.QuestionCode,
                    Explanation = item.Explanation,
                    ExtraInformation = item.ExtraInformation,
                    ComprehensiveChildQuestions = GetChildQuestions(item.QuestionCode),
                };
            }
            else
            {
                return new TestSeriesQuestions
                {
                    QuestionId = item.QuestionId,
                    QuestionDescription = item.QuestionDescription,
                    QuestionTypeId = item.QuestionTypeId,
                    subjectID = item.subjectID,
                    SubjectName = item.SubjectName,
                    IndexTypeId = item.IndexTypeId,
                    IndexTypeName = item.IndexTypeName,
                    ContentIndexId = item.ContentIndexId,
                    ContentIndexName = item.ContentIndexName,
                    QuestionTypeName = item.QuestionTypeName,
                    QuestionCode = item.QuestionCode,
                    Explanation = item.Explanation,
                    ExtraInformation = item.ExtraInformation,
                    MatchPairs = item.QuestionTypeId == 6 || item.QuestionTypeId == 12 ? GetMatchPairs(item.QuestionCode, item.QuestionId) : null,
                    MatchThePairType2Answers = item.QuestionTypeId == 12 ? GetMatchThePairType2Answers(item.QuestionCode, item.QuestionId) : null,
                    Answersingleanswercategories = (item.QuestionTypeId != 6 && item.QuestionTypeId != 12) ? GetSingleAnswer(item.QuestionCode, item.QuestionId) : null,
                    AnswerMultipleChoiceCategories = (item.QuestionTypeId != 12) ? GetMultipleAnswers(item.QuestionCode) : null
                };
            }
        }
                ).ToList();
        // Mapping sections to subjects
        foreach (var subject in subjects)
        {
            subject.TestSeriesQuestionsSection = sections
                .Where(s => s.SubjectId == subject.SubjectID)
                .ToList();
        }

        // Mapping questions to respective sections
        foreach (var section in sections)
        {
            section.QuestionsList = response
                .Where(q => q.QuestionTypeId == section.QuestionTypeId && q.subjectID == section.SubjectId)
                .ToList();
        }
        if (subjects.Any())
        {
            return new ServiceResponse<List<PreviousPaperTestSeriesSubjectDetails>>(true, "records found", subjects, 200);
        }
        else
        {
            return new ServiceResponse<List<PreviousPaperTestSeriesSubjectDetails>>(false, "records not found", [], 404);
        }
        // return subjects;
    }
    public async Task<ServiceResponse<string>> UpdateQuestionStatusAsync(int testSeriesId, int studentId, int questionId, bool isAnswered)
    {
        // Define status IDs
        const int Answered = 1;
        const int NotVisited = 4;
        const int Review = 3;
        const int ReviewWithAnswer = 5;
        const int Unanswered = 2;
        // Check current status
        var currentStatusId = await _connection.QuerySingleOrDefaultAsync<int>(
            @"SELECT QuestionStatusid
                  FROM tblPreviousPaperQuestions
                  WHERE TestSeriesid = @TestSeriesid AND Studentid = @Studentid AND Questionid = @Questionid",
            new { TestSeriesid = testSeriesId, Studentid = studentId, Questionid = questionId });

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
                @"INSERT INTO tblPreviousPaperQuestions (TestSeriesid, Studentid, Questionid, QuestionStatusid)
                      VALUES (@TestSeriesid, @Studentid, @Questionid, @QuestionStatusid)",
                new { TestSeriesid = testSeriesId, Studentid = studentId, Questionid = questionId, QuestionStatusid = newStatusId });
        }
        else
        {
            // Update existing mapping
            await _connection.ExecuteAsync(
                @"UPDATE tblPreviousPaperQuestions
                      SET QuestionStatusid = @QuestionStatusid
                      WHERE TestSeriesid = @TestSeriesid AND Studentid = @Studentid AND Questionid = @Questionid",
                new { QuestionStatusid = newStatusId, TestSeriesid = testSeriesId, Studentid = studentId, Questionid = questionId });
        }

        // Log the review
        await _connection.ExecuteAsync(
            @"INSERT INTO tblPreviousQuestionReviewed (QuestionId, StudentId, TestSeriesid)
                  VALUES (@QuestionId, @StudentId, @TestSeriesid)",
            new { QuestionId = questionId, StudentId = studentId, TestSeriesid = testSeriesId });

        return new ServiceResponse<string>(true, "status updated successfully", string.Empty, 200);
    }
    public async Task<ServiceResponse<string>> MarkQuestionAsSave(SaveQuestionPreviousPaperRequest request)
    {
        try
        {
            // Check if the record already exists
            string checkQuery = @"
            SELECT COUNT(1)
            FROM tblPPSaveQuestion
            WHERE Studentid = @RegistrationId
              AND Questionid = @QuestionId AND TestSeriesid = @TestSeriesid";

            int recordExists = await _connection.ExecuteScalarAsync<int>(checkQuery, new
            {
                request.RegistrationId,
                request.QuestionId,
                TestSeriesid = request.TestSeriesId
            });

            if (recordExists > 0)
            {
                // If record exists, delete it
                string deleteQuery = @"
                DELETE FROM tblPPSaveQuestion
                WHERE Studentid = @RegistrationId
                  AND Questionid = @QuestionId AND TestSeriesid = @TestSeriesid";

                await _connection.ExecuteAsync(deleteQuery, new
                {
                    Studentid = request.RegistrationId,
                    Questionid = request.QuestionId,
                    TestSeriesid = request.TestSeriesId
                });

                return new ServiceResponse<string>(true, "Question unsaved (deleted).", null, 200);
            }
            else
            {
                // If record does not exist, insert it
                string insertQuery = @"
                INSERT INTO tblPPSaveQuestion (Studentid, Questionid, SubjectId, TestSeriesid)
                VALUES (@Studentid, @Questionid, @SubjectId, @TestSeriesid)";

                await _connection.ExecuteAsync(insertQuery, new
                {
                    Studentid = request.RegistrationId,
                    Questionid = request.QuestionId,
                    request.SubjectId,
                    TestSeriesid = request.TestSeriesId
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

           // var data = GetQuestionById(questionId);

            return new ServiceResponse<string>(true, "Question shared successfully.", $"Shared with {classmates.Count} classmates", 200);
        }
        catch (Exception ex)
        {
            return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
        }

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
    private List<StudentApp_API.DTOs.Response.MatchThePairAnswer> GetMatchThePairType2Answers(string questionCode, int questionId)
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
            return new List<StudentApp_API.DTOs.Response.MatchThePairAnswer>();
        }

        return _connection.Query<StudentApp_API.DTOs.Response.MatchThePairAnswer>(getAnswersQuery, new { AnswerId = answerId }).ToList();

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