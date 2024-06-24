using Dapper;
using Schools_API.DTOs.Requests;
using Schools_API.DTOs.Response;
using Schools_API.DTOs.ServiceResponse;
using Schools_API.Models;
using Schools_API.Repository.Interfaces;
using System.Data;
using System.Data.SqlClient;

namespace Schools_API.Repository.Implementations
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly IDbConnection _connection;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public QuestionRepository(IDbConnection connection, IWebHostEnvironment hostingEnvironment)
        {
            _connection = connection;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<ServiceResponse<string>> AddUpdateQuestion(QuestionDTO request)
        {
            try
            {
                if (request.QuestionId == 0)
                {
                    var question = new Question
                    {
                        QuestionDescription = request.QuestionDescription,
                        QuestionTypeId = request.QuestionTypeId,
                        Status = true,
                        CreatedBy = request.CreatedBy,
                        CreatedOn = DateTime.Now,
                        subjectID = request.subjectID,
                        ContentIndexId = request.ContentIndexId,
                        EmployeeId = request.EmployeeId,
                        IndexTypeId = request.IndexTypeId,
                        IsApproved = false,
                        IsRejected = false
                    };

                    string insertQuery = @"
                    INSERT INTO tblQuestion (
                        QuestionDescription,
                        QuestionTypeId,
                        Status,
                        CreatedBy,
                        CreatedOn,
                        ModifiedBy,
                        ModifiedOn,
                        subjectID,
                        EmployeeId,
                        IndexTypeId,
                        ContentIndexId,
                        IsRejected,
                        IsApproved
                    ) VALUES (
                        @QuestionDescription,
                        @QuestionTypeId,
                        @Status,
                        @CreatedBy,
                        @CreatedOn,
                        @ModifiedBy,
                        @ModifiedOn,
                        @subjectID,
                        @EmployeeId,
                        @IndexTypeId,
                        @ContentIndexId,
                        @IsRejected,
                        @IsApproved
                    );
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                    int questionId = await _connection.QuerySingleOrDefaultAsync<int>(insertQuery, question);
                    if (questionId != 0)
                    {
                        //courses and difficulty mapping
                        var data = await AddUpdateQIDCourses(request.QIDCourses, questionId);

                        // reference mapping
                        var rowsAffected = await AddUpdateReference(request.References, questionId);

                        //subject details
                        var quesSub = await AddUpdateQuestionSubjectMap(request.QuestionSubjectMappings, questionId);

                        string getQuesType = @"select * from tblQBQuestionType where QuestionTypeID = @QuestionTypeID;";

                        var questTypedata = await _connection.QueryFirstOrDefaultAsync<QuestionTypes>(getQuesType, new { QuestionTypeID = request.QuestionTypeId });
                        var answer = 0;
                        string insertAnswerQuery = @"INSERT INTO [tblAnswerMaster] (Questionid, QuestionTypeid) VALUES (@Questionid, @QuestionTypeid);
                                                   SELECT CAST(SCOPE_IDENTITY() as int)";

                        var Answerid = await _connection.QuerySingleAsync<int>(insertAnswerQuery, new
                        { Questionid = questionId, QuestionTypeid = questTypedata?.QuestionTypeID });

                        if (questTypedata != null)
                        {
                            if (questTypedata.Code == "MCQ " || questTypedata.Code == "TF  " || questTypedata.Code == "MT1" || questTypedata.Code == "MAQ"
                                || questTypedata.Code == "MT2" || questTypedata.Code == "AR" || questTypedata.Code == "C")
                            {
                                if (request.AnswerMultipleChoiceCategories != null)
                                {
                                    foreach (var item in request.AnswerMultipleChoiceCategories)
                                    {
                                        item.Answerid = Answerid;
                                    }
                                    string insertAnsQuery = @"INSERT INTO tblAnswerMultipleChoiceCategory
                                                            (Answerid, Answer, Iscorrect, Matchid) 
                                                            VALUES (@AnswerId, @Answer, @IsCorrect, @MatchId);";
                                    answer = await _connection.ExecuteAsync(insertAnsQuery, request.AnswerMultipleChoiceCategories);
                                }
                            }
                            else
                            {
                                string sql = @"INSERT INTO tblAnswersingleanswercategory 
                                             ( Answerid, Answer)
                                             VALUES ( @AnswerId, @Answer);";
                                if (request.Answersingleanswercategories != null)
                                {
                                    request.Answersingleanswercategories.Answerid = Answerid;
                                    answer = await _connection.ExecuteAsync(sql, request.Answersingleanswercategories);
                                }
                            }
                        }

                        if (rowsAffected > 0 && data > 0 && quesSub > 0 && answer > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Question Added Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Some error occured", string.Empty, 500);
                    }
                }
                else
                {
                    var question = new Question
                    {
                        QuestionId = request.QuestionId,
                        QuestionDescription = request.QuestionDescription,
                        QuestionTypeId = request.QuestionTypeId,
                        Status = request.Status,
                        ModifiedBy = request.ModifiedBy,
                        ModifiedOn = DateTime.Now,
                        subjectID = request.subjectID,
                        IndexTypeId = request.IndexTypeId,
                        EmployeeId = request.EmployeeId,
                        ContentIndexId = request.ContentIndexId,
                        IsRejected = request.IsRejected,
                        IsApproved = request.IsApproved
                    };

                    string updateQuery = @"
                    UPDATE tblQuestion
                    SET 
                        QuestionDescription = @QuestionDescription,
                        QuestionTypeId = @QuestionTypeId,
                        Status = @Status,
                        ModifiedBy = @ModifiedBy,
                        ModifiedOn = @ModifiedOn,
                        subjectID = @subjectID,
                        EmployeeId = @EmployeeId,
                        IndexTypeId = @IndexTypeId,
                        ContentIndexId = @ContentIndexId,
                        IsRejected = @IsRejected,
                        IsApproved = @IsApproved
                    WHERE
                        QuestionId = @QuestionId;";


                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, question);
                    if (rowsAffected > 0)
                    {
                        int count = await _connection.ExecuteScalarAsync<int>("SELECT COUNT(*) " +
                            "FROM tblQIDCourse WHERE QID = @QuestionId", new { request.QuestionId });
                        if (count > 0)
                        {
                            string deleteQuery = @"DELETE FROM tblQIDCourse WHERE QID = @QuestionId";
                            int rowsAffected1 = await _connection.ExecuteAsync(deleteQuery, new { request.QuestionId });
                            if (rowsAffected1 > 0)
                            {
                                //courses and difficulty mapping
                                var data = await AddUpdateQIDCourses(request.QIDCourses, request.QuestionId);
                            }
                        }
                        else
                        {
                            //courses and difficulty mapping
                            var data = await AddUpdateQIDCourses(request.QIDCourses, request.QuestionId);
                        }
                        // reference mapping
                        var rowsAffected2 = await AddUpdateReference(request.References, request.QuestionId);

                        //subject details
                        var quesSub = await AddUpdateQuestionSubjectMap(request.QuestionSubjectMappings, request.QuestionId);


                        string getQuesType = @"select * from tblQBQuestionType where QuestionTypeID = @QuestionTypeID;";

                        var questTypedata = await _connection.QuerySingleAsync<QuestionTypes>(getQuesType, new { QuestionTypeID = request.QuestionTypeId });
                        var answer = 0;
                        string selectQuery = @"SELECT * FROM tblAnswerMaster WHERE Questionid = @Questionid";

                        var answerData = await _connection.QueryFirstOrDefaultAsync<AnswerMaster>(selectQuery, new { Questionid = request.QuestionId });

                        string updateAnswerQuery = @"UPDATE tblAnswerMaster SET Questionid = @Questionid, QuestionTypeid = @QuestionTypeid
                                             WHERE Answerid = @Answerid;";
                        var Answerid = await _connection.ExecuteAsync(updateAnswerQuery, new
                        {
                            Questionid = request.QuestionId,
                            QuestionTypeid = request.QuestionTypeId,
                            Answerid = answerData != null ? answerData.Answerid : 0
                        });

                        if (questTypedata != null)
                        {
                            if (questTypedata.Code == "MCQ " || questTypedata.Code == "TF  " || questTypedata.Code == "MT1" || questTypedata.Code == "MAQ"
                                || questTypedata.Code == "MT2" || questTypedata.Code == "AR" || questTypedata.Code == "C")
                            {
                                if (request.AnswerMultipleChoiceCategories != null)
                                {
                                    string deleteQuery = "DELETE FROM tblAnswerMultipleChoiceCategory WHERE Answerid = @Answerid";
                                    await _connection.ExecuteAsync(deleteQuery, new { answerData?.Answerid });

                                    foreach (var item in request.AnswerMultipleChoiceCategories)
                                    {
                                        item.Answerid = answerData?.Answerid;
                                    }

                                    string insertAnsQuery = @"INSERT INTO tblAnswerMultipleChoiceCategory
                                                            (Answerid, Answer, Iscorrect, Matchid) 
                                                            VALUES (@AnswerId, @Answer, @IsCorrect, @MatchId);";
                                    answer = await _connection.ExecuteAsync(insertAnsQuery, request.AnswerMultipleChoiceCategories);

                                }
                            }
                            else
                            {
                                string updateSingleAnswerQuery = @"UPDATE tblAnswersingleanswercategory SET
                                                                 Answer = @Answer WHERE Answerid = @Answerid";
                                if (request.Answersingleanswercategories != null)
                                    request.Answersingleanswercategories.Answerid = answerData?.Answerid;
                                answer = await _connection.ExecuteAsync(updateSingleAnswerQuery, request.Answersingleanswercategories);
                            }
                        }

                        if (rowsAffected > 0 && quesSub > 0 && answer > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Question Updated Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Some error occured", string.Empty, 500);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetAllQuestionsList(GetAllQuestionListRequest request)
        {
            try
            {
                string countSql = @"SELECT COUNT(*) FROM [tblQuestion]";
                int totalCount = await _connection.ExecuteScalarAsync<int>(countSql);
                string sql = @"
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
                LEFT JOIN tblEmployee e ON q.EmployeeId = e.Employeeid
                LEFT JOIN tblQBIndexType it ON q.IndexTypeId = it.IndexId
                 LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
        LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
        LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
                WHERE (@ContentIndexId = 0 OR q.ContentIndexId = @ContentIndexId)
                  AND (@IndexTypeId = 0 OR q.IndexTypeId = @IndexTypeId)
                  AND q.IsRejected = 0
                  AND q.IsApproved = 0";


                var parameters = new
                {
                    ContentIndexId = request.ContentIndexId == 0 ? 0 : request.ContentIndexId,
                    IndexTypeId = request.IndexTypeId == 0 ? 0 : request.IndexTypeId
                };

                var data = await _connection.QueryAsync<dynamic>(sql, parameters);

                if (data != null)
                {
                    var response = data.Select(item => new QuestionResponseDTO
                    {
                        QuestionId = item.QuestionId,
                        QuestionDescription = item.QuestionDescription,
                        QuestionTypeId = item.QuestionTypeId,
                        Status = item.Status,
                        CreatedBy = item.CreatedBy,
                        CreatedOn = item.CreatedOn,
                        ModifiedBy = item.ModifiedBy,
                        ModifiedOn = item.ModifiedOn,
                        subjectID = item.subjectID,
                        SubjectName = item.SubjectName,
                        EmployeeId = item.EmployeeId,
                        EmployeeName = item.EmployeeName,
                        IndexTypeId = item.IndexTypeId,
                        IndexTypeName = item.IndexTypeName,
                        ContentIndexId = item.ContentIndexId,
                        ContentIndexName = item.ContentIndexName,
                        QIDCourses = GetListOfQIDCourse(item.QuestionId),
                        QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionId),
                        Answersingleanswercategories = GetSingleAnswer(item.QuestionId),
                        AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionId),
                        References = GetQuestionReference(item.QuestionId),
                        IsApproved = item.IsApproved,
                        IsRejected = item.IsRejected
                    }).ToList();

                    var paginatedList = response.Skip((request.PageNumber - 1) * request.PageSize)
                                                .Take(request.PageSize)
                                                .ToList();

                    if (paginatedList.Count != 0)
                    {
                        return new ServiceResponse<List<QuestionResponseDTO>>(true, "Operation Successful", paginatedList, 200, totalCount);
                    }
                    else
                    {
                        return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", [], 404);
                    }
                }
                else
                {
                    return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", [], 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetApprovedQuestionsList(GetAllQuestionListRequest request)
        {
            try
            {
                string sql = @"
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
        LEFT JOIN tblEmployee e ON q.EmployeeId = e.Employeeid
        LEFT JOIN tblQBIndexType it ON q.IndexTypeId = it.IndexId
        LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
        LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
        LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
        WHERE (@ContentIndexId = 0 OR q.ContentIndexId = @ContentIndexId)
          AND (@IndexTypeId = 0 OR q.IndexTypeId = @IndexTypeId)
          AND q.IsApproved = 1";

                var parameters = new
                {
                    ContentIndexId = request.ContentIndexId == 0 ? 0 : request.ContentIndexId,
                    IndexTypeId = request.IndexTypeId == 0 ? 0 : request.IndexTypeId
                };

                var data = await _connection.QueryAsync<dynamic>(sql, parameters);

                if (data != null)
                {
                    var response = data.Select(item => new QuestionResponseDTO
                    {
                        QuestionId = item.QuestionId,
                        QuestionDescription = item.QuestionDescription,
                        QuestionTypeId = item.QuestionTypeId,
                        Status = item.Status,
                        CreatedBy = item.CreatedBy,
                        CreatedOn = item.CreatedOn,
                        ModifiedBy = item.ModifiedBy,
                        ModifiedOn = item.ModifiedOn,
                        subjectID = item.subjectID,
                        SubjectName = item.SubjectName,
                        EmployeeId = item.EmployeeId,
                        EmployeeName = item.EmployeeName,
                        IndexTypeId = item.IndexTypeId,
                        IndexTypeName = item.IndexTypeName,
                        ContentIndexId = item.ContentIndexId,
                        ContentIndexName = item.ContentIndexName,
                        QIDCourses = GetListOfQIDCourse(item.QuestionId),
                        QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionId),
                        Answersingleanswercategories = GetSingleAnswer(item.QuestionId),
                        AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionId),
                        References = GetQuestionReference(item.QuestionId),
                        IsApproved = item.IsApproved,
                        IsRejected = item.IsRejected
                    }).ToList();

                    var paginatedList = response.Skip((request.PageNumber - 1) * request.PageSize)
                                                .Take(request.PageSize)
                                                .ToList();

                    if (paginatedList.Count != 0)
                    {
                        return new ServiceResponse<List<QuestionResponseDTO>>(true, "Operation Successful", paginatedList, 200);
                    }
                    else
                    {
                        return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
                    }
                }
                else
                {
                    return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(false, ex.Message, new List<QuestionResponseDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetRejectedQuestionsList(GetAllQuestionListRequest request)
        {
            try
            {
                string sql = @"
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
        LEFT JOIN tblEmployee e ON q.EmployeeId = e.Employeeid
        LEFT JOIN tblQBIndexType it ON q.IndexTypeId = it.IndexId
        LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
        LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
        LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
        WHERE (@ContentIndexId = 0 OR q.ContentIndexId = @ContentIndexId)
          AND (@IndexTypeId = 0 OR q.IndexTypeId = @IndexTypeId)
          AND q.IsRejected = 1";

                var parameters = new
                {
                    ContentIndexId = request.ContentIndexId == 0 ? 0 : request.ContentIndexId,
                    IndexTypeId = request.IndexTypeId == 0 ? 0 : request.IndexTypeId
                };

                var data = await _connection.QueryAsync<dynamic>(sql, parameters);

                if (data != null)
                {
                    var response = data.Select(item => new QuestionResponseDTO
                    {
                        QuestionId = item.QuestionId,
                        QuestionDescription = item.QuestionDescription,
                        QuestionTypeId = item.QuestionTypeId,
                        Status = item.Status,
                        CreatedBy = item.CreatedBy,
                        CreatedOn = item.CreatedOn,
                        ModifiedBy = item.ModifiedBy,
                        ModifiedOn = item.ModifiedOn,
                        subjectID = item.subjectID,
                        SubjectName = item.SubjectName,
                        EmployeeId = item.EmployeeId,
                        EmployeeName = item.EmployeeName,
                        IndexTypeId = item.IndexTypeId,
                        IndexTypeName = item.IndexTypeName,
                        ContentIndexId = item.ContentIndexId,
                        ContentIndexName = item.ContentIndexName,
                        QIDCourses = GetListOfQIDCourse(item.QuestionId),
                        QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionId),
                        Answersingleanswercategories = GetSingleAnswer(item.QuestionId),
                        AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionId),
                        References = GetQuestionReference(item.QuestionId),
                        IsApproved = item.IsApproved,
                        IsRejected = item.IsRejected
                    }).ToList();

                    var paginatedList = response.Skip((request.PageNumber - 1) * request.PageSize)
                                                .Take(request.PageSize)
                                                .ToList();

                    if (paginatedList.Count != 0)
                    {
                        return new ServiceResponse<List<QuestionResponseDTO>>(true, "Operation Successful", paginatedList, 200);
                    }
                    else
                    {
                        return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
                    }
                }
                else
                {
                    return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(false, ex.Message, new List<QuestionResponseDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<QuestionResponseDTO>> GetQuestionById(int questionId)
        {
            try
            {
                string sql = @"
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
        WHERE q.QuestionId = @QuestionId";

                var parameters = new { QuestionId = questionId };

                var item = await _connection.QueryFirstOrDefaultAsync<dynamic>(sql, parameters);

                if (item != null)
                {
                    var questionResponse = new QuestionResponseDTO
                    {
                        QuestionId = item.QuestionId,
                        QuestionDescription = item.QuestionDescription,
                        SubjectName = item.SubjectName,
                        EmployeeName = item.EmpFirstName,
                        IndexTypeName = item.IndexTypeName,
                        ContentIndexName = item.ContentIndexName,
                        QIDCourses = GetListOfQIDCourse(item.QuestionId),
                        QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionId),
                        Answersingleanswercategories = GetSingleAnswer(item.QuestionId),
                        AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionId),
                        References = GetQuestionReference(item.QuestionId),
                        ContentIndexId = item.ContentIndexId,
                        CreatedBy = item.CreatedBy,
                        CreatedOn = item.CreatedOn,
                        EmployeeId = item.EmployeeId,
                        IndexTypeId = item.IndexTypeId,
                        subjectID=item.subjectID,
                        ModifiedOn = item.ModifiedOn,
                        QuestionTypeId = item.QuestionTypeId,
                        QuestionTypeName = item.QuestionTypeName,
                    };

                    return new ServiceResponse<QuestionResponseDTO>(true, "Operation Successful", questionResponse, 200);
                }
                else
                {
                    return new ServiceResponse<QuestionResponseDTO>(false, "No records found", new QuestionResponseDTO(), 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<QuestionResponseDTO>(false, ex.Message, new QuestionResponseDTO(), 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionComparisonDTO>>> CompareQuestionAsync(QuestionCompareRequest newQuestion)
        {
            string query = "SELECT QuestionId, QuestionDescription FROM tblQuestion";
            var existingQuestions = await _connection.QueryAsync<Question>(query);

            var comparisons = existingQuestions.Select(q => new QuestionComparisonDTO
            {
                QuestionID = q.QuestionId,
                QuestionText = q.QuestionDescription,
                Similarity = CalculateSimilarity(newQuestion.NewQuestion, q.QuestionDescription)
            }).OrderByDescending(c => c.Similarity).Take(10).ToList();

            return new ServiceResponse<List<QuestionComparisonDTO>>(true, "Comparison results", comparisons, 200);
        }
        public async Task<ServiceResponse<string>> RejectQuestion(QuestionRejectionRequestDTO request)
        {
            try
            {
                string updateSql = @"
                UPDATE [tblQuestion]
                SET 
                   IsRejected = @IsRejected
                WHERE
                    QuestionId = @QuestionId";

                var parameters = new
                {
                    request.QuestionId,
                    IsRejected = true
                };
                var affectedRows = await _connection.ExecuteAsync(updateSql, parameters);
                if (affectedRows > 0)
                {
                    string sql = @"
                    INSERT INTO [tblQuestionProfilerRejections]
                    ([Questionid], [CreatedDate], [QuestionRejectReason], [RejectedBy])
                    VALUES (@Questionid, @CreatedDate, @QuestionRejectReason, @RejectedBy);
        
                    SELECT CAST(SCOPE_IDENTITY() as int)";

                    var newId = await _connection.ExecuteScalarAsync<int>(sql, new
                    {
                        request.QuestionId,
                        CreatedDate = request.RejectedDate,
                        QuestionRejectReason = request.RejectedReason,
                        request.Rejectedby
                    });
                    if (newId > 0)
                    {
                        return new ServiceResponse<string>(true, "Question rejected successfully", "Success", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Some error occured", string.Empty, 500);
                    }
                }
                else
                {
                    return new ServiceResponse<string>(false, "Question not found", "Failure", 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<string>> ApproveQuestion(QuestionApprovalRequestDTO request)
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
                using (var transaction = _connection.BeginTransaction())
                {
                    // Update the question status to approved
                    string updateQuestionSql = @"
            UPDATE [tblQuestion]
            SET IsApproved = @IsApproved
            WHERE QuestionId = @QuestionId";

                    var updateParameters = new
                    {
                        request.QuestionId,
                        IsApproved = true
                    };

                    var affectedRows = await _connection.ExecuteAsync(updateQuestionSql, updateParameters, transaction);
                    if (affectedRows == 0)
                    {
                        return new ServiceResponse<string>(false, "Question not found", "Failure", 404);
                    }

                    // Insert into tblQuestionProfilerApproval
                    string insertApprovalSql = @"
            INSERT INTO [tblQuestionProfilerApproval]
            ([QuestionId], [ApprovedBy], [ApprovedDate])
            VALUES (@QuestionId, @ApprovedBy, @ApprovedDate)";

                    var insertApprovalParameters = new
                    {
                        request.QuestionId,
                        request.ApprovedBy,
                        ApprovedDate = request.ApprovedDate ?? DateTime.UtcNow
                    };

                    await _connection.ExecuteAsync(insertApprovalSql, insertApprovalParameters, transaction);

                    // Update tblQuestionProfiler to set status of the current profiler to inactive
                    string updateProfilerSql = @"
            UPDATE tblQuestionProfiler
            SET Status = 0
            WHERE Questionid = @QuestionId AND Status = 1";

                    await _connection.ExecuteAsync(updateProfilerSql, new { request.QuestionId }, transaction);

                    // Commit the transaction
                    transaction.Commit();
                }

                return new ServiceResponse<string>(true, "Question approved successfully", "Success", 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task<ServiceResponse<string>> AssignQuestionToProfiler(QuestionProfilerRequest request)
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
                using (var transaction = _connection.BeginTransaction())
                {
                    // Check if the question is already assigned to a profiler with active status
                    string checkSql = @"
                SELECT QPID
                FROM tblQuestionProfiler
                WHERE Questionid = @Questionid AND Status = 1";

                    var existingProfiler = await _connection.QueryFirstOrDefaultAsync<int?>(checkSql, new { request.Questionid }, transaction);

                    // If the question is already assigned, update the status of the current profiler to false
                    if (existingProfiler.HasValue)
                    {
                        string updateSql = @"
                    UPDATE tblQuestionProfiler
                    SET Status = 0
                    WHERE QPID = @QPID";

                        await _connection.ExecuteAsync(updateSql, new { QPID = existingProfiler.Value }, transaction);
                    }

                    // Insert a new record for the new profiler with ApprovedStatus = false and Status = true
                    string insertSql = @"
                INSERT INTO tblQuestionProfiler (Questionid, EmpId, ApprovedStatus, Status)
                VALUES (@Questionid, @EmpId, 0, 1)";

                    await _connection.ExecuteAsync(insertSql, new { request.Questionid, request.EmpId }, transaction);

                    // Commit the transaction
                    transaction.Commit();
                }

                return new ServiceResponse<string>(true, "Question successfully assigned to profiler", string.Empty, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task<ServiceResponse<QuestionProfilerResponse>> GetQuestionProfilerDetails(int QuestionId)
        {
            try
            {
                string sql = @"
        SELECT qp.QPID, qp.Questionid, qp.EmpId, qp.ApprovedStatus, 
               e.EmpFirstName + ' ' + e.EmpLastName AS EmpName, r.RoleName AS Role, r.RoleID,
               qr.QuestionProfilerRejectionsid AS RejectionId, qr.CreatedDate AS RejectedDate, 
               qr.QuestionRejectReason AS RejectedReason, qr.RejectedBy,
               c.CourseName, c.CourseId, qc.QIDCourseID, qc.LevelId, l.LevelName,
               qc.Status, qc.CreatedBy, qc.CreatedDate, qc.ModifiedBy, qc.ModifiedDate
        FROM tblQuestionProfiler qp
        LEFT JOIN tblEmployee e ON qp.EmpId = e.Employeeid
        LEFT JOIN tblRole r ON e.RoleID = r.RoleID
        LEFT JOIN tblQuestionProfilerRejections qr ON qp.Questionid = qr.Questionid
        LEFT JOIN tblQIDCourse qc ON qp.Questionid = qc.QID
        LEFT JOIN tblCourse c ON qc.CourseID = c.CourseId
        LEFT JOIN tbldifficultylevel l ON qc.LevelId = l.LevelId
        WHERE qp.Questionid = @QuestionId";

                var parameters = new { QuestionId };

                var data = (await _connection.QueryAsync<dynamic>(sql, parameters)).ToList();

                if (data != null && data.Any())
                {
                    var firstRecord = data.First();

                    var response = new QuestionProfilerResponse
                    {
                        QPID = firstRecord.QPID,
                        Questionid = firstRecord.Questionid,
                        ApprovedStatus = firstRecord.ApprovedStatus,
                        Proofers = data.GroupBy(d => new { d.EmpId, d.EmpName, d.Role, d.RoleID })
                                       .Select(g => g.First())
                                       .Select(g => new ProoferList
                                       {
                                           EmpId = g.EmpId,
                                           EmpName = g.EmpName,
                                           Role = g.Role,
                                           RoleId = g.RoleID
                                       }).ToList(),
                        QIDCourses = data.GroupBy(d => new { d.QIDCourseID, d.CourseId, d.CourseName, d.LevelId, d.LevelName, d.Status, d.CreatedBy, d.CreatedDate, d.ModifiedBy, d.ModifiedDate })
                                         .Select(g => g.First())
                                         .Select(g => new QIDCourseResponse
                                         {
                                             QIDCourseID = g.QIDCourseID,
                                             QID = firstRecord.QPID,
                                             CourseID = g.CourseId,
                                             CourseName = g.CourseName,
                                             LevelId = g.LevelId,
                                             LevelName = g.LevelName,
                                             Status = g.Status,
                                             CreatedBy = g.CreatedBy,
                                             CreatedDate = g.CreatedDate,
                                             ModifiedBy = g.ModifiedBy,
                                             ModifiedDate = g.ModifiedDate
                                         }).ToList(),
                        QuestionRejectionResponseDTOs = data.Where(d => d.RejectionId != null)
                                                            .GroupBy(d => new { d.RejectionId, d.Questionid, d.RejectedBy, d.RejectedDate, d.RejectedReason })
                                                            .Select(g => g.First())
                                                            .Select(g => new QuestionRejectionResponseDTO
                                                            {
                                                                RejectionId = g.RejectionId,
                                                                QuestionId = g.Questionid,
                                                                EmpId = g.RejectedBy,
                                                                EmpName = g.EmpName,
                                                                RejectedDate = g.RejectedDate,
                                                                RejectedReason = g.RejectedReason
                                                            }).ToList()
                    };

                    return new ServiceResponse<QuestionProfilerResponse>(true, "Operation Successful", response, 200);
                }
                else
                {
                    return new ServiceResponse<QuestionProfilerResponse>(false, "No records found", new QuestionProfilerResponse(), 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<QuestionProfilerResponse>(false, ex.Message, new QuestionProfilerResponse(), 500);
            }
        }
        private double CalculateSimilarity(string question1, string question2)
        {
            int maxLen = Math.Max(question1.Length, question2.Length);
            if (maxLen == 0) return 100.0;

            int distance = ComputeLevenshteinDistance(question1, question2);
            return (1.0 - (double)distance / maxLen) * 100;
        }
        private int ComputeLevenshteinDistance(string s, string t)
        {
            int[,] d = new int[s.Length + 1, t.Length + 1];

            // Step 1: Initialize the matrix
            for (int i = 0; i <= s.Length; i++)
            {
                d[i, 0] = i;
            }
            for (int j = 0; j <= t.Length; j++)
            {
                d[0, j] = j;
            }

            // Step 2: Fill the matrix
            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[s.Length, t.Length];
        }
        private async Task<int> AddUpdateQIDCourses(List<QIDCourse>? request, int questionId)
        {
            int rowsAffected = 0;
            if (request != null)
            {
                foreach (var data in request)
                {
                    var newQIDCourse = new QIDCourse
                    {
                        QID = questionId,
                        CourseID = data.CourseID,
                        LevelId = data.LevelId,
                        Status = true,
                        CreatedBy = 1,
                        CreatedDate = DateTime.Now,
                        ModifiedBy = 1,
                        ModifiedDate = DateTime.Now,
                        QIDCourseID = data.QIDCourseID
                    };
                    if (data.QIDCourseID == 0)
                    {
                        string insertQuery = @"
            INSERT INTO tblQIDCourse (QID, CourseID, LevelId, Status, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
            VALUES (@QID, @CourseID, @LevelId, @Status, @CreatedBy, @CreatedDate, @ModifiedBy, @ModifiedDate)";

                        rowsAffected = await _connection.ExecuteAsync(insertQuery, newQIDCourse);
                    }
                    else
                    {

                        string updateQuery = @"
            UPDATE tblQIDCourse
            SET QID = @QID,
                CourseID = @CourseID,
                LevelId = @LevelId,
                Status = @Status,
                CreatedBy = @CreatedBy,
                CreatedDate = @CreatedDate,
                ModifiedBy = @ModifiedBy,
                ModifiedDate = @ModifiedDate
            WHERE QIDCourseID = @QIDCourseID";
                        rowsAffected = await _connection.ExecuteAsync(updateQuery, newQIDCourse);
                    }
                }
                return rowsAffected;
            }
            else
            {
                return 0;
            }
        }
        private async Task<int> AddUpdateReference(Reference? request, int questionId)
        {
            if (request != null)
            {
                var newReference = new Reference
                {
                    ReferenceNotes = request.ReferenceNotes,
                    ReferenceURL = request.ReferenceURL,
                    QuestionId = questionId,
                    Status = true,
                    CreatedBy = 1,
                    CreatedOn = DateTime.Now,
                    ModifiedBy = 1,
                    ModifiedOn = DateTime.Now,
                    ReferenceId = request.ReferenceId,
                };
                int rowsAffected;
                if (request.ReferenceId == 0)
                {
                    string insertQuery = @"
                    INSERT INTO tblReference (ReferenceNotes, ReferenceURL, QuestionId,
                                            Status, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
                    VALUES (@ReferenceNotes, @ReferenceURL, @QuestionId,
                            @Status, @CreatedBy, @CreatedOn, @ModifiedBy, @ModifiedOn)";

                    rowsAffected = await _connection.ExecuteAsync(insertQuery, newReference);
                }
                else
                {
                    string updateQuery = @"
                    UPDATE tblReference
                    SET ReferenceNotes = @ReferenceNotes,
                        ReferenceURL = @ReferenceURL,
                        QuestionId = @QuestionId,
                        Status = @Status,
                        CreatedBy = @CreatedBy,
                        CreatedOn = @CreatedOn,
                        ModifiedBy = @ModifiedBy,
                        ModifiedOn = @ModifiedOn
                    WHERE ReferenceId = @ReferenceId";
                    rowsAffected = await _connection.ExecuteAsync(updateQuery, newReference);
                }
                return rowsAffected;
            }
            else
            {
                return 0;
            }
        }
        private async Task<int> AddUpdateQuestionSubjectMap(List<QuestionSubjectMapping>? request, int questionId)
        {
            if (request != null)
            {
                foreach (var data in request)
                {
                    data.questionid = questionId;
                }
                string query = "SELECT COUNT(*) FROM [tblQuestionSubjectMapping] WHERE [questionid] = @questionId";
                int count = await _connection.QueryFirstOrDefaultAsync<int>(query, new { questionId });
                if (count > 0)
                {
                    var deleteDuery = @"DELETE FROM [tblQuestionSubjectMapping] WHERE [questionid] = @questionId;";
                    var rowsAffected = await _connection.ExecuteAsync(deleteDuery, new { questionId });
                    if (rowsAffected > 0)
                    {
                        string insertQuery = @"INSERT INTO tblQuestionSubjectMapping (ContentIndexId, Indexid, questionid) 
                           VALUES (@ContentIndexId, @Indexid, @questionid)";
                        var valuesInserted = await _connection.ExecuteAsync(insertQuery, request);
                        return valuesInserted;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    string insertQuery = @"INSERT INTO tblQuestionSubjectMapping (ContentIndexId, Indexid, questionid) 
                           VALUES (@ContentIndexId, @Indexid, @questionid)";
                    var valuesInserted = await _connection.ExecuteAsync(insertQuery, request);
                    return valuesInserted;
                }
            }
            else
            {
                return 0;
            }
        }
        private List<QIDCourseResponse> GetListOfQIDCourse(int questionId)
        {
            var boardquery = @"
            SELECT qc.*, c.CourseName, l.LevelName
            FROM [tblQIDCourse] qc
            LEFT JOIN tblCourse c ON qc.CourseID = c.CourseID
            LEFT JOIN tbldifficultylevel l ON qc.LevelId = l.LevelId
            WHERE QID = @questionId";

            var data = _connection.Query<QIDCourseResponse>(boardquery, new { questionId });
            return data != null ? data.AsList() : [];
        }
        private Reference GetQuestionReference(int questionId)
        {
            var boardquery = @"SELECT * FROM [tblReference] WHERE QuestionId = @questionId;";

            var data = _connection.QueryFirstOrDefault<Reference>(boardquery, new { questionId });
            return data ?? new Reference();
        }
        private List<QuestionSubjectMappingResponse> GetListOfQuestionSubjectMapping(int questionId)
        {
            var boardquery = @"
            SELECT qsm.*, it.IndexType as IndexTypeName,
            CASE 
                WHEN qsm.Indexid = 1 THEN ci.ContentName_Chapter
                WHEN qsm.Indexid = 2 THEN ct.ContentName_Topic
                WHEN qsm.Indexid = 3 THEN cst.ContentName_SubTopic
            END AS ContentIndexName
            FROM [tblQuestionSubjectMapping] qsm
            LEFT JOIN tblQBIndexType it ON qsm.Indexid = it.IndexId
               LEFT JOIN tblContentIndexChapters ci ON qsm.ContentIndexId = ci.ContentIndexId AND qsm.Indexid = 1
                LEFT JOIN tblContentIndexTopics ct ON qsm.ContentIndexId = ct.ContInIdTopic AND qsm.Indexid = 2
                LEFT JOIN tblContentIndexSubTopics cst ON qsm.ContentIndexId = cst.ContInIdSubTopic AND qsm.Indexid = 3
            WHERE questionid = @questionId";

            var data = _connection.Query<QuestionSubjectMappingResponse>(boardquery, new { questionId });
            return data != null ? data.AsList() : [];
        }
        private Answersingleanswercategory GetSingleAnswer(int questionId)
        {
            var answerMaster = _connection.QueryFirstOrDefault<AnswerMaster>(@"
        SELECT * FROM tblAnswerMaster WHERE Questionid = @Questionid", new { Questionid = questionId });

            if (answerMaster != null)
            {
                string getQuery = @"
            SELECT * FROM [tblAnswersingleanswercategory] WHERE [Answerid] = @Answerid";

                var response = _connection.QueryFirstOrDefault<Answersingleanswercategory>(getQuery, new { answerMaster.Answerid });
                return response ?? new Answersingleanswercategory();
            }
            else
            {
                return new Answersingleanswercategory();
            }
        }
        private List<AnswerMultipleChoiceCategory> GetMultipleAnswers(int questionId)
        {
            var answerMaster = _connection.QueryFirstOrDefault<AnswerMaster>(@"
        SELECT * FROM tblAnswerMaster WHERE Questionid = @Questionid", new { Questionid = questionId });

            if (answerMaster != null)
            {
                string getQuery = @"
            SELECT * FROM [tblAnswerMultipleChoiceCategory] WHERE [Answerid] = @Answerid";

                var response = _connection.Query<AnswerMultipleChoiceCategory>(getQuery, new { answerMaster.Answerid });
                return response.AsList() ?? new List<AnswerMultipleChoiceCategory>();
            }
            else
            {
                return new List<AnswerMultipleChoiceCategory>();
            }
        }
    }
}
