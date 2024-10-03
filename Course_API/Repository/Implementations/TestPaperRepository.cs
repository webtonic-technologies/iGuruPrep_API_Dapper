using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;
using Course_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace Course_API.Repository.Implementations
{
    public class TestPaperRepository : ITestPaperRepository
    {
        private readonly IDbConnection _connection;

        public TestPaperRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<int>> AddUpdateTestPaper(TestPaperRequestDTO request)
        {
            try
            {
                _connection.Open();
                using (var transaction = _connection.BeginTransaction())
                {
                    // Insert or update TestPaper
                    int testPaperId = await AddOrUpdateTestPaper(request, _connection, transaction);

                    // Insert or update related entities (Board, Class, Course, Subject)
                    if (request.TestPaperBoards != null && request.TestPaperBoards.Count > 0)
                    {
                        await InsertTestPaperBoards(request.TestPaperBoards, testPaperId, _connection, transaction);
                    }

                    if (request.TestPaperClasses != null && request.TestPaperClasses.Count > 0)
                    {
                        await InsertTestPaperClasses(request.TestPaperClasses, testPaperId, _connection, transaction);
                    }

                    if (request.TestPaperCourses != null && request.TestPaperCourses.Count > 0)
                    {
                        await InsertTestPaperCourses(request.TestPaperCourses, testPaperId, _connection, transaction);
                    }

                    if (request.TestPaperSubjects != null && request.TestPaperSubjects.Count > 0)
                    {
                        await InsertTestPaperSubjects(request.TestPaperSubjects, testPaperId, _connection, transaction);
                    }

                    transaction.Commit();
                    return new ServiceResponse<int>(true, "TestPaper saved successfully.", testPaperId, 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<int>(false, ex.Message, 0, 0);
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task<ServiceResponse<TestPaperResponseDTO>> GetTestPaperById(int TestPaperId)
        {
            try
            {
                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }

                // Fetch the basic test paper details
                var testPaperQuery = @"
            SELECT TP.TestPaperId, TP.APID, AP.Name AS APName, TP.ExamTypeId, ET.Name AS ExamTypeName,
                   TP.PatternName, TP.TotalNumberOfQuestions, TP.Duration, TP.Status, TP.NameOfExam,
                   TP.ConductedDate, TP.ConductedTime, TP.createdon, TP.createdby, TP.modifiedon, 
                   TP.modifiedby, TP.EmployeeID, E.EmployeeName
            FROM tblTestPaper TP
            JOIN tblAP AP ON TP.APID = AP.APID
            left JOIN tblExamType ET ON TP.ExamTypeId = ET.ExamTypeId
            JOIN tblEmployee E ON TP.EmployeeID = E.EmployeeID
            WHERE TP.TestPaperId = @TestPaperId";

                var testPaper = await _connection.QueryFirstOrDefaultAsync<TestPaperResponseDTO>(testPaperQuery, new { TestPaperId });

                if (testPaper == null)
                {
                    return new ServiceResponse<TestPaperResponseDTO>(false, "Test paper not found", null, 404);
                }
               

                // Initialize the SubjectDetails list
                var testPaperSubjectDetailsList = new List<TestPaperSubjectDetails>();

                var testPaperContentIndexes = await GetTestPaperSubjectDetails(TestPaperId);
                var testPaperQuestionsSections = await GetTestPaperQuestionSection(TestPaperId);
                // Fetch associated boards, classes, and courses
                testPaper.TestPaperBoards = await GetTestPaperBoardsByTestPaperId(TestPaperId);
                testPaper.TestPaperClasses = await GetTestPaperClassesByTestPaperId(TestPaperId);
                testPaper.TestPaperCourses = await GetTestPaperCoursesByTestPaperId(TestPaperId);
                testPaper.TestPaperSubjects = await GetTestPaperSubjectsByTestPaperId(TestPaperId);
                testPaper.TestPaperInstruction = await GetTestPaperInstructions(TestPaperId);
                // Populate TestPaperSubjectDetails with content indexes and questions section
                foreach (var subject in testPaper.TestPaperSubjects)
                {
                    var subjectContentIndexes = testPaperContentIndexes
                        .Where(ci => ci.SubjectId == subject.SubjectId)
                        .ToList();

                    var subjectQuestionsSections = testPaperQuestionsSections
                        .Where(qs => qs.SubjectId == subject.SubjectId)
                        .ToList();

                    var subjectDetails = new TestPaperSubjectDetails
                    {
                        SubjectID = subject.SubjectId,
                        SubjectName = subject.Name,
                        TestSeriesContentIndexes = subjectContentIndexes,
                        TestSeriesQuestionsSection = subjectQuestionsSections
                    };

                    testPaperSubjectDetailsList.Add(subjectDetails);
                }
                if (testPaperQuestionsSections != null && testPaperQuestionsSections.Any())
                {
                    testPaper.TestPaperQuestions = new List<TestPaperQuestions>();
                    foreach (var section in testPaperQuestionsSections)
                    {
                        var questions = await GetTestPaperQuestions(section.TestPaperSectionId);
                        if (questions != null)
                        {
                            testPaper.TestPaperQuestions.AddRange(questions);
                        }
                    }
                }
                return new ServiceResponse<TestPaperResponseDTO>(true, "Test paper details fetched successfully", testPaper, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<TestPaperResponseDTO>(false, $"Error: {ex.Message}", null, 500);
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
            }
        }
        public Task<ServiceResponse<List<TestPaperResponseDTO>>> GetTestPaperList(TestPaperGetListRequest request)
        {
            throw new NotImplementedException();
        }
        public async Task<ServiceResponse<string>> TestPaperContentIndexMapping(List<TestPaperContentIndex>? request, int TestPaperId)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    return new ServiceResponse<string>(false, "No content indices provided", string.Empty, 400);
                }

                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }

                using (var transaction = _connection.BeginTransaction())
                {
                    // Step 1: Delete existing mappings for this TestPaperId
                    var deleteQuery = "DELETE FROM tblTestPaperContentIndex WHERE TestPaperId = @TestPaperId";
                    await _connection.ExecuteAsync(deleteQuery, new { TestPaperId }, transaction);

                    // Step 2: Insert new mappings from the request
                    var insertQuery = @"
                INSERT INTO tblTestPaperContentIndex (IndexTypeId, ContentIndexId, TestPaperId, SubjectId) 
                VALUES (@IndexTypeId, @ContentIndexId, @TestPaperId, @SubjectId)";

                    foreach (var index in request)
                    {
                        index.TestPaperId = TestPaperId; // Ensure TestPaperId is correctly set
                        await _connection.ExecuteAsync(insertQuery, index, transaction);
                    }

                    transaction.Commit();
                    return new ServiceResponse<string>(true, "Test Paper Content Index mapping updated successfully", string.Empty, 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, $"Error: {ex.Message}", string.Empty, 500);
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
            }
        }
        public async Task<ServiceResponse<string>> TestPaperInstructionsMapping(List<TestPaperInstructions>? request, int TestPaperId)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    return new ServiceResponse<string>(false, "No instructions provided", string.Empty, 400);
                }

                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }

                using (var transaction = _connection.BeginTransaction())
                {
                    // Step 1: Delete existing instructions for this TestPaperId
                    var deleteQuery = "DELETE FROM tblTestPaperInstructions WHERE TestPaperId = @TestPaperId";
                    await _connection.ExecuteAsync(deleteQuery, new { TestPaperId }, transaction);

                    // Step 2: Insert new instructions from the request
                    var insertQuery = @"
                INSERT INTO tblTestPaperInstructions (Instructions, TestPaperId) 
                VALUES (@Instructions, @TestPaperId)";

                    foreach (var instruction in request)
                    {
                        instruction.TestPaperId = TestPaperId; // Ensure TestPaperId is set
                        await _connection.ExecuteAsync(insertQuery, instruction, transaction);
                    }

                    transaction.Commit();
                    return new ServiceResponse<string>(true, "Test Paper Instructions mapping updated successfully", string.Empty, 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, $"Error: {ex.Message}", string.Empty, 500);
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
            }
        }
        public async Task<ServiceResponse<string>> TestPaperQuestionSectionMapping(List<TestPaperQuestionSection>? request, int TestPaperId)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    return new ServiceResponse<string>(false, "No question sections provided", string.Empty, 400);
                }

                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }

                using (var transaction = _connection.BeginTransaction())
                {
                    // Step 1: Delete existing sections for the TestPaperId
                    var deleteQuery = "DELETE FROM tblTestPaperQuestionSection WHERE TestPaperId = @TestPaperId";
                    await _connection.ExecuteAsync(deleteQuery, new { TestPaperId }, transaction);

                    // Step 2: Insert new sections from the request
                    var insertQuery = @"
                INSERT INTO tblTestPaperQuestionSection (
                    TestPaperId, DisplayOrder, SectionName, QuestionTypeId, TotalNumberOfQuestions,
                    MarksPerQuestion, NegativeMarks, NoOfQuestionsPerChoice, LevelId1, QuesPerDifficulty1, 
                    LevelId2, QuesPerDifficulty2, LevelId3, QuesPerDifficulty3, SubjectId)
                VALUES (
                    @TestPaperId, @DisplayOrder, @SectionName, @QuestionTypeId, @TotalNumberOfQuestions,
                    @MarksPerQuestion, @NegativeMarks, @NoOfQuestionsPerChoice, @LevelId1, @QuesPerDifficulty1, 
                    @LevelId2, @QuesPerDifficulty2, @LevelId3, @QuesPerDifficulty3, @SubjectId)";

                    foreach (var section in request)
                    {
                        section.TestPaperId = TestPaperId; // Ensure TestPaperId is set
                        await _connection.ExecuteAsync(insertQuery, section, transaction);
                    }

                    transaction.Commit();
                    return new ServiceResponse<string>(true, "Test Paper Question Section mapping updated successfully", string.Empty, 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, $"Error: {ex.Message}", string.Empty, 500);
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
            }
        }
        public async Task<ServiceResponse<string>> TestPaperQuestionsMapping(List<TestPaperQuestions>? request, int TestPaperId, int sectionId)
        {
            try
            {
                if (request == null || request.Count == 0)
                {
                    return new ServiceResponse<string>(false, "No questions provided", string.Empty, 400);
                }

                if (_connection.State == ConnectionState.Closed)
                {
                    _connection.Open();
                }

                using (var transaction = _connection.BeginTransaction())
                {
                    // Step 1: Delete existing questions for the given TestPaperId and sectionId
                    var deleteQuery = "DELETE FROM tblTestPaperQuestions WHERE TestPaperId = @TestPaperId AND TestPaperSectionId = @TestPaperSectionId";
                    await _connection.ExecuteAsync(deleteQuery, new { TestPaperId, TestPaperSectionId = sectionId }, transaction);

                    // Step 2: Insert new questions from the request
                    var insertQuery = @"
                INSERT INTO tblTestPaperQuestions (
                    TestPaperId, SubjectId, DisplayOrder, TestPaperSectionId, QuestionId, QuestionCode)
                VALUES (
                    @TestPaperId, @SubjectId, @DisplayOrder, @TestPaperSectionId, @QuestionId, @QuestionCode)";

                    foreach (var question in request)
                    {
                        question.TestPaperId = TestPaperId;
                        question.TestPaperSectionId = sectionId; // Ensure the correct section ID is used
                        await _connection.ExecuteAsync(insertQuery, question, transaction);
                    }

                    transaction.Commit();
                    return new ServiceResponse<string>(true, "Test Paper Questions mapping updated successfully", string.Empty, 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, $"Error: {ex.Message}", string.Empty, 500);
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
            }
        }
        private async Task<int> AddOrUpdateTestPaper(TestPaperRequestDTO request, IDbConnection connection, IDbTransaction transaction)
        {
            var query = @"
        IF EXISTS (SELECT 1 FROM tblTestPaper WHERE TestPaperId = @TestPaperId)
        BEGIN
            UPDATE tblTestPaper
            SET APID = @APID,
                ExamTypeId = @ExamTypeId,
                PatternName = @PatternName,
                TotalNumberOfQuestions = @TotalNumberOfQuestions,
                Duration = @Duration,
                Status = @Status,
                NameOfExam = @NameOfExam,
                ConductedDate = @ConductedDate,
                ConductedTime = @ConductedTime,
                modifiedon = @modifiedon,
                modifiedby = @modifiedby,
                EmployeeID = @EmployeeID
            WHERE TestPaperId = @TestPaperId;
            SELECT @TestPaperId;
        END
        ELSE
        BEGIN
            INSERT INTO tblTestPaper (
                APID, ExamTypeId, PatternName, TotalNumberOfQuestions, Duration, Status, NameOfExam, 
                ConductedDate, ConductedTime, createdon, createdby, EmployeeID)
            VALUES (
                @APID, @ExamTypeId, @PatternName, @TotalNumberOfQuestions, @Duration, @Status, @NameOfExam, 
                @ConductedDate, @ConductedTime, @createdon, @createdby, @EmployeeID);
            SELECT SCOPE_IDENTITY();
        END";

            return await connection.ExecuteScalarAsync<int>(query, new
            {
                request.TestPaperId,
                request.APID,
                request.ExamTypeId,
                request.PatternName,
                request.TotalNumberOfQuestions,
                request.Duration,
                request.Status,
                request.NameOfExam,
                request.ConductedDate,
                request.ConductedTime,
                request.createdon,
                request.createdby,
                request.modifiedon,
                request.modifiedby,
                request.EmployeeID
            }, transaction);
        }
        private async Task InsertTestPaperBoards(List<TestPaperBoard> boards, int testPaperId, IDbConnection connection, IDbTransaction transaction)
        {
            var deleteQuery = "DELETE FROM tblTestPaperBoards WHERE TestPaperId = @TestPaperId";
            await connection.ExecuteAsync(deleteQuery, new { TestPaperId = testPaperId }, transaction);

            var insertQuery = @"
        INSERT INTO tblTestPaperBoards (TestPaperId, BoardId)
        VALUES (@TestPaperId, @BoardId)";

            foreach (var board in boards)
            {
                await connection.ExecuteAsync(insertQuery, new
                {
                    TestPaperId = testPaperId,
                    BoardId = board.BoardId
                }, transaction);
            }
        }
        private async Task InsertTestPaperClasses(List<TestPaperClass> classes, int testPaperId, IDbConnection connection, IDbTransaction transaction)
        {
            var deleteQuery = "DELETE FROM tblTestPaperClass WHERE TestPaperId = @TestPaperId";
            await connection.ExecuteAsync(deleteQuery, new { TestPaperId = testPaperId }, transaction);

            var insertQuery = @"
        INSERT INTO tblTestPaperClass (TestPaperId, ClassId)
        VALUES (@TestPaperId, @ClassId)";

            foreach (var classItem in classes)
            {
                await connection.ExecuteAsync(insertQuery, new
                {
                    TestPaperId = testPaperId,
                    ClassId = classItem.ClassId
                }, transaction);
            }
        }
        private async Task InsertTestPaperCourses(List<TestPaperCourse> courses, int testPaperId, IDbConnection connection, IDbTransaction transaction)
        {
            var deleteQuery = "DELETE FROM tblTestPaperCourse WHERE TestPaperId = @TestPaperId";
            await connection.ExecuteAsync(deleteQuery, new { TestPaperId = testPaperId }, transaction);

            var insertQuery = @"
        INSERT INTO tblTestPaperCourse (TestPaperId, CourseId)
        VALUES (@TestPaperId, @CourseId)";

            foreach (var course in courses)
            {
                await connection.ExecuteAsync(insertQuery, new
                {
                    TestPaperId = testPaperId,
                    CourseId = course.CourseId
                }, transaction);
            }
        }
        private async Task InsertTestPaperSubjects(List<TestPaperSubject> subjects, int testPaperId, IDbConnection connection, IDbTransaction transaction)
        {
            var deleteQuery = "DELETE FROM tblTestPaperSubject WHERE TestPaperId = @TestPaperId";
            await connection.ExecuteAsync(deleteQuery, new { TestPaperId = testPaperId }, transaction);

            var insertQuery = @"
        INSERT INTO tblTestPaperSubject (TestPaperId, SubjectId)
        VALUES (@TestPaperId, @SubjectId)";

            foreach (var subject in subjects)
            {
                await connection.ExecuteAsync(insertQuery, new
                {
                    TestPaperId = testPaperId,
                    SubjectId = subject.SubjectId
                }, transaction);
            }
        }
        private async Task<List<TestPaperBoardResponse>> GetTestPaperBoardsByTestPaperId(int TestPaperId)
        {
            var query = @"
        SELECT TB.TestPaperBoardId, TB.TestPaperId, TB.BoardId, B.Name
        FROM tblTestPaperBoard TB
        JOIN tblBoard B ON TB.BoardId = B.BoardId
        WHERE TB.TestPaperId = @TestPaperId";

            return (await _connection.QueryAsync<TestPaperBoardResponse>(query, new { TestPaperId })).ToList();
        }
        private async Task<List<TestPaperClassResponse>> GetTestPaperClassesByTestPaperId(int TestPaperId)
        {
            var query = @"
        SELECT TC.TestPaperClassId, TC.TestPaperId, TC.ClassId, C.Name
        FROM tblTestPaperClass TC
        JOIN tblClass C ON TC.ClassId = C.ClassId
        WHERE TC.TestPaperId = @TestPaperId";

            return (await _connection.QueryAsync<TestPaperClassResponse>(query, new { TestPaperId })).ToList();
        }
        private async Task<List<TestPaperCourseResponse>> GetTestPaperCoursesByTestPaperId(int TestPaperId)
        {
            var query = @"
        SELECT TPC.TestPaperCourseId, TPC.TestPaperId, TPC.CourseId, C.Name
        FROM tblTestPaperCourse TPC
        JOIN tblCourse C ON TPC.CourseId = C.CourseId
        WHERE TPC.TestPaperId = @TestPaperId";

            return (await _connection.QueryAsync<TestPaperCourseResponse>(query, new { TestPaperId })).ToList();
        }
        private async Task<List<TestPaperSubjectResponse>> GetTestPaperSubjectsByTestPaperId(int TestPaperId)
        {
            var query = @"
        SELECT TS.TestPaperSubjectId, TS.TestPaperId, TS.SubjectId, S.SubjectName AS Name
        FROM tblTestPaperSubject TS
        JOIN tblSubject S ON TS.SubjectId = S.SubjectId
        WHERE TS.TestPaperId = @TestPaperId";

            return (await _connection.QueryAsync<TestPaperSubjectResponse>(query, new { TestPaperId })).ToList();
        }
        private async Task<List<TestPaperContentIndexResponse>> GetTestPaperSubjectDetails(int TestPaperId)
        {
            string query = @"
    SELECT 
        tsci.TestPaperContIndId,
        tsci.IndexTypeId,
        it.IndexType AS IndexTypeName,
        tsci.SubjectId,
        s.SubjectName as SubjectName,
        tsci.ContentIndexId,
        CASE 
            WHEN tsci.IndexTypeId = 1 THEN ci.ContentName_Chapter
            WHEN tsci.IndexTypeId = 2 THEN ct.ContentName_Topic
            WHEN tsci.IndexTypeId = 3 THEN cst.ContentName_SubTopic
        END AS ContentIndexName,
        tsci.TestPaperId
    FROM tblTestPaperContentIndex tsci
    LEFT JOIN tblSubject s ON tsci.SubjectId = s.SubjectId
    LEFT JOIN tblQBIndexType it ON tsci.IndexTypeId = it.IndexId
    LEFT JOIN tblContentIndexChapters ci ON tsci.ContentIndexId = ci.ContentIndexId AND tsci.IndexTypeId = 1
    LEFT JOIN tblContentIndexTopics ct ON tsci.ContentIndexId = ct.ContInIdTopic AND tsci.IndexTypeId = 2
    LEFT JOIN tblContentIndexSubTopics cst ON tsci.ContentIndexId = cst.ContInIdSubTopic AND tsci.IndexTypeId = 3
    WHERE tsci.TestPaperId = @TestPaperId";

            var data = await _connection.QueryAsync<TestPaperContentIndexResponse>(query, new { TestPaperId });
            return data != null ? data.AsList() : new List<TestPaperContentIndexResponse>();
        }
        private async Task<List<TestPaperInstructions>> GetTestPaperInstructions(int TestPaperId)
        {
            var query = "SELECT * FROM tblTestPaperInstructions WHERE TestPaperId = @TestPaperId";
            return (await _connection.QueryAsync<TestPaperInstructions>(query, new { TestPaperId })).ToList();
        }
        private async Task<List<TestPaperQuestions>> GetTestPaperQuestions(int SectionId)
        {
            var query = "SELECT * FROM tblTestPaperQuestions WHERE TestPaperSectionId = @SectionId";
            return (await _connection.QueryAsync<TestPaperQuestions>(query, new { SectionId })).ToList();
        }
        public async Task<List<TestPaperQuestionSection>> GetTestPaperQuestionSection(int testPaperId)
        {
            try
            {
                // SQL query to fetch question sections for the specified TestPaperId
                var query = @"
            SELECT 
                TestPaperSectionId,
                TestPaperId,
                DisplayOrder,
                SectionName,
                QuestionTypeId,
                TotalNumberOfQuestions,
                MarksPerQuestion,
                NegativeMarks,
                NoOfQuestionsPerChoice,
                LevelId1,
                QuesPerDifficulty1,
                LevelId2,
                QuesPerDifficulty2,
                LevelId3,
                QuesPerDifficulty3,
                SubjectId
            FROM 
                tblTestPaperQuestionSection
            WHERE 
                TestPaperId = @TestPaperId";

                // Execute the query and map the results to a list of TestPaperQuestionSection
                var questionSections = await _connection.QueryAsync<TestPaperQuestionSection>(query, new { TestPaperId = testPaperId });

                return questionSections.ToList();
            }
            catch (Exception ex)
            {
                // Optionally log the exception or handle it as per your application needs
                throw new Exception("An error occurred while fetching question sections: " + ex.Message);
            }
        }
    }
}
