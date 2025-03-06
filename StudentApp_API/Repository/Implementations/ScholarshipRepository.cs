using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using System.Data;
using System.Text;

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
            SELECT TOP 1 st.[ScholarshipTestId], st.[APID], st.[ExamTypeId], st.[PatternName],st.TotalMarks as TotalMarks,
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
                string checkQuery = "SELECT COUNT(*) FROM [tblStudentScholarship] WHERE StudentID = @RegistrationId";

                int existingCount = await _connection.ExecuteScalarAsync<int>(checkQuery, new
                {
                    RegistrationId = request.studentId
                });

                if (existingCount > 0)
                {
                    string fetchQuery = @"
    WITH QuestionStatusCTE AS (
        SELECT 
            ss.SSID,
            ss.ScholarshipID,
            ss.StudentID,
            ss.QuestionID,
            ss.SubjectID,
            ss.QuestionTypeID,
            ss.QuestionStatusId AS Status
        FROM tblStudentScholarship ss
        LEFT JOIN QuestionStatuses qs ON ss.QuestionStatusId = qs.StatusID
        WHERE ss.StudentID = @StudentId AND ss.ScholarshipID =  @ScholarshipID
    )
    SELECT 
        qsCTE.SSID,
        qsCTE.ScholarshipID,
        qsCTE.StudentID,
        q.QuestionId, q.QuestionCode, q.QuestionDescription, q.QuestionFormula, q.IsLive, q.QuestionTypeId,
        q.Status, q.CreatedBy, q.CreatedOn, q.ModifiedBy, q.ModifiedOn, q.SubjectID, s.SubjectName, 
        q.ExamTypeId, e.ExamTypeName, q.EmployeeId, emp.EmpFirstName as EmployeeName,
        q.IndexTypeId, it.IndexType as IndexTypeName, q.ContentIndexId,
        CASE 
            WHEN q.IndexTypeId = 1 THEN ci.ContentName_Chapter
            WHEN q.IndexTypeId = 2 THEN ct.ContentName_Topic
            WHEN q.IndexTypeId = 3 THEN cst.ContentName_SubTopic
        END AS ContentIndexName,
        qsCTE.Status
    FROM QuestionStatusCTE qsCTE
    INNER JOIN tblQuestion q ON qsCTE.QuestionID = q.QuestionId
    LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
    LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
    LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
    LEFT JOIN tblSubject s ON q.SubjectID = s.SubjectId
    LEFT JOIN tblEmployee emp ON q.EmployeeId = emp.Employeeid
    LEFT JOIN tblExamType e ON q.ExamTypeId = e.ExamTypeID
    LEFT JOIN tblQBIndexType it ON q.IndexTypeId = it.IndexId
    WHERE (@QuestionStatus IS NULL OR qsCTE.Status IN @QuestionStatus);";

                    var parameters = new
                    {
                        StudentId = request.studentId,
                        ScholarshipID = request.scholarshipTestId,
                        // QuestionTypeId = (request.QuestionTypeId != null && request.QuestionTypeId.Any()) ? request.QuestionTypeId : null,
                        QuestionStatus = (request.QuestionStatus != null && request.QuestionStatus.Any()) ? request.QuestionStatus : null,

                    };
                    questionsList = (await _connection.QueryAsync<QuestionResponseDTO>(fetchQuery, parameters)).ToList();
                    // Apply filter on QuestionTypeId if provided
                    if (request.QuestionTypeId != null && request.QuestionTypeId.Any())
                    {
                        questionsList = questionsList
                            .Where(q => request.QuestionTypeId.Contains(q.QuestionTypeId))
                            .ToList();
                    }

                    foreach (var question in questionsList)
                    {
                        var isSingleAnswer = question.QuestionTypeId == 9 || question.QuestionTypeId == 4 ;
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
                        var isSingleAnswer = section.QuestionTypeId == 9 || section.QuestionTypeId == 4 ;


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
        public async Task<ServiceResponse<ScholarshipQuestionsResponse>> AssignScholarshipAsync(AssignScholarshipRequest request)
        {
            try
            {
                // Step 1: Check if the student has already opted for a scholarship
                var existingScholarship = await _connection.QueryFirstOrDefaultAsync<int>(
                    @"SELECT COUNT(1) 
              FROM tblStudentScholarship
              WHERE StudentID = @RegistrationID",
                    new { RegistrationID = request.RegistrationID });

                if (existingScholarship > 0)
                {
                    return new ServiceResponse<ScholarshipQuestionsResponse>(false, "This student has already opted for a scholarship and cannot opt for another one.", null, 400);
                }

                // Step 2: Get scholarship data based on student's registration ID
                var scholarshipData = await GetScholarshipTestByRegistrationId(request.RegistrationID);

                if (scholarshipData?.Data?.ScholarshipTest == null)
                {
                    return new ServiceResponse<ScholarshipQuestionsResponse>(false, "No scholarship test found for this student.", null, 404);
                }

                // Step 3: Fetch mapped subjects for this scholarship test
                var scholarshipSubjects = await _connection.QueryAsync<ScholarshipSubjects>(
                    @"SELECT sst.SubjectId AS SubjectId, sub.SubjectName
              FROM tblScholarshipSubject sst
              JOIN tblSubject sub ON sst.SubjectId = sub.SubjectID
              WHERE sst.ScholarshipTestId = @ScholarshipTestId",
                    new { ScholarshipTestId = scholarshipData.Data.ScholarshipTest.ScholarshipTestId });

                var subjectsList = scholarshipSubjects.ToList();

                // Step 4: Fetch sections for each subject
                foreach (var subject in subjectsList)
                {
                    var sections = await _connection.QueryAsync<ScholarshipSections>(
                        @"SELECT SSTSectionId AS SectionId, SectionName, QuestionTypeId
                  FROM tblSSQuestionSection
                  WHERE ScholarshipTestId = @ScholarshipTestId AND SubjectId = @SubjectId",
                        new
                        {
                            ScholarshipTestId = scholarshipData.Data.ScholarshipTest.ScholarshipTestId,
                            SubjectId = subject.SubjectId
                        });

                    subject.ScholarshipSections = sections.ToList();
                }
                var requestbody = new GetScholarshipQuestionRequest
                {
                    scholarshipTestId = scholarshipData.Data.ScholarshipTest.ScholarshipTestId,
                    studentId = request.RegistrationID,
                    QuestionTypeId = null
                };
                // Step 3: Get the questions for the scholarship test
                var scholarshipQuestion = await GetQuestionsBySectionSettings(requestbody);
                // Step 5: Fetch questions and map them to respective sections
                var questions = scholarshipQuestion.Data;
                string insertQuery = @"INSERT INTO tblStudentScholarship (ScholarshipID, StudentID, QuestionID, SubjectID, QuestionTypeID, ExamDate, QuestionStatusId, SectionId)
                               VALUES (@ScholarshipID, @StudentID, @QuestionID, @SubjectID, @QuestionTypeID, @ExamDate, @QuestionStatusId, @SectionId)";

                foreach (var subject in subjectsList)
                {
                    foreach (var section in subject.ScholarshipSections)
                    {
                        // Filter questions for this section
                        var sectionQuestions = questions
                            .Where(q => q.QuestionTypeId == section.QuestionTypeId) // Matching QuestionTypeId
                            .ToList();

                        foreach (var question in sectionQuestions)
                        {
                            await _connection.ExecuteAsync(insertQuery, new
                            {
                                ScholarshipID = scholarshipData.Data.ScholarshipTest.ScholarshipTestId,
                                StudentID = request.RegistrationID,
                                QuestionID = question.QuestionId,
                                SubjectID = question.subjectID,
                                QuestionTypeID = question.QuestionTypeId,
                                ExamDate = DateTime.Now,
                                QuestionStatusId = 4,
                                SectionId = section.SectionId // ✅ Correctly mapping SectionId
                            });
                        }
                    }
                }

                //foreach (var subject in subjectsList)
                //{
                //    foreach (var section in subject.ScholarshipSections)
                //    {
                //        section.QuestionResponseDTOs = questions
                //            .Where(q => q.QuestionTypeId == section.QuestionTypeId) // Matching QuestionTypeId
                //            .ToList();
                //    }
                //}

                //// Step 6: Insert assigned scholarship questions into tblStudentScholarship
                //string insertQuery = @"INSERT INTO tblStudentScholarship (ScholarshipID, StudentID, QuestionID, SubjectID, QuestionTypeID, ExamDate, QuestionStatusId, SectionId)
                //               VALUES (@ScholarshipID, @StudentID, @QuestionID, @SubjectID, @QuestionTypeID, @ExamDate, @QuestionStatusId, @SectionId)";

                //foreach (var question in questions)
                //{
                //    await _connection.ExecuteAsync(insertQuery, new
                //    {
                //        ScholarshipID = scholarshipData.Data.ScholarshipTest.ScholarshipTestId,
                //        StudentID = request.RegistrationID,
                //        QuestionID = question.QuestionId,
                //        SubjectID = question.subjectID,
                //        QuestionTypeID = question.QuestionTypeId,
                //        ExamDate = DateTime.Now,
                //        QuestionStatusId = 4,

                //    });
                //}

                // Step 7: Return response in desired format
                var response = new ScholarshipQuestionsResponse
                {
                    ScholarshipId = scholarshipData.Data.ScholarshipTest.ScholarshipTestId,
                    ScholarshipSubjects = subjectsList
                };

                return new ServiceResponse<ScholarshipQuestionsResponse>(true, "Scholarship assigned successfully.", response, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ScholarshipQuestionsResponse>(false, ex.Message, null, 500);
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

                foreach (var subject in request.Subjects)
                {
                    foreach (var question in subject.Questions)
                    {
                        // Check if MultiOrSingleAnswerId is not null and contains at least one valid answer
                        if (question.MultiOrSingleAnswerId != null && question.MultiOrSingleAnswerId.Any(id => id != 0))
                        {
                            var data = new List<AnswerSubmissionRequest>
        {
            new AnswerSubmissionRequest
            {
                ScholarshipID = request.ScholarshipID,
                RegistrationId = request.StudentID,
                QuestionID = question.QuestionID,
                SubjectID = subject.SubjectId,
                QuestionTypeID = question.QuestionTypeID,
                MultiOrSingleAnswerId = question.MultiOrSingleAnswerId // Assuming question.AnswerID is a List<int>
            }
        };
                            // Submit the answer(s)
                            await SubmitAnswer(data);
                        }

                        // Insert time logs regardless of whether an answer is submitted or not
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
                        var updateQuestionStatusQuery = @"
    UPDATE [tblStudentScholarship] 
    SET QuestionStatusId = @QuestionStatusId 
    WHERE QuestionID = @QuestionId 
      AND ScholarshipID = @ScholarshipID 
      AND StudentID = @RegistrationId 
      AND SubjectID = @SubjectID;";

                        await _connection.ExecuteAsync(updateQuestionStatusQuery, new
                        {
                            QuestionId = question.QuestionID,
                            ScholarshipID = request.ScholarshipID,
                            RegistrationId = request.StudentID,
                            SubjectID = subject.SubjectId,
                            QuestionStatusId = question.QuestionstatusId
                        });

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
                // Prepare the response list
                var responses = new List<MarksAcquiredAfterAnswerSubmission>();

                foreach (var answer in request)
                {
                    // Fetch section ID based on the subject ID
                    var sectionQuery = @"
            SELECT SSTSectionId
            FROM tblSSQuestionSection
            WHERE SubjectId = @SubjectID and ScholarshipTestId = @ScholarshipTestId and QuestionTypeId = @QuestionTypeId";

                    var sectionId = await _connection.QueryFirstOrDefaultAsync<int>(sectionQuery, new { SubjectID = request.FirstOrDefault()?.SubjectID, ScholarshipTestId = request.FirstOrDefault()?.ScholarshipID, QuestionTypeId = answer.QuestionTypeID });

                    if (sectionId == 0)
                    {
                        return new ServiceResponse<List<MarksAcquiredAfterAnswerSubmission>>(false, "Section not found for the given subject.", null, 404);
                    }
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

                        if (correctAnswerId == answer.MultiOrSingleAnswerId.FirstOrDefault())
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
                        bool hasIncorrectAnswer = answer.MultiOrSingleAnswerId.Any(submittedAnswer => !correctAnswers.Contains(submittedAnswer));

                        // Calculate studentCorrectCount based on the correctness of the answers
                        int studentCorrectCount = hasIncorrectAnswer ? -1 : answer.MultiOrSingleAnswerId.Intersect(correctAnswers).Count();

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
                            if (actualCorrectCount == studentCorrectCount && answer.MultiOrSingleAnswerId.All(correctAnswers.Contains))
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
                        AnswerID = string.Join(",", answer.MultiOrSingleAnswerId),
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
                    var insertQuery = @"INSERT INTO tblScholarshipQuestionSave (StudentId, QuestionId, QuestionCode, ScholarshipId) 
                                VALUES (@StudentId, @QuestionId, @QuestionCode, @ScholarshipId)";
                    var rowsInserted = await _connection.ExecuteAsync(insertQuery, new
                    {
                        request.StudentId,
                        request.QuestionId,
                        request.QuestionCode,
                        request.ScholarshipId
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
        public async Task<ServiceResponse<ScholarshipViewKeyQuestionsResponse>> ViewKeyByStudentScholarship(GetScholarshipQuestionRequest request)
        {
            try
            {
                List<QuestionResponseDTO> questionsList = new List<QuestionResponseDTO>();

                // Dynamic query building
                var queryBuilder = new StringBuilder(@"
            SELECT ssch.QuestionID, ssch.ScholarshipID, ssch.StudentID, ssch.SubjectID, ssch.QuestionTypeID, 
                   ssch.QuestionStatusId, q.QuestionDescription
            FROM tblStudentScholarship ssch
            INNER JOIN tblQuestion q ON ssch.QuestionID = q.QuestionId
            WHERE ssch.StudentID = @StudentId AND ssch.ScholarshipID = @ScholarshipTestId");

                var queryParams = new DynamicParameters();
                queryParams.Add("StudentId", request.studentId);
                queryParams.Add("ScholarshipTestId", request.scholarshipTestId);

                // Apply QuestionTypeId filter if provided
                if (request.QuestionTypeId != null && request.QuestionTypeId.Any())
                {
                    queryBuilder.Append(" AND ssch.QuestionTypeID IN @QuestionTypeIds");
                    queryParams.Add("QuestionTypeIds", request.QuestionTypeId);
                }

                // Apply QuestionStatus filter if provided
                if (request.QuestionStatus != null && request.QuestionStatus.Any())
                {
                    queryBuilder.Append(" AND ssch.QuestionStatusId IN @QuestionStatusIds");
                    queryParams.Add("QuestionStatusIds", request.QuestionStatus);
                }

                // Execute query
                var selectedQuestions = (await _connection.QueryAsync<QuestionResponseDTO>(queryBuilder.ToString(), queryParams)).ToList();

                foreach (var question in selectedQuestions)
                {
                    // Fetch student's submitted answer
                    string studentAnswerQuery = @"
                SELECT AnswerID, AnswerStatus 
                FROM tblStudentScholarshipAnswerSubmission 
                WHERE StudentID = @StudentId AND QuestionID = @QuestionId AND ScholarshipID = @ScholarshipTestId";

                    var studentAnswer = await _connection.QuerySingleOrDefaultAsync<dynamic>(studentAnswerQuery, new
                    {
                        StudentId = request.studentId,
                        QuestionId = question.QuestionId,
                        ScholarshipTestId = request.scholarshipTestId
                    });

                    if (studentAnswer != null)
                    {
                        question.StudentAnswer = studentAnswer.AnswerID.ToString(); // Store student's answer
                        question.IsCorrect = studentAnswer.AnswerStatus == "Correct"; // Mark correct if status is 'Correct'
                    }

                    // Determine correct answers and additional question details
                    if (question.QuestionTypeId == 3 || question.QuestionTypeId == 4 || question.QuestionTypeId == 7 || question.QuestionTypeId == 8 || question.QuestionTypeId == 9) // Single answer type
                    {
                        string correctAnswerQuery = @"
                    SELECT a.Answerid, a.Answer 
                    FROM tblAnswersingleanswercategory a
                    INNER JOIN tblAnswerMaster am ON am.Answerid = a.Answerid
                    WHERE am.Questionid = @QuestionId";

                        question.Answersingleanswercategories = await _connection.QuerySingleOrDefaultAsync<Answersingleanswercategory>(correctAnswerQuery, new { QuestionId = question.QuestionId });

                        // Check correctness if not already determined
                        if (question.IsCorrect == null)
                        {
                            question.IsCorrect = question.StudentAnswer == question.Answersingleanswercategories?.Answer;
                        }
                    }
                    else // Multiple-choice type
                    {
                        string multipleAnswerQuery = @"
                    SELECT a.Answermultiplechoicecategoryid, a.Answerid, a.Answer, a.Iscorrect 
                    FROM tblAnswerMultipleChoiceCategory a
                    INNER JOIN tblAnswerMaster am ON am.Answerid = a.Answerid
                    WHERE am.Questionid = @QuestionId";

                        question.AnswerMultipleChoiceCategories = (await _connection.QueryAsync<AnswerMultipleChoiceCategory>(multipleAnswerQuery, new { QuestionId = question.QuestionId })).ToList();

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

                    questionsList.Add(question);
                }
                List<QuestionViewKeyResponseDTO> mappedList = questionsList.Select(q => new QuestionViewKeyResponseDTO
                {
                    QuestionId = q.QuestionId,
                    QuestionDescription = q.QuestionDescription,
                    QuestionTypeId = q.QuestionTypeId,
                    QuestionTypeName = q.QuestionTypeName,
                    IndexTypeId = q.IndexTypeId,
                    IndexTypeName = q.IndexTypeName,
                    ContentIndexId = q.ContentIndexId,
                    ContentIndexName = q.ContentIndexName,
                    QuestionCode = q.QuestionCode,
                    Explanation = q.Explanation,
                    ExtraInformation = q.ExtraInformation,

                    // Convert StudentAnswer from string to List<int>
                    StudentAnswer = !string.IsNullOrEmpty(q.StudentAnswer)
        ? q.StudentAnswer.Split(',').Select(s => int.TryParse(s, out int val) ? val : (int?)null).Where(v => v.HasValue).Select(v => v.Value).ToList()
        : new List<int>(),

                    IsCorrect = q.IsCorrect,
                    AnswerMultipleChoiceCategories = q.AnswerMultipleChoiceCategories,
                    Answersingleanswercategories = q.Answersingleanswercategories

                }).ToList();

                var scholarshipSubjects = await _connection.QueryAsync<ScholarshipViewKeySubjects>(
                  @"SELECT sst.SubjectId AS SubjectId, sub.SubjectName
              FROM tblScholarshipSubject sst
              JOIN tblSubject sub ON sst.SubjectId = sub.SubjectID
              WHERE sst.ScholarshipTestId = @ScholarshipTestId",
                  new { ScholarshipTestId = request.scholarshipTestId });

                var subjectsList = scholarshipSubjects.ToList();

                // Step 4: Fetch sections for each subject
                foreach (var subject in subjectsList)
                {
                    var sections = await _connection.QueryAsync<ScholarshipViewKeySections>(
                        @"SELECT SSTSectionId AS SectionId, SectionName, QuestionTypeId
                  FROM tblSSQuestionSection
                  WHERE ScholarshipTestId = @ScholarshipTestId AND SubjectId = @SubjectId",
                        new
                        {
                            ScholarshipTestId = request.scholarshipTestId,
                            SubjectId = subject.SubjectId
                        });

                    subject.ScholarshipSections = sections.ToList();
                }
                foreach (var subject in subjectsList)
                {
                    foreach (var section in subject.ScholarshipSections)
                    {
                        section.QuestionResponseDTOs = mappedList
                            .Where(q => q.QuestionTypeId == section.QuestionTypeId) // Matching QuestionTypeId
                            .ToList();
                    }
                }
                var response = new ScholarshipViewKeyQuestionsResponse
                {
                    ScholarshipId = request.scholarshipTestId,
                    ScholarshipSubjects = subjectsList
                };

                return new ServiceResponse<ScholarshipViewKeyQuestionsResponse>(true, "Scholarship assigned successfully.", response, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ScholarshipViewKeyQuestionsResponse>(false, ex.Message, null, 500);
            }
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
        //public async Task<ServiceResponse<ScholarshipAnalytics>> GetScholarshipAnalyticsAsync(int studentId, int scholarshipId)
        //{
        //    try
        //    {
        //        var sql = @"
        //-- 1. Retrieve test details
        //SELECT TotalNumberOfQuestions, Duration, TotalMarks 
        //FROM tblScholarshipTest
        //WHERE ScholarshipTestId = @ScholarshipId;

        //-- 2. Sum of marks obtained by the student
        //SELECT ISNULL(SUM(Marks), 0) 
        //FROM tblStudentScholarshipAnswerSubmission 
        //WHERE StudentID = @StudentId AND ScholarshipID = @ScholarshipId;

        //-- 3. Aggregate counts of answer statuses
        //SELECT 
        //    SUM(CASE WHEN AnswerStatus = 'Correct' THEN 1 ELSE 0 END) AS CorrectCount,
        //    SUM(CASE WHEN AnswerStatus = 'Incorrect' THEN 1 ELSE 0 END) AS IncorrectCount,
        //    SUM(CASE WHEN AnswerStatus = 'PartialCorrect' THEN 1 ELSE 0 END) AS PartiallyCorrectCount,
        //    COUNT(*) AS AttemptedCount
        //FROM tblStudentScholarshipAnswerSubmission 
        //WHERE StudentID = @StudentId AND ScholarshipID = @ScholarshipId;

        //-- 4. Count total assigned questions for the student
        //SELECT COUNT(*) FROM tblStudentScholarship
        //WHERE StudentID = @StudentId AND ScholarshipID = @ScholarshipId;";

        //        _connection.Open();
        //        using (var multi = await _connection.QueryMultipleAsync(sql, new { StudentId = studentId, ScholarshipId = scholarshipId }))
        //        {
        //            // 1. Read test details
        //            var testDetails = await multi.ReadFirstOrDefaultAsync<dynamic>();
        //            if (testDetails == null)
        //            {
        //                return new ServiceResponse<ScholarshipAnalytics>(false, "Test details not found for the given ScholarshipTestId.", null, 404);
        //            }

        //            int totalQuestions = testDetails.TotalNumberOfQuestions;
        //            string durationString = testDetails.Duration; // Example: "120 minutes"
        //            int duration = int.Parse(durationString.Split(' ')[0]); // Extracts "120" and converts to int

        //            decimal testTotalMarks = testDetails.TotalMarks;

        //            // 2. Read student's obtained marks
        //            int studentMarks = await multi.ReadFirstAsync<int>();

        //            // 3. Read answer status counts
        //            var counts = await multi.ReadFirstAsync<dynamic>();
        //            int correctCount = counts?.CorrectCount ?? 0;
        //            int incorrectCount = counts?.IncorrectCount ?? 0;
        //            int partiallyCorrectCount = counts?.PartiallyCorrectCount ?? 0;
        //            int attemptedCount = counts?.AttemptedCount ?? 0;

        //            // 4. Read total assigned questions for the student
        //            int assignedQuestions = await multi.ReadFirstAsync<int>();

        //            // Calculate unattempted questions
        //            int unattemptedCount = assignedQuestions - attemptedCount;
        //            if (unattemptedCount < 0) unattemptedCount = 0;

        //            // Calculate percentages safely
        //            decimal correctPercentage = assignedQuestions > 0 ? (correctCount * 100.0M / assignedQuestions) : 0;
        //            decimal incorrectPercentage = assignedQuestions > 0 ? (incorrectCount * 100.0M / assignedQuestions) : 0;
        //            decimal partiallyCorrectPercentage = assignedQuestions > 0 ? (partiallyCorrectCount * 100.0M / assignedQuestions) : 0;
        //            decimal unattemptedPercentage = assignedQuestions > 0 ? (unattemptedCount * 100.0M / assignedQuestions) : 0;

        //            // Construct the analytics result
        //            var response = new ScholarshipAnalytics
        //            {
        //                TotalQuestions = assignedQuestions,
        //                Duration = duration,
        //                TestTotalMarks = testTotalMarks,
        //                StudentMarks = studentMarks,
        //                CorrectCount = correctCount,
        //                CorrectPercentage = correctPercentage,
        //                IncorrectCount = incorrectCount,
        //                IncorrectPercentage = incorrectPercentage,
        //                PartiallyCorrectCount = partiallyCorrectCount,
        //                PartiallyCorrectPercentage = partiallyCorrectPercentage,
        //                UnattemptedCount = unattemptedCount,
        //                UnattemptedPercentage = unattemptedPercentage
        //            };
        //            _connection.Close();

        //            return new ServiceResponse<ScholarshipAnalytics>(true, "Operation Successful", response, 200);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<ScholarshipAnalytics>(false, ex.Message, null, 500);
        //    }
        //}
        public async Task<ServiceResponse<ScholarshipAnalytics>> GetScholarshipAnalyticsAsync(int studentId, int scholarshipId)
        {
            try
            {
                var sql = @"
        -- 1. Retrieve test details
        SELECT TotalMarks, Duration 
        FROM tblScholarshipTest 
        WHERE ScholarshipTestId = @ScholarshipId;

        -- 2. Retrieve all sections for the test
        SELECT SSTSectionId, TotalNumberOfQuestions, NoOfQuestionsPerChoice 
        FROM tblSSQuestionSection 
        WHERE ScholarshipTestId = @ScholarshipId;

        -- 3. Retrieve student's answers with SectionId mapping
        SELECT ss.SectionId, ssa.AnswerStatus, ssa.Marks 
        FROM tblStudentScholarshipAnswerSubmission ssa
        INNER JOIN tblStudentScholarship ss ON ssa.QuestionID = ss.QuestionID  
        WHERE ss.StudentID = @StudentId AND ss.ScholarshipID = @ScholarshipId AND ssa.StudentID = @StudentId and ssa.ScholarshipID = @ScholarshipId
        ORDER BY ss.SectionId, ssa.AnswerStatus DESC;";  // Prioritizing Correct over Incorrect

                _connection.Open();
                using (var multi = await _connection.QueryMultipleAsync(sql, new { StudentId = studentId, ScholarshipId = scholarshipId }))
                {
                    // Read test details
                    var testDetails = await multi.ReadFirstOrDefaultAsync<dynamic>();
                    if (testDetails == null)
                    {
                        return new ServiceResponse<ScholarshipAnalytics>(false, "Test details not found.", null, 404);
                    }

                    decimal testTotalMarks = testDetails.TotalMarks;
                    int duration;
                    int.TryParse(testDetails.Duration.Split(' ')[0], out duration); // Extract numeric value safely

                    // Read all sections
                    var sections = (await multi.ReadAsync<dynamic>()).ToList();

                    // Read student's answers (mapped with SectionId)
                    var allAnswers = (await multi.ReadAsync<dynamic>()).ToList();

                    // Initialize aggregated counts
                    int totalQuestions = 0;
                    int totalAttempted = 0, totalCorrect = 0, totalIncorrect = 0, totalPartial = 0;
                    decimal totalMarksObtained = 0;

                    // Process each section
                    foreach (var section in sections)
                    {
                        int sectionId = section.SSTSectionId;
                        int sectionTotalQuestions = section.TotalNumberOfQuestions;
                        int noOfQuestionsPerChoice = section.NoOfQuestionsPerChoice;
                        int maxQuestionsToConsider = sectionTotalQuestions - noOfQuestionsPerChoice;

                        // Filter answers for this section
                        var sectionAnswers = allAnswers.Where(a => a.SectionId == sectionId).ToList();

                        // Select `maxQuestionsToConsider` answers prioritizing correct ones
                        var selectedAnswers = sectionAnswers
                            .OrderByDescending(a => a.AnswerStatus == "Correct") // Prioritize Correct
                            .ThenBy(a => a.AnswerStatus == "PartialCorrect")      // Then Partial Correct
                            .Take(maxQuestionsToConsider)
                            .ToList();

                        // Section-wise calculations
                        int sectionAttempted = selectedAnswers.Count;
                        int sectionCorrect = selectedAnswers.Count(a => a.AnswerStatus == "Correct");
                        int sectionIncorrect = selectedAnswers.Count(a => a.AnswerStatus == "Incorrect");
                        int sectionPartial = selectedAnswers.Count(a => a.AnswerStatus == "PartialCorrect");
                        int sectionUnattempted = sectionTotalQuestions - sectionAttempted;
                        decimal sectionMarksObtained = selectedAnswers.Sum(a => (decimal?)a.Marks ?? 0); // Handle nulls

                        // Aggregate section data
                        totalQuestions += sectionTotalQuestions;
                        totalAttempted += sectionAttempted;
                        totalCorrect += sectionCorrect;
                        totalIncorrect += sectionIncorrect;
                        totalPartial += sectionPartial;
                        totalMarksObtained += sectionMarksObtained;
                    }

                    // Compute overall analytics
                    int totalUnattempted = totalQuestions - totalAttempted;
                    decimal correctPercentage = totalQuestions > 0 ? (totalCorrect * 100.0M / totalQuestions) : 0;
                    decimal incorrectPercentage = totalQuestions > 0 ? (totalIncorrect * 100.0M / totalQuestions) : 0;
                    decimal partialPercentage = totalQuestions > 0 ? (totalPartial * 100.0M / totalQuestions) : 0;
                    decimal unattemptedPercentage = totalQuestions > 0 ? (totalUnattempted * 100.0M / totalQuestions) : 0;

                    // Prepare response
                    var response = new ScholarshipAnalytics
                    {
                        TotalQuestions = totalQuestions,
                        Duration = duration,
                        TestTotalMarks = testTotalMarks,
                        StudentMarks = totalMarksObtained,
                        CorrectCount = totalCorrect,
                        CorrectPercentage = correctPercentage,
                        IncorrectCount = totalIncorrect,
                        IncorrectPercentage = incorrectPercentage,
                        PartiallyCorrectCount = totalPartial,
                        PartiallyCorrectPercentage = partialPercentage,
                        UnattemptedCount = totalUnattempted,
                        UnattemptedPercentage = unattemptedPercentage
                    };

                    _connection.Close();
                    return new ServiceResponse<ScholarshipAnalytics>(true, "Operation Successful", response, 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ScholarshipAnalytics>(false, ex.Message, null, 500);
            }
        }

        //public async Task<ServiceResponse<ScholarshipAnalytics>> GetSubjectWiseScholarshipAnalyticsAsync(int studentId, int scholarshipId, int subjectId)
        //{
        //    try
        //    {
        //        var sql = @"-- 1. Retrieve test details (Total Questions, Total Marks, Duration)
        //SELECT 
        //    COUNT(*) AS TotalQuestions,
        //    ISNULL(SUM(ssqs.MarksPerQuestion), 0) AS SubjectTotalMarks,
        //    (SELECT Duration FROM tblScholarshipTest WHERE ScholarshipTestId = @ScholarshipId) AS Duration
        //FROM tblStudentScholarship sss
        //INNER JOIN tblSSQuestionSection ssqs 
        //  ON sss.SubjectID = ssqs.SubjectId and sss.ScholarshipID = ssqs.ScholarshipTestId
        //    AND sss.QuestionTypeID = ssqs.QuestionTypeId 
        //WHERE sss.ScholarshipID = @ScholarshipId
        //AND sss.SubjectID = @SubjectId
        //AND sss.StudentID = @StudentId;

        //-- 2. Sum of marks obtained by the student in the specified subject
        //SELECT ISNULL(SUM(ssas.Marks), 0) AS StudentMarks
        //FROM tblStudentScholarshipAnswerSubmission ssas
        //INNER JOIN tblStudentScholarship sss ON ssas.QuestionID = sss.QuestionID
        //WHERE ssas.StudentID = @StudentId
        //AND ssas.ScholarshipID = @ScholarshipId
        //AND sss.SubjectId = @SubjectId;

        //-- 3. Count correct, incorrect, and partially correct answers
        //SELECT 
        //    SUM(CASE WHEN ssas.AnswerStatus = 'Correct' THEN 1 ELSE 0 END) AS CorrectCount,
        //    SUM(CASE WHEN ssas.AnswerStatus = 'Incorrect' THEN 1 ELSE 0 END) AS IncorrectCount,
        //    SUM(CASE WHEN ssas.AnswerStatus = 'PartialCorrect' THEN 1 ELSE 0 END) AS PartiallyCorrectCount
        //FROM tblStudentScholarshipAnswerSubmission ssas
        //INNER JOIN tblStudentScholarship sss ON ssas.QuestionID = sss.QuestionID
        //WHERE ssas.StudentID = @StudentId
        //AND ssas.ScholarshipID = @ScholarshipId
        //AND sss.SubjectId = @SubjectId;

        //-- 4. Count attempted & unattempted questions based on statuses
        //SELECT 
        //    SUM(CASE WHEN sss.QuestionStatusId IN (1, 5) THEN 1 ELSE 0 END) AS AttemptedCount,
        //    SUM(CASE WHEN sss.QuestionStatusId IN (2, 3, 4) THEN 1 ELSE 0 END) AS UnattemptedCount
        //FROM tblStudentScholarship sss
        //WHERE sss.StudentID = @StudentId
        //AND sss.ScholarshipID = @ScholarshipId
        //AND sss.SubjectID = @SubjectId;";

        //        using (var multi = await _connection.QueryMultipleAsync(sql, new { StudentId = studentId, ScholarshipId = scholarshipId, SubjectId = subjectId }))
        //        {
        //            // 1. Read subject-specific test details
        //            var subjectDetails = await multi.ReadFirstOrDefaultAsync<dynamic>();
        //            if (subjectDetails == null)
        //            {
        //                return new ServiceResponse<ScholarshipAnalytics>(false, "No questions found for the given SubjectId in this ScholarshipTest.", null, 404);
        //            }

        //            int totalQuestions = subjectDetails.TotalQuestions;
        //            decimal subjectTotalMarks = subjectDetails.SubjectTotalMarks ?? 0;
        //            string durationString = subjectDetails.Duration; // Example: "120 minutes"
        //            int duration = int.Parse(durationString.Split(' ')[0]); // Extracts "120" and converts to int


        //            // 2. Read student's obtained marks for the subject
        //            int studentMarks = await multi.ReadFirstOrDefaultAsync<int>();

        //            // 3. Read answer status counts
        //            var counts = await multi.ReadFirstOrDefaultAsync<dynamic>();
        //            int correctCount = counts?.CorrectCount ?? 0;
        //            int incorrectCount = counts?.IncorrectCount ?? 0;
        //            int partiallyCorrectCount = counts?.PartiallyCorrectCount ?? 0;

        //            // 4. Read attempted and unattempted counts
        //            var attemptData = await multi.ReadFirstOrDefaultAsync<dynamic>();
        //            int attemptedCount = attemptData?.AttemptedCount ?? 0;
        //            int unattemptedCount = attemptData?.UnattemptedCount ?? (totalQuestions - attemptedCount);

        //            // Calculate percentages (guarding against division by zero)
        //            decimal correctPercentage = totalQuestions > 0 ? (correctCount * 100.0M / totalQuestions) : 0;
        //            decimal incorrectPercentage = totalQuestions > 0 ? (incorrectCount * 100.0M / totalQuestions) : 0;
        //            decimal partiallyCorrectPercentage = totalQuestions > 0 ? (partiallyCorrectCount * 100.0M / totalQuestions) : 0;
        //            decimal unattemptedPercentage = totalQuestions > 0 ? (unattemptedCount * 100.0M / totalQuestions) : 0;

        //            // Construct and return the analytics result
        //            var response = new ScholarshipAnalytics
        //            {
        //                TotalQuestions = totalQuestions,
        //                Duration = duration,
        //                TestTotalMarks = subjectTotalMarks,
        //                StudentMarks = studentMarks,
        //                CorrectCount = correctCount,
        //                CorrectPercentage = correctPercentage,
        //                IncorrectCount = incorrectCount,
        //                IncorrectPercentage = incorrectPercentage,
        //                PartiallyCorrectCount = partiallyCorrectCount,
        //                PartiallyCorrectPercentage = partiallyCorrectPercentage,
        //                UnattemptedCount = unattemptedCount,
        //                UnattemptedPercentage = unattemptedPercentage
        //            };

        //            return new ServiceResponse<ScholarshipAnalytics>(true, "Operation successful", response, 200);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<ScholarshipAnalytics>(false, ex.Message, null, 500);
        //    }
        //}
        public async Task<ServiceResponse<ScholarshipAnalytics>> GetSubjectWiseScholarshipAnalyticsAsync(int studentId, int scholarshipId, int subjectId)
        {
            try
            {
                var sql = @"
        -- 1. Get Section-wise question constraints
        SELECT 
            SSTSectionId,
            ScholarshipTestId,
            SubjectId,
            TotalNumberOfQuestions,
            NoOfQuestionsPerChoice,
            MarksPerQuestion,
            NegativeMarks
        FROM tblSSQuestionSection
        WHERE ScholarshipTestId = @ScholarshipId
        AND SubjectId = @SubjectId;

        -- 2. Get Student's Answer Data (Ordered by submission time)
        SELECT 
            sss.QuestionID,
            ssas.Marks,
            ssas.AnswerStatus,
            sss.SSTSectionId,
            sss.QuestionStatusId,
            sss.QuestionTypeID
        FROM tblStudentScholarshipAnswerSubmission ssas
        INNER JOIN tblStudentScholarship sss 
            ON ssas.QuestionID = sss.QuestionID
        WHERE ssas.StudentID = @StudentId
        AND ssas.ScholarshipID = @ScholarshipId
        AND sss.SubjectID = @SubjectId
        ORDER BY ssas.SubmissionTime;";
        
        using (var multi = await _connection.QueryMultipleAsync(sql, new { StudentId = studentId, ScholarshipId = scholarshipId, SubjectId = subjectId }))
                {
                    var sections = (await multi.ReadAsync<dynamic>()).ToList();
                    var answers = (await multi.ReadAsync<dynamic>()).ToList();

                    if (!sections.Any())
                    {
                        return new ServiceResponse<ScholarshipAnalytics>(false, "No sections found for the given subject.", null, 404);
                    }

                    // Analytics Calculation Variables
                    int totalQuestions = 0, attemptedCount = 0, unattemptedCount = 0;
                    int correctCount = 0, incorrectCount = 0, partiallyCorrectCount = 0;
                    decimal totalMarks = 0, studentMarks = 0;

                    // Process each section independently
                    foreach (var section in sections)
                    {
                        var sectionAnswers = answers.Where(a => a.SSTSectionId == section.SSTSectionId).ToList();

                        int maxConsideredQuestions = Math.Min(section.TotalNumberOfQuestions, section.NoOfQuestionsPerChoice);
                        totalQuestions += maxConsideredQuestions; // Summing expected questions per section

                        var validAnswers = sectionAnswers.Take(maxConsideredQuestions).ToList(); // Consider only required responses

                        attemptedCount += validAnswers.Count;
                        unattemptedCount += maxConsideredQuestions - validAnswers.Count;

                        foreach (var ans in validAnswers)
                        {
                            studentMarks += ans.Marks;

                            switch (ans.AnswerStatus)
                            {
                                case "Correct":
                                    correctCount++;
                                    break;
                                case "Incorrect":
                                    incorrectCount++;
                                    break;
                                case "PartialCorrect":
                                    partiallyCorrectCount++;
                                    break;
                            }
                        }

                        totalMarks += section.MarksPerQuestion * maxConsideredQuestions;
                    }

                    // Compute Percentages
                    decimal correctPercentage = totalQuestions > 0 ? (correctCount * 100.0M / totalQuestions) : 0;
                    decimal incorrectPercentage = totalQuestions > 0 ? (incorrectCount * 100.0M / totalQuestions) : 0;
                    decimal partiallyCorrectPercentage = totalQuestions > 0 ? (partiallyCorrectCount * 100.0M / totalQuestions) : 0;
                    decimal unattemptedPercentage = totalQuestions > 0 ? (unattemptedCount * 100.0M / totalQuestions) : 0;

                    // Response Data
                    var response = new ScholarshipAnalytics
                    {
                        TotalQuestions = totalQuestions,
                        TestTotalMarks = totalMarks,
                        StudentMarks = studentMarks,
                        CorrectCount = correctCount,
                        CorrectPercentage = correctPercentage,
                        IncorrectCount = incorrectCount,
                        IncorrectPercentage = incorrectPercentage,
                        PartiallyCorrectCount = partiallyCorrectCount,
                        PartiallyCorrectPercentage = partiallyCorrectPercentage,
                        UnattemptedCount = unattemptedCount,
                        UnattemptedPercentage = unattemptedPercentage
                    };

                    return new ServiceResponse<ScholarshipAnalytics>(true, "Operation successful", response, 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ScholarshipAnalytics>(false, ex.Message, null, 500);
            }
        }
        //    public async Task<ServiceResponse<MarksCalculation>> GetMarksCalculationAsync(int studentId, int scholarshipId)
        //    {
        //        try
        //        {
        //            var sql = @"
        //    -- 1. Retrieve test total marks
        //    SELECT TotalMarks 
        //    FROM tblScholarshipTest
        //    WHERE ScholarshipTestId = @ScholarshipId;

        //    -- 2. Achieved Marks (sum of marks for 'Correct' and 'PartialCorrect' answers)
        //    SELECT ISNULL(SUM(Marks), 0)
        //    FROM tblStudentScholarshipAnswerSubmission
        //    WHERE StudentID = @StudentId 
        //      AND ScholarshipID = @ScholarshipId
        //      AND AnswerStatus IN ('Correct', 'PartialCorrect');

        //    -- 3. Negative Marks (absolute sum for 'Incorrect' answers)
        //    SELECT ABS(ISNULL(SUM(Marks), 0))
        //    FROM tblStudentScholarshipAnswerSubmission
        //    WHERE StudentID = @StudentId 
        //      AND ScholarshipID = @ScholarshipId
        //      AND AnswerStatus = 'Incorrect';
        //";
        //            _connection.Open();
        //            using (var multi = await _connection.QueryMultipleAsync(sql, new { StudentId = studentId, ScholarshipId = scholarshipId }))
        //            {
        //                int testTotalMarks = await multi.ReadFirstAsync<int>();
        //                int achievedMarks = await multi.ReadFirstAsync<int>();
        //                int negativeMarks = await multi.ReadFirstAsync<int>();

        //                int finalMarks = achievedMarks - negativeMarks;
        //                decimal marksPercentage = testTotalMarks > 0 ? (finalMarks * 100.0M / testTotalMarks) : 0;

        //                var response = new MarksCalculation
        //                {
        //                    AchievedMarks = achievedMarks,
        //                    NegativeMarks = negativeMarks,
        //                    FinalMarks = finalMarks,
        //                    MarksPercentage = marksPercentage
        //                };
        //                _connection.Close();
        //                return new ServiceResponse<MarksCalculation>(true, "Operation SUccessful", response, 200);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            return new ServiceResponse<MarksCalculation>(false, ex.Message, null, 500);
        //        }
        //    }
        public async Task<ServiceResponse<MarksCalculation>> GetMarksCalculationAsync(int studentId, int scholarshipId)
        {
            try
            {
                var sql = @"
        -- 1. Retrieve test total marks
        SELECT TotalMarks 
        FROM tblScholarshipTest
        WHERE ScholarshipTestId = @ScholarshipId;

        -- 2. Retrieve subject-section constraints
        SELECT SubjectId, SSTSectionId, TotalNumberOfQuestions, NoOfQuestionsPerChoice, MarksPerQuestion, NegativeMarks
        FROM tblSSQuestionSection
        WHERE ScholarshipTestId = @ScholarshipId;

        -- 3. Get student's answered questions sorted by attempt order
        SELECT ssas.QuestionID, ssas.Marks, ssas.AnswerStatus, sss.SubjectId, sss.SSTSectionId
        FROM tblStudentScholarshipAnswerSubmission ssas
        INNER JOIN tblStudentScholarship sss ON ssas.QuestionID = sss.QuestionID
        WHERE ssas.StudentID = @StudentId 
        AND ssas.ScholarshipID = @ScholarshipId
        ORDER BY ssas.QuestionID;
        ";

                using (var multi = await _connection.QueryMultipleAsync(sql, new { StudentId = studentId, ScholarshipId = scholarshipId }))
                {
                    int testTotalMarks = await multi.ReadFirstAsync<int>();

                    var sectionConstraints = (await multi.ReadAsync<dynamic>()).ToList();
                    var studentAnswers = (await multi.ReadAsync<dynamic>()).ToList();

                    decimal achievedMarks = 0, negativeMarks = 0;
                    int totalUnattemptedQuestions = 0;

                    var groupedBySubject = studentAnswers.GroupBy(a => new { a.SubjectId, a.SSTSectionId });

                    foreach (var group in groupedBySubject)
                    {
                        var subjectId = group.Key.SubjectId;
                        var sectionId = group.Key.SSTSectionId;

                        var sectionConfig = sectionConstraints.FirstOrDefault(sc => sc.SSTSectionId == sectionId);
                        if (sectionConfig == null) continue;

                        int N = sectionConfig.TotalNumberOfQuestions - sectionConfig.NoOfQuestionsPerChoice; // New N calculation
                        int totalQuestionsInSection = sectionConfig.TotalNumberOfQuestions;

                        // Consider only the first N valid answers
                        var validAnswers = group.Take(N).ToList();

                        // Count unattempted questions
                        int attemptedCount = validAnswers.Count;
                        int unattemptedCount = Math.Max(0, N - attemptedCount);
                        totalUnattemptedQuestions += unattemptedCount;

                        // Calculate marks
                        achievedMarks += validAnswers.Where(a => a.AnswerStatus == "Correct" || a.AnswerStatus == "PartialCorrect").Sum(a => a.Marks);
                        negativeMarks += Math.Abs(validAnswers.Where(a => a.AnswerStatus == "Incorrect").Sum(a => a.Marks));
                    }

                    decimal finalMarks = achievedMarks - negativeMarks;
                    decimal marksPercentage = testTotalMarks > 0 ? (finalMarks * 100.0M / testTotalMarks) : 0;

                    var response = new MarksCalculation
                    {
                        AchievedMarks = achievedMarks,
                        NegativeMarks = negativeMarks,
                        FinalMarks = finalMarks,
                        MarksPercentage = marksPercentage,
                      //  UnattemptedQuestions = totalUnattemptedQuestions
                    };

                    return new ServiceResponse<MarksCalculation>(true, "Operation Successful", response, 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<MarksCalculation>(false, ex.Message, null, 500);
            }
        }
        //        public async Task<ServiceResponse<MarksCalculation>> GetSubjectWiseMarksCalculationAsync(int studentId, int scholarshipId, int subjectId)
        //        {
        //            try
        //            {
        //                var sql = @"
        //-- 1. Retrieve total marks for the specified subject from the scholarship sections
        //SELECT ISNULL(SUM(TotalNumberOfQuestions * MarksPerQuestion), 0) AS SubjectTotalMarks
        //FROM tblSSQuestionSection
        //WHERE ScholarshipTestId = @ScholarshipId
        //AND SubjectId = @SubjectId;

        //-- 2. Achieved Marks (sum of marks for 'Correct' and 'PartialCorrect' answers)
        //SELECT ISNULL(SUM(ssa.Marks), 0) AS AchievedMarks
        //FROM tblStudentScholarshipAnswerSubmission ssa
        //INNER JOIN tblStudentScholarship ss ON ssa.QuestionID = ss.QuestionID
        //INNER JOIN tblSSQuestionSection ssqs ON ss.SubjectID = ssqs.SubjectID AND ss.QuestionTypeID = ssqs.QuestionTypeID
        //WHERE ssa.StudentID = @StudentId
        //AND ssa.ScholarshipID = @ScholarshipId
        //AND ss.SubjectID = @SubjectId
        //AND ssa.AnswerStatus IN ('Correct', 'PartialCorrect');

        //-- 3. Negative Marks (sum of absolute values for 'Incorrect' answers)
        //SELECT ISNULL(SUM(ssqs.NegativeMarks), 0) AS NegativeMarks
        //FROM tblStudentScholarshipAnswerSubmission ssa
        //INNER JOIN tblStudentScholarship ss ON ssa.QuestionID = ss.QuestionID
        //INNER JOIN tblSSQuestionSection ssqs ON ss.SubjectID = ssqs.SubjectID AND ss.QuestionTypeID = ssqs.QuestionTypeID
        //WHERE ssa.StudentID = @StudentId
        //AND ssa.ScholarshipID = @ScholarshipId
        //AND ss.SubjectID = @SubjectId
        //AND ssa.AnswerStatus = 'Incorrect';";

        //                using (var multi = await _connection.QueryMultipleAsync(sql, new { StudentId = studentId, ScholarshipId = scholarshipId, SubjectId = subjectId }))
        //                {
        //                    int subjectTotalMarks = await multi.ReadFirstAsync<int>();
        //                    int achievedMarks = await multi.ReadFirstAsync<int>();
        //                    int negativeMarks = await multi.ReadFirstAsync<int>();

        //                    int finalMarks = achievedMarks - negativeMarks;
        //                    decimal marksPercentage = subjectTotalMarks > 0 ? (finalMarks * 100.0M / subjectTotalMarks) : 0;

        //                    var response = new MarksCalculation
        //                    {
        //                        AchievedMarks = achievedMarks,
        //                        NegativeMarks = negativeMarks,
        //                        FinalMarks = finalMarks,
        //                        MarksPercentage = marksPercentage
        //                    };

        //                    return new ServiceResponse<MarksCalculation>(true, "Operation successful", response, 200);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                return new ServiceResponse<MarksCalculation>(false, ex.Message, null, 500);
        //            }
        //        }
        public async Task<ServiceResponse<MarksCalculation>> GetSubjectWiseMarksCalculationAsync(int studentId, int scholarshipId, int subjectId)
        {
            try
            {
                var sql = @"
        -- 1. Retrieve total marks for the specified subject
        SELECT ISNULL(SUM(TotalNumberOfQuestions * MarksPerQuestion), 0) AS SubjectTotalMarks
        FROM tblSSQuestionSection
        WHERE ScholarshipTestId = @ScholarshipId
        AND SubjectId = @SubjectId;

        -- 2. Retrieve section constraints for the subject
        SELECT SSTSectionId, TotalNumberOfQuestions, NoOfQuestionsPerChoice, MarksPerQuestion, NegativeMarks
        FROM tblSSQuestionSection
        WHERE ScholarshipTestId = @ScholarshipId
        AND SubjectId = @SubjectId;

        -- 3. Get student's answers for the subject, ordered by question attempt
        SELECT ssas.QuestionID, ssas.Marks, ssas.AnswerStatus, sss.SSTSectionId
        FROM tblStudentScholarshipAnswerSubmission ssas
        INNER JOIN tblStudentScholarship sss ON ssas.QuestionID = sss.QuestionID
        WHERE ssas.StudentID = @StudentId 
        AND ssas.ScholarshipID = @ScholarshipId
        AND sss.SubjectId = @SubjectId
        ORDER BY ssas.QuestionID;
        ";

                using (var multi = await _connection.QueryMultipleAsync(sql, new { StudentId = studentId, ScholarshipId = scholarshipId, SubjectId = subjectId }))
                {
                    int subjectTotalMarks = await multi.ReadFirstAsync<int>();
                    var sectionConstraints = (await multi.ReadAsync<dynamic>()).ToList();
                    var studentAnswers = (await multi.ReadAsync<dynamic>()).ToList();

                    decimal achievedMarks = 0, negativeMarks = 0;
                    int totalUnattemptedQuestions = 0;

                    var groupedBySection = studentAnswers.GroupBy(a => a.SSTSectionId);

                    foreach (var group in groupedBySection)
                    {
                        var sectionId = group.Key;
                        var sectionConfig = sectionConstraints.FirstOrDefault(sc => sc.SSTSectionId == sectionId);
                        if (sectionConfig == null) continue;

                        int N = sectionConfig.TotalNumberOfQuestions - sectionConfig.NoOfQuestionsPerChoice; // Adjusted limit per section

                        // Take only the first N answers
                        var validAnswers = group.Take(N).ToList();

                        // Count unattempted questions
                        int attemptedCount = validAnswers.Count;
                        int unattemptedCount = Math.Max(0, N - attemptedCount);
                        totalUnattemptedQuestions += unattemptedCount;

                        // Calculate marks
                        achievedMarks += validAnswers.Where(a => a.AnswerStatus == "Correct" || a.AnswerStatus == "PartialCorrect").Sum(a => a.Marks);
                        negativeMarks += Math.Abs(validAnswers.Where(a => a.AnswerStatus == "Incorrect").Sum(a => a.Marks));
                    }

                    decimal finalMarks = achievedMarks - negativeMarks;
                    decimal marksPercentage = subjectTotalMarks > 0 ? (finalMarks * 100.0M / subjectTotalMarks) : 0;

                    var response = new MarksCalculation
                    {
                        AchievedMarks = achievedMarks,
                        NegativeMarks = negativeMarks,
                        FinalMarks = finalMarks,
                        MarksPercentage = marksPercentage,
                       // UnattemptedQuestions = totalUnattemptedQuestions
                    };

                    return new ServiceResponse<MarksCalculation>(true, "Operation successful", response, 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<MarksCalculation>(false, ex.Message, null, 500);
            }
        }
        //        public async Task<ServiceResponse<TimeSpentReport>> GetTimeSpentReportAsync(int studentId, int scholarshipId)
        //        {
        //            var sql = @" --1.Aggregate time spent per question and answer status
        //WITH QuestionTimeAggregates AS(
        //    SELECT
        //        qn.QuestionID,
        //        COALESCE(s.AnswerStatus, 'Unattempted') AS AnswerStatus, --If no answer, mark as 'Unattempted'
        //        SUM(DATEDIFF(SECOND, qn.StartTime, qn.EndTime)) AS TotalTimeSpent
        //    FROM tblQuestionNavigation qn
        //    LEFT JOIN tblStudentScholarshipAnswerSubmission s
        //        ON qn.QuestionID = s.QuestionID
        //        AND qn.StudentID = s.StudentID
        //        AND qn.ScholarshipID = s.ScholarshipID
        //    WHERE qn.StudentID = @StudentId
        //      AND qn.ScholarshipID = @ScholarshipId
        //    GROUP BY qn.QuestionID, s.AnswerStatus
        //)
        //--2.Calculate overall and status - specific time metrics
        //SELECT
        //    ISNULL(SUM(TotalTimeSpent), 0) AS TotalTime,
        //    ISNULL(SUM(CASE WHEN AnswerStatus = 'Correct' THEN TotalTimeSpent ELSE 0 END), 0) AS CorrectTotalTime,
        //    ISNULL(NULLIF(AVG(CASE WHEN AnswerStatus = 'Correct' THEN TotalTimeSpent END), NULL), 0) AS CorrectAvgTime,
        //    ISNULL(SUM(CASE WHEN AnswerStatus = 'Incorrect' THEN TotalTimeSpent ELSE 0 END), 0) AS IncorrectTotalTime,
        //    ISNULL(NULLIF(AVG(CASE WHEN AnswerStatus = 'Incorrect' THEN TotalTimeSpent END), NULL), 0) AS IncorrectAvgTime,
        //    ISNULL(SUM(CASE WHEN AnswerStatus = 'PartialCorrect' THEN TotalTimeSpent ELSE 0 END), 0) AS PartialTotalTime,
        //    ISNULL(NULLIF(AVG(CASE WHEN AnswerStatus = 'PartialCorrect' THEN TotalTimeSpent END), NULL), 0) AS PartialAvgTime,
        //    ISNULL(SUM(CASE WHEN AnswerStatus = 'Unattempted' THEN TotalTimeSpent ELSE 0 END), 0) AS UnattemptedTotalTime,
        //    ISNULL(NULLIF(AVG(CASE WHEN AnswerStatus = 'Unattempted' THEN TotalTimeSpent END), NULL), 0) AS UnattemptedAvgTime
        //FROM QuestionTimeAggregates;";
        //            try
        //            {
        //                using (var multi = await _connection.QueryMultipleAsync(sql, new { StudentId = studentId, ScholarshipId = scholarshipId }))
        //                {
        //                    var report = await multi.ReadFirstOrDefaultAsync<TimeSpentReport>();
        //                    if (report == null)
        //                    {
        //                        return new ServiceResponse<TimeSpentReport>(false, "No data found for the given student and scholarship.", null, 404);
        //                    }

        //                    return new ServiceResponse<TimeSpentReport>(true, "Operation successful", report, 200);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                return new ServiceResponse<TimeSpentReport>(false, $"An unexpected error occurred: {ex.Message}", null, 500);
        //            }
        //        }
        public async Task<ServiceResponse<TimeSpentReport>> GetTimeSpentReportAsync(int studentId, int scholarshipId)
        {
            var sql = @"
    -- 1. Retrieve section constraints to determine N
    SELECT SSTSectionId, TotalNumberOfQuestions, NoOfQuestionsPerChoice
    FROM tblSSQuestionSection
    WHERE ScholarshipTestId = @ScholarshipId;

    -- 2. Fetch the student's answers ordered by attempt per section
    WITH OrderedAnswers AS (
        SELECT 
            ssas.QuestionID,
            ssas.AnswerStatus,
            qn.StartTime,
            qn.EndTime,
            ssas.StudentID,
            ssas.ScholarshipID,
            ssas.SSTSectionId,
            ROW_NUMBER() OVER (PARTITION BY ssas.SSTSectionId ORDER BY qn.StartTime) AS AttemptOrder
        FROM tblStudentScholarshipAnswerSubmission ssas
        INNER JOIN tblQuestionNavigation qn 
            ON ssas.QuestionID = qn.QuestionID 
            AND ssas.StudentID = qn.StudentID 
            AND ssas.ScholarshipID = qn.ScholarshipID
        WHERE ssas.StudentID = @StudentId
          AND ssas.ScholarshipID = @ScholarshipId
    )
    -- 3. Filter only the first N answers per section
    , ValidAnswers AS (
        SELECT oa.*
        FROM OrderedAnswers oa
        INNER JOIN tblSSQuestionSection ssqs ON oa.SSTSectionId = ssqs.SSTSectionId
        WHERE oa.AttemptOrder <= (ssqs.TotalNumberOfQuestions - ssqs.NoOfQuestionsPerChoice)
    )
    -- 4. Aggregate time spent per valid question
    , QuestionTimeAggregates AS (
        SELECT
            va.QuestionID,
            COALESCE(va.AnswerStatus, 'Unattempted') AS AnswerStatus,
            SUM(DATEDIFF(SECOND, va.StartTime, va.EndTime)) AS TotalTimeSpent
        FROM ValidAnswers va
        GROUP BY va.QuestionID, va.AnswerStatus
    )
    -- 5. Calculate overall and status-specific time metrics
    SELECT
        ISNULL(SUM(TotalTimeSpent), 0) AS TotalTime,
        ISNULL(SUM(CASE WHEN AnswerStatus = 'Correct' THEN TotalTimeSpent ELSE 0 END), 0) AS CorrectTotalTime,
        ISNULL(NULLIF(AVG(CASE WHEN AnswerStatus = 'Correct' THEN TotalTimeSpent END), NULL), 0) AS CorrectAvgTime,
        ISNULL(SUM(CASE WHEN AnswerStatus = 'Incorrect' THEN TotalTimeSpent ELSE 0 END), 0) AS IncorrectTotalTime,
        ISNULL(NULLIF(AVG(CASE WHEN AnswerStatus = 'Incorrect' THEN TotalTimeSpent END), NULL), 0) AS IncorrectAvgTime,
        ISNULL(SUM(CASE WHEN AnswerStatus = 'PartialCorrect' THEN TotalTimeSpent ELSE 0 END), 0) AS PartialTotalTime,
        ISNULL(NULLIF(AVG(CASE WHEN AnswerStatus = 'PartialCorrect' THEN TotalTimeSpent END), NULL), 0) AS PartialAvgTime,
        ISNULL(SUM(CASE WHEN AnswerStatus = 'Unattempted' THEN TotalTimeSpent ELSE 0 END), 0) AS UnattemptedTotalTime,
        ISNULL(NULLIF(AVG(CASE WHEN AnswerStatus = 'Unattempted' THEN TotalTimeSpent END), NULL), 0) AS UnattemptedAvgTime
    FROM QuestionTimeAggregates;
    ";

            try
            {
                using (var multi = await _connection.QueryMultipleAsync(sql, new { StudentId = studentId, ScholarshipId = scholarshipId }))
                {
                    var report = await multi.ReadFirstOrDefaultAsync<TimeSpentReport>();
                    if (report == null)
                    {
                        return new ServiceResponse<TimeSpentReport>(false, "No data found for the given student and scholarship.", null, 404);
                    }

                    return new ServiceResponse<TimeSpentReport>(true, "Operation successful", report, 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<TimeSpentReport>(false, $"An unexpected error occurred: {ex.Message}", null, 500);
            }
        }

        //        public async Task<ServiceResponse<TimeSpentReport>> GetSubjectWiseTimeSpentReportAsync(int studentId, int scholarshipId, int subjectId)
        //        {
        //            var sql = @"-- 1. Calculate total and average time spent on questions per answer status
        //WITH TimeSpentPerQuestion AS (
        //    SELECT 
        //        qn.QuestionID,
        //        sq.SubjectId,
        //        COALESCE(ssa.AnswerStatus, 'Unattempted') AS AnswerStatus, -- Assign 'Unattempted' before grouping
        //        SUM(DATEDIFF(SECOND, qn.StartTime, qn.EndTime)) AS TotalTimeSpent
        //    FROM tblQuestionNavigation qn
        //    INNER JOIN tblStudentScholarship sq ON qn.QuestionID = sq.QuestionId
        //    LEFT JOIN tblStudentScholarshipAnswerSubmission ssa 
        //        ON qn.QuestionID = ssa.QuestionID 
        //        AND qn.StudentID = ssa.StudentID 
        //        AND qn.ScholarshipID = ssa.ScholarshipID
        //    WHERE qn.StudentID = @StudentId
        //      AND qn.ScholarshipID = @ScholarshipId
        //      AND sq.SubjectId = @SubjectId
        //    GROUP BY qn.QuestionID, sq.SubjectId, COALESCE(ssa.AnswerStatus, 'Unattempted') -- Apply here
        //)
        //-- 2. Calculate total & average time for each category
        //SELECT 
        //    ISNULL(SUM(TotalTimeSpent), 0) AS TotalTime,
        //    ISNULL(SUM(CASE WHEN AnswerStatus = 'Correct' THEN TotalTimeSpent ELSE 0 END), 0) AS CorrectTotalTime,
        //    ISNULL(NULLIF(AVG(CASE WHEN AnswerStatus = 'Correct' THEN TotalTimeSpent END), NULL), 0) AS CorrectAvgTime,
        //    ISNULL(SUM(CASE WHEN AnswerStatus = 'Incorrect' THEN TotalTimeSpent ELSE 0 END), 0) AS IncorrectTotalTime,
        //    ISNULL(NULLIF(AVG(CASE WHEN AnswerStatus = 'Incorrect' THEN TotalTimeSpent END), NULL), 0) AS IncorrectAvgTime,
        //    ISNULL(SUM(CASE WHEN AnswerStatus = 'PartialCorrect' THEN TotalTimeSpent ELSE 0 END), 0) AS PartialTotalTime,
        //    ISNULL(NULLIF(AVG(CASE WHEN AnswerStatus = 'PartialCorrect' THEN TotalTimeSpent END), NULL), 0) AS PartialAvgTime,
        //    ISNULL(SUM(CASE WHEN AnswerStatus = 'Unattempted' THEN TotalTimeSpent ELSE 0 END), 0) AS UnattemptedTotalTime,
        //    ISNULL(NULLIF(AVG(CASE WHEN AnswerStatus = 'Unattempted' THEN TotalTimeSpent END), NULL), 0) AS UnattemptedAvgTime
        //FROM TimeSpentPerQuestion;"; 
        //            try
        //            {
        //                using (var multi = await _connection.QueryMultipleAsync(sql, new { StudentId = studentId, ScholarshipId = scholarshipId, SubjectId = subjectId }))
        //                {
        //                    var report = await multi.ReadFirstOrDefaultAsync<TimeSpentReport>();
        //                    if (report == null)
        //                    {
        //                        return new ServiceResponse<TimeSpentReport>(false, "No data found for the given student, scholarship, and subject.", null, 404);
        //                    }
        //                    return new ServiceResponse<TimeSpentReport>(true, "Operation successful", report, 200);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                return new ServiceResponse<TimeSpentReport>(false, $"An unexpected error occurred: {ex.Message}", null, 500);
        //            }
        //        }
        public async Task<ServiceResponse<TimeSpentReport>> GetSubjectWiseTimeSpentReportAsync(int studentId, int scholarshipId, int subjectId)
        {
            var sql = @"
    -- 1. Retrieve section constraints to determine N (per subject)
    SELECT SSTSectionId, TotalNumberOfQuestions, NoOfQuestionsPerChoice
    FROM tblSSQuestionSection
    WHERE ScholarshipTestId = @ScholarshipId AND SubjectId = @SubjectId;

    -- 2. Fetch the student's answers ordered by attempt per section (for the given subject)
    WITH OrderedAnswers AS (
        SELECT 
            ssas.QuestionID,
            sq.SubjectId,
            ssas.AnswerStatus,
            qn.StartTime,
            qn.EndTime,
            ssas.StudentID,
            ssas.ScholarshipID,
            ssas.SSTSectionId,
            ROW_NUMBER() OVER (PARTITION BY ssas.SSTSectionId ORDER BY qn.StartTime) AS AttemptOrder
        FROM tblStudentScholarshipAnswerSubmission ssas
        INNER JOIN tblQuestionNavigation qn 
            ON ssas.QuestionID = qn.QuestionID 
            AND ssas.StudentID = qn.StudentID 
            AND ssas.ScholarshipID = qn.ScholarshipID
        INNER JOIN tblStudentScholarship sq 
            ON ssas.QuestionID = sq.QuestionId
        WHERE ssas.StudentID = @StudentId
          AND ssas.ScholarshipID = @ScholarshipId
          AND sq.SubjectId = @SubjectId
    )
    -- 3. Filter only the first N answers per section (subject-specific)
    , ValidAnswers AS (
        SELECT oa.*
        FROM OrderedAnswers oa
        INNER JOIN tblSSQuestionSection ssqs 
            ON oa.SSTSectionId = ssqs.SSTSectionId
        WHERE oa.AttemptOrder <= (ssqs.TotalNumberOfQuestions - ssqs.NoOfQuestionsPerChoice)
    )
    -- 4. Aggregate time spent per valid question
    , TimeSpentPerQuestion AS (
        SELECT
            va.QuestionID,
            va.SubjectId,
            COALESCE(va.AnswerStatus, 'Unattempted') AS AnswerStatus,
            SUM(DATEDIFF(SECOND, va.StartTime, va.EndTime)) AS TotalTimeSpent
        FROM ValidAnswers va
        GROUP BY va.QuestionID, va.SubjectId, va.AnswerStatus
    )
    -- 5. Calculate total & average time for each category
    SELECT 
        ISNULL(SUM(TotalTimeSpent), 0) AS TotalTime,
        ISNULL(SUM(CASE WHEN AnswerStatus = 'Correct' THEN TotalTimeSpent ELSE 0 END), 0) AS CorrectTotalTime,
        ISNULL(NULLIF(AVG(CASE WHEN AnswerStatus = 'Correct' THEN TotalTimeSpent END), NULL), 0) AS CorrectAvgTime,
        ISNULL(SUM(CASE WHEN AnswerStatus = 'Incorrect' THEN TotalTimeSpent ELSE 0 END), 0) AS IncorrectTotalTime,
        ISNULL(NULLIF(AVG(CASE WHEN AnswerStatus = 'Incorrect' THEN TotalTimeSpent END), NULL), 0) AS IncorrectAvgTime,
        ISNULL(SUM(CASE WHEN AnswerStatus = 'PartialCorrect' THEN TotalTimeSpent ELSE 0 END), 0) AS PartialTotalTime,
        ISNULL(NULLIF(AVG(CASE WHEN AnswerStatus = 'PartialCorrect' THEN TotalTimeSpent END), NULL), 0) AS PartialAvgTime,
        ISNULL(SUM(CASE WHEN AnswerStatus = 'Unattempted' THEN TotalTimeSpent ELSE 0 END), 0) AS UnattemptedTotalTime,
        ISNULL(NULLIF(AVG(CASE WHEN AnswerStatus = 'Unattempted' THEN TotalTimeSpent END), NULL), 0) AS UnattemptedAvgTime
    FROM TimeSpentPerQuestion;
    ";

            try
            {
                using (var multi = await _connection.QueryMultipleAsync(sql, new { StudentId = studentId, ScholarshipId = scholarshipId, SubjectId = subjectId }))
                {
                    var report = await multi.ReadFirstOrDefaultAsync<TimeSpentReport>();
                    if (report == null)
                    {
                        return new ServiceResponse<TimeSpentReport>(false, "No data found for the given student, scholarship, and subject.", null, 404);
                    }
                    return new ServiceResponse<TimeSpentReport>(true, "Operation successful", report, 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<TimeSpentReport>(false, $"An unexpected error occurred: {ex.Message}", null, 500);
            }
        }
        public async Task<ServiceResponse<int>> AddReviewAsync(int scholarshipId, int studentId, int questionId)
        {
            const string insertQuery = @"
            INSERT INTO tblScholarshipQuestionReview (ScholarshipId, StudentId, QuestionId)
            VALUES (@ScholarshipId, @StudentId, @QuestionId);
            SELECT CAST(SCOPE_IDENTITY() as int);";

            try
            {
                var parameters = new { ScholarshipId = scholarshipId, StudentId = studentId, QuestionId = questionId };
                int newReviewId = await _connection.ExecuteScalarAsync<int>(insertQuery, parameters);
                if (newReviewId == 0)
                {
                    return new ServiceResponse<int>(false, string.Empty, 0, 500);
                }
                else
                {
                    return new ServiceResponse<int>(true, "Review added successfully.", newReviewId, 200);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (ex) as needed
                return new ServiceResponse<int>(false, $"An error occurred: {ex.Message}", 0, 500);
            }
        }
        public async Task<ServiceResponse<string>> UpdateQuestionStatusAsync(int ScholarshipID, int studentId, int questionId, bool isAnswered)
        {
            // Define status IDs
            const int Answered = 1;
            const int NotVisited = 4;
            const int Review = 3;
            const int ReviewWithAnswer = 5;
            const int Unanswered = 2;
            // Check current status
            var currentStatusId = await _connection.QuerySingleOrDefaultAsync<int>(
                @"SELECT QuestionStatusId
                  FROM [tblStudentScholarship]
                  WHERE ScholarshipID = @ScholarshipID AND StudentID = @StudentId AND QuestionID = @QuestionId",
                new { ScholarshipID = ScholarshipID, StudentId = studentId, QuestionId = questionId });
            
            int newStatusId;
            if (currentStatusId == 0)
            {
                // New entry
                newStatusId = isAnswered ? Answered : Review;
            }
            else if (currentStatusId == NotVisited)
            {
                newStatusId = isAnswered ? Answered : Review;
            }
            else if (currentStatusId == Review)
            {
                newStatusId = isAnswered ? Answered : Review;
            }
            else if (currentStatusId == Answered)
            {
                newStatusId = isAnswered ? Answered : ReviewWithAnswer;
            }
            else if (currentStatusId == ReviewWithAnswer)
            {
                newStatusId = isAnswered ? Answered : ReviewWithAnswer;
            }
            else if (currentStatusId == Unanswered)
            {
                newStatusId = isAnswered ? Answered : Unanswered;
            }
            else
            {
                // Default case
                newStatusId = currentStatusId;
            }
            // Update status in the mapping table
            if (currentStatusId == 0)
            {
                // Insert new mapping
                await _connection.ExecuteAsync(
                    @"INSERT INTO tblStudentScholarship (ScholarshipID, StudentID, QuestionID, QuestionStatusId)
                      VALUES (@ScholarshipID, @StudentID, @QuestionId, @QuestionStatusId)",
                    new { ScholarshipID = ScholarshipID, StudentID = studentId, QuestionId = questionId, QuestionStatusId = newStatusId });
            }
            else
            {
                // Update existing mapping
                await _connection.ExecuteAsync(
                    @"UPDATE tblStudentScholarship
                      SET QuestionStatusId = @QuestionStatusId
                      WHERE ScholarshipID = @ScholarshipID AND StudentID = @StudentId AND QuestionID = @QuestionId",
                    new { QuestionStatusId = newStatusId, ScholarshipID = ScholarshipID, StudentId = studentId, QuestionId = questionId });
            }

            // Log the review
            await _connection.ExecuteAsync(
                @"INSERT INTO tblScholarshipQuestionReview (QuestionId, StudentId, ScholarshipId)
                  VALUES (@QuestionId, @StudentId, @ScholarshipId)",
                new { QuestionId = questionId, StudentId = studentId, ScholarshipId = ScholarshipID });

            return new ServiceResponse<string>(true, "status updated successfully", string.Empty, 200);
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