using System.Data;
using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
namespace StudentApp_API.Repository.Implementations
{
    public class BoardPapersRepositiry : IBoardPapersRepository
    {
        private readonly IDbConnection _connection;

        public BoardPapersRepositiry(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<List<TestSeriesSubjectsResponse>>> GetAllTestSeriesSubjects(int RegistrationId)
        {
            List<TestSeriesSubjectsResponse> response = new List<TestSeriesSubjectsResponse>();

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            try
            {
                // Step 1: Fetch board, class, and course ID using RegistrationId
                string queryStudentMapping = @"
            SELECT CourseID, ClassID, BoardId
            FROM tblStudentClassCourseMapping
            WHERE RegistrationID = @RegistrationId";

                var studentMapping = await _connection.QueryFirstOrDefaultAsync(queryStudentMapping, new { RegistrationId });

                if (studentMapping == null)
                {
                    return new ServiceResponse<List<TestSeriesSubjectsResponse>>(false, "No mapping found for the given RegistrationId", null, 404);
                }

                int courseId = studentMapping.CourseID;
                int classId = studentMapping.ClassID;
                int boardId = studentMapping.BoardId;

                // Step 2: Fetch Test Series matching the class, course, and board
                string queryTestSeries = @"
            SELECT ts.TestSeriesId
            FROM tblTestSeries ts
            INNER JOIN tblTestSeriesBoards tsb ON ts.TestSeriesId = tsb.TestSeriesId
            INNER JOIN tblTestSeriesClass tsc ON ts.TestSeriesId = tsc.TestSeriesId
            INNER JOIN tblTestSeriesCourse tscs ON ts.TestSeriesId = tscs.TestSeriesId
            WHERE tsb.BoardId = @BoardId AND tsc.ClassId = @ClassId AND tscs.CourseId = @CourseId AND ts.TypeOfTestSeries = 1002";

                var testSeriesIds = (await _connection.QueryAsync<int>(queryTestSeries, new
                {
                    BoardId = boardId,
                    ClassId = classId,
                    CourseId = courseId
                })).ToList();

                if (!testSeriesIds.Any())
                {
                    return new ServiceResponse<List<TestSeriesSubjectsResponse>>(false, "No test series found for the given class, course, and board combination", null, 404);
                }

                // Step 3: Fetch subjects and count of test series grouped by subjects
                string querySubjects = @"
            SELECT s.SubjectId, s.SubjectName, COUNT(tsm.TestSeriesId) AS Count
            FROM tblSubject s
            INNER JOIN tblTestSeriesSubjects tsm ON s.SubjectId = tsm.SubjectId
            WHERE tsm.TestSeriesId IN @TestSeriesIds
            GROUP BY s.SubjectId, s.SubjectName";

                response = (await _connection.QueryAsync<TestSeriesSubjectsResponse>(querySubjects, new
                {
                    TestSeriesIds = testSeriesIds
                })).ToList();

                return new ServiceResponse<List<TestSeriesSubjectsResponse>>(true, "Success", response, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<TestSeriesSubjectsResponse>>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<List<TestSeriesResponse>>> GetTestSeriesBySubjectId(int subjectId)
        {
            List<TestSeriesResponse> response = new List<TestSeriesResponse>();

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            try
            {
                // Step 1: Fetch the TestSeriesIds associated with the given SubjectId
                string queryTestSeriesIds = @"
            SELECT tsm.TestSeriesId
            FROM [tblTestSeriesSubjects] tsm
            WHERE tsm.SubjectId = @SubjectId";

                var testSeriesIds = (await _connection.QueryAsync<int>(queryTestSeriesIds, new { SubjectId = subjectId })).ToList();

                if (!testSeriesIds.Any())
                {
                    return new ServiceResponse<List<TestSeriesResponse>>(false, "No test series found for the given SubjectId", null, 404);
                }

                // Step 2: Fetch details of test series from tblTestSeries using the retrieved TestSeriesIds
                string queryTestSeries = @"
            SELECT ts.TestSeriesId, ts.NameOfExam AS TestPatternName, ts.Duration, ts.StartDate, ts.StartTime, ts.ResultDate, ts.ResultTime, ts.TotalNoOfQuestions
            FROM tblTestSeries ts
            WHERE ts.TestSeriesId IN @TestSeriesIds";

                response = (await _connection.QueryAsync<TestSeriesResponse>(queryTestSeries, new
                {
                    TestSeriesIds = testSeriesIds
                })).ToList();

                return new ServiceResponse<List<TestSeriesResponse>>(true, "Success", response, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<TestSeriesResponse>>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<List<TestSeriesQuestionResponse>>> GetTestSeriesDescriptiveQuestions(TestSeriesQuestionRequest request)
        {
            List<TestSeriesQuestionResponse> response = new List<TestSeriesQuestionResponse>();

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            try
            {
                // Step 1: Fetch QuestionIds associated with the TestSeriesId from tbltestseriesQuestions
                string queryTestSeriesQuestions = @"
            SELECT tsq.Questionid
            FROM tbltestseriesQuestions tsq
            WHERE tsq.TestSeriesid = @TestSeriesId";

                var questionIds = (await _connection.QueryAsync<int>(queryTestSeriesQuestions, new { TestSeriesId = request.TestSeriesId })).ToList();

                if (!questionIds.Any())
                {
                    return new ServiceResponse<List<TestSeriesQuestionResponse>>(false, "No questions found for the given TestSeriesId", null, 404);
                }

                // Step 2: Fetch questions from tblQuestion associated with the SubjectId and filter for descriptive types (QuestionTypeId IN (3, 7, 8))
                string queryDescriptiveQuestions = @"
            SELECT q.QuestionId, q.QuestionDescription, q.QuestionFormula, q.QuestionImage, q.DifficultyLevelId, 
                   q.QuestionTypeId, q.Explanation
            FROM tblQuestion q
            WHERE q.QuestionId IN @QuestionIds
              AND q.SubjectID = @SubjectId
              AND q.QuestionTypeId IN (3, 7, 8) -- SA, LA, VSA";

                var questions = (await _connection.QueryAsync<TestSeriesQuestionResponse>(queryDescriptiveQuestions, new
                {
                    QuestionIds = questionIds,
                    SubjectId = request.SubjectId
                })).ToList();

                if (!questions.Any())
                {
                    return new ServiceResponse<List<TestSeriesQuestionResponse>>(false, "No descriptive questions found for the given criteria", null, 404);
                }

                // Step 3: Fetch the answers for each question from tblAnswerMaster and tblAnswersingleanswercategory
                foreach (var question in questions)
                {
                    string queryAnswers = @"
                SELECT am.Answerid, sac.Answer
                FROM tblAnswerMaster am
                INNER JOIN tblAnswersingleanswercategory sac ON am.Answerid = sac.Answerid
                WHERE am.Questionid = @QuestionId";

                    var answers = await _connection.QueryAsync<AnswerResponses>(queryAnswers, new { QuestionId = question.QuestionId });

                    question.Answers = answers.ToList();
                }

                return new ServiceResponse<List<TestSeriesQuestionResponse>>(true, "Success", questions, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<TestSeriesQuestionResponse>>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<string>> MarkQuestionAsRead(SaveQuestionRequest request)
        {
            try
            {
                // Check if the record already exists
                string checkQuery = @"
            SELECT COUNT(1)
            FROM tblBoardPaperQuestionRead
            WHERE StudentID = @StudentId
              AND QuestionID = @QuestionId
              AND QuestionCode = @QuestionCode";

                int recordExists = await _connection.ExecuteScalarAsync<int>(checkQuery, new
                {
                    request.StudentId,
                    request.QuestionId,
                    request.QuestionCode
                });

                if (recordExists > 0)
                {
                    // If record exists, delete it
                    string deleteQuery = @"
                DELETE FROM tblBoardPaperQuestionRead
                WHERE StudentID = @StudentId
                  AND QuestionID = @QuestionId
                  AND QuestionCode = @QuestionCode";

                    await _connection.ExecuteAsync(deleteQuery, new
                    {
                        request.StudentId,
                        request.QuestionId,
                        request.QuestionCode
                    });

                    return new ServiceResponse<string>(true, "Question marked as unread (deleted).", null, 200);
                }
                else
                {
                    // If record does not exist, insert it
                    string insertQuery = @"
                INSERT INTO tblBoardPaperQuestionRead (StudentID, QuestionID, QuestionCode)
                VALUES (@StudentId, @QuestionId, @QuestionCode)";

                    await _connection.ExecuteAsync(insertQuery, new
                    {
                        request.StudentId,
                        request.QuestionId,
                        request.QuestionCode
                    });

                    return new ServiceResponse<string>(true, "Question marked as read (inserted).", null, 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, $"An error occurred: {ex.Message}", null, 500);
            }
        }
        public async Task<ServiceResponse<string>> MarkQuestionAsSave(SaveQuestionRequest request)
        {
            try
            {
                // Check if the record already exists
                string checkQuery = @"
            SELECT COUNT(1)
            FROM tblBoardPaperQuestionSave
            WHERE StudentID = @StudentId
              AND QuestionID = @QuestionId
              AND QuestionCode = @QuestionCode";

                int recordExists = await _connection.ExecuteScalarAsync<int>(checkQuery, new
                {
                    request.StudentId,
                    request.QuestionId,
                    request.QuestionCode
                });

                if (recordExists > 0)
                {
                    // If record exists, delete it
                    string deleteQuery = @"
                DELETE FROM tblBoardPaperQuestionSave
                WHERE StudentID = @StudentId
                  AND QuestionID = @QuestionId
                  AND QuestionCode = @QuestionCode";

                    await _connection.ExecuteAsync(deleteQuery, new
                    {
                        request.StudentId,
                        request.QuestionId,
                        request.QuestionCode
                    });

                    return new ServiceResponse<string>(true, "Question unsaved (deleted).", null, 200);
                }
                else
                {
                    // If record does not exist, insert it
                    string insertQuery = @"
                INSERT INTO tblBoardPaperQuestionSave (StudentID, QuestionID, QuestionCode)
                VALUES (@StudentId, @QuestionId, @QuestionCode)";

                    await _connection.ExecuteAsync(insertQuery, new
                    {
                        request.StudentId,
                        request.QuestionId,
                        request.QuestionCode
                    });

                    return new ServiceResponse<string>(true, "Question saved (inserted).", null, 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, $"An error occurred: {ex.Message}", null, 500);
            }
        }
    }
}
