using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using System.Data;

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
                var scholarshipData = await GetScholarshipTestByRegistrationId(request.RegistrationID);

                var scholarshipQuestion = await GetQuestionsBySectionSettings(scholarshipData.Data.ScholarshipTest.ScholarshipTestId);

                string insertQuery = @"INSERT INTO tblStudentScholarship (ScholarshipID, StudentID, QuestionID, SubjectID, QuestionTypeID, ExamDate)
                                       VALUES (@ScholarshipID, @StudentID, @QuestionID, @SubjectID, @QuestionTypeID, @ExamDate)";

                foreach (var data in scholarshipQuestion.Data)
                {
                    await _connection.ExecuteAsync(insertQuery, new
                    {
                        ScholarshipID = scholarshipData.Data.ScholarshipTest.ScholarshipTestId,
                        StudentID = request.RegistrationID,
                        QuestionID = data.QuestionId,
                        SubjectID = data.subjectID,
                        QuestionTypeID = data.QuestionTypeId,
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
                                        WHERE SS.StudentID = @RegistrationId AND SS.ScholarshipID = @ScholarshipID";

                var subjects = await _connection.QueryAsync(subjectQuery, new { request.RegistrationId, request.ScholarshipID });

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
                                             WHERE SS.StudentID = @RegistrationId AND SS.ScholarshipID = @ScholarshipID AND SS.SubjectID = @SubjectID";

                    var questions = await _connection.QueryAsync(questionQuery, new { request.RegistrationId, request.ScholarshipID, SubjectID = subject.SubjectID });

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
                                 VALUES (@QuestionID, @StartTime, @EndTime, @ScholarshipID, @RegistrationId);
                                 SELECT CAST(SCOPE_IDENTITY() AS INT);";

                var navigationId = await _connection.ExecuteScalarAsync<int>(query, new
                {
                    request.QuestionID,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    request.ScholarshipID,
                    request.RegistrationId
                });

                var response = new UpdateQuestionNavigationResponse
                {
                    NavigationID = navigationId,
                    ScholarshipID = request.ScholarshipID,
                    StudentID = request.RegistrationId,
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
        public async Task<ServiceResponse<ScholarshipTestResponse>> GetScholarshipTestByRegistrationId(int registrationId)
        {
            var response = new ServiceResponse<ScholarshipTestResponse>(true,string.Empty,null,200);

            // Step 1: Fetch Board, Class, Course based on RegistrationId
            string queryMapping = @"
            SELECT TOP 1 [BoardId], [ClassID], [CourseID]
            FROM [tblStudentClassCourseMapping]
            WHERE [RegistrationID] = @RegistrationId";

            var studentMapping = await _connection.QueryFirstOrDefaultAsync<StudentClassCourseMappings>(
                queryMapping, new { RegistrationId = registrationId });

            if (studentMapping == null)
            {
                response.Success = false;
                response.Message = "No mapping found for the given Registration ID.";
                return response;
            }

            // Step 2: Fetch Scholarship Test based on Board, Class, Course
            string queryScholarshipTest = @"
            SELECT TOP 1 st.[ScholarshipTestId], st.[APID], st.[ExamTypeId], st.[PatternName], 
                         st.[TotalNumberOfQuestions], st.[Duration], st.[Status], st.[createdon], 
                         st.[createdby], st.[modifiedon], st.[modifiedby], st.[EmployeeID]
            FROM [tblScholarshipTest] st
            INNER JOIN [tblScholarshipBoards] sb ON st.[ScholarshipTestId] = sb.[ScholarshipTestId]
            INNER JOIN [tblScholarshipClass] sc ON st.[ScholarshipTestId] = sc.[ScholarshipTestId]
            INNER JOIN [tblScholarshipCourse] scc ON st.[ScholarshipTestId] = scc.[ScholarshipTestId]
            WHERE sb.[BoardId] = @BoardId AND sc.[ClassId] = @ClassId AND scc.[CourseId] = @CourseId";

            var scholarshipTest = await _connection.QueryFirstOrDefaultAsync<ScholarshipTest>(
                queryScholarshipTest,
                new { studentMapping.BoardId, studentMapping.ClassId, studentMapping.CourseId });

            if (scholarshipTest == null)
            {
                response.Success = false;
                response.Message = "No scholarship test found for the given combination.";
                return response;
            }

            // Step 3: Fetch Instructions associated with the Scholarship Test
            string queryInstructions = @"
            SELECT [SSTInstructionsId], [Instructions], [ScholarshipTestId], [InstructionName], [InstructionId]
            FROM [tblSSTInstructions]
            WHERE [ScholarshipTestId] = @ScholarshipTestId";

            var instructions = await _connection.QueryAsync<ScholarshipTestInstruction>(
                queryInstructions, new { ScholarshipTestId = scholarshipTest.ScholarshipTestId });

            // Combine Scholarship Test and Instructions into response
            var result = new ScholarshipTestResponse
            {
                ScholarshipTest = scholarshipTest,
                Instructions = instructions.ToList()
            };

            response.Data = result;
            response.Success = true;
            return response;
        }
        public async Task<ServiceResponse<List<SubjectQuestionCountResponse>>> GetScholarshipSubjectQuestionCount(int scholarshipTestId)
        {
            var response = new ServiceResponse<List<SubjectQuestionCountResponse>>(true, string.Empty, [],200);
           
                string query = @"
            WITH SubjectQuestionCount AS (
                SELECT 
                    s.SubjectId,
                    SUM(qs.TotalNumberOfQuestions) AS TotalQuestions
                FROM tblScholarshipSubject s
                INNER JOIN tblSSQuestionSection qs 
                    ON s.ScholarshipTestId = qs.ScholarshipTestId 
                    AND s.SubjectId = qs.SubjectId
                WHERE s.ScholarshipTestId = @ScholarshipTestId
                GROUP BY s.SubjectId
            )
            SELECT 
                s.SubjectId, 
                s.SubjectName, 
                qc.TotalQuestions
            FROM SubjectQuestionCount qc
            INNER JOIN tblSubject s 
                ON qc.SubjectId = s.SubjectId;";

                var result = await _connection.QueryAsync<SubjectQuestionCountResponse>(
                    query, new { ScholarshipTestId = scholarshipTestId });

                if (result != null && result.Any())
                {
                    response.Data = result.ToList();
                    response.Success = true;
                }
                else
                {
                    response.Success = false;
                    response.Message = "No subjects found for the given ScholarshipTestId.";
                }
            return response;
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuestionsBySectionSettings(int scholarshipTestId)
        {
            ServiceResponse<List<QuestionResponseDTO>> response = new ServiceResponse<List<QuestionResponseDTO>>(true, string.Empty, [], 200);
            try
            {
                // Fetch the sections with their question type and question count settings
                string sectionQuery = @"
            SELECT 
                qs.SSTSectionId, 
                qs.SectionName, 
                qs.QuestionTypeId, 
                qs.TotalNumberOfQuestions, 
                qs.SubjectId 
            FROM tblSSQuestionSection qs
            WHERE qs.ScholarshipTestId = @ScholarshipTestId";

                var sections = await _connection.QueryAsync<SectionSettingDTO>(sectionQuery, new { ScholarshipTestId = scholarshipTestId });

                // Initialize the question list
                List<QuestionResponseDTO> questionsList = new List<QuestionResponseDTO>();

                // Loop through each section to fetch the corresponding questions
                foreach (var section in sections)
                {
                    // Determine if the question type is single or multiple answer
                    var isSingleAnswer = section.QuestionTypeId == 3 || section.QuestionTypeId == 7 ||
                                         section.QuestionTypeId == 8 || section.QuestionTypeId == 10;
                    string questionQuery = @"
                    SELECT q.*, 
                           c.CourseName, 
                           b.BoardName, 
                           cl.ClassName, 
                           s.SubjectName,
                           et.ExamTypeName,
                           e.EmpFirstName,
                           qt.QuestionType as QuestionTypeName,
                           it.IndexType as IndexTypeName,
                           CASE 
                               WHEN q.IndexTypeId = 1 THEN ci.ContentName_Chapter
                               WHEN q.IndexTypeId = 2 THEN ct.ContentName_Topic
                               WHEN q.IndexTypeId = 3 THEN cst.ContentName_SubTopic
                           END AS ContentIndexName
                    FROM tblQuestion q
                    LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
                    LEFT JOIN tblCourse c ON q.courseid = c.CourseID
                    LEFT JOIN tblBoard b ON q.boardid = b.BoardID
                    LEFT JOIN tblClass cl ON q.classid = cl.ClassID
                    LEFT JOIN tblSubject s ON q.subjectID = s.SubjectID
                    LEFT JOIN tblExamType et ON q.ExamTypeId = et.ExamTypeId
                    LEFT JOIN tblEmployee e ON q.EmployeeId = e.EmployeeId
                    LEFT JOIN tblQBIndexType it ON q.IndexTypeId = it.IndexId
                    LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
                    LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
                    LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
                    WHERE q.IsActive = 1 
                      AND q.SubjectID = @SubjectId 
                      AND q.QuestionTypeId = @QuestionTypeId 
                      AND q.IsConfigure = 1
                    ORDER BY q.CreatedOn
                    OFFSET 0 ROWS FETCH NEXT @TotalNumberOfQuestions ROWS ONLY";

                    var questions = await _connection.QueryAsync<QuestionResponseDTO>(questionQuery,
                        new { SubjectId = section.SubjectId, QuestionTypeId = section.QuestionTypeId, TotalNumberOfQuestions = section.TotalNumberOfQuestions });

                    // Loop through each question to fetch its answers
                    foreach (var question in questions)
                    {
                        if (isSingleAnswer)
                        {
                            // Fetch the single-answer category
                            string singleAnswerQuery = @"
                        SELECT 
                            a.Answersingleanswercategoryid, 
                            a.Answerid, 
                            a.Answer 
                        FROM tblAnswersingleanswercategory a
                        INNER JOIN tblAnswerMaster am ON am.Answerid = a.Answerid
                        WHERE am.Questionid = @QuestionId";

                            question.Answersingleanswercategories = await _connection.QuerySingleOrDefaultAsync<Answersingleanswercategory>(singleAnswerQuery, new { QuestionId = question.QuestionId });
                        }
                        else
                        {
                            // Fetch the multiple-answer category
                            string multipleAnswerQuery = @"
                        SELECT 
                            a.Answermultiplechoicecategoryid, 
                            a.Answerid, 
                            a.Answer, 
                            a.Iscorrect, 
                            a.Matchid 
                        FROM tblAnswerMultipleChoiceCategory a
                        INNER JOIN tblAnswerMaster am ON am.Answerid = a.Answerid
                        WHERE am.Questionid = @QuestionId";

                            question.AnswerMultipleChoiceCategories = (await _connection.QueryAsync<AnswerMultipleChoiceCategory>(multipleAnswerQuery, new { QuestionId = question.QuestionId })).ToList();
                        }
                    }

                    // Add questions to the list
                    questionsList.AddRange(questions);
                }

                // Return the fetched questions list
                response.Data = questionsList;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }
        public async Task<ServiceResponse<List<MarksAcquiredAfterAnswerSubmission>>> SubmitAnswer(List<AnswerSubmissionRequest> request)
        {
            try
            {
                // Validate the request
                if (request == null || !request.Any())
                {
                    return new ServiceResponse<List<MarksAcquiredAfterAnswerSubmission>>(false, "Request is empty or invalid.", null, 400);
                }

                // Fetch section ID based on the subject ID
                var sectionQuery = @"
            SELECT SSTSectionId
            FROM tblSSQuestionSection
            WHERE SubjectId = @SubjectID";

                var sectionId = await _connection.QueryFirstOrDefaultAsync<int>(sectionQuery, new { SubjectID = request.FirstOrDefault()?.SubjectID });

                if (sectionId == 0)
                {
                    return new ServiceResponse<List<MarksAcquiredAfterAnswerSubmission>>(false, "Section not found for the given subject.", null, 404);
                }

                // Prepare the response list
                var responses = new List<MarksAcquiredAfterAnswerSubmission>();

                foreach (var answer in request)
                {
                    // Fetch question and answer details
                    var query = @"
                SELECT q.QuestionId, 
                       q.QuestionTypeId, 
                       a.Answerid AS CorrectAnswerId, 
                       s.MarksPerQuestion, 
                       s.NegativeMarks
                FROM tblQuestion q
                LEFT JOIN tblAnswerMaster a ON q.QuestionId = a.Questionid
                LEFT JOIN tblSSQuestionSection s ON s.SSTSectionId = @SectionID
                WHERE q.QuestionId = @QuestionID";

                    var questionData = await _connection.QueryFirstOrDefaultAsync<QuestionAnswerData>(query, new
                    {
                        QuestionID = answer.QuestionID,
                        SectionID = sectionId
                    });

                    if (questionData == null)
                        continue;

                    decimal marksAwarded = 0;
                    string answerStatus = "Incorrect";

                    // Handle single answer types
                    if (IsSingleAnswerType(questionData.QuestionTypeId))
                    {
                        var singleAnswerQuery = @"
                    SELECT sac.Answerid
                    FROM tblAnswersingleanswercategory sac
                    INNER JOIN tblAnswerMaster am ON sac.Answerid = am.Answerid
                    WHERE am.Questionid = @QuestionID";

                        var correctAnswerId = await _connection.QueryFirstOrDefaultAsync<int>(singleAnswerQuery, new { QuestionID = answer.QuestionID });

                        if (correctAnswerId == answer.AnswerID.FirstOrDefault())
                        {
                            marksAwarded = questionData.MarksPerQuestion;
                            answerStatus = "Correct";
                        }
                        else
                        {
                            marksAwarded = -questionData.NegativeMarks;
                        }
                    }
                    // Handle multiple-answer types
                    else
                    {
                        var multipleAnswersQuery = @"
                    SELECT amc.Answermultiplechoicecategoryid
                    FROM tblAnswerMultipleChoiceCategory amc
                    INNER JOIN tblAnswerMaster am ON amc.Answerid = am.Answerid
                    WHERE am.Questionid = @QuestionID AND amc.IsCorrect = 1";

                        var correctAnswers = await _connection.QueryAsync<int>(multipleAnswersQuery, new { QuestionID = answer.QuestionID });

                        var actualCorrectCount = correctAnswers.Count();
                        var studentCorrectCount = answer.AnswerID.Intersect(correctAnswers).Count();

                        var (partialMarks, successRate) = await CalculatePartialMarksAsync(
                            actualCorrectCount,
                            studentCorrectCount,
                            questionData.NegativeMarks > 0);

                        marksAwarded = partialMarks;
                        answerStatus = successRate == 1 ? "Correct" : successRate > 0 ? "PartialCorrect" : "Incorrect";
                    }

                    // Check for existing submission and remove it
                    var existingRecordQuery = @"
                SELECT COUNT(*) 
                FROM tblStudentScholarshipAnswerSubmission
                WHERE StudentID = @RegistrationId AND QuestionID = @QuestionID AND ScholarshipID = @ScholarshipID";

                    var existingRecordCount = await _connection.ExecuteScalarAsync<int>(existingRecordQuery, new
                    {
                        RegistrationId = answer.RegistrationId,
                        QuestionID = answer.QuestionID,
                        ScholarshipID = answer.ScholarshipID
                    });

                    if (existingRecordCount > 0)
                    {
                        var deleteQuery = @"
                    DELETE FROM tblStudentScholarshipAnswerSubmission
                    WHERE StudentID = @RegistrationId AND QuestionID = @QuestionID AND ScholarshipID = @ScholarshipID";

                        await _connection.ExecuteAsync(deleteQuery, new
                        {
                            RegistrationId = answer.RegistrationId,
                            QuestionID = answer.QuestionID,
                            ScholarshipID = answer.ScholarshipID
                        });
                    }

                    // Insert the new submission
                    var insertQuery = @"
                INSERT INTO tblStudentScholarshipAnswerSubmission 
                    (ScholarshipID, StudentID, QuestionID, SubjectID, QuestionTypeID, AnswerID, AnswerStatus, Marks)
                VALUES 
                    (@ScholarshipID, @RegistrationId, @QuestionID, @SubjectID, @QuestionTypeID, @AnswerID, @AnswerStatus, @Marks)";

                    await _connection.ExecuteAsync(insertQuery, new
                    {
                        ScholarshipID = answer.ScholarshipID,
                        RegistrationId = answer.RegistrationId,
                        QuestionID = answer.QuestionID,
                        SubjectID = answer.SubjectID,
                        QuestionTypeID = answer.QuestionTypeID,
                        AnswerID = string.Join(",", answer.AnswerID),
                        AnswerStatus = answerStatus,
                        Marks = marksAwarded
                    });

                    // Add to the response list
                    responses.Add(new MarksAcquiredAfterAnswerSubmission
                    {
                        RegistrationId = answer.RegistrationId,
                        ScholarshipId = answer.ScholarshipID,
                        QuestionId = answer.QuestionID,
                        MarksAcquired = marksAwarded
                    });
                }

                return new ServiceResponse<List<MarksAcquiredAfterAnswerSubmission>>(true, "Operation successful", responses, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<MarksAcquiredAfterAnswerSubmission>>(false, ex.Message, null, 500);
            }
        }

        private async Task<(decimal acquiredMarks, decimal successRate)> CalculatePartialMarksAsync(
            int actualCorrectCount,
            int studentCorrectCount,
            bool isNegative)
        {
            var query = @"
        SELECT TOP 1 AcquiredMarks, SuccessRate
        FROM tbl_PartialMarksMapping
        WHERE NoOfCorrectOptions = @ActualCorrectCount
          AND NoOfOptionsSelected = @StudentCorrectCount
          AND IsNegative = @IsNegative";

            var result = await _connection.QueryFirstOrDefaultAsync<(decimal AcquiredMarks, decimal SuccessRate)>(query, new
            {
                ActualCorrectCount = actualCorrectCount,
                StudentCorrectCount = studentCorrectCount,
                IsNegative = isNegative
            });

            return result == default ? (0, 0) : result;
        }
        private bool IsSingleAnswerType(int questionTypeId)
        {
            // Assuming the following are single answer type IDs based on your data
            return questionTypeId == 3 || questionTypeId == 7 || questionTypeId == 8 || questionTypeId == 10 || questionTypeId == 11;
        }
    }
}