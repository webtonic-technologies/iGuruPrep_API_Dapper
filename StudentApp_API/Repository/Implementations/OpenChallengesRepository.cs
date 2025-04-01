using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using System.Data;

namespace StudentApp_API.Repository.Implementations
{
    public class OpenChallengesRepository: IOpenChallengesRepository
    {
        private readonly IDbConnection _connection;

        public OpenChallengesRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<List<CYOTResponse>>> GetOpenChallengesAsync(CYOTListRequest request)
        {
            try
            {
                var query = @"SELECT C.CYOTID, 
       C.ChallengeName AS CYOTName, 
       C.NoOfQuestions AS TotalQuestions, 
       C.Duration, 
       C.CYOTStatusID, 
       CS.CYOTStatus, 
       C.CreatedOn,
       CASE WHEN P.CYOTStatusID = 2 THEN 1 ELSE 0 END AS IsViewKey,
       CASE WHEN P.CYOTStatusID = 2 THEN 1 ELSE 0 END AS IsAnalytics
FROM tblCYOT C
JOIN tblRegistration R ON C.CreatedBy = R.RegistrationID
LEFT JOIN tblStudentClassCourseMapping SCM ON R.RegistrationID = SCM.RegistrationID
LEFT JOIN tblCYOTParticipant P ON C.CYOTID = P.CYOTID AND P.StudentID = @StudentID
LEFT JOIN CYOTStatus CS ON P.CYOTStatusID = CS.CYOTStatusID
WHERE 
    ((SCM.ClassID = (SELECT ClassID FROM tblStudentClassCourseMapping WHERE RegistrationID = @StudentID) 
      AND C.CreatedBy <> @StudentID) 
    AND C.CYOTStatusID = 3)
    AND (@StatusId = 0 OR 
         (@StatusId = 1 AND (P.CYOTStatusID IS NULL OR P.CYOTStatusID = 1)) OR 
         (@StatusId = 2 AND P.CYOTStatusID = 2))
ORDER BY C.CreatedOn DESC;";

                var result = await _connection.QueryAsync<CYOTResponse>(query, new
                {
                    StudentID = request.RegistrationId,
                    StatusId = request.StatusId
                });

                return new ServiceResponse<List<CYOTResponse>>(
                    true,
                    "Challenges fetched successfully",
                    result.ToList(),
                    200
                );
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<CYOTResponse>>(
                    false,
                    ex.Message,
                    null,
                    500
                );
            }
        }
        public async Task<ServiceResponse<bool>> StartChallengeAsync(int studentId, int cyotId)
        {
            try
            {
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open(); // Open the connection if closed

                using (var transaction = _connection.BeginTransaction()) // Start transaction
                {
                    // Step 1: Update challenge status in tblCYOTParticipant
                    var updateQuery = @"
                UPDATE tblCYOTParticipant
                SET CYOTStatusID = 2, IsStarted = 1
                WHERE StudentID = @StudentID AND CYOTID = @CYOTID;";

                    var rowsAffected = await _connection.ExecuteAsync(updateQuery, new { StudentID = studentId, CYOTID = cyotId }, transaction);

                    if (rowsAffected == 0)
                    {
                        transaction.Rollback();
                        _connection.Close();
                        return new ServiceResponse<bool>(false, "No challenge found to update", false, 404);
                    }

                    // Step 2: Fetch all questions mapped to the given CYOTID along with SubjectID
                    var fetchQuestionsQuery = @"
                SELECT Q.QuestionID, Q.SubjectID
                FROM tblCYOTQuestions CQ
                JOIN tblQuestion Q ON CQ.QuestionID = Q.QuestionID
                WHERE CQ.CYOTID = @CYOTID;";

                    var questions = await _connection.QueryAsync<(int QuestionID, int SubjectID)>(fetchQuestionsQuery, new { CYOTID = cyotId }, transaction);

                    if (!questions.Any())
                    {
                        transaction.Rollback();
                        _connection.Close();
                        return new ServiceResponse<bool>(false, "No questions found for this challenge", false, 404);
                    }

                    // Step 3: Insert student-question mappings (if not already mapped)
                    var insertQuery = @"
                INSERT INTO tblCYOTStudentQuestionMapping (CYOTId, StudentId, QuestionId, QuestionStatusId, SubjectId)
                SELECT @CYOTID, @StudentID, Q.QuestionID, 4, Q2.SubjectID
                FROM tblCYOTQuestions Q
                JOIN tblQuestion Q2 ON Q.QuestionID = Q2.QuestionID
                WHERE Q.CYOTID = @CYOTID
                AND NOT EXISTS (
                    SELECT 1 FROM tblCYOTStudentQuestionMapping 
                    WHERE CYOTID = @CYOTID AND StudentID = @StudentID AND QuestionID = Q.QuestionID
                );";

                    await _connection.ExecuteAsync(insertQuery, new { StudentID = studentId, CYOTID = cyotId }, transaction);

                    transaction.Commit();
                    _connection.Close();
                    return new ServiceResponse<bool>(true, "Challenge started and questions mapped successfully", true, 200);
                }
            }
            catch (Exception ex)
            {
                _connection.Close();
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }
    }
}
