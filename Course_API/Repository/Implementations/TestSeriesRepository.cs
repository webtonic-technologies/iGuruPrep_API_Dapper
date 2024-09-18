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
                        RepeatExamStarttime , RepeatExamResulttimeId, IsAdmin
                    ) 
                    VALUES 
                    (
                        @TestPatternName, @Duration, @Status, @APID, @TotalNoOfQuestions, @ExamTypeID,
                        @MethodofAddingType, @StartDate, @StartTime, @ResultDate, @ResultTime, 
                        @EmployeeID, @NameOfExam, @RepeatedExams, @TypeOfTestSeries, 
                        @createdon, @createdby, @RepeatExamStartDate , @RepeatExamEndDate ,
                        @RepeatExamStarttime , @RepeatExamResulttimeId, @IsAdmin
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
                        request.IsAdmin
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
                        IsAdmin = @IsAdmin
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
                        request.IsAdmin
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
            JOIN tblExamType ttt ON ts.ExamTypeID = ttt.ExamTypeID
            JOIN tblTestSeriesResultTime rt ON ts.RepeatExamResulttimeId = rt.ResultTimeId
            WHERE ts.TestSeriesId = @TestSeriesId";

                var testSeries = await _connection.QueryFirstOrDefaultAsync<TestSeriesResponseDTO>(query, new { TestSeriesId });

                if (testSeries == null)
                {
                    return new ServiceResponse<TestSeriesResponseDTO>(false, "Test Series not found", new TestSeriesResponseDTO(), 404);
                }

                // Fetch related data
                var testSeriesBoards =  GetListOfTestSeriesBoards(TestSeriesId);
                var testSeriesClasses =  GetListOfTestSeriesClasses(TestSeriesId);
                var testSeriesCourses =  GetListOfTestSeriesCourse(TestSeriesId);
                var testSeriesSubjects =  GetListOfTestSeriesSubjects(TestSeriesId);
                var testSeriesContentIndexes =  GetListOfTestSeriesSubjectIndex(TestSeriesId);
                var testSeriesQuestionsSections =  GetTestSeriesQuestionSection(TestSeriesId);
                var testSeriesInstructions =  GetListOfTestSeriesInstructions(TestSeriesId);

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

                return new ServiceResponse<TestSeriesResponseDTO>(true, "Success", testSeries, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<TestSeriesResponseDTO>(false, ex.Message, new TestSeriesResponseDTO(), 500);
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
JOIN tblExamType ttt ON ts.ExamTypeID = ttt.ExamTypeID
LEFT JOIN tblTestSeriesClass tc ON ts.TestSeriesId = tc.TestSeriesId
LEFT JOIN tblTestSeriesCourse tco ON ts.TestSeriesId = tco.TestSeriesId
LEFT JOIN tblTestSeriesBoard tb ON ts.TestSeriesId = tb.TestSeriesId
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
                    // Calculate RepeatExamEndTime using RepeatExamStarttime and Duration
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
                        DateTime startDateTime = testSeries.StartDate.Value.Add(TimeSpan.Parse(testSeries.StartTime));
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
        //public async Task<ServiceResponse<List<TestSeriesResponseDTO>>> GetTestSeriesList(TestSeriesListRequest request)
        //{
        //    try
        //    {
        //        // Construct the SQL query with parameters
        //        var query = @"
        //SELECT 
        //    ts.TestSeriesId,
        //    ts.TestPatternName,
        //    ts.Duration,
        //    ts.Status,
        //    ts.APID,
        //    ap.APName AS APName,
        //    ts.TotalNoOfQuestions,
        //    ts.MethodofAddingType,
        //    ts.StartDate,
        //    ts.StartTime,
        //    ts.ResultDate,
        //    ts.ResultTime,
        //    ts.EmployeeID,
        //    emp.EmpFirstName AS EmpFirstName,
        //    ts.NameOfExam,
        //    ts.RepeatedExams,
        //    ts.TypeOfTestSeries,
        //    tts.TestSeriesName AS TypeOfTestSeriesName,
        //    ts.ExamTypeID,
        //    ttt.ExamTypeName AS ExamTypeName,
        //    ts.createdon,
        //    ts.createdby,
        //    ts.modifiedon,
        //    ts.modifiedby,
        //    ts.RepeatExamStartDate,
        //    ts.RepeatExamEndDate,
        //    ts.RepeatExamStarttime,
        //    ts.RepeatExamResulttimeId,
        //    rt.ResultTime AS RepeatedExamResultTime
        //FROM tblTestSeries ts
        //JOIN tblCategory ap ON ts.APID = ap.APID
        //JOIN tblEmployee emp ON ts.EmployeeID = emp.EmployeeID
        //JOIN tblTypeOfTestSeries tts ON ts.TypeOfTestSeries = tts.TTSId
        //JOIN tblExamType ttt ON ts.ExamTypeID = ttt.ExamTypeID
        //JOIN tblTestSeriesResultTime rt ON ts.RepeatExamResulttimeId = rt.ResultTimeId
        //LEFT JOIN tblTestSeriesClass tc ON ts.TestSeriesId = tc.TestSeriesId
        //LEFT JOIN tblTestSeriesCourse tco ON ts.TestSeriesId = tco.TestSeriesId
        //LEFT JOIN tblTestSeriesBoard tb ON ts.TestSeriesId = tb.TestSeriesId
        //WHERE 1=1";

        //        // Apply filters dynamically
        //        if (request.APId > 0)
        //        {
        //            query += " AND ts.APID = @APId";
        //        }
        //        if (request.ClassId > 0)
        //        {
        //            query += " AND tc.ClassId = @ClassId";
        //        }
        //        if (request.CourseId > 0)
        //        {
        //            query += " AND tco.CourseId = @CourseId";
        //        }
        //        if (request.BoardId > 0)
        //        {
        //            query += " AND tb.BoardId = @BoardId";
        //        }
        //        if (request.ExamTypeId > 0)
        //        {
        //            query += " AND ts.ExamTypeID = @ExamTypeId";
        //        }
        //        if (request.TypeOfTestSeries > 0)
        //        {
        //            query += " AND ts.TypeOfTestSeries = @TypeOfTestSeries";
        //        }
        //        if (!string.IsNullOrEmpty(request.ExamStatus))
        //        {
        //            query += " AND (@ExamStatus IS NULL OR " +
        //                      "(ts.RepeatedExams = 1 AND ts.RepeatExamStartDate <= @Date AND ts.RepeatExamEndDate >= @Date) OR " +
        //                      "(ts.RepeatedExams = 0 AND ts.StartDate <= @Date AND DATEADD(MINUTE, CAST(ts.Duration AS INT), ts.StartDate) >= @Date))";
        //        }

        //        // Prepare the parameters for the query
        //        var parameters = new
        //        {
        //            APId = request.APId == 0 ? (int?)null : request.APId,
        //            ClassId = request.ClassId == 0 ? (int?)null : request.ClassId,
        //            CourseId = request.CourseId == 0 ? (int?)null : request.CourseId,
        //            BoardId = request.BoardId == 0 ? (int?)null : request.BoardId,
        //            ExamTypeId = request.ExamTypeId == 0 ? (int?)null : request.ExamTypeId,
        //            TypeOfTestSeries = request.TypeOfTestSeries == 0 ? (int?)null : request.TypeOfTestSeries,
        //            ExamStatus = string.IsNullOrEmpty(request.ExamStatus) ? (string)null : request.ExamStatus,
        //            Date = request.Date
        //        };

        //        // Execute the query
        //        var testSeriesList = await _connection.QueryAsync<TestSeriesResponseDTO>(query, parameters);

        //        if (testSeriesList == null || !testSeriesList.Any())
        //        {
        //            return new ServiceResponse<List<TestSeriesResponseDTO>>(true, "No test series found", new List<TestSeriesResponseDTO>(), 200);
        //        }
        //        // Fetch related data for each test series
        //        foreach (var testSeries in testSeriesList)
        //        {
        //            // Determine the exam status based on repeated exams
        //            if (testSeries.RepeatedExams)
        //            {
        //                // Current date and time
        //                DateTime currentDateTime = DateTime.Now;
        //                // Exam start and end times
        //                TimeSpan examStartTime = TimeSpan.Parse(testSeries.RepeatExamStarttime);
        //                TimeSpan examEndTime = TimeSpan.Parse(testSeries.repeatedexamen);

        //                // Exam period start and end dates
        //                DateTime repeatExamStartDate = testSeries.RepeatExamStartDate.Value.Date;
        //                DateTime repeatExamEndDate = testSeries.RepeatExamEndDate.Value.Date;

        //                // Exam start and end DateTime for the current day
        //                DateTime dailyExamStartDateTime = repeatExamStartDate.Add(examStartTime);
        //                DateTime dailyExamEndDateTime = repeatExamStartDate.Add(examEndTime);

        //                if (currentDateTime < dailyExamStartDateTime)
        //                {
        //                    testSeries.ExamStatus = "Upcoming";
        //                }
        //                else if (currentDateTime >= dailyExamStartDateTime && currentDateTime <= dailyExamEndDateTime)
        //                {
        //                    testSeries.ExamStatus = "Ongoing";
        //                }
        //                else if (currentDateTime > dailyExamEndDateTime && currentDateTime < repeatExamEndDate.AddDays(1).Add(examStartTime))
        //                {
        //                    testSeries.ExamStatus = "Upcoming";
        //                }
        //                else if (currentDateTime >= repeatExamEndDate.Add(examEndTime))
        //                {
        //                    testSeries.ExamStatus = "Completed";
        //                }
        //            }
        //            else
        //            {
        //                DateTime startDateTime = testSeries.StartDate.Value.Add(TimeSpan.Parse(testSeries.StartTime));
        //                if (DateTime.Now < startDateTime)
        //                {
        //                    testSeries.ExamStatus = "Upcoming";
        //                }
        //                else if (DateTime.Now >= startDateTime && DateTime.Now <= testSeries.ResultDate)
        //                {
        //                    testSeries.ExamStatus = "Ongoing";
        //                }
        //                else
        //                {
        //                    testSeries.ExamStatus = "Completed";
        //                }
        //            }

        //            // Fetch related data
        //            var testSeriesBoards = GetListOfTestSeriesBoards(testSeries.TestSeriesId);
        //            var testSeriesClasses = GetListOfTestSeriesClasses(testSeries.TestSeriesId);
        //            var testSeriesCourses = GetListOfTestSeriesCourse(testSeries.TestSeriesId);
        //            var testSeriesSubjects = GetListOfTestSeriesSubjects(testSeries.TestSeriesId);
        //            var testSeriesContentIndexes = GetListOfTestSeriesSubjectIndex(testSeries.TestSeriesId);
        //            var testSeriesQuestionsSections = GetTestSeriesQuestionSection(testSeries.TestSeriesId);
        //            var testSeriesInstructions = GetListOfTestSeriesInstructions(testSeries.TestSeriesId);

        //            // Initialize the SubjectDetails list
        //            var testSeriesSubjectDetailsList = new List<TestSeriesSubjectDetails>();

        //            // Populate TestSeriesSubjectDetails with content indexes and questions section
        //            foreach (var subject in testSeriesSubjects)
        //            {
        //                var subjectContentIndexes = testSeriesContentIndexes
        //                    .Where(ci => ci.SubjectId == subject.SubjectID)
        //                    .ToList();

        //                var subjectQuestionsSections = testSeriesQuestionsSections
        //                    .Where(qs => qs.SubjectId == subject.SubjectID)
        //                    .ToList();

        //                var subjectDetails = new TestSeriesSubjectDetails
        //                {
        //                    SubjectID = subject.SubjectID,
        //                    SubjectName = subject.SubjectName,
        //                    TestSeriesContentIndexes = subjectContentIndexes,
        //                    TestSeriesQuestionsSection = subjectQuestionsSections
        //                };

        //                testSeriesSubjectDetailsList.Add(subjectDetails);
        //            }

        //            // Map the fetched data to the TestSeriesResponseDTO
        //            testSeries.TestSeriesBoard = testSeriesBoards;
        //            testSeries.TestSeriesClasses = testSeriesClasses;
        //            testSeries.TestSeriesCourses = testSeriesCourses;
        //            testSeries.TestSeriesSubjectDetails = testSeriesSubjectDetailsList; // Populate SubjectDetails
        //            testSeries.TestSeriesInstruction = testSeriesInstructions;

        //            // Fetch TestSeriesQuestions based on TestSeriesQuestionsSection
        //            if (testSeriesQuestionsSections != null && testSeriesQuestionsSections.Any())
        //            {
        //                testSeries.TestSeriesQuestions = new List<TestSeriesQuestions>();
        //                foreach (var section in testSeriesQuestionsSections)
        //                {
        //                    var questions = GetListOfTestSeriesQuestion(section.testseriesQuestionSectionid);
        //                    if (questions != null)
        //                    {
        //                        testSeries.TestSeriesQuestions.AddRange(questions);
        //                    }
        //                }
        //            }
        //        }
        //        //// Fetch related data for each test series
        //        //foreach (var testSeries in testSeriesList)
        //        //{
        //        //    // Determine the exam status based on repeated exams
        //        //    if (testSeries.RepeatedExams)
        //        //    {
        //        //        if (DateTime.Now < testSeries.RepeatExamStartDate)
        //        //        {
        //        //            testSeries.ExamStatus = "Upcoming";
        //        //        }
        //        //        else if (DateTime.Now >= testSeries.RepeatExamStartDate && DateTime.Now <= testSeries.RepeatExamEndDate)
        //        //        {
        //        //            testSeries.ExamStatus = "Ongoing";
        //        //        }
        //        //        else
        //        //        {
        //        //            testSeries.ExamStatus = "Completed";
        //        //        }
        //        //    }
        //        //    else
        //        //    {
        //        //        DateTime startDateTime = testSeries.StartDate.Value.Add(TimeSpan.Parse(testSeries.StartTime));
        //        //        if (DateTime.Now < startDateTime)
        //        //        {
        //        //            testSeries.ExamStatus = "Upcoming";
        //        //        }
        //        //        else if (DateTime.Now >= startDateTime && DateTime.Now <= testSeries.ResultDate)
        //        //        {
        //        //            testSeries.ExamStatus = "Ongoing";
        //        //        }
        //        //        else
        //        //        {
        //        //            testSeries.ExamStatus = "Completed";
        //        //        }
        //        //    }

        //        //    // Fetch related data
        //        //    var testSeriesBoards = GetListOfTestSeriesBoards(testSeries.TestSeriesId);
        //        //    var testSeriesClasses = GetListOfTestSeriesClasses(testSeries.TestSeriesId);
        //        //    var testSeriesCourses = GetListOfTestSeriesCourse(testSeries.TestSeriesId);
        //        //    var testSeriesSubjects = GetListOfTestSeriesSubjects(testSeries.TestSeriesId);
        //        //    var testSeriesContentIndexes = GetListOfTestSeriesSubjectIndex(testSeries.TestSeriesId);
        //        //    var testSeriesQuestionsSections = GetTestSeriesQuestionSection(testSeries.TestSeriesId);
        //        //    var testSeriesInstructions = GetListOfTestSeriesInstructions(testSeries.TestSeriesId);

        //        //    // Initialize the SubjectDetails list
        //        //    var testSeriesSubjectDetailsList = new List<TestSeriesSubjectDetails>();

        //        //    // Populate TestSeriesSubjectDetails with content indexes and questions section
        //        //    foreach (var subject in testSeriesSubjects)
        //        //    {
        //        //        var subjectContentIndexes = testSeriesContentIndexes
        //        //            .Where(ci => ci.SubjectId == subject.SubjectID)
        //        //            .ToList();

        //        //        var subjectQuestionsSections = testSeriesQuestionsSections
        //        //            .Where(qs => qs.SubjectId == subject.SubjectID)
        //        //            .ToList();

        //        //        var subjectDetails = new TestSeriesSubjectDetails
        //        //        {
        //        //            SubjectID = subject.SubjectID,
        //        //            SubjectName = subject.SubjectName,
        //        //            TestSeriesContentIndexes = subjectContentIndexes,
        //        //            TestSeriesQuestionsSection = subjectQuestionsSections
        //        //        };

        //        //        testSeriesSubjectDetailsList.Add(subjectDetails);
        //        //    }

        //        //    // Map the fetched data to the TestSeriesResponseDTO
        //        //    testSeries.TestSeriesBoard = testSeriesBoards;
        //        //    testSeries.TestSeriesClasses = testSeriesClasses;
        //        //    testSeries.TestSeriesCourses = testSeriesCourses;
        //        //    testSeries.TestSeriesSubjectDetails = testSeriesSubjectDetailsList; // Populate SubjectDetails
        //        //    testSeries.TestSeriesInstruction = testSeriesInstructions;

        //        //    // Fetch TestSeriesQuestions based on TestSeriesQuestionsSection
        //        //    if (testSeriesQuestionsSections != null && testSeriesQuestionsSections.Any())
        //        //    {
        //        //        testSeries.TestSeriesQuestions = new List<TestSeriesQuestions>();
        //        //        foreach (var section in testSeriesQuestionsSections)
        //        //        {
        //        //            var questions = GetListOfTestSeriesQuestion(section.testseriesQuestionSectionid);
        //        //            if (questions != null)
        //        //            {
        //        //                testSeries.TestSeriesQuestions.AddRange(questions);
        //        //            }
        //        //        }
        //        //    }
        //        //}

        //        return new ServiceResponse<List<TestSeriesResponseDTO>>(true, "Success", testSeriesList.ToList(), 200);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<List<TestSeriesResponseDTO>>(false, ex.Message, new List<TestSeriesResponseDTO>(), 500);
        //    }
        //}
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
        public async Task<ServiceResponse<string>> TestSeriesInstructionsMapping(List<TestSeriesInstructions> request, int TestSeriesId)
        {
            foreach (var data in request)
            {
                data.TestSeriesID = TestSeriesId;
            }
            string query = "SELECT COUNT(*) FROM [tblTestInstructions] WHERE [TestSeriesID] = @TestSeriesId";
            int count =  await _connection.QueryFirstOrDefaultAsync<int>(query, new { TestSeriesId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblTestInstructions] WHERE [TestSeriesID] = @TestSeriesId;";
                var rowsAffected = _connection.Execute(deleteDuery, new { TestSeriesId });
                if (rowsAffected > 0)
                {
                    string insertQuery = @"
                    INSERT INTO tblTestInstructions (Instructions,TestSeriesID)
                    VALUES (@Instructions,@TestSeriesID);";
                    var valuesInserted = _connection.ExecuteAsync(insertQuery, request);
                    return new ServiceResponse<string>(true, "operation successful", "values added successfully", 200);
                }
                else
                {
                    return new ServiceResponse<string>(false, "operation failed", string.Empty, 200);
                }
            }
            else
            {
                string insertQuery = @"
                    INSERT INTO tblTestInstructions (Instructions,TestSeriesID)
                    VALUES (@Instructions,@TestSeriesID);";
                var valuesInserted = _connection.ExecuteAsync(insertQuery, request);
                return new ServiceResponse<string>(true, "operation successful", "values added successfully", 200);
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
                // Step 2: Fetch TotalNoofQuestions from tbltestseriesQuestionSection for the given sectionId
                string getTotalQuestionsQuery = "SELECT TotalNoofQuestions FROM [tbltestseriesQuestionSection] WHERE [TestSeriesid] = @TestSeriesId AND [testseriesQuestionSectionid] = @testseriesQuestionSectionid";
                int totalNoofQuestions = await _connection.QueryFirstOrDefaultAsync<int>(getTotalQuestionsQuery, new { TestSeriesId, testseriesQuestionSectionid = sectionId });

                // Step 3: Check if the count of request exceeds the total allowed questions
                if (request.Count > totalNoofQuestions)
                {
                    return new ServiceResponse<string>(false, "operation failed", $"Number of questions exceeds the allowed limit of {totalNoofQuestions} for this section.", 400);
                }

                // Step 4: Check if there are existing questions for this section
                string query = "SELECT COUNT(*) FROM [tbltestseriesQuestions] WHERE [testseriesQuestionSectionid] = @testseriesQuestionSectionid";
                int count = await _connection.QueryFirstOrDefaultAsync<int>(query, new { testseriesQuestionSectionid = sectionId });

                if (count > 0)
                {
                    // Step 5: Delete existing questions if any
                    var deleteQuery = @"DELETE FROM [tbltestseriesQuestions] WHERE [testseriesQuestionSectionid] = @testseriesQuestionSectionid;";
                    await _connection.ExecuteAsync(deleteQuery, new { testseriesQuestionSectionid = sectionId });
                }

                // Step 6: Insert new questions
                string insertQuery = @"
            INSERT INTO tbltestseriesQuestions (TestSeriesid, Questionid, DisplayOrder, Status, testseriesQuestionSectionid) 
            VALUES (@TestSeriesid, @Questionid, @DisplayOrder, @Status, @testseriesQuestionSectionid);";
                var valuesInserted = await _connection.ExecuteAsync(insertQuery, request);

                return new ServiceResponse<string>(true, "operation successful", "Data added successfully", 200);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating test series questions", ex);
            }
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuestionsList(GetAllQuestionListRequest request)
        {
            try
            {
                // Define the SQL query with additional filters, including DifficultyLevelId
                string sql = @"
                SELECT 
                    q.QuestionId, q.QuestionDescription, q.QuestionFormula, q.QuestionTypeId, q.ApprovedStatus, 
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
                LEFT JOIN tblQIDCourse qc ON q.QuestionCode = qc.QuestionCode
                WHERE q.subjectID = @Subjectid
                  AND (@IndexTypeId = 0 OR q.IndexTypeId = @IndexTypeId)
                  AND (@ContentId = 0 OR q.ContentIndexId = @ContentId)
                  AND (@QuestionTypeId = 0 OR q.QuestionTypeId = @QuestionTypeId)
                  AND (@DifficultyLevelId = 0 OR qc.LevelId = @DifficultyLevelId)
                  AND q.IsLive = 1";

                // Execute the query and retrieve the questions
                var questions = await _connection.QueryAsync<QuestionResponseDTO>(sql, new
                {
                    Subjectid = request.Subjectid,
                    IndexTypeId = request.IndexTypeId,
                    ContentId = request.ContentId,
                    QuestionTypeId = request.QuestionTypeId,
                    DifficultyLevelId = request.DifficultyLevelId
                });

                // If no questions found, return empty response
                if (!questions.Any())
                {
                    return new ServiceResponse<List<QuestionResponseDTO>>(true, "Operation Successful", new List<QuestionResponseDTO>(), 200);
                }

                return new ServiceResponse<List<QuestionResponseDTO>>(true, "Questions retrieved successfully", questions.ToList(), 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(false, ex.Message, new List<QuestionResponseDTO>(), 500);
            }
        }
        //public async Task<ServiceResponse<string>> TestSeriesQuestionsMapping(List<TestSeriesQuestions> request, int TestSeriesId, int sectionId)
        //{
        //    foreach (var data in request)
        //    {
        //        data.TestSeriesid = TestSeriesId;
        //        data.testseriesQuestionSectionid = sectionId;
        //    }
        //    string query = "SELECT COUNT(*) FROM [tbltestseriesQuestions] WHERE [testseriesQuestionSectionid] = @testseriesQuestionSectionid";
        //    int count = await _connection.QueryFirstOrDefaultAsync<int>(query, new { testseriesQuestionSectionid = sectionId });
        //    if (count > 0)
        //    {
        //        var deleteDuery = @"DELETE FROM [tbltestseriesQuestions] WHERE [testseriesQuestionSectionid] = @testseriesQuestionSectionid;";
        //        var rowsAffected = await _connection.ExecuteAsync(deleteDuery, new { testseriesQuestionSectionid = sectionId });
        //        if (rowsAffected > 0)
        //        {
        //            string insertQuery = @"
        //            INSERT INTO tbltestseriesQuestions (TestSeriesid, Questionid, DisplayOrder, Status, testseriesQuestionSectionid) 
        //            VALUES (@TestSeriesid, @Questionid, @DisplayOrder, @Status, @testseriesQuestionSectionid);";
        //            var valuesInserted = await _connection.ExecuteAsync(insertQuery, request);
        //            return new ServiceResponse<string>(true, "operation successful", "Data added successfully", 200);
        //        }
        //        else
        //        {
        //            return new ServiceResponse<string>(false, "operation failed", string.Empty, 500);
        //        }
        //    }
        //    else
        //    {
        //        string insertQuery = @"
        //            INSERT INTO tbltestseriesQuestions (TestSeriesid, Questionid, DisplayOrder, Status, testseriesQuestionSectionid) 
        //            VALUES (@TestSeriesid, @Questionid, @DisplayOrder, @Status, @testseriesQuestionSectionid);";
        //        var valuesInserted = _connection.Execute(insertQuery, request);
        //        return new ServiceResponse<string>(true, "operation successful", "Data added successfully", 200);
        //    }
        //}
        //    public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuestionsList(GetAllQuestionListRequest request)
        //    {
        //        try
        //        {
        //            string sql = @"
        //SELECT 
        //    q.QuestionId, q.QuestionDescription, q.QuestionFormula, q.QuestionTypeId, q.ApprovedStatus, 
        //    q.ApprovedBy, q.ReasonNote, q.Status, q.CreatedBy, q.CreatedOn, q.ModifiedBy, q.ModifiedOn, 
        //    q.Verified, q.courseid, c.CourseName, q.boardid, b.BoardName, q.classid, cl.ClassName, 
        //    q.subjectID, s.SubjectName, q.ExamTypeId, e.ExamTypeName, q.EmployeeId, emp.EmpFirstName as EmployeeName, 
        //    q.Rejectedby, q.RejectedReason, q.IndexTypeId, it.IndexType as IndexTypeName, q.ContentIndexId,
        //    CASE 
        //        WHEN q.IndexTypeId = 1 THEN ci.ContentName_Chapter
        //        WHEN q.IndexTypeId = 2 THEN ct.ContentName_Topic
        //        WHEN q.IndexTypeId = 3 THEN cst.ContentName_SubTopic
        //    END AS ContentIndexName
        //FROM tblQuestion q
        //LEFT JOIN tblCourse c ON q.courseid = c.CourseId
        //LEFT JOIN tblBoard b ON q.boardid = b.BoardId
        //LEFT JOIN tblClass cl ON q.classid = cl.ClassId
        //LEFT JOIN tblSubject s ON q.subjectID = s.SubjectId
        //LEFT JOIN tblExamType e ON q.ExamTypeId = e.ExamTypeId
        //LEFT JOIN tblEmployee emp ON q.EmployeeId = emp.EmployeeId
        //LEFT JOIN tblQBIndexType it ON q.IndexTypeId = it.IndexId
        //LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
        //LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
        //LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
        //WHERE q.subjectID = @Subjectid";


        //            // Execute the query and retrieve the questions
        //            var questions = await _connection.QueryAsync<QuestionResponseDTO>(sql, new { Subjectid = request.Subjectid });

        //            // If no questions found, return empty response
        //            if (!questions.Any())
        //            {

        //                return new ServiceResponse<List<QuestionResponseDTO>>(true, "Operation Successful", [], 200);
        //            }

        //            return new ServiceResponse<List<QuestionResponseDTO>> (true, "Questions retrieved successfully", questions.ToList(), 200);
        //        }
        //        catch (Exception ex)
        //        {
        //            return new ServiceResponse<List<QuestionResponseDTO>> (false, ex.Message, [], 500);
        //        }
        //    }
        public async Task<ServiceResponse<List<ContentIndexResponses>>> GetSyllabusDetailsBySubject(SyllabusDetailsRequest request)
        {
            try
            {
                // SQL Query
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
                    request.APId,
                    request.BoardId,
                    request.ClassId,
                    request.CourseId,
                    request.SubjectId
                });

                // Process the results to create a hierarchical structure
                var contentIndexResponse = new List<ContentIndexResponses>();

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
                // Define the SQL query to fetch all relevant questions based on the request criteria
                string query = @"
                SELECT 
                    [QuestionId],
                    [QuestionDescription],
                    [QuestionFormula],
                    [QuestionTypeId],
                    [ApprovedStatus],
                    [ApprovedBy],
                    [ReasonNote],
                    [Status],
                    [CreatedBy],
                    [CreatedOn],
                    [ModifiedBy],
                    [ModifiedOn],
                    [Verified],
                    [courseid],
                    [boardid],
                    [classid],
                    [subjectID],
                    [Rejectedby],
                    [RejectedReason],
                    [IndexTypeId],
                    [ContentIndexId],
                    [EmployeeId],
                    [ExamTypeId],
                    [IsActive]
                FROM 
                    [tblQuestion]
                WHERE 
                    IsActive = 1 
                    AND IsRejected = 0 
                    AND IsLive = 1
                    AND (BoardId = @BoardId OR @BoardId = 0)
                    AND (ClassId = @ClassId OR @ClassId = 0)
                    AND (CourseId = @CourseId OR @CourseId = 0)
                    AND (ExamTypeId = @ExamTypeId OR @ExamTypeId = 0)
                    AND (SubjectId = @SubjectId OR @SubjectId = 0);"; // Fetch all relevant questions

                // Fetch the data
                var questions = await _connection.QueryAsync<QuestionResponseDTO>(query, new
                {
                    BoardId = request.BoardId,
                    ClassId = request.ClassId,
                    CourseId = request.CourseId,
                    ExamTypeId = request.ExamTypeId,
                    SubjectId = request.SubjectId
                });

                // Randomly select 30 questions from the fetched results
                var randomQuestions = questions.OrderBy(q => Guid.NewGuid()).Take(30).ToList();

                // Return the randomly selected questions
                return new ServiceResponse<List<QuestionResponseDTO>>(true, "Operation Successful", randomQuestions, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(false, ex.Message, new List<QuestionResponseDTO>(), 500);
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
    }
}
