using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;
using Course_API.Repository.Interfaces;
using Dapper;
using OfficeOpenXml;
using System.Data;
using OfficeOpenXml.Style;
using System.Data;
using System.Drawing;

namespace Course_API.Repository.Implementations
{
    public class TestSeriesRepository : ITestSeriesRepository
    {
        private readonly IDbConnection _connection;

        public TestSeriesRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<int>> AddUpdateTestSeries(TestSeriesDTO request)
        {
            try
            {
                if (request.TestSeriesId == 0)
                {
                    string insertQuery = @"
                    INSERT INTO tblTestSeries 
                    (
                        TestPatternName, Duration, Status, APID, TotalNoOfQuestions, ExamTypeID,
                        MethodofAddingType, StartDate, StartTime, ResultDate, ResultTime, 
                        EmployeeID, NameOfExam, RepeatedExams, TypeOfTestSeries, 
                        createdon, createdby, RepeatExamStartDate , RepeatExamEndDate ,
                        RepeatExamStarttime , RepeatExamResulttimeId, IsAdmin, TestSeriesStatusId
                    ) 
                    VALUES 
                    (
                        @TestPatternName, @Duration, @Status, @APID, @TotalNoOfQuestions, @ExamTypeID,
                        @MethodofAddingType, @StartDate, @StartTime, @ResultDate, @ResultTime, 
                        @EmployeeID, @NameOfExam, @RepeatedExams, @TypeOfTestSeries, 
                        @createdon, @createdby, @RepeatExamStartDate , @RepeatExamEndDate ,
                        @RepeatExamStarttime , @RepeatExamResulttimeId, @IsAdmin, @TestSeriesStatusId
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
                        request.createdby,
                        request.ExamTypeID,
                        request.RepeatExamEndDate,
                        request.RepeatExamStartDate,
                        request.RepeatExamStarttime,
                        request.RepeatExamResulttimeId,
                        request.IsAdmin,
                        TestSeriesStatusId = 11
                    };
                    int newId = await _connection.QuerySingleAsync<int>(insertQuery, parameters);
                    if (newId > 0)
                    {
                        int sub = TestSeriesSubjectMapping(request.TestSeriesSubject ??= ([]), newId);
                        int cla = TestSeriesClassMapping(request.TestSeriesClasses ??= ([]), newId);
                        int board = TestSeriesBoardMapping(request.TestSeriesBoard ??= ([]), newId);
                        int course = TestSeriesCourseMapping(request.TestSeriesCourses ??= ([]), newId);
                        //int subIn = TestSeriesContentIndexMapping(request.TestSeriesContentIndexes ??= ([]), newId);
                        //int quesSec = TestSeriesQuestionSectionMapping(request.TestSeriesQuestionsSection ??= new TestSeriesQuestionSection(), newId);
                        //int queType = TestSeriesQuestionTypeMapping(request.TestSeriesQuestionTypes ??= ([]), newId);
                        //int inst = TestSeriesInstructionsMapping(request.TestSeriesInstruction ??= ([]), newId);
                        // int que = TestSeriesQuestionsMapping(request.TestSeriesQuestions ??= ([]), newId, quesSec);

                        if (sub > 0 && cla > 0 && board > 0 && course > 0)
                        {
                            string updateQuery = @"
                            UPDATE tblTestSeries
                            SET TestSeriesStatusId = @TestSeriesStatusId
                            WHERE TestSeriesId = @TestSeriesId;";
                            var data = 0;
                            if (request.IsMandatory == false)
                            {
                                data = 13;
                            }
                            else
                            {
                                data = 10;
                            }
                            int rowsAffected = await _connection.ExecuteAsync(updateQuery, new { TestSeriesStatusId = data, TestSeriesId = newId });
                            return new ServiceResponse<int>(true, "operation successful", newId, 200);
                        }
                        else
                        {
                            return new ServiceResponse<int>(false, "Some error occured", 0, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<int>(false, "Some error occured", 0, 500);
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
                        modifiedby = @modifiedby,
                        ExamTypeID = @ExamTypeID,
                        RepeatExamEndDate = @RepeatExamEndDate,
                        RepeatExamStartDate = @RepeatExamStartDate,
                        RepeatExamStarttime = @RepeatExamStarttime,
                        RepeatExamResulttimeId = @RepeatExamResulttimeId,
                        IsAdmin = @IsAdmin,
                        TestSeriesStatusId = @TestSeriesStatusId
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
                        request.TestSeriesId,
                        request.ExamTypeID,
                        request.RepeatExamEndDate,
                        request.RepeatExamStartDate,
                        request.RepeatExamStarttime,
                        request.RepeatExamResulttimeId,
                        request.IsAdmin,
                        TestSeriesStatusId = 11
                    };
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, parameters);
                    if (rowsAffected > 0)
                    {
                        int sub = TestSeriesSubjectMapping(request.TestSeriesSubject ??= ([]), request.TestSeriesId);
                        int cla = TestSeriesClassMapping(request.TestSeriesClasses ??= ([]), request.TestSeriesId);
                        int board = TestSeriesBoardMapping(request.TestSeriesBoard ??= ([]), request.TestSeriesId);
                        int course = TestSeriesCourseMapping(request.TestSeriesCourses ??= ([]), request.TestSeriesId);
                        //int subIn = TestSeriesContentIndexMapping(request.TestSeriesContentIndexes ??= ([]), request.TestSeriesId);
                        //int quesSec = TestSeriesQuestionSectionMapping(request.TestSeriesQuestionsSection ??= new TestSeriesQuestionSection(), request.TestSeriesId);
                        //int queType = TestSeriesQuestionTypeMapping(request.TestSeriesQuestionTypes ??= ([]), request.TestSeriesId);
                        //int inst = TestSeriesInstructionsMapping(request.TestSeriesInstruction ??= ([]), request.TestSeriesId);
                        //int que = TestSeriesQuestionsMapping(request.TestSeriesQuestions ??= ([]), request.TestSeriesId, quesSec);

                        if (sub > 0 && cla > 0 && board > 0 && course > 0)
                        {
                            string updateQuery1 = @"
                            UPDATE tblTestSeries
                            SET TestSeriesStatusId = @TestSeriesStatusId
                            WHERE TestSeriesId = @TestSeriesId;";
                            var data = 0;
                            if (request.IsMandatory == false)
                            {
                                data = 13;
                            }
                            else
                            {
                                data = 10;
                            }
                            int rowsAffected1 = await _connection.ExecuteAsync(updateQuery1, new { TestSeriesStatusId = data, TestSeriesId = request.TestSeriesId });
                            return new ServiceResponse<int>(true, "operation successful", request.TestSeriesId, 200);
                        }
                        else
                        {
                            return new ServiceResponse<int>(false, "Some error occured", 0, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<int>(false, "Some error occured", 0, 500);
                    }

                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<int>(false, ex.Message, 0, 500);
            }
        }
        public async Task<ServiceResponse<TestSeriesResponseDTO>> GetTestSeriesById(int TestSeriesId)
        {
            try
            {
                // Fetch the main TestSeries data
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
                ts.ExamTypeID,
                ttt.ExamTypeName AS ExamTypeName,
                ts.createdon,
                ts.createdby,
                ts.modifiedon,
                ts.modifiedby,
                ts.RepeatExamStartDate,
                ts.RepeatExamEndDate,
                ts.RepeatExamStarttime,
                ts.RepeatExamResulttimeId,
                ts.IsAdmin,
                rt.ResultTime as RepeatedExamResultTime
            FROM tblTestSeries ts
            JOIN tblCategory ap ON ts.APID = ap.APID
            JOIN tblEmployee emp ON ts.EmployeeID = emp.EmployeeID
            JOIN tblTypeOfTestSeries tts ON ts.TypeOfTestSeries = tts.TTSId
            LEFT JOIN tblExamType ttt ON ts.ExamTypeID = ttt.ExamTypeID
            LEFT JOIN tblTestSeriesResultTime rt ON ts.RepeatExamResulttimeId = rt.ResultTimeId
            WHERE ts.TestSeriesId = @TestSeriesId";

                var testSeries = await _connection.QueryFirstOrDefaultAsync<TestSeriesResponseDTO>(query, new { TestSeriesId });

                if (testSeries == null)
                {
                    return new ServiceResponse<TestSeriesResponseDTO>(false, "Test Series not found", new TestSeriesResponseDTO(), 404);
                }

                // Fetch related data
                var testSeriesBoards = GetListOfTestSeriesBoards(TestSeriesId);
                var testSeriesClasses = GetListOfTestSeriesClasses(TestSeriesId);
                var testSeriesCourses = GetListOfTestSeriesCourse(TestSeriesId);
                var testSeriesSubjects = GetListOfTestSeriesSubjects(TestSeriesId);
                var testSeriesContentIndexes = GetListOfTestSeriesSubjectIndex(TestSeriesId);
                var testSeriesQuestionsSections = GetTestSeriesQuestionSection(TestSeriesId);
                var testSeriesInstructions = GetListOfTestSeriesInstructions(TestSeriesId);

                // Initialize the SubjectDetails list
                var testSeriesSubjectDetailsList = new List<TestSeriesSubjectDetails>();

                // Populate TestSeriesSubjectDetails with content indexes and questions section
                foreach (var subject in testSeriesSubjects)
                {
                    var subjectContentIndexes = testSeriesContentIndexes
                        .Where(ci => ci.SubjectId == subject.SubjectID)
                        .ToList();

                    var subjectQuestionsSections = testSeriesQuestionsSections
                        .Where(qs => qs.SubjectId == subject.SubjectID)
                        .ToList();

                    var subjectDetails = new TestSeriesSubjectDetails
                    {
                        SubjectID = subject.SubjectID,
                        SubjectName = subject.SubjectName,
                        TestSeriesContentIndexes = subjectContentIndexes,
                        TestSeriesQuestionsSection = subjectQuestionsSections
                    };

                    testSeriesSubjectDetailsList.Add(subjectDetails);
                }

                // Map the fetched data to the TestSeriesResponseDTO
                testSeries.TestSeriesBoard = testSeriesBoards;
                testSeries.TestSeriesClasses = testSeriesClasses;
                testSeries.TestSeriesCourses = testSeriesCourses;
                testSeries.TestSeriesSubjectDetails = testSeriesSubjectDetailsList; // Populate SubjectDetails
                testSeries.TestSeriesInstruction = testSeriesInstructions;
                testSeries.TestSeriesSubject = testSeriesSubjects;
                // Fetch TestSeriesQuestions based on TestSeriesQuestionsSection
                if (testSeriesQuestionsSections != null && testSeriesQuestionsSections.Any())
                {
                    testSeries.TestSeriesQuestions = new List<TestSeriesQuestions>();
                    foreach (var section in testSeriesQuestionsSections)
                    {
                        var questions = GetListOfTestSeriesQuestion(section.testseriesQuestionSectionid);
                        if (questions != null)
                        {
                            testSeries.TestSeriesQuestions.AddRange(questions);
                        }
                    }
                }

                return new ServiceResponse<TestSeriesResponseDTO>(true, "Success", testSeries, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<TestSeriesResponseDTO>(false, ex.Message, new TestSeriesResponseDTO(), 500);
            }
        }
        public async Task<ServiceResponse<string>> AssignTestSeries(TestseriesProfilerRequest request)
        {
            try
            {

                // Step 1: Check if there is an active assignment for this TestSeriesId
                string checkActiveSql = @"SELECT TOP 1 TSProfilerId 
                                      FROM [dbo].[tblTestSeriesProfiler]
                                      WHERE TestSeriesId = @TestSeriesId 
                                      AND IsActive = 1";

                var existingProfilerId = await _connection.QueryFirstOrDefaultAsync<int?>(checkActiveSql, new
                {
                    request.TestSeriesId
                });

                // Step 2: If there is an active record, deactivate it
                if (existingProfilerId.HasValue)
                {
                    string deactivateSql = @"UPDATE [dbo].[tblTestSeriesProfiler]
                                         SET IsActive = 0
                                         WHERE TSProfilerId = @TSProfilerId";

                    await _connection.ExecuteAsync(deactivateSql, new
                    {
                        TSProfilerId = existingProfilerId.Value
                    });
                }

                // Step 3: Insert a new active record for the new employee
                string insertSql = @"INSERT INTO [dbo].[tblTestSeriesProfiler] 
                                 (TestSeriesId, EmployeeId, AssignedDate, IsActive) 
                                 VALUES (@TestSeriesId, @EmployeeId, @AssignedDate, 1)";

                var parameters = new
                {
                    request.TestSeriesId,
                    request.EmployeeId,
                    AssignedDate = DateTime.Now
                };

                int rowsAffected = await _connection.ExecuteAsync(insertSql, parameters);

                if (rowsAffected > 0)
                {

                    return new ServiceResponse<string>(true, "Test series assigned successfully", string.Empty, 200);
                }
                else
                {
                    // Rollback the transaction in case of failure
                    return new ServiceResponse<string>(false, "Failed to assign test series", string.Empty, 500);
                }

            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<List<TestSeriesResponseDTO>>> GetTestSeriesList(TestSeriesListRequest request)
        {
            try
            {
                // Construct the SQL query with parameters
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
                    ts.ExamTypeID,
                    ttt.ExamTypeName AS ExamTypeName,
                    ts.createdon,
                    ts.createdby,
                    ts.modifiedon,
                    ts.modifiedby,
                    ts.RepeatExamStartDate,
                    ts.RepeatExamEndDate,
                    ts.RepeatExamStarttime,
                    ts.RepeatExamResulttimeId,
                    rt.ResultTime AS RepeatedExamResultTime,
                    ts.IsAdmin
                FROM tblTestSeries ts
                JOIN tblCategory ap ON ts.APID = ap.APID
                JOIN tblEmployee emp ON ts.EmployeeID = emp.EmployeeID
                JOIN tblTypeOfTestSeries tts ON ts.TypeOfTestSeries = tts.TTSId
                Left JOIN tblExamType ttt ON ts.ExamTypeID = ttt.ExamTypeID
                LEFT JOIN tblTestSeriesClass tc ON ts.TestSeriesId = tc.TestSeriesId
                LEFT JOIN tblTestSeriesCourse tco ON ts.TestSeriesId = tco.TestSeriesId
                LEFT JOIN tblTestSeriesBoards tb ON ts.TestSeriesId = tb.TestSeriesId
                LEFT JOIN tblTestSeriesResultTime rt ON ts.RepeatExamResulttimeId = rt.ResultTimeId
                WHERE 1=1 AND ts.IsAdmin = @IsAdmin";

                // Apply filters dynamically
                if (request.APId > 0)
                {
                    query += " AND ts.APID = @APId";
                }
                if (request.ClassId > 0)
                {
                    query += " AND tc.ClassId = @ClassId";
                }
                if (request.CourseId > 0)
                {
                    query += " AND tco.CourseId = @CourseId";
                }
                if (request.BoardId > 0)
                {
                    query += " AND tb.BoardId = @BoardId";
                }
                if (request.ExamTypeId > 0)
                {
                    query += " AND ts.ExamTypeID = @ExamTypeId";
                }
                if (request.TypeOfTestSeries > 0)
                {
                    query += " AND ts.TypeOfTestSeries = @TypeOfTestSeries";
                }
                if (!string.IsNullOrEmpty(request.ExamStatus))
                {
                    query += " AND (@ExamStatus IS NULL OR " +
                              "(ts.RepeatedExams = 1 AND ts.RepeatExamStartDate <= @Date AND ts.RepeatExamEndDate >= @Date) OR " +
                              "(ts.RepeatedExams = 0 AND ts.StartDate <= @Date AND DATEADD(MINUTE, CAST(ts.Duration AS INT), ts.StartDate) >= @Date))";
                }

                // Prepare the parameters for the query
                var parameters = new
                {
                    APId = request.APId == 0 ? (int?)null : request.APId,
                    ClassId = request.ClassId == 0 ? (int?)null : request.ClassId,
                    CourseId = request.CourseId == 0 ? (int?)null : request.CourseId,
                    BoardId = request.BoardId == 0 ? (int?)null : request.BoardId,
                    ExamTypeId = request.ExamTypeId == 0 ? (int?)null : request.ExamTypeId,
                    TypeOfTestSeries = request.TypeOfTestSeries == 0 ? (int?)null : request.TypeOfTestSeries,
                    ExamStatus = string.IsNullOrEmpty(request.ExamStatus) ? (string)null : request.ExamStatus,
                    Date = request.Date,
                    request.IsAdmin
                };

                // Execute the query
                var testSeriesList = await _connection.QueryAsync<TestSeriesResponseDTO>(query, parameters);

                if (testSeriesList == null || !testSeriesList.Any())
                {
                    return new ServiceResponse<List<TestSeriesResponseDTO>>(true, "No test series found", new List<TestSeriesResponseDTO>(), 200);
                }
                var totalRecords = testSeriesList.Count();
                var paginatedTestSeriesList = testSeriesList
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();
                // Fetch related data for each test series
                foreach (var testSeries in testSeriesList)
                {
                    if (testSeries.RepeatedExams)
                    {
                        // Current date and time
                        DateTime currentDateTime = DateTime.Now;

                        // Exam start and end times
                        TimeSpan examStartTime = TimeSpan.Parse(testSeries.RepeatExamStarttime);
                        int durationInMinutes = int.Parse(testSeries.Duration);

                        // Calculate the end time based on the duration
                        TimeSpan examEndTime = examStartTime.Add(TimeSpan.FromMinutes(durationInMinutes));
                        testSeries.RepeatedExamEndTime = examEndTime.ToString();

                        // Exam period start and end dates
                        DateTime repeatExamStartDate = testSeries.RepeatExamStartDate;
                        DateTime repeatExamEndDate = testSeries.RepeatExamEndDate;

                        // Exam start and end DateTime for the current day
                        DateTime dailyExamStartDateTime = repeatExamStartDate.Add(examStartTime);
                        DateTime dailyExamEndDateTime = repeatExamStartDate.Add(examEndTime);

                        if (currentDateTime < dailyExamStartDateTime)
                        {
                            testSeries.ExamStatus = "Upcoming";
                        }
                        else if (currentDateTime >= dailyExamStartDateTime && currentDateTime <= dailyExamEndDateTime)
                        {
                            testSeries.ExamStatus = "Ongoing";
                        }
                        else if (currentDateTime > dailyExamEndDateTime && currentDateTime < repeatExamEndDate.AddDays(1).Add(examStartTime))
                        {
                            testSeries.ExamStatus = "Upcoming";
                        }
                        else if (currentDateTime >= repeatExamEndDate.Add(examEndTime))
                        {
                            testSeries.ExamStatus = "Completed";
                        }
                    }
                    else
                    {
                        DateTime startDateTime = testSeries.StartDate.Value.Add(DateTime.Parse(testSeries.StartTime).TimeOfDay);

                        if (DateTime.Now < startDateTime)
                        {
                            testSeries.ExamStatus = "Upcoming";
                        }
                        else if (DateTime.Now >= startDateTime && DateTime.Now <= testSeries.ResultDate)
                        {
                            testSeries.ExamStatus = "Ongoing";
                        }
                        else
                        {
                            testSeries.ExamStatus = "Completed";
                        }
                    }

                    // Fetch related data
                    var testSeriesBoards = GetListOfTestSeriesBoards(testSeries.TestSeriesId);
                    var testSeriesClasses = GetListOfTestSeriesClasses(testSeries.TestSeriesId);
                    var testSeriesCourses = GetListOfTestSeriesCourse(testSeries.TestSeriesId);
                    var testSeriesSubjects = GetListOfTestSeriesSubjects(testSeries.TestSeriesId);
                    var testSeriesContentIndexes = GetListOfTestSeriesSubjectIndex(testSeries.TestSeriesId);
                    var testSeriesQuestionsSections = GetTestSeriesQuestionSection(testSeries.TestSeriesId);
                    var testSeriesInstructions = GetListOfTestSeriesInstructions(testSeries.TestSeriesId);

                    // Initialize the SubjectDetails list
                    var testSeriesSubjectDetailsList = new List<TestSeriesSubjectDetails>();

                    // Populate TestSeriesSubjectDetails with content indexes and questions section
                    foreach (var subject in testSeriesSubjects)
                    {
                        var subjectContentIndexes = testSeriesContentIndexes
                            .Where(ci => ci.SubjectId == subject.SubjectID)
                            .ToList();

                        var subjectQuestionsSections = testSeriesQuestionsSections
                            .Where(qs => qs.SubjectId == subject.SubjectID)
                            .ToList();

                        var subjectDetails = new TestSeriesSubjectDetails
                        {
                            SubjectID = subject.SubjectID,
                            SubjectName = subject.SubjectName,
                            TestSeriesContentIndexes = subjectContentIndexes,
                            TestSeriesQuestionsSection = subjectQuestionsSections
                        };

                        testSeriesSubjectDetailsList.Add(subjectDetails);
                    }

                    // Map the fetched data to the TestSeriesResponseDTO
                    testSeries.TestSeriesBoard = testSeriesBoards;
                    testSeries.TestSeriesClasses = testSeriesClasses;
                    testSeries.TestSeriesCourses = testSeriesCourses;
                    testSeries.TestSeriesSubjectDetails = testSeriesSubjectDetailsList; // Populate SubjectDetails
                    testSeries.TestSeriesInstruction = testSeriesInstructions;

                    // Fetch TestSeriesQuestions based on TestSeriesQuestionsSection
                    if (testSeriesQuestionsSections != null && testSeriesQuestionsSections.Any())
                    {
                        testSeries.TestSeriesQuestions = new List<TestSeriesQuestions>();
                        foreach (var section in testSeriesQuestionsSections)
                        {
                            var questions = GetListOfTestSeriesQuestion(section.testseriesQuestionSectionid);
                            if (questions != null)
                            {
                                testSeries.TestSeriesQuestions.AddRange(questions);
                            }
                        }
                    }
                }

                return new ServiceResponse<List<TestSeriesResponseDTO>>(true, "Test series retrieved successfully", testSeriesList.ToList(), 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<TestSeriesResponseDTO>>(false, "An error occurred while retrieving test series: " + ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<string>> TestSeriesContentIndexMapping(List<TestSeriesContentIndex> request, int TestSeriesId)
        {
            foreach (var data in request)
            {
                data.TestSeriesID = TestSeriesId;
            }
            string query = "SELECT COUNT(*) FROM [tblTestSeriesContentIndex] WHERE [TestSeriesID] = @TestSeriesId";
            int count = await _connection.QueryFirstOrDefaultAsync<int>(query, new { TestSeriesId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblTestSeriesContentIndex] WHERE [TestSeriesID] = @TestSeriesId;";
                var rowsAffected = await _connection.ExecuteAsync(deleteDuery, new { TestSeriesId });
                if (rowsAffected > 0)
                {
                    string insertQuery = @"
                    INSERT INTO tblTestSeriesContentIndex (IndexTypeId, ContentIndexId, TestSeriesID, SubjectId)
                    VALUES (@IndexTypeId, @ContentIndexId, @TestSeriesID, @SubjectId);";

                    var valuesInserted = await _connection.ExecuteAsync(insertQuery, request);

                    return new ServiceResponse<string>(true, "Success", "Data added successfully", 200);
                }
                else
                {
                    return new ServiceResponse<string>(true, "failure", "Data addition failed", 500);
                }
            }
            else
            {
                string insertQuery = @"
                    INSERT INTO tblTestSeriesContentIndex (IndexTypeId, ContentIndexId, TestSeriesID, SubjectId)
                    VALUES (@IndexTypeId, @ContentIndexId, @TestSeriesID, @SubjectId);";

                var valuesInserted = await _connection.ExecuteAsync(insertQuery, request);
                return new ServiceResponse<string>(true, "Success", "Data added successfully", 200);
            }
        }
        public async Task<ServiceResponse<string>> TestSeriesQuestionSectionMapping(List<TestSeriesQuestionSection> request, int TestSeriesId)
        {
            // Update TestSeriesId for all items in the request list
            foreach (var section in request)
            {
                section.TestSeriesid = TestSeriesId;
            }
            try
            {
                // Delete existing records for the given TestSeriesId
                var deleteQuery = "DELETE FROM [tbltestseriesQuestionSection] WHERE [TestSeriesid] = @TestSeriesId";
                await _connection.ExecuteAsync(deleteQuery, new { TestSeriesId });

                // Insert new records
                string insertQuery = @"
            INSERT INTO tbltestseriesQuestionSection 
            (TestSeriesid, DisplayOrder, SectionName, Status, LevelID1, QuesPerDifficulty1, LevelID2, QuesPerDifficulty2, LevelID3, QuesPerDifficulty3, QuestionTypeID, EntermarksperCorrectAnswer, EnterNegativeMarks, TotalNoofQuestions, NoofQuestionsforChoice, SubjectId)
            VALUES 
            (@TestSeriesid, @DisplayOrder, @SectionName, @Status, @LevelID1, @QuesPerDifficulty1, @LevelID2, @QuesPerDifficulty2, @LevelID3, @QuesPerDifficulty3, @QuestionTypeID, @EntermarksperCorrectAnswer, @EnterNegativeMarks, @TotalNoofQuestions, @NoofQuestionsforChoice, @SubjectId);";

                var valuesInserted = await _connection.ExecuteAsync(insertQuery, request);
                return new ServiceResponse<string>(true, "operation successful", "values added successfully", 200);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating test series question sections", ex);
            }
        }
        public async Task<ServiceResponse<string>> TestSeriesInstructionsMapping(TestSeriesInstructions request, int TestSeriesId)
        {
            // Step 1: Set TestSeriesID for the request object
            request.TestSeriesID = TestSeriesId;

            // Step 2: Check if it's an update operation (when TestInstructionsId is passed)
            if (request.TestInstructionsId > 0)
            {
                // Update the existing record
                string updateQuery = @"
            UPDATE [tblTestInstructions]
            SET Instructions = @Instructions,
                InstructionName = @InstructionName,
                InstructionId = @InstructionId
            WHERE TestInstructionsId = @TestInstructionsId AND TestSeriesID = @TestSeriesID";

                int rowsUpdated = await _connection.ExecuteAsync(updateQuery, request);

                if (rowsUpdated > 0)
                {
                    return new ServiceResponse<string>(true, "Operation successful", "Record updated successfully", 200);
                }
                else
                {
                    return new ServiceResponse<string>(false, "Operation failed", "Failed to update the record", 500);
                }
            }
            else
            {
                // Insert a new record
                string insertQuery = @"
            INSERT INTO [tblTestInstructions] (Instructions, TestSeriesID, InstructionName, InstructionId)
            VALUES (@Instructions, @TestSeriesID, @InstructionName, @InstructionId)";

                int rowsInserted = await _connection.ExecuteAsync(insertQuery, request);

                if (rowsInserted > 0)
                {
                    return new ServiceResponse<string>(true, "Operation successful", "Record added successfully", 200);
                }
                else
                {
                    return new ServiceResponse<string>(false, "Operation failed", "Failed to insert the record", 500);
                }
            }
        }
        public async Task<ServiceResponse<string>> TestSeriesQuestionsMapping(List<TestSeriesQuestions> request, int TestSeriesId, int sectionId)
        {
            // Step 1: Assign TestSeriesId and sectionId to each question in the request
            foreach (var data in request)
            {
                data.TestSeriesid = TestSeriesId;
                data.testseriesQuestionSectionid = sectionId;
            }

            try
            {
                // Step 2: Fetch total allowed questions and difficulty level data from tbltestseriesQuestionSection
                string getQuestionSectionQuery = @"
            SELECT TotalNoofQuestions, 
                   LevelID1, QuesPerDifficulty1, 
                   LevelID2, QuesPerDifficulty2, 
                   LevelID3, QuesPerDifficulty3
            FROM tbltestseriesQuestionSection 
            WHERE TestSeriesid = @TestSeriesId AND testseriesQuestionSectionid = @testseriesQuestionSectionid";

                var questionSection = await _connection.QueryFirstOrDefaultAsync(getQuestionSectionQuery, new { TestSeriesId, testseriesQuestionSectionid = sectionId });

                if (questionSection == null)
                {
                    return new ServiceResponse<string>(false, "operation failed", "Test series question section not found.", 400);
                }

                // Step 3: Validate total number of questions
                //if (request.Count > questionSection.TotalNoofQuestions)
                //{
                //    return new ServiceResponse<string>(false, "operation failed", $"Number of questions exceeds the allowed limit of {questionSection.TotalNoofQuestions} for this section.", 400);
                //}

                // Step 4: Get the question codes and fetch their difficulty levels from tblQIDCourse
                string difficultyLevelQuery = @"
            SELECT qc.QID, qc.LevelId
            FROM tblQIDCourse qc
            WHERE qc.QID IN @QuestionCodes";

                var questionCodes = request.Select(q => q.Questionid).ToList();
                var difficultyLevels = await _connection.QueryAsync(difficultyLevelQuery, new { QuestionCodes = questionCodes });

                // Step 5: Group questions by their difficulty level
                var questionsGroupedByDifficulty = difficultyLevels.GroupBy(q => q.LevelId)
                    .Select(g => new { LevelId = g.Key, QuestionCount = g.Count() })
                    .ToList();

                // Step 6: Validate the number of questions per difficulty level
                //foreach (var group in questionsGroupedByDifficulty)
                //{
                //    if (group.LevelId == questionSection.LevelID1 && group.QuestionCount > questionSection.QuesPerDifficulty1)
                //    {
                //        return new ServiceResponse<string>(false, "operation failed", $"Number of questions for difficulty level {questionSection.LevelID1} exceeds the allowed limit of {questionSection.QuesPerDifficulty1}.", 400);
                //    }

                //    if (group.LevelId == questionSection.LevelID2 && group.QuestionCount > questionSection.QuesPerDifficulty2)
                //    {
                //        return new ServiceResponse<string>(false, "operation failed", $"Number of questions for difficulty level {questionSection.LevelID2} exceeds the allowed limit of {questionSection.QuesPerDifficulty2}.", 400);
                //    }

                //    if (group.LevelId == questionSection.LevelID3 && group.QuestionCount > questionSection.QuesPerDifficulty3)
                //    {
                //        return new ServiceResponse<string>(false, "operation failed", $"Number of questions for difficulty level {questionSection.LevelID3} exceeds the allowed limit of {questionSection.QuesPerDifficulty3}.", 400);
                //    }
                //}

                // Step 7: Check if there are existing questions for this section and delete them if necessary
                string existingQuestionsQuery = "SELECT COUNT(*) FROM tbltestseriesQuestions WHERE testseriesQuestionSectionid = @testseriesQuestionSectionid";
                int existingQuestionsCount = await _connection.QueryFirstOrDefaultAsync<int>(existingQuestionsQuery, new { testseriesQuestionSectionid = sectionId });

                if (existingQuestionsCount > 0)
                {
                    // Step 8: Delete existing questions
                    string deleteQuery = "DELETE FROM tbltestseriesQuestions WHERE testseriesQuestionSectionid = @testseriesQuestionSectionid";
                    await _connection.ExecuteAsync(deleteQuery, new { testseriesQuestionSectionid = sectionId });
                }

                // Step 9: Insert new questions
                string insertQuery = @"
            INSERT INTO tbltestseriesQuestions (TestSeriesid, Questionid, DisplayOrder, Status, testseriesQuestionSectionid, QuestionCode) 
            VALUES (@TestSeriesid, @Questionid, @DisplayOrder, @Status, @testseriesQuestionSectionid, @QuestionCode);";
                await _connection.ExecuteAsync(insertQuery, request);

                return new ServiceResponse<string>(true, "operation successful", "Questions mapped successfully", 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, "An error occurred while mapping test series questions", 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuestionsList(GetAllQuestionListRequest request)
        {
            try
            {
                // Fetch the total number of questions and question limits from tbltestseriesQuestionSection
                string queryForQuestionLimits = @"
        SELECT TotalNoofQuestions, 
               QuesPerDifficulty1, 
               QuesPerDifficulty2, 
               QuesPerDifficulty3, 
               LevelID1, 
               LevelID2, 
               LevelID3 
        FROM tbltestseriesQuestionSection 
        WHERE [testseriesQuestionSectionid] = @SectionId";

                var sectionLimits = await _connection.QueryFirstOrDefaultAsync(queryForQuestionLimits, new { SectionId = request.SectionId });
                if (sectionLimits == null)
                {
                    return new ServiceResponse<List<QuestionResponseDTO>>(false, "Section not found", new List<QuestionResponseDTO>(), 404);
                }

                int totalQuestionsAllowed = sectionLimits.TotalNoofQuestions;
                int maxEasyQuestions = sectionLimits.QuesPerDifficulty1;
                int maxMediumQuestions = sectionLimits.QuesPerDifficulty2;
                int maxHardQuestions = sectionLimits.QuesPerDifficulty3;

                // SQL query to fetch questions based on difficulty level and question type
                string sql = @"
        SELECT 
            q.QuestionCode, q.QuestionDescription, q.QuestionFormula,q.IsLive, q.QuestionTypeId, q.ApprovedStatus, 
            q.ApprovedBy, q.ReasonNote, q.Status, q.CreatedBy, q.CreatedOn, q.ModifiedBy, q.ModifiedOn, 
            q.Verified, q.courseid, c.CourseName, q.boardid, b.BoardName, q.classid, cl.ClassName, 
            q.subjectID, s.SubjectName, q.ExamTypeId, e.ExamTypeName, q.EmployeeId, emp.EmpFirstName as EmployeeName, 
            q.Rejectedby, q.RejectedReason, q.IndexTypeId, it.IndexType as IndexTypeName, q.ContentIndexId,
            CASE 
                WHEN q.IndexTypeId = 1 THEN ci.ContentName_Chapter
                WHEN q.IndexTypeId = 2 THEN ct.ContentName_Topic
                WHEN q.IndexTypeId = 3 THEN cst.ContentName_SubTopic
            END AS ContentIndexName
        FROM tblQuestion q
        LEFT JOIN tblCourse c ON q.courseid = c.CourseId
        LEFT JOIN tblBoard b ON q.boardid = b.BoardId
        LEFT JOIN tblClass cl ON q.classid = cl.ClassId
        LEFT JOIN tblSubject s ON q.subjectID = s.SubjectId
        LEFT JOIN tblExamType e ON q.ExamTypeId = e.ExamTypeId
        LEFT JOIN tblEmployee emp ON q.EmployeeId = emp.EmployeeId
        LEFT JOIN tblQBIndexType it ON q.IndexTypeId = it.IndexId
        LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
        LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
        LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
        WHERE q.subjectID = @Subjectid
          AND (@IndexTypeId = 0 OR q.IndexTypeId = @IndexTypeId)
          AND (@ContentId = 0 OR q.ContentIndexId = @ContentId)
          AND (@QuestionTypeId = 0 OR q.QuestionTypeId = @QuestionTypeId)
          AND EXISTS (SELECT 1 FROM tblQIDCourse qc WHERE qc.QuestionCode = q.QuestionCode AND qc.LevelId = @DifficultyLevelId)
          AND q.IsLive = 1
        ORDER BY NEWID()"; // Randomly select questions

                // Fetch questions for each difficulty level, ensuring the distribution
                var easyQuestions = await _connection.QueryAsync<QuestionResponseDTO>(sql, new
                {
                    Subjectid = request.Subjectid,
                    IndexTypeId = request.IndexTypeId,
                    ContentId = request.ContentId,
                    QuestionTypeId = request.QuestionTypeId,
                    DifficultyLevelId = sectionLimits.LevelID1 // Easy
                });

                var mediumQuestions = await _connection.QueryAsync<QuestionResponseDTO>(sql, new
                {
                    Subjectid = request.Subjectid,
                    IndexTypeId = request.IndexTypeId,
                    ContentId = request.ContentId,
                    QuestionTypeId = request.QuestionTypeId,
                    DifficultyLevelId = sectionLimits.LevelID2 // Medium
                });

                var hardQuestions = await _connection.QueryAsync<QuestionResponseDTO>(sql, new
                {
                    Subjectid = request.Subjectid,
                    IndexTypeId = request.IndexTypeId,
                    ContentId = request.ContentId,
                    QuestionTypeId = request.QuestionTypeId,
                    DifficultyLevelId = sectionLimits.LevelID3 // Hard
                });

                // Select the required number of questions while ensuring the distribution does not exceed the limits
                var selectedQuestions = easyQuestions.Take(maxEasyQuestions)
                    .Concat(mediumQuestions.Take(maxMediumQuestions))
                    .Concat(hardQuestions.Take(maxHardQuestions))
                    .ToList();
                var paginatedResponse = selectedQuestions
                  .Skip((request.PageNumber - 1) * request.PageSize)
                  .Take(request.PageSize)
                  .ToList();
                // Check if selected questions exceed the allowed total
                if (selectedQuestions.Count > totalQuestionsAllowed)
                {
                    return new ServiceResponse<List<QuestionResponseDTO>>(false, "More questions than allowed", new List<QuestionResponseDTO>(), 400);
                }

                return new ServiceResponse<List<QuestionResponseDTO>>(true, "Questions retrieved successfully", paginatedResponse, 200, selectedQuestions.Count);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(false, ex.Message, new List<QuestionResponseDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<List<ContentIndexResponses>>> GetSyllabusDetailsBySubject(SyllabusDetailsRequest request)
        {
            try
            {
                // Fetch APID and ExamTypeID from the TestSeries table
                string testSeriesSql = @"
            SELECT APID, ExamTypeID
            FROM tblTestSeries
            WHERE TestSeriesId = @TestSeriesId";

                var testSeries = await _connection.QueryFirstOrDefaultAsync<dynamic>(testSeriesSql, new { request.TestSeriesId });

                if (testSeries == null)
                {
                    return new ServiceResponse<List<ContentIndexResponses>>(false, "Test Series not found", new List<ContentIndexResponses>(), 404);
                }

                int APId = testSeries.APID;
                int? examTypeId = testSeries.ExamTypeID;

                int boardId = 0, classId = 0, courseId = 0;

                if (APId == 1)
                {
                    // Fetch Board, Class, and Course details if APId is 1
                    var boardSql = @"SELECT BoardId FROM tblTestSeriesBoards WHERE TestSeriesId = @TestSeriesId";
                    var classSql = @"SELECT ClassId FROM tblTestSeriesClass WHERE TestSeriesId = @TestSeriesId";
                    var courseSql = @"SELECT CourseId FROM tblTestSeriesCourse WHERE TestSeriesId = @TestSeriesId";

                    boardId = await _connection.QueryFirstOrDefaultAsync<int>(boardSql, new { request.TestSeriesId });
                    classId = await _connection.QueryFirstOrDefaultAsync<int>(classSql, new { request.TestSeriesId });
                    courseId = await _connection.QueryFirstOrDefaultAsync<int>(courseSql, new { request.TestSeriesId });
                }

                // Now proceed with syllabus details retrieval
                string sql = @"
        SELECT sd.*, s.*
        FROM [tblSyllabus] s
        JOIN [tblSyllabusDetails] sd ON s.SyllabusId = sd.SyllabusID
        WHERE s.APID = @APId
        AND (s.BoardID = @BoardId OR @BoardId = 0)
        AND (s.ClassId = @ClassId OR @ClassId = 0)
        AND (s.CourseId = @CourseId OR @CourseId = 0)
        AND (sd.SubjectId = @SubjectId OR @SubjectId = 0)";

                var syllabusDetails = await _connection.QueryAsync<dynamic>(sql, new
                {
                    APId = APId,
                    BoardId = boardId,
                    ClassId = classId,
                    CourseId = courseId,
                    SubjectId = request.SubjectId
                });

                var contentIndexResponse = new List<ContentIndexResponses>();

                // Existing logic to process the syllabus details goes here
                foreach (var detail in syllabusDetails)
                {
                    int indexTypeId = detail.IndexTypeId;
                    if (indexTypeId == 1) // Chapter
                    {
                        // Fetch and map chapter data
                        string getchapter = @"select * from tblContentIndexChapters where ContentIndexId = @ContentIndexId;";
                        var data = await _connection.QueryFirstOrDefaultAsync<ContentIndexResponses>(getchapter, new { ContentIndexId = detail.ContentIndexId });

                        var chapter = new ContentIndexResponses
                        {
                            ContentIndexId = data.ContentIndexId,
                            SubjectId = data.SubjectId,
                            ContentName_Chapter = data.ContentName_Chapter,
                            Status = data.Status,
                            IndexTypeId = indexTypeId,
                            BoardId = data.BoardId,
                            ClassId = data.ClassId,
                            CourseId = data.CourseId,
                            APID = data.APID,
                            CreatedOn = data.CreatedOn,
                            CreatedBy = data.CreatedBy,
                            ModifiedOn = data.ModifiedOn,
                            ModifiedBy = data.ModifiedBy,
                            EmployeeId = data.EmployeeId,
                            ExamTypeId = data.ExamTypeId,
                            IsActive = data.Status, // Assuming Status is used for IsActive
                            ChapterCode = data.ChapterCode,
                            DisplayName = data.DisplayName,
                            DisplayOrder = data.DisplayOrder,
                            ContentIndexTopics = new List<ContentIndexTopicsResponse>()
                        };

                        // Add chapter to response list
                        contentIndexResponse.Add(chapter);
                    }
                    else if (indexTypeId == 2) // Topic
                    {
                        // Fetch and map topic data
                        string gettopic = @"select * from tblContentIndexTopics where ContInIdTopic = @ContentIndexId;";
                        var data = await _connection.QueryFirstOrDefaultAsync<ContentIndexTopicsResponse>(gettopic, new { ContentIndexId = detail.ContentIndexId });

                        var topic = new ContentIndexTopicsResponse
                        {
                            ContInIdTopic = data.ContInIdTopic,
                            ContentIndexId = data.ContentIndexId,
                            ContentName_Topic = data.ContentName_Topic,
                            Status = data.Status,
                            IndexTypeId = indexTypeId,
                            CreatedOn = data.CreatedOn,
                            CreatedBy = data.CreatedBy,
                            ModifiedOn = data.ModifiedOn,
                            ModifiedBy = data.ModifiedBy,
                            EmployeeId = data.EmployeeId,
                            IsActive = data.Status,
                            TopicCode = data.TopicCode,
                            DisplayName = data.DisplayName,
                            DisplayOrder = data.DisplayOrder,
                            ChapterCode = data.ChapterCode,
                            ContentIndexSubTopics = new List<ContentIndexSubTopicResponse>()
                        };

                        // Ensure chapter exists or create a dummy one
                        var existingChapter = contentIndexResponse.FirstOrDefault(c => c.ChapterCode == data.ChapterCode);
                        if (existingChapter == null)
                        {
                            existingChapter = new ContentIndexResponses
                            {
                                ChapterCode = data.ChapterCode,
                                ContentName_Chapter = "N/A", // Dummy entry for the chapter
                                ContentIndexTopics = new List<ContentIndexTopicsResponse> { topic }
                            };
                            contentIndexResponse.Add(existingChapter);
                        }
                        else
                        {
                            existingChapter.ContentIndexTopics.Add(topic);
                        }
                    }
                    else if (indexTypeId == 3) // SubTopic
                    {
                        // Fetch and map subtopic data
                        string getsubtopic = @"select * from tblContentIndexSubTopics where ContInIdSubTopic = @ContentIndexId;";
                        var data = await _connection.QueryFirstOrDefaultAsync<ContentIndexSubTopicResponse>(getsubtopic, new { ContentIndexId = detail.ContentIndexId });

                        var subTopic = new ContentIndexSubTopicResponse
                        {
                            ContInIdSubTopic = data.ContInIdSubTopic,
                            ContInIdTopic = data.ContInIdTopic,
                            ContentName_SubTopic = data.ContentName_SubTopic,
                            Status = data.Status,
                            IndexTypeId = indexTypeId,
                            CreatedOn = data.CreatedOn,
                            CreatedBy = data.CreatedBy,
                            ModifiedOn = data.ModifiedOn,
                            ModifiedBy = data.ModifiedBy,
                            EmployeeId = data.EmployeeId,
                            IsActive = data.Status,
                            SubTopicCode = data.SubTopicCode,
                            DisplayName = data.DisplayName,
                            DisplayOrder = data.DisplayOrder,
                            TopicCode = data.TopicCode
                        };

                        // Ensure topic exists or create a dummy one
                        var existingTopic = contentIndexResponse
                            .SelectMany(c => c.ContentIndexTopics)
                            .FirstOrDefault(t => t.TopicCode == data.TopicCode);

                        if (existingTopic == null)
                        {
                            var dummyTopic = new ContentIndexTopicsResponse
                            {
                                TopicCode = data.TopicCode,
                                ContentName_Topic = "N/A", // Dummy entry for the topic
                                ContentIndexSubTopics = new List<ContentIndexSubTopicResponse> { subTopic }
                            };

                            // Ensure chapter exists or create a dummy one
                            var chapterForTopic = contentIndexResponse.FirstOrDefault(c => c.ChapterCode == detail.ChapterCode);
                            if (chapterForTopic == null)
                            {
                                chapterForTopic = new ContentIndexResponses
                                {
                                    ChapterCode = detail.ChapterCode,
                                    ContentName_Chapter = "N/A", // Dummy entry for the chapter
                                    ContentIndexTopics = new List<ContentIndexTopicsResponse> { dummyTopic }
                                };
                                contentIndexResponse.Add(chapterForTopic);
                            }
                            else
                            {
                                chapterForTopic.ContentIndexTopics.Add(dummyTopic);
                            }
                        }
                        else
                        {
                            existingTopic.ContentIndexSubTopics.Add(subTopic);
                        }
                    }
                }

                return new ServiceResponse<List<ContentIndexResponses>>(true, "Syllabus details retrieved successfully", contentIndexResponse, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ContentIndexResponses>>(false, ex.Message, new List<ContentIndexResponses>(), 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetAutoGeneratedQuestionList(QuestionListRequest request)
        {
            try
            {
                // Fetch content indices mapped to the given SubjectId and TestSeriesId
                string contentIndexQuery = @"
        SELECT ContentIndexId, IndexTypeId
        FROM tblTestSeriesContentIndex
        WHERE TestSeriesID = @TestSeriesId AND SubjectId = @SubjectId";

                var contentIndices = await _connection.QueryAsync<dynamic>(contentIndexQuery, new
                {
                    TestSeriesId = request.TestSeriesId,
                    SubjectId = request.SubjectId
                });

                var contentIndexIds = contentIndices.Select(ci => ci.ContentIndexId).ToList();

                if (contentIndexIds.Count == 0)
                {
                    return new ServiceResponse<List<QuestionResponseDTO>>(false, "No content indices found.", new List<QuestionResponseDTO>(), 404);
                }

                // Fetch a randomly selected section associated with the TestSeriesId
                string sectionQuery = @"
        SELECT * 
        FROM tbltestseriesQuestionSection 
        WHERE testseriesQuestionSectionid = @SectionId";

                var selectedSection = await _connection.QuerySingleOrDefaultAsync<dynamic>(sectionQuery, new
                {
                    SectionId = request.SectionId
                });

                if (selectedSection == null)
                {
                    return new ServiceResponse<List<QuestionResponseDTO>>(false, "No sections found for the test series.", new List<QuestionResponseDTO>(), 404);
                }

                // Fetch questions based on the chapters, topics, and subtopics with difficulty levels
                string questionQuery = @"
        SELECT 
            q.QuestionCode,
            q.QuestionDescription,
            q.QuestionFormula,
            q.QuestionTypeId,
            q.ApprovedStatus,
            q.ApprovedBy,
            q.ReasonNote,
            q.Status,
            q.CreatedBy,
            q.CreatedOn,
            q.ModifiedBy,
            q.ModifiedOn,
            q.Verified,
            q.courseid,
            q.boardid,
            q.classid,
            q.subjectID,
            q.Rejectedby,
            q.RejectedReason,
            q.IndexTypeId,
            q.ContentIndexId,
            q.IsLive,
            q.EmployeeId,
            q.ExamTypeId,
            q.IsActive,
            qc.LevelId,
            s.SubjectName,
            e.EmpFirstName as EmployeeName,
            it.IndexType AS IndexTypeName
        FROM 
            tblQuestion q
        INNER JOIN 
            tblQIDCourse qc ON q.QuestionCode = qc.QuestionCode
        LEFT JOIN 
            tblSubject s ON q.subjectID = s.SubjectId
        LEFT JOIN 
            tblEmployee e ON q.EmployeeId = e.EmployeeID
        LEFT JOIN 
            tblQBIndexType it ON q.IndexTypeId = it.IndexId
        WHERE 
            q.ContentIndexId IN @ContentIndexIds
            AND q.IsActive = 1
            AND q.IsRejected = 0
            AND q.IsLive = 1";

                var questions = await _connection.QueryAsync<QuestionResponseDTO>(questionQuery, new
                {
                    ContentIndexIds = contentIndexIds
                });

                if (!questions.Any())
                {
                    return new ServiceResponse<List<QuestionResponseDTO>>(false, "No questions found.", new List<QuestionResponseDTO>(), 404);
                }

                // Create a list to hold selected questions
                var selectedQuestions = new List<QuestionResponseDTO>();

                // Logic to select questions based on levels
                int totalEasyQuestions = 0;
                int totalMediumQuestions = 0;
                int totalHardQuestions = 0;

                foreach (var question in questions)
                {
                    if (question.LevelId == selectedSection.LevelID1 && totalEasyQuestions < selectedSection.QuesPerDifficulty1)
                    {
                        selectedQuestions.Add(question);
                        totalEasyQuestions++;
                    }
                    else if (question.LevelId == selectedSection.LevelID2 && totalMediumQuestions < selectedSection.QuesPerDifficulty2)
                    {
                        selectedQuestions.Add(question);
                        totalMediumQuestions++;
                    }
                    else if (question.LevelId == selectedSection.LevelID3 && totalHardQuestions < selectedSection.QuesPerDifficulty3)
                    {
                        selectedQuestions.Add(question);
                        totalHardQuestions++;
                    }

                    // Break out of the loop if the total number of selected questions reaches the limit
                    if (selectedQuestions.Count >= selectedSection.TotalNoofQuestions)
                    {
                        break;
                    }
                }

                // If total questions exceed the desired count, truncate the list manually
                if (selectedQuestions.Count > selectedSection.TotalNoofQuestions)
                {
                    var truncatedQuestions = new List<QuestionResponseDTO>();

                    for (int i = 0; i < selectedSection.TotalNoofQuestions; i++)
                    {
                        // Check to prevent IndexOutOfRangeException
                        if (i < selectedQuestions.Count)
                        {
                            truncatedQuestions.Add(selectedQuestions[i]);
                        }
                    }
                    selectedQuestions = truncatedQuestions; // Replace with the truncated list
                }

                // Return the selected questions
                return new ServiceResponse<List<QuestionResponseDTO>>(true, "Questions fetched successfully.", selectedQuestions, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(false, ex.Message, new List<QuestionResponseDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<List<TestSeriesSectionDTO>>> GetSectionsByTestSeriesId(int testSeriesId)
        {
            try
            {
                string query = @"
        SELECT 
            testseriesQuestionSectionid AS TestSeriesQuestionSectionId,
            SectionName,
            TotalNoofQuestions
        FROM tbltestseriesQuestionSection
        WHERE TestSeriesid = @TestSeriesId AND Status = 1"; // Assuming Status 1 is active

                var sections = await _connection.QueryAsync<TestSeriesSectionDTO>(query, new { TestSeriesId = testSeriesId });

                if (!sections.Any())
                {
                    return new ServiceResponse<List<TestSeriesSectionDTO>>(false, "No sections found.", new List<TestSeriesSectionDTO>(), 404);
                }

                return new ServiceResponse<List<TestSeriesSectionDTO>>(true, "Sections fetched successfully.", sections.ToList(), 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<TestSeriesSectionDTO>>(false, ex.Message, new List<TestSeriesSectionDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionTypeDTO>>> GetQuestionTypesBySectionId(int sectionId)
        {
            try
            {
                string query = @"
        SELECT 
            tsqs.[testseriesQuestionSectionid],
            tsqs.QuestionTypeID,
            qt.QuestionType
        FROM tbltestseriesQuestionSection tsqs
        JOIN tblQBQuestionType qt ON tsqs.[QuestionTypeID] = qt.QuestionTypeID
        WHERE tsqs.testseriesQuestionSectionid = @SectionId AND qt.Status = 1"; // Assuming Status 1 is active

                var questionTypes = await _connection.QueryAsync<QuestionTypeDTO>(query, new { SectionId = sectionId });

                if (!questionTypes.Any())
                {
                    return new ServiceResponse<List<QuestionTypeDTO>>(false, "No question types found for the given section.", new List<QuestionTypeDTO>(), 404);
                }

                return new ServiceResponse<List<QuestionTypeDTO>>(true, "Question types fetched successfully.", questionTypes.ToList(), 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionTypeDTO>>(false, ex.Message, new List<QuestionTypeDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<List<DifficultyLevelDTO>>> GetDifficultyLevelsBySectionId(int sectionId)
        {
            try
            {
                string query = @"
        SELECT 
            dl.LevelId,
            dl.LevelName,
            dl.LevelCode
        FROM tbltestseriesQuestionSection tsqs
        INNER JOIN tbldifficultylevel dl ON dl.LevelId IN (tsqs.LevelID1, tsqs.LevelID2, tsqs.LevelID3)
        WHERE tsqs.testseriesQuestionSectionid = @SectionId AND dl.Status = 1"; // Assuming Status 1 is active

                var difficultyLevels = await _connection.QueryAsync<DifficultyLevelDTO>(query, new { SectionId = sectionId });

                if (!difficultyLevels.Any())
                {
                    return new ServiceResponse<List<DifficultyLevelDTO>>(false, "No difficulty levels found for the given section.", new List<DifficultyLevelDTO>(), 404);
                }

                return new ServiceResponse<List<DifficultyLevelDTO>>(true, "Difficulty levels fetched successfully.", difficultyLevels.ToList(), 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<DifficultyLevelDTO>>(false, ex.Message, new List<DifficultyLevelDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<List<ChapterDTO>>> GetTestSeriesContentIndexHierarchy(int testSeriesId)
        {
            try
            {
                // Query to get chapters
                string chapterQuery = @"
        SELECT tsci.TestSeriesContentIndexId, tsci.ContentIndexId, tsci.SubjectId, tsci.IndexTypeId, tsci.Status, ci.ContentName_Chapter
        FROM tblTestSeriesContentIndex tsci
        INNER JOIN tblContentIndexChapters ci ON tsci.ContentIndexId = ci.ContentIndexId
        WHERE tsci.TestSeriesID = @TestSeriesId AND tsci.IndexTypeId = 1";

                // Query to get topics
                string topicQuery = @"
        SELECT tsci.TestSeriesContentIndexId, tsci.ContentIndexId, tsci.SubjectId, tsci.IndexTypeId, tsci.Status, ti.ContentName_Topic, ti.ContInIdTopic
        FROM tblTestSeriesContentIndex tsci
        INNER JOIN tblContentIndexTopics ti ON tsci.ContentIndexId = ti.ContentIndexId
        WHERE tsci.TestSeriesID = @TestSeriesId AND tsci.IndexTypeId = 2";

                // Query to get subtopics
                string subTopicQuery = @"
        SELECT tsci.TestSeriesContentIndexId, tsci.ContentIndexId, tsci.SubjectId, tsci.IndexTypeId, tsci.Status, sti.ContentName_SubTopic, sti.ContInIdSubTopic
        FROM tblTestSeriesContentIndex tsci
        INNER JOIN tblContentIndexSubTopics sti ON tsci.ContentIndexId = sti.ContentIndexId
        WHERE tsci.TestSeriesID = @TestSeriesId AND tsci.IndexTypeId = 3";

                // Fetch the data
                var chapters = (await _connection.QueryAsync<ChapterDTO>(chapterQuery, new { TestSeriesId = testSeriesId })).ToList();
                var topics = (await _connection.QueryAsync<ConceptDTO>(topicQuery, new { TestSeriesId = testSeriesId })).ToList();
                var subTopics = (await _connection.QueryAsync<SubConceptDTO>(subTopicQuery, new { TestSeriesId = testSeriesId })).ToList();

                // Map subtopics to their corresponding topics
                foreach (var topic in topics)
                {
                    topic.SubConcepts = subTopics.Where(st => st.ContInIdTopic == topic.ContInIdTopic).ToList();
                }

                // Map topics to their corresponding chapters
                foreach (var chapter in chapters)
                {
                    chapter.Concepts = topics.Where(t => t.ContentIndexId == chapter.ContentIndexId).ToList();
                }

                // Handle cases where parent topics or chapters are not mapped (set as NA)
                foreach (var topic in topics.Where(t => !chapters.Any(c => c.ContentIndexId == t.ContentIndexId)))
                {
                    chapters.Add(new ChapterDTO
                    {
                        TestseriesContentIndexId = 0, // NA
                        SubjectId = topic.SubjectId,
                        ContentIndexId = 0, // NA
                        IndexTypeId = 1, // Chapter
                        Status = true,
                        Concepts = new List<ConceptDTO> { topic }
                    });
                }

                foreach (var subTopic in subTopics.Where(st => !topics.Any(t => t.ContInIdTopic == st.ContInIdTopic)))
                {
                    var parentTopic = new ConceptDTO
                    {
                        TestseriesConceptIndexId = 0, // NA
                        ContInIdTopic = 0, // NA
                        SubjectId = subTopic.SubjectId,
                        ContentIndexId = 0, // NA
                        IndexTypeId = 2, // Topic
                        Status = true,
                        SubConcepts = new List<SubConceptDTO> { subTopic }
                    };

                    chapters.Add(new ChapterDTO
                    {
                        TestseriesContentIndexId = 0, // NA
                        SubjectId = subTopic.SubjectId,
                        ContentIndexId = 0, // NA
                        IndexTypeId = 1, // Chapter
                        Status = true,
                        Concepts = new List<ConceptDTO> { parentTopic }
                    });
                }

                return new ServiceResponse<List<ChapterDTO>>(true, "Test series content hierarchy fetched successfully.", chapters.ToList(), 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ChapterDTO>>(false, ex.Message, new List<ChapterDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<string>> UpdateQuestion(QuestionDTO request)
        {
            try
            {
                if (!string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode != "string")
                {
                    //request.QuestionCode = GenerateCode();
                    // Check for existing entries with the same QuestionCode and deactivate them
                    string deactivateQuery = @"
                UPDATE tblQuestion
                SET IsActive = 0
                WHERE QuestionCode = @QuestionCode AND IsActive = 1";

                    await _connection.ExecuteAsync(deactivateQuery, new { request.QuestionCode });

                }
                // Prepare new question entry
                string query = @"
                    UPDATE tblQuestions
                    SET 
                        QuestionDescription = @QuestionDescription,
                        QuestionTypeId = @QuestionTypeId,
                        Status = @Status,
                        CreatedBy = @CreatedBy,
                        CreatedOn = @CreatedOn,
                        ModifiedBy = @ModifiedBy,
                        ModifiedOn = @ModifiedOn,
                        subjectID = @SubjectID,
                        EmployeeId = @EmployeeId,
                        ModifierId = @ModifierId,
                        IndexTypeId = @IndexTypeId,
                        ContentIndexId = @ContentIndexId,
                        IsRejected = @IsRejected,
                        IsApproved = @IsApproved,
                        QuestionCode = @QuestionCode,
                        Explanation = @Explanation,
                        ExtraInformation = @ExtraInformation,
                        IsActive = @IsActive,
                        IsConfigure = @IsConfigure
                    WHERE 
                        QuestionCode = @QuestionCode";
                var parameters = new
                {
                    QuestionId = request.QuestionId,
                    QuestionDescription = request.QuestionDescription,
                    QuestionTypeId = request.QuestionTypeId,
                    Status = request.Status,
                    CreatedBy = request.CreatedBy,
                    CreatedOn = request.CreatedOn,
                    ModifiedBy = request.ModifiedBy,
                    ModifiedOn = request.ModifiedOn,
                    SubjectID = request.subjectID,
                    EmployeeId = request.EmployeeId,
                    IndexTypeId = request.IndexTypeId,
                    ContentIndexId = request.ContentIndexId,
                    IsRejected = request.IsRejected,
                    IsApproved = request.IsApproved,
                    QuestionCode = request.QuestionCode,
                    Explanation = request.Explanation,
                    ExtraInformation = request.ExtraInformation,
                    IsActive = request.IsActive,
                    IsConfigure = false
                };

                // Retrieve the QuestionCode after insertion
                // var insertedQuestionCode = await _connection.QuerySingleOrDefaultAsync<string>(insertQuery, question);
                var insertedQuestionId = await _connection.QuerySingleOrDefaultAsync<int>(query, parameters);

                string insertedQuestionCode = request.QuestionCode;

                if (!string.IsNullOrEmpty(insertedQuestionCode))
                {
                    // Handle QIDCourses mapping
                    var data = await AddUpdateQIDCourses(request.QIDCourses, request.QuestionId);

                    // Handle Answer mappings
                    string getQuesType = @"SELECT * FROM tblQBQuestionType WHERE QuestionTypeID = @QuestionTypeID;";
                    var questTypedata = await _connection.QueryFirstOrDefaultAsync<QuestionTypes>(getQuesType, new { QuestionTypeID = request.QuestionTypeId });

                    int answer = 0;
                    int Answerid = 0;

                    // Check if the answer already exists in AnswerMaster
                    string getAnswerQuery = @"SELECT Answerid FROM tblAnswerMaster WHERE QuestionCode = @QuestionCode;";
                    Answerid = await _connection.QueryFirstOrDefaultAsync<int>(getAnswerQuery, new { QuestionCode = insertedQuestionCode });

                    if (Answerid == 0)  // If no entry exists, insert a new one
                    {
                        string insertAnswerQuery = @"
        INSERT INTO [tblAnswerMaster] (Questionid, QuestionTypeid, QuestionCode)
        VALUES (@Questionid, @QuestionTypeid, @QuestionCode);
        SELECT CAST(SCOPE_IDENTITY() as int);";

                        Answerid = await _connection.QuerySingleAsync<int>(insertAnswerQuery, new
                        {
                            Questionid = 0, // Set to 0 or remove if QuestionId is not required
                            QuestionTypeid = questTypedata?.QuestionTypeID,
                            QuestionCode = insertedQuestionCode
                        });
                    }

                    // If the question type supports multiple-choice or similar categories
                    if (questTypedata != null)
                    {
                        if (questTypedata.Code.Trim() == "MCQ" || questTypedata.Code.Trim() == "TF" || questTypedata.Code.Trim() == "MT1" ||
                            questTypedata.Code.Trim() == "MAQ" || questTypedata.Code.Trim() == "MT2" || questTypedata.Code.Trim() == "AR" || questTypedata.Code.Trim() == "C")
                        {
                            if (request.AnswerMultipleChoiceCategories != null)
                            {
                                // First, delete existing multiple-choice entries if present
                                string deleteMCQQuery = @"DELETE FROM tblAnswerMultipleChoiceCategory WHERE Answerid = @Answerid;";
                                await _connection.ExecuteAsync(deleteMCQQuery, new { Answerid });

                                // Insert new multiple-choice answers
                                foreach (var item in request.AnswerMultipleChoiceCategories)
                                {
                                    item.Answerid = Answerid;
                                }
                                string insertMCQQuery = @"
                    INSERT INTO tblAnswerMultipleChoiceCategory
                    (Answerid, Answer, Iscorrect, Matchid) 
                    VALUES (@Answerid, @Answer, @Iscorrect, @Matchid);";
                                answer = await _connection.ExecuteAsync(insertMCQQuery, request.AnswerMultipleChoiceCategories);
                            }
                        }
                        else  // Handle single-answer category
                        {
                            string sql = @"
                INSERT INTO tblAnswersingleanswercategory (Answerid, Answer)
                VALUES (@Answerid, @Answer);";

                            if (request.Answersingleanswercategories != null)
                            {
                                // First, delete existing single-answer entries if present
                                string deleteSingleQuery = @"DELETE FROM tblAnswersingleanswercategory WHERE Answerid = @Answerid;";
                                await _connection.ExecuteAsync(deleteSingleQuery, new { Answerid });

                                // Insert new single-answer answers
                                request.Answersingleanswercategories.Answerid = Answerid;
                                answer = await _connection.ExecuteAsync(sql, request.Answersingleanswercategories);
                            }
                        }
                    }

                    if (data > 0 && answer > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Question Added Successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                    }
                }
                else
                {
                    return new ServiceResponse<string>(false, "Some error occurred", string.Empty, 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<byte[]>> GenerateExcelFile(DownExcelRequest request)
        {
            try
            {

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Create an ExcelPackage
                using (var package = new ExcelPackage())
                {
                    // Fetch the list of subject IDs, IndexTypeIds, and ContentIndexIds based on the provided TestSeriesId
                    var testSeriesContentQuery = @"
                SELECT distinct SubjectId 
                FROM [tblTestSeriesSubjects] 
                WHERE TestSeriesID = @TestSeriesId";

                    var testSeriesContent = await _connection.QueryAsync<int>(testSeriesContentQuery, new { TestSeriesId = request.TestSeriesId });

                    if (testSeriesContent == null || !testSeriesContent.Any())
                    {
                        return new ServiceResponse<byte[]>(false, "", [], 500);
                    }
                    // Fetch the section data, including question type and difficulty levels
                    var sectionDataQuery = @"
                SELECT [SubjectId], [QuestionTypeID], [LevelID1], [QuesPerDifficulty1], [LevelID2], 
                       [QuesPerDifficulty2], [LevelID3], [QuesPerDifficulty3]
                FROM [tbltestseriesQuestionSection]
                WHERE TestSeriesId = @TestSeriesId";

                    var sectionData = await _connection.QueryAsync<dynamic>(sectionDataQuery, new { TestSeriesId = request.TestSeriesId });

                    if (sectionData == null || !sectionData.Any())
                    {
                        return new ServiceResponse<byte[]>(false, "No section data found", [], 500);
                    }
                    // Fetch test series details from tblTestSeries
                    var testSeriesQuery = @"
                SELECT ts.TestSeriesId, ts.TotalNoOfQuestions
                FROM [tblTestSeries] ts
                WHERE ts.TestSeriesId = @TestSeriesId";

                    var testSeriesDetails = await _connection.QuerySingleOrDefaultAsync<dynamic>(testSeriesQuery, new { TestSeriesId = request.TestSeriesId });

                    if (testSeriesDetails == null)
                    {
                        return new ServiceResponse<byte[]>(false, "Test Series not found", null, 404);
                    }
                    // Create a worksheet for Questions
                    var worksheet = package.Workbook.Worksheets.Add("Questions");
                    
                    // Add static headers
                    worksheet.Cells[1, 1].Value = "Exam Paper ID";
                    worksheet.Cells[1, 2].Value = "Subject ID";
                    worksheet.Cells[1, 3].Value = "Question Type";
                    worksheet.Cells[1, 4].Value = "Difficulty Level";
                    worksheet.Cells[1, 6].Value = "Question";
                    worksheet.Cells[1, 7].Value = "Answer";

                    // Add headers for options and other details
                    for (int i = 1; i <= 4; i++)
                    {
                        worksheet.Cells[1, 7 + i].Value = $"Option{i}";
                    }

                    worksheet.Cells[1, 12].Value = "Explanation";
                    worksheet.Cells[1, 13].Value = "Extra Information";
                    worksheet.Cells[1, 5].Value = "Display Order";
                    
                    // Format headers
                    using (var range = worksheet.Cells[1, 1, 1, 27])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }

                    // Loop through the questions and add rows
                    int row = 2;
                    int displayOrder = 1; // To increment display order

                    foreach (var data in sectionData)
                    {
                        // Repeat the row for each difficulty level (Level 1, 2, and 3)
                        for(int i = 0;i< data.QuesPerDifficulty1; i++)
                        {
                            // 1st Row for Difficulty Level 1
                            worksheet.Cells[row, 1].Value = request.TestSeriesId; // Exam Paper ID
                            worksheet.Cells[row, 2].Value = data.SubjectId; // Subject ID
                            worksheet.Cells[row, 3].Value = data.QuestionTypeID; // Question Type
                            worksheet.Cells[row, 4].Value = data.LevelID1; // Difficulty Level 1
                            worksheet.Cells[row, 6].Value = "Q"; // Question
                            worksheet.Cells[row, 7].Value = "A"; // Answer

                            FillOptionsBasedOnQuestionType(data.QuestionTypeID, worksheet, row);

                            // Fill explanation, extra information, and display order
                            worksheet.Cells[row, 12].Value = "Explanation"; // Dummy explanation
                            worksheet.Cells[row, 13].Value = "Extra Info"; // Dummy extra information
                            worksheet.Cells[row, 5].Value = displayOrder++; // Display order
                            row++; // Move to next row
                        }

                        for (int i = 0; i < data.QuesPerDifficulty2; i++)
                        {
                            // 2nd Row for Difficulty Level 2
                            worksheet.Cells[row, 1].Value = request.TestSeriesId; // Exam Paper ID
                            worksheet.Cells[row, 2].Value = data.SubjectId; // Subject ID
                            worksheet.Cells[row, 3].Value = data.QuestionTypeID; // Question Type
                            worksheet.Cells[row, 4].Value = data.LevelID2; // Difficulty Level 2
                            worksheet.Cells[row, 6].Value = "Q"; // Question
                            worksheet.Cells[row, 7].Value = "A"; // Answer

                            FillOptionsBasedOnQuestionType(data.QuestionTypeID, worksheet, row);

                            // Fill explanation, extra information, and display order
                            worksheet.Cells[row, 12].Value = "Explanation"; // Dummy explanation
                            worksheet.Cells[row, 13].Value = "Extra Info"; // Dummy extra information
                            worksheet.Cells[row, 5].Value = displayOrder++; // Display order
                            row++; // Move to next row
                        }

                        for (int i = 0; i < data.QuesPerDifficulty3; i++)
                        {
                            // 3rd Row for Difficulty Level 3
                            worksheet.Cells[row, 1].Value = request.TestSeriesId; // Exam Paper ID
                            worksheet.Cells[row, 2].Value = data.SubjectId; // Subject ID
                            worksheet.Cells[row, 3].Value = data.QuestionTypeID; // Question Type
                            worksheet.Cells[row, 4].Value = data.LevelID3; // Difficulty Level 3
                            worksheet.Cells[row, 6].Value = "Q"; // Question
                            worksheet.Cells[row, 7].Value = "A"; // Answer

                            FillOptionsBasedOnQuestionType(data.QuestionTypeID, worksheet, row);

                            // Fill explanation, extra information, and display order
                            worksheet.Cells[row, 12].Value = "Explanation"; // Dummy explanation
                            worksheet.Cells[row, 13].Value = "Extra Info"; // Dummy extra information
                            worksheet.Cells[row, 5].Value = displayOrder++; // Display order
                            row++; // Move to next row
                        }
                    }

                    // Auto fit columns for better readability
                    worksheet.Cells.AutoFitColumns();
                    // Protect the worksheet without setting a password
                    worksheet.Protection.IsProtected = true;

                    // Unlock the columns that should be editable (from column 7 onward)
                    for (int col = 6; col <= worksheet.Dimension.End.Column; col++) // Start from column 7 onwards
                    {
                        worksheet.Column(col).Style.Locked = false;
                    }
                    AddMasterDataSheets(package, testSeriesContent.ToList());
                    var fileBytes = package.GetAsByteArray();
                    // Return the file as a response
                    return new ServiceResponse<byte[]>(true, "Excel file generated successfully", fileBytes, 200);
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                return new ServiceResponse<byte[]>(false, ex.Message, [], 500);
            }
        }
        private void FillOptionsBasedOnQuestionType(int questionTypeId, ExcelWorksheet worksheet, int row)
        {
            switch (questionTypeId)
            {
                case 1: // Multiple choice (4 columns)
                case 4: // Fill in the blanks (4 columns)
                case 5: // Matching (4 columns)
                case 6: // Multiple answers (4 columns)
                case 9: // Matching 2 (4 columns)
                    worksheet.Cells[row, 8].Value = "A"; // Option 1
                    worksheet.Cells[row, 9].Value = "B"; // Option 2
                    worksheet.Cells[row, 10].Value = "C"; // Option 3
                    worksheet.Cells[row, 11].Value = "D"; // Option 4
                    break;

                case 2: // True/False (2 columns)
                    worksheet.Cells[row, 8].Value = "A";  // Option 1
                    worksheet.Cells[row, 9].Value = "B"; // Option 2
                    break;

                case 3: // Short Answer (1 column)
                case 7: // Long Answer (1 column)
                case 8: // Very Short Answer (1 column)
                case 10: // Assertion and Reason (1 column)
                case 11: // Numerical (1 column)
                case 12: // Comprehensive (1 column)
                    worksheet.Cells[row, 8].Value = "A"; // Option 1 only
                    break;

                default:
                    // Default case if there is an unexpected question type.
                    worksheet.Cells[row, 8].Value = "A"; // Option 1
                    worksheet.Cells[row, 9].Value = "B"; // Option 2
                    worksheet.Cells[row, 10].Value = "C"; // Option 3
                    worksheet.Cells[row, 11].Value = "D"; // Option 4
                    break;
            }
        }
        public async Task<ServiceResponse<string>> UploadQuestionsFromExcel(IFormFile file, int testSeriesId, int sectionId)
        {
            var quesionsList = new List<int>();
            List<TestSeriesQuestions> testSeriesQuestionsList = new List<TestSeriesQuestions>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var questions = new List<QuestionDTO>();
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using (var package = new ExcelPackage(stream))
                {
                    // Process main worksheet for questions
                    var worksheet = package.Workbook.Worksheets["Questions"];
                    var rowCount = worksheet.Dimension.Rows;
                  

                    for (int row = 2; row <= rowCount; row++) // Skip header row
                    {
                        var qidCourses = new List<QIDCourse>
                        {
                            new QIDCourse
                            {
                                QIDCourseID = 0, // Assuming you want to set this later or handle it in the AddUpdateQuestion method
                                QID = 0, // Populate this as needed
                                QuestionCode = string.IsNullOrEmpty(worksheet.Cells[row, 27].Text) ? null : worksheet.Cells[row, 27].Text,
                                CourseID = 0,
                                LevelId = int.Parse(worksheet.Cells[row, 4].Text), // Set this based on your logic or fetch from another source
                                Status = true, // Set as needed
                                CreatedBy = "YourUsername", // Set the creator's username or similar info
                                CreatedDate = DateTime.UtcNow, // Use the current date and time
                                ModifiedBy = "YourUsername", // Set as needed
                                ModifiedDate = DateTime.UtcNow // Set as needed
                            }
                        };
                        // Create the question DTO
                        var question = new QuestionDTO
                        {
                            QuestionDescription = worksheet.Cells[row, 6].Text,
                            QuestionTypeId = int.Parse( worksheet.Cells[row, 3].Text),
                            subjectID = int.Parse(worksheet.Cells[row, 2].Text),
                            IndexTypeId = 0,
                            Explanation = string.IsNullOrEmpty(worksheet.Cells[row, 12].Text) ? null : worksheet.Cells[row, 12].Text,
                            QuestionCode = string.IsNullOrEmpty(worksheet.Cells[row, 27].Text) ? null : worksheet.Cells[row, 27].Text,
                            ContentIndexId = 0,
                            AnswerMultipleChoiceCategories = GetAnswerMultipleChoiceCategories(worksheet, row),
                            Answersingleanswercategories = GetAnswerSingleAnswerCategories(worksheet, row, int.Parse(worksheet.Cells[row, 3].Text)),
                            QIDCourses = qidCourses,
                            IsActive = true,
                            IsConfigure = false
                        };

                        // Add question to the list for bulk processing
                        questions.Add(question);

                        // Call AddUpdateQuestion for each question
                        var response = await AddUpdateQuestion(question);
                        quesionsList.Add(response.Data);
                        if (!response.Success)
                        {
                            return new ServiceResponse<string>(false, $"Failed to add/update question at row {row}: {response.Message}", string.Empty, 500);
                        }
                    }
                }
            }
            foreach (int questionId in quesionsList)
            {
                TestSeriesQuestions testSeriesQuestion = new TestSeriesQuestions
                {
                    TestSeriesid = testSeriesId,
                    testseriesQuestionSectionid = sectionId,
                    QuestionCode = "",
                    Questionid = questionId,
                    Status = 1 // Assuming status is active or some default value
                };
                testSeriesQuestionsList.Add(testSeriesQuestion);
            }
            var quesMapping = await TestSeriesQuestionsMapping(testSeriesQuestionsList, testSeriesId, sectionId);
            return new ServiceResponse<string>(true, "All questions uploaded successfully.", "Data uploaded successfully.", 200);
        }
        public async Task<ServiceResponse<int>> AddUpdateQuestion(QuestionDTO request)
        {
            try
            {
                if (!string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode != "string")
                {
                    //request.QuestionCode = GenerateCode();
                    // Check for existing entries with the same QuestionCode and deactivate them
                    string deactivateQuery = @"
                UPDATE tblQuestion
                SET IsActive = 0
                WHERE QuestionCode = @QuestionCode AND IsActive = 1";

                    await _connection.ExecuteAsync(deactivateQuery, new { request.QuestionCode });

                }
                // Prepare new question entry
                var question = new Question
                {
                    QuestionDescription = request.QuestionDescription,
                    QuestionTypeId = request.QuestionTypeId,
                    Status = true,
                    CreatedBy = request.CreatedBy,
                    CreatedOn = DateTime.Now,
                    subjectID = request.subjectID,
                    ContentIndexId = request.ContentIndexId,
                    EmployeeId = request.EmployeeId,
                    IndexTypeId = request.IndexTypeId,
                    IsApproved = false,
                    IsRejected = false,
                    QuestionCode = request.QuestionCode,
                    Explanation = request.Explanation,
                    ExtraInformation = request.ExtraInformation,
                    IsActive = true,
                    IsConfigure = false
                };
                string insertQuery = @"
              INSERT INTO tblQuestion (
                  QuestionDescription,
                  QuestionTypeId,
                  Status,
                  CreatedBy,
                  CreatedOn,
                  subjectID,
                  EmployeeId,
                  IndexTypeId,
                  ContentIndexId,
                  IsRejected,
                  IsApproved,
                  QuestionCode,
                  Explanation,
                  ExtraInformation,
                  IsActive,
                  IsConfigure
              ) VALUES (
                  @QuestionDescription,
                  @QuestionTypeId,
                  @Status,
                  @CreatedBy,
                  @CreatedOn,
                  @subjectID,
                  @EmployeeId,
                  @IndexTypeId,
                  @ContentIndexId,
                  @IsRejected,
                  @IsApproved,
                  @QuestionCode,
                  @Explanation,
                  @ExtraInformation,
                  @IsActive, @IsConfigure
              );
  
              -- Fetch the QuestionId of the newly inserted row
              SELECT CAST(SCOPE_IDENTITY() AS INT);";

                // Retrieve the QuestionCode after insertion
                // var insertedQuestionCode = await _connection.QuerySingleOrDefaultAsync<string>(insertQuery, question);
                var insertedQuestionId = await _connection.QuerySingleOrDefaultAsync<int>(insertQuery, question);
                string code = string.Empty;
                if (string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string")
                {
                    code = GenerateQuestionCode(request.IndexTypeId, request.ContentIndexId, insertedQuestionId);

                    string questionCodeQuery = @"
                UPDATE tblQuestion
                SET QuestionCode = @QuestionCode
                WHERE QuestionId = @QuestionId AND IsActive = 1";

                    await _connection.ExecuteAsync(questionCodeQuery, new { QuestionCode = code, QuestionId = insertedQuestionId });
                }
                string insertedQuestionCode = string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string" ? code : request.QuestionCode;

                if (string.IsNullOrEmpty(insertedQuestionCode))
                {
                    // Handle QIDCourses mapping
                    var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionId);

                    // Handle Answer mappings
                    string getQuesType = @"SELECT * FROM tblQBQuestionType WHERE QuestionTypeID = @QuestionTypeID;";
                    var questTypedata = await _connection.QueryFirstOrDefaultAsync<QuestionTypes>(getQuesType, new { QuestionTypeID = request.QuestionTypeId });

                    int answer = 0;
                    int Answerid = 0;

                    // Check if the answer already exists in AnswerMaster
                    string getAnswerQuery = @"SELECT Answerid FROM tblAnswerMaster WHERE Questionid = @Questionid;";
                    Answerid = await _connection.QueryFirstOrDefaultAsync<int>(getAnswerQuery, new { Questionid = insertedQuestionId });

                    if (Answerid == 0)  // If no entry exists, insert a new one
                    {
                        string insertAnswerQuery = @"
        INSERT INTO [tblAnswerMaster] (Questionid, QuestionTypeid, QuestionCode)
        VALUES (@Questionid, @QuestionTypeid, @QuestionCode);
        SELECT CAST(SCOPE_IDENTITY() as int);";

                        Answerid = await _connection.QuerySingleAsync<int>(insertAnswerQuery, new
                        {
                            Questionid = insertedQuestionId, // Set to 0 or remove if QuestionId is not required
                            QuestionTypeid = questTypedata?.QuestionTypeID,
                            QuestionCode = insertedQuestionCode
                        });
                    }

                    // If the question type supports multiple-choice or similar categories
                    if (questTypedata != null)
                    {
                        if (questTypedata.Code.Trim() == "MCQ" || questTypedata.Code.Trim() == "TF" || questTypedata.Code.Trim() == "MT1" || questTypedata.Code.Trim() == "FB" || questTypedata.Code.Trim() == "MT" ||
                            questTypedata.Code.Trim() == "MAQ" || questTypedata.Code.Trim() == "MT2" || questTypedata.Code.Trim() == "AR" || questTypedata.Code.Trim() == "C")
                        {
                            if (request.AnswerMultipleChoiceCategories != null)
                            {
                                // First, delete existing multiple-choice entries if present
                                string deleteMCQQuery = @"DELETE FROM tblAnswerMultipleChoiceCategory WHERE Answerid = @Answerid;";
                                await _connection.ExecuteAsync(deleteMCQQuery, new { Answerid });

                                // Insert new multiple-choice answers
                                foreach (var item in request.AnswerMultipleChoiceCategories)
                                {
                                    item.Answerid = Answerid;
                                }
                                string insertMCQQuery = @"
                    INSERT INTO tblAnswerMultipleChoiceCategory
                    (Answerid, Answer, Iscorrect, Matchid) 
                    VALUES (@Answerid, @Answer, @Iscorrect, @Matchid);";
                                answer = await _connection.ExecuteAsync(insertMCQQuery, request.AnswerMultipleChoiceCategories);
                            }
                        }
                        else  // Handle single-answer category
                        {
                            string sql = @"
                INSERT INTO tblAnswersingleanswercategory (Answerid, Answer)
                VALUES (@Answerid, @Answer);";

                            if (request.Answersingleanswercategories != null)
                            {
                                // First, delete existing single-answer entries if present
                                string deleteSingleQuery = @"DELETE FROM tblAnswersingleanswercategory WHERE Answerid = @Answerid;";
                                await _connection.ExecuteAsync(deleteSingleQuery, new { Answerid });

                                // Insert new single-answer answers
                                request.Answersingleanswercategories.Answerid = Answerid;
                                answer = await _connection.ExecuteAsync(sql, request.Answersingleanswercategories);
                            }
                        }
                    }

                    if (data > 0 && answer > 0)
                    {
                        return new ServiceResponse<int>(true, "Operation Successful", insertedQuestionId, 200);
                    }
                    else
                    {
                        return new ServiceResponse<int>(false, "Operation Failed", 0, 500);
                    }
                }
                else
                {
                    return new ServiceResponse<int>(false, "Some error occurred", 0, 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<int>(false, ex.Message, 0, 500);
            }
        }
        //get records
        private List<DTOs.Requests.AnswerMultipleChoiceCategory> GetAnswerMultipleChoiceCategories(ExcelWorksheet worksheet, int row)
        {
            var categories = new List<DTOs.Requests.AnswerMultipleChoiceCategory>();

            // Get the correct answer from cell 9
            var correctAnswer = worksheet.Cells[row, 7].Text; // Correct answer

            // Find the column with the header "Explanation" and stop before that
            int optionStartColumn = 8; // Assuming the options start from column 10
            int explanationColumn = -1;

            // Iterate through columns starting from optionStartColumn to find "Explanation"
            for (int col = optionStartColumn; col <= worksheet.Dimension.End.Column; col++)
            {
                var headerText = worksheet.Cells[1, col].Text; // Assuming headers are in the first row
                if (headerText.Equals("Explanation", StringComparison.OrdinalIgnoreCase))
                {
                    explanationColumn = col;
                    break;
                }
            }

            // If "Explanation" is found, loop through the columns before it
            if (explanationColumn > optionStartColumn)
            {
                for (int i = optionStartColumn; i < explanationColumn; i++)
                {
                    var answer = worksheet.Cells[row, i].Text; // Answer option
                    bool isCorrect = answer.Equals(correctAnswer, StringComparison.OrdinalIgnoreCase); // Check if the answer matches the correct answer

                    categories.Add(new DTOs.Requests.AnswerMultipleChoiceCategory
                    {
                        Answer = answer,
                        Iscorrect = isCorrect, // Set isCorrect based on the comparison
                        Matchid = 0 // Assuming MatchId is still 0
                    });
                }
            }

            return categories;
        }

        private DTOs.Requests.Answersingleanswercategory GetAnswerSingleAnswerCategories(ExcelWorksheet worksheet, int row, int questionTypeId)
        {
            if (questionTypeId == 7 || questionTypeId == 8 || questionTypeId == 10 || questionTypeId == 11 || questionTypeId == 12)
            {
                var answer = worksheet.Cells[row, 7].Text; // Single answer category in column 15

                return new DTOs.Requests.Answersingleanswercategory
                {
                    Answer = answer
                };
            }
            else
            {
                return null;
            }
        }
        public string GenerateQuestionCode(int indexTypeId, int contentId, int questionId)
        {
            string questionCode = "";
            int subjectId = 0;
            int chapterId = 0;
            int topicId = 0;
            int subTopicId = 0;

            // Fetch subject ID and related hierarchy based on indexTypeId
            if (indexTypeId == 1)  // Chapter
            {
                // Fetch subject directly from chapter
                var chapter = _connection.QueryFirstOrDefault("SELECT SubjectId, ContentIndexId FROM tblContentIndexChapters WHERE ContentIndexId = @contentId", new { contentId });
                if (chapter != null)
                {
                    subjectId = chapter.SubjectId;
                    chapterId = chapter.ContentIndexId;
                }
            }
            else if (indexTypeId == 2)  // Topic
            {
                // Fetch parent chapter from topic, then get subject from the chapter
                var topic = _connection.QueryFirstOrDefault("SELECT ContentIndexId, ContInIdTopic FROM tblContentIndexTopics WHERE ContInIdTopic = @contentId", new { contentId });
                if (topic != null)
                {
                    topicId = topic.ContInIdTopic;
                    chapterId = topic.ContentIndexId;

                    // Now fetch the subject from the parent chapter
                    var chapter = _connection.QueryFirstOrDefault("SELECT SubjectId FROM tblContentIndexChapters WHERE ContentIndexId = @chapterId", new { chapterId });
                    if (chapter != null)
                    {
                        subjectId = chapter.SubjectId;
                    }
                }
            }
            else if (indexTypeId == 3)  // SubTopic
            {
                // Fetch parent topic from subtopic, then get the chapter, and then the subject
                var subTopic = _connection.QueryFirstOrDefault("SELECT ContInIdTopic, ContInIdSubTopic FROM tblContentIndexSubTopics WHERE ContInIdSubTopic = @contentId", new { contentId });
                if (subTopic != null)
                {
                    subTopicId = subTopic.ContInIdSubTopic;
                    topicId = subTopic.ContInIdTopic;

                    // Now fetch the chapter from the parent topic
                    var topic = _connection.QueryFirstOrDefault("SELECT ContentIndexId FROM tblContentIndexTopics WHERE ContInIdTopic = @topicId", new { topicId });
                    if (topic != null)
                    {
                        chapterId = topic.ContentIndexId;

                        // Now fetch the subject from the parent chapter
                        var chapter = _connection.QueryFirstOrDefault("SELECT SubjectId FROM tblContentIndexChapters WHERE ContentIndexId = @chapterId", new { chapterId });
                        if (chapter != null)
                        {
                            subjectId = chapter.SubjectId;
                        }
                    }
                }
            }
            // Construct the question code based on IndexTypeId and IDs
            if (indexTypeId == 1)  // Chapter
            {
                questionCode = $"S{subjectId}C{chapterId}Q{questionId}";
            }
            else if (indexTypeId == 2)  // Topic
            {
                questionCode = $"S{subjectId}C{chapterId}T{topicId}Q{questionId}";
            }
            else if (indexTypeId == 3)  // SubTopic
            {
                questionCode = $"S{subjectId}C{chapterId}T{topicId}ST{subTopicId}Q{questionId}";
            }

            return questionCode;
        }
        private void LoadSubjectCodes(ExcelWorksheet sheet, Dictionary<string, int> dictionary)
        {
            int rowCount = sheet.Dimension.Rows;
            var query = "SELECT SubjectId FROM tblSubject WHERE [SubjectName] = @subjectName";
            for (int row = 2; row <= rowCount; row++) // Assuming the first row contains headers
            {
                var subjectName = sheet.Cells[row, 1].Text; // Assuming subject codes are in the second column
                var subjectId = _connection.QuerySingleOrDefault<int>(query, new { subjectName = subjectName });

                if (!string.IsNullOrEmpty(subjectName) && !dictionary.ContainsKey(subjectName))
                {
                    dictionary.Add(subjectName, subjectId);
                }
            }
        }
        private void LoadCourseCodes(ExcelWorksheet sheet, Dictionary<string, int> dictionary)
        {
            int rowCount = sheet.Dimension.Rows;
            var query = "SELECT CourseId FROM tblCourse WHERE [CourseName] = @CourseName";
            for (int row = 2; row <= rowCount; row++) // Assuming the first row contains headers
            {
                var courseName = sheet.Cells[row, 1].Text; // Assuming subject codes are in the second column
                var courseId = _connection.QuerySingleOrDefault<int>(query, new { CourseName = courseName });

                if (!string.IsNullOrEmpty(courseName) && !dictionary.ContainsKey(courseName))
                {
                    dictionary.Add(courseName, courseId);
                }
            }
        }
        // Helper method to load master data from a sheet into a dictionary
        private void LoadMasterData(ExcelWorksheet sheet, Dictionary<string, int> dictionary, bool isIdInFirstColumn)
        {
            int rowCount = sheet.Dimension.Rows;

            for (int row = 2; row <= rowCount; row++) // Assuming the first row contains headers
            {
                var idColumn = isIdInFirstColumn ? 1 : 2; // Determine where the ID is based on the flag
                var nameColumn = isIdInFirstColumn ? 2 : 3; // The name will be in the other column

                var id = int.Parse(sheet.Cells[row, idColumn].Text); // Get ID from the appropriate column
                var name = sheet.Cells[row, nameColumn].Text; // Get name from the appropriate column

                if (!string.IsNullOrEmpty(name) && !dictionary.ContainsKey(name))
                {
                    dictionary.Add(name, id);
                }
            }
        }
        private DTOs.Response.Answersingleanswercategory GetSingleAnswer(string QuestionCode)
        {
            var answerMaster = _connection.QueryFirstOrDefault<AnswerMaster>(@"
        SELECT * FROM tblAnswerMaster WHERE QuestionCode = @QuestionCode", new { QuestionCode });

            if (answerMaster != null)
            {
                string getQuery = @"
            SELECT * FROM [tblAnswersingleanswercategory] WHERE [Answerid] = @Answerid";

                var response = _connection.QueryFirstOrDefault<DTOs.Response.Answersingleanswercategory>(getQuery, new { answerMaster.Answerid });
                return response ?? new DTOs.Response.Answersingleanswercategory();
            }
            else
            {
                return new DTOs.Response.Answersingleanswercategory();
            }
        }
        private IEnumerable<Questiontype> GetQuestionTypes()
        {
            var query = "SELECT [QuestionTypeID], [QuestionType], [Code], [Status], [MinNoOfOptions], [modifiedon], [modifiedby], [createdon], [createdby], [EmployeeID], [EmpFirstName], [TypeOfOption], [Question] FROM [tblQBQuestionType]";
            return _connection.Query<Questiontype>(query);
        }
        private async Task<IEnumerable<Question>> GetQuestionsData(int subjectId, int indexTypeId, int contentId)
        {
            // Example query to fetch questions from the database
            var query = @"SELECT * FROM tblQuestion 
                  WHERE subjectID = @SubjectId 
                  AND IndexTypeId = @IndexTypeId 
                  AND [ContentIndexId] = @ContentId AND IsActive = 1";
            var result = await _connection.QueryAsync<Question>(query, new { SubjectId = subjectId, IndexTypeId = indexTypeId, ContentId = contentId });
            var resposne = result.Select(item => new Question
            {
                QuestionId = item.QuestionId,
                ContentIndexId = item.ContentIndexId,
                CreatedBy = item.CreatedBy,
                CreatedOn = item.CreatedOn,
                EmployeeId = item.EmployeeId,
                Explanation = item.Explanation,
                ExtraInformation = item.ExtraInformation,
                IndexTypeId = item.IndexTypeId,
                IsActive = item.IsActive,
                IsApproved = item.IsApproved,
                IsRejected = item.IsRejected,
                ModifiedBy = item.ModifiedBy,
                ModifiedOn = item.ModifiedOn,
                QuestionCode = item.QuestionCode,
                QuestionDescription = item.QuestionDescription,
                QuestionTypeId = item.QuestionTypeId,
                Status = item.Status,
                subjectID = item.subjectID
            });

            return resposne;
        }
        private IEnumerable<SubjectData> GetSubjects()
        {
            var query = "SELECT * FROM tblSubject WHERE Status = 1";
            var result = _connection.Query<dynamic>(query);
            var resposne = result.Select(item => new SubjectData
            {
                SubjectCode = item.SubjectCode,
                SubjectId = item.SubjectId,
                SubjectName = item.SubjectName,
                SubjectType = item.SubjectType
            });
            return resposne;
        }
        private IEnumerable<Chapters> GetChapters(int subjectId)
        {
            var query = "SELECT * FROM tblContentIndexChapters WHERE IndexTypeId = 1 AND IsActive = 1 AND SubjectId = @subjectId";
            return _connection.Query<Chapters>(query, new { subjectId });
        }
        private IEnumerable<Topics> GetTopics(int chapterId)
        {
            var query = "SELECT * FROM tblContentIndexTopics WHERE IndexTypeId = 2 AND IsActive = 1 AND ContentIndexId = @chapterId";
            return _connection.Query<Topics>(query, new { chapterId });
        }
        private IEnumerable<SubTopic> GetSubTopics(int TopicId)
        {
            var query = "SELECT * FROM tblContentIndexSubTopics WHERE IndexTypeId = 3 AND IsActive = 1 AND ContInIdTopic = @TopicId";
            return _connection.Query<SubTopic>(query, new { TopicId });
        }
        private IEnumerable<DifficultyLevel> GetDifficultyLevels()
        {
            var query = "SELECT [LevelId], [LevelName], [LevelCode], [Status], [NoofQperLevel], [SuccessRate], [createdon], [patterncode], [modifiedon], [modifiedby], [createdby], [EmployeeID], [EmpFirstName] FROM [tbldifficultylevel]";
            return _connection.Query<DifficultyLevel>(query);
        }
        private IEnumerable<Course> GetCourses()
        {
            var query = "SELECT CourseName, CourseCode FROM [tblCourse]";
            return _connection.Query<Course>(query);
        }
        private void AddMasterDataSheets(ExcelPackage package, List<int> subjectIds)
        {
            // Create worksheets for master data
            var subjectWorksheet = package.Workbook.Worksheets.Add("Subjects");
          //  var chapterWorksheet = package.Workbook.Worksheets.Add("Chapters");
          //  var topicWorksheet = package.Workbook.Worksheets.Add("Topics");
          //  var subTopicWorksheet = package.Workbook.Worksheets.Add("SubTopics");
            var difficultyLevelWorksheet = package.Workbook.Worksheets.Add("Difficulty Levels");
            var questionTypeWorksheet = package.Workbook.Worksheets.Add("Question Types");
         //   var coursesWorksheet = package.Workbook.Worksheets.Add("Courses");

            // Set headers for each worksheet
            subjectWorksheet.Cells[1, 1].Value = "SubjectName";
            subjectWorksheet.Cells[1, 2].Value = "SubjectCode";

            //chapterWorksheet.Cells[1, 1].Value = "SubjectId";
            //chapterWorksheet.Cells[1, 2].Value = "ContentIndexId";
            //chapterWorksheet.Cells[1, 3].Value = "ContentName_Chapter";

            //topicWorksheet.Cells[1, 1].Value = "ChapterId";
            //topicWorksheet.Cells[1, 2].Value = "ContInIdTopic";
            //topicWorksheet.Cells[1, 3].Value = "ContentName_Topic";

            //subTopicWorksheet.Cells[1, 1].Value = "TopicId";
            //subTopicWorksheet.Cells[1, 2].Value = "ContInIdSubTopic";
            //subTopicWorksheet.Cells[1, 3].Value = "ContentName_SubTopic";

            // Initialize row counters
            int subjectRow = 2, chapterRow = 2, topicRow = 2, subTopicRow = 2;

            // Loop through each subjectId and populate data for Subjects, Chapters, Topics, and SubTopics
            foreach (var subjectId in subjectIds)
            {
                // Fetch subjects based on the current subjectId
                var subjects = GetSubjects().Where(s => s.SubjectId == subjectId);
                foreach (var subject in subjects)
                {
                    subjectWorksheet.Cells[subjectRow, 1].Value = subject.SubjectName;
                    subjectWorksheet.Cells[subjectRow, 2].Value = subject.SubjectCode;
                    subjectRow++;
                }

                // Fetch chapters based on the current subjectId
                var chapters = GetChapters(subjectId);
                //foreach (var chapter in chapters)
                //{
                //    chapterWorksheet.Cells[chapterRow, 1].Value = chapter.SubjectId;
                //    chapterWorksheet.Cells[chapterRow, 2].Value = chapter.ContentIndexId;
                //    chapterWorksheet.Cells[chapterRow, 3].Value = chapter.ContentName_Chapter;
                //    chapterRow++;

                //    // Fetch topics for each chapter
                //    var topics = GetTopics(chapter.ContentIndexId);
                //    foreach (var topic in topics)
                //    {
                //        topicWorksheet.Cells[topicRow, 1].Value = chapter.ContentIndexId;
                //        topicWorksheet.Cells[topicRow, 2].Value = topic.ContInIdTopic;
                //        topicWorksheet.Cells[topicRow, 3].Value = topic.ContentName_Topic;
                //        topicRow++;

                //        // Fetch subtopics for each topic
                //        var subTopics = GetSubTopics(topic.ContInIdTopic);
                //        foreach (var subTopic in subTopics)
                //        {
                //            subTopicWorksheet.Cells[subTopicRow, 1].Value = topic.ContInIdTopic;
                //            subTopicWorksheet.Cells[subTopicRow, 2].Value = subTopic.ContInIdSubTopic;
                //            subTopicWorksheet.Cells[subTopicRow, 3].Value = subTopic.ContentName_SubTopic;
                //            subTopicRow++;
                //        }
                //    }
                //}
            }

            // Populate data for Difficulty Levels
            difficultyLevelWorksheet.Cells[1, 1].Value = "LevelId";
            difficultyLevelWorksheet.Cells[1, 2].Value = "LevelName";
            difficultyLevelWorksheet.Cells[1, 3].Value = "LevelCode";

            var difficultyLevels = GetDifficultyLevels();
            int levelRow = 2;
            foreach (var level in difficultyLevels)
            {
                difficultyLevelWorksheet.Cells[levelRow, 1].Value = level.LevelId;
                difficultyLevelWorksheet.Cells[levelRow, 2].Value = level.LevelName;
                difficultyLevelWorksheet.Cells[levelRow, 3].Value = level.LevelCode;
                levelRow++;
            }

            // Populate data for Question Types
            questionTypeWorksheet.Cells[1, 1].Value = "QuestionTypeID";
            questionTypeWorksheet.Cells[1, 2].Value = "QuestionType";

            var questionTypes = GetQuestionTypes();
            int typeRow = 2;
            foreach (var type in questionTypes)
            {
                questionTypeWorksheet.Cells[typeRow, 1].Value = type.QuestionTypeID;
                questionTypeWorksheet.Cells[typeRow, 2].Value = type.QuestionType;
                typeRow++;
            }

            //// Populate data for Courses
            //coursesWorksheet.Cells[1, 1].Value = "CourseName";
            //coursesWorksheet.Cells[1, 2].Value = "CourseCode";

            //var courses = GetCourses();
            //int courseRow = 2;
            //foreach (var course in courses)
            //{
            //    coursesWorksheet.Cells[courseRow, 1].Value = course.CourseName;
            //    coursesWorksheet.Cells[courseRow, 2].Value = course.CourseCode;
            //    courseRow++;
            //}

            // AutoFit columns for all worksheets
            subjectWorksheet.Cells[subjectWorksheet.Dimension.Address].AutoFitColumns();
          //  chapterWorksheet.Cells[chapterWorksheet.Dimension.Address].AutoFitColumns();
          //  topicWorksheet.Cells[topicWorksheet.Dimension.Address].AutoFitColumns();
         //   subTopicWorksheet.Cells[subTopicWorksheet.Dimension.Address].AutoFitColumns();
            difficultyLevelWorksheet.Cells[difficultyLevelWorksheet.Dimension.Address].AutoFitColumns();
            questionTypeWorksheet.Cells[questionTypeWorksheet.Dimension.Address].AutoFitColumns();
          //  coursesWorksheet.Cells[coursesWorksheet.Dimension.Address].AutoFitColumns();
        }
        public async Task<List<Option>> GetOptionsForQuestion(string questionId)
        {
            string query = @"
            SELECT mc.Answer, mc.Iscorrect 
            FROM tblAnswerMultipleChoiceCategory mc
            INNER JOIN tblAnswerMaster am ON mc.Answerid = am.Answerid
            WHERE am.QuestionCode = @QuestionId AND am.QuestionTypeid = 1";

            var options = await _connection.QueryAsync<Option>(query, new { QuestionId = questionId });

            return options.ToList();
        }
        private List<TestSeriesSubjectsResponse> GetListOfTestSeriesSubjects(int TestSeriesId)
        {
            string query = @"
        SELECT 
            tss.TestSeriesSubjectId,
            tss.SubjectID,
            tss.TestSeriesID,
            s.SubjectName AS SubjectName
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
                tsci.SubjectId,
                s.SubjectName as SubjectName,
                tsci.ContentIndexId,
                CASE 
                    WHEN tsci.IndexTypeId = 1 THEN ci.ContentName_Chapter
                    WHEN tsci.IndexTypeId = 2 THEN ct.ContentName_Topic
                    WHEN tsci.IndexTypeId = 3 THEN cst.ContentName_SubTopic
                END AS ContentIndexName,
                tsci.TestSeriesID
            FROM tblTestSeriesContentIndex tsci
            LEFT JOIN tblSubject s ON tsci.SubjectId = s.SubjectId
            LEFT JOIN tblQBIndexType it ON tsci.IndexTypeId = it.IndexId
            LEFT JOIN tblContentIndexChapters ci ON tsci.ContentIndexId = ci.ContentIndexId AND tsci.IndexTypeId = 1
            LEFT JOIN tblContentIndexTopics ct ON tsci.ContentIndexId = ct.ContInIdTopic AND tsci.IndexTypeId = 2
            LEFT JOIN tblContentIndexSubTopics cst ON tsci.ContentIndexId = cst.ContInIdSubTopic AND tsci.IndexTypeId = 3
            WHERE tsci.TestSeriesID = @TestSeriesID";

            var data = _connection.Query<TestSeriesContentIndexResponse>(query, new { TestSeriesID = TestSeriesId });
            return data != null ? data.AsList() : new List<TestSeriesContentIndexResponse>();
        }
        private List<TestSeriesQuestionSection> GetTestSeriesQuestionSection(int TestSeriesId)
        {
            string query = "SELECT * FROM tbltestseriesQuestionSection WHERE [TestSeriesid] = @TestSeriesID";

            var data = _connection.Query<TestSeriesQuestionSection>(query, new { TestSeriesID = TestSeriesId });
            return data.AsList() ?? [];
        }
        private TestSeriesInstructions GetListOfTestSeriesInstructions(int TestSeriesId)
        {
            string query = "SELECT * FROM tblTestInstructions WHERE [TestSeriesID] = @TestSeriesID";

            // Execute the SQL query with the SOTDID parameter
            var data = _connection.QueryFirstOrDefault<TestSeriesInstructions>(query, new { TestSeriesID = TestSeriesId });
            return data != null ? data : new TestSeriesInstructions();
        }
        private List<TestSeriesQuestions> GetListOfTestSeriesQuestion(int sectionId)
        {
            string query = "SELECT * FROM tbltestseriesQuestions WHERE [testseriesQuestionSectionid] = @testseriesQuestionSectionid";

            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<TestSeriesQuestions>(query, new { testseriesQuestionSectionid = sectionId });
            return data != null ? data.AsList() : [];
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
                    INSERT INTO tblTestSeriesSubjects (SubjectID, TestSeriesID)
                    VALUES (@SubjectID, @TestSeriesID);";

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
                    INSERT INTO tblTestSeriesSubjects (SubjectID, TestSeriesID)
                    VALUES (@SubjectID, @TestSeriesID);";

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
        private async Task<int> AddUpdateQIDCourses(List<QIDCourse>? request, int insertedQuestionId)
        {
            int rowsAffected = 0;
            if (request != null)
            {
                // Use questionCode to get questionId
                string getQuestionIdQuery = "SELECT QuestionID FROM tblQuestion WHERE QuestionCode = @QuestionCode AND IsActive = 1";
                int questionId = insertedQuestionId;

                if (questionId > 0)
                {
                    foreach (var data in request)
                    {
                        var newQIDCourse = new QIDCourse
                        {
                            QID = questionId,
                            CourseID = data.CourseID,
                            LevelId = data.LevelId,
                            Status = true,
                            // CreatedBy = 1,
                            CreatedDate = DateTime.Now,
                            //  ModifiedBy = 1,
                            ModifiedDate = DateTime.Now,
                            QIDCourseID = data.QIDCourseID,
                            QuestionCode = ""
                        };
                        if (data.QIDCourseID == 0)
                        {
                            string insertQuery = @"
                            INSERT INTO tblQIDCourse (QID, CourseID, LevelId, Status, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, QuestionCode)
                            VALUES (@QID, @CourseID, @LevelId, @Status, @CreatedBy, @CreatedDate, @ModifiedBy, @ModifiedDate, @QuestionCode)";

                            rowsAffected = await _connection.ExecuteAsync(insertQuery, newQIDCourse);
                        }
                        else
                        {
                            string updateQuery = @"
                           UPDATE tblQIDCourse
                           SET QID = @QID,
                               CourseID = @CourseID,
                               LevelId = @LevelId,
                               Status = @Status,
                               CreatedBy = @CreatedBy,
                               CreatedDate = @CreatedDate,
                               ModifiedBy = @ModifiedBy,
                               ModifiedDate = @ModifiedDate,
                               QuestionCode = @QuestionCode
                           WHERE QIDCourseID = @QIDCourseID";
                            rowsAffected = await _connection.ExecuteAsync(updateQuery, newQIDCourse);
                        }
                    }
                    return rowsAffected;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
    }
    public class ContentIndexDetails
    {
        public int IndexTypeId { get; set; }
        public int ContentIndexId { get; set; }
        public int SubjectId { get; set; }
    }
}