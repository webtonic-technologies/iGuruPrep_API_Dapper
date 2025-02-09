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
        public async Task<ServiceResponse<ScholarshipTestResponse>> GetScholarshipTestByRegistrationId(int registrationId)
        {
            var response = new ServiceResponse<ScholarshipTestResponse>(true, string.Empty, null, 200);

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
                         st.[createdby], st.[modifiedon], st.[modifiedby], st.[EmployeeID], ds.Discount as Discount
            FROM [tblScholarshipTest] st
            Left join tblSSTDiscountScheme ds on st.ScholarshipTestId = ds.ScholarshipTestId
            INNER JOIN [tblScholarshipBoards] sb ON st.[ScholarshipTestId] = sb.[ScholarshipTestId]
            INNER JOIN [tblScholarshipClass] sc ON st.[ScholarshipTestId] = sc.[ScholarshipTestId]
            INNER JOIN [tblScholarshipCourse] scc ON st.[ScholarshipTestId] = scc.[ScholarshipTestId]
            WHERE sb.[BoardId] = @BoardId AND sc.[ClassId] = @ClassId AND scc.[CourseId] = @CourseId   AND st.[Status] = 1 ORDER BY st.[createdon] DESC";

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
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuestionsBySectionSettings(GetScholarshipQuestionRequest request)
        {
            ServiceResponse<List<QuestionResponseDTO>> response = new ServiceResponse<List<QuestionResponseDTO>>(true, string.Empty, new List<QuestionResponseDTO>(), 200);
            try
            {

                // Initialize the question list
                List<QuestionResponseDTO> questionsList = new List<QuestionResponseDTO>();

                // Step 1: Check if entries already exist for the given RegistrationId
                string checkQuery = "SELECT COUNT(*) FROM tblScholarshipQuestions WHERE RegistrationId = @RegistrationId";

                int existingCount = await _connection.ExecuteScalarAsync<int>(checkQuery, new
                {
                    RegistrationId = request.studentId
                });

                if (existingCount > 0)
                {
                    // Step 2: Fetch existing questions
                    string fetchQuery = @"
    SELECT sq.STQuestionsId, sq.ScholarshipTestId, sq.SubjectId, sq.QuestionId, sq.QuestionCode, sq.RegistrationId, q.QuestionTypeId
    FROM tblScholarshipQuestions sq
    INNER JOIN tblQuestions q ON sq.QuestionId = q.QuestionId
    WHERE sq.RegistrationId = @RegistrationId";

                    questionsList = (await _connection.QueryAsync<QuestionResponseDTO>(fetchQuery, new
                    {
                        RegistrationId = request.studentId
                    })).ToList();

                    // Step 3: Apply filters on QuestionTypeId if provided
                    if (request.QuestionTypeId != null && request.QuestionTypeId.Any())
                    {
                        questionsList = questionsList
                            .Where(q => request.QuestionTypeId.Contains(q.QuestionTypeId))
                            .ToList();
                    }
                }
                else
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

                    var sections = await _connection.QueryAsync<SectionSettingDTO>(sectionQuery, new { ScholarshipTestId = request.scholarshipTestId });


                    // Loop through each section to fetch the corresponding questions
                    foreach (var section in sections)
                    {
                        var contentIndices = (await _connection.QueryAsync<(int ContentIndexId, int IndexTypeId)>(
                   "SELECT [ContentIndexId], [IndexTypeId] FROM [tblScholarshipContentIndex] WHERE [ScholarshipTestId] = @ScholarshipTestId AND [SubjectId] = @SubjectId",
                   new { request.scholarshipTestId, section.SubjectId })).ToList();

                        if (!contentIndices.Any())
                            continue;
                        var contentIndexIds = contentIndices.Select(ci => ci.ContentIndexId).ToList();
                        var indexTypeIds = contentIndices.Select(ci => ci.IndexTypeId).Distinct().ToList();

                        // Step 3: Fetch difficulty level limits
                        string difficultyQuery = @"
            SELECT DifficultyLevelId, QuesPerDiffiLevel
            FROM tblScholarshipQuestionDifficulty
            WHERE SectionId = @SectionId";

                        var difficultyLimits = (await _connection.QueryAsync<dynamic>(difficultyQuery, new { SectionId = section.SSTSectionId }))
                            .ToDictionary(dl => (int)dl.DifficultyLevelId, dl => (int)dl.QuesPerDiffiLevel);

                        if (!difficultyLimits.Any())
                        {
                            return new ServiceResponse<List<QuestionResponseDTO>>(false, "No difficulty level limits found.", [], 404);
                        }
                        var isSingleAnswer = section.QuestionTypeId == 3 || section.QuestionTypeId == 4 ||
                                             section.QuestionTypeId == 8 || section.QuestionTypeId == 7 || section.QuestionTypeId == 9;


                        // Step 5: Fetch questions based on difficulty levels and limits
                        string sql = @"
        SELECT 
            q.QuestionId, q.QuestionCode, q.QuestionDescription, q.QuestionFormula, q.IsLive, q.QuestionTypeId,
            q.Status, q.CreatedBy, q.CreatedOn, q.ModifiedBy, q.ModifiedOn, q.SubjectID, s.SubjectName, 
            q.ExamTypeId, e.ExamTypeName, q.EmployeeId, emp.EmpFirstName as EmployeeName,
            q.IndexTypeId, it.IndexType as IndexTypeName, q.ContentIndexId,
            CASE 
                WHEN q.IndexTypeId = 1 THEN ci.ContentName_Chapter
                WHEN q.IndexTypeId = 2 THEN ct.ContentName_Topic
                WHEN q.IndexTypeId = 3 THEN cst.ContentName_SubTopic
            END AS ContentIndexName
        FROM tblQuestion q
        LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
        LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
        LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
     LEFT JOIN tblSubject s ON q.subjectID = s.SubjectId
     LEFT JOIN tblEmployee emp ON q.EmployeeId = emp.Employeeid
     LEFT JOIN tblExamType e ON q.ExamTypeId = e.ExamTypeID
     LEFT JOIN tblQBIndexType it ON q.IndexTypeId = it.IndexId
        WHERE q.SubjectID = @SubjectId
          AND q.IndexTypeId IN @IndexTypeIds
          AND q.ContentIndexId IN @ContentIndexIds
          AND (@QuestionTypeId = 0 OR q.QuestionTypeId = @QuestionTypeId)
          AND EXISTS (
              SELECT 1 
              FROM tblQIDCourse qc 
              WHERE qc.QuestionCode = q.QuestionCode 
                AND qc.LevelId = @DifficultyLevelId AND qc.CourseID = @CourseID
          ) AND q.IsLive = 1";
                        var coureId = _connection.QueryFirstOrDefault<int>(@"select CourseId from tblScholarshipCourse where ScholarshipTestId = @ScholarshipTestId", new { ScholarshipTestId = request.scholarshipTestId });
                        var selectedQuestions = new List<QuestionResponseDTO>();
                        foreach (var difficultyLimit in difficultyLimits)
                        {
                            int difficultyLevelId = difficultyLimit.Key;
                            int questionsToFetch = difficultyLimit.Value;

                            var difficultyQuestions = (await _connection.QueryAsync<QuestionResponseDTO>(sql, new
                            {
                                SubjectId = section.SubjectId,
                                IndexTypeIds = indexTypeIds,
                                ContentIndexIds = contentIndexIds,
                                QuestionTypeId = section.QuestionTypeId,
                                DifficultyLevelId = difficultyLevelId,
                                CourseID = coureId
                            })).ToList();

                            var randomQuestions = difficultyQuestions
                                .OrderBy(_ => Guid.NewGuid())
                                .Take(questionsToFetch)
                                .ToList();

                            selectedQuestions.AddRange(randomQuestions);
                        }

                        foreach (var question in selectedQuestions)
                        {
                            // Fetch the student's submitted answer
                            string studentAnswerQuery = @"
                SELECT AnswerID, AnswerStatus 
                FROM tblStudentScholarshipAnswerSubmission 
                WHERE StudentID = @StudentId AND QuestionID = @QuestionId AND ScholarshipID = @ScholarshipTestId";

                            var studentAnswer = await _connection.QuerySingleOrDefaultAsync<dynamic>(studentAnswerQuery,
                                new { StudentId = request.studentId, QuestionId = question.QuestionId, ScholarshipTestId = request.scholarshipTestId });

                            if (studentAnswer != null)
                            {
                                question.StudentAnswer = studentAnswer.AnswerID.ToString();
                                //question.IsCorrect = studentAnswer.AnswerStatus;
                            }

                            // Fetch correct answers and additional question details
                            if (isSingleAnswer)
                            {
                                string singleAnswerQuery = @"
                    SELECT 
                        a.Answersingleanswercategoryid, 
                        a.Answerid, 
                        a.Answer 
                    FROM tblAnswersingleanswercategory a
                    INNER JOIN tblAnswerMaster am ON am.Answerid = a.Answerid
                    WHERE am.Questionid = @QuestionId";

                                question.Answersingleanswercategories = await _connection.QuerySingleOrDefaultAsync<Answersingleanswercategory>(singleAnswerQuery, new { QuestionId = question.QuestionId });

                                // Determine correctness if not already fetched
                                if (question.IsCorrect == null)
                                {
                                    question.IsCorrect = question.StudentAnswer == question.Answersingleanswercategories?.Answer;
                                }
                            }
                            else
                            {
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

                                // Determine correctness if not already fetched
                                if (question.IsCorrect == null)
                                {
                                    var correctAnswers = question.AnswerMultipleChoiceCategories
                                        .Where(a => a.Iscorrect)
                                        .Select(a => a.Answermultiplechoicecategoryid)
                                        .ToList();
                                    if (studentAnswer != null)
                                    {
                                        var studentAnswers = question.StudentAnswer?
                                    .Split(',')
                                    .Select(a => int.TryParse(a.Trim(), out var id) ? id : (int?)null)
                                    .Where(id => id.HasValue)
                                    .Select(id => id.Value)
                                    .ToList();

                                        question.IsCorrect = studentAnswers != null
                                            && correctAnswers.All(studentAnswers.Contains)
                                            && studentAnswers.All(correctAnswers.Contains);
                                    }
                                }
                            }
                        }

                        // Add questions to the list
                        questionsList.AddRange(selectedQuestions);
                    }
                    // Step 2: Insert multiple questions using bulk insert
                    string insertQuery = @"
                    INSERT INTO tblScholarshipQuestions (ScholarshipTestId, SubjectId, QuestionId, QuestionCode, RegistrationId) 
                    VALUES (@ScholarshipTestId, @SubjectId, @QuestionId, @QuestionCode, @RegistrationId)";

                    int rowsInserted = await _connection.ExecuteAsync(insertQuery, questionsList.Select(q => new
                    {
                        ScholarshipTestId = request.scholarshipTestId,
                        SubjectId = q.subjectID,
                        QuestionId = q.QuestionId,
                        QuestionCode = q.QuestionCode,
                        RegistrationId = request.studentId
                    }).ToList());

                    if (rowsInserted == 0)
                    {
                        return new ServiceResponse<List<QuestionResponseDTO>>(false, "Failed to insert questions.", null, 500);
                    }
                }

                response.Data = questionsList;
                response.Success = true;
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }
        public async Task<ServiceResponse<bool>> AssignScholarshipAsync(AssignScholarshipRequest request)
        {
            try
            {
                // Step 1: Check if the student has already opted for any scholarship
                var existingScholarship = await _connection.QueryFirstOrDefaultAsync<int>(
                    @"SELECT COUNT(1) 
              FROM tblStudentScholarship
              WHERE StudentID = @RegistrationID",
                    new { RegistrationID = request.RegistrationID });

                // If the student has already opted for a scholarship, return a response indicating so
                if (existingScholarship > 0)
                {
                    return new ServiceResponse<bool>(false, "This student has already opted for a scholarship and cannot opt for another one.", false, 400);
                }

                // Step 2: Get scholarship data based on the student's registration ID
                var scholarshipData = await GetScholarshipTestByRegistrationId(request.RegistrationID);
                var requestbody = new GetScholarshipQuestionRequest
                {
                    scholarshipTestId = scholarshipData.Data.ScholarshipTest.ScholarshipTestId,
                    studentId = request.RegistrationID,
                    QuestionTypeId = null
                };
                // Step 3: Get the questions for the scholarship test
                var scholarshipQuestion = await GetQuestionsBySectionSettings(requestbody);

                // Step 4: Insert the scholarship details for the student
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
        public async Task<ServiceResponse<List<QuestionTypeResponse>>> GetQuestionTypesByScholarshipId(int scholarshipId)
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            try
            {
                // Query to fetch sections for the given ScholarshipId
                string querySections = @"
            SELECT SSTSectionId, ScholarshipTestId, SectionName, QuestionTypeId
            FROM tblSSQuestionSection
            WHERE ScholarshipTestId = @ScholarshipId";

                // Fetch the sections
                var sections = (await _connection.QueryAsync<ScholarshipSectionResponse>(querySections, new { ScholarshipId = scholarshipId })).ToList();

                if (!sections.Any())
                {
                    return new ServiceResponse<List<QuestionTypeResponse>>(false, "No sections found for the given ScholarshipId", null, 404);
                }

                // Extract the unique QuestionTypeIds from the sections
                var questionTypeIds = sections.Select(s => s.QuestionTypeId).Distinct().ToList();

                // Query to fetch question type details
                string queryQuestionTypes = @"
            SELECT QuestionTypeID, QuestionType, Code
            FROM tblQBQuestionType
            WHERE QuestionTypeID IN @QuestionTypeIds";

                // Fetch the question types
                var questionTypes = (await _connection.QueryAsync<QuestionTypeResponse>(queryQuestionTypes, new { QuestionTypeIds = questionTypeIds })).ToList();

                if (!questionTypes.Any())
                {
                    return new ServiceResponse<List<QuestionTypeResponse>>(false, "No question types found for the given ScholarshipId", null, 404);
                }

                return new ServiceResponse<List<QuestionTypeResponse>>(true, "Success", questionTypes, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionTypeResponse>>(false, ex.Message, null, 500);
            }
        }
        //public async Task<ServiceResponse<bool>> AssignScholarshipAsync(AssignScholarshipRequest request)
        //{
        //    try
        //    {
        //        var scholarshipData = await GetScholarshipTestByRegistrationId(request.RegistrationID);

        //        var scholarshipQuestion = await GetQuestionsBySectionSettings(scholarshipData.Data.ScholarshipTest.ScholarshipTestId);

        //        string insertQuery = @"INSERT INTO tblStudentScholarship (ScholarshipID, StudentID, QuestionID, SubjectID, QuestionTypeID, ExamDate)
        //                               VALUES (@ScholarshipID, @StudentID, @QuestionID, @SubjectID, @QuestionTypeID, @ExamDate)";

        //        foreach (var data in scholarshipQuestion.Data)
        //        {
        //            await _connection.ExecuteAsync(insertQuery, new
        //            {
        //                ScholarshipID = scholarshipData.Data.ScholarshipTest.ScholarshipTestId,
        //                StudentID = request.RegistrationID,
        //                QuestionID = data.QuestionId,
        //                SubjectID = data.subjectID,
        //                QuestionTypeID = data.QuestionTypeId,
        //                ExamDate = DateTime.Now
        //            });
        //        }

        //        return new ServiceResponse<bool>(true, "Scholarship assigned successfully.", true, 200);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<bool>(false, ex.Message, false, 500);
        //    }
        //}
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

                    string questionQuery = @"SELECT SS.QuestionID, Q.QuestionDescription, Q.QuestionTypeId,qt.QuestionType
                                             FROM tblStudentScholarship SS
                                             JOIN tblQuestion Q ON SS.QuestionID = Q.QuestionID
                                             Join tblQBQuestionType qt on Q.QuestionTypeId = qt.QuestionTypeID
                                             WHERE SS.StudentID = @RegistrationId AND SS.ScholarshipID = @ScholarshipID AND SS.SubjectID = @SubjectID";

                    var questions = await _connection.QueryAsync(questionQuery, new { request.RegistrationId, request.ScholarshipID, SubjectID = subject.SubjectID });

                    foreach (var question in questions)
                    {
                        var questionDetail = new QuestionDetail
                        {
                            QuestionID = question.QuestionID,
                            Question = question.QuestionDescription,
                            QuestionTypeId = question.QuestionTypeId,
                            QuestionType = question.QuestionType,
                            Answers = new List<AnswerDetail>()
                        };

                        string answerQuery = @"
    SELECT 
        AM.AnswerID, 
        MC.Answer AS MCQAnswer, 
        MC.Answermultiplechoicecategoryid,
        SA.Answer AS SingleAnswer, 
        SA.Answersingleanswercategoryid
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
                                Answer = answer.MCQAnswer ?? answer.SingleAnswer, // Use MCQ answer or Single answer based on availability
                                Answermultiplechoicecategoryid = answer.Answermultiplechoicecategoryid,
                                Answersingleanswercategoryid = answer.Answersingleanswercategoryid
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
                string query = @"INSERT INTO tblQuestionNavigation 
                         (QuestionID, StartTime, EndTime, ScholarshipID, StudentID) 
                         VALUES (@QuestionID, @StartTime, @EndTime, @ScholarshipID, @StudentID);
                         SELECT CAST(SCOPE_IDENTITY() AS INT);";

                //foreach (var subject in request.Subjects)
                //{
                //    foreach (var question in subject.Questions)
                //    {
                //        var data = new List<AnswerSubmissionRequest>
                //        {

                //        };
                //        await SubmitAnswer(data);
                //        foreach (var log in question.TimeLogs)
                //        {
                //            // Execute query for each time log
                //            var navigationId = await _connection.ExecuteScalarAsync<int>(query, new
                //            {
                //                QuestionID = question.QuestionID,
                //                StartTime = log.StartTime,
                //                EndTime = log.EndTime,
                //                ScholarshipID = request.ScholarshipID,
                //                StudentID = request.StudentID
                //            });
                //        }
                //    }
                //}
                foreach (var subject in request.Subjects)
                {
                    foreach (var question in subject.Questions)
                    {
                        // Create a list with one AnswerSubmissionRequest for this question
                        var data = new List<AnswerSubmissionRequest>
        {
            new AnswerSubmissionRequest
            {
                ScholarshipID = request.ScholarshipID,
                RegistrationId = request.StudentID,
                QuestionID = question.QuestionID,
                SubjectID = subject.SubjectId,
                QuestionTypeID = question.QuestionTypeID,
                AnswerID = question.AnswerID  // Assuming question.AnswerID is a List<int>
            }
        };

                        // Submit the answer(s)
                        await SubmitAnswer(data);

                        foreach (var log in question.TimeLogs)
                        {
                            // Execute query for each time log
                            var navigationId = await _connection.ExecuteScalarAsync<int>(query, new
                            {
                                QuestionID = question.QuestionID,
                                StartTime = log.StartTime,
                                EndTime = log.EndTime,
                                ScholarshipID = request.ScholarshipID,
                                StudentID = request.StudentID
                            });
                        }
                    }
                }

                // Response message
                return new ServiceResponse<UpdateQuestionNavigationResponse>(
                    true,
                    "Navigation updated successfully.",
                    new UpdateQuestionNavigationResponse
                    {
                        ScholarshipID = request.ScholarshipID,
                        StudentID = request.StudentID,
                        Message = "All records inserted successfully."
                    },
                    200
                );
            }
            catch (Exception ex)
            {
                return new ServiceResponse<UpdateQuestionNavigationResponse>(
                    false,
                    ex.Message,
                    null,
                    500
                );
            }
        }
        //public async Task<ServiceResponse<UpdateQuestionNavigationResponse>> UpdateQuestionNavigationAsync(UpdateQuestionNavigationRequest request)
        //{
        //    try
        //    {
        //        string query = @"INSERT INTO tblQuestionNavigation (QuestionID, StartTime, EndTime, ScholarshipID, StudentID)
        //                         VALUES (@QuestionID, @StartTime, @EndTime, @ScholarshipID, @RegistrationId);
        //                         SELECT CAST(SCOPE_IDENTITY() AS INT);";

        //        var navigationId = await _connection.ExecuteScalarAsync<int>(query, new
        //        {
        //            request.QuestionID,
        //            StartTime = request.StartTime,
        //            EndTime = request.EndTime,
        //            request.ScholarshipID,
        //            request.RegistrationId
        //        });

        //        var response = new UpdateQuestionNavigationResponse
        //        {
        //            NavigationID = navigationId,
        //            ScholarshipID = request.ScholarshipID,
        //            StudentID = request.RegistrationId,
        //            QuestionID = request.QuestionID,
        //            StartTime = request.StartTime,
        //            EndTime = request.EndTime,
        //            Message = "Navigation updated successfully."
        //        };

        //        return new ServiceResponse<UpdateQuestionNavigationResponse>(true, "Navigation updated successfully.", response, 200);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<UpdateQuestionNavigationResponse>(false, ex.Message, null, 500);
        //    }
        //}
        public async Task<ServiceResponse<List<SubjectQuestionCountResponse>>> GetScholarshipSubjectQuestionCount(int scholarshipTestId)
        {
            var response = new ServiceResponse<List<SubjectQuestionCountResponse>>(true, string.Empty, [], 200);

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
                SELECT 
                    q.QuestionId, 
                    q.QuestionTypeId, 
                    s.MarksPerQuestion, 
                    s.NegativeMarks
                FROM tblQuestion q
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

                        // Check if any of the submitted answers are incorrect
                        bool hasIncorrectAnswer = answer.AnswerID.Any(submittedAnswer => !correctAnswers.Contains(submittedAnswer));

                        // Calculate studentCorrectCount based on the correctness of the answers
                        int studentCorrectCount = hasIncorrectAnswer ? -1 : answer.AnswerID.Intersect(correctAnswers).Count();

                        // Determine if negative marking applies
                        bool isNegative = hasIncorrectAnswer;

                        var questionTypeInSection = await _connection.QueryFirstOrDefaultAsync<dynamic>(@"
                    SELECT PartialMarkRuleId, QuestionTypeId
                    FROM tblSSQuestionSection 
                    WHERE SSTSectionId = @sectionId", new { sectionId });

                        int questionTypePartialMarkRule = await _connection.QueryFirstOrDefaultAsync<int>(@"
                    SELECT QuestionTypeId 
                    FROM tbl_PartialMarksRules
                    WHERE PartialMarksId = @PartialMarksId", new { PartialMarksId = questionTypeInSection.PartialMarkRuleId });

                        if (questionTypeInSection.QuestionTypeId == questionTypePartialMarkRule)
                        {
                            var (partialMarks, successRate) = await CalculatePartialMarksAsync(
                                actualCorrectCount,
                                studentCorrectCount,
                                isNegative);

                            marksAwarded = partialMarks;
                            answerStatus = successRate == 1 ? "Correct" : successRate > 0 ? "PartialCorrect" : "Incorrect";
                        }
                        else
                        {
                            if (actualCorrectCount == studentCorrectCount && answer.AnswerID.All(correctAnswers.Contains))
                            {
                                marksAwarded = questionData.MarksPerQuestion;
                                answerStatus = "Correct";
                            }
                            else
                            {
                                marksAwarded = -questionData.NegativeMarks;
                                answerStatus = "Incorrect";
                            }
                        }
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
        public async Task<ServiceResponse<string>> MarkScholarshipQuestionAsSave(ScholarshipQuestionSaveRequest request)
        {
            var response = new ServiceResponse<string>(true, string.Empty, string.Empty, 200);

            try
            {
                // Check if the question is already saved by this student
                var existingRecord = await _connection.QueryFirstOrDefaultAsync<int?>(
                    @"SELECT SQSId 
              FROM tblScholarshipQuestionSave 
              WHERE StudentId = @StudentId AND QuestionId = @QuestionId",
                    new { StudentId = request.StudentId, request.QuestionId });

                if (existingRecord != null)
                {
                    // If record exists, delete it
                    var deleteQuery = @"DELETE FROM tblScholarshipQuestionSave 
                                WHERE StudentId = @StudentId AND QuestionId = @QuestionId";
                    var rowsDeleted = await _connection.ExecuteAsync(deleteQuery, new
                    {
                        request.StudentId,
                        request.QuestionId
                    });

                    if (rowsDeleted > 0)
                    {
                        response.Data = "Scholarship question unsaved successfully.";
                        response.Success = true;
                    }
                    else
                    {
                        response.Data = "Failed to unsave the scholarship question.";
                        response.Success = false;
                    }
                }
                else
                {
                    // If no record exists, insert a new saved question record
                    var insertQuery = @"INSERT INTO tblScholarshipQuestionSave (StudentId, QuestionId, QuestionCode) 
                                VALUES (@StudentId, @QuestionId, @QuestionCode)";
                    var rowsInserted = await _connection.ExecuteAsync(insertQuery, new
                    {
                        request.StudentId,
                        request.QuestionId,
                        request.QuestionCode
                    });

                    if (rowsInserted > 0)
                    {
                        response.Data = "Scholarship question saved successfully.";
                        response.Success = true;
                    }
                    else
                    {
                        response.Data = "Failed to save the scholarship question.";
                        response.Success = false;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                response.Data = $"An error occurred: {ex.Message}";
                response.Success = false;
            }

            return response;
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetQuestionsByStudentScholarship(GetScholarshipQuestionRequest request)
        {
            ServiceResponse<List<QuestionResponseDTO>> response = new ServiceResponse<List<QuestionResponseDTO>>(true, string.Empty, new List<QuestionResponseDTO>(), 200);
            try
            {

                // Initialize the question list
                List<QuestionResponseDTO> questionsList = new List<QuestionResponseDTO>();
                // Step: Fetch questions from tblScholarshipQuestions mapped to the given ScholarshipTestId and RegistrationId (studentId)
                string mappingQuery = @"
SELECT STQuestionsId, ScholarshipTestId, SubjectId, QuestionId, QuestionCode, RegistrationId
FROM tblScholarshipQuestions
WHERE ScholarshipTestId = @ScholarshipTestId AND RegistrationId = @studentId";

                var selectedMappings = (await _connection.QueryAsync<dynamic>(mappingQuery, new
                {
                    ScholarshipTestId = request.scholarshipTestId,
                    studentId = request.studentId
                })).ToList();
                // Step 1: Check if entries already exist for the given RegistrationId
                string checkQuery = "SELECT COUNT(*) FROM tblScholarshipQuestions WHERE RegistrationId = @RegistrationId";

                int existingCount = await _connection.ExecuteScalarAsync<int>(checkQuery, new
                {
                    RegistrationId = request.studentId
                });

                if (existingCount > 0)
                {
                    // Step 2: Fetch existing questions
                    string fetchQuery = @"
    SELECT sq.STQuestionsId, sq.ScholarshipTestId, sq.SubjectId, sq.QuestionId, sq.QuestionCode, sq.RegistrationId, q.QuestionTypeId
    FROM tblScholarshipQuestions sq
    INNER JOIN tblQuestions q ON sq.QuestionId = q.QuestionId
    WHERE sq.RegistrationId = @RegistrationId";

                    questionsList = (await _connection.QueryAsync<QuestionResponseDTO>(fetchQuery, new
                    {
                        RegistrationId = request.studentId
                    })).ToList();

                    // Step 3: Apply filters on QuestionTypeId if provided
                    if (request.QuestionTypeId != null && request.QuestionTypeId.Any())
                    {
                        questionsList = questionsList
                            .Where(q => request.QuestionTypeId.Contains(q.QuestionTypeId))
                            .ToList();
                    }
                }
                else
                {
                    // Fetch the sections with their question type and question count settings

                    var selectedQuestions = new List<QuestionResponseDTO>();

                    foreach (var question in selectedQuestions)
                    {

                        var questionTypeData = await _connection.QueryFirstOrDefaultAsync<int>(@"select QuestionTypeId from tblQuestion where QuestionId = @QuestionId", new { QuestionId = question.QuestionId });
                        var isSingleAnswer = questionTypeData == 3 || questionTypeData == 4 ||
                                         questionTypeData == 8 || questionTypeData == 7 || questionTypeData == 9;


                        // Fetch the student's submitted answer
                        string studentAnswerQuery = @"
                SELECT AnswerID, AnswerStatus 
                FROM tblStudentScholarshipAnswerSubmission 
                WHERE StudentID = @StudentId AND QuestionID = @QuestionId AND ScholarshipID = @ScholarshipTestId";

                        var studentAnswer = await _connection.QuerySingleOrDefaultAsync<dynamic>(studentAnswerQuery,
                            new { StudentId = request.studentId, QuestionId = question.QuestionId, ScholarshipTestId = request.scholarshipTestId });

                        if (studentAnswer != null)
                        {
                            question.StudentAnswer = studentAnswer.AnswerID.ToString();
                            //question.IsCorrect = studentAnswer.AnswerStatus;
                        }

                        // Fetch correct answers and additional question details
                        if (isSingleAnswer)
                        {
                            string singleAnswerQuery = @"
                    SELECT 
                        a.Answersingleanswercategoryid, 
                        a.Answerid, 
                        a.Answer 
                    FROM tblAnswersingleanswercategory a
                    INNER JOIN tblAnswerMaster am ON am.Answerid = a.Answerid
                    WHERE am.Questionid = @QuestionId";

                            question.Answersingleanswercategories = await _connection.QuerySingleOrDefaultAsync<Answersingleanswercategory>(singleAnswerQuery, new { QuestionId = question.QuestionId });

                            // Determine correctness if not already fetched
                            if (question.IsCorrect == null)
                            {
                                question.IsCorrect = question.StudentAnswer == question.Answersingleanswercategories?.Answer;
                            }
                        }
                        else
                        {
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

                            // Determine correctness if not already fetched
                            if (question.IsCorrect == null)
                            {
                                var correctAnswers = question.AnswerMultipleChoiceCategories
                                    .Where(a => a.Iscorrect)
                                    .Select(a => a.Answermultiplechoicecategoryid)
                                    .ToList();
                                if (studentAnswer != null)
                                {
                                    var studentAnswers = question.StudentAnswer?
                                .Split(',')
                                .Select(a => int.TryParse(a.Trim(), out var id) ? id : (int?)null)
                                .Where(id => id.HasValue)
                                .Select(id => id.Value)
                                .ToList();

                                    question.IsCorrect = studentAnswers != null
                                        && correctAnswers.All(studentAnswers.Contains)
                                        && studentAnswers.All(correctAnswers.Contains);
                                }
                            }
                        }
                    }

                    // Add questions to the list
                    questionsList.AddRange(selectedQuestions);

                }

                response.Data = questionsList;
                response.Success = true;
                response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }
        public async Task<ServiceResponse<StudentDiscountResponse>> GetStudentDiscountAsync(int studentId, int scholarshipTestId)
        {
            try
            {
                // Step 1: Calculate the total marks gained by the student for the given scholarship test.
                string marksQuery = @"
            SELECT ISNULL(SUM(Marks), 0)
            FROM tblStudentScholarshipAnswerSubmission
            WHERE ScholarshipID = @ScholarshipTestId AND StudentID = @StudentId";
                decimal totalMarksGained = await _connection.ExecuteScalarAsync<decimal>(marksQuery, new { ScholarshipTestId = scholarshipTestId, StudentId = studentId });

                // Step 2: Retrieve the total marks possible from the scholarship test.
                string totalMarksQuery = @"
            SELECT ISNULL(TotalMarks, 0)
            FROM tblScholarshipTest
            WHERE ScholarshipTestId = @ScholarshipTestId";
                decimal totalMarksPossible = await _connection.ExecuteScalarAsync<decimal>(totalMarksQuery, new { ScholarshipTestId = scholarshipTestId });

                if (totalMarksPossible == 0)
                {
                    return new ServiceResponse<StudentDiscountResponse>(false, "Total marks for the scholarship test is not set.", null, 400);
                }

                // Step 3: Compute the percentage of marks obtained.
                decimal percentage = (totalMarksGained / totalMarksPossible) * 100;

                // Step 4: Fetch the discount from tblSSTDiscountScheme based on the student's percentage.
                // Assumes that Discount Scheme is set per ScholarshipTestId and defines a range.
                string discountQuery = @"
            SELECT TOP 1 Discount 
            FROM tblSSTDiscountScheme
            WHERE ScholarshipTestId = @ScholarshipTestId
              AND @Percentage BETWEEN PercentageStartRange AND PercentageEndRange
            ORDER BY PercentageStartRange DESC";
                decimal discount = await _connection.ExecuteScalarAsync<decimal>(discountQuery, new { ScholarshipTestId = scholarshipTestId, Percentage = percentage });

                // Step 5: Build the response object.
                var responseData = new StudentDiscountResponse
                {
                    TotalMarksGained = totalMarksGained,
                    TotalMarksPossible = totalMarksPossible,
                    Percentage = percentage,
                    Discount = discount
                };

                return new ServiceResponse<StudentDiscountResponse>(true, "Discount retrieved successfully.", responseData, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<StudentDiscountResponse>(false, ex.Message, null, 500);
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