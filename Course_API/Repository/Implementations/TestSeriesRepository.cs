using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;
using Course_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace Course_API.Repository.Implementations
{
    public class TestSeriesRepository : ITestSeriesRepository
    {
        private readonly IDbConnection _connection;

        public TestSeriesRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateTestSeries(TestSeriesDTO request)
        {
            try
            {
                if (request.TestSeriesId == 0)
                {
                    string insertQuery = @"
                    INSERT INTO tblTestSeries 
                    (
                        TestPatternName, Duration, Status, APID, TotalNoOfQuestions, 
                        MethodofAddingType, StartDate, StartTime, ResultDate, ResultTime, 
                        EmployeeID, NameOfExam, RepeatedExams, TypeOfTestSeries, 
                        createdon, createdby
                    ) 
                    VALUES 
                    (
                        @TestPatternName, @Duration, @Status, @APID, @TotalNoOfQuestions, 
                        @MethodofAddingType, @StartDate, @StartTime, @ResultDate, @ResultTime, 
                        @EmployeeID, @NameOfExam, @RepeatedExams, @TypeOfTestSeries, 
                        @createdon, @createdby
                    ); 
                    SELECT CAST(SCOPE_IDENTITY() as int);";
                    var parameters = new
                    {
                        request.TestPatternName,
                        request.Duration,
                        request.Status,
                        request.APID,
                        request.TotalNoOfQuestions,
                        request.MethodofAddingType,
                        request.StartDate,
                        request.StartTime,
                        request.ResultDate,
                        request.ResultTime,
                        request.EmployeeID,
                        request.NameOfExam,
                        request.RepeatedExams,
                        request.TypeOfTestSeries,
                        createdon = DateTime.Now,
                        request.createdby
                    };
                    int newId = await _connection.QuerySingleAsync<int>(insertQuery, parameters);
                    if (newId > 0)
                    {
                        int sub = TestSeriesSubjectMapping(request.TestSeriesSubject ??= ([]), newId);
                        int cla = TestSeriesClassMapping(request.TestSeriesClasses ??= ([]), newId);
                        int board = TestSeriesBoardMapping(request.TestSeriesBoard ??= ([]), newId);
                        int course = TestSeriesCourseMapping(request.TestSeriesCourses ??= ([]), newId);
                        int subIn = TestSeriesContentIndexMapping(request.TestSeriesContentIndexes ??= ([]), newId);
                        int quesSec = TestSeriesQuestionSectionMapping(request.TestSeriesQuestionsSection ??= new TestSeriesQuestionSection(), newId);
                        int queType = TestSeriesQuestionTypeMapping(request.TestSeriesQuestionTypes ??= ([]), newId);
                        int inst = TestSeriesInstructionsMapping(request.TestSeriesInstruction ??= ([]), newId);
                        int que = TestSeriesQuestionsMapping(request.TestSeriesQuestions ??= ([]), newId, quesSec);

                        if (sub > 0 && cla > 0 && board > 0 && course > 0 && subIn > 0 && quesSec > 0 && queType > 0 && inst > 0 && que > 0)
                        {
                            return new ServiceResponse<string>(true, "operation successful", "Test series added successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Some error occured", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Some error occured", string.Empty, 500);
                    }
                }
                else
                {
                    string updateQuery = @"
                    UPDATE tblTestSeries
                    SET
                        TestPatternName = @TestPatternName,
                        Duration = @Duration,
                        Status = @Status,
                        APID = @APID,
                        TotalNoOfQuestions = @TotalNoOfQuestions,
                        MethodofAddingType = @MethodofAddingType,
                        StartDate = @StartDate,
                        StartTime = @StartTime,
                        ResultDate = @ResultDate,
                        ResultTime = @ResultTime,
                        EmployeeID = @EmployeeID,
                        NameOfExam = @NameOfExam,
                        RepeatedExams = @RepeatedExams,
                        TypeOfTestSeries = @TypeOfTestSeries,
                        modifiedon = @modifiedon,
                        modifiedby = @modifiedby
                    WHERE TestSeriesId = @TestSeriesId;";
                    var parameters = new
                    {
                        request.TestPatternName,
                        request.Duration,
                        request.Status,
                        request.APID,
                        request.TotalNoOfQuestions,
                        request.MethodofAddingType,
                        request.StartDate,
                        request.StartTime,
                        request.ResultDate,
                        request.ResultTime,
                        request.EmployeeID,
                        request.NameOfExam,
                        request.RepeatedExams,
                        request.TypeOfTestSeries,
                        modifiedon = DateTime.Now,
                        request.modifiedby,
                        request.TestSeriesId
                    };
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, parameters);
                    if (rowsAffected > 0)
                    {
                        int sub = TestSeriesSubjectMapping(request.TestSeriesSubject ??= ([]), request.TestSeriesId);
                        int cla = TestSeriesClassMapping(request.TestSeriesClasses ??= ([]), request.TestSeriesId);
                        int board = TestSeriesBoardMapping(request.TestSeriesBoard ??= ([]), request.TestSeriesId);
                        int course = TestSeriesCourseMapping(request.TestSeriesCourses ??= ([]), request.TestSeriesId);
                        int subIn = TestSeriesContentIndexMapping(request.TestSeriesContentIndexes ??= ([]), request.TestSeriesId);
                        int quesSec = TestSeriesQuestionSectionMapping(request.TestSeriesQuestionsSection ??= new TestSeriesQuestionSection(), request.TestSeriesId);
                        int queType = TestSeriesQuestionTypeMapping(request.TestSeriesQuestionTypes ??= ([]), request.TestSeriesId);
                        int inst = TestSeriesInstructionsMapping(request.TestSeriesInstruction ??= ([]), request.TestSeriesId);
                        int que = TestSeriesQuestionsMapping(request.TestSeriesQuestions ??= ([]), request.TestSeriesId, quesSec);

                        if (sub > 0 && cla > 0 && board > 0 && course > 0 && subIn > 0 && quesSec > 0 && queType > 0 && inst > 0 && que > 0)
                        {
                            return new ServiceResponse<string>(true, "operation successful", "Test series updated successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Some error occured", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Some error occured", string.Empty, 500);
                    }

                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<TestSeriesResponseDTO>> GetTestSeriesById(int TestSeriesId)
        {
            try
            {
                var query = @"
            SELECT 
                ts.TestSeriesId,
                ts.TestPatternName,
                ts.Duration,
                ts.Status,
                ts.APID,
                ap.APName AS APName,
                ts.TotalNoOfQuestions,
                ts.MethodofAddingType,
                ts.StartDate,
                ts.StartTime,
                ts.ResultDate,
                ts.ResultTime,
                ts.EmployeeID,
                emp.EmpFirstName AS EmpFirstName,
                ts.NameOfExam,
                ts.RepeatedExams,
                ts.TypeOfTestSeries,
                tts.TestSeriesName AS TypeOfTestSeriesName,
                ts.createdon,
                ts.createdby,
                ts.modifiedon,
                ts.modifiedby
            FROM tblTestSeries ts
            JOIN tblCategory ap ON ts.APID = ap.APID
            JOIN tblEmployee emp ON ts.EmployeeID = emp.EmployeeID
            JOIN tblTypeOfTestSeries tts ON ts.TypeOfTestSeries = tts.TTSId
            WHERE ts.TestSeriesId = @TestSeriesId";

                var testSeries = await _connection.QueryFirstOrDefaultAsync<TestSeriesResponseDTO>(query, new { TestSeriesId });

                if (testSeries == null)
                {
                    return new ServiceResponse<TestSeriesResponseDTO>(false, "Test Series not found", new TestSeriesResponseDTO(), 404);
                }

                // Fetch related data using the existing methods
                testSeries.TestSeriesSubject = GetListOfTestSeriesSubjects(TestSeriesId);
                testSeries.TestSeriesClasses = GetListOfTestSeriesClasses(TestSeriesId);
                testSeries.TestSeriesBoard = GetListOfTestSeriesBoards(TestSeriesId);
                testSeries.TestSeriesCourses = GetListOfTestSeriesCourse(TestSeriesId);
                testSeries.TestSeriesContentIndexes = GetListOfTestSeriesSubjectIndex(TestSeriesId);
                testSeries.TestSeriesQuestionsSection = GetTestSeriesQuestionSection(TestSeriesId);
                testSeries.TestSeriesQuestionTypes = GetListOfTestSeriesQuestionType(TestSeriesId);
                testSeries.TestSeriesInstruction = GetListOfTestSeriesInstructions(TestSeriesId);

                // Fetch TestSeriesQuestions based on sectionId if TestSeriesQuestionsSection is not null
                if (testSeries.TestSeriesQuestionsSection != null)
                {
                    testSeries.TestSeriesQuestions = GetListOfTestSeriesQuestion(testSeries.TestSeriesQuestionsSection.testseriesQuestionSectionid);
                }

                return new ServiceResponse<TestSeriesResponseDTO>(true, "Success", testSeries, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<TestSeriesResponseDTO>(false, ex.Message, new TestSeriesResponseDTO(), 500);
            }
        }
        private int TestSeriesSubjectMapping(List<TestSeriesSubjects> request, int TestSeriesId)
        {
            foreach (var data in request)
            {
                data.TestSeriesID = TestSeriesId;
            }
            string query = "SELECT COUNT(*) FROM tblTestSeriesSubjects WHERE [TestSeriesID] = @TestSeriesID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { TestSeriesID = TestSeriesId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblTestSeriesSubjects] WHERE [TestSeriesID] = @TestSeriesID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { TestSeriesID = TestSeriesId });
                if (rowsAffected > 0)
                {
                    string insertQuery = @"
                    INSERT INTO tblTestSeriesSubjects (SubjectID, TestSeriesID, NoOfQuestions)
                    VALUES (@SubjectID, @TestSeriesID, @NoOfQuestions);";

                    var valuesInserted = _connection.Execute(insertQuery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                string insertQuery = @"
                    INSERT INTO tblTestSeriesSubjects (SubjectID, TestSeriesID, NoOfQuestions)
                    VALUES (@SubjectID, @TestSeriesID, @NoOfQuestions);";

                var valuesInserted = _connection.Execute(insertQuery, request);
                return valuesInserted;
            }
        }
        private int TestSeriesClassMapping(List<TestSeriesClass> request, int TestSeriesId)
        {
            foreach (var data in request)
            {
                data.TestSeriesId = TestSeriesId;
            }
            string query = "SELECT COUNT(*) FROM [tblTestSeriesClass] WHERE [TestSeriesId] = @TestSeriesId";
            int count = _connection.QueryFirstOrDefault<int>(query, new { TestSeriesId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblTestSeriesClass] WHERE [TestSeriesId] = @TestSeriesId;";
                var rowsAffected = _connection.Execute(deleteDuery, new { TestSeriesId });
                if (rowsAffected > 0)
                {
                    string insertQuery = @"INSERT INTO tblTestSeriesClass (TestSeriesId, ClassId)
                    VALUES (@TestSeriesId, @ClassId);";
                    var valuesInserted = _connection.Execute(insertQuery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                string insertQuery = @"INSERT INTO tblTestSeriesClass (TestSeriesId, ClassId)
                VALUES (@TestSeriesId, @ClassId);";
                var valuesInserted = _connection.Execute(insertQuery, request);
                return valuesInserted;
            }
        }
        private int TestSeriesBoardMapping(List<TestSeriesBoards> request, int TestSeriesId)
        {
            foreach (var data in request)
            {
                data.TestSeriesId = TestSeriesId;
            }
            string query = "SELECT COUNT(*) FROM [tblTestSeriesBoards] WHERE [TestSeriesId] = @TestSeriesId";
            int count = _connection.QueryFirstOrDefault<int>(query, new { TestSeriesId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblTestSeriesBoards] WHERE [TestSeriesId] = @TestSeriesId;";
                var rowsAffected = _connection.Execute(deleteDuery, new { TestSeriesId });
                if (rowsAffected > 0)
                {
                    string insertQuery = @"
                    INSERT INTO tblTestSeriesBoards (TestSeriesId, BoardId)
                    VALUES (@TestSeriesId, @BoardId);";
                    var valuesInserted = _connection.Execute(insertQuery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                string insertQuery = @"
                    INSERT INTO tblTestSeriesBoards (TestSeriesId, BoardId)
                    VALUES (@TestSeriesId, @BoardId);";
                var valuesInserted = _connection.Execute(insertQuery, request);
                return valuesInserted;
            }
        }
        private int TestSeriesCourseMapping(List<TestSeriesCourse> request, int TestSeriesId)
        {
            foreach (var data in request)
            {
                data.TestSeriesId = TestSeriesId;
            }
            string query = "SELECT COUNT(*) FROM [tblTestSeriesCourse] WHERE [TestSeriesId] = @TestSeriesId";
            int count = _connection.QueryFirstOrDefault<int>(query, new { TestSeriesId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblTestSeriesCourse] WHERE [TestSeriesId] = @TestSeriesId;";
                var rowsAffected = _connection.Execute(deleteDuery, new { TestSeriesId });
                if (rowsAffected > 0)
                {
                    string insertQuery = @"
                    INSERT INTO tblTestSeriesCourse (TestSeriesId, CourseId)
                    VALUES (@TestSeriesId, @CourseId);";
                    var valuesInserted = _connection.Execute(insertQuery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                string insertQuery = @"
                    INSERT INTO tblTestSeriesCourse (TestSeriesId, CourseId)
                    VALUES (@TestSeriesId, @CourseId);";
                var valuesInserted = _connection.Execute(insertQuery, request);
                return valuesInserted;
            }
        }
        private int TestSeriesContentIndexMapping(List<TestSeriesContentIndex> request, int TestSeriesId)
        {
            foreach (var data in request)
            {
                data.TestSeriesID = TestSeriesId;
            }
            string query = "SELECT COUNT(*) FROM [tblTestSeriesContentIndex] WHERE [TestSeriesID] = @TestSeriesId";
            int count = _connection.QueryFirstOrDefault<int>(query, new { TestSeriesId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblTestSeriesContentIndex] WHERE [TestSeriesID] = @TestSeriesId;";
                var rowsAffected = _connection.Execute(deleteDuery, new { TestSeriesId });
                if (rowsAffected > 0)
                {
                    string insertQuery = @"
                    INSERT INTO tblTestSeriesContentIndex (IndexTypeId, ContentIndexId, TestSeriesID)
                    VALUES (@IndexTypeId, @ContentIndexId, @TestSeriesID);";

                    var valuesInserted = _connection.Execute(insertQuery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                string insertQuery = @"
                    INSERT INTO tblTestSeriesContentIndex (IndexTypeId, ContentIndexId, TestSeriesID)
                    VALUES (@IndexTypeId, @ContentIndexId, @TestSeriesID);";

                var valuesInserted = _connection.Execute(insertQuery, request);
                return valuesInserted;
            }
        }
        private int TestSeriesQuestionSectionMapping(TestSeriesQuestionSection request, int TestSeriesId)
        {
            if (request.testseriesQuestionSectionid == 0)
            {
                request.TestSeriesid = TestSeriesId;
                string insertQuery = @"
                INSERT INTO tbltestseriesQuestionSection (TestSeriesid, DisplayOrder, SectionName, Status)
                VALUES (@TestSeriesid, @DisplayOrder, @SectionName, @Status);
                SELECT CAST(SCOPE_IDENTITY() as int)";
                int newId = _connection.QuerySingle<int>(insertQuery, request);
                return newId;
            }
            else
            {
                string updateQuery = @"
                UPDATE tbltestseriesQuestionSection
                SET TestSeriesid = @TestSeriesid,
                DisplayOrder = @DisplayOrder,
                SectionName = @SectionName,
                Status = @Status
                WHERE testseriesQuestionSectionId = @testseriesQuestionSectionId;";

                int rowsAffected = _connection.QuerySingle<int>(updateQuery, request);
                return request.testseriesQuestionSectionid;
            }
        }
        private int TestSeriesQuestionTypeMapping(List<TestSeriesQuestionType> request, int TestSeriesId)
        {
            foreach (var data in request)
            {
                data.TestSeriesID = TestSeriesId;
            }

            string query = "SELECT COUNT(*) FROM [tblTestSeriesQuestionType] WHERE [TestSeriesID] = @TestSeriesId";
            int count = _connection.QueryFirstOrDefault<int>(query, new { TestSeriesId });

            if (count > 0)
            {
                var deleteQuery = @"DELETE FROM [tblTestSeriesQuestionType] WHERE [TestSeriesID] = @TestSeriesId;";
                var rowsAffected = _connection.Execute(deleteQuery, new { TestSeriesId });

                if (rowsAffected > 0)
                {
                    int valuesInserted = 0;

                    foreach (var questionType in request)
                    {
                        string insertQuery = @"
                INSERT INTO tblTestSeriesQuestionType (
                    QuestionTypeID,
                    TestSeriesID,
                    EnterMarksPerCorrectAnswer,
                    EnterNegativeMarks,
                    PerNoOfQuestions,
                    NoOfQuestionsForChoice
                )
                VALUES (
                    @QuestionTypeID,
                    @TestSeriesID,
                    @EnterMarksPerCorrectAnswer,
                    @EnterNegativeMarks,
                    @PerNoOfQuestions,
                    @NoOfQuestionsForChoice
                );
                SELECT CAST(SCOPE_IDENTITY() as int);";

                        int insertedId = _connection.QuerySingle<int>(insertQuery, questionType);
                        valuesInserted++;

                        if (questionType.TestSeriesQuestionDifficultyLevel != null && questionType.TestSeriesQuestionDifficultyLevel.Count != 0)
                        {
                            TestSeriesQuestionDifficultyMapping(questionType.TestSeriesQuestionDifficultyLevel, TestSeriesId, insertedId);
                        }
                    }

                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                int valuesInserted = 0;

                foreach (var questionType in request)
                {
                    string insertQuery = @"
            INSERT INTO tblTestSeriesQuestionType (
                QuestionTypeID,
                TestSeriesID,
                EnterMarksPerCorrectAnswer,
                EnterNegativeMarks,
                PerNoOfQuestions,
                NoOfQuestionsForChoice
            )
            VALUES (
                @QuestionTypeID,
                @TestSeriesID,
                @EnterMarksPerCorrectAnswer,
                @EnterNegativeMarks,
                @PerNoOfQuestions,
                @NoOfQuestionsForChoice
            );
            SELECT CAST(SCOPE_IDENTITY() as int);";

                    int insertedId = _connection.QuerySingle<int>(insertQuery, questionType);
                    valuesInserted++;

                    if (questionType.TestSeriesQuestionDifficultyLevel != null && questionType.TestSeriesQuestionDifficultyLevel.Count != 0)
                    {
                        TestSeriesQuestionDifficultyMapping(questionType.TestSeriesQuestionDifficultyLevel, TestSeriesId, insertedId);
                    }
                }

                return valuesInserted;
            }
        }

        private int TestSeriesQuestionDifficultyMapping(List<TestSeriesQuestionDifficulty> request, int TestSeriesId, int TestSeriesQuestionTypeId)
        {
            foreach (var data in request)
            {
                data.TestSeriesID = TestSeriesId;
                data.TestSeriesQuestionTypeId = TestSeriesQuestionTypeId;
            }

            string query = "SELECT COUNT(*) FROM [tblTestSeriesQuestionDifficulty] WHERE [TestSeriesQuestionTypeId] = @TestSeriesQuestionTypeId";
            int count = _connection.QueryFirstOrDefault<int>(query, new { TestSeriesQuestionTypeId });

            if (count > 0)
            {
                var deleteQuery = @"DELETE FROM [tblTestSeriesQuestionDifficulty] WHERE [TestSeriesQuestionTypeId] = @TestSeriesQuestionTypeId;";
                var rowsAffected = _connection.Execute(deleteQuery, new { TestSeriesQuestionTypeId });

                if (rowsAffected > 0)
                {
                    string insertQuery = @"
            INSERT INTO tblTestSeriesQuestionDifficulty (
                LevelID,
                TestSeriesID,
                PercentagePerDifficulty,
                TestSeriesQuestionTypeId
            )
            VALUES (
                @LevelID,
                @TestSeriesID,
                @PercentagePerDifficulty,
                @TestSeriesQuestionTypeId
            );";

                    var valuesInserted = _connection.Execute(insertQuery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                string insertQuery = @"
        INSERT INTO tblTestSeriesQuestionDifficulty (
            LevelID,
            TestSeriesID,
            PercentagePerDifficulty,
            TestSeriesQuestionTypeId
        )
        VALUES (
            @LevelID,
            @TestSeriesID,
            @PercentagePerDifficulty,
            @TestSeriesQuestionTypeId
        );";

                var valuesInserted = _connection.Execute(insertQuery, request);
                return valuesInserted;
            }
        }


        private int TestSeriesInstructionsMapping(List<TestSeriesInstructions> request, int TestSeriesId)
        {
            foreach (var data in request)
            {
                data.TestSeriesID = TestSeriesId;
            }
            string query = "SELECT COUNT(*) FROM [tblTestInstructions] WHERE [TestSeriesID] = @TestSeriesId";
            int count = _connection.QueryFirstOrDefault<int>(query, new { TestSeriesId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblTestInstructions] WHERE [TestSeriesID] = @TestSeriesId;";
                var rowsAffected = _connection.Execute(deleteDuery, new { TestSeriesId });
                if (rowsAffected > 0)
                {
                    string insertQuery = @"
                    INSERT INTO tblTestInstructions (Instructions,TestSeriesID)
                    VALUES (@Instructions,@TestSeriesID);";
                    var valuesInserted = _connection.Execute(insertQuery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                string insertQuery = @"
                    INSERT INTO tblTestInstructions (Instructions,TestSeriesID)
                    VALUES (@Instructions,@TestSeriesID);";
                var valuesInserted = _connection.Execute(insertQuery, request);
                return valuesInserted;
            }
        }
        private int TestSeriesQuestionsMapping(List<TestSeriesQuestions> request, int TestSeriesId, int sectionId)
        {
            foreach (var data in request)
            {
                data.TestSeriesid = TestSeriesId;
                data.testseriesQuestionSectionid = sectionId;
            }
            string query = "SELECT COUNT(*) FROM [tbltestseriesQuestions] WHERE [testseriesQuestionSectionid] = @testseriesQuestionSectionid";
            int count = _connection.QueryFirstOrDefault<int>(query, new { testseriesQuestionSectionid = sectionId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tbltestseriesQuestions] WHERE [testseriesQuestionSectionid] = @testseriesQuestionSectionid;";
                var rowsAffected = _connection.Execute(deleteDuery, new { testseriesQuestionSectionid = sectionId });
                if (rowsAffected > 0)
                {
                    string insertQuery = @"
                    INSERT INTO tbltestseriesQuestions (TestSeriesid, Questionid, DisplayOrder, Status, testseriesQuestionSectionid) 
                    VALUES (@TestSeriesid, @Questionid, @DisplayOrder, @Status, @testseriesQuestionSectionid);";
                    var valuesInserted = _connection.Execute(insertQuery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                string insertQuery = @"
                    INSERT INTO tbltestseriesQuestions (TestSeriesid, Questionid, DisplayOrder, Status, testseriesQuestionSectionid) 
                    VALUES (@TestSeriesid, @Questionid, @DisplayOrder, @Status, @testseriesQuestionSectionid);";
                var valuesInserted = _connection.Execute(insertQuery, request);
                return valuesInserted;
            }
        }
        //get records
        private List<TestSeriesSubjectsResponse> GetListOfTestSeriesSubjects(int TestSeriesId)
        {
            string query = @"
        SELECT 
            tss.TestSeriesSubjectId,
            tss.SubjectID,
            tss.TestSeriesID,
            s.SubjectName AS SubjectName,
            tss.NoOfQuestions
        FROM tblTestSeriesSubjects tss
        JOIN tblSubject s ON tss.SubjectID = s.SubjectID
        WHERE tss.TestSeriesID = @TestSeriesID";

            var data = _connection.Query<TestSeriesSubjectsResponse>(query, new { TestSeriesID = TestSeriesId });
            return data != null ? data.AsList() : new List<TestSeriesSubjectsResponse>();
        }
        private List<TestSeriesClassResponse> GetListOfTestSeriesClasses(int TestSeriesId)
        {
            string query = @"
        SELECT 
            tsc.TestSeriesClassesId,
            tsc.TestSeriesId,
            tsc.ClassId,
            c.ClassName AS Name
        FROM tblTestSeriesClass tsc
        JOIN tblClass c ON tsc.ClassId = c.ClassId
        WHERE tsc.TestSeriesID = @TestSeriesID";

            var data = _connection.Query<TestSeriesClassResponse>(query, new { TestSeriesID = TestSeriesId });
            return data != null ? data.AsList() : new List<TestSeriesClassResponse>();
        }
        private List<TestSeriesBoardsResponse> GetListOfTestSeriesBoards(int TestSeriesId)
        {
            string query = @"
        SELECT 
            tsb.TestSeriesBoardsId,
            tsb.TestSeriesId,
            tsb.BoardId,
            b.BoardName AS Name
        FROM tblTestSeriesBoards tsb
        JOIN tblBoard b ON tsb.BoardId = b.BoardId
        WHERE tsb.TestSeriesID = @TestSeriesID";

            var data = _connection.Query<TestSeriesBoardsResponse>(query, new { TestSeriesID = TestSeriesId });
            return data != null ? data.AsList() : new List<TestSeriesBoardsResponse>();
        }
        private List<TestSeriesCourseResponse> GetListOfTestSeriesCourse(int TestSeriesId)
        {
            string query = @"
        SELECT 
            tsc.TestSeriesCourseId,
            tsc.TestSeriesId,
            tsc.CourseId,
            c.CourseName AS Name
        FROM tblTestSeriesCourse tsc
        JOIN tblCourse c ON tsc.CourseId = c.CourseId
        WHERE tsc.TestSeriesID = @TestSeriesID";

            var data = _connection.Query<TestSeriesCourseResponse>(query, new { TestSeriesID = TestSeriesId });
            return data != null ? data.AsList() : new List<TestSeriesCourseResponse>();
        }
        private List<TestSeriesContentIndexResponse> GetListOfTestSeriesSubjectIndex(int TestSeriesId)
        {
            string query = @"
            SELECT 
                tsci.TestSeriesContentIndexId,
                tsci.IndexTypeId,
                it.IndexType AS IndexTypeName,
                tsci.ContentIndexId,
                CASE 
                    WHEN tsci.IndexTypeId = 1 THEN ci.ContentName_Chapter
                    WHEN tsci.IndexTypeId = 2 THEN ct.ContentName_Topic
                    WHEN tsci.IndexTypeId = 3 THEN cst.ContentName_SubTopic
                END AS ContentIndexName,
                tsci.TestSeriesID
            FROM tblTestSeriesContentIndex tsci
            LEFT JOIN tblQBIndexType it ON tsci.IndexTypeId = it.IndexId
            LEFT JOIN tblContentIndexChapters ci ON tsci.ContentIndexId = ci.ContentIndexId AND tsci.IndexTypeId = 1
            LEFT JOIN tblContentIndexTopics ct ON tsci.ContentIndexId = ct.ContInIdTopic AND tsci.IndexTypeId = 2
            LEFT JOIN tblContentIndexSubTopics cst ON tsci.ContentIndexId = cst.ContInIdSubTopic AND tsci.IndexTypeId = 3
            WHERE tsci.TestSeriesID = @TestSeriesID";

            var data = _connection.Query<TestSeriesContentIndexResponse>(query, new { TestSeriesID = TestSeriesId });
            return data != null ? data.AsList() : new List<TestSeriesContentIndexResponse>();
        }

        private TestSeriesQuestionSection GetTestSeriesQuestionSection(int TestSeriesId)
        {
            string query = "SELECT * FROM tbltestseriesQuestionSection WHERE [TestSeriesid] = @TestSeriesID";

            var data = _connection.QueryFirstOrDefault<TestSeriesQuestionSection>(query, new { TestSeriesID = TestSeriesId });
            return data ?? new TestSeriesQuestionSection();
        }
        private List<TestSeriesQuestionType> GetListOfTestSeriesQuestionType(int TestSeriesId)
        {
            string query = @"
        SELECT 
            tstqt.TestSeriesQuestionTypeId,
            tstqt.QuestionTypeID,
            tstqt.TestSeriesID,
            tstqt.EnterMarksPerCorrectAnswer,
            tstqt.EnterNegativeMarks,
            tstqt.PerNoOfQuestions,
            tstqt.NoOfQuestionsForChoice
        FROM tblTestSeriesQuestionType tstqt
        WHERE tstqt.TestSeriesID = @TestSeriesID";

            var questionTypes = _connection.Query<TestSeriesQuestionType>(query, new { TestSeriesID = TestSeriesId }).ToList();

            foreach (var questionType in questionTypes)
            {
                questionType.TestSeriesQuestionDifficultyLevel = GetListOfTestSeriesQuestionDifficulty(questionType.TestSeriesQuestionTypeId);
            }

            return questionTypes;
        }
        private List<TestSeriesQuestionDifficulty> GetListOfTestSeriesQuestionDifficulty(int TestSeriesQuestionTypeId)
        {
            string query = @"
        SELECT 
            tstqd.TestSeriesQuestionDifficultyId,
            tstqd.LevelID,
            tstqd.TestSeriesID,
            tstqd.TestSeriesQuestionTypeId,
            tstqd.PercentagePerDifficulty
        FROM tblTestSeriesQuestionDifficulty tstqd
        WHERE tstqd.TestSeriesQuestionTypeId = @TestSeriesQuestionTypeId";

            var difficulties = _connection.Query<TestSeriesQuestionDifficulty>(query, new { TestSeriesQuestionTypeId }).ToList();

            return difficulties;
        }
        private List<TestSeriesInstructions> GetListOfTestSeriesInstructions(int TestSeriesId)
        {
            string query = "SELECT * FROM tblTestInstructions WHERE [TestSeriesID] = @TestSeriesID";

            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<TestSeriesInstructions>(query, new { TestSeriesID = TestSeriesId });
            return data != null ? data.AsList() : [];
        }
        private List<TestSeriesQuestions> GetListOfTestSeriesQuestion(int sectionId)
        {
            string query = "SELECT * FROM tbltestseriesQuestions WHERE [testseriesQuestionSectionid] = @testseriesQuestionSectionid";

            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<TestSeriesQuestions>(query, new { testseriesQuestionSectionid = sectionId });
            return data != null ? data.AsList() : [];
        }
    }
}
