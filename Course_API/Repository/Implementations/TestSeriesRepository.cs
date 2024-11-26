using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;
using Course_API.Repository.Interfaces;
using Dapper;
using OfficeOpenXml;
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
                // Fetch existing test papers for the test series (both regular and repeated exams)
                string timeValidationQuery = @"
SELECT StartDate, StartTime, Duration, RepeatedExams, RepeatExamStartDate, RepeatExamEndDate
FROM tblTestSeries 
WHERE TestSeriesId != @TestSeriesId 
  AND APID = @APID";  // Fetch test papers for the same APID

                var existingTestPapers = await _connection.QueryAsync<(DateTime StartDate, string StartTime, string Duration, bool RepeatedExams, DateTime? RepeatExamStartDate, DateTime? RepeatExamEndDate)>(timeValidationQuery,
                    new
                    {
                        TestSeriesId = request.TestSeriesId,
                        APID = request.APID
                    });

                // Check if RepeatedExams is false (Non-repeated test case)
                if (request.RepeatedExams == false)
                {
                    DateTime requestedStartDateTime;
                    if (DateTime.TryParse(request.StartTime, out DateTime parsedStartTime))
                    {
                        // Combine StartDate with parsed StartTime (the date part of parsedStartTime will be ignored)
                         requestedStartDateTime = request.StartDate.Value.Add(parsedStartTime.TimeOfDay);

                        // Now you can proceed with the rest of your logic using requestedStartDateTime
                    }
                    else
                    {
                        // Handle invalid time format
                        return new ServiceResponse<int>(false, "Invalid StartTime format.", 0, 400);
                    }

                    foreach (var testPaper in existingTestPapers)
                    {
                        // Check for overlap with other regular tests
                        if (!testPaper.RepeatedExams)
                        {
                            // Assuming testPaper.StartTime is a string in the format "HH:mm:ss"
                            if (TimeSpan.TryParse(testPaper.StartTime, out TimeSpan startTimeSpan))
                            {
                                // Combine StartDate with StartTime to create the full DateTime for the test start
                                DateTime existingStartDateTime = testPaper.StartDate.Add(startTimeSpan);

                                // Extract numeric value from testPaper.Duration (e.g., "120 minutes")
                                if (!string.IsNullOrWhiteSpace(testPaper.Duration) &&
                                    double.TryParse(new string(testPaper.Duration.TakeWhile(char.IsDigit).ToArray()), out double durationInMinutes))
                                {
                                    // Calculate the end time of the existing test
                                    DateTime existingEndDateTime = existingStartDateTime.AddMinutes(durationInMinutes);

                                    // Ensure the requested test does not overlap or is less than 15 minutes apart
                                    if (requestedStartDateTime < existingEndDateTime.AddMinutes(15))
                                    {
                                        return new ServiceResponse<int>(false, "Test papers cannot overlap or be less than 15 minutes apart.", 0, 400);
                                    }
                                }
                                else
                                {
                                    // Handle invalid or missing duration
                                    return new ServiceResponse<int>(false, "Invalid test duration.", 0, 400);
                                }

                            }
                            else
                            {
                                return new ServiceResponse<int>(false, "Invalid StartTime format.", 0, 400);
                            }
                        }
                        // Check for overlap with repeated exams' time window
                        if (testPaper.RepeatedExams && testPaper.RepeatExamStartDate.HasValue && testPaper.RepeatExamEndDate.HasValue)
                        {
                            if (request.StartDate >= testPaper.RepeatExamStartDate && request.StartDate <= testPaper.RepeatExamEndDate)
                            {
                                return new ServiceResponse<int>(false, "The test cannot overlap with an existing repeated exam period.", 0, 400);
                            }
                        }
                    }
                }
                else // Repeated exam case
                {
                    // Ensure no overlap with other repeated exams' time windows
                    if (request.RepeatExamStartDate.HasValue && request.RepeatExamEndDate.HasValue)
                    {
                        foreach (var testPaper in existingTestPapers)
                        {
                            // Check for overlap with other repeated exams
                            if (testPaper.RepeatedExams)
                            {
                                if ((request.RepeatExamStartDate <= testPaper.RepeatExamEndDate && request.RepeatExamStartDate >= testPaper.RepeatExamStartDate) ||
                                    (request.RepeatExamEndDate <= testPaper.RepeatExamEndDate && request.RepeatExamEndDate >= testPaper.RepeatExamStartDate))
                                {
                                    return new ServiceResponse<int>(false, "Repeated exams cannot overlap with each other.", 0, 400);
                                }
                            }

                            // Check for overlap with regular test timings during repeat exam period
                            if (!testPaper.RepeatedExams)
                            {
                                // Assuming testPaper.StartTime is a string in the format "HH:mm:ss"
                                if (TimeSpan.TryParse(testPaper.StartTime, out TimeSpan startTimeSpan))
                                {
                                    // Combine StartDate with StartTime to create the full DateTime for the test start
                                    DateTime existingStartDateTime = testPaper.StartDate.Add(startTimeSpan);

                                    // Convert testPaper.Duration to double if necessary
                                    if (double.TryParse(testPaper.Duration.ToString(), out double durationInMinutes))
                                    {
                                        DateTime existingEndDateTime = existingStartDateTime.AddMinutes(durationInMinutes);

                                        // Ensure repeated test's repeat period does not overlap with non-repeated test timings
                                        if ((existingStartDateTime >= request.RepeatExamStartDate && existingStartDateTime <= request.RepeatExamEndDate) ||
                                            (existingEndDateTime >= request.RepeatExamStartDate && existingEndDateTime <= request.RepeatExamEndDate))
                                        {
                                            return new ServiceResponse<int>(false, "Regular test timings cannot overlap with the repeated exam period.", 0, 400);
                                        }
                                    }
                                    else
                                    {
                                        return new ServiceResponse<int>(false, "Invalid test duration.", 0, 400);
                                    }
                                }
                                else
                                {
                                    return new ServiceResponse<int>(false, "Invalid StartTime format.", 0, 400);
                                }
                            }
                        }
                    }
                    else
                    {
                        return new ServiceResponse<int>(false, "Invalid repeat exam period provided.", 0, 400);
                    }
                }

                // Proceed with inserting or updating the test after validation
                if (request.TestSeriesId == 0)
                {
                    bool IsAdmin;
                    string roleQuery = @"
                    SELECT r.RoleName 
                    FROM tblEmployee e 
                    JOIN tblRole r ON e.RoleID = r.RoleID 
                    WHERE e.EmployeeId = @EmployeeId";

                    string roledata = _connection.QueryFirstOrDefault<string>(roleQuery, new { EmployeeId = request.EmployeeID });

                    // Correct ternary assignment
                    IsAdmin = roledata == "Admin" ? true : false;

                    string insertQuery = @"
                    INSERT INTO tblTestSeries 
                    (
                        TestPatternName, Duration, Status, APID, TotalNoOfQuestions, ExamTypeID,
                        ManualQuestionSelect, StartDate, StartTime, ResultDate, ResultTime, 
                        EmployeeID, NameOfExam, RepeatedExams, TypeOfTestSeries, 
                        createdon, createdby, RepeatExamStartDate , RepeatExamEndDate ,
                        RepeatExamStarttime , RepeatExamResulttimeId, IsAdmin, DownloadStatusId
                    ) 
                    VALUES 
                    (
                        @TestPatternName, @Duration, @Status, @APID, @TotalNoOfQuestions, @ExamTypeID,
                        @ManualQuestionSelect, @StartDate, @StartTime, @ResultDate, @ResultTime, 
                        @EmployeeID, @NameOfExam, @RepeatedExams, @TypeOfTestSeries, 
                        @createdon, @createdby, @RepeatExamStartDate , @RepeatExamEndDate ,
                        @RepeatExamStarttime , @RepeatExamResulttimeId, @IsAdmin, @DownloadStatusId
                    ); 
                    SELECT CAST(SCOPE_IDENTITY() as int);";
                    var parameters = new
                    {
                        request.TestPatternName,
                        request.Duration,
                        request.Status,
                        request.APID,
                        request.TotalNoOfQuestions,
                        request.ManualQuestionSelect,
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
                        IsAdmin = IsAdmin,
                        DownloadStatusId = 1
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
                        ManualQuestionSelect = @ManualQuestionSelect,
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
                        RepeatExamResulttimeId = @RepeatExamResulttimeId
                    WHERE TestSeriesId = @TestSeriesId;";
                    var parameters = new
                    {
                        request.TestPatternName,
                        request.Duration,
                        request.Status,
                        request.APID,
                        request.TotalNoOfQuestions,
                        request.ManualQuestionSelect,
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
                       // request.IsAdmin
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
                ts.ManualQuestionSelect,
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
                        var questions = GetListOfTestSeriesQuestion(TestSeriesId);
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
                    int downloadStatus = await _connection.QueryFirstOrDefaultAsync<int>(@"select DownloadStatusId from tblTestSeries where TestSeriesId = @TestSeriesId", new { TestSeriesId = request.TestSeriesId });
                    if (downloadStatus == 6)
                    {
                        string updateDownloadstatus = @"update tblTestSeries set DownloadStatusId = 7 where TestSeriesId = @TestSeriesId";
                        await _connection.ExecuteAsync(updateDownloadstatus, new { TestSeriesId = request.TestSeriesId });
                    }
                    else
                    {
                        string updateDownloadstatus = @"update tblTestSeries set DownloadStatusId = 5 where TestSeriesId = @TestSeriesId";
                        await _connection.ExecuteAsync(updateDownloadstatus, new { TestSeriesId = request.TestSeriesId });
                    }

                    string roleQuery = @"
                    SELECT r.RoleName 
                    FROM tblEmployee e 
                    JOIN tblRole r ON e.RoleID = r.RoleID 
                    WHERE e.EmployeeId = @EmployeeId";
                    string roledata = _connection.QueryFirstOrDefault<string>(roleQuery, new { Employeeid = request.EmployeeId });

                    if (string.Equals(roledata, "Admin"))
                    {
                        int count = await _connection.QueryFirstOrDefaultAsync<int>(@"select count(*) from tblTestSeriesRejectedRemarks where TestSeriesId = @TestSeriesId", new { TestSeriesId = request.TestSeriesId });
                        if (count == 0)
                        {
                            string updateDownloadstatus1 = @"update tblTestSeries set DownloadStatusId = 8 where TestSeriesId = @TestSeriesId";
                            await _connection.ExecuteAsync(updateDownloadstatus1, new { TestSeriesId = request.TestSeriesId });
                        }
                    }
                }
                else
                {
                    string updateDownloadstatus = @"update tblTestSeries set DownloadStatusId = 4 where TestSeriesId = @TestSeriesId";
                    await _connection.ExecuteAsync(updateDownloadstatus, new { TestSeriesId = request.TestSeriesId });
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
                // Query for test series created by employee
                var createdTestSeriesQuery = @"
        SELECT 
            ts.TestSeriesId,
            ts.TestPatternName,
            ts.Duration,
            ts.Status,
            ts.APID,
            ap.APName AS APName,
            ts.TotalNoOfQuestions,
            ts.ManualQuestionSelect,
            ts.StartDate,
            ts.IsMandatory,
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
            ts.DownloadStatusId,
            ts.RepeatExamEndDate,
            ts.RepeatExamStarttime,
            ts.RepeatExamResulttimeId,
            rt.ResultTime AS RepeatedExamResultTime,
            ts.IsAdmin
        FROM tblTestSeries ts
        JOIN tblCategory ap ON ts.APID = ap.APID
        JOIN tblEmployee emp ON ts.EmployeeID = emp.EmployeeID
        JOIN tblTypeOfTestSeries tts ON ts.TypeOfTestSeries = tts.TTSId
        LEFT JOIN tblExamType ttt ON ts.ExamTypeID = ttt.ExamTypeID
        LEFT JOIN tblTestSeriesResultTime rt ON ts.RepeatExamResulttimeId = rt.ResultTimeId
        LEFT JOIN tblTestSeriesClass tc ON ts.TestSeriesId = tc.TestSeriesId
        LEFT JOIN tblTestSeriesCourse tco ON ts.TestSeriesId = tco.TestSeriesId
        LEFT JOIN tblTestSeriesBoards tb ON ts.TestSeriesId = tb.TestSeriesId
        WHERE ts.EmployeeID = @EmployeeId AND ts.IsAdmin = @IsAdmin";

                // Query for test series assigned to employee
                var assignedTestSeriesQuery = @"
        SELECT 
            tsp.TSProfilerId,
            tsp.TestSeriesId,
            tsp.EmployeeId,
            tsp.AssignedDate,
            tsp.IsActive,
            ts.TestPatternName,
            ts.Duration,
            ts.Status,
            ts.NameOfExam,
            ts.StartDate,
            ts.ResultDate,
            ts.RepeatedExams,
            ts.RepeatExamStartDate,
            ts.RepeatExamEndDate
        FROM tblTestSeriesProfiler tsp
        JOIN tblTestSeries ts ON tsp.TestSeriesId = ts.TestSeriesId
        LEFT JOIN tblTestSeriesClass tc ON ts.TestSeriesId = tc.TestSeriesId
        LEFT JOIN tblTestSeriesCourse tco ON ts.TestSeriesId = tco.TestSeriesId
        LEFT JOIN tblTestSeriesBoards tb ON ts.TestSeriesId = tb.TestSeriesId
        WHERE tsp.EmployeeId = @EmployeeId";

                // Apply filters dynamically in both queries
                var whereConditions = new List<string>();
                if (request.APId > 0)
                {
                    whereConditions.Add("ts.APID = @APId");
                }
                if (request.ClassId > 0)
                {
                    whereConditions.Add("tc.ClassId = @ClassId");
                }
                if (request.CourseId > 0)
                {
                    whereConditions.Add("tco.CourseId = @CourseId");
                }
                if (request.BoardId > 0)
                {
                    whereConditions.Add("tb.BoardId = @BoardId");
                }
                if (request.ExamTypeId > 0)
                {
                    whereConditions.Add("ts.ExamTypeID = @ExamTypeId");
                }
                if (request.TypeOfTestSeries > 0)
                {
                    whereConditions.Add("ts.TypeOfTestSeries = @TypeOfTestSeries");
                }
                if (request.ExamStatus > 0 && request.Date.HasValue)
                {
                    whereConditions.Add(@"
            (@ExamStatus IS NULL OR 
                (ts.RepeatedExams = 1 AND ts.RepeatExamStartDate <= @Date AND ts.RepeatExamEndDate >= @Date) OR 
                (ts.RepeatedExams = 0 AND ts.StartDate <= @Date AND DATEADD(MINUTE, CAST(ts.Duration AS INT), ts.StartDate) >= @Date))");
                }

                // Add where conditions to both queries
                if (whereConditions.Any())
                {
                    var whereClause = " AND " + string.Join(" AND ", whereConditions);
                    createdTestSeriesQuery += whereClause;
                    assignedTestSeriesQuery += whereClause;
                }

                // Parameters for filtering
                var parameters = new
                {
                    EmployeeId = request.EmployeeId,
                    IsAdmin = request.IsAdmin,
                    APId = request.APId == 0 ? (int?)null : request.APId,
                    ClassId = request.ClassId == 0 ? (int?)null : request.ClassId,
                    CourseId = request.CourseId == 0 ? (int?)null : request.CourseId,
                    BoardId = request.BoardId == 0 ? (int?)null : request.BoardId,
                    ExamTypeId = request.ExamTypeId == 0 ? (int?)null : request.ExamTypeId,
                    TypeOfTestSeries = request.TypeOfTestSeries == 0 ? (int?)null : request.TypeOfTestSeries,
                    ExamStatus = request.ExamStatus == 0 ? (int?)null : request.ExamStatus,
                    Date = request.Date
                };

                // Execute both queries
                var createdTestSeries = await _connection.QueryAsync<TestSeriesResponseDTO>(createdTestSeriesQuery, parameters);
                var assignedTestSeries = await _connection.QueryAsync<TestSeriesResponseDTO>(assignedTestSeriesQuery, parameters);

                // Combine both results into a single list
                var allTestSeries = createdTestSeries.Union(assignedTestSeries).ToList();

                // Paginate the results
                var totalRecords = allTestSeries.Count();
                var paginatedTestSeriesList = allTestSeries
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                // Fetch related data for each test series (Boards, Classes, Courses, etc.)
                foreach (var testSeries in paginatedTestSeriesList)
                {
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
                foreach(var data in paginatedTestSeriesList)
                {
                    if (data.IsAdmin == false)
                    {
                        if (data.DownloadStatusId >= 8)
                        {
                            DateTime startDateTime = data.StartDate.Value.Add(DateTime.Parse(data.StartTime).TimeOfDay);

                            if (DateTime.Now < startDateTime)
                            {
                                data.ExamStatus = "Upcoming";
                            }
                            else if (DateTime.Now >= startDateTime && DateTime.Now <= data.ResultDate)
                            {
                                data.ExamStatus = "Ongoing";
                            }
                            else
                            {
                                data.ExamStatus = "Completed";
                            }
                        }
                        else
                        {
                            string q1 = @"Select DownloadStatusId from tblTestSeries where TestSeriesId = @TestSeriesId";
                            var d1 = await _connection.QueryFirstOrDefaultAsync<int>(q1, new { TestSeriesId = data.TestSeriesId });
                            string currentowner = @"select EmployeeId from tblTestSeriesProfiler where TestSeriesId = @TestSeriesId  AND IsActive = 1";
                            var owner = _connection.QueryFirstOrDefault<int>(currentowner, new { TestSeriesId = data.TestSeriesId });
                            if (owner == 0)
                            {
                                owner = request.EmployeeId;
                            }
                            int count = await _connection.QueryFirstOrDefaultAsync<int>(@"select count(*) from tblTestSeriesRejectedRemarks where TestSeriesId = @TestSeriesId", new { TestSeriesId = data.TestSeriesId });
                            if (count > 0)
                            {
                                data.ExamStatus = TestSeriesStatus(request.EmployeeId, d1, owner, true);
                            }
                            else
                            {
                                data.ExamStatus = TestSeriesStatus(request.EmployeeId, d1, owner, false);
                            }
                        }
                    }
                    else
                    {
                        if (data.RepeatedExams)
                        {
                            // Current date and time
                            DateTime currentDateTime = DateTime.Now;

                            // Exam start and end times
                            TimeSpan examStartTime = TimeSpan.Parse(data.RepeatExamStarttime);
                            int durationInMinutes = int.Parse(data.Duration);

                            // Calculate the end time based on the duration
                            TimeSpan examEndTime = examStartTime.Add(TimeSpan.FromMinutes(durationInMinutes));
                            data.RepeatedExamEndTime = examEndTime.ToString();

                            // Exam period start and end dates
                            DateTime repeatExamStartDate = data.RepeatExamStartDate;
                            DateTime repeatExamEndDate = data.RepeatExamEndDate;

                            // Exam start and end DateTime for the current day
                            DateTime dailyExamStartDateTime = repeatExamStartDate.Add(examStartTime);
                            DateTime dailyExamEndDateTime = repeatExamStartDate.Add(examEndTime);

                            if (currentDateTime < dailyExamStartDateTime)
                            {
                                data.ExamStatus = "Upcoming";
                            }
                            else if (currentDateTime >= dailyExamStartDateTime && currentDateTime <= dailyExamEndDateTime)
                            {
                                data.ExamStatus = "Ongoing";
                            }
                            else if (currentDateTime > dailyExamEndDateTime && currentDateTime < repeatExamEndDate.AddDays(1).Add(examStartTime))
                            {
                                data.ExamStatus = "Upcoming";
                            }
                            else if (currentDateTime >= repeatExamEndDate.Add(examEndTime))
                            {
                                data.ExamStatus = "Completed";
                            }
                        }
                        else
                        {
                            DateTime startDateTime = data.StartDate.Value.Add(DateTime.Parse(data.StartTime).TimeOfDay);

                            if (DateTime.Now < startDateTime)
                            {
                                data.ExamStatus = "Upcoming";
                            }
                            else if (DateTime.Now >= startDateTime && DateTime.Now <= data.ResultDate)
                            {
                                data.ExamStatus = "Ongoing";
                            }
                            else
                            {
                                data.ExamStatus = "Completed";
                            }
                        }

                    }
                }
                // Return the paginated result
                return new ServiceResponse<List<TestSeriesResponseDTO>>(true, "Test series retrieved successfully", paginatedTestSeriesList, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<TestSeriesResponseDTO>>(false, "An error occurred while retrieving test series: " + ex.Message, null, 500);
            }
        }
        private string TestSeriesStatus(int EmployeeId, int DownloadStatusId, int ownerId, bool IsRejected)
        {
            string roleId = @"Select RoleID from tblEmployee where Employeeid = @Employeeid";
            var RoleId = _connection.QueryFirstOrDefault<int>(roleId, new { Employeeid = EmployeeId });
            var ownerId1 = _connection.QueryFirstOrDefault<int>(roleId, new { Employeeid = ownerId });
            string statusQuery = @"select StatusID from TestSeriesStatus where RoleID = @RoleID AND DownloadStatus = @DownloadStatus AND
                                    OwnerRole = @OwnerRole AND IsRejected = @IsRejected  AND StatusID != 0";
            var data = _connection.QueryFirstOrDefault<int>(statusQuery, new { RoleID = ownerId1, DownloadStatus = DownloadStatusId, OwnerRole = RoleId, IsRejected = IsRejected });

            var status = _connection.QueryFirstOrDefault<string>(@"select RQSName from tblStatus where RQSID = @statusId", new { statusId = data });
            return status;
        }
        public async Task<ServiceResponse<string>> SMECreatedTestseriesAssignTime(TestSeriesAddTime request)
        {
            try
            {
                if (!request.IsMandatory)
                {
                    //add a check to see if given test series is repeated type;
                    string updateDownloadstatus = @"update tblTestSeries set DownloadStatusId = 9,
                                               IsMandatory = 0 where TestSeriesId = @TestSeriesId";
                    await _connection.ExecuteAsync(updateDownloadstatus, new { TestSeriesId = request.TestSeriesID });
                    return new ServiceResponse<string>(true, "Operation successful", "status updated successfully", 200);
                }
                else
                {
                    string updateDownloadstatus = @"update tblTestSeries set StartDate = @StartDate,
                        StartTime = @StartTime,
                        ResultDate = @ResultDate,
                        ResultTime = @ResultTime,
                        IsMandatory = 1 where TestSeriesId = @TestSeriesId";
                    await _connection.ExecuteAsync(updateDownloadstatus, new
                    {
                        TestSeriesId = request.TestSeriesID,
                        StartDate = request.StartDate,
                        StartTime = request.StartTime,
                        ResultDate = request.ResultDate,
                        ResultTime = request.ResultTime
                    });
                    return new ServiceResponse<string>(true, "Operation successful", "status updated successfully", 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
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
            try
            {
                // Step 1: Retrieve the total number of questions for the test series from tblTestSeries
                var testSeriesQuery = "SELECT TotalNoOfQuestions FROM tblTestSeries WHERE TestSeriesId = @TestSeriesId";
                int totalNoOfQuestionsForTestSeries = await _connection.QueryFirstOrDefaultAsync<int>(testSeriesQuery, new { TestSeriesId });

                // Step 2: Retrieve all existing sections for the given TestSeriesId
                var existingSectionsQuery = "SELECT TotalNoofQuestions FROM tbltestseriesQuestionSection WHERE TestSeriesid = @TestSeriesId";
                var existingSections = await _connection.QueryAsync<int>(existingSectionsQuery, new { TestSeriesId });

                // Step 3: Sum up the total number of questions from existing sections
                int totalExistingQuestions = existingSections.Sum();

                // Step 4: Sum up TotalNoofQuestions from the request body
                int totalRequestedQuestions = request.Sum(section => section.TotalNoofQuestions);

                // Step 5: Calculate the total number of questions (existing + requested)
                int totalAssignedQuestions = totalExistingQuestions + totalRequestedQuestions;

                // Step 6: Validate that the total assigned questions do not exceed the total number of questions for the test series
                if (totalAssignedQuestions > totalNoOfQuestionsForTestSeries)
                {
                    return new ServiceResponse<string>(false, $"The total number of questions assigned ({totalAssignedQuestions}) exceeds the limit of {totalNoOfQuestionsForTestSeries} for the test series.", null, StatusCodes.Status400BadRequest);
                }

                // Step 7: Update TestSeriesId for all sections in the request
                foreach (var section in request)
                {
                    section.TestSeriesid = TestSeriesId;
                }

                // Step 8: Delete existing records for the given TestSeriesId (if you want to replace them)
                var deleteQuery = "DELETE FROM [tbltestseriesQuestionSection] WHERE [TestSeriesid] = @TestSeriesId";
                await _connection.ExecuteAsync(deleteQuery, new { TestSeriesId });

                // Step 9: Insert new records for sections
                string insertQuery = @"
            INSERT INTO tbltestseriesQuestionSection 
            (TestSeriesid, DisplayOrder, SectionName, Status, LevelID1, QuesPerDifficulty1, LevelID2, QuesPerDifficulty2, LevelID3, QuesPerDifficulty3, QuestionTypeID, EntermarksperCorrectAnswer, EnterNegativeMarks, TotalNoofQuestions, NoofQuestionsforChoice, SubjectId)
            VALUES 
            (@TestSeriesid, @DisplayOrder, @SectionName, @Status, @LevelID1, @QuesPerDifficulty1, @LevelID2, @QuesPerDifficulty2, @LevelID3, @QuesPerDifficulty3, @QuestionTypeID, @EntermarksperCorrectAnswer, @EnterNegativeMarks, @TotalNoofQuestions, @NoofQuestionsforChoice, @SubjectId);";

                await _connection.ExecuteAsync(insertQuery, request);

                return new ServiceResponse<string>(true, "Operation successful", "Sections mapped successfully", StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while mapping test series sections", ex);
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

                //if (questionSection == null)
                //{
                //    return new ServiceResponse<string>(false, "operation failed", "Test series question section not found.", 400);
                //}

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
                string existingQuestionsQuery = "SELECT COUNT(*) FROM tbltestseriesQuestions WHERE TestSeriesid = @TestSeriesid";
                int existingQuestionsCount = await _connection.QueryFirstOrDefaultAsync<int>(existingQuestionsQuery, new { TestSeriesid = TestSeriesId });


                // Step 8: Delete existing questions
                string deleteQuery = "DELETE FROM tbltestseriesQuestions WHERE TestSeriesid = @TestSeriesid";
                await _connection.ExecuteAsync(deleteQuery, new { TestSeriesid = TestSeriesId });


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
                    UPDATE tblQuestion
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
                        QuestionId = @QuestionId";
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
                var insertedQuestionId = request.QuestionId;
                await _connection.QuerySingleOrDefaultAsync<int>(query, parameters);

                string insertedQuestionCode = "string";

                if (!string.IsNullOrEmpty(insertedQuestionCode))
                {
                    // Handle QIDCourses mapping
                    if (request.QIDCourses != null)
                    {
                        var data = await AddUpdateQIDCourses(request.QIDCourses, request.QuestionId);
                    }
                 
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
                            Questionid = 0, // Set to 0 or remove if QuestionId is not required
                            QuestionTypeid = questTypedata?.QuestionTypeID,
                            QuestionCode = insertedQuestionCode
                        });
                    }

                    // If the question type supports multiple-choice or similar categories
                    if (questTypedata != null)
                    {
                        if (questTypedata.Code.Trim() == "MCQ" || questTypedata.Code.Trim() == "TF" || questTypedata.Code.Trim() == "MF" ||
                            questTypedata.Code.Trim() == "MAQ" || questTypedata.Code.Trim() == "MF2" || questTypedata.Code.Trim() == "AR" || questTypedata.Code.Trim() == "CT")
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

                    if ( answer > 0)
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
        public async Task<ServiceResponse<string>> ApproveRejectedQuestion(int testSeriesId, int QuestionId)
        {
            try
            {

                string updateQuery = @"update tblQuestion set IsRejected = 0 where QuestionId = @QuestionId";
                var data = await _connection.ExecuteAsync(updateQuery, new { QuestionId = QuestionId });

                await _connection.ExecuteAsync(@"delete from tblTestSeriesRejectedRemarks where QuestionId = QuestionId", new { QuestionId = QuestionId });
               
                return new ServiceResponse<string>(true, "question approval successfully done.", "operation successful", 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<string>> TestSeriesRejectedQuestionRemarks(RejectedQuestionRemark request)
        {
            try
            {
                // SQL query to insert the rejection remark
                string insertQuery = @"
        INSERT INTO tblTestSeriesRejectedRemarks 
        (TestSeriesId, QuestionId, Remarks, RejectedBy, ImageOrPdf)
        VALUES 
        (@TestSeriesId, @QuestionId, @Remarks, @RejectedBy, @ImageOrPdf)";

                // Execute the query using Dapper
                await _connection.ExecuteAsync(insertQuery, new
                {
                    request.TestSeriesId,
                    request.QuestionId,
                    request.Remarks,
                    request.RejectedBy,
                    request.ImageOrPdf
                });
               
                string updateQuery = @"update tblQuestion set IsRejected = 1 where QuestionId = @QuestionId";
                var data = await _connection.ExecuteAsync(updateQuery, new { QuestionId = request.QuestionId });
                // If the query succeeds, return a success response

                int count = await _connection.QueryFirstOrDefaultAsync<int>(@"select count (*) from tblTestSeriesRejectedRemarks where TestSeriesId = @TestSeriesId",
                    new { TestSeriesId = request.TestSeriesId });
                
                    string updateDownloadstatus = @"update tblTestSeries set DownloadStatusId = 6 where TestSeriesId = @TestSeriesId";
                    await _connection.ExecuteAsync(updateDownloadstatus, new { TestSeriesId = request.TestSeriesId });
                
                return new ServiceResponse<string>(true, "Rejection remark added successfully.", "operation successful", 200);
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
                    var testSeriesDetailsQuery = @"
    SELECT 
        DISTINCT LevelID1, LevelID2, LevelID3, QuestionTypeID
    FROM 
        [tbltestseriesQuestionSection]
    WHERE 
        TestSeriesid = @TestSeriesid";

                    // Execute the query
                    var testSeriesDetails1 = await _connection.QueryAsync<dynamic>(testSeriesDetailsQuery, new { TestSeriesid = request.TestSeriesId });

                    var difficultyLevels = new List<int>();
                    var questionTypes = new List<int>();

                    foreach (var detail in testSeriesDetails1)
                    {
                        // Add the difficulty levels if they are not already in the list
                        if (detail.LevelID1 != null && !difficultyLevels.Contains(detail.LevelID1))
                            difficultyLevels.Add(detail.LevelID1);

                        if (detail.LevelID2 != null && !difficultyLevels.Contains(detail.LevelID2))
                            difficultyLevels.Add(detail.LevelID2);

                        if (detail.LevelID3 != null && !difficultyLevels.Contains(detail.LevelID3))
                            difficultyLevels.Add(detail.LevelID3);

                        // Add the question type if it's not already in the list
                        if (detail.QuestionTypeID != null && !questionTypes.Contains(detail.QuestionTypeID))
                            questionTypes.Add(detail.QuestionTypeID);
                    }

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
                    worksheet.Cells[1, 5].Value = "Display Order";
                    worksheet.Cells[1, 6].Value = "ParagraphId";
                    worksheet.Cells[1, 7].Value = "ParagraphQuestionTypeId";
                    worksheet.Cells[1, 8].Value = "Question";
                    worksheet.Cells[1, 9].Value = "Answer";

                    // Add headers for options and other details
                    for (int i = 1; i <= 4; i++)
                    {
                        worksheet.Cells[1, 9 + i].Value = $"Option{i}";
                    }

                    worksheet.Cells[1, 14].Value = "Explanation";
                    worksheet.Cells[1, 15].Value = "Extra Information";
                  
                    
                    // Format headers
                    using (var range = worksheet.Cells[1, 1, 1, 27])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                       // range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
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
                            worksheet.Cells[row, 5].Value = displayOrder++; // Display order
                            worksheet.Cells[row, 6].Value = ""; // ParagraphId
                            worksheet.Cells[row, 7].Value = ""; // Answer
                            worksheet.Cells[row, 8].Value = "Q"; // Question
                            worksheet.Cells[row, 9].Value = "A"; // Answer

                            FillOptionsBasedOnQuestionType(data.QuestionTypeID, worksheet, row);

                            // Fill explanation, extra information, and display order
                            worksheet.Cells[row, 14].Value = "Explanation"; // Dummy explanation
                            worksheet.Cells[row, 15].Value = "Extra Info"; // Dummy extra information
                         
                            row++; // Move to next row
                        }

                        for (int i = 0; i < data.QuesPerDifficulty2; i++)
                        {
                            // 2nd Row for Difficulty Level 2
                            worksheet.Cells[row, 1].Value = request.TestSeriesId; // Exam Paper ID
                            worksheet.Cells[row, 2].Value = data.SubjectId; // Subject ID
                            worksheet.Cells[row, 3].Value = data.QuestionTypeID; // Question Type
                            worksheet.Cells[row, 4].Value = data.LevelID2; // Difficulty Level 2
                            worksheet.Cells[row, 5].Value = displayOrder++; // Display order
                            worksheet.Cells[row, 6].Value = ""; // ParagraphId
                            worksheet.Cells[row, 7].Value = ""; // ParagraphQuestionId
                            worksheet.Cells[row, 8].Value = "Q"; // Question
                            worksheet.Cells[row, 9].Value = "A"; // Answer

                            FillOptionsBasedOnQuestionType(data.QuestionTypeID, worksheet, row);

                            // Fill explanation, extra information, and display order
                            worksheet.Cells[row, 14].Value = "Explanation"; // Dummy explanation
                            worksheet.Cells[row, 15].Value = "Extra Info"; // Dummy extra information
                          
                            row++; // Move to next row
                        }

                        for (int i = 0; i < data.QuesPerDifficulty3; i++)
                        {
                            // 3rd Row for Difficulty Level 3
                            worksheet.Cells[row, 1].Value = request.TestSeriesId; // Exam Paper ID
                            worksheet.Cells[row, 2].Value = data.SubjectId; // Subject ID
                            worksheet.Cells[row, 3].Value = data.QuestionTypeID; // Question Type
                            worksheet.Cells[row, 4].Value = data.LevelID3; // Difficulty Level 3
                            worksheet.Cells[row, 5].Value = displayOrder++; // Display order
                            worksheet.Cells[row, 6].Value = ""; // ParagraphId
                            worksheet.Cells[row, 7].Value = ""; // ParagraphQuestionId
                            worksheet.Cells[row, 8].Value = "Q"; // Question
                            worksheet.Cells[row, 9].Value = "A"; // Answer

                            FillOptionsBasedOnQuestionType(data.QuestionTypeID, worksheet, row);

                            // Fill explanation, extra information, and display order
                            worksheet.Cells[row, 14].Value = "Explanation"; // Dummy explanation
                            worksheet.Cells[row, 15].Value = "Extra Info"; // Dummy extra information

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

                    AddMasterDataSheets(package, testSeriesContent.ToList(), difficultyLevels, questionTypes);
                    var fileBytes = package.GetAsByteArray();
                    string updateDownloadstatus = @"update tblTestSeries set DownloadStatusId = 2 where TestSeriesId = @TestSeriesId";
                    await _connection.ExecuteAsync(updateDownloadstatus, new { TestSeriesId = request.TestSeriesId });
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
        public async Task<ServiceResponse<string>> AddUpdateComprehensiveQuestion(ComprehensiveQuestionRequest request)
        {
            try
            {
                string query = @"
        INSERT INTO tblQuestion 
        (Paragraph, QuestionTypeId, Status, CategoryId, CreatedBy, CreatedOn, SubjectID, EmployeeId, ModifierId, 
         IndexTypeId, ContentIndexId, IsRejected, IsApproved, QuestionCode, Explanation, ExtraInformation, IsActive, IsConfigure, QuestionDescription)
        VALUES 
        (@Paragraph, @QuestionTypeId, @Status, @CategoryId, @CreatedBy, @CreatedOn, @SubjectID, @EmployeeId, @ModifierId, 
         @IndexTypeId, @ContentIndexId, @IsRejected, @IsApproved, @QuestionCode, @Explanation, @ExtraInformation, @IsActive, @IsConfigure, 'string');
        SELECT CAST(SCOPE_IDENTITY() AS INT);";
                // Execute the insert query and return the generated QuestionId

                string deactivateQuery = @"UPDATE tblQuestion SET IsActive = 0 WHERE QuestionCode = @QuestionCode AND IsActive = 1";
                await _connection.ExecuteAsync(deactivateQuery, new { request.QuestionCode });
                // Retrieve the QuestionCode after insertion

                var insertedQuestionId = await _connection.QuerySingleOrDefaultAsync<int>(query, request);
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

               
                    // Handle QIDCourses mapping
                    var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionId);
                    foreach (var record in request.Questions)
                    {
                        record.ParentQCode = insertedQuestionCode;
                        record.ParentQId = insertedQuestionId;
                        string insertQuery1 = @"
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
                  IsConfigure,
                  CategoryId,
                  ParentQId, ParentQCode
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
                  @IsActive, @IsConfigure, @CategoryId, @ParentQId, @ParentQCode
              );

              -- Fetch the QuestionId of the newly inserted row
              SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        string deactivateQuery1 = @"UPDATE tblQuestion SET IsActive = 0 WHERE QuestionCode = @QuestionCode AND IsActive = 1";
                        await _connection.ExecuteAsync(deactivateQuery1, new { request.QuestionCode });
                        // Retrieve the QuestionCode after insertion
                        // var insertedQuestionCode = await _connection.QuerySingleOrDefaultAsync<string>(insertQuery, question);
                        var insertedQuestionId1 = await _connection.QuerySingleOrDefaultAsync<int>(insertQuery1, record);
                        string code1 = string.Empty;
                        if (string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string")
                        {
                            code = GenerateQuestionCode(request.IndexTypeId, request.ContentIndexId, insertedQuestionId1);

                            string questionCodeQuery = @"
                            UPDATE tblQuestion
                            SET QuestionCode = @QuestionCode
                            WHERE QuestionId = @QuestionId AND IsActive = 1";

                            await _connection.ExecuteAsync(questionCodeQuery, new { QuestionCode = code, QuestionId = insertedQuestionId1 });
                        }
                        string insertedQuestionCode1 = string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string" ? code : request.QuestionCode;


                        var answer = await AnswerHandling(record.QuestionTypeId, record.AnswerMultipleChoiceCategories, insertedQuestionId1, insertedQuestionCode, record.Answersingleanswercategories);

                    }
                    if (data > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Question Added Successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                    }
               
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        private string GetParagraphById(ExcelWorksheet paragraphSheet, int paragraphId)
        {
            if (paragraphSheet == null) return null;

            var rowCount = paragraphSheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++) // Assuming the header is in row 1
            {
                if (Convert.ToInt32(paragraphSheet.Cells[row, 1].Text) == paragraphId) // Assuming ParagraphId is in column 1
                {
                    return paragraphSheet.Cells[row, 2].Text; // Assuming Paragraph text is in column 2
                }
            }

            return null; // Return null if no matching ParagraphId is found
        }
        private async Task<int> GetChapterIdByCode(string chapterCode)
        {
            string query = "SELECT TOP 1 ContentIndexId FROM tblContentIndexChapters WHERE ChapterCode = @ChapterCode AND IsActive = 1";
            return await _connection.QueryFirstOrDefaultAsync<int>(query, new { ChapterCode = chapterCode });
        }
        private async Task<int> GetTopicIdByCode(string topicCode)
        {
            string query = "SELECT TOP 1 ContentIndexId FROM tblContentIndexTopics WHERE TopicCode = @TopicCode AND IsActive = 1";
            return await _connection.QueryFirstOrDefaultAsync<int>(query, new { TopicCode = topicCode });
        }
        private async Task<int> GetSubTopicIdByCode(string subTopicCode)
        {
            string query = "SELECT TOP 1 ContInIdSubTopic FROM tblContentIndexSubTopics WHERE SubTopicCode = @SubTopicCode AND IsActive = 1";
            return await _connection.QueryFirstOrDefaultAsync<int>(query, new { SubTopicCode = subTopicCode });
        }
        private List<ParagraphQuestionDTO> GetChildQuestions(ExcelWorksheet worksheet, int rowCount,
      Dictionary<string, int> subjectDictionary, int EmployeeId, int ParagraphId)
        {
            var questions = new List<ParagraphQuestionDTO>();
            for (int row = 2; row <= rowCount; row++) // Skip header row
            {
                int questionTypeId = Convert.ToInt32(worksheet.Cells[row, 3].Text);
                var paragraphID = string.IsNullOrWhiteSpace(worksheet.Cells[row, 5].Text)
        ? 0
        : Convert.ToInt32(worksheet.Cells[row, 5].Text);
                if (paragraphID != ParagraphId)
                {
                    break;
                }
                int extraInfo = 0; // Assuming extra info column is just before courses
                int explanationCol = 0;
                for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                {
                    if (worksheet.Cells[1, col].Text.Equals("Explanation", StringComparison.OrdinalIgnoreCase))
                    {
                        explanationCol = col;
                        break;
                    }
                }
                for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                {
                    if (worksheet.Cells[1, col].Text.Equals("Extra Information", StringComparison.OrdinalIgnoreCase))
                    {
                        extraInfo = col;
                        break;
                    }
                }
                string explanation = explanationCol > 0 ? worksheet.Cells[row, explanationCol].Text : null;
                string extraInfoCol = extraInfo > 0 ? worksheet.Cells[row, extraInfo].Text : null;
                // Fetch the SubjectID from the row
                int subjectIDFromRow = Convert.ToInt32(worksheet.Cells[row, 2].Text);
                int validSubjectId = subjectDictionary.ContainsKey(subjectIDFromRow.ToString()) ? subjectIDFromRow : 0;
                // Subject ID validation - if it doesn't match, skip this record
                if (subjectIDFromRow != validSubjectId)
                {
                    // Log the skipped question if needed
                    Console.WriteLine($"Skipped question at row {row}: Subject ID {subjectIDFromRow} does not match {validSubjectId}.");
                    continue; // Skip to the next question
                }
                // Create the question DTO
                var question = new ParagraphQuestionDTO
                {
                    QuestionDescription = worksheet.Cells[row, 8].Text,
                    QuestionTypeId = Convert.ToInt32(worksheet.Cells[row, 1].Text),
                    subjectID = Convert.ToInt32(worksheet.Cells[row, 2].Text),
                    IndexTypeId = 0,
                    Explanation = explanation,
                    QuestionCode = "string",//string.IsNullOrEmpty(worksheet.Cells[row, 27].Text) ? null : worksheet.Cells[row, 27].Text,
                    ContentIndexId = 0,
                    AnswerMultipleChoiceCategories = GetAnswerMultipleChoiceCategories(worksheet, row),
                    Answersingleanswercategories = GetAnswerSingleAnswerCategories(worksheet, row, Convert.ToInt32(worksheet.Cells[row, 3].Text)),
                    IsActive = true,
                    IsConfigure = true,
                    EmployeeId = EmployeeId,
                    CategoryId = Convert.ToInt32(worksheet.Cells[row, 1].Text),
                    ExtraInformation = extraInfoCol,
                    Status = true,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = ""
                };

                // Add question to the list for bulk processing
                questions.Add(question);
            }
            return questions;
        }
        public async Task<ServiceResponse<string>> UploadQuestionsFromExcel(IFormFile file, int testSeriesId, int EmployeeId)
        {
            Dictionary<string, int> subjectDictionary = new Dictionary<string, int>();
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
                    var subjectSheet = package.Workbook.Worksheets["Subjects"];
                    LoadSubjectCodes(subjectSheet, subjectDictionary);
                    for (int row = 2; row <= rowCount; row++) // Skip header row
                    {

                        int questionTypeId = Convert.ToInt32(worksheet.Cells[row, 3].Text);
                        if (questionTypeId == 11) // Handle paragraph type
                        {
                            var paragraphIdPrevious = (row > 2) ? Convert.ToInt32(worksheet.Cells[row - 1, 6].Text) : 0;

                            var paragraphId = Convert.ToInt32(worksheet.Cells[row, 6].Text); // Assuming ParagraphId is in column 6

                            if (paragraphId != paragraphIdPrevious)
                            {
                                var paragraphSheet = package.Workbook.Worksheets["Paragraph"];
                                var paragraph = GetParagraphById(paragraphSheet, paragraphId);

                                if (string.IsNullOrEmpty(paragraph))
                                {
                                    return new ServiceResponse<string>(false, $"Paragraph not found for ParagraphId {paragraphId} at row {row}.", string.Empty, 400);
                                }
                                // Fetch the SubjectID from the row
                                int subjectIDFromRow = Convert.ToInt32(worksheet.Cells[row, 2].Text);
                                int validSubjectId = subjectDictionary.ContainsKey(subjectIDFromRow.ToString()) ? subjectIDFromRow : 0;
                                // Subject ID validation - if it doesn't match, skip this record
                                if (subjectIDFromRow != validSubjectId)
                                {
                                    // Log the skipped question if needed
                                    Console.WriteLine($"Skipped question at row {row}: Subject ID {subjectIDFromRow} does not match {validSubjectId}.");
                                    continue; // Skip to the next question
                                }
                                var qidCourses = new List<QIDCourse>
                        {
                            new QIDCourse
                            {
                                QIDCourseID = 0, // Assuming you want to set this later or handle it in the AddUpdateQuestion method
                                QID = 0, // Populate this as needed
                                QuestionCode = "string",
                                CourseID = 0,
                                LevelId = int.Parse(worksheet.Cells[row, 4].Text), // Set this based on your logic or fetch from another source
                                Status = true, // Set as needed
                                CreatedBy = "YourUsername", // Set the creator's username or similar info
                                CreatedDate = DateTime.UtcNow, // Use the current date and time
                                ModifiedBy = "YourUsername", // Set as needed
                                ModifiedDate = DateTime.UtcNow // Set as needed
                            }
                        };
                                var comprehensiveQuestionRequest = new ComprehensiveQuestionRequest
                                {
                                    Paragraph = paragraph,
                                    QuestionTypeId = questionTypeId,
                                    Status = true, // Set as required
                                    CategoryId = Convert.ToInt32(worksheet.Cells[row, 1].Text),
                                    CreatedBy = "YourUsername",
                                    CreatedOn = DateTime.UtcNow,
                                    subjectID = Convert.ToInt32(worksheet.Cells[row, 2].Text),
                                    EmployeeId = EmployeeId,
                                    IndexTypeId = 0, // Populate as required
                                    ContentIndexId = 0, // Populate as required
                                    IsRejected = false,
                                    IsApproved = false,
                                    QuestionCode = "string", // Generate or populate as required
                                    IsActive = true,
                                    IsConfigure = true,
                                    QIDCourses = qidCourses,
                                    Questions = GetChildQuestions(worksheet, rowCount, subjectDictionary, EmployeeId, paragraphId), // Populate child questions as needed
                                };

                                var comprehensiveResponse = await AddUpdateComprehensiveQuestion(comprehensiveQuestionRequest);
                                // Update the previousParagraphId to the current one after processing
                                paragraphIdPrevious = paragraphId;
                                if (!comprehensiveResponse.Success)
                                {
                                    return new ServiceResponse<string>(false, $"Failed to add/update comprehensive question at row {row}: {comprehensiveResponse.Message}", string.Empty, 500);
                                }
                            }
                            else
                            {
                                // Skip processing, as it's the same paragraph as the previous question
                                continue;
                            }
                        }
                        else
                        {
                            var qidCourses = new List<QIDCourse>
                        {
                            new QIDCourse
                            {
                                QIDCourseID = 0, // Assuming you want to set this later or handle it in the AddUpdateQuestion method
                                QID = 0, // Populate this as needed
                                QuestionCode = "string",
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
                                QuestionDescription = worksheet.Cells[row, 8].Text,
                                QuestionTypeId = int.Parse(worksheet.Cells[row, 3].Text),
                                subjectID = int.Parse(worksheet.Cells[row, 2].Text),
                                IndexTypeId = 0,
                                Explanation = string.IsNullOrEmpty(worksheet.Cells[row, 14].Text) ? null : worksheet.Cells[row, 14].Text,
                                QuestionCode = "string",
                                ContentIndexId = 0,
                                AnswerMultipleChoiceCategories = GetAnswerMultipleChoiceCategories(worksheet, row),
                                Answersingleanswercategories = GetAnswerSingleAnswerCategories(worksheet, row, int.Parse(worksheet.Cells[row, 3].Text)),
                                QIDCourses = qidCourses,
                                IsActive = true,
                                IsConfigure = false,
                                ExtraInformation = worksheet.Cells[row, 15].Text,
                                DifficultyLevel = int.Parse(worksheet.Cells[row, 4].Text),
                                EmployeeId = EmployeeId
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
            }
            foreach (int questionId in quesionsList)
            {
                TestSeriesQuestions testSeriesQuestion = new TestSeriesQuestions
                {
                    TestSeriesid = testSeriesId,
                    testseriesQuestionSectionid = 0,
                    QuestionCode = "",
                    Questionid = questionId,
                    Status = 1 // Assuming status is active or some default value
                };
                testSeriesQuestionsList.Add(testSeriesQuestion);
            }
            var quesMapping = await TestSeriesQuestionsMapping(testSeriesQuestionsList, testSeriesId, 0);
            string updateDownloadstatus = @"update tblTestSeries set DownloadStatusId = 3 where TestSeriesId = @TestSeriesId";
            await _connection.ExecuteAsync(updateDownloadstatus, new { TestSeriesId = testSeriesId });
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
                    IsConfigure = false,
                    DifficultyLevelId = request.DifficultyLevel
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
                  IsConfigure,
                  DifficultyLevelId
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
                  @IsActive, @IsConfigure, @DifficultyLevelId
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
                        if (questTypedata.Code.Trim() == "MCQ" || questTypedata.Code.Trim() == "TF" || questTypedata.Code.Trim() == "FB" || questTypedata.Code.Trim() == "MF" ||
                            questTypedata.Code.Trim() == "MAQ" || questTypedata.Code.Trim() == "MF2" || questTypedata.Code.Trim() == "AR" || questTypedata.Code.Trim() == "CT")
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
        private async Task<ServiceResponse<int>> AnswerHandling(int QuestionTypeId, List<DTOs.Requests.AnswerMultipleChoiceCategory>? multiAnswerRequest, int QuestionId, string QuestionCode, DTOs.Requests.Answersingleanswercategory singleAnswerRequest)
        {
            // Handle Answer mappings
            string getQuesType = @"SELECT * FROM tblQBQuestionType WHERE QuestionTypeID = @QuestionTypeID;";
            var questTypedata = await _connection.QueryFirstOrDefaultAsync<QuestionTypes>(getQuesType, new { QuestionTypeID = QuestionTypeId });

            int answer = 0;
            int Answerid = 0;

            // Check if the answer already exists in AnswerMaster
            string getAnswerQuery = @"SELECT Answerid FROM tblAnswerMaster WHERE Questionid = @Questionid;";
            Answerid = await _connection.QueryFirstOrDefaultAsync<int>(getAnswerQuery, new { Questionid = QuestionId });

            if (Answerid == 0)  // If no entry exists, insert a new one
            {
                string insertAnswerQuery = @"
        INSERT INTO [tblAnswerMaster] (Questionid, QuestionTypeid, QuestionCode)
        VALUES (@Questionid, @QuestionTypeid, @QuestionCode);
        SELECT CAST(SCOPE_IDENTITY() as int);";

                Answerid = await _connection.QuerySingleAsync<int>(insertAnswerQuery, new
                {
                    Questionid = QuestionId, // Set to 0 or remove if QuestionId is not required
                    QuestionTypeid = questTypedata?.QuestionTypeID,
                    QuestionCode = QuestionCode
                });
            }

            // If the question type supports multiple-choice or similar categories
            if (questTypedata != null)
            {
                if (questTypedata.Code.Trim() == "MCQ" || questTypedata.Code.Trim() == "TF" || questTypedata.Code.Trim() == "MF" ||
                    questTypedata.Code.Trim() == "MAQ" || questTypedata.Code.Trim() == "MF2" || questTypedata.Code.Trim() == "AR" || questTypedata.Code.Trim() == "CT")
                {
                    if (multiAnswerRequest != null)
                    {
                        // First, delete existing multiple-choice entries if present
                        string deleteMCQQuery = @"DELETE FROM tblAnswerMultipleChoiceCategory WHERE Answerid = @Answerid;";
                        await _connection.ExecuteAsync(deleteMCQQuery, new { Answerid });

                        // Insert new multiple-choice answers
                        foreach (var item in multiAnswerRequest)
                        {
                            item.Answerid = Answerid;
                        }
                        string insertMCQQuery = @"
                    INSERT INTO tblAnswerMultipleChoiceCategory
                    (Answerid, Answer, Iscorrect, Matchid) 
                    VALUES (@Answerid, @Answer, @Iscorrect, @Matchid);";
                        answer = await _connection.ExecuteAsync(insertMCQQuery, multiAnswerRequest);
                    }
                }
                else  // Handle single-answer category
                {
                    string sql = @"
                INSERT INTO tblAnswersingleanswercategory (Answerid, Answer)
                VALUES (@Answerid, @Answer);";

                    if (singleAnswerRequest != null)
                    {
                        // First, delete existing single-answer entries if present
                        string deleteSingleQuery = @"DELETE FROM tblAnswersingleanswercategory WHERE Answerid = @Answerid;";
                        await _connection.ExecuteAsync(deleteSingleQuery, new { Answerid });

                        // Insert new single-answer answers
                        singleAnswerRequest.Answerid = Answerid;
                        answer = await _connection.ExecuteAsync(sql, singleAnswerRequest);
                    }
                }
            }
            return new ServiceResponse<int>(true, string.Empty, answer, 200);
        }
        //get records
        private List<DTOs.Requests.AnswerMultipleChoiceCategory> GetAnswerMultipleChoiceCategories(ExcelWorksheet worksheet, int row)
        {
            var categories = new List<DTOs.Requests.AnswerMultipleChoiceCategory>();

            // Get the correct answer from cell 9
            var correctAnswer = worksheet.Cells[row, 9].Text; // Correct answer

            // Find the column with the header "Explanation" and stop before that
            int optionStartColumn = 10; // Assuming the options start from column 10
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
               // var answer = worksheet.Cells[row, 9].Text; // Single answer category in column 15
                                                           // Find the column with the header "Explanation" and stop before that
                int optionStartColumn = 10; // Assuming the options start from column 10
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
                string answer = explanationColumn > 0 ? worksheet.Cells[row, explanationColumn].Text : null;
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
        private void AddMasterDataSheets(ExcelPackage package, List<int> subjectIds, List<int> levelIds, List<int> QuestionTypes)
        {
            // Create worksheets for master data
            var subjectWorksheet = package.Workbook.Worksheets.Add("Subjects");
          //  var chapterWorksheet = package.Workbook.Worksheets.Add("Chapters");
          //  var topicWorksheet = package.Workbook.Worksheets.Add("Topics");
          //  var subTopicWorksheet = package.Workbook.Worksheets.Add("SubTopics");
            var difficultyLevelWorksheet = package.Workbook.Worksheets.Add("Difficulty Levels");
            var questionTypeWorksheet = package.Workbook.Worksheets.Add("Question Types");
            //   var coursesWorksheet = package.Workbook.Worksheets.Add("Courses");
            var paragraphWorksheet = package.Workbook.Worksheets.Add("Paragraph");


            paragraphWorksheet.Cells[1, 1].Value = "ParagraphId";
            paragraphWorksheet.Cells[1, 2].Value = "Paragraph";

            // Set headers for each worksheet
            subjectWorksheet.Cells[1, 1].Value = "SubjectId";
            subjectWorksheet.Cells[1, 2].Value = "SubjectCode";
            subjectWorksheet.Cells[1, 3].Value = "SubjectName";
      

            // Initialize row counters
            int subjectRow = 2, chapterRow = 2, topicRow = 2, subTopicRow = 2;

            // Loop through each subjectId and populate data for Subjects, Chapters, Topics, and SubTopics
            foreach (var subjectId in subjectIds)
            {
                // Fetch subjects based on the current subjectId
                var subjects = GetSubjects().Where(s => s.SubjectId == subjectId);
                foreach (var subject in subjects)
                {
                    subjectWorksheet.Cells[subjectRow, 1].Value = subject.SubjectId;
                    subjectWorksheet.Cells[subjectRow, 2].Value = subject.SubjectCode;
                    subjectWorksheet.Cells[subjectRow, 3].Value = subject.SubjectName;
                    subjectRow++;
                }
            }

            // Populate data for Difficulty Levels
            difficultyLevelWorksheet.Cells[1, 1].Value = "LevelId";
            difficultyLevelWorksheet.Cells[1, 2].Value = "LevelCode";
            difficultyLevelWorksheet.Cells[1, 3].Value = "LevelName";
            int levelRow = 2;
            foreach (var levelId in levelIds)
            {
                var difficultyLevels = GetDifficultyLevels().Where(l => l.LevelId == levelId);
       
                foreach (var level in difficultyLevels)
                {
                    difficultyLevelWorksheet.Cells[levelRow, 1].Value = level.LevelId;
                    difficultyLevelWorksheet.Cells[levelRow, 2].Value = level.LevelCode;
                    difficultyLevelWorksheet.Cells[levelRow, 3].Value = level.LevelName;

                    levelRow++;
                }
            }
            // Populate data for Question Types
            questionTypeWorksheet.Cells[1, 1].Value = "QuestionTypeID";
            questionTypeWorksheet.Cells[1, 2].Value = "QuestionType";
            int typeRow = 2;
            foreach (var typeId in QuestionTypes)
            {
                var questionTypes = GetQuestionTypes().Where(t => t.QuestionTypeID == typeId);

                foreach (var type in questionTypes)
                {
                    questionTypeWorksheet.Cells[typeRow, 1].Value = type.QuestionTypeID;
                    questionTypeWorksheet.Cells[typeRow, 2].Value = type.QuestionType;
                    typeRow++;
                }
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
            paragraphWorksheet.Cells[paragraphWorksheet.Dimension.Address].AutoFitColumns();
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
        private List<TestSeriesQuestions> GetListOfTestSeriesQuestion(int TestSeriesId)
        {
            // Query to join tbltestseriesQuestions with tblQuestion to fetch QuestionDescription
            string query = @"
    SELECT 
        tsq.testseriesquestionsid,
        tsq.TestSeriesid,
        tsq.Questionid,
        tsq.DisplayOrder,
        tsq.Status,
        tsq.testseriesQuestionSectionid,
        tsq.QuestionCode,
        q.QuestionDescription,
        q.QuestionTypeId
    FROM 
        tbltestseriesQuestions tsq
    JOIN 
        tblQuestion q ON tsq.Questionid = q.QuestionId
    WHERE 
        tsq.TestSeriesid = @TestSeriesid";

            // Fetch the basic test series questions
            var questions = _connection.Query<TestSeriesQuestions>(query, new { TestSeriesid = TestSeriesId }).ToList();

            // Loop through each question and fetch corresponding answers
            foreach (var question in questions)
            {
                // Query to fetch AnswerMaster details
                string answerMasterQuery = @"
        SELECT 
            Answerid, 
            Questionid, 
            QuestionTypeid 
        FROM 
            tblAnswerMaster 
        WHERE 
            Questionid = @Questionid";

                var answerMaster = _connection.QueryFirstOrDefault(answerMasterQuery, new { Questionid = question.Questionid });
                string getQuesType = @"SELECT * FROM tblQBQuestionType WHERE QuestionTypeID = @QuestionTypeID;";
                var questTypedata =  _connection.QueryFirstOrDefault<QuestionTypes>(getQuesType, new { QuestionTypeID = question.QuestionTypeId });
                if (answerMaster != null)
                {
                    // Handle Multiple Choice Questions (QuestionTypeId = X, assuming a value for multiple choice)
                    if (questTypedata.Code.Trim() == "MCQ" || questTypedata.Code.Trim() == "TF" || questTypedata.Code.Trim() == "MT" ||
                                questTypedata.Code.Trim() == "MAQ" || questTypedata.Code.Trim() == "MT2" || questTypedata.Code.Trim() == "AR" || questTypedata.Code.Trim() == "C")  // Replace X with actual ID representing multiple-choice questions
                    {
                        string multipleChoiceQuery = @"
                SELECT 
                    Answermultiplechoicecategoryid,
                    Answerid,
                    Answer,
                    Iscorrect,
                    Matchid
                FROM 
                    tblAnswerMultipleChoiceCategory 
                WHERE 
                    Answerid = @Answerid";

                        var multipleChoiceCategories = _connection.Query<AnswerMultipleChoiceCategorys>(multipleChoiceQuery, new { Answerid = answerMaster.Answerid }).ToList();

                        question.AnswerMultipleChoiceCategories = multipleChoiceCategories;
                    }
                    // Handle Single Answer Questions (QuestionTypeId = Y, assuming a value for single answer)
                    else  // Replace Y with actual ID representing single-answer questions
                    {
                        string singleAnswerQuery = @"
                SELECT 
                    Answersingleanswercategoryid,
                    Answerid,
                    Answer
                FROM 
                    tblAnswersingleanswercategory 
                WHERE 
                    Answerid = @Answerid";

                        var singleAnswerCategory = _connection.QueryFirstOrDefault<Answersingleanswercategorys>(singleAnswerQuery, new { Answerid = answerMaster.Answerid });

                        question.Answersingleanswercategories = singleAnswerCategory;
                    }
                }
            }

            return questions;
        }
        //private List<TestSeriesQuestions> GetListOfTestSeriesQuestion(int TestSeriesId)
        //{
        //    string query = "SELECT * FROM tbltestseriesQuestions WHERE [TestSeriesid] = @TestSeriesid";

        //    // Execute the SQL query with the SOTDID parameter
        //    var data = _connection.Query<TestSeriesQuestions>(query, new { TestSeriesid = TestSeriesId });
        //    return data != null ? data.AsList() : [];
        //}
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
}