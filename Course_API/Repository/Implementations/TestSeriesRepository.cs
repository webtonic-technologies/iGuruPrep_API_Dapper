using Course_API.DTOs;
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
                boardid, classId, CourseId, ExamTypeID, createdon, createdby, 
                TestPatternName, BoardName, ClassName, 
                CourseName, ExamTypeName, FirstName, Duration, Status, APID, 
                APName, TotalNoOfQuestions, MethodofAddingType, StartDate, 
                StartTime, ResultDate, ResultTime, EmployeeID, EmpFirstName, 
                NameOfExam, RepeatedExams
            ) 
            VALUES 
            (
                @boardid, @classId, @CourseId, @ExamTypeID, @createdon, @createdby, 
                @TestPatternName, @BoardName, @ClassName, 
                @CourseName, @ExamTypeName, @FirstName, @Duration, @Status, @APID, 
                @APName, @TotalNoOfQuestions, @MethodofAddingType, @StartDate, 
                @StartTime, @ResultDate, @ResultTime, @EmployeeID, @EmpFirstName, 
                @NameOfExam, @RepeatedExams
            ); 
            SELECT CAST(SCOPE_IDENTITY() as int)";
                    var parameters = new
                    {
                        request.boardid,
                        request.classId,
                        request.CourseId,
                        request.ExamTypeID,
                        createdon = DateTime.Now,
                        request.createdby,
                        request.TestPatternName,
                        request.BoardName,
                        request.ClassName,
                        request.CourseName,
                        request.ExamTypeName,
                        request.FirstName,
                        request.Duration,
                        Status = true,
                        request.APID,
                        request.APName,
                        request.TotalNoOfQuestions,
                        request.MethodofAddingType,
                        request.StartDate,
                        request.StartTime,
                        request.ResultDate,
                        request.ResultTime,
                        request.EmployeeID,
                        request.EmpFirstName,
                        request.NameOfExam,
                        request.RepeatedExams
                    };
                    int newId = await _connection.QuerySingleAsync<int>(insertQuery, parameters);
                    if (newId > 0)
                    {
                        int sub = TestSeriesSubjectMapping(request.TestSeriesSubject ??= ([]), newId);
                        int cla = TestSeriesClassMapping(request.TestSeriesClasses ??= ([]), newId);
                        int board = TestSeriesBoardMapping(request.TestSeriesBoard ??= ([]), newId);
                        int course = TestSeriesCourseMapping(request.TestSeriesCourses ??= ([]), newId);
                        int subIn = TestSeriesSubjectIndexMapping(request.TestSeriesSubjectIndexes ??= ([]), newId);
                        int quesSec = TestSeriesQuestionSectionMapping(request.TestSeriesQuestionsSection ??= new TestSeriesQuestionSection(), newId);
                        int queType = TestSeriesQuestionTypeMapping(request.TestSeriesQuestionTypes ??= ([]), newId);
                        int queDif = TestSeriesQuestionDifficultyMapping(request.TestSeriesQuestionDifficultyLevel ??= ([]), newId);
                        int inst = TestSeriesInstructionsMapping(request.TestSeriesInstruction ??= ([]), newId);
                        int que = TestSeriesQuestionsMapping(request.TestSeriesQuestions ??= ([]), newId, quesSec);

                        if (sub > 0 && cla > 0 && board > 0 && course > 0 && subIn > 0 && quesSec > 0 && queType > 0 && queDif > 0 && inst > 0 && que > 0)
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
                boardid = @boardid,
                classId = @classId,
                CourseId = @CourseId,
                ExamTypeID = @ExamTypeID,
                modifiedon = @modifiedon,
                modifiedby = @modifiedby,
                TestPatternName = @TestPatternName,
                BoardName = @BoardName,
                ClassName = @ClassName,
                CourseName = @CourseName,
                ExamTypeName = @ExamTypeName,
                FirstName = @FirstName,
                Duration = @Duration,
                Status = @Status,
                APID = @APID,
                APName = @APName,
                TotalNoOfQuestions = @TotalNoOfQuestions,
                MethodofAddingType = @MethodofAddingType,
                StartDate = @StartDate,
                StartTime = @StartTime,
                ResultDate = @ResultDate,
                ResultTime = @ResultTime,
                EmployeeID = @EmployeeID,
                EmpFirstName = @EmpFirstName,
                NameOfExam = @NameOfExam,
                RepeatedExams = @RepeatedExams
            WHERE 
                TestSeriesId = @TestSeriesId";
                    var parameters = new
                    {
                        request.TestSeriesId,
                        request.boardid,
                        request.classId,
                        request.CourseId,
                        request.ExamTypeID,
                        modifiedon = DateTime.Now,
                        request.modifiedby,
                        request.TestPatternName,
                        request.BoardName,
                        request.ClassName,
                        request.CourseName,
                        request.ExamTypeName,
                        request.FirstName,
                        request.Duration,
                        request.Status,
                        request.APID,
                        request.APName,
                        request.TotalNoOfQuestions,
                        request.MethodofAddingType,
                        request.StartDate,
                        request.StartTime,
                        request.ResultDate,
                        request.ResultTime,
                        request.EmployeeID,
                        request.EmpFirstName,
                        request.NameOfExam,
                        request.RepeatedExams
                    };
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, parameters);
                    if (rowsAffected > 0)
                    {
                        int sub = TestSeriesSubjectMapping(request.TestSeriesSubject ??= ([]), request.TestSeriesId);
                        int cla = TestSeriesClassMapping(request.TestSeriesClasses ??= ([]), request.TestSeriesId);
                        int board = TestSeriesBoardMapping(request.TestSeriesBoard ??= ([]), request.TestSeriesId);
                        int course = TestSeriesCourseMapping(request.TestSeriesCourses ??= ([]), request.TestSeriesId);
                        int subIn = TestSeriesSubjectIndexMapping(request.TestSeriesSubjectIndexes ??= ([]), request.TestSeriesId);
                        int quesSec = TestSeriesQuestionSectionMapping(request.TestSeriesQuestionsSection ??= new TestSeriesQuestionSection(), request.TestSeriesId);
                        int queType = TestSeriesQuestionTypeMapping(request.TestSeriesQuestionTypes ??= ([]), request.TestSeriesId);
                        int queDif = TestSeriesQuestionDifficultyMapping(request.TestSeriesQuestionDifficultyLevel ??= ([]), request.TestSeriesId);
                        int inst = TestSeriesInstructionsMapping(request.TestSeriesInstruction ??= ([]), request.TestSeriesId);
                        int que = TestSeriesQuestionsMapping(request.TestSeriesQuestions ??= ([]), request.TestSeriesId, quesSec);

                        if (sub > 0 && cla > 0 && board > 0 && course > 0 && subIn > 0 && quesSec > 0 && queType > 0 && queDif > 0 && inst > 0 && que > 0)
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
                    string insertQuery = @"INSERT INTO tblTestSeriesClass (TestSeriesId, ClassId, ClassName)
                    VALUES (@TestSeriesId, @ClassId, @ClassName);";
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
                string insertQuery = @"INSERT INTO tblTestSeriesClass (TestSeriesId, ClassId, ClassName)
                VALUES (@TestSeriesId, @ClassId, @ClassName);";
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
                    INSERT INTO tblTestSeriesBoards (TestSeriesId, BoardId, BoardName)
                    VALUES (@TestSeriesId, @BoardId, @BoardName);";
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
                    INSERT INTO tblTestSeriesBoards (TestSeriesId, BoardId, BoardName)
                    VALUES (@TestSeriesId, @BoardId, @BoardName);";
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
                    INSERT INTO tblTestSeriesCourse (TestSeriesId, CourseId, CourseName)
                    VALUES (@TestSeriesId, @CourseId, @CourseName);";
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
                    INSERT INTO tblTestSeriesCourse (TestSeriesId, CourseId, CourseName)
                    VALUES (@TestSeriesId, @CourseId, @CourseName);";
                var valuesInserted = _connection.Execute(insertQuery, request);
                return valuesInserted;
            }
        }
        private int TestSeriesSubjectIndexMapping(List<TestSeriesSubjectIndex> request, int TestSeriesId)
        {
            foreach (var data in request)
            {
                data.TestSeriesID = TestSeriesId;
            }
            string query = "SELECT COUNT(*) FROM [tblTestSeriesSubjectIndex] WHERE [TestSeriesID] = @TestSeriesId";
            int count = _connection.QueryFirstOrDefault<int>(query, new { TestSeriesId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblTestSeriesSubjectIndex] WHERE [TestSeriesID] = @TestSeriesId;";
                var rowsAffected = _connection.Execute(deleteDuery, new { TestSeriesId });
                if (rowsAffected > 0)
                {
                    string insertQuery = @"
                    INSERT INTO tblTestSeriesSubjectIndex (SubjectIndexID, TestSeriesID)
                    VALUES (@SubjectIndexID, @TestSeriesID);";

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
                    INSERT INTO tblTestSeriesSubjectIndex (SubjectIndexID, TestSeriesID)
                    VALUES (@SubjectIndexID, @TestSeriesID);";

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
                int newId = _connection.Execute(insertQuery, request);
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

                int rowsAffected = _connection.Execute(updateQuery, request);
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
                var deleteDuery = @"DELETE FROM [tblTestSeriesQuestionType] WHERE [TestSeriesID] = @TestSeriesId;";
                var rowsAffected = _connection.Execute(deleteDuery, new { TestSeriesId });
                if (rowsAffected > 0)
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
                INSERT INTO tblTestSeriesQuestionType (
                QuestionTypeID,
                TestSeriesID,
                EnterMarksPerCorrectAnswer,
                EnterNegativeMarks,
                PerNoOfQuestions,
                NoOfQuestionsForChoice )
                VALUES (
                @QuestionTypeID,
                @TestSeriesID,
                @EnterMarksPerCorrectAnswer,
                @EnterNegativeMarks,
                @PerNoOfQuestions,
                @NoOfQuestionsForChoice );";
                var valuesInserted = _connection.Execute(insertQuery, request);
                return valuesInserted;
            }
        }
        private int TestSeriesQuestionDifficultyMapping(List<TestSeriesQuestionDifficulty> request, int TestSeriesId)
        {
            foreach (var data in request)
            {
                data.TestSeriesID = TestSeriesId;
            }
            string query = "SELECT COUNT(*) FROM [tblTestSeriesQuestionDifficulty] WHERE [TestSeriesID] = @TestSeriesId";
            int count = _connection.QueryFirstOrDefault<int>(query, new { TestSeriesId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblTestSeriesQuestionDifficulty] WHERE [TestSeriesID] = @TestSeriesId;";
                var rowsAffected = _connection.Execute(deleteDuery, new { TestSeriesId });
                if (rowsAffected > 0)
                {
                    string insertQuery = @"
                    INSERT INTO tblTestSeriesQuestionDifficulty (LevelID,TestSeriesID,PercentagePerDifficulty)
                    VALUES (@LevelID,@TestSeriesID,@PercentagePerDifficulty);";
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
                    INSERT INTO tblTestSeriesQuestionDifficulty (LevelID,TestSeriesID,PercentagePerDifficulty)
                    VALUES (@LevelID,@TestSeriesID,@PercentagePerDifficulty);";
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
        private List<TestSeriesSubjects> GetListOfTestSeriesSubjects(int TestSeriesId)
        {
            string query = "SELECT * FROM tblTestSeriesSubjects WHERE [TestSeriesID] = @TestSeriesID";

            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<TestSeriesSubjects>(query, new { TestSeriesID = TestSeriesId });
            return data != null ? data.AsList() : [];
        }
        private List<TestSeriesClass> GetListOfTestSeriesClasses(int TestSeriesId)
        {
            string query = "SELECT * FROM tblTestSeriesClass WHERE [TestSeriesId] = @TestSeriesID";

            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<TestSeriesClass>(query, new { TestSeriesID = TestSeriesId });
            return data != null ? data.AsList() : [];
        }
        private List<TestSeriesBoards> GetListOfTestSeriesBoards(int TestSeriesId)
        {
            string query = "SELECT * FROM tblTestSeriesBoards WHERE [TestSeriesId] = @TestSeriesID";

            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<TestSeriesBoards>(query, new { TestSeriesID = TestSeriesId });
            return data != null ? data.AsList() : [];
        }
        private List<TestSeriesCourse> GetListOfTestSeriesCourse(int TestSeriesId)
        {
            string query = "SELECT * FROM tblTestSeriesCourse WHERE [TestSeriesId] = @TestSeriesID";

            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<TestSeriesCourse>(query, new { TestSeriesID = TestSeriesId });
            return data != null ? data.AsList() : [];
        }
        private List<TestSeriesSubjectIndex> GetListOfTestSeriesSubjectIndex(int TestSeriesId)
        {
            string query = "SELECT * FROM tblTestSeriesSubjectIndex WHERE [TestSeriesID] = @TestSeriesID";

            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<TestSeriesSubjectIndex>(query, new { TestSeriesID = TestSeriesId });
            return data != null ? data.AsList() : [];
        }
        private TestSeriesQuestionSection GetTestSeriesQuestionSection(int TestSeriesId)
        {
            string query = "SELECT * FROM tbltestseriesQuestionSection WHERE [TestSeriesid] = @TestSeriesID";

            var data = _connection.QueryFirstOrDefault<TestSeriesQuestionSection>(query, new { TestSeriesID = TestSeriesId });
            return data ?? new TestSeriesQuestionSection();
        }
        private List<TestSeriesQuestionType> GetListOfTestSeriesQuestionType(int TestSeriesId)
        {
            string query = "SELECT * FROM tblTestSeriesQuestionType WHERE [TestSeriesID] = @TestSeriesID";

            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<TestSeriesQuestionType>(query, new { TestSeriesID = TestSeriesId });
            return data != null ? data.AsList() : [];
        }
        private List<TestSeriesQuestionDifficulty> GetListOfTestSeriesQuestionDifficulty(int TestSeriesId)
        {
            string query = "SELECT * FROM tblTestSeriesQuestionDifficulty WHERE [TestSeriesID] = @TestSeriesID";

            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<TestSeriesQuestionDifficulty>(query, new { TestSeriesID = TestSeriesId });
            return data != null ? data.AsList() : [];
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
