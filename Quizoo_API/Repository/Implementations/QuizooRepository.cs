using Quizoo_API.DTOs.Response;
using Quizoo_API.Repository.Interfaces;
using System.Data;
using Dapper;
using Quizoo_API.DTOs.ServiceResponse;
using Quizoo_API.DTOs.Request;

namespace Quizoo_API.Repository.Implementations
{
    public class QuizooRepository : IQuizooRepository
    {
        private readonly IDbConnection _connection;

        public QuizooRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<List<SubjectDTO>>> GetSubjectsAsync(int registrationId)
        {
            try
            {
                // Step 1: Fetch Board, Class, and Course using RegistrationID
                var mapping = await _connection.QueryFirstOrDefaultAsync<dynamic>(
                    @"SELECT BoardId, ClassId, CourseId
              FROM tblStudentClassCourseMapping
              WHERE RegistrationID = @RegistrationID",
                    new { RegistrationID = registrationId }) ?? throw new Exception("No mapping found for the given RegistrationID.");

                // Step 2: Fetch SyllabusID using Board, Class, and Course
                var syllabusId = await _connection.QueryFirstOrDefaultAsync<int?>(
                    @"SELECT SyllabusId
              FROM tblSyllabus
              WHERE BoardID = @BoardId AND ClassID = @ClassId AND CourseID = @CourseId AND Status = 1",
                    new { mapping.BoardId, mapping.ClassId, mapping.CourseId });

                if (!syllabusId.HasValue)
                    throw new Exception("No syllabus found for the given mapping.");

                // Step 3: Fetch Subjects mapped to the SyllabusID
                var subjects = await _connection.QueryAsync<SubjectDTO>(
                    @"SELECT SS.SubjectID, S.SubjectName
              FROM tblSyllabusSubjects SS
              INNER JOIN tblSubject S ON SS.SubjectID = S.SubjectId
              WHERE SS.SyllabusID = @SyllabusID AND SS.Status = 1 AND S.Status = 1",
                    new { SyllabusID = syllabusId.Value });
                // Null check or empty list check
                if (subjects == null || !subjects.Any())
                    return new ServiceResponse<List<SubjectDTO>>(false, "No subjects found for the given SyllabusID.", new List<SubjectDTO>(), 404);

                return new ServiceResponse<List<SubjectDTO>>(true, "Records found", subjects.ToList(), 200);

            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<SubjectDTO>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<List<ChapterDTO>>> GetChaptersAsync(int registrationId, int subjectId)
        {
            try
            {
                // Step 1: Fetch Board, Class, and Course using RegistrationID
                var mapping = await _connection.QueryFirstOrDefaultAsync<dynamic>(
                    @"SELECT BoardId, ClassId, CourseId
              FROM tblStudentClassCourseMapping
              WHERE RegistrationID = @RegistrationID",
                    new { RegistrationID = registrationId }) ?? throw new Exception("No mapping found for the given RegistrationID.");

                // Step 2: Fetch SyllabusID using Board, Class, and Course
                var actualSyllabusId = await _connection.QueryFirstOrDefaultAsync<int?>(
                    @"SELECT SyllabusId
              FROM tblSyllabus
              WHERE BoardID = @BoardId AND ClassID = @ClassId AND CourseID = @CourseId AND Status = 1",
                    new { mapping.BoardId, mapping.ClassId, mapping.CourseId });

                if (!actualSyllabusId.HasValue)
                    throw new Exception("Syllabus ID does not match or no syllabus found.");

                // Step 3: Fetch Chapters mapped to SyllabusID and SubjectID
                var chapters = await _connection.QueryAsync<ChapterDTO>(
                    @"SELECT C.ContentIndexId AS ChapterId, C.ContentName_Chapter AS ChapterName, C.ChapterCode, C.DisplayOrder
              FROM tblSyllabusDetails S
              INNER JOIN tblContentIndexChapters C ON S.ContentIndexId = C.ContentIndexId
              WHERE S.SyllabusID = @SyllabusID AND S.SubjectID = @SubjectID AND S.IndexTypeId = 1",
                    new { SyllabusID = actualSyllabusId, SubjectID = subjectId });

                if (chapters == null || !chapters.Any())
                    return new ServiceResponse<List<ChapterDTO>>(false, "No chapters found for the given SyllabusID and SubjectID.", new List<ChapterDTO>(), 404);

                return new ServiceResponse<List<ChapterDTO>>(true, "Records found", chapters.ToList(), 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ChapterDTO>>(false, ex.Message, new List<ChapterDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<int>> InsertOrUpdateQuizooAsync(QuizooDTO quizoo)
        {

            try
            {
                int quizooId;

                // Step 1: Insert or Update `tblQuizoo`
                if (quizoo.QuizooID == 0)
                {
                    // Insert
                    var insertQuery = @"
                        INSERT INTO tblQuizoo (
                            QuizooName, QuizooDate, QuizooStartTime, Duration, NoOfQuestions, 
                            NoOfPlayers, QuizooLink, CreatedBy, QuizooDuration, IsSystemGenerated, 
                            ClassID, CourseID, BoardID
                        ) VALUES (
                            @QuizooName, @QuizooDate, @QuizooStartTime, @Duration, @NoOfQuestions, 
                            @NoOfPlayers, @QuizooLink, @CreatedBy, @QuizooDuration, @IsSystemGenerated, 
                            @ClassID, @CourseID, @BoardID
                        ); 
                        SELECT CAST(SCOPE_IDENTITY() as int)";

                    quizooId = await _connection.ExecuteScalarAsync<int>(insertQuery, quizoo);
                }
                else
                {
                    // Update
                    var updateQuery = @"
                        UPDATE tblQuizoo
                        SET 
                            QuizooName = @QuizooName, QuizooDate = @QuizooDate, QuizooStartTime = @QuizooStartTime, 
                            Duration = @Duration, NoOfQuestions = @NoOfQuestions, NoOfPlayers = @NoOfPlayers, 
                            QuizooLink = @QuizooLink, CreatedBy = @CreatedBy, QuizooDuration = @QuizooDuration, 
                            IsSystemGenerated = @IsSystemGenerated, ClassID = @ClassID, CourseID = @CourseID, 
                            BoardID = @BoardID
                        WHERE QuizooID = @QuizooID";

                    await _connection.ExecuteAsync(updateQuery, quizoo);
                    quizooId = quizoo.QuizooID;
                }

                // Step 2: Insert or Update `tblQuizooSyllabus`
                // Insert new syllabus records
                var insertSyllabusQuery = @"
                    INSERT INTO tblQuizooSyllabus (QuizooID, SubjectID, ChapterID)
                    VALUES (@QuizooID, @SubjectID, @ChapterID)";

                foreach (var syllabus in quizoo.QuizooSyllabus)
                {
                    await _connection.ExecuteAsync(insertSyllabusQuery, new
                    {
                        QuizooID = quizooId,
                        SubjectID = syllabus.SubjectID,
                        ChapterID = syllabus.ChapterID
                    });
                }



                return new ServiceResponse<int>(true, "Quizoo inserted/updated successfully.", quizooId, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<int>(false, $"Error: {ex.Message}", 0, 500);
            }
        }
        public async Task<ServiceResponse<bool>> UpdateQuizooSyllabusAsync(int quizooId, List<QuizooSyllabusDTO> syllabusList)
        {

            try
            {
                // Step 1: Delete existing records for the QuizooID
                var deleteQuery = "DELETE FROM tblQuizooSyllabus WHERE QuizooID = @QuizooID";
                await _connection.ExecuteAsync(deleteQuery, new { QuizooID = quizooId });

                // Step 2: Insert new records
                var insertQuery = @"
                    INSERT INTO tblQuizooSyllabus (QuizooID, SubjectID, ChapterID)
                    VALUES (@QuizooID, @SubjectID, @ChapterID)";

                foreach (var syllabus in syllabusList)
                {
                    await _connection.ExecuteAsync(insertQuery, new
                    {
                        QuizooID = quizooId,
                        SubjectID = syllabus.SubjectID,
                        ChapterID = syllabus.ChapterID
                    });
                }

                return new ServiceResponse<bool>(true, "Syllabus updated successfully.", true, 200);
            }
            catch (Exception ex)
            {

                return new ServiceResponse<bool>(false, $"Error: {ex.Message}", false, 500);
            }

        }
        public async Task<ServiceResponse<List<QuizooDTO>>> GetQuizoosByRegistrationIdAsync(int registrationId)
        {
            try
            {
                var query = @"
                SELECT 
                    QuizooID,
                    QuizooName,
                    QuizooDate,
                    QuizooStartTime,
                    Duration,
                    NoOfQuestions,
                    NoOfPlayers,
                    QuizooLink,
                    CreatedBy,
                    QuizooDuration,
                    IsSystemGenerated,
                    ClassID,
                    CourseID,
                    BoardID
                FROM tblQuizoo
                WHERE CreatedBy = @RegistrationId";

                var result = await _connection.QueryAsync<QuizooDTO>(query, new { RegistrationId = registrationId });

                return new ServiceResponse<List<QuizooDTO>>(true, "Quizoos fetched successfully.", result.AsList(), 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuizooDTO>>(false, $"Error: {ex.Message}", [], 500);
            }
        }
        public async Task<ServiceResponse<List<QuizooDTO>>> GetInvitedQuizoosByRegistrationId(int registrationId)
        {
            try
            {
                var query = @"
                SELECT 
                    q.QuizooID,
                    q.QuizooName,
                    q.QuizooDate,
                    q.QuizooStartTime,
                    q.Duration,
                    q.NoOfQuestions,
                    q.NoOfPlayers,
                    q.QuizooLink,
                    q.CreatedBy,
                    q.QuizooDuration,
                    q.IsSystemGenerated,
                    q.ClassID,
                    q.CourseID,
                    q.BoardID
                FROM tblQuizooInvitation qi
                INNER JOIN tblQuizoo q ON qi.QuizooID = q.QuizooID
                WHERE qi.QInvitee = @RegistrationId";

                var result = await _connection.QueryAsync<QuizooDTO>(query, new { RegistrationId = registrationId });

                return new ServiceResponse<List<QuizooDTO>>(true, "Quizoos invited successfully fetched.", result.AsList(), 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuizooDTO>>(false, $"Error: {ex.Message}", [], 500);
            }
        }
    }
}