using System.Data;
using Dapper;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Responses;
using StudentApp_API.Repository.Interfaces;

namespace StudentApp_API.Repository.Implementations
{
    public class MyChallengesRepository: IMyChallengesRepository
    {
        private readonly IDbConnection _connection;

        public MyChallengesRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<List<CYOTResponse>>> GetCYOTListByStudent(CYOTListRequest request)
        {
            try
            {
                // Query to get CYOT details filtered by StudentID
                var cyotQuery = @"
        SELECT 
            CYOT.CYOTID,
            CYOT.ChallengeName AS CYOTName,
            CYOT.NoOfQuestions AS TotalQuestions,
            CYOT.Duration,
            CYOT.CreatedOn,
            CYOT.CYOTStatusID,
            CYOT.MarksPerIncorrectAnswer,
            CYOT.MarksPerCorrectAnswer,
            CYOT.IsStarted,
            COALESCE((
                SELECT COUNT(*)
                FROM tblCYOTAnswers AS A
                WHERE A.CYOTID = CYOT.CYOTID AND A.StudentID = @StudentID
            ), 0) AS AttemptedQuestions,
            COALESCE((
                SELECT SUM(CASE WHEN A.IsCorrect = 1 THEN 1 ELSE 0 END)
                FROM tblCYOTAnswers AS A
                WHERE A.CYOTID = CYOT.CYOTID AND A.StudentID = @StudentID
            ), 0) AS CorrectAnswers,
            COALESCE((
                SELECT SUM(CASE WHEN A.IsCorrect = 0 THEN 1 ELSE 0 END)
                FROM tblCYOTAnswers AS A
                WHERE A.CYOTID = CYOT.CYOTID AND A.StudentID = @StudentID
            ), 0) AS IncorrectAnswers
        FROM tblCYOT AS CYOT
        WHERE CYOT.CreatedBy = @StudentID";

                // Fetch CYOT data
                var cyotList = (await _connection.QueryAsync<dynamic>(cyotQuery, new { StudentID = request.RegistrationId })).ToList();

                var typedResponse = new List<CYOTResponse>();

                foreach (var cyot in cyotList)
                {
                    // Calculate total marks obtained
                    decimal totalMarksObtained = (cyot.CorrectAnswers * cyot.MarksPerCorrectAnswer) - (cyot.IncorrectAnswers * cyot.MarksPerIncorrectAnswer);
                    int totalPossibleMarks = cyot.TotalQuestions * cyot.MarksPerCorrectAnswer;
                    int percentage = totalPossibleMarks > 0 ? (int)((totalMarksObtained / totalPossibleMarks) * 100) : 0;
                    bool isChallengeApplicable = percentage >= 80;

                    // Determine status
                    string cyotStatus;
                    bool ViewKey, Analytics;
                    if (cyot.AttemptedQuestions == 0)
                    {
                        cyotStatus = "Pending";
                    }
                    else if (isChallengeApplicable)
                    {
                        cyotStatus = "Challenged";
                        ViewKey = true;
                        Analytics = true;
                    }
                    else
                    {
                        cyotStatus = "Complete";
                        ViewKey = true;
                        Analytics = true;
                    }

                    // Filter by status if a specific status ID is provided
                    if (request.StatusId == 0 || request.StatusId == cyot.CYOTStatusID)
                    {
                        typedResponse.Add(new CYOTResponse
                        {
                            CYOTID = cyot.CYOTID,
                            CYOTName = cyot.CYOTName,
                            TotalQuestions = cyot.TotalQuestions,
                            Duration = cyot.Duration + " min",
                            CYOTStatusID = cyot.CYOTStatusID,
                            CYOTStatus = cyotStatus,
                            Percentage = percentage,
                            IsChallengeApplicable = isChallengeApplicable,
                            CreatedOn = cyot.CreatedOn
                        });
                    }
                }

                if (!typedResponse.Any())
                {
                    return new ServiceResponse<List<CYOTResponse>>(false, "No records found.", new List<CYOTResponse>(), 200);
                }

                return new ServiceResponse<List<CYOTResponse>>(true, "CYOT list fetched successfully.", typedResponse, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<CYOTResponse>>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<string>> DeleteCYOT(int CYOTId)
        {
            try
            {
                // Check if the record exists
                string existsQuery = "SELECT COUNT(*) FROM tblCYOT WHERE CYOTID = @CYOTID";
                int count = await _connection.ExecuteScalarAsync<int>(existsQuery, new { CYOTID = CYOTId });

                if (count == 0)
                {
                    return new ServiceResponse<string>(false, "Record not found", string.Empty, 404);
                }

                // Delete the record
                string deleteQuery = @"
            DELETE FROM tblCYOT
            WHERE CYOTID = @CYOTID";
                await _connection.ExecuteAsync(deleteQuery, new { CYOTID = CYOTId });

                return new ServiceResponse<string>(true, "Operation successful", "Record deleted successfully", 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<bool>> MakeCYOTOpenChallenge(int CYOTId, int studentId)
        {
            if (_connection.State == ConnectionState.Closed)
                _connection.Open(); // Open the connection if closed
            using var transaction = _connection.BeginTransaction(); // Ensure atomicity

            try
            {
                // Step 1: Fetch the creator's Board, Class, and Course
                var creatorDetails = await _connection.QueryFirstOrDefaultAsync<StudentClassCourseMappings>(
                    @"SELECT BoardID, ClassID, CourseID 
              FROM tblStudentClassCourseMapping 
              WHERE RegistrationID = @StudentID",
                    new { StudentID = studentId }, transaction);

                if (creatorDetails == null)
                    return new ServiceResponse<bool>(false, "Creator details not found", false, 400);

                // Step 2: Fetch all classmates with the same BoardID, ClassID, and CourseID
                var classmates = await _connection.QueryAsync<int>(
                    @"SELECT RegistrationID FROM tblStudentClassCourseMapping 
              WHERE BoardID = @BoardID AND ClassID = @ClassID AND CourseID = @CourseID 
              AND RegistrationID <> @StudentID", // Exclude the creator
                    new { creatorDetails.BoardId, creatorDetails.ClassId, creatorDetails.CourseId, StudentID = studentId }, transaction);

                if (!classmates.Any())
                    return new ServiceResponse<bool>(false, "No classmates found for this challenge.", false, 400);

                // Step 3: Insert classmates into tblCYOTParticipant (if not already there)
                var insertQuery = @"
            IF NOT EXISTS (SELECT 1 FROM tblCYOTParticipant WHERE StudentID = @StudentID AND CYOTID = @CYOTID)
            BEGIN
                INSERT INTO tblCYOTParticipant (StudentID, CYOTID, IsCompleted, IsStarted, CYOTStatusID)
                VALUES (@StudentID, @CYOTID, 0, 0, 1) -- Default values: Not started, Not completed
            END";

                foreach (var classmateId in classmates)
                {
                    await _connection.ExecuteAsync(insertQuery, new { StudentID = classmateId, CYOTID = CYOTId }, transaction);
                }

                // Step 4: Mark the CYOT as an Open Challenge
                await _connection.ExecuteAsync(@"UPDATE [tblCYOT] SET CYOTStatusID = 3 WHERE CYOTID = @CYOTID",
                    new { CYOTID = CYOTId }, transaction);

                transaction.Commit();
                _connection.Close();// Commit transaction
                return new ServiceResponse<bool>(true, "CYOT marked as Open Challenge & classmates added.", true, 200);
            }
            catch (Exception ex)
            {
                transaction.Rollback(); // Rollback on error
                _connection.Close();
                return new ServiceResponse<bool>(false, $"Error: {ex.Message}", false, 500);
            }
        }
        public async Task<ServiceResponse<CYOTMyChallengesAnalyticsResponse>> GetCYOTAnalyticsAsync(int studentId, int cyotId)
        {
            try
            {
                // Step 1: Fetch Student's Performance
                var query = @"
        SELECT 
            SUM(CASE WHEN A.IsCorrect = 1 THEN CYOT.MarksPerCorrectAnswer ELSE 0 END) AS AchievedMarks,
            SUM(CASE WHEN A.IsCorrect = 0 THEN CYOT.MarksPerIncorrectAnswer ELSE 0 END) AS NegativeMarks,
            (SELECT COUNT(*) * MAX(CYOT.MarksPerCorrectAnswer) 
             FROM tblCYOTQuestions 
             WHERE CYOTID = @CYOTID) AS TotalMarks
        FROM tblCYOTAnswers AS A
        JOIN tblCYOT AS CYOT ON A.CYOTID = CYOT.CYOTID
        WHERE A.CYOTID = @CYOTID AND A.StudentID = @StudentID;";

                var studentResult = await _connection.QueryFirstOrDefaultAsync<dynamic>(query, new
                {
                    CYOTID = cyotId,
                    StudentID = studentId
                });

                if (studentResult == null)
                {
                    return new ServiceResponse<CYOTMyChallengesAnalyticsResponse>(
                        false, "No analytics data found", null, 404);
                }

                decimal achievedMarks = (decimal)studentResult.AchievedMarks;
                decimal negativeMarks = (decimal)studentResult.NegativeMarks;
                decimal totalMarks = (decimal)studentResult.TotalMarks;
                decimal finalMarks = achievedMarks - negativeMarks;
                decimal percentage = totalMarks > 0 ? Math.Round(finalMarks / totalMarks * 100, 2) : 0;

                // Step 2: Fetch All Participants' Performance
                var allScoresQuery = @"
        SELECT 
            R.RegistrationID,
            R.CountryID,
            SUM(CASE WHEN A.IsCorrect = 1 THEN CYOT.MarksPerCorrectAnswer ELSE 0 END) 
            - SUM(CASE WHEN A.IsCorrect = 0 THEN CYOT.MarksPerIncorrectAnswer ELSE 0 END) AS FinalMarks
        FROM tblCYOTAnswers AS A
        JOIN tblCYOT AS CYOT ON A.CYOTID = CYOT.CYOTID
        JOIN tblRegistration AS R ON A.StudentID = R.RegistrationID
        WHERE A.CYOTID = @CYOTID
        GROUP BY R.RegistrationID, R.CountryID
        ORDER BY FinalMarks DESC;";

                var allScores = (await _connection.QueryAsync<dynamic>(allScoresQuery, new { CYOTID = cyotId })).ToList();
                int totalStudents = allScores.Count;
                int rank = allScores.FindIndex(x => x.StudentID == studentId) + 1;
                int studentsAbove = rank - 1;
                decimal percentile = Math.Round(((totalStudents - rank) / (decimal)totalStudents) * 100, 2);

                // Step 3: Fetch Country Rank
                var studentCountryId = allScores.FirstOrDefault(x => x.StudentID == studentId)?.CountryID;
        //        var countryRankQuery = @"
        //SELECT COUNT(*) + 1 
        //FROM tblCYOTAnswers AS A
        //JOIN tblRegistration AS R ON A.StudentID = R.RegistrationID
        //WHERE A.CYOTID = @CYOTID AND R.CountryID = @CountryID
        //AND (SUM(CASE WHEN A.IsCorrect = 1 THEN CYOT.MarksPerCorrectAnswer ELSE 0 END) 
        //- SUM(CASE WHEN A.IsCorrect = 0 THEN CYOT.MarksPerIncorrectAnswer ELSE 0 END)) > @FinalMarks;";

        //        var countryRank = await _connection.ExecuteScalarAsync<int>(countryRankQuery, new { CYOTID = cyotId, CountryID = studentCountryId, FinalMarks = finalMarks });

        //        // Step 4: Fetch National Rank
        //        var nationalRankQuery = @"
        //SELECT COUNT(*) + 1 
        //FROM tblCYOTAnswers AS A
        //WHERE A.CYOTID = @CYOTID
        //    AND (SUM(CASE WHEN A.IsCorrect = 1 THEN CYOT.MarksPerCorrectAnswer ELSE 0 END) 
        //    - SUM(CASE WHEN A.IsCorrect = 0 THEN CYOT.MarksPerIncorrectAnswer ELSE 0 END)) > @FinalMarks;";

        //        var nationalRank = await _connection.ExecuteScalarAsync<int>(nationalRankQuery, new { CYOTID = cyotId, FinalMarks = finalMarks });

                // Step 5: Prepare Response
                var response = new CYOTMyChallengesAnalyticsResponse
                {
                    AchievedMarks = achievedMarks,
                    NegativeMarks = negativeMarks,
                    FinalMarks = finalMarks,
                    FinalPercentage = percentage,
                    Percentile = percentile,
                    StudentsAboveMe = studentsAbove,
                    TotalStudentsAttempted = totalStudents,
                    CountryRank = 0,
                    NationalRank = 0
                };

                return new ServiceResponse<CYOTMyChallengesAnalyticsResponse>(
                    true, "Analytics data fetched successfully", response, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<CYOTMyChallengesAnalyticsResponse>(
                    false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<CYOTMyChallengesTimeAnalytics>> GetCYOTTimeAnalyticsAsync(int studentId, int cyotId)
        {
            try
            {
                var query = @"WITH StudentTime AS (
    SELECT 
        N.StudentId,
        SUM(DATEDIFF(SECOND, N.StartTime, N.EndTime)) AS TotalTimeSpent,
        AVG(DATEDIFF(SECOND, N.StartTime, N.EndTime)) AS AvgTimePerQuestion,
        
        SUM(CASE WHEN A.IsCorrect = 1 THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE 0 END) AS TotalTimeSpentCorrect,
        AVG(CASE WHEN A.IsCorrect = 1 THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE NULL END) AS AvgTimeSpentCorrect,

        SUM(CASE WHEN A.IsCorrect = 0 THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE 0 END) AS TotalTimeSpentWrong,
        AVG(CASE WHEN A.IsCorrect = 0 THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE NULL END) AS AvgTimeSpentWrong,

        SUM(CASE WHEN SQM.QuestionStatusId IN (2, 4) THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE 0 END) AS TotalTimeSpentUnattempted,
        AVG(CASE WHEN SQM.QuestionStatusId IN (2, 4) THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE NULL END) AS AvgTimeSpentUnattempted
    FROM tblCYOTQuestionNavigation AS N
    LEFT JOIN tblCYOTAnswers AS A ON N.QuestionId = A.QuestionID AND N.StudentId = A.StudentID AND N.CYOTId = A.CYOTID
    LEFT JOIN tblCYOTStudentQuestionMapping AS SQM ON N.QuestionId = SQM.QuestionId AND N.StudentId = SQM.StudentId AND N.CYOTId = SQM.CYOTId
    WHERE N.CYOTId = @CYOTId
    GROUP BY N.StudentId
),
OthersTime AS (
    SELECT 
        AVG(TotalTimeSpent) AS AvgTimeSpentByOthers,
        AVG(AvgTimePerQuestion) AS AvgTimePerQuestionByOthers,
        AVG(TotalTimeSpentCorrect) AS AvgTimeSpentCorrectByOthers,
        AVG(AvgTimeSpentCorrect) AS AvgAvgTimeSpentCorrectByOthers,
        AVG(TotalTimeSpentWrong) AS AvgTimeSpentWrongByOthers,
        AVG(AvgTimeSpentWrong) AS AvgAvgTimeSpentWrongByOthers,
        AVG(TotalTimeSpentUnattempted) AS AvgTimeSpentUnattemptedByOthers,
        AVG(AvgTimeSpentUnattempted) AS AvgAvgTimeSpentUnattemptedByOthers
    FROM StudentTime
    WHERE StudentId != @StudentId
)
SELECT ST.*, OT.*
FROM StudentTime ST
CROSS JOIN OthersTime OT
WHERE ST.StudentId = @StudentId;";

                var result = await _connection.QueryFirstOrDefaultAsync<dynamic>(query, new
                {
                    CYOTID = cyotId,
                    StudentID = studentId
                });

                if (result != null)
                {
                    var response = new CYOTMyChallengesTimeAnalytics
                    {
                        // Student's time analytics
                        TotalTimeSpent = ConvertSecondsToTimeFormat(result.TotalTimeSpent ?? 0),
                        AvgTimePerQuestion = ConvertSecondsToTimeFormat(result.AvgTimePerQuestion ?? 0),

                        TotalTimeSpentCorrect = ConvertSecondsToTimeFormat(result.TotalTimeSpentCorrect ?? 0),
                        AvgTimeSpentCorrect = ConvertSecondsToTimeFormat(result.AvgTimeSpentCorrect ?? 0),

                        TotalTimeSpentWrong = ConvertSecondsToTimeFormat(result.TotalTimeSpentWrong ?? 0),
                        AvgTimeSpentWrong = ConvertSecondsToTimeFormat(result.AvgTimeSpentWrong ?? 0),

                        TotalTimeSpentUnattempted = ConvertSecondsToTimeFormat(result.TotalTimeSpentUnattempted ?? 0),
                        AvgTimeSpentUnattempted = ConvertSecondsToTimeFormat(result.AvgTimeSpentUnattempted ?? 0),

                        // Other students' average time analytics
                        AvgTimeSpentByOthers = ConvertSecondsToTimeFormat(result.AvgTimeSpentByOthers ?? 0),
                        AvgTimePerQuestionByOthers = ConvertSecondsToTimeFormat(result.AvgTimePerQuestionByOthers ?? 0),

                        AvgTimeSpentCorrectByOthers = ConvertSecondsToTimeFormat(result.AvgTimeSpentCorrectByOthers ?? 0),
                        AvgAvgTimeSpentCorrectByOthers = ConvertSecondsToTimeFormat(result.AvgAvgTimeSpentCorrectByOthers ?? 0),

                        AvgTimeSpentWrongByOthers = ConvertSecondsToTimeFormat(result.AvgTimeSpentWrongByOthers ?? 0),
                        AvgAvgTimeSpentWrongByOthers = ConvertSecondsToTimeFormat(result.AvgAvgTimeSpentWrongByOthers ?? 0),

                        AvgTimeSpentUnattemptedByOthers = ConvertSecondsToTimeFormat(result.AvgTimeSpentUnattemptedByOthers ?? 0),
                        AvgAvgTimeSpentUnattemptedByOthers = ConvertSecondsToTimeFormat(result.AvgAvgTimeSpentUnattemptedByOthers ?? 0)
                    };

                    return new ServiceResponse<CYOTMyChallengesTimeAnalytics>(
                        true,
                        "Time analytics fetched successfully",
                        response,
                        200
                    );
                }

                return new ServiceResponse<CYOTMyChallengesTimeAnalytics>(
                    false,
                    "No analytics data found",
                    null,
                    404
                );
            }
            catch (Exception ex)
            {
                return new ServiceResponse<CYOTMyChallengesTimeAnalytics>(
                    false,
                    ex.Message,
                    null,
                    500
                );
            }
        }
        public async Task<ServiceResponse<CYOTMyChallengesAnalyticsResponse>> GetCYOTSubjectWiseAnalyticsAsync(int studentId, int cyotId, int subjectId)
        {
            try
            {
                var query = @"
      SELECT 
    SUM(CASE WHEN A.IsCorrect = 1 THEN CYOT.MarksPerCorrectAnswer ELSE 0 END) AS AchievedMarks,
    SUM(CASE WHEN A.IsCorrect = 0 THEN CYOT.MarksPerIncorrectAnswer ELSE 0 END) AS NegativeMarks,
    (SELECT COUNT(*) * MAX(CYOT.MarksPerCorrectAnswer) 
     FROM tblCYOTQuestions 
     WHERE CYOTID = @CYOTID AND SubjectID = @SubjectId) AS TotalMarks
FROM tblCYOTAnswers AS A
JOIN tblCYOT AS CYOT ON A.CYOTID = CYOT.CYOTID
WHERE A.CYOTID = @CYOTID AND A.StudentID = @StudentID AND A.SubjectId = @SubjectID
GROUP BY A.SubjectId;";

                var studentResult = await _connection.QueryFirstOrDefaultAsync<dynamic>(query, new
                {
                    CYOTID = cyotId,
                    StudentID = studentId,
                    SubjectID = subjectId
                });

                if (studentResult == null)
                {
                    return new ServiceResponse<CYOTMyChallengesAnalyticsResponse>(
                        false, "No analytics data found for this subject", null, 404);
                }

                decimal achievedMarks = (decimal)studentResult.AchievedMarks;
                decimal negativeMarks = (decimal)studentResult.NegativeMarks;
                decimal totalMarks = (decimal)studentResult.TotalMarks;
                decimal finalMarks = achievedMarks - negativeMarks;
                decimal percentage = totalMarks > 0 ? Math.Round(finalMarks / totalMarks * 100, 2) : 0;

                // Fetch all students' scores for this subject
                var allScoresQuery = @"
        SELECT 
            R.RegistrationID,
            R.CountryID,
            SUM(CASE WHEN A.IsCorrect = 1 THEN CYOT.MarksPerCorrectAnswer ELSE 0 END) 
            - SUM(CASE WHEN A.IsCorrect = 0 THEN CYOT.MarksPerIncorrectAnswer ELSE 0 END) AS FinalMarks
        FROM tblCYOTAnswers AS A
        JOIN tblCYOT AS CYOT ON A.CYOTID = CYOT.CYOTID
        JOIN tblCYOTQuestions AS Q ON A.QuestionID = Q.QuestionID
        JOIN tblRegistration AS R ON A.StudentID = R.RegistrationID
        WHERE A.CYOTID = @CYOTID AND A.SubjectId = @SubjectID
        GROUP BY R.RegistrationID, R.CountryID
        ORDER BY FinalMarks DESC;";

                var allScores = (await _connection.QueryAsync<dynamic>(allScoresQuery, new { CYOTID = cyotId, SubjectID = subjectId })).ToList();
                int totalStudents = allScores.Count;
                int rank = allScores.FindIndex(x => x.StudentID == studentId) + 1;
                int studentsAbove = rank - 1;
                decimal percentile = Math.Round(((totalStudents - rank) / (decimal)totalStudents) * 100, 2);

                // Fetch country rank for this subject
                var studentCountryId = allScores.FirstOrDefault(x => x.StudentID == studentId)?.CountryID;
        //        var countryRankQuery = @"
        //SELECT COUNT(*) + 1 
        //FROM tblCYOTAnswers AS A
        //JOIN tblRegistration AS R ON A.StudentID = R.RegistrationID
        //JOIN tblCYOTQuestions AS Q ON A.QuestionID = Q.QuestionID
        //WHERE A.CYOTID = @CYOTID AND R.CountryID = @CountryID AND Q.SubjectID = @SubjectID
        //AND (SUM(CASE WHEN A.IsCorrect = 1 THEN CYOT.MarksPerCorrectAnswer ELSE 0 END) 
        //- SUM(CASE WHEN A.IsCorrect = 0 THEN CYOT.MarksPerIncorrectAnswer ELSE 0 END)) > @FinalMarks;";

        //        var countryRank = await _connection.ExecuteScalarAsync<int>(countryRankQuery, new { CYOTID = cyotId, CountryID = studentCountryId, SubjectID = subjectId, FinalMarks = finalMarks });

        //        // Fetch national rank for this subject
        //        var nationalRankQuery = @"
        //SELECT COUNT(*) + 1 
        //FROM tblCYOTAnswers AS A
        //JOIN tblCYOTQuestions AS Q ON A.QuestionID = Q.QuestionID
        //WHERE A.CYOTID = @CYOTID AND Q.SubjectID = @SubjectID
        //    AND (SUM(CASE WHEN A.IsCorrect = 1 THEN CYOT.MarksPerCorrectAnswer ELSE 0 END) 
        //    - SUM(CASE WHEN A.IsCorrect = 0 THEN CYOT.MarksPerIncorrectAnswer ELSE 0 END)) > @FinalMarks;";

        //        var nationalRank = await _connection.ExecuteScalarAsync<int>(nationalRankQuery, new { CYOTID = cyotId, SubjectID = subjectId, FinalMarks = finalMarks });

                var response = new CYOTMyChallengesAnalyticsResponse
                {
                  //  SubjectID = subjectId,
                    AchievedMarks = achievedMarks,
                    NegativeMarks = negativeMarks,
                    FinalMarks = finalMarks,
                    FinalPercentage = percentage,
                    Percentile = percentile,
                    StudentsAboveMe = studentsAbove,
                    TotalStudentsAttempted = totalStudents,
                    CountryRank = 0,
                    NationalRank = 0
                };

                return new ServiceResponse<CYOTMyChallengesAnalyticsResponse>(
                    true, "Subject-wise analytics data fetched successfully", response, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<CYOTMyChallengesAnalyticsResponse>(
                    false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<CYOTMyChallengesTimeAnalytics>> GetCYOTSubjectWiseTimeAnalyticsAsync(int studentId, int cyotId, int subjectId)
        {
            try
            {
                var query = @"WITH StudentTime AS (
    SELECT 
        N.StudentId,
        SUM(DATEDIFF(SECOND, N.StartTime, N.EndTime)) AS TotalTimeSpent,
        AVG(DATEDIFF(SECOND, N.StartTime, N.EndTime)) AS AvgTimePerQuestion,

        SUM(CASE WHEN A.IsCorrect = 1 THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE 0 END) AS TotalTimeSpentCorrect,
        AVG(CASE WHEN A.IsCorrect = 1 THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE NULL END) AS AvgTimeSpentCorrect,

        SUM(CASE WHEN A.IsCorrect = 0 THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE 0 END) AS TotalTimeSpentWrong,
        AVG(CASE WHEN A.IsCorrect = 0 THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE NULL END) AS AvgTimeSpentWrong,

        SUM(CASE WHEN SQM.QuestionStatusId IN (2, 4) THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE 0 END) AS TotalTimeSpentUnattempted,
        AVG(CASE WHEN SQM.QuestionStatusId IN (2, 4) THEN DATEDIFF(SECOND, N.StartTime, N.EndTime) ELSE NULL END) AS AvgTimeSpentUnattempted
    FROM tblCYOTQuestionNavigation AS N
    LEFT JOIN tblCYOTAnswers AS A ON N.QuestionId = A.QuestionID AND N.StudentId = A.StudentID AND N.CYOTId = A.CYOTID
    LEFT JOIN tblCYOTStudentQuestionMapping AS SQM ON N.QuestionId = SQM.QuestionId AND N.StudentId = SQM.StudentId AND N.CYOTId = SQM.CYOTId
    JOIN tblCYOTQuestions AS Q ON N.QuestionId = Q.QuestionID  -- Ensuring only questions for the subject are considered
    WHERE N.CYOTId = @CYOTId AND A.SubjectID = @SubjectID
    GROUP BY N.StudentId
),
OthersTime AS (
    SELECT 
        1 AS DummyKey, -- Adding a static key for JOIN
        AVG(TotalTimeSpent) AS AvgTimeSpentByOthers,
        AVG(AvgTimePerQuestion) AS AvgTimePerQuestionByOthers,
        AVG(TotalTimeSpentCorrect) AS AvgTimeSpentCorrectByOthers,
        AVG(AvgTimeSpentCorrect) AS AvgAvgTimeSpentCorrectByOthers,
        AVG(TotalTimeSpentWrong) AS AvgTimeSpentWrongByOthers,
        AVG(AvgTimeSpentWrong) AS AvgAvgTimeSpentWrongByOthers,
        AVG(TotalTimeSpentUnattempted) AS AvgTimeSpentUnattemptedByOthers,
        AVG(AvgTimeSpentUnattempted) AS AvgAvgTimeSpentUnattemptedByOthers
    FROM StudentTime
    WHERE StudentId != @StudentId
)
SELECT ST.*, OT.*
FROM StudentTime ST
LEFT JOIN OthersTime OT ON 1=1
WHERE ST.StudentId = @StudentId;";

                var result = await _connection.QueryFirstOrDefaultAsync<dynamic>(query, new
                {
                    CYOTID = cyotId,
                    StudentID = studentId,
                    SubjectID = subjectId
                });

                if (result != null)
                {
                    var response = new CYOTMyChallengesTimeAnalytics
                    {
                        // Student's time analytics
                        TotalTimeSpent = ConvertSecondsToTimeFormat(result.TotalTimeSpent ?? 0),
                        AvgTimePerQuestion = ConvertSecondsToTimeFormat(result.AvgTimePerQuestion ?? 0),

                        TotalTimeSpentCorrect = ConvertSecondsToTimeFormat(result.TotalTimeSpentCorrect ?? 0),
                        AvgTimeSpentCorrect = ConvertSecondsToTimeFormat(result.AvgTimeSpentCorrect ?? 0),

                        TotalTimeSpentWrong = ConvertSecondsToTimeFormat(result.TotalTimeSpentWrong ?? 0),
                        AvgTimeSpentWrong = ConvertSecondsToTimeFormat(result.AvgTimeSpentWrong ?? 0),

                        TotalTimeSpentUnattempted = ConvertSecondsToTimeFormat(result.TotalTimeSpentUnattempted ?? 0),
                        AvgTimeSpentUnattempted = ConvertSecondsToTimeFormat(result.AvgTimeSpentUnattempted ?? 0),

                        // Other students' average time analytics
                        AvgTimeSpentByOthers = ConvertSecondsToTimeFormat(result.AvgTimeSpentByOthers ?? 0),
                        AvgTimePerQuestionByOthers = ConvertSecondsToTimeFormat(result.AvgTimePerQuestionByOthers ?? 0),

                        AvgTimeSpentCorrectByOthers = ConvertSecondsToTimeFormat(result.AvgTimeSpentCorrectByOthers ?? 0),
                        AvgAvgTimeSpentCorrectByOthers = ConvertSecondsToTimeFormat(result.AvgAvgTimeSpentCorrectByOthers ?? 0),

                        AvgTimeSpentWrongByOthers = ConvertSecondsToTimeFormat(result.AvgTimeSpentWrongByOthers ?? 0),
                        AvgAvgTimeSpentWrongByOthers = ConvertSecondsToTimeFormat(result.AvgAvgTimeSpentWrongByOthers ?? 0),

                        AvgTimeSpentUnattemptedByOthers = ConvertSecondsToTimeFormat(result.AvgTimeSpentUnattemptedByOthers ?? 0),
                        AvgAvgTimeSpentUnattemptedByOthers = ConvertSecondsToTimeFormat(result.AvgAvgTimeSpentUnattemptedByOthers ?? 0)
                    };

                    return new ServiceResponse<CYOTMyChallengesTimeAnalytics>(
                        true,
                        "Subject-wise time analytics fetched successfully",
                        response,
                        200
                    );
                }

                return new ServiceResponse<CYOTMyChallengesTimeAnalytics>(
                    false,
                    "No analytics data found for this subject",
                    null,
                    404
                );
            }
            catch (Exception ex)
            {
                return new ServiceResponse<CYOTMyChallengesTimeAnalytics>(
                    false,
                    ex.Message,
                    null,
                    500
                );
            }
        }
        public async Task<ServiceResponse<MarksComparison>> GetCYOTMarksComparisonAsync(int studentId, int cyotId)
        {
            try
            {
                var query = @"WITH StudentMarks AS (
    SELECT 
        StudentID,
        SUM(Marks) AS TotalMarks
    FROM tblCYOTAnswers
    WHERE CYOTID = @CYOTID
    GROUP BY StudentID
),
TopperMarks AS (
    SELECT MAX(TotalMarks) AS MarksByTopper FROM StudentMarks
),
AverageMarks AS (
    SELECT AVG(TotalMarks) AS AvgMarksByOthers FROM StudentMarks WHERE StudentID != @StudentID
)
SELECT 
    (SELECT TotalMarks FROM StudentMarks WHERE StudentID = @StudentID) AS MarksByMe,
    (SELECT MarksByTopper FROM TopperMarks) AS MarksByTopper,
    (SELECT AvgMarksByOthers FROM AverageMarks) AS AvgMarksByOthers;";

                var result = await _connection.QueryFirstOrDefaultAsync<dynamic>(query, new
                {
                    CYOTID = cyotId,
                    StudentID = studentId
                });

                if (result != null)
                {
                    var response = new MarksComparison
                    {
                        MarksByMe = result.MarksByMe ?? 0,
                        MarksByTopper = result.MarksByTopper ?? 0,
                        AvgMarksByOthers = result.AvgMarksByOthers ?? 0
                    };

                    return new ServiceResponse<MarksComparison>(
                        true,
                        "Marks comparison fetched successfully",
                        response,
                        200
                    );
                }

                return new ServiceResponse<MarksComparison>(
                    false,
                    "No marks data found",
                    null,
                    404
                );
            }
            catch (Exception ex)
            {
                return new ServiceResponse<MarksComparison>(
                    false,
                    ex.Message,
                    null,
                    500
                );
            }
        }
        public async Task<ServiceResponse<List<LeaderboardResponse>>> GetCYOTLeaderboardAsync(int cyotId, int studentId)
        {
            try
            {
                var query = @"
            WITH Leaderboard AS (
                SELECT 
                    r.RegistrationID AS StudentID,
                    r.FirstName,
                    r.LastName,
                    COALESCE(SUM(ca.Marks), 0) AS TotalScore
                FROM tblRegistration r
                LEFT JOIN tblCYOTAnswers ca ON r.RegistrationID = ca.StudentID
                WHERE ca.CYOTID = @CYOTID
                GROUP BY r.RegistrationID, r.FirstName, r.LastName
            )
            SELECT 
                StudentID,
                FirstName,
                LastName,
                TotalScore
            FROM Leaderboard
            ORDER BY 
                CASE 
                    WHEN StudentID = @StudentID THEN 0 
                    ELSE 1 
                END,
                TotalScore DESC;";

                var leaderboard = await _connection.QueryAsync<LeaderboardResponse>(
                    query,
                    new { CYOTID = cyotId, StudentID = studentId }
                );

                return new ServiceResponse<List<LeaderboardResponse>>(
                    true,
                    "Leaderboard fetched successfully",
                    leaderboard.ToList(),
                    200
                );
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<LeaderboardResponse>>(
                    false,
                    $"Error: {ex.Message}",
                    null,
                    500
                );
            }
        }
        private static string ConvertSecondsToTimeFormat(int seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(seconds);
            if (time.Hours > 0)
                return $"{time.Hours} hours {time.Minutes} minutes {time.Seconds} seconds";
            else if (time.Minutes > 0)
                return $"{time.Minutes} minutes {time.Seconds} seconds";
            else
                return $"{time.Seconds} seconds";
        }
    }
}
