using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using System;
using System.Data;
using System.Threading.Tasks;

namespace StudentApp_API.Repository.Implementations
{
    public class ScholarshipRepository : IScholarshipRepository
    {
        private readonly IDbConnection _connection;

        public ScholarshipRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<ServiceResponse<bool>> AssignScholarshipAsync(AssignScholarshipRequest request)
        {
            try
            {
                // Part I: Get CourseID and ClassID
                var courseClassQuery = @"SELECT CourseID, ClassID 
                                         FROM tblStudentClassCourseMapping 
                                         WHERE RegistrationID = @RegistrationID";
                var courseClass = await _connection.QueryFirstOrDefaultAsync(courseClassQuery, new { request.RegistrationID });

                if (courseClass == null)
                {
                    return new ServiceResponse<bool>(false, "No course and class found for this registration ID.", false, 404);
                }

                int courseId = courseClass.CourseID;
                int classId = courseClass.ClassID;

                // Part II: Fetch ScholarshipTest and Questions based on CourseID and ClassID
                var scholarshipQuery = @"SELECT ST.ScholarshipTestId AS ScholarshipID, 
                                                Q.QuestionId AS QuestionID, 
                                                Q.SubjectID AS SubjectID, 
                                                A.QuestionTypeID AS QuestionTypeID
                                         FROM [tblQuestion] Q
                                         LEFT JOIN [tblAnswerMaster] A ON Q.QuestionId = A.QuestionID
                                         LEFT JOIN [tblSSTQuestions] SS ON SS.QuestionId = Q.QuestionId
                                         LEFT JOIN [tblScholarshipTest] ST ON ST.ScholarshipTestId = SS.ScholarshipTestId
                                         LEFT JOIN [tblScholarshipCourse] C ON C.ScholarshipTestId = ST.ScholarshipTestId
                                         LEFT JOIN [tblScholarshipClass] CL ON CL.ScholarshipTestId = ST.ScholarshipTestId
                                         WHERE C.CourseId = @CourseId AND CL.ClassId = @ClassId";

                var scholarshipData = await _connection.QueryAsync(scholarshipQuery, new { courseId, classId });

                // Part III: Insert fetched data into tblStudentScholarship
                string insertQuery = @"INSERT INTO tblStudentScholarship (ScholarshipID, StudentID, QuestionID, SubjectID, QuestionTypeID, ExamDate)
                                       VALUES (@ScholarshipID, @StudentID, @QuestionID, @SubjectID, @QuestionTypeID, @ExamDate)";

                foreach (var data in scholarshipData)
                {
                    await _connection.ExecuteAsync(insertQuery, new
                    {
                        ScholarshipID = data.ScholarshipID,
                        StudentID = request.RegistrationID,
                        QuestionID = data.QuestionID,
                        SubjectID = data.SubjectID,
                        QuestionTypeID = data.QuestionTypeID,
                        ExamDate = DateTime.Now
                    });
                }

                return new ServiceResponse<bool>(true, "Scholarship assigned successfully.", true, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }

        public async Task<ServiceResponse<GetScholarshipTestResponseWrapper>> GetScholarshipTestAsync(GetScholarshipTestRequest request)
        {
            try
            {
                // Get subjects and questions related to the student and scholarship
                string subjectQuery = @"SELECT DISTINCT SS.SubjectID, S.SubjectName
                                        FROM tblStudentScholarship SS
                                        JOIN tblSubject S ON SS.SubjectID = S.SubjectID
                                        WHERE SS.StudentID = @StudentID AND SS.ScholarshipID = @ScholarshipID";

                var subjects = await _connection.QueryAsync(subjectQuery, new { request.StudentID, request.ScholarshipID });

                if (subjects == null || !subjects.Any())
                {
                    return new ServiceResponse<GetScholarshipTestResponseWrapper>(false, "No subjects found for this student and scholarship.", null, 404);
                }

                List<GetScholarshipTestResponse> responseList = new List<GetScholarshipTestResponse>();

                foreach (var subject in subjects)
                {
                    var subjectResponse = new GetScholarshipTestResponse
                    {
                        SubjectID = subject.SubjectID,
                        SubjectName = subject.SubjectName,
                        Questions = new List<QuestionDetail>()
                    };

                    string questionQuery = @"SELECT SS.QuestionID, Q.QuestionDescription
                                             FROM tblStudentScholarship SS
                                             JOIN tblQuestion Q ON SS.QuestionID = Q.QuestionID
                                             WHERE SS.StudentID = @StudentID AND SS.ScholarshipID = @ScholarshipID AND SS.SubjectID = @SubjectID";

                    var questions = await _connection.QueryAsync(questionQuery, new { request.StudentID, request.ScholarshipID, SubjectID = subject.SubjectID });

                    foreach (var question in questions)
                    {
                        var questionDetail = new QuestionDetail
                        {
                            QuestionID = question.QuestionID,
                            Question = question.QuestionDescription,
                            Answers = new List<AnswerDetail>()
                        };

                        string answerQuery = @"SELECT AM.AnswerID, COALESCE(MC.Answer, SA.Answer) AS Answer
                                               FROM tblAnswerMaster AM
                                               LEFT JOIN tblAnswerMultipleChoiceCategory MC ON AM.AnswerID = MC.AnswerID
                                               LEFT JOIN tblAnswersingleanswercategory SA ON AM.AnswerID = SA.AnswerID
                                               WHERE AM.QuestionID = @QuestionID";

                        var answers = await _connection.QueryAsync(answerQuery, new { QuestionID = question.QuestionID });

                        foreach (var answer in answers)
                        {
                            questionDetail.Answers.Add(new AnswerDetail
                            {
                                AnswerID = answer.AnswerID,
                                Answer = answer.Answer
                            });
                        }

                        subjectResponse.Questions.Add(questionDetail);
                    }

                    responseList.Add(subjectResponse);
                }

                // Wrap the responseList in GetScholarshipTestResponseWrapper
                var wrapperResponse = new GetScholarshipTestResponseWrapper
                {
                    ScholarshipDetails = responseList
                };

                return new ServiceResponse<GetScholarshipTestResponseWrapper>(true, "Data retrieved successfully.", wrapperResponse, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<GetScholarshipTestResponseWrapper>(false, ex.Message, null, 500);
            }
        }

        public async Task<ServiceResponse<UpdateQuestionNavigationResponse>> UpdateQuestionNavigationAsync(UpdateQuestionNavigationRequest request)
        {
            try
            {
                string query = @"INSERT INTO tblQuestionNavigation (QuestionID, StartTime, EndTime, ScholarshipID, StudentID)
                                 VALUES (@QuestionID, @StartTime, @EndTime, @ScholarshipID, @StudentID);
                                 SELECT CAST(SCOPE_IDENTITY() AS INT);";

                var navigationId = await _connection.ExecuteScalarAsync<int>(query, new
                {
                    request.QuestionID,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    request.ScholarshipID,
                    request.StudentID
                });

                var response = new UpdateQuestionNavigationResponse
                {
                    NavigationID = navigationId,
                    ScholarshipID = request.ScholarshipID,
                    StudentID = request.StudentID,
                    QuestionID = request.QuestionID,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    Message = "Navigation updated successfully."
                };

                return new ServiceResponse<UpdateQuestionNavigationResponse>(true, "Navigation updated successfully.", response, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<UpdateQuestionNavigationResponse>(false, ex.Message, null, 500);
            }
        }
    }
}
