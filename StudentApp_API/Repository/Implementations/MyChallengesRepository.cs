using System.Data;
using Dapper;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Responses;
using StudentApp_API.Repository.Interfaces;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StudentApp_API.Repository.Implementations
{
    public class MyChallengesRepository : IMyChallengesRepository
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

                bool ViewKey = false, Analytics = false;
                foreach (var cyot in cyotList)
                {
                    // Calculate total marks obtained
                    decimal totalMarksObtained = (cyot.CorrectAnswers * cyot.MarksPerCorrectAnswer) - (cyot.IncorrectAnswers * cyot.MarksPerIncorrectAnswer);
                    int totalPossibleMarks = cyot.TotalQuestions * cyot.MarksPerCorrectAnswer;
                    int percentage = totalPossibleMarks > 0 ? (int)((totalMarksObtained / totalPossibleMarks) * 100) : 0;
                    bool isChallengeApplicable = percentage >= 80;
                    string status = _connection.QueryFirstOrDefault<string>(@"select CYOTStatus from [CYOTStatus] where CYOTStatusID = @CYOTStatusID", new
                    {
                        CYOTStatusID = cyot.CYOTStatusID
                    });
                    // Determine status
                    string cyotStatus;
                    if (cyot.AttemptedQuestions == 0)
                    {
                        cyotStatus = status;
                        ViewKey = false;
                        Analytics = false;
                    }
                    else if (isChallengeApplicable)
                    {
                        cyotStatus = status;
                        ViewKey = true;
                        Analytics = true;
                    }
                    else
                    {
                        cyotStatus = status;
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
                            CreatedOn = cyot.CreatedOn,
                            IsViewKey = ViewKey,
                            IsAnalytics = Analytics,
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

                decimal achievedMarks = (decimal)(studentResult.AchievedMarks ?? 0);
                decimal negativeMarks = (decimal)(studentResult.NegativeMarks ?? 0);
                decimal totalMarks = (decimal)(studentResult.TotalMarks ?? 0);

                decimal finalMarks = achievedMarks - negativeMarks;
                decimal percentage = totalMarks > 0 ? Math.Round(finalMarks / totalMarks * 100, 2) : 0;

                //decimal achievedMarks = (decimal)studentResult.AchievedMarks;
                //decimal negativeMarks = (decimal)studentResult.NegativeMarks;
                //decimal totalMarks = (decimal)studentResult.TotalMarks;
                //decimal finalMarks = achievedMarks - negativeMarks;
                //decimal percentage = totalMarks > 0 ? Math.Round(finalMarks / totalMarks * 100, 2) : 0;

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
                int rank = allScores.FindIndex(x => x.RegistrationID == studentId) + 1;
                int studentsAbove = rank > 0 ? rank - 1 : 0;
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
                string query = @"
SELECT 
    N.StudentId,
    N.QuestionId,
    N.StartTime,
    N.EndTime,
    A.IsCorrect,
    SQM.QuestionStatusId
FROM tblCYOTQuestionNavigation N
LEFT JOIN tblCYOTAnswers A 
    ON N.QuestionId = A.QuestionID AND N.StudentId = A.StudentID AND N.CYOTId = A.CYOTID
LEFT JOIN tblCYOTStudentQuestionMapping SQM 
    ON N.QuestionId = SQM.QuestionId AND N.StudentId = SQM.StudentId AND N.CYOTId = SQM.CYOTId
WHERE N.CYOTId = @CYOTId";

                var data = (await _connection.QueryAsync(query, new { CYOTId = cyotId })).ToList();

                if (data == null || data.Count == 0)
                    return new ServiceResponse<CYOTMyChallengesTimeAnalytics>(false, "No analytics data found", null, 404);

                // Student specific analytics
                var studentQuestionGroups = data
                    .Where(x => x.StudentId == studentId && x.StartTime != null && x.EndTime != null)
                    .GroupBy(x => x.QuestionId)
                    .ToList();

                double totalTime = 0, totalCorrect = 0, totalWrong = 0, totalUnattempted = 0;
                int totalCorrectQuestions = 0, totalWrongQuestions = 0, totalUnattemptedQuestions = 0;

                foreach (var group in studentQuestionGroups)
                {
                    var totalDuration = group.Sum(x => ((DateTime)x.EndTime - (DateTime)x.StartTime).TotalSeconds);
                    var first = group.FirstOrDefault();

                    totalTime += totalDuration;

                    if (first?.IsCorrect == true)
                    {
                        totalCorrect += totalDuration;
                        totalCorrectQuestions++;
                    }
                    else if (first?.IsCorrect == false)
                    {
                        totalWrong += totalDuration;
                        totalWrongQuestions++;
                    }
                    else if (first?.QuestionStatusId == 2 || first?.QuestionStatusId == 4 || first?.IsCorrect == null)
                    {
                        totalUnattempted += totalDuration;
                        totalUnattemptedQuestions++;
                    }
                }

                int totalQuestions = await _connection.QueryFirstOrDefaultAsync<int>(
                    @"SELECT COUNT(*) FROM tblCYOTStudentQuestionMapping 
              WHERE StudentId = @StudentId AND CYOTId = @CYOTId",
                    new { StudentId = studentId, CYOTId = cyotId });

                double avgTime = totalQuestions > 0 ? totalTime / totalQuestions : 0;
                double avgCorrect = totalCorrectQuestions > 0 ? totalCorrect / totalCorrectQuestions : 0;
                double avgWrong = totalWrongQuestions > 0 ? totalWrong / totalWrongQuestions : 0;
                double avgUnattempted = totalUnattemptedQuestions > 0 ? totalUnattempted / totalUnattemptedQuestions : 0;

                // Others' analytics
                var othersGrouped = data
                    .Where(x => x.StudentId != studentId && x.StartTime != null && x.EndTime != null)
                    .GroupBy(x => new { x.StudentId, x.QuestionId })
                    .GroupBy(g => g.Key.StudentId)
                    .Select(g =>
                    {
                        double tTotal = 0, tCorrect = 0, tWrong = 0, tUnattempted = 0;
                        int tCorrectCount = 0, tWrongCount = 0, tUnattemptedCount = 0;

                        foreach (var qGroup in g)
                        {
                            var totalDuration = qGroup.Sum(x => ((DateTime)x.EndTime - (DateTime)x.StartTime).TotalSeconds);
                            var first = qGroup.FirstOrDefault();

                            tTotal += totalDuration;

                            if (first?.IsCorrect == true)
                            {
                                tCorrect += totalDuration;
                                tCorrectCount++;
                            }
                            else if (first?.IsCorrect == false)
                            {
                                tWrong += totalDuration;
                                tWrongCount++;
                            }
                            else if (first?.QuestionStatusId == 2 || first?.QuestionStatusId == 4 || first?.IsCorrect == null)
                            {
                                tUnattempted += totalDuration;
                                tUnattemptedCount++;
                            }
                        }

                        double tAvg = g.Count() > 0 ? tTotal / g.Count() : 0;
                        double tCorrectAvg = tCorrectCount > 0 ? tCorrect / tCorrectCount : 0;
                        double tWrongAvg = tWrongCount > 0 ? tWrong / tWrongCount : 0;
                        double tUnattemptedAvg = tUnattemptedCount > 0 ? tUnattempted / tUnattemptedCount : 0;

                        return new
                        {
                            Total = tTotal,
                            Avg = tAvg,
                            Correct = tCorrect,
                            CorrectAvg = tCorrectAvg,
                            Wrong = tWrong,
                            WrongAvg = tWrongAvg,
                            Unattempted = tUnattempted,
                            UnattemptedAvg = tUnattemptedAvg
                        };
                    }).ToList();

                int othersCount = othersGrouped.Count;

                var othersAnalytics = othersCount > 0
                    ? new
                    {
                        AvgTotal = (int)othersGrouped.Average(x => x.Total),
                        AvgPerQ = (int)othersGrouped.Average(x => x.Avg),
                        Correct = (int)othersGrouped.Average(x => x.Correct),
                        CorrectAvg = (int)othersGrouped.Average(x => x.CorrectAvg),
                        Wrong = (int)othersGrouped.Average(x => x.Wrong),
                        WrongAvg = (int)othersGrouped.Average(x => x.WrongAvg),
                        Unattempted = (int)othersGrouped.Average(x => x.Unattempted),
                        UnattemptedAvg = (int)othersGrouped.Average(x => x.UnattemptedAvg)
                    }
                    : new { AvgTotal = 0, AvgPerQ = 0, Correct = 0, CorrectAvg = 0, Wrong = 0, WrongAvg = 0, Unattempted = 0, UnattemptedAvg = 0 };

                var response = new CYOTMyChallengesTimeAnalytics
                {
                    TotalTimeSpent = ConvertSecondsToTimeFormat((int)totalTime),
                    AvgTimePerQuestion = ConvertSecondsToTimeFormat((int)avgTime),
                    TotalTimeSpentCorrect = ConvertSecondsToTimeFormat((int)totalCorrect),
                    AvgTimeSpentCorrect = ConvertSecondsToTimeFormat((int)avgCorrect),
                    TotalTimeSpentWrong = ConvertSecondsToTimeFormat((int)totalWrong),
                    AvgTimeSpentWrong = ConvertSecondsToTimeFormat((int)avgWrong),
                    TotalTimeSpentUnattempted = ConvertSecondsToTimeFormat((int)totalUnattempted),
                    AvgTimeSpentUnattempted = ConvertSecondsToTimeFormat((int)avgUnattempted),

                    AvgTimeSpentByOthers = ConvertSecondsToTimeFormat(othersAnalytics.AvgTotal),
                    AvgTimePerQuestionByOthers = ConvertSecondsToTimeFormat(othersAnalytics.AvgPerQ),
                    AvgTimeSpentCorrectByOthers = ConvertSecondsToTimeFormat(othersAnalytics.Correct),
                    AvgAvgTimeSpentCorrectByOthers = ConvertSecondsToTimeFormat(othersAnalytics.CorrectAvg),
                    AvgTimeSpentWrongByOthers = ConvertSecondsToTimeFormat(othersAnalytics.Wrong),
                    AvgAvgTimeSpentWrongByOthers = ConvertSecondsToTimeFormat(othersAnalytics.WrongAvg),
                    AvgTimeSpentUnattemptedByOthers = ConvertSecondsToTimeFormat(othersAnalytics.Unattempted),
                    AvgAvgTimeSpentUnattemptedByOthers = ConvertSecondsToTimeFormat(othersAnalytics.UnattemptedAvg)
                };

                return new ServiceResponse<CYOTMyChallengesTimeAnalytics>(true, "Time analytics fetched successfully", response, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<CYOTMyChallengesTimeAnalytics>(false, ex.Message, null, 500);
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
                int studentsAbove = rank > 0 ? rank - 1 : 0;
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
                string query = @"
        SELECT 
            N.StudentId,
            N.QuestionId,
            N.StartTime,
            N.EndTime,
            A.IsCorrect,
            SQM.QuestionStatusId
        FROM tblCYOTQuestionNavigation AS N
        LEFT JOIN tblCYOTAnswers AS A 
            ON N.QuestionId = A.QuestionID 
            AND N.StudentId = A.StudentID 
            AND N.CYOTId = A.CYOTID 
            AND A.SubjectID = @SubjectId
        LEFT JOIN tblCYOTStudentQuestionMapping AS SQM 
            ON N.QuestionId = SQM.QuestionId 
            AND N.StudentId = SQM.StudentId 
            AND N.CYOTId = SQM.CYOTId
        WHERE N.CYOTId = @CYOTId";

                var data = (await _connection.QueryAsync(query, new { CYOTId = cyotId, SubjectId = subjectId })).ToList();

                if (data == null || data.Count == 0)
                    return new ServiceResponse<CYOTMyChallengesTimeAnalytics>(false, "No analytics data found", null, 404);

                // Student-specific data
                var studentQuestionGroups = data
                    .Where(x => x.StudentId == studentId && x.StartTime != null && x.EndTime != null)
                    .GroupBy(x => x.QuestionId);

                double totalTime = 0, totalCorrect = 0, totalWrong = 0, totalUnattempted = 0;
                int totalCorrectQuestions = 0, totalWrongQuestions = 0, totalUnattemptedQuestions = 0;

                foreach (var group in studentQuestionGroups)
                {
                    var totalDuration = group.Sum(x => ((DateTime)x.EndTime - (DateTime)x.StartTime).TotalSeconds);
                    var first = group.FirstOrDefault();

                    totalTime += totalDuration;

                    if (first?.IsCorrect == true)
                    {
                        totalCorrect += totalDuration;
                        totalCorrectQuestions++;
                    }
                    else if (first?.IsCorrect == false)
                    {
                        totalWrong += totalDuration;
                        totalWrongQuestions++;
                    }
                    else if (first?.QuestionStatusId == 2 || first?.QuestionStatusId == 4 || first?.IsCorrect == null)
                    {
                        totalUnattempted += totalDuration;
                        totalUnattemptedQuestions++;
                    }
                }

                int totalQuestions = await _connection.QueryFirstOrDefaultAsync<int>(
                    @"SELECT COUNT(*) FROM tblCYOTStudentQuestionMapping 
              WHERE StudentId = @StudentId AND CYOTId = @CYOTId AND SubjectID = @SubjectId",
                    new { StudentId = studentId, CYOTId = cyotId, SubjectId = subjectId });

                double avgTime = totalQuestions > 0 ? totalTime / totalQuestions : 0;
                double avgCorrect = totalCorrectQuestions > 0 ? totalCorrect / totalCorrectQuestions : 0;
                double avgWrong = totalWrongQuestions > 0 ? totalWrong / totalWrongQuestions : 0;
                double avgUnattempted = totalUnattemptedQuestions > 0 ? totalUnattempted / totalUnattemptedQuestions : 0;

                // Others' analytics
                var othersGrouped = data
                    .Where(x => x.StudentId != studentId && x.StartTime != null && x.EndTime != null)
                    .GroupBy(x => new { x.StudentId, x.QuestionId })
                    .GroupBy(g => g.Key.StudentId)
                    .Select(g =>
                    {
                        double tTotal = 0, tCorrect = 0, tWrong = 0, tUnattempted = 0;
                        int tCorrectCount = 0, tWrongCount = 0, tUnattemptedCount = 0;

                        foreach (var qGroup in g)
                        {
                            var totalDuration = qGroup.Sum(x => ((DateTime)x.EndTime - (DateTime)x.StartTime).TotalSeconds);
                            var first = qGroup.FirstOrDefault();

                            tTotal += totalDuration;

                            if (first?.IsCorrect == true)
                            {
                                tCorrect += totalDuration;
                                tCorrectCount++;
                            }
                            else if (first?.IsCorrect == false)
                            {
                                tWrong += totalDuration;
                                tWrongCount++;
                            }
                            else if (first?.QuestionStatusId == 2 || first?.QuestionStatusId == 4 || first?.IsCorrect == null)
                            {
                                tUnattempted += totalDuration;
                                tUnattemptedCount++;
                            }
                        }

                        return new
                        {
                            Total = tTotal,
                            Avg = g.Count() > 0 ? tTotal / g.Count() : 0,
                            Correct = tCorrect,
                            CorrectAvg = tCorrectCount > 0 ? tCorrect / tCorrectCount : 0,
                            Wrong = tWrong,
                            WrongAvg = tWrongCount > 0 ? tWrong / tWrongCount : 0,
                            Unattempted = tUnattempted,
                            UnattemptedAvg = tUnattemptedCount > 0 ? tUnattempted / tUnattemptedCount : 0
                        };
                    }).ToList();

                int othersCount = othersGrouped.Count;

                var othersAnalytics = othersCount > 0
                    ? new
                    {
                        AvgTotal = (int)othersGrouped.Average(x => x.Total),
                        AvgPerQ = (int)othersGrouped.Average(x => x.Avg),
                        Correct = (int)othersGrouped.Average(x => x.Correct),
                        CorrectAvg = (int)othersGrouped.Average(x => x.CorrectAvg),
                        Wrong = (int)othersGrouped.Average(x => x.Wrong),
                        WrongAvg = (int)othersGrouped.Average(x => x.WrongAvg),
                        Unattempted = (int)othersGrouped.Average(x => x.Unattempted),
                        UnattemptedAvg = (int)othersGrouped.Average(x => x.UnattemptedAvg)
                    }
                    : new { AvgTotal = 0, AvgPerQ = 0, Correct = 0, CorrectAvg = 0, Wrong = 0, WrongAvg = 0, Unattempted = 0, UnattemptedAvg = 0 };

                var response = new CYOTMyChallengesTimeAnalytics
                {
                    TotalTimeSpent = ConvertSecondsToTimeFormat((int)totalTime),
                    AvgTimePerQuestion = ConvertSecondsToTimeFormat((int)avgTime),
                    TotalTimeSpentCorrect = ConvertSecondsToTimeFormat((int)totalCorrect),
                    AvgTimeSpentCorrect = ConvertSecondsToTimeFormat((int)avgCorrect),
                    TotalTimeSpentWrong = ConvertSecondsToTimeFormat((int)totalWrong),
                    AvgTimeSpentWrong = ConvertSecondsToTimeFormat((int)avgWrong),
                    TotalTimeSpentUnattempted = ConvertSecondsToTimeFormat((int)totalUnattempted),
                    AvgTimeSpentUnattempted = ConvertSecondsToTimeFormat((int)avgUnattempted),

                    AvgTimeSpentByOthers = ConvertSecondsToTimeFormat(othersAnalytics.AvgTotal),
                    AvgTimePerQuestionByOthers = ConvertSecondsToTimeFormat(othersAnalytics.AvgPerQ),
                    AvgTimeSpentCorrectByOthers = ConvertSecondsToTimeFormat(othersAnalytics.Correct),
                    AvgAvgTimeSpentCorrectByOthers = ConvertSecondsToTimeFormat(othersAnalytics.CorrectAvg),
                    AvgTimeSpentWrongByOthers = ConvertSecondsToTimeFormat(othersAnalytics.Wrong),
                    AvgAvgTimeSpentWrongByOthers = ConvertSecondsToTimeFormat(othersAnalytics.WrongAvg),
                    AvgTimeSpentUnattemptedByOthers = ConvertSecondsToTimeFormat(othersAnalytics.Unattempted),
                    AvgAvgTimeSpentUnattemptedByOthers = ConvertSecondsToTimeFormat(othersAnalytics.UnattemptedAvg)
                };

                return new ServiceResponse<CYOTMyChallengesTimeAnalytics>(true, "Analytics data found for this subject", response, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<CYOTMyChallengesTimeAnalytics>(false, ex.Message, null, 500);
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
        public async Task<ServiceResponse<MarksComparison>> GetCYOTPercentageComparisonAsync(int studentId, int cyotId)
        {
            try
            {
                var query = @"
        WITH StudentMarks AS (
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
        ),
        CYOTDetails AS (
            SELECT 
                NoOfQuestions, 
                MarksPerCorrectAnswer,
                (NoOfQuestions * MarksPerCorrectAnswer) AS TotalObtainableMarks
            FROM tblCYOT 
            WHERE CYOTID = @CYOTID
        )
        SELECT 
            (SELECT TotalMarks FROM StudentMarks WHERE StudentID = @StudentID) * 100.0 / (SELECT TotalObtainableMarks FROM CYOTDetails) AS PercentageByMe,
            (SELECT MarksByTopper FROM TopperMarks) * 100.0 / (SELECT TotalObtainableMarks FROM CYOTDetails) AS PercentageByTopper,
            (SELECT AvgMarksByOthers FROM AverageMarks) * 100.0 / (SELECT TotalObtainableMarks FROM CYOTDetails) AS AvgPercentageByOthers;";

                var result = await _connection.QueryFirstOrDefaultAsync<dynamic>(query, new
                {
                    CYOTID = cyotId,
                    StudentID = studentId
                });

                if (result != null)
                {
                    var response = new MarksComparison
                    {
                        MarksByMe = result.PercentageByMe ?? 0,
                        MarksByTopper = result.PercentageByTopper ?? 0,
                        AvgMarksByOthers = result.AvgPercentageByOthers ?? 0
                    };

                    return new ServiceResponse<MarksComparison>(
                        true,
                        "Percentage comparison fetched successfully",
                        response,
                        200
                    );
                }

                return new ServiceResponse<MarksComparison>(
                    false,
                    "No percentage data found",
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
        public async Task<ServiceResponse<CorrectAnswersComparison>> GetCYOTCorrectAnswersComparisonAsync(int studentId, int cyotId)
        {
            try
            {
                var query = @"
        WITH StudentCorrectAnswers AS (
            SELECT 
                StudentID,
                COUNT(*) AS CorrectAnswers
            FROM tblCYOTAnswers
            WHERE CYOTID = @CYOTID AND IsCorrect = 1
            GROUP BY StudentID
        ),
        TopperCorrect AS (
            SELECT MAX(CorrectAnswers) AS CorrectByTopper FROM StudentCorrectAnswers
        ),
        AverageCorrect AS (
            SELECT AVG(CorrectAnswers) AS AvgCorrectByOthers FROM StudentCorrectAnswers WHERE StudentID != @StudentID
        )
        SELECT 
            (SELECT CorrectAnswers FROM StudentCorrectAnswers WHERE StudentID = @StudentID) AS CorrectByMe,
            (SELECT CorrectByTopper FROM TopperCorrect) AS CorrectByTopper,
            (SELECT AvgCorrectByOthers FROM AverageCorrect) AS AvgCorrectByOthers,
c.ChallengeName,
    c.ChallengeDate
FROM tblCYOT c
WHERE c.CYOTID = @CYOTID;;";

                var result = await _connection.QueryFirstOrDefaultAsync<dynamic>(query, new
                {
                    CYOTID = cyotId,
                    StudentID = studentId
                });

                if (result != null)
                {
                    var response = new CorrectAnswersComparison
                    {
                        CorrectByMe = result.CorrectByMe ?? 0,
                        HighestCorrect = result.CorrectByTopper ?? 0,
                        AvgCorrectByOthers = result.AvgCorrectByOthers ?? 0,
                        ChallengeDate = result.ChallengeDate ?? DateTime.MinValue,
                        ChallengeName = result.ChallengeName ?? string.Empty
                    };

                    return new ServiceResponse<CorrectAnswersComparison>(
                        true,
                        "Correct answers comparison fetched successfully",
                        response,
                        200
                    );
                }

                return new ServiceResponse<CorrectAnswersComparison>(
                    false,
                    "No correct answers data found",
                    null,
                    404
                );
            }
            catch (Exception ex)
            {
                return new ServiceResponse<CorrectAnswersComparison>(
                    false,
                    ex.Message,
                    null,
                    500
                );
            }
        }
        public async Task<ServiceResponse<IncorrectAnswersComparison>> GetCYOTIncorrectAnswersComparisonAsync(int studentId, int cyotId)
        {
            try
            {
                var query = @"
        WITH StudentIncorrectAnswers AS (
            SELECT 
                StudentID,
                COUNT(*) AS IncorrectAnswers
            FROM tblCYOTAnswers
            WHERE CYOTID = @CYOTID AND IsCorrect = 0
            GROUP BY StudentID
        ),
        TopperIncorrect AS (
            SELECT MAX(IncorrectAnswers) AS IncorrectByTopper FROM StudentIncorrectAnswers
        ),
        AverageIncorrect AS (
            SELECT AVG(IncorrectAnswers) AS AvgIncorrectByOthers FROM StudentIncorrectAnswers WHERE StudentID != @StudentID
        )
        SELECT 
            (SELECT IncorrectAnswers FROM StudentIncorrectAnswers WHERE StudentID = @StudentID) AS IncorrectByMe,
            (SELECT IncorrectByTopper FROM TopperIncorrect) AS IncorrectByTopper,
            (SELECT AvgIncorrectByOthers FROM AverageIncorrect) AS AvgIncorrectByOthers,
   c.ChallengeName,
    c.ChallengeDate
FROM tblCYOT c
WHERE c.CYOTID = @CYOTID;";

                var result = await _connection.QueryFirstOrDefaultAsync<dynamic>(query, new
                {
                    CYOTID = cyotId,
                    StudentID = studentId
                });

                if (result != null)
                {
                    var response = new IncorrectAnswersComparison
                    {
                        IncorrectByMe = result.IncorrectByMe ?? 0,
                        HighestIncorrect = result.IncorrectByTopper ?? 0,
                        AvgIncorrectByOthers = result.AvgIncorrectByOthers ?? 0,
                        ChallengeDate = result.ChallengeDate ?? DateTime.MinValue,
                        ChallengeName = result.ChallengeName ?? string.Empty
                    };
                }


                return new ServiceResponse<IncorrectAnswersComparison>(
                    false,
                    "No incorrect answers data found",
                    null,
                    404
                );
            }
            catch (Exception ex)
            {
                return new ServiceResponse<IncorrectAnswersComparison>(
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
        SELECT 
            r.RegistrationID AS StudentID,
            r.FirstName,
            r.LastName,
            COALESCE(SUM(ca.Marks), 0) AS TotalScore,
            cy.ChallengeDate
        FROM tblRegistration r
        LEFT JOIN tblCYOTAnswers ca ON r.RegistrationID = ca.StudentID AND ca.CYOTID = @CYOTID
        INNER JOIN tblCYOT cy ON cy.CYOTID = @CYOTID
        GROUP BY r.RegistrationID, r.FirstName, r.LastName, cy.ChallengeDate
        ORDER BY TotalScore DESC;";

                var rawData = await _connection.QueryAsync(query, new { CYOTID = cyotId });

                // Convert to in-memory list
                var rawList = rawData.ToList();

                // Rank calculation
                int rank = 1;
                decimal? lastScore = null;

                var leaderboard = new List<LeaderboardResponse>();

                for (int i = 0; i < rawList.Count; i++)
                {
                    var row = rawList[i];
                    decimal score = row.TotalScore;

                    if (lastScore != score)
                    {
                        rank = i + 1;
                        lastScore = score;
                    }

                    leaderboard.Add(new LeaderboardResponse
                    {
                        StudentID = row.StudentID,
                        FirstName = row.FirstName,
                        LastName = row.LastName,
                        Date = row.ChallengeDate,
                        StudentRank = rank
                    });
                }

                // Move requested student to top
                leaderboard = leaderboard
                    .OrderByDescending(x => x.StudentID == studentId)
                    .ThenBy(x => x.StudentRank)
                    .ToList();

                return new ServiceResponse<List<LeaderboardResponse>>(
                    true,
                    "Leaderboard fetched successfully",
                    leaderboard,
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
