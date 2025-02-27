using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;
using Course_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace Course_API.Repository.Implementations
{
    public class ScholarshipTestRepository : IScholarshipTestRepository
    {
        private readonly IDbConnection _connection;

        public ScholarshipTestRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<List<QuestionResponseDTO>> GetScholarshipQuestionsAsync(int scholarshipTestId, int studentId)
        {
            using (var connection = _connection)
            {
                // Step 1: Fetch mapped question IDs and codes
                string questionMappingQuery = @"
        SELECT QuestionId, QuestionCode
        FROM tblScholarshipQuestions
        WHERE ScholarshipTestId = @ScholarshipTestId AND RegistrationId = @StudentId";

                var questionMappings = (await connection.QueryAsync<dynamic>(
                    questionMappingQuery,
                    new { ScholarshipTestId = scholarshipTestId, StudentId = studentId }
                )).ToList();

                if (!questionMappings.Any()) return new List<QuestionResponseDTO>();

                // Step 2: Fetch question details
                string questionDetailsQuery = @"
        SELECT 
            q.QuestionId, q.QuestionCode, q.QuestionDescription, q.QuestionFormula, q.IsLive, 
            q.QuestionTypeId, q.Status, q.CreatedBy, q.CreatedOn, q.ModifiedBy, q.ModifiedOn, 
            q.SubjectID, s.SubjectName, q.ExamTypeId, e.ExamTypeName, q.EmployeeId, 
            emp.EmpFirstName as EmployeeName, q.IndexTypeId, it.IndexType as IndexTypeName, 
            q.ContentIndexId,
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
        WHERE q.QuestionId IN @QuestionIds";

                var questions = (await connection.QueryAsync<QuestionResponseDTO>(
                    questionDetailsQuery,
                    new { QuestionIds = questionMappings.Select(q => q.QuestionId).ToList() }
                )).ToList();

                // Step 3: Fetch answers for each question
                foreach (var question in questions)
                {
                    bool isSingleAnswer = question.QuestionTypeId == 3 || question.QuestionTypeId == 4 ||
                                          question.QuestionTypeId == 8 || question.QuestionTypeId == 7 ||
                                          question.QuestionTypeId == 9;

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

                        question.Answersingleanswercategories = await connection.QuerySingleOrDefaultAsync<DTOs.Response.Answersingleanswercategory>(
                            singleAnswerQuery,
                            new { QuestionId = question.QuestionId }
                        );
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

                        question.AnswerMultipleChoiceCategories = (await connection.QueryAsync<DTOs.Response.AnswerMultipleChoiceCategory>(
                            multipleAnswerQuery,
                            new { QuestionId = question.QuestionId }
                        )).ToList();
                    }
                }

                return questions;
            }
        }
        public async Task<ServiceResponse<string>> AssignScholarshipQuestionsAsync(int scholarshipTestId)
        {
            try
            {
                var courseId = _connection.QueryFirstOrDefault<int>(@"select CourseId from tblScholarshipCourse where ScholarshipTestId =
@ScholarshipTestId", new { ScholarshipTestId = scholarshipTestId });
                // Fetch sections for the scholarship test
                var sections = await _connection.QueryAsync<dynamic>(
                    @"SELECT SSTSectionId, ScholarshipTestId, SectionName, QuestionTypeId, TotalNumberOfQuestions, SubjectId
              FROM tblSSQuestionSection WHERE ScholarshipTestId = @ScholarshipTestId;",
                    new { ScholarshipTestId = scholarshipTestId });

                foreach (var section in sections)
                {
                    int sectionId = section.SSTSectionId;
                    int totalQuestions = section.TotalNumberOfQuestions;
                    int questionTypeId = section.QuestionTypeId;
                    int subjectId = section.SubjectId;

                    // Fetch difficulty-level configuration for the section
                    var difficulties = await _connection.QueryAsync<dynamic>(
                        @"SELECT DifficultyLevelId, QuesPerDiffiLevel
                  FROM tblScholarshipQuestionDifficulty WHERE SectionId = @SectionId;",
                        new { SectionId = sectionId });

                    foreach (var difficulty in difficulties)
                    {
                        int difficultyLevelId = difficulty.DifficultyLevelId;
                        int quesPerDiffLevel = difficulty.QuesPerDiffiLevel;

                        // Fetch questions for the given difficulty level and section criteria
                        var questions = await _connection.QueryAsync<dynamic>(
                            @"WITH FilteredQuestions AS (
                          SELECT q.QuestionId, q.SubjectId, q.QuestionTypeId, qc.LevelId AS DifficultyLevelId, q.ContentIndexId, q.IsLive
                          FROM tblQuestion q
                          INNER JOIN tblQIDCourse qc 
                              ON q.QuestionId = qc.QID
                          WHERE q.SubjectId = @SubjectId
                            AND q.QuestionTypeId = @QuestionTypeId
                            AND qc.LevelId = @DifficultyLevelId AND qc.CourseID = @CourseID
                            AND q.IsLive = 1
                      )
                      SELECT TOP (@Limit) * FROM FilteredQuestions ORDER BY NEWID();",
                            new
                            {
                                SubjectId = subjectId,
                                QuestionTypeId = questionTypeId,
                                DifficultyLevelId = difficultyLevelId,
                                Limit = quesPerDiffLevel,
                                CourseID = courseId
                            });

                        // Insert selected questions into tblScholarshipQuestions
                        foreach (var question in questions)
                        {
                            await _connection.ExecuteAsync(
                                "INSERT INTO tblScholarshipQuestions (ScholarshipTestId, SubjectId, QuestionId, QuestionCode) VALUES (@ScholarshipTestId, @SubjectId, @QuestionId, @QuestionCode);",
                                new
                                {
                                    ScholarshipTestId = scholarshipTestId,
                                    SubjectId = question.SubjectId,
                                    QuestionId = question.QuestionId,
                                    QuestionCode = $"Q-{question.QuestionId}"
                                });
                        }
                    }
                }

                return new ServiceResponse<string>(true, "Questions assigned successfully", string.Empty, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<string>> ToggleScholarshipTestStatus(int scholarshipTestId)
        {
            if (scholarshipTestId <= 0)
            {
                return new ServiceResponse<string>(false, "Invalid ScholarshipTestId", string.Empty, 400);
            }

            try
            {
                string toggleStatusQuery = @"
        UPDATE tblScholarshipTest
        SET Status = CASE 
            WHEN Status = 1 THEN 0
            ELSE 1
        END
        WHERE ScholarshipTestId = @ScholarshipTestId;

        SELECT Status 
        FROM tblScholarshipTest 
        WHERE ScholarshipTestId = @ScholarshipTestId;";

             
                    var status = await _connection.QueryFirstOrDefaultAsync<int?>(
                        toggleStatusQuery,
                        new { ScholarshipTestId = scholarshipTestId }
                    );

                    if (status.HasValue)
                    {
                        string statusMessage = status.Value == 1 ? "Active" : "Inactive";
                        return new ServiceResponse<string>(
                            true,
                            "Status toggled successfully",
                            $"ScholarshipTestId {scholarshipTestId} is now {statusMessage}.",
                            200
                        );
                    }
                    else
                    {
                        return new ServiceResponse<string>(
                            false,
                            "ScholarshipTestId not found",
                            string.Empty,
                            404
                        );
                    }
                
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(
                    false,
                    "An error occurred while toggling status",
                    ex.Message,
                    500
                );
            }
        }
        public async Task<ServiceResponse<ScholarshipDetailsDTO>> GetScholarshipDetails(int scholarshipTestId)
        {
            var response = new ServiceResponse<ScholarshipDetailsDTO>(true, string.Empty, null, 200);
            try
            {
                // Fetch Scholarship Details
                var scholarshipQuery = @"
                WITH ScholarshipDetails AS (
                    SELECT 
                        st.ScholarshipTestId,
                        st.PatternName,
                        st.TotalNumberOfQuestions,
                        st.Duration,
                        cat.APName,
                        STRING_AGG(b.BoardName, ', ') AS BoardNames,
                        STRING_AGG(cl.ClassName, ', ') AS ClassNames,
                        STRING_AGG(c.CourseName, ', ') AS CourseNames
                    FROM 
                        tblScholarshipTest st
                    LEFT JOIN 
                        tblScholarshipBoards sb ON st.ScholarshipTestId = sb.ScholarshipTestId
                    LEFT JOIN 
                        tblBoard b ON sb.BoardId = b.BoardId
                    LEFT JOIN 
                        tblScholarshipClass sc ON st.ScholarshipTestId = sc.ScholarshipTestId
                    LEFT JOIN 
                        tblClass cl ON sc.ClassId = cl.ClassId
                    LEFT JOIN 
                        tblScholarshipCourse scs ON st.ScholarshipTestId = scs.ScholarshipTestId
                    LEFT JOIN 
                        tblCourse c ON scs.CourseId = c.CourseId
                    LEFT JOIN 
                        tblCategory cat ON st.APID = cat.APId
                    WHERE 
                        st.ScholarshipTestId = @ScholarshipTestId
                    GROUP BY 
                        st.ScholarshipTestId, st.PatternName, st.TotalNumberOfQuestions, st.Duration, cat.APName
                )
                SELECT * FROM ScholarshipDetails";

                var scholarshipDetails = await _connection.QueryFirstOrDefaultAsync<ScholarshipDetailsDTO>(
                    scholarshipQuery, new { ScholarshipTestId = scholarshipTestId });

                // Fetch Student Details
                var studentQuery = @"
                SELECT 
                    r.FirstName,
                    r.LastName,
                    r.EmailID,
                    r.MobileNumber
                FROM 
                    tblStudentScholarship ss
                INNER JOIN 
                    tblRegistration r ON ss.StudentID = r.RegistrationID
                WHERE 
                    ss.ScholarshipID = @ScholarshipTestId";

                var studentDetails = (await _connection.QueryAsync<StudentDetailsDTO>(
                    studentQuery, new { ScholarshipTestId = scholarshipTestId })).ToList();

                // Combine Data
                if (scholarshipDetails != null)
                {
                    scholarshipDetails.Students = studentDetails;
                    response.Data = scholarshipDetails;
                    response.Success = true;
                    response.Message = "Scholarship details fetched successfully.";
                    response.TotalCount = studentDetails.Count();
                }
                else
                {
                    response.Success = false;
                    response.Message = "Scholarship not found.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = $"An error occurred: {ex.Message}";
            }

            return response;
        }
        public async Task<ServiceResponse<int>> AddUpdateScholarshipTest(ScholarshipTestRequestDTO request)
        {
            try
            {
                if (request.ScholarshipTestId == 0)
                {
                    string insertQuery = @"
            INSERT INTO tblScholarshipTest 
            (
                APID, ExamTypeId, PatternName, TotalNumberOfQuestions, Duration, TotalMarks,
                Status, createdon, createdby, EmployeeID
            ) 
            VALUES 
            (
                @APID, @ExamTypeId, @PatternName, @TotalNumberOfQuestions, @Duration, @TotalMarks,
                @Status, @createdon, @createdby, @EmployeeID
            ); 
            SELECT CAST(SCOPE_IDENTITY() as int);";
                    var parameters = new
                    {
                        request.APID,
                        request.ExamTypeId,
                        request.PatternName,
                        request.TotalNumberOfQuestions,
                        request.Duration,
                        request.Status,
                        createdon = DateTime.Now,
                        request.createdby,
                        request.EmployeeID,
                        request.TotalMarks
                    };
                    int newId = await _connection.QuerySingleAsync<int>(insertQuery, parameters);

                    if (newId > 0)
                    {
                        int sub = await ScholarshipTestSubjectMapping(request.ScholarshipSubjects ?? new List<ScholarshipSubjects>(), newId);
                        int cla = await ScholarshipTestClassMapping(request.ScholarshipClasses ?? new List<ScholarshipClass>(), newId);
                        int board = await ScholarshipTestBoardMapping(request.ScholarshipBoards ?? new List<ScholarshipBoards>(), newId);
                        int course = await ScholarshipTestCourseMapping(request.ScholarshipCourses ?? new List<ScholarshipCourse>(), newId);

                        if (sub > 0 && cla > 0 && board > 0 && course > 0)
                        {
                            return new ServiceResponse<int>(true, "Operation successful", newId, 200);
                        }
                        else
                        {
                            return new ServiceResponse<int>(false, "Some error occurred", 0, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<int>(false, "Some error occurred", 0, 500);
                    }
                }
                else
                {
                    string updateQuery = @"
            UPDATE tblScholarshipTest
            SET
                APID = @APID,
                ExamTypeId = @ExamTypeId,
                PatternName = @PatternName,
                TotalNumberOfQuestions = @TotalNumberOfQuestions,
                Duration = @Duration,
                Status = @Status,
                modifiedon = @modifiedon,
                modifiedby = @modifiedby
            WHERE ScholarshipTestId = @ScholarshipTestId;";
                    var parameters = new
                    {
                        request.APID,
                        request.ExamTypeId,
                        request.PatternName,
                        request.TotalNumberOfQuestions,
                        request.Duration,
                        request.Status,
                        modifiedon = DateTime.Now,
                        request.modifiedby,
                        request.ScholarshipTestId
                    };
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, parameters);

                    if (rowsAffected > 0)
                    {
                        int sub = await ScholarshipTestSubjectMapping(request.ScholarshipSubjects ?? new List<ScholarshipSubjects>(), request.ScholarshipTestId);
                        int cla = await ScholarshipTestClassMapping(request.ScholarshipClasses ?? new List<ScholarshipClass>(), request.ScholarshipTestId);
                        int board = await ScholarshipTestBoardMapping(request.ScholarshipBoards ?? new List<ScholarshipBoards>(), request.ScholarshipTestId);
                        int course = await ScholarshipTestCourseMapping(request.ScholarshipCourses ?? new List<ScholarshipCourse>(), request.ScholarshipTestId);

                        if (sub > 0 && cla > 0 && board > 0 && course > 0)
                        {
                            return new ServiceResponse<int>(true, "Operation successful", request.ScholarshipTestId, 200);
                        }
                        else
                        {
                            return new ServiceResponse<int>(false, "Some error occurred", 0, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<int>(false, "Some error occurred", 0, 500);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<int>(false, ex.Message, 0, 500);
            }
        }
        public async Task<ServiceResponse<List<ContentIndexResponses>>> GetSyllabusDetailsBySubject(SyllabusDetailsRequestScholarship request)
        {
            try
            {
                // Fetch APID and ExamTypeID from the TestSeries table
                string scholarshipSql = @"
            SELECT APID, ExamTypeId
            FROM tblScholarshipTest
            WHERE ScholarshipTestId = @ScholarshipTestId";

                var scholarship = await _connection.QueryFirstOrDefaultAsync<dynamic>(scholarshipSql, new { request.ScholarshipTestId });

                if (scholarship == null)
                {
                    return new ServiceResponse<List<ContentIndexResponses>>(false, "Test Series not found", new List<ContentIndexResponses>(), 404);
                }

                int APId = scholarship.APID;
                int? examTypeId = scholarship.ExamTypeId;

                int boardId = 0, classId = 0, courseId = 0;

                if (APId == 1)
                {
                    // Fetch Board, Class, and Course details if APId is 1
                    var boardSql = @"SELECT BoardId FROM tblScholarshipBoards WHERE ScholarshipTestId = @ScholarshipTestId";
                    var classSql = @"SELECT ClassId FROM tblScholarshipClass WHERE ScholarshipTestId = @ScholarshipTestId";
                    var courseSql = @"SELECT CourseId FROM tblScholarshipCourse WHERE ScholarshipTestId = @ScholarshipTestId";

                    boardId = await _connection.QueryFirstOrDefaultAsync<int>(boardSql, new { request.ScholarshipTestId });
                    classId = await _connection.QueryFirstOrDefaultAsync<int>(classSql, new { request.ScholarshipTestId });
                    courseId = await _connection.QueryFirstOrDefaultAsync<int>(courseSql, new { request.ScholarshipTestId });
                }

                // Now proceed with syllabus details retrieval
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
                    APId = APId,
                    BoardId = boardId,
                    ClassId = classId,
                    CourseId = courseId,
                    SubjectId = request.SubjectId
                });

                var contentIndexResponse = new List<ContentIndexResponses>();

                // Existing logic to process the syllabus details goes here
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
        public async Task<ServiceResponse<string>> ScholarshipDiscountSchemeMapping(List<ScholarshipTestDiscountScheme>? request, int ScholarshipTestId)
        {
            // Ensure the connection is initialized
            if (_connection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }

            try
            {
                // Check if the connection is open
                if (_connection.State != System.Data.ConnectionState.Open)
                {
                    _connection.Open();
                }

                if (request == null || !request.Any())
                {
                    return new ServiceResponse<string>(false, "No discount schemes provided", string.Empty, 400);
                }

                // Begin transaction
                using (var transaction = _connection.BeginTransaction())
                {
                    try
                    {
                        // Step 1: Delete existing schemes for the given ScholarshipTestId
                        string deleteQuery = "DELETE FROM tblSSTDiscountScheme WHERE ScholarshipTestId = @ScholarshipTestId";
                        await _connection.ExecuteAsync(deleteQuery, new { ScholarshipTestId }, transaction);

                        // Step 2: Insert new schemes
                        string insertQuery = @"
                INSERT INTO tblSSTDiscountScheme (ScholarshipTestId, PercentageStartRange, PercentageEndRange, Discount)
                VALUES (@ScholarshipTestId, @PercentageStartRange, @PercentageEndRange, @Discount)";

                        await _connection.ExecuteAsync(insertQuery, request.Select(s => new
                        {
                            ScholarshipTestId = s.ScholarshipTestId,
                            PercentageStartRange = s.PercentageStartRange,
                            PercentageEndRange = s.PercentageEndRange,
                            Discount = s.Discount
                        }), transaction);

                        // Commit transaction
                        transaction.Commit();
                    }
                    catch
                    {
                        // Rollback transaction on error
                        transaction.Rollback();
                        throw;
                    }
                }

                return new ServiceResponse<string>(true, "Discount schemes updated successfully", string.Empty, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
            finally
            {
                // Ensure the connection is closed
                if (_connection.State == System.Data.ConnectionState.Open)
                {
                    _connection.Close();
                }
            }
        }
        public async Task<ServiceResponse<string>> ScholarshipContentIndexMapping(List<ScholarshipContentIndex> request, int ScholarshipTestId)
        {
            try
            {
                // Delete existing content index mappings for the given ScholarshipTestId
                await DeleteScholarshipContentIndexes(ScholarshipTestId);

                if (request.Any())
                {
                    string insertQuery = @"
            INSERT INTO tblScholarshipContentIndex
            (ScholarshipTestId, IndexTypeId, ContentIndexId, SubjectId)
            VALUES
            (@ScholarshipTestId, @IndexTypeId, @ContentIndexId, @SubjectId)";

                    int rowsAffected = await _connection.ExecuteAsync(insertQuery, request.Select(contentIndex => new
                    {
                        ScholarshipTestId = ScholarshipTestId,
                        IndexTypeId = contentIndex.IndexTypeId,
                        ContentIndexId = contentIndex.ContentIndexId,
                        SubjectId = contentIndex.SubjectId
                    }));

                    return rowsAffected > 0
                        ? new ServiceResponse<string>(true, "Operation successful", null, 200)
                        : new ServiceResponse<string>(false, "Some error occurred", null, 500);
                }

                return new ServiceResponse<string>(true, "No content indexes to process", null, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<string>> ScholarshipInstructionsMapping(ScholarshipTestInstructions? request, int ScholarshipTestId)
        {


            if (request == null)
            {
                return new ServiceResponse<string>(false, "Invalid request.", string.Empty, 500);
            }
            string query;

            // If SSTInstructionsId is provided and greater than 0, we update the existing record
            if (request.SSTInstructionsId > 0)
            {
                query = @"
                UPDATE [tblSSTInstructions]
                SET 
                    [Instructions] = @Instructions,
                    [InstructionName] = @InstructionName,
                    [InstructionId] = @InstructionId,
                    [ScholarshipTestId] = @ScholarshipTestId
                WHERE [SSTInstructionsId] = @SSTInstructionsId;
            ";
            }
            else
            {
                // Otherwise, we insert a new record
                query = @"
                INSERT INTO [tblSSTInstructions] 
                    ([Instructions], [InstructionName], [InstructionId], [ScholarshipTestId])
                VALUES 
                    (@Instructions, @InstructionName, @InstructionId, @ScholarshipTestId);
                
                -- Optionally return the newly generated SSTInstructionsId
                SELECT CAST(SCOPE_IDENTITY() as int);
            ";
            }

            // Set the parameters and execute the query
            var parameters = new
            {
                request.Instructions,
                request.InstructionName,
                request.InstructionId,
                ScholarshipTestId = request.ScholarshipTestId > 0 ? request.ScholarshipTestId : ScholarshipTestId,
                request.SSTInstructionsId // Used only for updates
            };

            try
            {
                string responseData;
                if (request.SSTInstructionsId > 0)
                {
                    // Perform the update operation
                    await _connection.ExecuteAsync(query, parameters);
                    responseData = $"Updated ScholarshipTestInstruction with ID: {request.SSTInstructionsId}";
                }
                else
                {
                    // Perform the insert operation and get the newly inserted ID
                    var newId = await _connection.QuerySingleAsync<int>(query, parameters);
                    responseData = $"Inserted new ScholarshipTestInstruction with ID: {newId}";
                }

                return new ServiceResponse<string>(true, "Operation Successful", responseData, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<string>> ScholarshipQuestionSectionMapping(List<ScholarshipQuestionSection> request, int ScholarshipId)
        {
            try
            {
                // Step 1: Validate that each subject has at least one question
                var subjectsWithNoQuestions = request
                    .GroupBy(section => section.SubjectId)
                    .Where(group => group.Sum(section => section.TotalNumberOfQuestions) == 0)
                    .Select(group => group.Key)
                    .ToList();

                if (subjectsWithNoQuestions.Any())
                {
                    string missingSubjects = string.Join(", ", subjectsWithNoQuestions);
                    return new ServiceResponse<string>(
                        false,
                        $"The following subjects must have at least one question: {missingSubjects}.",
                        null,
                        StatusCodes.Status400BadRequest
                    );
                }

                // Step 2: Retrieve the total number of questions for the scholarship
                var scholarshipQuery = "SELECT TotalMarks, TotalNumberOfQuestions FROM [tblScholarshipTest] WHERE ScholarshipTestId = @ScholarshipId";

                var scholarshipDateData = await _connection.QueryFirstOrDefaultAsync<(decimal TotalMarks, int TotalNoOfQuestions)>(scholarshipQuery, new { ScholarshipId });
                decimal totalMarksForScholarship = scholarshipDateData.TotalMarks;
                int totalNoOfQuestionsForScholarship = scholarshipDateData.TotalNoOfQuestions;

                // Step 3: Retrieve existing sections and difficulty levels for the scholarship
                var existingSectionsQuery = "SELECT TotalNumberOfQuestions FROM tblSSQuestionSection WHERE ScholarshipTestId = @ScholarshipId";
                var existingSections = await _connection.QueryAsync<int>(existingSectionsQuery, new { ScholarshipId });

                var existingDifficultyLevelsQuery = @"
            SELECT DifficultyLevelId, SUM(QuesPerDiffiLevel) AS TotalQuestions
            FROM tblScholarshipQuestionDifficulty
            WHERE SectionId IN (
                SELECT SSTSectionId
                FROM tblSSQuestionSection
                WHERE ScholarshipTestId = @ScholarshipId
            )
            GROUP BY DifficultyLevelId";
                var existingDifficultyLevels = await _connection.QueryAsync<(int DifficultyLevelId, int TotalQuestions)>(existingDifficultyLevelsQuery, new { ScholarshipId });

                // Step 4: Sum up questions from existing sections
                int totalExistingQuestions = existingSections.Sum();

                // Step 5: Calculate questions from the incoming request
                int totalRequestedQuestions = request.Sum(section => section.TotalNumberOfQuestions);

                // Step 6: Validate the total number of questions
                int totalAssignedQuestions = totalExistingQuestions + totalRequestedQuestions;
                decimal totalRequestedMarks = request.Sum(section => section.TotalNumberOfQuestions * section.MarksPerQuestion);
                if (totalAssignedQuestions != totalNoOfQuestionsForScholarship || totalRequestedMarks != totalMarksForScholarship)
                {
                    return new ServiceResponse<string>(
                      false,
                      $"The total marks ({totalRequestedMarks}) or total questions ({totalRequestedQuestions}) exceed the limits of the test series ({totalMarksForScholarship} marks, {totalNoOfQuestionsForScholarship} questions).",
                      null,
                      StatusCodes.Status400BadRequest
                  );
                }

                // Step 7: Validate questions for difficulty levels
                foreach (var difficultyGroup in request.SelectMany(section => section.ScholarshipSectionQuestionDifficulties)
                                                       .GroupBy(d => d.DifficultyLevelId))
                {
                    int requestedDifficultyTotal = difficultyGroup.Sum(d => d.QuesPerDiffiLevel);
                    int existingDifficultyTotal = existingDifficultyLevels.FirstOrDefault(x => x.DifficultyLevelId == difficultyGroup.Key).TotalQuestions;

                    if (existingDifficultyTotal + requestedDifficultyTotal > totalNoOfQuestionsForScholarship)
                    {
                        return new ServiceResponse<string>(
                            false,
                            $"The total questions for Difficulty Level {difficultyGroup.Key} exceed the allowed limit of {totalNoOfQuestionsForScholarship}.",
                            null,
                            StatusCodes.Status400BadRequest
                        );
                    }
                }

                // Step 8: Update ScholarshipId for all sections in the request
                foreach (var section in request)
                {
                    section.ScholarshipTestId = ScholarshipId;
                }

                // Step 9: Perform Insert or Update operations
                string checkExistenceQuery = @"
            SELECT COUNT(1) 
            FROM tblSSQuestionSection 
            WHERE SSTSectionId = @ScholarshipQuestionSectionId";

                string updateQuery = @"
            UPDATE tblSSQuestionSection
            SET 
                QuestionTypeId = @QuestionTypeId,
                MarksPerQuestion = @MarksPerQuestion,
                NegativeMarks = @NegativeMarks,
                TotalNumberOfQuestions = @TotalNumberOfQuestions,
                NoOfQuestionsPerChoice = @NoOfQuestionsPerChoice,
                SubjectId = @SubjectId, PartialMarkRuleId = @PartialMarkRuleId, SectionName = @SectionName, DisplayOrder = @DisplayOrder
            WHERE SSTSectionId = @SSTSectionId";

                string insertQuery = @"
            INSERT INTO tblSSQuestionSection 
            (ScholarshipTestId,DisplayOrder, SectionName, QuestionTypeId,PartialMarkRuleId, MarksPerQuestion, NegativeMarks, TotalNumberOfQuestions, NoOfQuestionsPerChoice, SubjectId)
            VALUES 
            (@ScholarshipTestId,@DisplayOrder, @SectionName, @QuestionTypeId, @PartialMarkRuleId,@MarksPerQuestion, @NegativeMarks, @TotalNumberOfQuestions, @NoOfQuestionsPerChoice, @SubjectId);
            SELECT CAST(SCOPE_IDENTITY() as int);";

                foreach (var section in request)
                {
                    // Check if the record exists
                    int recordExists = await _connection.ExecuteScalarAsync<int>(checkExistenceQuery, new { ScholarshipQuestionSectionId = section.SSTSectionId });

                    if (recordExists > 0)
                    {
                        // Update existing record
                        await _connection.ExecuteAsync(updateQuery, section);
                    }
                    else
                    {
                        // Insert new record
                        section.SSTSectionId = await _connection.QuerySingleAsync<int>(insertQuery, section);
                    }

                    foreach (var record in section.ScholarshipSectionQuestionDifficulties)
                    {
                        record.SectionId = section.SSTSectionId;
                    }

                    // Handle difficulties for the section
                    await AddUpdateScholarshipQuestionDifficultyAsync(section.ScholarshipSectionQuestionDifficulties);
                }

                return new ServiceResponse<string>(true, "Operation successful", "Sections mapped successfully", StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while mapping scholarship sections", ex);
            }
        }
        private async Task AddUpdateScholarshipQuestionDifficultyAsync(List<ScholarshipSectionQuestionDifficulty> difficulties)
        {
            try
            {
                // SQL queries for inserting or updating difficulties
                string checkExistenceQuery = @"
            SELECT COUNT(1)
            FROM tblScholarshipQuestionDifficulty
            WHERE SectionId = @SectionId AND DifficultyLevelId = @DifficultyLevelId";

                string updateQuery = @"
            UPDATE tblScholarshipQuestionDifficulty
            SET QuesPerDiffiLevel = @QuesPerDiffiLevel
            WHERE SectionId = @SectionId AND DifficultyLevelId = @DifficultyLevelId";

                string insertQuery = @"
            INSERT INTO tblScholarshipQuestionDifficulty
            (SectionId, DifficultyLevelId, QuesPerDiffiLevel)
            VALUES
            (@SectionId, @DifficultyLevelId, @QuesPerDiffiLevel)";

                foreach (var difficulty in difficulties)
                {
                    int recordExists = await _connection.ExecuteScalarAsync<int>(checkExistenceQuery, new
                    {
                        difficulty.SectionId,
                        difficulty.DifficultyLevelId
                    });

                    if (recordExists > 0)
                    {
                        await _connection.ExecuteAsync(updateQuery, difficulty);
                    }
                    else
                    {
                        await _connection.ExecuteAsync(insertQuery, difficulty);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating scholarship question difficulties", ex);
            }
        }
        private async Task DeleteScholarshipQuestionSections(int ScholarshipTestId)
        {
            try
            {
                string deleteSectionsQuery = "DELETE FROM tblSSQuestionSection WHERE ScholarshipTestId = @ScholarshipTestId";
                string deleteDifficultiesQuery = @"
            DELETE FROM tblScholarshipQuestionDifficulty
            WHERE SSTSectionId IN (
                SELECT SSTSectionId
                FROM tblSSQuestionSection
                WHERE ScholarshipTestId = @ScholarshipTestId
            )";

                await _connection.ExecuteAsync(deleteDifficultiesQuery, new { ScholarshipTestId });
                await _connection.ExecuteAsync(deleteSectionsQuery, new { ScholarshipTestId });
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while deleting scholarship question sections", ex);
            }
        }

        //public async Task<ServiceResponse<string>> ScholarshipQuestionSectionMapping(List<ScholarshipQuestionSection> request, int ScholarshipTestId)
        //{
        //    try
        //    {
        //        // Delete existing question sections for the given ScholarshipTestId
        //        await DeleteScholarshipQuestionSections(ScholarshipTestId);

        //        if (request != null && request.Any())
        //        {
        //            string insertQuery = @"
        //    INSERT INTO tblSSQuestionSection
        //    (
        //        ScholarshipTestId, DisplayOrder, SectionName,
        //        LevelId1, QuesPerDifficulty1, LevelId2, QuesPerDifficulty2,
        //        LevelId3, QuesPerDifficulty3, QuestionTypeId, MarksPerQuestion,
        //        NegativeMarks, TotalNumberOfQuestions, NoOfQuestionsPerChoice, SubjectId
        //    )
        //    VALUES
        //    (
        //        @ScholarshipTestId, @DisplayOrder, @SectionName,
        //        @LevelId1, @QuesPerDifficulty1, @LevelId2, @QuesPerDifficulty2,
        //        @LevelId3, @QuesPerDifficulty3, @QuestionTypeId, @MarksPerQuestion,
        //        @NegativeMarks, @TotalNumberOfQuestions, @NoOfQuestionsPerChoice, @SubjectId
        //    )";

        //            var rowsAffected = await _connection.ExecuteAsync(insertQuery, request.Select(section => new
        //            {
        //                ScholarshipTestId = ScholarshipTestId,
        //                DisplayOrder = section.DisplayOrder,
        //                SectionName = section.SectionName,
        //                Status = section.Status,
        //                LevelId1 = section.LevelId1,
        //                QuesPerDifficulty1 = section.QuesPerDifficulty1,
        //                LevelId2 = section.LevelId2,
        //                QuesPerDifficulty2 = section.QuesPerDifficulty2,
        //                LevelId3 = section.LevelId3,
        //                QuesPerDifficulty3 = section.QuesPerDifficulty3,
        //                QuestionTypeId = section.QuestionTypeId,
        //                MarksPerQuestion = section.MarksPerQuestion,
        //                NegativeMarks = section.NegativeMarks,
        //                TotalNumberOfQuestions = section.TotalNumberOfQuestions,
        //                NoOfQuestionsPerChoice = section.NoOfQuestionsPerChoice,
        //                SubjectId = section.SubjectId
        //            }));

        //            return rowsAffected > 0
        //                ? new ServiceResponse<string>(true, "Operation successful", null, 200)
        //                : new ServiceResponse<string>(false, "Some error occurred", null, 500);
        //        }

        //        return new ServiceResponse<string>(true, "No question sections to process", null, 200);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<string>(false, ex.Message, null, 500);
        //    }
        //}
        public async Task<ServiceResponse<string>> ScholarshipQuestionsMapping(List<ScholarshipTestQuestion> request, int ScholarshipTestId, int SSTSectionId)
        {
            try
            {
                // Delete existing questions for the given ScholarshipTestId and SSTSectionId
                await DeleteScholarshipTestQuestions(ScholarshipTestId, SSTSectionId);

                if (request != null && request.Any())
                {
                    // Fetch QuestionId for each QuestionCode where IsActive = 1
                    var questionCodes = request.Select(q => q.QuestionCode).Distinct().ToList();
                    var questionIdLookup = await GetActiveQuestionIdsByCodes(questionCodes);

                    // Prepare data for insertion
                    var insertData = request.Select(question => new
                    {
                        ScholarshipTestId = ScholarshipTestId,
                        SubjectId = question.SubjectId,
                        DisplayOrder = question.DisplayOrder,
                        SSTSectionId = SSTSectionId,
                        QuestionId = questionIdLookup.ContainsKey(question.QuestionCode) ? questionIdLookup[question.QuestionCode] : (int?)null,
                        QuestionCode = question.QuestionCode
                    }).ToList();

                    // Filter out null QuestionIds if necessary
                    insertData = insertData.Where(q => q.QuestionId.HasValue).ToList();

                    if (insertData.Any())
                    {
                        string insertQuery = @"
                INSERT INTO tblSSTQuestions
                (
                    ScholarshipTestId, SubjectId, DisplayOrder, SSTSectionId,
                    QuestionId, QuestionCode
                )
                VALUES
                (
                    @ScholarshipTestId, @SubjectId, @DisplayOrder, @SSTSectionId,
                    @QuestionId, @QuestionCode
                )";

                        var rowsAffected = await _connection.ExecuteAsync(insertQuery, insertData);

                        return rowsAffected > 0
                            ? new ServiceResponse<string>(true, "Operation successful", null, 200)
                            : new ServiceResponse<string>(false, "Some error occurred", null, 500);
                    }
                    else
                    {
                        return new ServiceResponse<string>(true, "No valid questions to process", null, 200);
                    }
                }

                return new ServiceResponse<string>(true, "No questions to process", null, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<List<ScholarshipTestResponseDTO>>> GetScholarshipTestList(ScholarshipGetListRequest request)
        {
            try
            {
                string baseQuery = @"
        SELECT DISTINCT
            st.ScholarshipTestId,
            st.APID,
            ap.APName AS APName,
            st.ExamTypeId,
            et.ExamTypeName AS ExamTypeName,
            st.TotalNumberOfQuestions,
            st.Duration,
            st.Status,
            st.createdon,
            st.createdby,
            st.modifiedon,
            st.modifiedby
        FROM tblScholarshipTest st
        LEFT JOIN tblCategory ap ON st.APID = ap.APID
        LEFT JOIN tblExamType et ON st.ExamTypeId = et.ExamTypeID
        WHERE EXISTS (
            SELECT 1 
            FROM tblSSTDiscountScheme ds 
            WHERE ds.ScholarshipTestId = st.ScholarshipTestId
        )    ORDER BY st.createdon DESC";

                // Applying filters
                if (request.APId > 0)
                {
                    baseQuery += " AND st.APID = @APId";
                }
                if (request.BoardId > 0)
                {
                    baseQuery += " AND EXISTS (SELECT 1 FROM tblScholarshipBoards sb WHERE st.ScholarshipTestId = sb.ScholarshipTestId AND sb.BoardId = @BoardId)";
                }
                if (request.ClassId > 0)
                {
                    baseQuery += " AND EXISTS (SELECT 1 FROM tblScholarshipClass sc WHERE st.ScholarshipTestId = sc.ScholarshipTestId AND sc.ClassId = @ClassId)";
                }
                if (request.CourseId > 0)
                {
                    baseQuery += " AND EXISTS (SELECT 1 FROM tblScholarshipCourse sc WHERE st.ScholarshipTestId = sc.ScholarshipTestId AND sc.CourseId = @CourseId)";
                }
                if (request.ExamTypeId > 0)
                {
                    baseQuery += " AND st.ExamTypeId = @ExamTypeId";
                }

                // Parameters for the query
                var parameters = new
                {
                    APId = request.APId,
                    BoardId = request.BoardId,
                    ClassId = request.ClassId,
                    CourseId = request.CourseId,
                    ExamTypeId = request.ExamTypeId
                };

                // Fetch all matching Scholarship Tests
                var scholarshipTests = (await _connection.QueryAsync<ScholarshipTestResponseDTO>(baseQuery, parameters)).ToList();

                // Total count before pagination
                int totalCount = scholarshipTests.Count;

                // Fetch related data (Board, Class, Course, Subject) for each Scholarship Test
                foreach (var test in scholarshipTests)
                {
                    // Fetch related boards
                    test.ScholarshipBoards = (await _connection.QueryAsync<ScholarshipBoardsResponse>(
                        @"SELECT sb.SSTBoardId, sb.ScholarshipTestId, sb.BoardId, b.BoardName AS Name
                  FROM tblScholarshipBoards sb
                  JOIN tblBoard b ON sb.BoardId = b.BoardId
                  WHERE sb.ScholarshipTestId = @ScholarshipTestId",
                        new { ScholarshipTestId = test.ScholarshipTestId }
                    )).ToList();

                    // Fetch related classes
                    test.ScholarshipClasses = (await _connection.QueryAsync<ScholarshipClassResponse>(
                        @"SELECT sc.SSTClassId, sc.ScholarshipTestId, sc.ClassId, c.ClassName AS Name
                  FROM tblScholarshipClass sc
                  JOIN tblClass c ON sc.ClassId = c.ClassId
                  WHERE sc.ScholarshipTestId = @ScholarshipTestId",
                        new { ScholarshipTestId = test.ScholarshipTestId }
                    )).ToList();

                    // Fetch related courses
                    test.ScholarshipCourses = (await _connection.QueryAsync<ScholarshipCourseResponse>(
                        @"SELECT sc.SSTCourseId, sc.ScholarshipTestId, sc.CourseId, c.CourseName AS Name
                  FROM tblScholarshipCourse sc
                  JOIN tblCourse c ON sc.CourseId = c.CourseId
                  WHERE sc.ScholarshipTestId = @ScholarshipTestId",
                        new { ScholarshipTestId = test.ScholarshipTestId }
                    )).ToList();

                    // Fetch related subjects
                    test.ScholarshipSubjects = (await _connection.QueryAsync<ScholarshipSubjectsResponse>(
                        @"SELECT ss.SSTSubjectId, ss.ScholarshipTestId, ss.SubjectId, s.SubjectName AS SubjectName
                  FROM tblScholarshipSubject ss
                  JOIN tblSubject s ON ss.SubjectId = s.SubjectId
                  WHERE ss.ScholarshipTestId = @ScholarshipTestId",
                        new { ScholarshipTestId = test.ScholarshipTestId }
                    )).ToList();
                }

                // Apply logical pagination
                var paginatedResponse = scholarshipTests
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                // Check if there are records
                if (paginatedResponse.Any())
                {
                    return new ServiceResponse<List<ScholarshipTestResponseDTO>>(true, "Records found", paginatedResponse, 200, totalCount);
                }
                else
                {
                    return new ServiceResponse<List<ScholarshipTestResponseDTO>>(false, "Records not found", new List<ScholarshipTestResponseDTO>(), 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ScholarshipTestResponseDTO>>(false, ex.Message, new List<ScholarshipTestResponseDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<ScholarshipTestResponseDTO>> GetScholarshipTestById(int ScholarshipTestId)
        {
            try
            {
                // Fetch the main ScholarshipTest data
                var query = @"
        SELECT 
            st.ScholarshipTestId,
            st.APID,
            ap.APName AS APName,
            st.ExamTypeId,
            et.ExamTypeName AS ExamTypeName,
            st.PatternName,
            st.TotalNumberOfQuestions,
            st.Duration,
            st.Status,
            st.createdon,
            st.createdby,
            st.modifiedon,
            st.modifiedby,
            st.EmployeeID,
            emp.EmpFirstName AS EmpFirstName
        FROM tblScholarshipTest st
        JOIN tblCategory ap ON st.APID = ap.APID
        JOIN tblEmployee emp ON st.EmployeeID = emp.EmployeeID
        left JOIN tblExamType et ON st.ExamTypeId = et.ExamTypeId
        WHERE st.ScholarshipTestId = @ScholarshipTestId";

                var scholarshipTest = await _connection.QueryFirstOrDefaultAsync<ScholarshipTestResponseDTO>(query, new { ScholarshipTestId });

                if (scholarshipTest == null)
                {
                    return new ServiceResponse<ScholarshipTestResponseDTO>(false, "Scholarship Test not found", new ScholarshipTestResponseDTO(), 404);
                }

                // Fetch related data
                var scholarshipBoards = await GetListOfScholarshipBoards(ScholarshipTestId);
                var scholarshipClasses = await GetListOfScholarshipClasses(ScholarshipTestId);
                var scholarshipCourses = await GetListOfScholarshipCourses(ScholarshipTestId);
                var scholarshipSubjects = await GetListOfScholarshipSubjects(ScholarshipTestId);
                var scholarshipContentIndexes = await GetListOfScholarshipContentIndexes(ScholarshipTestId);
                var scholarshipQuestionSections = await GetListOfScholarshipQuestionSections(ScholarshipTestId);
                var scholarshipInstructions = await GetListOfScholarshipInstructions(ScholarshipTestId);
                var scholarshipDiscounts = await GetListOfScholarshipTestDiscountSchemes(ScholarshipTestId);
                // Initialize the SubjectDetails list
                var scholarshipSubjectDetailsList = new List<ScholarshipSubjectDetails>();

                // Populate ScholarshipSubjectDetails with content indexes and question sections
                foreach (var subject in scholarshipSubjects)
                {
                    var subjectContentIndexes = scholarshipContentIndexes
                        .Where(ci => ci.SubjectId == subject.SubjectID)
                        .ToList();

                    var subjectQuestionSections = scholarshipQuestionSections
                        .Where(qs => qs.SubjectId == subject.SubjectID)
                        .ToList();

                    var subjectDetails = new ScholarshipSubjectDetails
                    {
                        SubjectID = subject.SubjectID,
                        SubjectName = subject.SubjectName,
                        ScholarshipContentIndexResponses = subjectContentIndexes,
                        ScholarshipQuestionSections = subjectQuestionSections
                    };

                    scholarshipSubjectDetailsList.Add(subjectDetails);
                }

                // Map the fetched data to the ScholarshipTestResponseDTO
                scholarshipTest.ScholarshipBoards = scholarshipBoards;
                scholarshipTest.ScholarshipClasses = scholarshipClasses;
                scholarshipTest.ScholarshipCourses = scholarshipCourses;
                scholarshipTest.ScholarshipSubjectDetails = scholarshipSubjectDetailsList;
                scholarshipTest.ScholarshipTestInstructions = scholarshipInstructions;
                scholarshipTest.ScholarshipTestDiscountSchemes = scholarshipDiscounts;
                scholarshipTest.ScholarshipSubjects = scholarshipSubjects;
                // Fetch ScholarshipTestQuestions based on ScholarshipQuestionSections
                if (scholarshipQuestionSections != null && scholarshipQuestionSections.Any())
                {
                    scholarshipTest.ScholarshipTestQuestions = new List<ScholarshipTestQuestion>();
                    foreach (var section in scholarshipQuestionSections)
                    {
                        var questions = await GetListOfScholarshipTestQuestions(section.SSTSectionId);
                        if (questions != null)
                        {
                            scholarshipTest.ScholarshipTestQuestions.AddRange(questions);
                        }
                    }
                }

                return new ServiceResponse<ScholarshipTestResponseDTO>(true, "Success", scholarshipTest, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ScholarshipTestResponseDTO>(false, ex.Message, new ScholarshipTestResponseDTO(), 500);
            }
        }
        private async Task<List<ScholarshipBoardsResponse>> GetListOfScholarshipBoards(int ScholarshipTestId)
        {
            string query = @"
        SELECT 
            stsb.SSTBoardId,
            stsb.ScholarshipTestId,
            stsb.BoardId,
            b.BoardName AS Name
        FROM tblScholarshipBoards stsb
        JOIN tblBoard b ON stsb.BoardId = b.BoardId
        WHERE stsb.ScholarshipTestId = @ScholarshipTestId";
            var data = await _connection.QueryAsync<ScholarshipBoardsResponse>(query, new { ScholarshipTestId });
            return data.ToList();
        }
        private async Task<List<ScholarshipTestDiscountScheme>> GetListOfScholarshipTestDiscountSchemes(int ScholarshipTestId)
        {
            // SQL query to fetch discount schemes based on the ScholarshipTestId
            string query = @"
        SELECT 
            SSTDiscountSchemeId,
            ScholarshipTestId,
            PercentageStartRange,
            PercentageEndRange,
            Discount
        FROM tblSSTDiscountScheme
        WHERE ScholarshipTestId = @ScholarshipTestId";

            // Execute the query and map the result to a list of ScholarshipTestDiscountScheme
            var data = await _connection.QueryAsync<ScholarshipTestDiscountScheme>(query, new { ScholarshipTestId });

            return data.ToList();
        }
        private async Task<List<ScholarshipClassResponse>> GetListOfScholarshipClasses(int ScholarshipTestId)
        {
            string query = @"
        SELECT 
            tsc.SSTClassId,
            tsc.ScholarshipTestId,
            tsc.ClassId,
            c.ClassName AS Name
        FROM tblScholarshipClass tsc
        JOIN tblClass c ON tsc.ClassId = c.ClassId
        WHERE tsc.ScholarshipTestId = @ScholarshipTestId";
            var data = await _connection.QueryAsync<ScholarshipClassResponse>(query, new { ScholarshipTestId });
            return data.ToList();
        }
        private async Task<List<ScholarshipCourseResponse>> GetListOfScholarshipCourses(int ScholarshipTestId)
        {
            string query = @"
        SELECT 
            tsc.SSTCourseId,
            tsc.ScholarshipTestId,
            tsc.CourseId,
            c.CourseName AS Name
        FROM tblScholarshipCourse tsc
        JOIN tblCourse c ON tsc.CourseId = c.CourseId
        WHERE tsc.ScholarshipTestId = @ScholarshipTestId";
            var data = await _connection.QueryAsync<ScholarshipCourseResponse>(query, new { ScholarshipTestId });
            return data.ToList();
        }
        private async Task<List<ScholarshipSubjectsResponse>> GetListOfScholarshipSubjects(int ScholarshipTestId)
        {
            string query = @"
        SELECT 
            tss.SSTSubjectId,
            tss.SubjectId,
            tss.ScholarshipTestId,
            s.SubjectName AS SubjectName
        FROM tblScholarshipSubject tss
        JOIN tblSubject s ON tss.SubjectID = s.SubjectID
        WHERE tss.ScholarshipTestId = @ScholarshipTestId";
            var data = await _connection.QueryAsync<ScholarshipSubjectsResponse>(query, new { ScholarshipTestId });
            return data.ToList();
        }
        private async Task<List<ScholarshipContentIndexResponse>> GetListOfScholarshipContentIndexes(int ScholarshipTestId)
        {
            string query = @"
  SELECT 
      sci.ContentIndexId,
      sci.IndexTypeId,
      it.IndexType AS IndexTypeName,
      sci.SubjectId,
      s.SubjectName AS SubjectName,
      sci.SSTContIndId,
      CASE 
          WHEN sci.IndexTypeId = 1 THEN ci.ContentName_Chapter
          WHEN sci.IndexTypeId = 2 THEN ct.ContentName_Topic
          WHEN sci.IndexTypeId = 3 THEN cst.ContentName_SubTopic
      END AS ContentIndexName,
      sci.ScholarshipTestId
  FROM tblScholarshipContentIndex sci
  LEFT JOIN tblSubject s ON sci.SubjectId = s.SubjectId
  LEFT JOIN tblQBIndexType it ON sci.IndexTypeId = it.IndexId
  LEFT JOIN tblContentIndexChapters ci ON sci.ContentIndexId = ci.ContentIndexId AND sci.IndexTypeId = 1
  LEFT JOIN tblContentIndexTopics ct ON sci.ContentIndexId = ct.ContInIdTopic AND sci.IndexTypeId = 2
  LEFT JOIN tblContentIndexSubTopics cst ON sci.ContentIndexId = cst.ContInIdSubTopic AND sci.IndexTypeId = 3
  WHERE sci.ScholarshipTestId = @ScholarshipTestId";

            var data = await _connection.QueryAsync<ScholarshipContentIndexResponse>(query, new { ScholarshipTestId });
            return data.ToList();
        }
        private async Task<List<ScholarshipQuestionSection>> GetListOfScholarshipQuestionSections(int ScholarshipTestId)
        {
            string query = "SELECT * FROM tblSSQuestionSection WHERE ScholarshipTestId = @ScholarshipTestId";
            var data = await _connection.QueryAsync<ScholarshipQuestionSection>(query, new { ScholarshipTestId });
            return data.ToList();
        }
        private async Task<List<ScholarshipTestInstructions>> GetListOfScholarshipInstructions(int ScholarshipTestId)
        {
            string query = "SELECT * FROM tblSSTInstructions WHERE ScholarshipTestId = @ScholarshipTestId";
            var data = await _connection.QueryAsync<ScholarshipTestInstructions>(query, new { ScholarshipTestId });
            return data.ToList();
        }
        private async Task<List<ScholarshipTestQuestion>> GetListOfScholarshipTestQuestions(int SSTSectionId)
        {
            string query = "SELECT * FROM tblSSTQuestions WHERE SSTSectionId = @SSTSectionId";
            var data = await _connection.QueryAsync<ScholarshipTestQuestion>(query, new { SSTSectionId });
            return data.ToList();
        }
        private async Task<Dictionary<string, int>> GetActiveQuestionIdsByCodes(IEnumerable<string> questionCodes)
        {
            var query = @"
    SELECT QuestionCode, QuestionId
    FROM tblQuestion
    WHERE QuestionCode IN @QuestionCodes AND IsActive = 1";

            var result = await _connection.QueryAsync(query, new { QuestionCodes = questionCodes });

            return result.ToDictionary(row => (string)row.QuestionCode, row => (int)row.QuestionId);
        }
        private async Task DeleteScholarshipTestQuestions(int ScholarshipTestId, int SSTSectionId)
        {
            string query = @"
    DELETE FROM tblSSTQuestions
    WHERE ScholarshipTestId = @ScholarshipTestId AND SSTSectionId = @SSTSectionId";
            await _connection.ExecuteAsync(query, new { ScholarshipTestId = ScholarshipTestId, SSTSectionId = SSTSectionId });
        }
        private async Task<int> ScholarshipTestSubjectMapping(List<ScholarshipSubjects> subjects, int scholarshipTestId)
        {
            await DeleteScholarshipSubjects(scholarshipTestId);

            if (subjects.Any())
            {
                string insertQuery = @"
        INSERT INTO tblScholarshipSubject
        (ScholarshipTestId, SubjectId)
        VALUES
        (@ScholarshipTestId, @SubjectId)";

                int rowsAffected = await _connection.ExecuteAsync(insertQuery, subjects.Select(subject => new
                {
                    ScholarshipTestId = scholarshipTestId,
                    SubjectId = subject.SubjectId
                }));

                return rowsAffected;
            }

            return 1; // Return 1 to indicate success even if no subjects to insert.
        }
        private async Task<int> ScholarshipTestClassMapping(List<ScholarshipClass> classes, int scholarshipTestId)
        {
            await DeleteScholarshipClasses(scholarshipTestId);

            if (classes.Any())
            {
                string insertQuery = @"
        INSERT INTO tblScholarshipClass
        (ScholarshipTestId, ClassId)
        VALUES
        (@ScholarshipTestId, @ClassId)";

                int rowsAffected = await _connection.ExecuteAsync(insertQuery, classes.Select(cls => new
                {
                    ScholarshipTestId = scholarshipTestId,
                    ClassId = cls.ClassId
                }));

                return rowsAffected;
            }

            return 1; // Return 1 to indicate success even if no classes to insert.
        }
        private async Task<int> ScholarshipTestBoardMapping(List<ScholarshipBoards> boards, int scholarshipTestId)
        {
            await DeleteScholarshipBoards(scholarshipTestId);

            if (boards.Any())
            {
                string insertQuery = @"
        INSERT INTO tblScholarshipBoards
        (ScholarshipTestId, BoardId)
        VALUES
        (@ScholarshipTestId, @BoardId)";

                int rowsAffected = await _connection.ExecuteAsync(insertQuery, boards.Select(board => new
                {
                    ScholarshipTestId = scholarshipTestId,
                    BoardId = board.BoardId
                }));

                return rowsAffected;
            }

            return 1; // Return 1 to indicate success even if no boards to insert.
        }
        private async Task<int> ScholarshipTestCourseMapping(List<ScholarshipCourse> courses, int scholarshipTestId)
        {
            await DeleteScholarshipCourses(scholarshipTestId);

            if (courses.Any())
            {
                string insertQuery = @"
        INSERT INTO tblScholarshipCourse
        (ScholarshipTestId, CourseId)
        VALUES
        (@ScholarshipTestId, @CourseId)";

                int rowsAffected = await _connection.ExecuteAsync(insertQuery, courses.Select(course => new
                {
                    ScholarshipTestId = scholarshipTestId,
                    CourseId = course.CourseId
                }));

                return rowsAffected;
            }

            return 1; // Return 1 to indicate success even if no courses to insert.
        }
        private async Task DeleteScholarshipSubjects(int scholarshipTestId)
        {
            string query = "DELETE FROM tblScholarshipSubject WHERE ScholarshipTestId = @ScholarshipTestId";
            await _connection.ExecuteAsync(query, new { ScholarshipTestId = scholarshipTestId });
        }
        private async Task DeleteScholarshipClasses(int scholarshipTestId)
        {
            string query = "DELETE FROM tblScholarshipClass WHERE ScholarshipTestId = @ScholarshipTestId";
            await _connection.ExecuteAsync(query, new { ScholarshipTestId = scholarshipTestId });
        }
        private async Task DeleteScholarshipBoards(int scholarshipTestId)
        {
            string query = "DELETE FROM tblScholarshipBoards WHERE ScholarshipTestId = @ScholarshipTestId";
            await _connection.ExecuteAsync(query, new { ScholarshipTestId = scholarshipTestId });
        }
        private async Task DeleteScholarshipCourses(int scholarshipTestId)
        {
            string query = "DELETE FROM tblScholarshipCourse WHERE ScholarshipTestId = @ScholarshipTestId";
            await _connection.ExecuteAsync(query, new { ScholarshipTestId = scholarshipTestId });
        }
        private async Task DeleteScholarshipContentIndexes(int ScholarshipTestId)
        {
            string query = "DELETE FROM tblScholarshipContentIndex WHERE ScholarshipTestId = @ScholarshipTestId";
            await _connection.ExecuteAsync(query, new { ScholarshipTestId = ScholarshipTestId });
        }
        private async Task DeleteScholarshipTestInstructions(int ScholarshipTestId)
        {
            string query = "DELETE FROM tblSSTInstructions WHERE ScholarshipTestId = @ScholarshipTestId";
            await _connection.ExecuteAsync(query, new { ScholarshipTestId = ScholarshipTestId });
        }
    }
}