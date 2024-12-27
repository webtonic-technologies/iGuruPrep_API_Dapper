using Dapper;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Schools_API.DTOs.Requests;
using Schools_API.DTOs.Response;
using Schools_API.DTOs.ServiceResponse;
using Schools_API.Models;
using Schools_API.Repository.Interfaces;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;

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
                string roleQuery = @"select r.RoleCode from  [tblEmployee] e 
                                                 LEFT JOIN [tblRole] r ON e.RoleID = r.RoleID
                                                 WHERE e.Employeeid = @EmployeeId";
                string GetRoleName = await _connection.QueryFirstOrDefaultAsync<string>(roleQuery, new { EmployeeId = request.ModifierId });

                if (request.EmployeeId != request.ModifierId && request.QuestionCode != "string" && request.QuestionCode != null && GetRoleName != "AD")
                {
                    int QuestionModifier = await _connection.QueryFirstOrDefaultAsync<int>(@"select ModifierId from tblQuestion where QuestionCode = @QuestionCode
                     and IsActive = 1", new { QuestionCode = request.QuestionCode });
                    if (QuestionModifier == request.ModifierId)
                    {
                        string query = @"
                    UPDATE tblQuestion
                    SET 
                        QuestionDescription = @QuestionDescription,
                        QuestionTypeId = @QuestionTypeId,
                        Status = @Status,
                        CreatedBy = @CreatedBy,
                        CreatedOn = @CreatedOn,
                        ModifiedBy = @ModifiedBy,
                        ModifiedOn = @ModifiedOn,
                        subjectID = @SubjectID,
                        EmployeeId = @EmployeeId,
                        ModifierId = @ModifierId,
                        IndexTypeId = @IndexTypeId,
                        ContentIndexId = @ContentIndexId,
                        IsRejected = @IsRejected,
                        IsApproved = @IsApproved,
                        QuestionCode = @QuestionCode,
                        Explanation = @Explanation,
                        ExtraInformation = @ExtraInformation,
                        IsConfigure = @IsConfigure,
                        CategoryId = @CategoryId
                        
                    WHERE 
                        QuestionCode = @QuestionCode and IsActive = 1";
                        var parameters = new
                        {
                            QuestionId = request.QuestionId,
                            QuestionDescription = request.QuestionDescription,
                            QuestionTypeId = request.QuestionTypeId,
                            Status = request.Status,
                            CreatedBy = request.CreatedBy,
                            CreatedOn = request.CreatedOn,
                            ModifiedBy = request.ModifiedBy,
                            ModifiedOn = request.ModifiedOn,
                            SubjectID = request.subjectID,
                            EmployeeId = request.EmployeeId,
                            ModifierId = request.ModifierId,
                            IndexTypeId = request.IndexTypeId,
                            ContentIndexId = request.ContentIndexId,
                            IsRejected = request.IsRejected,
                            IsApproved = request.IsApproved,
                            QuestionCode = request.QuestionCode,
                            Explanation = request.Explanation,
                            ExtraInformation = request.ExtraInformation,
                            IsActive = request.IsActive,
                            IsConfigure = request.IsConfigure,
                            CategoryId = request.CategoryId
                        };

                        int rowsAffected = _connection.Execute(query, parameters);
                        var insertedQuestionCode = request.QuestionCode;
                        if (!string.IsNullOrEmpty(insertedQuestionCode))
                        {
                            // Handle QIDCourses mapping
                            var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);
                            var answer = await AnswerHandling(request.QuestionTypeId, request.AnswerMultipleChoiceCategories, request.QuestionId, insertedQuestionCode, request.Answersingleanswercategories);

                            if (data > 0 && answer.Data > 0)
                            {
                                return new ServiceResponse<string>(true, "Operation Successful", "Question Updated Successfully", 200);
                            }
                            else
                            {
                                return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                            }
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Some error occurred", string.Empty, 500);
                        }
                    }
                    else
                    {
                        int count = await _connection.QueryFirstOrDefaultAsync<int>(@"select count(*) from tblQuestionProfilerRejections where QuestionCode = @QuestionCode", new { QuestionCode = request.QuestionCode });
                        bool isRejectedQuestion = await _connection.QueryFirstOrDefaultAsync<bool>(@"select IsRejected from tblQuestion where QuestionCode = @QuestionCode and IsActive = 1", new { QuestionCode = request.QuestionCode });
                        bool isRejected = false;
                        if (count == 0 || !isRejectedQuestion)
                        {
                            isRejected = false;
                        }
                        else
                        {
                            isRejected = true;
                        }
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
                            IsRejected = isRejected,
                            QuestionCode = request.QuestionCode,
                            Explanation = request.Explanation,
                            ExtraInformation = request.ExtraInformation,
                            IsActive = true,
                            IsConfigure = true,
                            ModifierId = request.ModifierId,
                            CategoryId = request.CategoryId
                        };
                        string insertQuery = @"
              INSERT INTO tblQuestion (
                  QuestionDescription,
                  QuestionTypeId,
                  Status,
                  CreatedBy,
                  CreatedOn,
                  subjectID,
                  EmployeeId,
                  IndexTypeId,
                  ContentIndexId,
                  IsRejected,
                  IsApproved,
                  QuestionCode,
                  Explanation,
                  ExtraInformation,
                  IsActive,
                  IsConfigure,
                  ModifierId, CategoryId
              ) VALUES (
                  @QuestionDescription,
                  @QuestionTypeId,
                  @Status,
                  @CreatedBy,
                  @CreatedOn,
                  @subjectID,
                  @EmployeeId,
                  @IndexTypeId,
                  @ContentIndexId,
                  @IsRejected,
                  @IsApproved,
                  @QuestionCode,
                  @Explanation,
                  @ExtraInformation,
                  @IsActive, @IsConfigure, @ModifierId, @CategoryId
              );

              -- Fetch the QuestionId of the newly inserted row
              SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        string deactivateQuery = @"UPDATE tblQuestion SET IsActive = 0 WHERE QuestionCode = @QuestionCode AND IsActive = 1";
                        await _connection.ExecuteAsync(deactivateQuery, new { request.QuestionCode });
                        // Retrieve the QuestionCode after insertion
                        // var insertedQuestionCode = await _connection.QuerySingleOrDefaultAsync<string>(insertQuery, question);
                        var insertedQuestionId = await _connection.QuerySingleOrDefaultAsync<int>(insertQuery, question);
                        string code = string.Empty;
                        if (string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string")
                        {
                            code = GenerateQuestionCode(request.IndexTypeId, request.ContentIndexId, insertedQuestionId);

                            string questionCodeQuery = @"
                UPDATE tblQuestion
                SET QuestionCode = @QuestionCode
                WHERE QuestionId = @QuestionId AND IsActive = 1";

                            await _connection.ExecuteAsync(questionCodeQuery, new { QuestionCode = code, QuestionId = insertedQuestionId });
                        }
                        string insertedQuestionCode = string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string" ? code : request.QuestionCode;

                        if (!string.IsNullOrEmpty(insertedQuestionCode))
                        {
                            // Handle QIDCourses mapping
                            foreach (var record in request.QIDCourses)
                            {
                                record.QIDCourseID = 0;
                            }
                            var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);
                            if (request.AnswerMultipleChoiceCategories != null)
                            {
                                foreach (var detail in request.AnswerMultipleChoiceCategories)
                                {
                                    detail.Answermultiplechoicecategoryid = 0;
                                }
                            }
                            if (request.Answersingleanswercategories != null)
                            {
                                request.Answersingleanswercategories.Answersingleanswercategoryid = 0;
                            }
                            var answer = await AnswerHandling(request.QuestionTypeId, request.AnswerMultipleChoiceCategories, insertedQuestionId, insertedQuestionCode, request.Answersingleanswercategories);

                            if (data > 0 && answer.Data > 0)
                            {
                                return new ServiceResponse<string>(true, "Operation Successful", "Question Added Successfully", 200);
                            }
                            else
                            {
                                return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                            }
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Some error occurred", string.Empty, 500);
                        }
                    }
                }
                else if (request.EmployeeId == request.ModifierId && request.ModifierId > 0)
                {
                    var count1 = _connection.QueryFirstOrDefault<int>(@"select * from tblQuestionProfiler where QuestionCode = @QuestionCode "
                                , new { QuestionCode = request.QuestionCode, EmpId = request.ModifierId });
                    if (count1 > 0)
                    {
                        int count = await _connection.QueryFirstOrDefaultAsync<int>(@"select count(*) from tblQuestionProfilerRejections where QuestionCode = @QuestionCode", new { QuestionCode = request.QuestionCode });
                        //bool isRejectedQuestion = await _connection.QueryFirstOrDefaultAsync<bool>(@"select IsRejected from tblQuestion where QuestionCode = @QuestionCode and IsActive = 1", new { QuestionCode = request.QuestionCode });
                        bool isRejected = false;
                        if (count > 0)
                        {
                            isRejected = true;
                        }

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
                            IsRejected = isRejected,
                            QuestionCode = request.QuestionCode,
                            Explanation = request.Explanation,
                            ExtraInformation = request.ExtraInformation,
                            IsActive = true,
                            IsConfigure = true,
                            ModifierId = request.ModifierId,
                            CategoryId = request.CategoryId
                        };
                        string insertQuery = @"
              INSERT INTO tblQuestion (
                  QuestionDescription,
                  QuestionTypeId,
                  Status,
                  CreatedBy,
                  CreatedOn,
                  subjectID,
                  EmployeeId,
                  IndexTypeId,
                  ContentIndexId,
                  IsRejected,
                  IsApproved,
                  QuestionCode,
                  Explanation,
                  ExtraInformation,
                  IsActive,
                  IsConfigure,
                  ModifierId,
                CategoryId
              ) VALUES (
                  @QuestionDescription,
                  @QuestionTypeId,
                  @Status,
                  @CreatedBy,
                  @CreatedOn,
                  @subjectID,
                  @EmployeeId,
                  @IndexTypeId,
                  @ContentIndexId,
                  @IsRejected,
                  @IsApproved,
                  @QuestionCode,
                  @Explanation,
                  @ExtraInformation,
                  @IsActive, @IsConfigure, @ModifierId, @CategoryId
              );

              -- Fetch the QuestionId of the newly inserted row
              SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        string deactivateQuery = @"UPDATE tblQuestion SET IsActive = 0 WHERE QuestionCode = @QuestionCode AND IsActive = 1";
                        await _connection.ExecuteAsync(deactivateQuery, new { request.QuestionCode });
                        // Retrieve the QuestionCode after insertion
                        // var insertedQuestionCode = await _connection.QuerySingleOrDefaultAsync<string>(insertQuery, question);
                        var insertedQuestionId = await _connection.QuerySingleOrDefaultAsync<int>(insertQuery, question);
                        string code = string.Empty;
                        if (string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string")
                        {
                            code = GenerateQuestionCode(request.IndexTypeId, request.ContentIndexId, insertedQuestionId);

                            string questionCodeQuery = @"
                UPDATE tblQuestion
                SET QuestionCode = @QuestionCode
                WHERE QuestionId = @QuestionId AND IsActive = 1";

                            await _connection.ExecuteAsync(questionCodeQuery, new { QuestionCode = code, QuestionId = insertedQuestionId });
                        }
                        string insertedQuestionCode = string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string" ? code : request.QuestionCode;

                        if (!string.IsNullOrEmpty(insertedQuestionCode))
                        {
                            // Handle QIDCourses mapping
                            foreach (var record in request.QIDCourses)
                            {
                                record.QIDCourseID = 0;
                            }
                            var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);
                            if (request.AnswerMultipleChoiceCategories != null)
                            {
                                foreach (var detail in request.AnswerMultipleChoiceCategories)
                                {
                                    detail.Answermultiplechoicecategoryid = 0;
                                }
                            }
                            if (request.Answersingleanswercategories != null)
                            {
                                request.Answersingleanswercategories.Answersingleanswercategoryid = 0;
                            }
                            var answer = await AnswerHandling(request.QuestionTypeId, request.AnswerMultipleChoiceCategories, insertedQuestionId, insertedQuestionCode, request.Answersingleanswercategories);

                            if (data > 0 && answer.Data > 0)
                            {
                                return new ServiceResponse<string>(true, "Operation Successful", "Question Added Successfully", 200);
                            }
                            else
                            {
                                return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                            }
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Some error occurred", string.Empty, 500);
                        }
                    }
                    else
                    {
                        string query = @"
                    UPDATE tblQuestion
                    SET 
                        QuestionDescription = @QuestionDescription,
                        QuestionTypeId = @QuestionTypeId,
                        Status = @Status,
                        CreatedBy = @CreatedBy,
                        CreatedOn = @CreatedOn,
                        ModifiedBy = @ModifiedBy,
                        ModifiedOn = @ModifiedOn,
                        subjectID = @SubjectID,
                        EmployeeId = @EmployeeId,
                        ModifierId = @ModifierId,
                        IndexTypeId = @IndexTypeId,
                        ContentIndexId = @ContentIndexId,
                        IsRejected = @IsRejected,
                        IsApproved = @IsApproved,
                        QuestionCode = @QuestionCode,
                        Explanation = @Explanation,
                        ExtraInformation = @ExtraInformation,
                        IsConfigure = @IsConfigure,
                        CategoryId = @CategoryId
                    WHERE 
                        QuestionCode = @QuestionCode and IsActive = 1";
                        var parameters = new
                        {
                            QuestionId = request.QuestionId,
                            QuestionDescription = request.QuestionDescription,
                            QuestionTypeId = request.QuestionTypeId,
                            Status = request.Status,
                            CreatedBy = request.CreatedBy,
                            CreatedOn = request.CreatedOn,
                            ModifiedBy = request.ModifiedBy,
                            ModifiedOn = request.ModifiedOn,
                            SubjectID = request.subjectID,
                            EmployeeId = request.EmployeeId,
                            ModifierId = request.ModifierId,
                            IndexTypeId = request.IndexTypeId,
                            ContentIndexId = request.ContentIndexId,
                            IsRejected = request.IsRejected,
                            IsApproved = request.IsApproved,
                            QuestionCode = request.QuestionCode,
                            Explanation = request.Explanation,
                            ExtraInformation = request.ExtraInformation,
                            IsActive = request.IsActive,
                            IsConfigure = request.IsConfigure,
                            CategoryId = request.CategoryId
                        };

                        int rowsAffected = _connection.Execute(query, parameters);
                        var insertedQuestionCode = request.QuestionCode;
                        if (!string.IsNullOrEmpty(insertedQuestionCode))
                        {
                            // Handle QIDCourses mapping
                            var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);

                            var answer = await AnswerHandling(request.QuestionTypeId, request.AnswerMultipleChoiceCategories, request.QuestionId, insertedQuestionCode, request.Answersingleanswercategories);

                            if (data > 0 && answer.Data > 0)
                            {
                                return new ServiceResponse<string>(true, "Operation Successful", "Question updated Successfully", 200);
                            }
                            else
                            {
                                return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                            }
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Some error occurred", string.Empty, 500);
                        }
                    }
                }
                else if (request.QuestionCode != null && request.QuestionCode != "string" && GetRoleName != "AD")
                {
                    return new ServiceResponse<string>(true, "Operation Successful", string.Empty, 200);
                }
                else if (GetRoleName == "AD")
                {
                    bool isLive = await _connection.QueryFirstOrDefaultAsync<bool>(@"select IsLive from tblQuestion where QuestionCode = @QuestionCode", new { QuestionCode = request.QuestionCode });
                    if (isLive)
                    {
                        string query = @"
                    UPDATE tblQuestion
                    SET 
                        QuestionDescription = @QuestionDescription,
                        QuestionTypeId = @QuestionTypeId,
                        Status = @Status,
                        CreatedBy = @CreatedBy,
                        CreatedOn = @CreatedOn,
                        ModifiedBy = @ModifiedBy,
                        ModifiedOn = @ModifiedOn,
                        subjectID = @SubjectID,
                        EmployeeId = @EmployeeId,
                        ModifierId = @ModifierId,
                        IndexTypeId = @IndexTypeId,
                        ContentIndexId = @ContentIndexId,
                        QuestionCode = @QuestionCode,
                        Explanation = @Explanation,
                        ExtraInformation = @ExtraInformation,
                        IsConfigure = @IsConfigure,
                        CategoryId = @CategoryId
                    WHERE 
                        QuestionCode = @QuestionCode and IsLIve = 1";
                        var parameters = new
                        {
                            QuestionId = request.QuestionId,
                            QuestionDescription = request.QuestionDescription,
                            QuestionTypeId = request.QuestionTypeId,
                            Status = request.Status,
                            CreatedBy = request.CreatedBy,
                            CreatedOn = request.CreatedOn,
                            ModifiedBy = request.ModifiedBy,
                            ModifiedOn = request.ModifiedOn,
                            SubjectID = request.subjectID,
                            EmployeeId = request.EmployeeId,
                            ModifierId = request.ModifierId,
                            IndexTypeId = request.IndexTypeId,
                            ContentIndexId = request.ContentIndexId,
                            QuestionCode = request.QuestionCode,
                            Explanation = request.Explanation,
                            ExtraInformation = request.ExtraInformation,
                            IsActive = request.IsActive,
                            IsConfigure = request.IsConfigure,
                            CategoryId = request.CategoryId
                        };

                        int rowsAffected = _connection.Execute(query, parameters);
                        var insertedQuestionCode = request.QuestionCode;
                        if (!string.IsNullOrEmpty(insertedQuestionCode))
                        {
                            // Handle QIDCourses mapping
                            var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);

                            var answer = await AnswerHandling(request.QuestionTypeId, request.AnswerMultipleChoiceCategories, request.QuestionId, insertedQuestionCode, request.Answersingleanswercategories);

                            if (data > 0 && answer.Data > 0)
                            {
                                return new ServiceResponse<string>(true, "Operation Successful", "Question updated Successfully", 200);
                            }
                            else
                            {
                                return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                            }
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Some error occurred", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Cannot modify question", string.Empty, 500);
                    }
                }
                else
                {
                    // Prepare new question entry
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
                        IsRejected = false,
                        QuestionCode = request.QuestionCode,
                        Explanation = request.Explanation,
                        ExtraInformation = request.ExtraInformation,
                        IsActive = true,
                        IsConfigure = true,
                        CategoryId = request.CategoryId
                    };
                    string insertQuery = @"
              INSERT INTO tblQuestion (
                  QuestionDescription,
                  QuestionTypeId,
                  Status,
                  CreatedBy,
                  CreatedOn,
                  subjectID,
                  EmployeeId,
                  IndexTypeId,
                  ContentIndexId,
                  IsRejected,
                  IsApproved,
                  QuestionCode,
                  Explanation,
                  ExtraInformation,
                  IsActive,
                  IsConfigure,
                  CategoryId
              ) VALUES (
                  @QuestionDescription,
                  @QuestionTypeId,
                  @Status,
                  @CreatedBy,
                  @CreatedOn,
                  @subjectID,
                  @EmployeeId,
                  @IndexTypeId,
                  @ContentIndexId,
                  @IsRejected,
                  @IsApproved,
                  @QuestionCode,
                  @Explanation,
                  @ExtraInformation,
                  @IsActive, @IsConfigure, @CategoryId
              );

              -- Fetch the QuestionId of the newly inserted row
              SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    string deactivateQuery = @"UPDATE tblQuestion SET IsActive = 0 WHERE QuestionCode = @QuestionCode AND IsActive = 1";
                    await _connection.ExecuteAsync(deactivateQuery, new { request.QuestionCode });
                    // Retrieve the QuestionCode after insertion
                    // var insertedQuestionCode = await _connection.QuerySingleOrDefaultAsync<string>(insertQuery, question);
                    var insertedQuestionId = await _connection.QuerySingleOrDefaultAsync<int>(insertQuery, question);
                    string code = string.Empty;
                    if (string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string")
                    {
                        code = GenerateQuestionCode(request.IndexTypeId, request.ContentIndexId, insertedQuestionId);

                        string questionCodeQuery = @"
                UPDATE tblQuestion
                SET QuestionCode = @QuestionCode
                WHERE QuestionId = @QuestionId AND IsActive = 1";

                        await _connection.ExecuteAsync(questionCodeQuery, new { QuestionCode = code, QuestionId = insertedQuestionId });
                    }
                    string insertedQuestionCode = string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string" ? code : request.QuestionCode;

                    if (!string.IsNullOrEmpty(insertedQuestionCode))
                    {
                        // Handle QIDCourses mapping
                        foreach (var record in request.QIDCourses)
                        {
                            record.QIDCourseID = 0;
                        }
                        var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);

                        var answer = await AnswerHandling(request.QuestionTypeId, request.AnswerMultipleChoiceCategories, insertedQuestionId, insertedQuestionCode, request.Answersingleanswercategories);

                        if (data > 0 && answer.Data > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Question Added Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Some error occurred", string.Empty, 500);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        private async Task<ServiceResponse<int>> AnswerHandling(
      int QuestionTypeId,
      List<AnswerMultipleChoiceCategory>? multiAnswerRequest,
      int QuestionId,
      string QuestionCode,
      Answersingleanswercategory singleAnswerRequest)
        {
            string getQuesType = @"SELECT * FROM tblQBQuestionType WHERE QuestionTypeID = @QuestionTypeID;";
            var questTypedata = await _connection.QueryFirstOrDefaultAsync<QuestionTypes>(getQuesType, new { QuestionTypeID = QuestionTypeId });

            int answer = 0;
            int Answerid = 0;

            // Check if the answer already exists in AnswerMaster
            string getAnswerQuery = @"SELECT Answerid FROM tblAnswerMaster WHERE QuestionCode = @QuestionCode AND QuestionId = @QuestionId;";
            Answerid = await _connection.QueryFirstOrDefaultAsync<int>(getAnswerQuery, new { QuestionCode, QuestionId });

            if (Answerid == 0) // If no entry exists, insert a new one
            {
                string insertAnswerQuery = @"
            INSERT INTO [tblAnswerMaster] (Questionid, QuestionTypeid, QuestionCode)
            VALUES (@Questionid, @QuestionTypeid, @QuestionCode);
            SELECT CAST(SCOPE_IDENTITY() as int);";

                Answerid = await _connection.QuerySingleAsync<int>(insertAnswerQuery, new
                {
                    Questionid = QuestionId,
                    QuestionTypeid = questTypedata?.QuestionTypeID,
                    QuestionCode
                });
            }

            if (questTypedata != null)
            {
                if (questTypedata.Code.Trim() == "MCQ" || questTypedata.Code.Trim() == "TF" || questTypedata.Code.Trim() == "MF" ||
                    questTypedata.Code.Trim() == "MAQ" || questTypedata.Code.Trim() == "MF2" || questTypedata.Code.Trim() == "AR" || questTypedata.Code.Trim() == "FB"
                    || questTypedata.Code.Trim() == "NT" || questTypedata.Code.Trim() == "T/F")
                {
                    if (multiAnswerRequest != null)
                    {
                        foreach (var item in multiAnswerRequest)
                        {
                            item.Answerid = Answerid;

                            // Check if the answer already exists
                            string checkExistingMCQQuery = @"
                        SELECT COUNT(*) FROM tblAnswerMultipleChoiceCategory
                        WHERE Answerid = @Answerid;";

                            if (item.Answermultiplechoicecategoryid > 0)
                            {
                                // Update existing record
                                string updateMCQQuery = @"
                            UPDATE tblAnswerMultipleChoiceCategory
                            SET Iscorrect = @Iscorrect, Matchid = @Matchid, Answer = @Answer
                            WHERE Answerid = @Answerid and Answermultiplechoicecategoryid = @Answermultiplechoicecategoryid;";

                                await _connection.ExecuteAsync(updateMCQQuery, item);
                            }
                            else
                            {
                                // Insert new record
                                string insertMCQQuery = @"
                            INSERT INTO tblAnswerMultipleChoiceCategory
                            (Answerid, Answer, Iscorrect, Matchid)
                            VALUES (@Answerid, @Answer, @Iscorrect, @Matchid);";

                                await _connection.ExecuteAsync(insertMCQQuery, item);
                            }
                        }
                        answer = multiAnswerRequest.Count; // Return the count of processed answers
                    }
                }
                else
                {
                    if (singleAnswerRequest != null)
                    {
                        singleAnswerRequest.Answerid = Answerid;

                        if (singleAnswerRequest.Answersingleanswercategoryid > 0)
                        {
                            // Update existing record
                            string updateSingleQuery = @"
                        UPDATE tblAnswersingleanswercategory
                        SET Answer = @Answer
                        WHERE Answerid = @Answerid and Answersingleanswercategoryid = @Answersingleanswercategoryid;";

                            await _connection.ExecuteAsync(updateSingleQuery, singleAnswerRequest);
                        }
                        else
                        {
                            // Insert new record
                            string insertSingleQuery = @"
                        INSERT INTO tblAnswersingleanswercategory (Answerid, Answer)
                        VALUES (@Answerid, @Answer);";

                            await _connection.ExecuteAsync(insertSingleQuery, singleAnswerRequest);
                        }
                        answer = 1; // Single answer processed
                    }
                }
            }
            return new ServiceResponse<int>(true, string.Empty, answer, 200);
        }
        public async Task<ServiceResponse<int>> UpdateQIDCourseAsync(int qidCourseId, UpdateQIDCourseRequest request)
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            try
            {
                // Update query for tblQIDCourse
                string updateQuery = @"
            UPDATE tblQIDCourse
            SET QID = @QID,
                CourseID = @CourseID,
                LevelId = @LevelId,
                Status = @Status,
                ModifiedBy = @ModifiedBy,
                ModifiedDate = GETDATE(),
                QuestionCode = @QuestionCode
            WHERE QIDCourseID = @QIDCourseID";

                var rowsAffected = await _connection.ExecuteAsync(updateQuery, new
                {
                    QID = request.QID,
                    CourseID = request.CourseID,
                    LevelId = request.LevelId,
                    Status = request.Status,
                    ModifiedBy = request.ModifiedBy,
                    QuestionCode = request.QuestionCode,
                    QIDCourseID = qidCourseId
                });

                if (rowsAffected > 0)
                {
                    return new ServiceResponse<int>(true, "Record updated successfully.", qidCourseId, 200);
                }

                return new ServiceResponse<int>(false, "No record found to update.", 0, 404);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<int>(false, ex.Message, 0, 500);
            }
        }
        public async Task<ServiceResponse<string>> AddUpdateComprehensiveQuestion(ComprehensiveQuestionRequest request)
        {
            try
            {
                string roleQuery = @"select r.RoleCode from  [tblEmployee] e 
                                                 LEFT JOIN [tblRole] r ON e.RoleID = r.RoleID
                                                 WHERE e.Employeeid = @EmployeeId";
                string GetRoleName = await _connection.QueryFirstOrDefaultAsync<string>(roleQuery, new { EmployeeId = request.ModifierId });

                if (request.EmployeeId != request.ModifierId && request.QuestionCode != "string" && request.QuestionCode != null && GetRoleName != "AD")
                {
                    int QuestionModifier = await _connection.QueryFirstOrDefaultAsync<int>(@"select ModifierId from tblQuestion where QuestionCode = @QuestionCode
                     and IsActive = 1", new { QuestionCode = request.QuestionCode });
                    if (QuestionModifier == request.ModifierId)
                    {
                        string query = @"UPDATE tblQuestion
                        SET 
                            Paragraph = @Paragraph,
                            QuestionTypeId = @QuestionTypeId,
                            Status = @Status,
                            CategoryId = @CategoryId,
                            CreatedBy = @CreatedBy,
                            CreatedOn = @CreatedOn,
                            SubjectID = @SubjectID,
                            EmployeeId = @EmployeeId,
                            ModifierId = @ModifierId,
                            IndexTypeId = @IndexTypeId,
                            ContentIndexId = @ContentIndexId,
                            IsRejected = @IsRejected,
                            IsApproved = @IsApproved,
                            Explanation = @Explanation,
                            ExtraInformation = @ExtraInformation,
                            IsConfigure = @IsConfigure,
                            QuestionDescription = @QuestionDescription
                        WHERE QuestionCode = @QuestionCode";
                        var parameters = new
                        {
                            // QuestionId = request.QuestionId,
                            Paragraph = request.Paragraph,
                            QuestionTypeId = request.QuestionTypeId,
                            Status = request.Status,
                            CreatedBy = request.CreatedBy,
                            CreatedOn = request.CreatedOn,
                            ModifiedBy = request.ModifiedBy,
                            ModifiedOn = request.ModifiedOn,
                            SubjectID = request.subjectID,
                            EmployeeId = request.EmployeeId,
                            ModifierId = request.ModifierId,
                            IndexTypeId = request.IndexTypeId,
                            ContentIndexId = request.ContentIndexId,
                            IsRejected = request.IsRejected,
                            IsApproved = request.IsApproved,
                            QuestionCode = request.QuestionCode,
                            Explanation = request.Explanation,
                            ExtraInformation = request.ExtraInformation,
                            IsActive = request.IsActive,
                            IsConfigure = request.IsConfigure,
                            CategoryId = request.CategoryId,
                            QuestionDescription = "string"
                        };
                        int rowsAffected = _connection.Execute(query, parameters);
                        var insertedQuestionCode = request.QuestionCode;
                        var insertedQuestionId = request.QuestionId;
                        if (!string.IsNullOrEmpty(insertedQuestionCode))
                        {
                            // Handle QIDCourses mapping
                            var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);
                            foreach (var record in request.Questions)
                            {
                                var newQuestion = new
                                {
                                    QuestionDescription = record.QuestionDescription,
                                    QuestionTypeId = record.QuestionTypeId,
                                    Status = record.Status,
                                    CreatedBy = record.CreatedBy,
                                    CreatedOn = record.CreatedOn,
                                    subjectID = request.subjectID,
                                    IndexTypeId = request.IndexTypeId,
                                    ContentIndexId = request.ContentIndexId,
                                    IsRejected = request.IsRejected,
                                    IsApproved = request.IsApproved,
                                    QuestionCode = record.QuestionCode,
                                    Explanation = record.Explanation,
                                    ExtraInformation = record.ExtraInformation,
                                    IsActive = request.IsActive,
                                    IsConfigure = request.IsConfigure,
                                    CategoryId = request.CategoryId,
                                    ParentQId = insertedQuestionId,
                                    ParentQCode = insertedQuestionCode
                                };
                                string updateQuery1 = @"UPDATE tblQuestion
                                SET 
                                    QuestionDescription = @QuestionDescription,
                                    QuestionTypeId = @QuestionTypeId,
                                    Status = @Status,
                                    CreatedBy = @CreatedBy,
                                    CreatedOn = @CreatedOn,
                                    SubjectID = @SubjectID,
                                    IndexTypeId = @IndexTypeId,
                                    ContentIndexId = @ContentIndexId,
                                    IsRejected = @IsRejected,
                                    IsApproved = @IsApproved,
                                    Explanation = @Explanation,
                                    ExtraInformation = @ExtraInformation,
                                    IsConfigure = @IsConfigure,
                                    CategoryId = @CategoryId,
                                    ParentQId = @ParentQId,
                                    ParentQCode = @ParentQCode
                                WHERE QuestionCode = @QuestionCode";
                                var updatedQuestionId1 = await _connection.QuerySingleOrDefaultAsync<int>(updateQuery1, newQuestion);
                                var answer = await AnswerHandling(record.QuestionTypeId, record.AnswerMultipleChoiceCategories, record.QuestionId, record.QuestionCode, record.Answersingleanswercategories);
                            }
                            if (data > 0)
                            {
                                return new ServiceResponse<string>(true, "Operation Successful", "Question Updated Successfully", 200);
                            }
                            else
                            {
                                return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                            }
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Some error occurred", string.Empty, 500);
                        }
                    }
                    else
                    {
                        int count = await _connection.QueryFirstOrDefaultAsync<int>(@"select count(*) from tblQuestionProfilerRejections where QuestionCode = @QuestionCode", new { QuestionCode = request.QuestionCode });
                        bool isRejectedQuestion = await _connection.QueryFirstOrDefaultAsync<bool>(@"select IsRejected from tblQuestion where QuestionCode = @QuestionCode and IsActive = 1", new { QuestionCode = request.QuestionCode });
                        bool isRejected = false;
                        if (count == 0 || !isRejectedQuestion)
                        {
                            isRejected = false;
                        }
                        else
                        {
                            isRejected = true;
                        }
                        request.IsRejected = isRejected;
                        string query = @"
        INSERT INTO tblQuestion 
        (Paragraph, QuestionTypeId, Status, CategoryId, CreatedBy, CreatedOn, SubjectID, EmployeeId, ModifierId, 
         IndexTypeId, ContentIndexId, IsRejected, IsApproved, QuestionCode, Explanation, ExtraInformation, IsActive, IsConfigure, QuestionDescription)
        VALUES 
        (@Paragraph, @QuestionTypeId, @Status, @CategoryId, @CreatedBy, @CreatedOn, @SubjectID, @EmployeeId, @ModifierId, 
         @IndexTypeId, @ContentIndexId, @IsRejected, @IsApproved, @QuestionCode, @Explanation, @ExtraInformation, @IsActive, @IsConfigure, 'string');
        SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        // Execute the insert query and return the generated QuestionId

                        string deactivateQuery = @"UPDATE tblQuestion SET IsActive = 0 WHERE QuestionCode = @QuestionCode AND IsActive = 1";
                        await _connection.ExecuteAsync(deactivateQuery, new { request.QuestionCode });
                        // Retrieve the QuestionCode after insertion

                        var insertedQuestionId = await _connection.QuerySingleOrDefaultAsync<int>(query, request);
                        string code = string.Empty;
                        if (string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string")
                        {
                            code = GenerateQuestionCode(request.IndexTypeId, request.ContentIndexId, insertedQuestionId);

                            string questionCodeQuery = @"
                UPDATE tblQuestion
                SET QuestionCode = @QuestionCode
                WHERE QuestionId = @QuestionId AND IsActive = 1";

                            await _connection.ExecuteAsync(questionCodeQuery, new { QuestionCode = code, QuestionId = insertedQuestionId });
                        }
                        string insertedQuestionCode = string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string" ? code : request.QuestionCode;
                        if (!string.IsNullOrEmpty(insertedQuestionCode))
                        {
                            // Handle QIDCourses mapping
                            // Handle QIDCourses mapping
                            foreach (var record in request.QIDCourses)
                            {
                                record.QIDCourseID = 0;
                            }
                            var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);
                            foreach (var record in request.Questions)
                            {
                                var newQuestion = new
                                {
                                    QuestionDescription = record.QuestionDescription,
                                    QuestionTypeId = record.QuestionTypeId,
                                    Status = record.Status,
                                    CreatedBy = record.CreatedBy,
                                    CreatedOn = record.CreatedOn,
                                    subjectID = request.subjectID,
                                    IndexTypeId = request.IndexTypeId,
                                    ContentIndexId = request.ContentIndexId,
                                    IsRejected = request.IsRejected,
                                    IsApproved = request.IsApproved,
                                    QuestionCode = record.QuestionCode,
                                    Explanation = record.Explanation,
                                    ExtraInformation = record.ExtraInformation,
                                    IsActive = request.IsActive,
                                    IsConfigure = request.IsConfigure,
                                    CategoryId = request.CategoryId,
                                    ParentQId = insertedQuestionId,
                                    ParentQCode = insertedQuestionCode
                                };
                                string insertQuery1 = @"
              INSERT INTO tblQuestion (
                  QuestionDescription,
                  QuestionTypeId,
                  Status,
                  CreatedBy,
                  CreatedOn,
                  subjectID,
                  IndexTypeId,
                  ContentIndexId,
                  IsRejected,
                  IsApproved,
                  QuestionCode,
                  Explanation,
                  ExtraInformation,
                  IsActive,
                  IsConfigure,
                  CategoryId,
                  ParentQId, ParentQCode
              ) VALUES (
                  @QuestionDescription,
                  @QuestionTypeId,
                  @Status,
                  @CreatedBy,
                  @CreatedOn,
                  @subjectID,
                  @IndexTypeId,
                  @ContentIndexId,
                  @IsRejected,
                  @IsApproved,
                  @QuestionCode,
                  @Explanation,
                  @ExtraInformation,
                  @IsActive, @IsConfigure, @CategoryId, @ParentQId, @ParentQCode
              );

              -- Fetch the QuestionId of the newly inserted row
              SELECT CAST(SCOPE_IDENTITY() AS INT);";
                                string deactivateQuery1 = @"UPDATE tblQuestion SET IsActive = 0 WHERE QuestionCode = @QuestionCode AND IsActive = 1";
                                await _connection.ExecuteAsync(deactivateQuery1, new { record.QuestionCode });
                                // Retrieve the QuestionCode after insertion
                                // var insertedQuestionCode = await _connection.QuerySingleOrDefaultAsync<string>(insertQuery, question);
                                var insertedQuestionId1 = await _connection.QuerySingleOrDefaultAsync<int>(insertQuery1, newQuestion);
                                string code1 = string.Empty;
                                if (string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string")
                                {
                                    code = GenerateQuestionCode(request.IndexTypeId, request.ContentIndexId, insertedQuestionId1);

                                    string questionCodeQuery = @"
                            UPDATE tblQuestion
                            SET QuestionCode = @QuestionCode
                            WHERE QuestionId = @QuestionId AND IsActive = 1";

                                    await _connection.ExecuteAsync(questionCodeQuery, new { QuestionCode = code, QuestionId = insertedQuestionId1 });
                                }
                                string insertedQuestionCode1 = string.IsNullOrEmpty(record.QuestionCode) || record.QuestionCode == "string" ? code : record.QuestionCode;

                                if (!string.IsNullOrEmpty(insertedQuestionCode1))
                                {
                                    if (record.AnswerMultipleChoiceCategories != null)
                                    {
                                        foreach (var detail in record.AnswerMultipleChoiceCategories)
                                        {
                                            detail.Answermultiplechoicecategoryid = 0;
                                        }
                                    }
                                    if (record.Answersingleanswercategories != null)
                                    {
                                        record.Answersingleanswercategories.Answersingleanswercategoryid = 0;
                                    }
                                    var answer = await AnswerHandling(record.QuestionTypeId, record.AnswerMultipleChoiceCategories, insertedQuestionId1, insertedQuestionCode1, record.Answersingleanswercategories);
                                }
                                else
                                {
                                    return new ServiceResponse<string>(false, "Some error occurred", string.Empty, 500);
                                }
                            }
                            if (data > 0)
                            {
                                return new ServiceResponse<string>(true, "Operation Successful", "Question Added Successfully", 200);
                            }
                            else
                            {
                                return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                            }
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Some error occurred", string.Empty, 500);
                        }
                    }
                }
                else if (request.EmployeeId == request.ModifierId && request.ModifierId > 0)
                {
                    var count1 = _connection.QueryFirstOrDefault<int>(@"select * from tblQuestionProfiler where QuestionCode = @QuestionCode "
                              , new { QuestionCode = request.QuestionCode, EmpId = request.ModifierId });
                    if (count1 > 0)
                    {
                        int count = await _connection.QueryFirstOrDefaultAsync<int>(@"select count(*) from tblQuestionProfilerRejections where QuestionCode = @QuestionCode", new { QuestionCode = request.QuestionCode });
                        bool isRejected = false;
                        if (count > 0)
                        {
                            isRejected = true;
                        }
                        string query = @"
        INSERT INTO tblQuestion 
        (Paragraph, QuestionTypeId, Status, CategoryId, CreatedBy, CreatedOn, SubjectID, EmployeeId, ModifierId, 
         IndexTypeId, ContentIndexId, IsRejected, IsApproved, QuestionCode, Explanation, ExtraInformation, IsActive, IsConfigure, QuestionDescription)
        VALUES 
        (@Paragraph, @QuestionTypeId, @Status, @CategoryId, @CreatedBy, @CreatedOn, @SubjectID, @EmployeeId, @ModifierId, 
         @IndexTypeId, @ContentIndexId, @IsRejected, @IsApproved, @QuestionCode, @Explanation, @ExtraInformation, @IsActive, @IsConfigure, 'string');
        SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        // Execute the insert query and return the generated QuestionId
                        request.IsRejected = isRejected;
                        string deactivateQuery = @"UPDATE tblQuestion SET IsActive = 0 WHERE QuestionCode = @QuestionCode AND IsActive = 1";
                        await _connection.ExecuteAsync(deactivateQuery, new { request.QuestionCode });
                        // Retrieve the QuestionCode after insertion

                        var insertedQuestionId = await _connection.QuerySingleOrDefaultAsync<int>(query, request);
                        string code = string.Empty;
                        if (string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string")
                        {
                            code = GenerateQuestionCode(request.IndexTypeId, request.ContentIndexId, insertedQuestionId);

                            string questionCodeQuery = @"
                UPDATE tblQuestion
                SET QuestionCode = @QuestionCode
                WHERE QuestionId = @QuestionId AND IsActive = 1";

                            await _connection.ExecuteAsync(questionCodeQuery, new { QuestionCode = code, QuestionId = insertedQuestionId });
                        }
                        string insertedQuestionCode = string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string" ? code : request.QuestionCode;

                        if (!string.IsNullOrEmpty(insertedQuestionCode))
                        {
                            // Handle QIDCourses mapping
                            foreach (var record in request.QIDCourses)
                            {
                                record.QIDCourseID = 0;
                            }
                            var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);
                            foreach (var record in request.Questions)
                            {

                                string insertQuery1 = @"
              INSERT INTO tblQuestion (
                  QuestionDescription,
                  QuestionTypeId,
                  Status,
                  CreatedBy,
                  CreatedOn,
                  subjectID,
                  IndexTypeId,
                  ContentIndexId,
                  IsRejected,
                  IsApproved,
                  QuestionCode,
                  Explanation,
                  ExtraInformation,
                  IsActive,
                  IsConfigure,
                  CategoryId,
                  ParentQId, ParentQCode
              ) VALUES (
                  @QuestionDescription,
                  @QuestionTypeId,
                  @Status,
                  @CreatedBy,
                  @CreatedOn,
                  @subjectID,
                  @IndexTypeId,
                  @ContentIndexId,
                  @IsRejected,
                  @IsApproved,
                  @QuestionCode,
                  @Explanation,
                  @ExtraInformation,
                  @IsActive, @IsConfigure, @CategoryId, @ParentQId, @ParentQCode
              );

              -- Fetch the QuestionId of the newly inserted row
              SELECT CAST(SCOPE_IDENTITY() AS INT);";
                                var newQuestion = new
                                {
                                    QuestionDescription = record.QuestionDescription,
                                    QuestionTypeId = record.QuestionTypeId,
                                    Status = record.Status,
                                    CreatedBy = record.CreatedBy,
                                    CreatedOn = record.CreatedOn,
                                    subjectID = request.subjectID,
                                    IndexTypeId = request.IndexTypeId,
                                    ContentIndexId = request.ContentIndexId,
                                    IsRejected = request.IsRejected,
                                    IsApproved = request.IsApproved,
                                    QuestionCode = record.QuestionCode,
                                    Explanation = record.Explanation,
                                    ExtraInformation = record.ExtraInformation,
                                    IsActive = request.IsActive,
                                    IsConfigure = request.IsConfigure,
                                    CategoryId = request.CategoryId,
                                    ParentQId = insertedQuestionId,
                                    ParentQCode = insertedQuestionCode
                                };
                                string deactivateQuery1 = @"UPDATE tblQuestion SET IsActive = 0 WHERE QuestionCode = @QuestionCode AND IsActive = 1";
                                await _connection.ExecuteAsync(deactivateQuery1, new { record.QuestionCode });
                                // Retrieve the QuestionCode after insertion
                                // var insertedQuestionCode = await _connection.QuerySingleOrDefaultAsync<string>(insertQuery, question);
                                var insertedQuestionId1 = await _connection.QuerySingleOrDefaultAsync<int>(insertQuery1, newQuestion);
                                string code1 = string.Empty;
                                if (string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string")
                                {
                                    code = GenerateQuestionCode(request.IndexTypeId, request.ContentIndexId, insertedQuestionId1);

                                    string questionCodeQuery = @"
                            UPDATE tblQuestion
                            SET QuestionCode = @QuestionCode
                            WHERE QuestionId = @QuestionId AND IsActive = 1";

                                    await _connection.ExecuteAsync(questionCodeQuery, new { QuestionCode = code, QuestionId = insertedQuestionId1 });
                                }
                                string insertedQuestionCode1 = string.IsNullOrEmpty(record.QuestionCode) || record.QuestionCode == "string" ? code : record.QuestionCode;

                                if (!string.IsNullOrEmpty(insertedQuestionCode1))
                                {
                                    if (record.AnswerMultipleChoiceCategories != null)
                                    {
                                        foreach (var detail in record.AnswerMultipleChoiceCategories)
                                        {
                                            detail.Answermultiplechoicecategoryid = 0;
                                        }
                                    }
                                    if (record.Answersingleanswercategories != null)
                                    {
                                        record.Answersingleanswercategories.Answersingleanswercategoryid = 0;
                                    }
                                    var answer = await AnswerHandling(record.QuestionTypeId, record.AnswerMultipleChoiceCategories, insertedQuestionId1, insertedQuestionCode1, record.Answersingleanswercategories);
                                }
                                else
                                {
                                    return new ServiceResponse<string>(false, "Some error occurred", string.Empty, 500);
                                }
                            }
                            if (data > 0)
                            {
                                return new ServiceResponse<string>(true, "Operation Successful", "Question Added Successfully", 200);
                            }
                            else
                            {
                                return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                            }
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Some error occurred", string.Empty, 500);
                        }
                    }
                    else
                    {
                        string query = @"UPDATE tblQuestion
                        SET 
                            Paragraph = @Paragraph,
                            QuestionTypeId = @QuestionTypeId,
                            Status = @Status,
                            CategoryId = @CategoryId,
                            CreatedBy = @CreatedBy,
                            CreatedOn = @CreatedOn,
                            SubjectID = @SubjectID,
                            EmployeeId = @EmployeeId,
                            ModifierId = @ModifierId,
                            IndexTypeId = @IndexTypeId,
                            ContentIndexId = @ContentIndexId,
                            IsRejected = @IsRejected,
                            IsApproved = @IsApproved,
                            Explanation = @Explanation,
                            ExtraInformation = @ExtraInformation,
                            IsConfigure = @IsConfigure,
                            QuestionDescription = @QuestionDescription
                        WHERE QuestionCode = @QuestionCode";
                        var parameters = new
                        {
                            // QuestionId = request.QuestionId,
                            Paragraph = request.Paragraph,
                            QuestionTypeId = request.QuestionTypeId,
                            Status = request.Status,
                            CreatedBy = request.CreatedBy,
                            CreatedOn = request.CreatedOn,
                            ModifiedBy = request.ModifiedBy,
                            ModifiedOn = request.ModifiedOn,
                            SubjectID = request.subjectID,
                            EmployeeId = request.EmployeeId,
                            ModifierId = request.ModifierId,
                            IndexTypeId = request.IndexTypeId,
                            ContentIndexId = request.ContentIndexId,
                            IsRejected = request.IsRejected,
                            IsApproved = request.IsApproved,
                            QuestionCode = request.QuestionCode,
                            Explanation = request.Explanation,
                            ExtraInformation = request.ExtraInformation,
                            IsActive = request.IsActive,
                            IsConfigure = request.IsConfigure,
                            CategoryId = request.CategoryId,
                            QuestionDescription = "string"
                        };
                        int rowsAffected = _connection.Execute(query, parameters);
                        var insertedQuestionCode = request.QuestionCode;
                        var insertedQuestionId = request.QuestionId;
                        if (!string.IsNullOrEmpty(insertedQuestionCode))
                        {
                            // Handle QIDCourses mapping
                            var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);
                            foreach (var record in request.Questions)
                            {
                                var newQuestion = new
                                {
                                    QuestionDescription = record.QuestionDescription,
                                    QuestionTypeId = record.QuestionTypeId,
                                    Status = record.Status,
                                    CreatedBy = record.CreatedBy,
                                    CreatedOn = record.CreatedOn,
                                    subjectID = request.subjectID,
                                    IndexTypeId = request.IndexTypeId,
                                    ContentIndexId = request.ContentIndexId,
                                    IsRejected = request.IsRejected,
                                    IsApproved = request.IsApproved,
                                    QuestionCode = record.QuestionCode,
                                    Explanation = record.Explanation,
                                    ExtraInformation = record.ExtraInformation,
                                    IsActive = request.IsActive,
                                    IsConfigure = request.IsConfigure,
                                    CategoryId = request.CategoryId,
                                    ParentQId = insertedQuestionId,
                                    ParentQCode = insertedQuestionCode
                                };
                                string updateQuery1 = @"UPDATE tblQuestion
                                SET 
                                    QuestionDescription = @QuestionDescription,
                                    QuestionTypeId = @QuestionTypeId,
                                    Status = @Status,
                                    CreatedBy = @CreatedBy,
                                    CreatedOn = @CreatedOn,
                                    SubjectID = @SubjectID,
                                    IndexTypeId = @IndexTypeId,
                                    ContentIndexId = @ContentIndexId,
                                    IsRejected = @IsRejected,
                                    IsApproved = @IsApproved,
                                    Explanation = @Explanation,
                                    ExtraInformation = @ExtraInformation,
                                    IsConfigure = @IsConfigure,
                                    CategoryId = @CategoryId,
                                    ParentQId = @ParentQId,
                                    ParentQCode = @ParentQCode
                                WHERE QuestionCode = @QuestionCode";
                                var updatedQuestionId1 = await _connection.QuerySingleOrDefaultAsync<int>(updateQuery1, newQuestion);
                                var answer = await AnswerHandling(record.QuestionTypeId, record.AnswerMultipleChoiceCategories, record.QuestionId, record.QuestionCode, record.Answersingleanswercategories);
                            }
                            if (data > 0)
                            {
                                return new ServiceResponse<string>(true, "Operation Successful", "Question Updated Successfully", 200);
                            }
                            else
                            {
                                return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                            }
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Some error occurred", string.Empty, 500);
                        }
                    }
                }
                else if (request.QuestionCode != null && request.QuestionCode != "string" && GetRoleName != "AD")
                {
                    return new ServiceResponse<string>(true, "Operation Successful", string.Empty, 200);
                }
                else if (GetRoleName == "AD")
                {
                    bool isLive = await _connection.QueryFirstOrDefaultAsync<bool>(@"select IsLive from tblQuestion where QuestionCode = @QuestionCode", new { QuestionCode = request.QuestionCode });
                    if (isLive)
                    {
                        string query = @"UPDATE tblQuestion
                        SET 
                            Paragraph = @Paragraph,
                            QuestionTypeId = @QuestionTypeId,
                            Status = @Status,
                            CategoryId = @CategoryId,
                            CreatedBy = @CreatedBy,
                            CreatedOn = @CreatedOn,
                            SubjectID = @SubjectID,
                            EmployeeId = @EmployeeId,
                            ModifierId = @ModifierId,
                            IndexTypeId = @IndexTypeId,
                            ContentIndexId = @ContentIndexId,
                            IsRejected = @IsRejected,
                            IsApproved = @IsApproved,
                            Explanation = @Explanation,
                            ExtraInformation = @ExtraInformation,
                            IsConfigure = @IsConfigure,
                            QuestionDescription = @QuestionDescription
                        WHERE QuestionCode = @QuestionCode";
                        var parameters = new
                        {
                            // QuestionId = request.QuestionId,
                            Paragraph = request.Paragraph,
                            QuestionTypeId = request.QuestionTypeId,
                            Status = request.Status,
                            CreatedBy = request.CreatedBy,
                            CreatedOn = request.CreatedOn,
                            ModifiedBy = request.ModifiedBy,
                            ModifiedOn = request.ModifiedOn,
                            SubjectID = request.subjectID,
                            EmployeeId = request.EmployeeId,
                            ModifierId = request.ModifierId,
                            IndexTypeId = request.IndexTypeId,
                            ContentIndexId = request.ContentIndexId,
                            IsRejected = request.IsRejected,
                            IsApproved = request.IsApproved,
                            QuestionCode = request.QuestionCode,
                            Explanation = request.Explanation,
                            ExtraInformation = request.ExtraInformation,
                            IsActive = request.IsActive,
                            IsConfigure = request.IsConfigure,
                            CategoryId = request.CategoryId,
                            QuestionDescription = "string"
                        };
                        int rowsAffected = _connection.Execute(query, parameters);
                        var insertedQuestionCode = request.QuestionCode;
                        var insertedQuestionId = request.QuestionId;
                        if (!string.IsNullOrEmpty(insertedQuestionCode))
                        {
                            // Handle QIDCourses mapping
                            var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);
                            foreach (var record in request.Questions)
                            {
                                var newQuestion = new
                                {
                                    QuestionDescription = record.QuestionDescription,
                                    QuestionTypeId = record.QuestionTypeId,
                                    Status = record.Status,
                                    CreatedBy = record.CreatedBy,
                                    CreatedOn = record.CreatedOn,
                                    subjectID = request.subjectID,
                                    IndexTypeId = request.IndexTypeId,
                                    ContentIndexId = request.ContentIndexId,
                                    IsRejected = request.IsRejected,
                                    IsApproved = request.IsApproved,
                                    QuestionCode = record.QuestionCode,
                                    Explanation = record.Explanation,
                                    ExtraInformation = record.ExtraInformation,
                                    IsActive = request.IsActive,
                                    IsConfigure = request.IsConfigure,
                                    CategoryId = request.CategoryId,
                                    ParentQId = insertedQuestionId,
                                    ParentQCode = insertedQuestionCode
                                };
                                string updateQuery1 = @"UPDATE tblQuestion
                                SET 
                                    QuestionDescription = @QuestionDescription,
                                    QuestionTypeId = @QuestionTypeId,
                                    Status = @Status,
                                    CreatedBy = @CreatedBy,
                                    CreatedOn = @CreatedOn,
                                    SubjectID = @SubjectID,
                                    IndexTypeId = @IndexTypeId,
                                    ContentIndexId = @ContentIndexId,
                                    Explanation = @Explanation,
                                    ExtraInformation = @ExtraInformation,
                                    IsConfigure = @IsConfigure,
                                    CategoryId = @CategoryId,
                                    ParentQId = @ParentQId,
                                    ParentQCode = @ParentQCode
                                WHERE QuestionCode = @QuestionCode";
                                var updatedQuestionId1 = await _connection.QuerySingleOrDefaultAsync<int>(updateQuery1, newQuestion);
                                var answer = await AnswerHandling(record.QuestionTypeId, record.AnswerMultipleChoiceCategories, record.QuestionId, record.QuestionCode, record.Answersingleanswercategories);
                            }
                            if (data > 0)
                            {
                                return new ServiceResponse<string>(true, "Operation Successful", "Question Updated Successfully", 200);
                            }
                            else
                            {
                                return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                            }
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Some error occurred", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Cannot modify question", string.Empty, 500);
                    }
                }
                else
                {
                    string query = @"
        INSERT INTO tblQuestion 
        (Paragraph, QuestionTypeId, Status, CategoryId, CreatedBy, CreatedOn, SubjectID, EmployeeId, ModifierId, 
         IndexTypeId, ContentIndexId, IsRejected, IsApproved, QuestionCode, Explanation, ExtraInformation, IsActive, IsConfigure, QuestionDescription)
        VALUES 
        (@Paragraph, @QuestionTypeId, @Status, @CategoryId, @CreatedBy, @CreatedOn, @SubjectID, @EmployeeId, @ModifierId, 
         @IndexTypeId, @ContentIndexId, @IsRejected, @IsApproved, @QuestionCode, @Explanation, @ExtraInformation, @IsActive, @IsConfigure, 'string');
        SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    // Execute the insert query and return the generated QuestionId

                    string deactivateQuery = @"UPDATE tblQuestion SET IsActive = 0 WHERE QuestionCode = @QuestionCode AND IsActive = 1";
                    await _connection.ExecuteAsync(deactivateQuery, new { request.QuestionCode });
                    // Retrieve the QuestionCode after insertion

                    var insertedQuestionId = await _connection.QuerySingleOrDefaultAsync<int>(query, request);
                    string code = string.Empty;
                    if (string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string")
                    {
                        code = GenerateQuestionCode(request.IndexTypeId, request.ContentIndexId, insertedQuestionId);

                        string questionCodeQuery = @"
                UPDATE tblQuestion
                SET QuestionCode = @QuestionCode
                WHERE QuestionId = @QuestionId AND IsActive = 1";

                        await _connection.ExecuteAsync(questionCodeQuery, new { QuestionCode = code, QuestionId = insertedQuestionId });
                    }
                    string insertedQuestionCode = string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string" ? code : request.QuestionCode;

                    if (!string.IsNullOrEmpty(insertedQuestionCode))
                    {
                        // Handle QIDCourses mapping
                        // Handle QIDCourses mapping
                        foreach (var record in request.QIDCourses)
                        {
                            record.QIDCourseID = 0;
                        }
                        var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);
                        foreach (var record in request.Questions)
                        {
                            var newQuestion = new
                            {
                                QuestionDescription = record.QuestionDescription,
                                QuestionTypeId = record.QuestionTypeId,
                                Status = record.Status,
                                CreatedBy = record.CreatedBy,
                                CreatedOn = record.CreatedOn,
                                subjectID = request.subjectID,
                                IndexTypeId = request.IndexTypeId,
                                ContentIndexId = request.ContentIndexId,
                                IsRejected = request.IsRejected,
                                IsApproved = request.IsApproved,
                                QuestionCode = record.QuestionCode,
                                Explanation = record.Explanation,
                                ExtraInformation = record.ExtraInformation,
                                IsActive = request.IsActive,
                                IsConfigure = request.IsConfigure,
                                CategoryId = request.CategoryId,
                                ParentQId = insertedQuestionId,
                                ParentQCode = insertedQuestionCode
                            };
                            string insertQuery1 = @"
              INSERT INTO tblQuestion (
                  QuestionDescription,
                  QuestionTypeId,
                  Status,
                  CreatedBy,
                  CreatedOn,
                  subjectID,
                  IndexTypeId,
                  ContentIndexId,
                  IsRejected,
                  IsApproved,
                  QuestionCode,
                  Explanation,
                  ExtraInformation,
                  IsActive,
                  IsConfigure,
                  CategoryId,
                  ParentQId, ParentQCode
              ) VALUES (
                  @QuestionDescription,
                  @QuestionTypeId,
                  @Status,
                  @CreatedBy,
                  @CreatedOn,
                  @subjectID,
                  @IndexTypeId,
                  @ContentIndexId,
                  @IsRejected,
                  @IsApproved,
                  @QuestionCode,
                  @Explanation,
                  @ExtraInformation,
                  @IsActive, @IsConfigure, @CategoryId, @ParentQId, @ParentQCode
              );

              -- Fetch the QuestionId of the newly inserted row
              SELECT CAST(SCOPE_IDENTITY() AS INT);";
                            string deactivateQuery1 = @"UPDATE tblQuestion SET IsActive = 0 WHERE QuestionCode = @QuestionCode AND IsActive = 1";
                            await _connection.ExecuteAsync(deactivateQuery1, new { record.QuestionCode });
                            // Retrieve the QuestionCode after insertion
                            // var insertedQuestionCode = await _connection.QuerySingleOrDefaultAsync<string>(insertQuery, question);
                            var insertedQuestionId1 = await _connection.QuerySingleOrDefaultAsync<int>(insertQuery1, newQuestion);
                            string code1 = string.Empty;
                            if (string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string")
                            {
                                code = GenerateQuestionCode(request.IndexTypeId, request.ContentIndexId, insertedQuestionId1);

                                string questionCodeQuery = @"
                            UPDATE tblQuestion
                            SET QuestionCode = @QuestionCode
                            WHERE QuestionId = @QuestionId AND IsActive = 1";

                                await _connection.ExecuteAsync(questionCodeQuery, new { QuestionCode = code, QuestionId = insertedQuestionId1 });
                            }
                            string insertedQuestionCode1 = string.IsNullOrEmpty(record.QuestionCode) || record.QuestionCode == "string" ? code : record.QuestionCode;

                            if (!string.IsNullOrEmpty(insertedQuestionCode1))
                            {
                                var answer = await AnswerHandling(record.QuestionTypeId, record.AnswerMultipleChoiceCategories, insertedQuestionId1, insertedQuestionCode1, record.Answersingleanswercategories);
                            }
                            else
                            {
                                return new ServiceResponse<string>(false, "Some error occurred", string.Empty, 500);
                            }
                        }
                        if (data > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Question Added Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Some error occurred", string.Empty, 500);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetAllLiveQuestionsList(int SubjectId)
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
        WHERE q.IsApproved = 1
          AND q.IsRejected = 0
          AND q.IsActive = 1
          AND q.SubjectID = @SubjectId
          AND q.IsLive = 1 AND q.IsConfigure = 1"; // Adjusted to ensure the questions are live

                var parameters = new
                {
                    SubjectId
                };

                var data = await _connection.QueryAsync<dynamic>(sql, parameters);

                if (data != null)
                {
                    var response = data.Select(item =>
                    {
                        if (item.QuestionTypeId == 11)
                        {
                            return new QuestionResponseDTO
                            {
                                QuestionId = item.QuestionId,
                                Paragraph = item.Paragraph,
                                SubjectName = item.SubjectName,
                                EmployeeName = item.EmpFirstName,
                                IndexTypeName = item.IndexTypeName,
                                ContentIndexName = item.ContentIndexName,
                                QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                                ContentIndexId = item.ContentIndexId,
                                CreatedBy = item.CreatedBy,
                                CreatedOn = item.CreatedOn,
                                EmployeeId = item.EmployeeId,
                                IndexTypeId = item.IndexTypeId,
                                subjectID = item.subjectID,
                                ModifiedOn = item.ModifiedOn,
                                QuestionTypeId = item.QuestionTypeId,
                                QuestionTypeName = item.QuestionTypeName,
                                QuestionCode = item.QuestionCode,
                                Explanation = item.Explanation,
                                ExtraInformation = item.ExtraInformation,
                                IsActive = item.IsActive,
                                ComprehensiveChildQuestions = GetChildQuestions(item.QuestionCode)
                            };
                        }
                        else
                        {
                            return new QuestionResponseDTO
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
                                EmployeeName = item.EmpFirstName,
                                IndexTypeId = item.IndexTypeId,
                                IndexTypeName = item.IndexTypeName,
                                ContentIndexId = item.ContentIndexId,
                                ContentIndexName = item.ContentIndexName,
                                IsRejected = item.IsRejected,
                                IsApproved = item.IsApproved,
                                QuestionTypeName = item.QuestionTypeName,
                                QuestionCode = item.QuestionCode,
                                Explanation = item.Explanation,
                                ExtraInformation = item.ExtraInformation,
                                IsActive = item.IsActive,
                                QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                                //QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
                                //Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
                                //AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode)
                                MatchPairs = item.QuestionTypeId == 6 || item.QuestionTypeId == 12 ? GetMatchPairs(item.QuestionCode, item.QuestionId) : null,
                                MatchThePairType2Answers = item.QuestionTypeId == 12 ? GetMatchThePairType2Answers(item.QuestionCode, item.QuestionId) : null,
                                Answersingleanswercategories = (item.QuestionTypeId != 6 && item.QuestionTypeId != 12) ? GetSingleAnswer(item.QuestionCode, item.QuestionId) : null,
                                AnswerMultipleChoiceCategories = (item.QuestionTypeId != 12) ? GetMultipleAnswers(item.QuestionCode) : null
                            };
                        }
                    }
                  ).ToList();
                    //var response = data.Select(item => new QuestionResponseDTO
                    //{
                    //    QuestionId = item.QuestionId,
                    //    QuestionDescription = item.QuestionDescription,
                    //    QuestionTypeId = item.QuestionTypeId,
                    //    Status = item.Status,
                    //    CreatedBy = item.CreatedBy,
                    //    CreatedOn = item.CreatedOn,
                    //    ModifiedBy = item.ModifiedBy,
                    //    ModifiedOn = item.ModifiedOn,
                    //    subjectID = item.subjectID,
                    //    SubjectName = item.SubjectName,
                    //    EmployeeId = item.EmployeeId,
                    //    EmployeeName = item.EmpFirstName,
                    //    IndexTypeId = item.IndexTypeId,
                    //    IndexTypeName = item.IndexTypeName,
                    //    ContentIndexId = item.ContentIndexId,
                    //    ContentIndexName = item.ContentIndexName,
                    //    QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                    //    //QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
                    //    Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
                    //    AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode),
                    //    IsApproved = item.IsApproved,
                    //    IsRejected = item.IsRejected,
                    //    QuestionTypeName = item.QuestionTypeName,
                    //    QuestionCode = item.QuestionCode,
                    //    Explanation = item.Explanation,
                    //    ExtraInformation = item.ExtraInformation,
                    //    IsActive = item.IsActive
                    //}).ToList();
                    // Adding role information and pagination logic
                    foreach (var record in response)
                    {
                        var employeeRoleId = _connection.QuerySingleOrDefault<int?>("SELECT RoleID FROM tblEmployee WHERE EmployeeID = @EmployeeId", new { EmployeeId = record.EmployeeId });
                        record.userRole = employeeRoleId.HasValue ? GetRoleName(employeeRoleId.Value) : string.Empty;
                    }
                    if (response.Count != 0)
                    {
                        return new ServiceResponse<List<QuestionResponseDTO>>(true, "Operation Successful", response, 200);
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
        public async Task<ServiceResponse<string>> MarkQuestionLive(string questionCode)
        {
            try
            {
                // Check if the question is already marked live
                string checkLiveSql = @"
        SELECT COUNT(*)
        FROM [tblQuestion]
        WHERE QuestionCode = @QuestionCode AND IsApproved = 1 AND IsRejected = 0 AND IsActive = 1 AND IsConfigure = 1 And IsLive = 1";

                var liveexists = await _connection.ExecuteScalarAsync<int>(checkLiveSql, new { QuestionCode = questionCode });

                if (liveexists > 0)
                {
                    return new ServiceResponse<string>(false, "Question already marked as live ", string.Empty, 500);
                }
                // Check if the question is approved and not rejected
                string checkSql = @"
        SELECT COUNT(*)
        FROM [tblQuestion]
        WHERE QuestionCode = @QuestionCode AND IsApproved = 1 AND IsRejected = 0 AND IsActive = 1 AND IsConfigure = 1";

                var exists = await _connection.ExecuteScalarAsync<int>(checkSql, new { QuestionCode = questionCode });

                if (exists > 0)
                {
                    // Update the question to mark it as live
                    string updateSql = @"
            UPDATE [tblQuestion]
            SET IsLive = 1
            WHERE QuestionCode = @QuestionCode";

                    // Ensure ExecuteAsync is called on an IDbConnection instance
                    await _connection.ExecuteAsync(updateSql, new { QuestionCode = questionCode });

                    // Fetch IDs of older versions (IsActive = 0) for the given QuestionCode
                    string fetchOlderVersionsSql = @"
            SELECT QuestionId
            FROM [tblQuestion]
            WHERE QuestionCode = @QuestionCode AND IsActive = 0";

                    var olderVersionIds = (await _connection.QueryAsync<int>(fetchOlderVersionsSql, new { QuestionCode = questionCode })).ToList();

                    foreach (var questionId in olderVersionIds)
                    {
                        // Transfer data from tblQuestion
                        string fetchQuestionSql = @"
                SELECT *
                FROM tblQuestion
                WHERE QuestionId = @QuestionId";

                        var questionData = await _connection.QuerySingleAsync(fetchQuestionSql, new { QuestionId = questionId });

                        string transferQuestionSql = @"
                INSERT INTO tbl_QuestionVersion
                (
                    QuestionId, QuestionDescription, QuestionImage, DifficultyLevelId, 
                    QuestionTypeId, Status, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn,
                    SubjectID, IndexTypeId, ContentIndexId, EmployeeId, ExamTypeId,
                    IsRejected, IsApproved, QuestionCode, Explanation, ExtraInformation,
                    IsActive, IsLive, IsConfigure, ModifierId, CategoryId, Paragraph, 
                    ParentQId, ParentQCode
                )
                VALUES
                (
                    @QuestionId, @QuestionDescription, @DifficultyLevelId, @QuestionImage,
                    @QuestionTypeId, @Status, @CreatedBy, @CreatedOn, @ModifiedBy, @ModifiedOn,
                    @SubjectID, @IndexTypeId, @ContentIndexId, @EmployeeId, @ExamTypeId,
                    @IsRejected, @IsApproved, @QuestionCode, @Explanation, @ExtraInformation,
                    @IsActive, @IsLive, @IsConfigure, @ModifierId, @CategoryId, @Paragraph, 
                    @ParentQId, @ParentQCode
                )";
                        var parameters = new Question
                        {
                            QuestionId = questionData.QuestionId,
                            QuestionDescription = questionData.QuestionDescription,
                            QuestionImage = questionData.QuestionImage,
                            DifficultyLevelId = questionData.DifficultyLevelId,
                            QuestionTypeId = questionData.QuestionTypeId,
                            Status = questionData.Status,
                            CreatedBy = questionData.CreatedBy,
                            CreatedOn = questionData.CreatedOn,
                            ModifiedBy = questionData.ModifiedBy,
                            ModifiedOn = questionData.ModifiedOn,
                            subjectID = questionData.SubjectID,
                            IndexTypeId = questionData.IndexTypeId,
                            ContentIndexId = questionData.ContentIndexId,
                            EmployeeId = questionData.EmployeeId,
                            ExamTypeId = questionData.ExamTypeId,
                            IsRejected = questionData.IsRejected,
                            IsApproved = questionData.IsApproved,
                            QuestionCode = questionData.QuestionCode,
                            Explanation = questionData.Explanation,
                            ExtraInformation = questionData.ExtraInformation,
                            IsActive = questionData.IsActive,
                            IsLive = questionData.IsLive,
                            IsConfigure = questionData.IsConfigure,
                            ModifierId = questionData.ModifierId,
                            CategoryId = questionData.CategoryId,
                            Paragraph = questionData.Paragraph,
                            ParentQId = questionData.ParentQId,
                            ParentQCode = questionData.ParentQCode
                        };

                        // Execute the SQL with parameter mapping
                        await _connection.ExecuteAsync(transferQuestionSql, parameters);


                        // Transfer data from tblQIDCourse
                        string fetchQIDCourseSql = @"
                SELECT *
                FROM tblQIDCourse
                WHERE QID = @QuestionId";

                        var qidCourseData = await _connection.QueryAsync(fetchQIDCourseSql, new { QuestionId = questionId });

                        foreach (var course in qidCourseData)
                        {
                            string transferQIDCourseSql = @"
                    INSERT INTO tbl_QIdCoursesVersion
                    (
                        QIDCourseId, QId, QuestionCode, CourseId, LevelId,
                        CreatedBy, CreatedOn, ModifiedBy, ModifiedOn
                    )
                    VALUES
                    (
                        @QIDCourseId, @QId, @QuestionCode, @CourseId, @LevelId,
                        @CreatedBy, @CreatedOn, @ModifiedBy, @ModifiedOn
                    )";

                            var parameters1 = new
                            {
                                QIDCourseId = course.QIDCourseID,
                                QId = course.QID,
                                QuestionCode = course.QuestionCode,
                                CourseId = course.CourseID,
                                LevelId = course.LevelId,
                                CreatedBy = course.CreatedBy,
                                CreatedOn = course.CreatedDate,
                                ModifiedBy = course.ModifiedBy,
                                ModifiedOn = course.ModifiedDate
                            };

                            await _connection.ExecuteAsync(transferQIDCourseSql, parameters1);
                        }

                        // Transfer data from tblAnswerMaster
                        string fetchAnswerMasterSql = @"
                SELECT *
                FROM tblAnswerMaster
                WHERE QuestionId = @QuestionId";

                        var answerMasterData = await _connection.QueryAsync(fetchAnswerMasterSql, new { QuestionId = questionId });
                        foreach (var answer in answerMasterData)
                        {
                            // Insert into tbl_AnswerMasterVersion
                            string transferAnswerMasterSql = @"
    INSERT INTO tbl_AnswerMasterVersion
    (
        AnswerId, QuestionId, QuestionTypeId, QuestionCode
    )
    VALUES
    (
        @AnswerId, @QuestionId, @QuestionTypeId, @QuestionCode
    )";

                            var answerMasterParams = new
                            {
                                answer.Answerid,
                                answer.Questionid,
                                answer.QuestionTypeid,
                                answer.QuestionCode
                            };

                            await _connection.ExecuteAsync(transferAnswerMasterSql, answerMasterParams);

                            // Fetch data from tblAnswerMultipleChoiceCategory
                            string fetchAnswerMultipleSql = @"
                            SELECT *
                            FROM tblAnswerMultipleChoiceCategory
                            WHERE AnswerId = @AnswerId";

                            var answerMultipleData = await _connection.QueryAsync(
                                fetchAnswerMultipleSql,
                                new { AnswerId = answer.Answerid }
                            );

                            foreach (var multiAnswer in answerMultipleData)
                            {
                                // Insert into tbl_AnswerMultipleVersion
                                string transferAnswerMultipleSql = @"
        INSERT INTO tbl_AnswerMultipleVersion
        (
            AnsMultiChoiceId, AnswerId, Answer, IsCorrect, MatchId
        )
        VALUES
        (
            @AnsMultiChoiceId, @AnswerId, @Answer, @IsCorrect, @MatchId
        )";

                                var multiAnswerParams = new
                                {
                                    AnsMultiChoiceId = multiAnswer.Answermultiplechoicecategoryid,
                                    AnswerId = multiAnswer.Answerid,
                                    Answer = multiAnswer.Answer,
                                    IsCorrect = multiAnswer.Iscorrect,
                                    MatchId = multiAnswer.Matchid
                                };

                                await _connection.ExecuteAsync(transferAnswerMultipleSql, multiAnswerParams);
                            }

                            // Fetch data from tblAnswersingleanswercategory
                            string fetchAnswerSingleSql = @"
    SELECT *
    FROM tblAnswersingleanswercategory
    WHERE AnswerId = @AnswerId";

                            var answerSingleData = await _connection.QueryAsync(
                                fetchAnswerSingleSql,
                                new { answer.Answerid }
                            );

                            foreach (var singleAnswer in answerSingleData)
                            {
                                // Insert into tbl_AnswerSingleVersion
                                string transferAnswerSingleSql = @"
        INSERT INTO tbl_AnswerSingleVersion
        (
            AnswerSingleId, AnswerId, Answer
        )
        VALUES
        (
            @AnswerSingleId, @AnswerId, @Answer
        )";

                                var singleAnswerParams = new
                                {
                                    AnswerSingleId = singleAnswer.Answersingleanswercategoryid,
                                    AnswerId = singleAnswer.Answerid,
                                    Answer = singleAnswer.Answer
                                };

                                await _connection.ExecuteAsync(transferAnswerSingleSql, singleAnswerParams);
                            }
                        }

                        string deleteQuestionSql = "DELETE FROM tblQuestion WHERE QuestionId = @QuestionId";
                        await _connection.ExecuteAsync(deleteQuestionSql, new { QuestionId = questionId });

                        string deleteQIDCourseSql = "DELETE FROM tblQIDCourse WHERE QID = @QuestionId";
                        await _connection.ExecuteAsync(deleteQIDCourseSql, new { QuestionId = questionId });

                        string deleteAnswerMasterSql = "DELETE FROM tblAnswerMaster WHERE QuestionId = @QuestionId";
                        await _connection.ExecuteAsync(deleteAnswerMasterSql, new { QuestionId = questionId });

                        string deleteAnswerMultipleSql = @"
                DELETE tblAnswerMultipleChoiceCategory 
                WHERE Answerid IN (SELECT Answerid FROM tblAnswerMaster WHERE QuestionId = @QuestionId)";
                        await _connection.ExecuteAsync(deleteAnswerMultipleSql, new { QuestionId = questionId });

                        string deleteAnswerSingleSql = @"
                DELETE tblAnswersingleanswercategory 
                WHERE AnswerId IN (SELECT Answerid FROM tblAnswerMaster WHERE QuestionId = @QuestionId)";
                        await _connection.ExecuteAsync(deleteAnswerSingleSql, new { QuestionId = questionId });

                    }

                    return new ServiceResponse<string>(true, "Question marked as live successfully and old versions archived", string.Empty, 200);
                }
                else
                {
                    return new ServiceResponse<string>(false, "Question is either not approved, rejected, or not active", string.Empty, 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<List<EmployeeListAssignedQuestionCount>>> GetAssignedQuestionsCount(int EmployeeId, int SubjectId)
        {
            try
            {
                // Initialize the list to hold the employee details with assigned question count
                List<EmployeeListAssignedQuestionCount> employeeList = new List<EmployeeListAssignedQuestionCount>();

                // SQL query to get the role of the given EmployeeId
                string employeeRoleQuery = @"
        SELECT e.RoleID, r.RoleCode
        FROM [tblEmployee] e
        JOIN [tblRole] r ON e.RoleID = r.RoleID
        WHERE e.EmployeeId = @EmployeeId";

                // Get the RoleID of the given EmployeeId
                var employeeRole = await _connection.QueryFirstOrDefaultAsync(employeeRoleQuery, new { EmployeeId });
                if (employeeRole == null)
                {
                    return new ServiceResponse<List<EmployeeListAssignedQuestionCount>>(false, "Employee not found.", employeeList, 404);
                }

                int roleID = employeeRole.RoleID;
                List<int> targetRoleIDs = new List<int>();

                // Determine the target roles based on the current role
                if (employeeRole.RoleCode == "TR")
                {
                    string sql = "SELECT [RoleID] FROM [tblRole] WHERE [RoleCode] = @RoleCode;";
                    var roleId = await _connection.QueryFirstOrDefaultAsync<int>(sql, new { RoleCode = "SE" });

                    // If the current role is Transcriber, target employees with SME role
                    targetRoleIDs.Add(roleId); // Assume RoleID for SME is 3
                }
                else if (employeeRole.RoleCode == "SE")
                {
                    string sql = "SELECT [RoleID] FROM [tblRole] WHERE [RoleCode] = @RoleCode;";
                    var roleId = await _connection.QueryFirstOrDefaultAsync<int>(sql, new { RoleCode = "PR" });
                    // If the current role is SME, target employees with Proofer and Transcriber roles
                    targetRoleIDs.Add(roleId); // Assume RoleID for Proofer is 4
                                               //var roleId1 = await _connection.QueryFirstOrDefaultAsync<int>(sql, new { RoleCode = "TR" });
                                               //targetRoleIDs.Add(roleId1); // Assume RoleID for Transcriber is 5
                }
                else if (employeeRole.RoleCode == "PR")
                {
                    string sql = "SELECT [RoleID] FROM [tblRole] WHERE [RoleCode] = @RoleCode;";
                    var roleId = await _connection.QueryFirstOrDefaultAsync<int>(sql, new { RoleCode = "SE" });
                    // If the current role is Proofer, target employees with SME role
                    targetRoleIDs.Add(roleId); // Assume RoleID for SME is 3
                }
                else
                {
                    return new ServiceResponse<List<EmployeeListAssignedQuestionCount>>(false, "Invalid role for the operation.", employeeList, 400);
                }

                // SQL query to fetch employees and their assigned question counts based on SubjectId
                string assignedQuestionsCountQuery = @"
        SELECT e.EmployeeId, 
               CONCAT(e.EmpFirstName, ' ', e.EmpMiddleName, ' ', e.EmpLastName) AS EmployeeName,
               COUNT(qp.Questionid) AS Count
        FROM [tblEmployee] e
        LEFT JOIN [tblQuestionProfiler] qp ON e.EmployeeId = qp.EmpId AND qp.Status = 1 AND qp.RejectedStatus = 0 AND qp.ApprovedStatus = 0
        JOIN [tblEmployeeSubject] es ON e.EmployeeId = es.Employeeid
        WHERE e.RoleID IN @TargetRoleIDs
        AND es.SubjectID = @SubjectId
        GROUP BY e.EmployeeId, e.EmpFirstName, e.EmpMiddleName, e.EmpLastName
        ORDER BY EmployeeName";

                // Execute query to get the list of employees with the count of assigned questions, filtered by SubjectId
                employeeList = (await _connection.QueryAsync<EmployeeListAssignedQuestionCount>(
                    assignedQuestionsCountQuery, new { TargetRoleIDs = targetRoleIDs, SubjectId }
                )).ToList();

                return new ServiceResponse<List<EmployeeListAssignedQuestionCount>>(true, "Employee list with assigned questions count retrieved successfully.", employeeList, 200);
            }
            catch (Exception ex)
            {
                // Return failure response with error message
                return new ServiceResponse<List<EmployeeListAssignedQuestionCount>>(false, ex.Message, new List<EmployeeListAssignedQuestionCount>(), 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetAllQuestionsList(GetAllQuestionListRequest request)
        {
            try
            {
                // Initialize a list to hold all question codes to fetch
                List<string> assignedQuestionCodes = new List<string>();

                // Step 1: Fetch list of QuestionCodes assigned to the given employee if EmployeeId is provided
                if (request.EmployeeId > 0)
                {
                    string fetchAssignedQuestionsSql = @"
                    SELECT QuestionCode 
                    FROM tblQuestionProfiler 
                    WHERE EmpId = @EmployeeId AND Status = 1";

                    assignedQuestionCodes = (await _connection.QueryAsync<string>(fetchAssignedQuestionsSql, new { EmployeeId = request.EmployeeId })).ToList();
                }

                // Step 2: Fetch questions based on the provided filters
                string fetchQuestionsSql = @"
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
                  AND q.IsApproved = 0
                  AND (q.EmployeeId = @EmployeeId OR q.QuestionCode IN @QuestionCodes)
                  AND q.IsActive = 1
                  AND q.IsLive = 0 AND q.IsConfigure = 1";

                var parameters = new
                {
                    ContentIndexId = request.ContentIndexId,
                    IndexTypeId = request.IndexTypeId,
                    EmployeeId = request.EmployeeId,
                    QuestionCodes = assignedQuestionCodes
                };

                var data = await _connection.QueryAsync<dynamic>(fetchQuestionsSql, parameters);

                if (data != null)
                {
                    // Convert the data to a list of DTOs
                    var response = data.Select(item =>
                    {
                        if (item.QuestionTypeId == 11)
                        {
                            return new QuestionResponseDTO
                            {
                                QuestionId = item.QuestionId,
                                Paragraph = item.Paragraph,
                                SubjectName = item.SubjectName,
                                EmployeeName = item.EmpFirstName,
                                IndexTypeName = item.IndexTypeName,
                                ContentIndexName = item.ContentIndexName,
                                QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                                ContentIndexId = item.ContentIndexId,
                                CreatedBy = item.CreatedBy,
                                CreatedOn = item.CreatedOn,
                                EmployeeId = item.EmployeeId,
                                IndexTypeId = item.IndexTypeId,
                                subjectID = item.subjectID,
                                ModifiedOn = item.ModifiedOn,
                                QuestionTypeId = item.QuestionTypeId,
                                QuestionTypeName = item.QuestionTypeName,
                                QuestionCode = item.QuestionCode,
                                Explanation = item.Explanation,
                                ExtraInformation = item.ExtraInformation,
                                IsActive = item.IsActive,
                                ComprehensiveChildQuestions = GetChildQuestions(item.QuestionCode)
                            };
                        }
                        else
                        {
                            return new QuestionResponseDTO
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
                                EmployeeName = item.EmpFirstName,
                                IndexTypeId = item.IndexTypeId,
                                IndexTypeName = item.IndexTypeName,
                                ContentIndexId = item.ContentIndexId,
                                ContentIndexName = item.ContentIndexName,
                                IsRejected = item.IsRejected,
                                IsApproved = item.IsApproved,
                                QuestionTypeName = item.QuestionTypeName,
                                QuestionCode = item.QuestionCode,
                                Explanation = item.Explanation,
                                ExtraInformation = item.ExtraInformation,
                                IsActive = item.IsActive,
                                QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                                //QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
                                //Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
                                //AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode)
                                MatchPairs = item.QuestionTypeId == 6 || item.QuestionTypeId == 12 ? GetMatchPairs(item.QuestionCode, item.QuestionId) : null,
                                MatchThePairType2Answers = item.QuestionTypeId == 12 ? GetMatchThePairType2Answers(item.QuestionCode, item.QuestionId) : null,
                                Answersingleanswercategories = (item.QuestionTypeId != 6 && item.QuestionTypeId != 12) ? GetSingleAnswer(item.QuestionCode, item.QuestionId) : null,
                                AnswerMultipleChoiceCategories = (item.QuestionTypeId != 12) ? GetMultipleAnswers(item.QuestionCode) : null
                            };
                        }
                    }
                    ).ToList();

                    // Step 3: Filter out questions assigned to other employees
                    var questionCodesCreatedByEmployee = response.Where(r => r.EmployeeId == request.EmployeeId).Select(r => r.QuestionCode).ToList();
                    string fetchAssignedToOthersSql = @"
                    SELECT DISTINCT QuestionCode 
                    FROM tblQuestionProfiler 
                    WHERE QuestionCode IN @QuestionCodes 
                    AND EmpId != @EmployeeId 
                    AND Status = 1";

                    var assignedToOthersQuestionCodes = (await _connection.QueryAsync<string>(fetchAssignedToOthersSql, new
                    {
                        QuestionCodes = questionCodesCreatedByEmployee,
                        EmployeeId = request.EmployeeId
                    })).ToList();

                    response = response.Where(r => !assignedToOthersQuestionCodes.Contains(r.QuestionCode)).ToList();

                    // Adding role information and pagination logic
                    foreach (var record in response)
                    {
                        var employeeRoleId = _connection.QuerySingleOrDefault<int?>("SELECT RoleID FROM tblEmployee WHERE EmployeeID = @EmployeeId", new { EmployeeId = record.EmployeeId });
                        record.userRole = employeeRoleId.HasValue ? GetRoleName(employeeRoleId.Value) : string.Empty;
                    }

                    int totalCount = response.Count;

                    var paginatedList = response.Skip((request.PageNumber - 1) * request.PageSize)
                                                .Take(request.PageSize)
                                                .ToList();

                    if (paginatedList.Count != 0)
                    {
                        return new ServiceResponse<List<QuestionResponseDTO>>(true, "Operation Successful", paginatedList, 200, totalCount);
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
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetAssignedQuestionsList(int employeeId)
        {
            try
            {
                string query = @"
        SELECT q.*, s.SubjectName, e.EmpFirstName + ' ' + e.EmpLastName AS EmployeeName,
               it.IndexType as IndexTypeName,
               CASE 
                   WHEN q.IndexTypeId = 1 THEN ci.ContentName_Chapter
                   WHEN q.IndexTypeId = 2 THEN ct.ContentName_Topic
                   WHEN q.IndexTypeId = 3 THEN cst.ContentName_SubTopic
               END AS ContentIndexName,
               qt.QuestionType as QuestionTypeName
        FROM tblQuestion q
        LEFT JOIN tblSubject s ON q.subjectID = s.SubjectID
        LEFT JOIN tblEmployee e ON q.EmployeeId = e.Employeeid
        LEFT JOIN tblQBIndexType it ON q.IndexTypeId = it.IndexId
        LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
        LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
        LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
        LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
        WHERE q.EmployeeId = @EmployeeId AND q.IsActive = 1 AND IsLive = 0 AND q.IsConfigure = 1";

                var questions = await _connection.QueryAsync<QuestionResponseDTO>(query, new { EmployeeId = employeeId });

                // Fetch additional details for each question
                foreach (var question in questions)
                {
                    question.QIDCourses = GetListOfQIDCourse(question.QuestionCode);
                    // question.QuestionSubjectMappings = GetListOfQuestionSubjectMapping(question.QuestionCode);
                    question.AnswerMultipleChoiceCategories = GetMultipleAnswers(question.QuestionCode);
                    question.Answersingleanswercategories = GetSingleAnswer(question.QuestionCode, question.QuestionId);
                }

                return new ServiceResponse<List<QuestionResponseDTO>>(true, "Questions fetched successfully", questions.ToList(), 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetApprovedQuestionsList(GetAllQuestionListRequest request)
        {
            try
            {
                // Step 1: Fetch the role of the employee based on EmployeeId
                string fetchRoleSql = @"
        SELECT r.RoleCode 
        FROM tblEmployee e
        JOIN tblRole r ON e.RoleID = r.RoleID
        WHERE e.Employeeid = @EmployeeId";

                string roleCode = await _connection.QueryFirstOrDefaultAsync<string>(fetchRoleSql, new { EmployeeId = request.EmployeeId });

                // Step 2: Handle Transcriptor scenario
                if (roleCode == "TR" || roleCode == "PR")
                {
                    // Return empty list for Transcriptors
                    return new ServiceResponse<List<QuestionResponseDTO>>(false, "You are not allowed to view questions.", new List<QuestionResponseDTO>(), 403);
                }

                // Initialize a list to hold question codes that are assigned to other employees for review
                List<string> assignedToOtherEmployeesQuestionCodes = new List<string>();

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
          AND q.IsApproved = 1
          AND q.IsActive = 1
          AND IsLive = 0 AND q.IsConfigure = 1
          AND EXISTS (
              SELECT *
              FROM tblQuestionProfiler qp
              WHERE qp.QuestionCode = q.QuestionCode
                AND qp.Status = 1
                AND qp.EmpId = @EmployeeId
                AND qp.ApprovedStatus = 1
                AND qp.RejectedStatus = 0)
          AND EXISTS (
              SELECT *
              FROM tblQuestionProfilerApproval qpa
              WHERE qpa.QuestionCode = q.QuestionCode)";
                var parameters = new
                {
                    ContentIndexId = request.ContentIndexId,
                    IndexTypeId = request.IndexTypeId,
                    request.EmployeeId
                };
                var data = await _connection.QueryAsync<dynamic>(sql, parameters);

                if (data != null)
                {

                    // Convert the data to a list of DTOs
                    var response = data.Select(item =>
                    {
                        if (item.QuestionTypeId == 11)
                        {
                            return new QuestionResponseDTO
                            {
                                QuestionId = item.QuestionId,
                                Paragraph = item.Paragraph,
                                SubjectName = item.SubjectName,
                                EmployeeName = item.EmpFirstName,
                                IndexTypeName = item.IndexTypeName,
                                ContentIndexName = item.ContentIndexName,
                                QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                                ContentIndexId = item.ContentIndexId,
                                CreatedBy = item.CreatedBy,
                                CreatedOn = item.CreatedOn,
                                EmployeeId = item.EmployeeId,
                                IndexTypeId = item.IndexTypeId,
                                subjectID = item.subjectID,
                                ModifiedOn = item.ModifiedOn,
                                QuestionTypeId = item.QuestionTypeId,
                                QuestionTypeName = item.QuestionTypeName,
                                QuestionCode = item.QuestionCode,
                                Explanation = item.Explanation,
                                ExtraInformation = item.ExtraInformation,
                                IsActive = item.IsActive,
                                ComprehensiveChildQuestions = GetChildQuestions(item.QuestionCode)
                            };
                        }
                        else
                        {
                            return new QuestionResponseDTO
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
                                EmployeeName = item.EmpFirstName,
                                IndexTypeId = item.IndexTypeId,
                                IndexTypeName = item.IndexTypeName,
                                ContentIndexId = item.ContentIndexId,
                                ContentIndexName = item.ContentIndexName,
                                IsRejected = item.IsRejected,
                                IsApproved = item.IsApproved,
                                QuestionTypeName = item.QuestionTypeName,
                                QuestionCode = item.QuestionCode,
                                Explanation = item.Explanation,
                                ExtraInformation = item.ExtraInformation,
                                IsActive = item.IsActive,
                                QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                                //QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
                                // Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
                                // AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode)
                                MatchPairs = item.QuestionTypeId == 6 || item.QuestionTypeId == 12 ? GetMatchPairs(item.QuestionCode, item.QuestionId) : null,
                                MatchThePairType2Answers = item.QuestionTypeId == 12 ? GetMatchThePairType2Answers(item.QuestionCode, item.QuestionId) : null,
                                Answersingleanswercategories = (item.QuestionTypeId != 6 && item.QuestionTypeId != 12) ? GetSingleAnswer(item.QuestionCode, item.QuestionId) : null,
                                AnswerMultipleChoiceCategories = (item.QuestionTypeId != 12) ? GetMultipleAnswers(item.QuestionCode) : null

                            };
                        }
                    }
                    ).ToList();

                    //var response = data.Select(item => new QuestionResponseDTO
                    //{
                    //    QuestionId = item.QuestionId,
                    //    QuestionDescription = item.QuestionDescription,
                    //    QuestionTypeId = item.QuestionTypeId,
                    //    Status = item.Status,
                    //    CreatedBy = item.CreatedBy,
                    //    CreatedOn = item.CreatedOn,
                    //    ModifiedBy = item.ModifiedBy,
                    //    ModifiedOn = item.ModifiedOn,
                    //    subjectID = item.subjectID,
                    //    SubjectName = item.SubjectName,
                    //    EmployeeId = item.EmployeeId,
                    //    EmployeeName = item.EmpFirstName,
                    //    IndexTypeId = item.IndexTypeId,
                    //    IndexTypeName = item.IndexTypeName,
                    //    ContentIndexId = item.ContentIndexId,
                    //    ContentIndexName = item.ContentIndexName,
                    //    QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                    //    Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
                    //    AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode),
                    //    IsApproved = item.IsApproved,
                    //    IsRejected = item.IsRejected,
                    //    QuestionTypeName = item.QuestionTypeName,
                    //    QuestionCode = item.QuestionCode,
                    //    Explanation = item.Explanation,
                    //    ExtraInformation = item.ExtraInformation,
                    //    IsActive = item.IsActive
                    //}).ToList();
                    // Adding role information and pagination logic
                    foreach (var record in response)
                    {
                        var employeeRoleId = _connection.QuerySingleOrDefault<int?>("SELECT RoleID FROM tblEmployee WHERE EmployeeID = @EmployeeId", new { EmployeeId = record.EmployeeId });
                        record.userRole = employeeRoleId.HasValue ? GetRoleName(employeeRoleId.Value) : string.Empty;
                    }
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

                string roleQuery = @"select r.RoleCode from  [tblEmployee] e 
                                                 LEFT JOIN [tblRole] r ON e.RoleID = r.RoleID
                                                 WHERE e.Employeeid = @EmployeeId";
                string Role = await _connection.QueryFirstOrDefaultAsync<string>(roleQuery, new { EmployeeId = request.EmployeeId });
                string sql;
                if (Role == "PR")
                {
                    sql = @"
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
          AND q.IsRejected = 1
          AND q.IsApproved = 0
          AND q.IsActive = 1
          AND IsLive = 0 AND q.IsConfigure = 1
          AND EXISTS (
              SELECT *
              FROM tblQuestionProfiler qp
              WHERE qp.QuestionCode = q.QuestionCode
                AND qp.Status = 1
                AND qp.EmpId = @EmployeeId
                AND qp.RejectedStatus = 0)
          AND EXISTS (
              SELECT *
              FROM tblQuestionProfilerRejections qpr
              WHERE qpr.QuestionCode = q.QuestionCode
                AND qpr.RejectedBy = @EmployeeId)";
                }
                else
                {
                    sql = @"
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
          AND q.IsRejected = 1
          AND q.IsApproved = 0
          AND q.IsActive = 1
          AND IsLive = 0 AND q.IsConfigure = 1
          AND EXISTS (
              SELECT *
              FROM tblQuestionProfiler qp
              WHERE qp.QuestionCode = q.QuestionCode
                AND qp.Status = 1
                AND qp.EmpId = @EmployeeId
                AND qp.RejectedStatus = 1)";
                }
                var parameters = new
                {
                    ContentIndexId = request.ContentIndexId,
                    IndexTypeId = request.IndexTypeId,
                    request.EmployeeId
                };

                var data = await _connection.QueryAsync<dynamic>(sql, parameters);

                if (data != null)
                {
                    //var response = data.Select(item => new QuestionResponseDTO
                    //{
                    //    QuestionId = item.QuestionId,
                    //    QuestionDescription = item.QuestionDescription,
                    //    QuestionTypeId = item.QuestionTypeId,
                    //    Status = item.Status,
                    //    CreatedBy = item.CreatedBy,
                    //    CreatedOn = item.CreatedOn,
                    //    ModifiedBy = item.ModifiedBy,
                    //    ModifiedOn = item.ModifiedOn,
                    //    subjectID = item.subjectID,
                    //    SubjectName = item.SubjectName,
                    //    EmployeeId = item.EmployeeId,
                    //    EmployeeName = item.EmpFirstName,
                    //    IndexTypeId = item.IndexTypeId,
                    //    IndexTypeName = item.IndexTypeName,
                    //    ContentIndexId = item.ContentIndexId,
                    //    ContentIndexName = item.ContentIndexName,
                    //    QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                    //    //QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
                    //    Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
                    //    AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode),
                    //    IsApproved = item.IsApproved,
                    //    IsRejected = item.IsRejected,
                    //    QuestionTypeName = item.QuestionTypeName,
                    //    QuestionCode = item.QuestionCode,
                    //    Explanation = item.Explanation,
                    //    ExtraInformation = item.ExtraInformation,
                    //    IsActive = item.IsActive
                    //}).ToList();

                    // Convert the data to a list of DTOs
                    var response = data.Select(item =>
                    {
                        if (item.QuestionTypeId == 11)
                        {
                            return new QuestionResponseDTO
                            {
                                QuestionId = item.QuestionId,
                                Paragraph = item.Paragraph,
                                SubjectName = item.SubjectName,
                                EmployeeName = item.EmpFirstName,
                                IndexTypeName = item.IndexTypeName,
                                ContentIndexName = item.ContentIndexName,
                                QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                                ContentIndexId = item.ContentIndexId,
                                CreatedBy = item.CreatedBy,
                                CreatedOn = item.CreatedOn,
                                EmployeeId = item.EmployeeId,
                                IndexTypeId = item.IndexTypeId,
                                subjectID = item.subjectID,
                                ModifiedOn = item.ModifiedOn,
                                QuestionTypeId = item.QuestionTypeId,
                                QuestionTypeName = item.QuestionTypeName,
                                QuestionCode = item.QuestionCode,
                                Explanation = item.Explanation,
                                ExtraInformation = item.ExtraInformation,
                                IsActive = item.IsActive,
                                ComprehensiveChildQuestions = GetChildQuestions(item.QuestionCode)
                            };
                        }
                        else
                        {
                            return new QuestionResponseDTO
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
                                EmployeeName = item.EmpFirstName,
                                IndexTypeId = item.IndexTypeId,
                                IndexTypeName = item.IndexTypeName,
                                ContentIndexId = item.ContentIndexId,
                                ContentIndexName = item.ContentIndexName,
                                IsRejected = item.IsRejected,
                                IsApproved = item.IsApproved,
                                QuestionTypeName = item.QuestionTypeName,
                                QuestionCode = item.QuestionCode,
                                Explanation = item.Explanation,
                                ExtraInformation = item.ExtraInformation,
                                IsActive = item.IsActive,
                                QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                                //QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
                                // Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
                                // AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode)
                                MatchPairs = item.QuestionTypeId == 6 || item.QuestionTypeId == 12 ? GetMatchPairs(item.QuestionCode, item.QuestionId) : null,
                                MatchThePairType2Answers = item.QuestionTypeId == 12 ? GetMatchThePairType2Answers(item.QuestionCode, item.QuestionId) : null,
                                Answersingleanswercategories = (item.QuestionTypeId != 6 && item.QuestionTypeId != 12) ? GetSingleAnswer(item.QuestionCode, item.QuestionId) : null,
                                AnswerMultipleChoiceCategories = (item.QuestionTypeId != 12) ? GetMultipleAnswers(item.QuestionCode) : null

                            };
                        }
                    }
                    ).ToList();
                    // Adding role information and pagination logic
                    foreach (var record in response)
                    {
                        var employeeRoleId = _connection.QuerySingleOrDefault<int?>("SELECT RoleID FROM tblEmployee WHERE EmployeeID = @EmployeeId", new { EmployeeId = record.EmployeeId });
                        record.userRole = employeeRoleId.HasValue ? GetRoleName(employeeRoleId.Value) : string.Empty;
                    }
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
        public async Task<ServiceResponse<QuestionResponseDTO>> GetQuestionByCode(string questionCode)
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
                WHERE q.QuestionCode = @QuestionCode AND q.IsActive = 1 AND q.IsConfigure = 1";

                var parameters = new { QuestionCode = questionCode };

                var item = await _connection.QueryFirstOrDefaultAsync<dynamic>(sql, parameters);

                if (item != null)
                {
                    if (item.QuestionTypeId == 11)
                    {
                        var questionResponse = new QuestionResponseDTO
                        {
                            QuestionId = item.QuestionId,
                            Paragraph = item.Paragraph,
                            SubjectName = item.SubjectName,
                            EmployeeName = item.EmpFirstName,
                            IndexTypeName = item.IndexTypeName,
                            ContentIndexName = item.ContentIndexName,
                            QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                            //QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
                            //Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
                            //AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode),
                            ContentIndexId = item.ContentIndexId,
                            CreatedBy = item.CreatedBy,
                            CreatedOn = item.CreatedOn,
                            EmployeeId = item.EmployeeId,
                            IndexTypeId = item.IndexTypeId,
                            subjectID = item.subjectID,
                            ModifiedOn = item.ModifiedOn,
                            QuestionTypeId = item.QuestionTypeId,
                            QuestionTypeName = item.QuestionTypeName,
                            QuestionCode = item.QuestionCode,
                            Explanation = item.Explanation,
                            ExtraInformation = item.ExtraInformation,
                            IsActive = item.IsActive,
                            ComprehensiveChildQuestions = GetChildQuestions(item.QuestionCode)
                        };
                        return new ServiceResponse<QuestionResponseDTO>(true, "Operation Successful", questionResponse, 200);
                    }
                    else
                    {
                        var questionResponse = new QuestionResponseDTO
                        {
                            QuestionId = item.QuestionId,
                            QuestionDescription = item.QuestionDescription,
                            SubjectName = item.SubjectName,
                            EmployeeName = item.EmpFirstName,
                            IndexTypeName = item.IndexTypeName,
                            ContentIndexName = item.ContentIndexName,
                            QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                            //QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
                            //Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
                            //AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode),
                            ContentIndexId = item.ContentIndexId,
                            CreatedBy = item.CreatedBy,
                            CreatedOn = item.CreatedOn,
                            EmployeeId = item.EmployeeId,
                            IndexTypeId = item.IndexTypeId,
                            subjectID = item.subjectID,
                            ModifiedOn = item.ModifiedOn,
                            QuestionTypeId = item.QuestionTypeId,
                            QuestionTypeName = item.QuestionTypeName,
                            QuestionCode = item.QuestionCode,
                            Explanation = item.Explanation,
                            ExtraInformation = item.ExtraInformation,
                            IsActive = item.IsActive,
                            MatchPairs = item.QuestionTypeId == 6 || item.QuestionTypeId == 12 ? GetMatchPairs(item.QuestionCode, item.QuestionId) : null,
                            MatchThePairType2Answers = item.QuestionTypeId == 12 ? GetMatchThePairType2Answers(item.QuestionCode, item.QuestionId) : null,
                            Answersingleanswercategories = (item.QuestionTypeId != 6 && item.QuestionTypeId != 12) ? GetSingleAnswer(item.QuestionCode, item.QuestionId) : null,
                            AnswerMultipleChoiceCategories = (item.QuestionTypeId != 12) ? GetMultipleAnswers(item.QuestionCode) : null

                        };
                        return new ServiceResponse<QuestionResponseDTO>(true, "Operation Successful", questionResponse, 200);
                    }

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
        private List<MatchPair> GetMatchPairs(string questionCode, int questionId)
        {
            const string query = @"
        SELECT MatchThePairId, PairColumn, PairRow, PairValue
        FROM tblQuestionMatchThePair
        WHERE QuestionCode = @QuestionCode AND QuestionId = @QuestionId";


            return _connection.Query<MatchPair>(query, new { QuestionCode = questionCode, QuestionId = questionId }).ToList();

        }
        private List<MatchThePairAnswer> GetMatchThePairType2Answers(string questionCode, int questionId)
        {
            const string getAnswerIdQuery = @"
        SELECT AnswerId 
        FROM tblAnswerMaster
        WHERE QuestionCode = @QuestionCode AND QuestionId = @QuestionId";

            const string getAnswersQuery = @"
        SELECT MatchThePair2Id, PairColumn, PairRow
        FROM tblOptionsMatchThePair2
        WHERE AnswerId = @AnswerId";

           
                var answerId = _connection.QueryFirstOrDefault<int?>(getAnswerIdQuery, new { QuestionCode = questionCode, QuestionId = questionId });

                if (answerId == null)
                {
                    return new List<MatchThePairAnswer>();
                }

                return _connection.Query<MatchThePairAnswer>(getAnswersQuery, new { AnswerId = answerId }).ToList();
            
        }
        private List<ParagraphQuestions> GetChildQuestions(string QuestionCode)
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
                WHERE q.ParentQCode = @QuestionCode AND q.IsActive = 1 AND IsLive = 0 AND q.IsConfigure = 1";
            var parameters = new { QuestionCode = QuestionCode };
            var item = _connection.Query<dynamic>(sql, parameters);
            var response = item.Select(m => new ParagraphQuestions
            {
                QuestionId = m.QuestionId,
                QuestionDescription = m.QuestionDescription,
                ParentQId = m.ParentQId,
                ParentQCode = m.ParentQCode,
                QuestionTypeId = m.QuestionTypeId,
                Status = m.Status,
                CategoryId = m.CategoryId,
                CreatedBy = m.CreatedBy,
                CreatedOn = m.CreatedOn,
                ModifiedBy = m.ModifiedBy,
                ModifiedOn = m.ModifiedOn,
                subjectID = m.SubjectID,
                EmployeeId = m.EmployeeId,
                ModifierId = m.ModifierId,
                IndexTypeId = m.IndexTypeId,
                ContentIndexId = m.ContentIndexId,
                IsRejected = m.IsRejected,
                IsApproved = m.IsApproved,
                QuestionCode = m.QuestionCode,
                Explanation = m.Explanation,
                ExtraInformation = m.ExtraInformation,
                IsActive = m.IsActive,
                IsConfigure = m.IsConfigure,
                AnswerMultipleChoiceCategories = GetMultipleAnswers(m.QuestionCode),
                Answersingleanswercategories = GetSingleAnswer(m.QuestionCode, m.QuestionId)
            }).ToList();
            return response;
        }
        public async Task<ServiceResponse<List<QuestionComparisonDTO>>> CompareQuestionAsync(QuestionCompareRequest newQuestion)
        {
            // Fetch only active questions
            string query = "SELECT QuestionCode, QuestionId, QuestionDescription FROM tblQuestion WHERE IsActive = 1 AND IsConfigure = 1 ";
            var existingQuestions = await _connection.QueryAsync<Question>(query);

            // Calculate similarity and create comparison objects
            var comparisons = existingQuestions.Select(q => new QuestionComparisonDTO
            {
                QuestionCode = q.QuestionCode,
                QuestionID = q.QuestionId,
                QuestionText = q.QuestionDescription,
                Similarity = CalculateSimilarity(newQuestion.NewQuestion, q.QuestionDescription)
            })
            .Where(c => c.Similarity > 59.0 && c.Similarity < 101.0) // Filter based on similarity
            .OrderByDescending(c => c.Similarity) // Order by similarity in descending order
            .Take(10) // Take top 10 results
            .ToList();

            return new ServiceResponse<List<QuestionComparisonDTO>>(true, "Comparison results", comparisons, 200, comparisons.Count);
        }
        public async Task<ServiceResponse<string>> RejectQuestion(QuestionRejectionRequestDTO request)
        {
            try
            {
                string updateSql = @"
        UPDATE [tblQuestion]
        SET 
           IsRejected = @IsRejected,
           IsApproved = 0 
        WHERE
           QuestionCode = @QuestionCode AND IsActive = 1";

                var parameters = new
                {
                    request.QuestionCode,
                    IsRejected = true
                };

                var affectedRows = await _connection.ExecuteAsync(updateSql, parameters);
                if (affectedRows > 0)
                {
                    string sql = @"
            INSERT INTO [tblQuestionProfilerRejections]
            ([Questionid], [CreatedDate], [QuestionRejectReason], [RejectedBy], QuestionCode, FileUpload)
            VALUES (@QuestionId, @CreatedDate, @QuestionRejectReason, @RejectedBy, @QuestionCode, @FileUpload);

            SELECT CAST(SCOPE_IDENTITY() as int)";

                    var questionId = await _connection.QueryFirstOrDefaultAsync<int>(@"
                SELECT QuestionId FROM [tblQuestion] 
                WHERE QuestionCode = @QuestionCode AND IsActive = 1",
                        new { request.QuestionCode });

                    if (questionId > 0)
                    {
                        var newId = await _connection.ExecuteScalarAsync<int>(sql, new
                        {
                            QuestionId = questionId,
                            CreatedDate = request.RejectedDate,
                            QuestionRejectReason = request.RejectedReason,
                            request.Rejectedby,
                            request.QuestionCode,
                            FileUpload = FileUpload(request.FileUpload)
                        });

                        if (newId > 0)
                        {
                            // Update tblQuestionProfiler to set status of the current profiler to inactive
                            string updateProfilerSql = @"
                            UPDATE tblQuestionProfiler
                            SET RejectedStatus = 1, Status = 0
                            WHERE Status = 1 AND QuestionCode = @QuestionCode";

                            await _connection.ExecuteAsync(updateProfilerSql, new { request.QuestionCode });


                            int questionOwner = await _connection.QueryFirstOrDefaultAsync<int>(@"select EmployeeId from [tblQuestion] where QuestionCode = @QuestionCode and 
                            IsActive = 1", new { QuestionCode = request.QuestionCode });

                            string roleQuery = @"select r.RoleCode from  [tblEmployee] e 
                                                 LEFT JOIN [tblRole] r ON e.RoleID = r.RoleID
                                                 WHERE e.Employeeid = @EmployeeId";
                            string ownerRole = await _connection.QueryFirstOrDefaultAsync<string>(roleQuery, new { EmployeeId = questionOwner });
                            string rejectorRole = await _connection.QueryFirstOrDefaultAsync<string>(roleQuery, new { EmployeeId = request.Rejectedby });

                            if (ownerRole == "TR" && rejectorRole == "PR")
                            {
                                // Fetch second last record from tblQuestionProfiler
                                var profilerList = (await _connection.QueryAsync<dynamic>(@"
    SELECT * FROM 
    (
        SELECT *, ROW_NUMBER() OVER (ORDER BY QPID DESC) AS RowNum
        FROM tblQuestionProfiler
        WHERE QuestionCode = @QuestionCode
    ) AS OrderedProfilerList
    WHERE RowNum = 2", new { QuestionCode = request.QuestionCode })).FirstOrDefault();

                                if (profilerList != null)
                                {
                                    string insertSql = @"
    INSERT INTO tblQuestionProfiler (Questionid, QuestionCode, EmpId, RejectedStatus, ApprovedStatus, Status, AssignedDate)
    VALUES (@Questionid, @QuestionCode, @EmpId, 1, 0, 1, @AssignedDate)";

                                    await _connection.ExecuteAsync(insertSql, new
                                    {
                                        Questionid = questionId,
                                        request.QuestionCode,
                                        EmpId = profilerList.EmpId, // Map EmpId from the second last record
                                        AssignedDate = DateTime.Now
                                    });
                                }
                            }
                            else
                            {
                                // Insert a new record for the new profiler with ApprovedStatus = false and Status = true
                                string insertSql = @"
            INSERT INTO tblQuestionProfiler (Questionid, QuestionCode, EmpId, RejectedStatus, ApprovedStatus, Status, AssignedDate)
            VALUES (@Questionid, @QuestionCode, @EmpId, 1, 0, 1, @AssignedDate)";

                                await _connection.ExecuteAsync(insertSql, new
                                {
                                    Questionid = questionId,
                                    request.QuestionCode,
                                    EmpId = questionOwner,
                                    AssignedDate = DateTime.Now
                                });
                            }
                            return new ServiceResponse<string>(true, "Question rejected successfully", "Success", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Some error occurred", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Question not found", "Failure", 404);
                    }
                }
                else
                {
                    return new ServiceResponse<string>(false, "Question not found or already inactive", "Failure", 404);
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
            SET IsApproved = @IsApproved, IsRejected = 0
            WHERE QuestionCode = @QuestionCode AND IsActive = 1";

                    var updateParameters = new
                    {
                        request.QuestionCode,
                        IsApproved = true
                    };

                    var affectedRows = await _connection.ExecuteAsync(updateQuestionSql, updateParameters, transaction);
                    if (affectedRows == 0)
                    {
                        return new ServiceResponse<string>(false, "Question not found or already inactive", "Failure", 404);
                    }

                    // Get the QuestionId based on QuestionCode and IsActive = 1
                    var questionId = await _connection.QueryFirstOrDefaultAsync<int>(@"
            SELECT QuestionId FROM [tblQuestion]
            WHERE QuestionCode = @QuestionCode AND IsActive = 1",
                    new { request.QuestionCode }, transaction);

                    if (questionId == 0)
                    {
                        return new ServiceResponse<string>(false, "Question not found or already inactive", "Failure", 404);
                    }

                    // Insert into tblQuestionProfilerApproval
                    string insertApprovalSql = @"
            INSERT INTO [tblQuestionProfilerApproval]
            ([QuestionId], [ApprovedBy], [ApprovedDate], QuestionCode)
            VALUES (@QuestionId, @ApprovedBy, @ApprovedDate, @QuestionCode)";

                    var insertApprovalParameters = new
                    {
                        QuestionId = questionId,
                        request.ApprovedBy,
                        ApprovedDate = request.ApprovedDate ?? DateTime.UtcNow,
                        QuestionCode = request.QuestionCode
                    };

                    await _connection.ExecuteAsync(insertApprovalSql, insertApprovalParameters, transaction);

                    // Update tblQuestionProfiler to set status of the current profiler to inactive
                    string updateProfilerSql = @"
            UPDATE tblQuestionProfiler
            SET Status = 0, ApprovedStatus = 1
            WHERE Status = 1 AND QuestionCode = @QuestionCode";

                    await _connection.ExecuteAsync(updateProfilerSql, new { request.QuestionCode }, transaction);
                    // Fetch second last record from tblQuestionProfiler
                    var profilerList = (await _connection.QueryAsync<dynamic>(@"
    SELECT * FROM 
    (
        SELECT *, ROW_NUMBER() OVER (ORDER BY QPID DESC) AS RowNum
        FROM tblQuestionProfiler
        WHERE QuestionCode = @QuestionCode
    ) AS OrderedProfilerList
    WHERE RowNum = 2", new { QuestionCode = request.QuestionCode }, transaction)).FirstOrDefault();
                    string insertSql = @"
    INSERT INTO tblQuestionProfiler (Questionid, QuestionCode, EmpId, RejectedStatus, ApprovedStatus, Status, AssignedDate)
    VALUES (@Questionid, @QuestionCode, @EmpId, 0, 1, 1, @AssignedDate)";
                    if (profilerList != null)
                    {
                        await _connection.ExecuteAsync(insertSql, new
                        {
                            Questionid = questionId,
                            request.QuestionCode,
                            EmpId = profilerList.EmpId, // Map EmpId from the second last record
                            AssignedDate = DateTime.Now
                        }, transaction);
                    }
                    else
                    {
                        int employeeId = await _connection.QueryFirstOrDefaultAsync<int>(@"select EmployeeId from tblQuestion where QuestionCode = @QuestionCode and IsActive = 1", new { QuestionCode = request.QuestionCode }, transaction);
                        await _connection.ExecuteAsync(insertSql, new
                        {
                            Questionid = questionId,
                            request.QuestionCode,
                            EmpId = employeeId, // Map EmpId from the second last record
                            AssignedDate = DateTime.Now
                        }, transaction);
                    }
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
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
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


                // Check if the question is already assigned to a profiler with active status based on QuestionCode
                string checkSql = @"
            SELECT QPID, EmpId
            FROM tblQuestionProfiler
            WHERE QuestionCode = @QuestionCode AND Status = 1";

                var existingProfiler = await _connection.QueryFirstOrDefaultAsync<(int? QPID, int EmpId)>(checkSql, new { request.QuestionCode });

                // If the question is already assigned, update the status of the current profiler to inactive
                if (existingProfiler.QPID.HasValue)
                {
                    // Check the role of the current employee (SME or Proofer)
                    string fetchRoleCodeSql = @"
                        SELECT r.RoleCode
                        FROM tblEmployee e
                        JOIN tblRole r ON e.RoleID = r.RoleID
                        WHERE e.Employeeid = @EmpId";

                    var currentRoleCode = await _connection.QueryFirstOrDefaultAsync<string>(fetchRoleCodeSql, new { EmpId = existingProfiler.EmpId });

                    // Fetch the role of the new employee (to whom the question is being assigned)
                    var newRoleCode = await _connection.QueryFirstOrDefaultAsync<string>(fetchRoleCodeSql, new { EmpId = request.EmpId });

                    // If current role is SME and the new role is Proofer, approve the question
                    if (currentRoleCode == "SE" && newRoleCode == "PR") // RoleCode 3 = SME, 4 = Proofer
                    {
                        // Get the QuestionId based on QuestionCode and IsActive = 1
                        var questionId1 = await _connection.QueryFirstOrDefaultAsync<int>(@"
            SELECT QuestionId FROM [tblQuestion]
            WHERE QuestionCode = @QuestionCode AND IsActive = 1",
                        new { request.QuestionCode });

                        if (questionId1 == 0)
                        {
                            return new ServiceResponse<string>(false, "Question not found or already inactive", "Failure", 404);
                        }

                        // Insert into tblQuestionProfilerApproval
                        string insertApprovalSql = @"
            INSERT INTO [tblQuestionProfilerApproval]
            ([QuestionId], [ApprovedBy], [ApprovedDate], QuestionCode)
            VALUES (@QuestionId, @ApprovedBy, @ApprovedDate, @QuestionCode)";

                        var insertApprovalParameters = new
                        {
                            QuestionId = questionId1,
                            ApprovedBy = existingProfiler.EmpId,
                            ApprovedDate = DateTime.UtcNow,
                            QuestionCode = request.QuestionCode
                        };

                        await _connection.ExecuteAsync(insertApprovalSql, insertApprovalParameters);

                        // Update tblQuestionProfiler to set status of the current profiler to inactive
                        string updateProfilerSql = @"
            UPDATE tblQuestionProfiler
            SET Status = 0, ApprovedStatus = 1
            WHERE Status = 1 AND QuestionCode = @QuestionCode";

                        await _connection.ExecuteAsync(updateProfilerSql, new { request.QuestionCode });

                    }

                    // Update the status of the current profiler to inactive
                    string updateSql = @"
                UPDATE tblQuestionProfiler
                SET Status = 0
                WHERE QPID = @QPID";

                    await _connection.ExecuteAsync(updateSql, new { QPID = existingProfiler.QPID });
                }

                // Fetch the QuestionId from the main table using QuestionCode and IsActive = 1
                string fetchQuestionIdSql = @"
            SELECT QuestionId
            FROM tblQuestion
            WHERE QuestionCode = @QuestionCode AND IsActive = 1";

                var questionId = await _connection.QueryFirstOrDefaultAsync<int?>(fetchQuestionIdSql, new { request.QuestionCode });

                if (!questionId.HasValue)
                {
                    return new ServiceResponse<string>(false, "Question not found or inactive", string.Empty, 404);
                }
                string roleQuery = @"select r.RoleCode from  [tblEmployee] e 
                                                 LEFT JOIN [tblRole] r ON e.RoleID = r.RoleID
                                                 WHERE e.Employeeid = @EmployeeId";
                string GetRoleName = await _connection.QueryFirstOrDefaultAsync<string>(roleQuery, new { EmployeeId = request.EmpId });
                if (GetRoleName == "SE")
                {
                    // Update the tblQuestion to set IsRejected and IsApproved to 0
                    string updateQuestionSql = @"
            UPDATE tblQuestion
            SET IsRejected = 0, IsApproved = 0
            WHERE QuestionId = @QuestionId";

                    await _connection.ExecuteAsync(updateQuestionSql, new { QuestionId = questionId.Value });
                }
                // Insert a new record for the new profiler with ApprovedStatus = false and Status = true
                string insertSql = @"
            INSERT INTO tblQuestionProfiler (Questionid, QuestionCode, EmpId, RejectedStatus, ApprovedStatus, Status, AssignedDate)
            VALUES (@Questionid, @QuestionCode, @EmpId, 0, 0, 1, @AssignedDate)";

                await _connection.ExecuteAsync(insertSql, new
                {
                    Questionid = questionId.Value,
                    request.QuestionCode,
                    request.EmpId,
                    AssignedDate = DateTime.Now
                });

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
        public async Task<ServiceResponse<List<ContentIndexResponses>>> GetSyllabusDetailsBySubject(SyllabusDetailsRequest request)
        {
            try
            {
                // SQL Query
                string sql = @"
            SELECT sd.*, s.*
            FROM [tblSyllabus] s
            JOIN [tblSyllabusDetails] sd ON s.SyllabusId = sd.SyllabusID
            WHERE s.APID = @APId
            AND (sd.SubjectId = @SubjectId OR @SubjectId = 0)";

                var syllabusDetails = await _connection.QueryAsync<dynamic>(sql, new
                {
                    request.APId,
                    request.SubjectId
                });

                // Process the results to create a hierarchical structure
                var contentIndexResponse = new List<ContentIndexResponses>();

                foreach (var detail in syllabusDetails)
                {
                    int indexTypeId = detail.IndexTypeId;

                    // Query to count questions
                    string countQuestionsSql = @"
                SELECT COUNT(*) 
                FROM [tblQuestion] 
                WHERE IndexTypeId = @IndexTypeId AND ContentIndexId = @ContentIndexId AND IsActive = 1";

                    int questionCount = await _connection.ExecuteScalarAsync<int>(countQuestionsSql, new
                    {
                        IndexTypeId = indexTypeId,
                        ContentIndexId = detail.ContentIndexId
                    });

                    if (indexTypeId == 1) // Chapter
                    {
                        string getchapter = @"SELECT * FROM tblContentIndexChapters WHERE ContentIndexId = @ContentIndexId;";
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
                            Count = questionCount, // Adding question count here
                            ContentIndexTopics = new List<ContentIndexTopicsResponse>()
                        };

                        // Add to response list
                        contentIndexResponse.Add(chapter);
                    }
                    else if (indexTypeId == 2) // Topic
                    {
                        string gettopic = @"SELECT * FROM tblContentIndexTopics WHERE ContInIdTopic = @ContentIndexId;";
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
                            Count = questionCount, // Adding question count here
                            ContentIndexSubTopics = new List<ContentIndexSubTopicResponse>()
                        };

                        // Check if the chapter exists in the response
                        var existingChapter = contentIndexResponse.FirstOrDefault(c => c.ChapterCode == data.ChapterCode);
                        if (existingChapter != null)
                        {
                            existingChapter.ContentIndexTopics.Add(topic);
                        }
                        else
                        {
                            // Create a new chapter entry if it doesn't exist
                            var newChapter = new ContentIndexResponses
                            {
                                ChapterCode = detail.ChapterCode,
                                ContentIndexTopics = new List<ContentIndexTopicsResponse> { topic }
                            };
                            contentIndexResponse.Add(newChapter);
                        }
                    }
                    else if (indexTypeId == 3) // SubTopic
                    {
                        string getsubtopic = @"SELECT * FROM tblContentIndexSubTopics WHERE ContInIdSubTopic = @ContentIndexId;";
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
                            TopicCode = data.TopicCode,
                            Count = questionCount // Adding question count here
                        };

                        // Find the corresponding topic
                        var existingTopic = contentIndexResponse
                            .SelectMany(c => c.ContentIndexTopics)
                            .FirstOrDefault(t => t.TopicCode == data.TopicCode);

                        if (existingTopic != null)
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
        public async Task<ServiceResponse<QuestionProfilerResponse>> GetQuestionProfilerDetails(string QuestionCode)
        {
            try
            {

                string fetchQuestionIdSql = @"
            SELECT QuestionId
            FROM tblQuestion
            WHERE QuestionCode = @QuestionCode AND IsActive = 1 AND IsConfigure = 1 "
                ;

                var questionId = await _connection.QueryFirstOrDefaultAsync<int?>(fetchQuestionIdSql, new { QuestionCode });

                if (!questionId.HasValue)
                {
                    return new ServiceResponse<QuestionProfilerResponse>(false, "Question not found or inactive", new QuestionProfilerResponse(), 404);
                }

                string sql = @"
        SELECT qp.QPID, qp.Questionid, qp.EmpId, qp.ApprovedStatus, qp.AssignedDate,
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
        WHERE qp.QuestionCode = @QuestionCode";

                var parameters = new { QuestionCode };

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
                                           QPId = g.QPID,
                                           EmpId = g.EmpId,
                                           EmpName = g.EmpName,
                                           Role = g.Role,
                                           RoleId = g.RoleID,
                                           QuestionCode = g.QuestionCode,
                                           AssignedDate = g.AssignedDate,
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
                                             ModifiedDate = g.ModifiedDate,
                                             QuestionCode = g.QuestionsCode
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
                                                                RejectedReason = g.RejectedReason,
                                                                QuestionCode = g.QuestionCode,
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
        public async Task<ServiceResponse<object>> CompareQuestionVersions(string questionCode)
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
               END AS ContentIndexName,
               q.IsActive
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
        WHERE q.QuestionCode = @QuestionCode AND q.IsConfigure = 1
        ORDER BY q.CreatedOn DESC";

                var parameters = new { QuestionCode = questionCode };

                var items = (await _connection.QueryAsync<dynamic>(sql, parameters)).ToList();

                var inactiveItems = items.Where(x => x.IsActive == false).ToList();
                var activeItems = items.Where(x => x.IsActive == true).ToList();

                if (inactiveItems.Count == 0 || activeItems.Count == 0)
                {
                    return new ServiceResponse<object>(false, "Required versions not found", null, 404);
                }

                var originalVersion = await CreateQuestionResponseDTO(inactiveItems.First());
                var finalVersion = await CreateQuestionResponseDTO(activeItems.First());

                var response = new
                {
                    OriginalVersion = originalVersion,
                    FinalVersion = finalVersion
                };

                return new ServiceResponse<object>(true, "Operation Successful", response, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<object>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<byte[]>> GenerateExcelFile(DownExcelRequest request)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Create an ExcelPackage
                using (var package = new ExcelPackage())
                {
                    string contentCode = string.Empty;
                    int subjectId = request.subjectId;

                    if (request.indexTypeId == 1)
                    {
                        // Fetch Chapter Data
                        var chapter = await _connection.QueryFirstOrDefaultAsync("SELECT ChapterCode, SubjectId FROM tblContentIndexChapters WHERE ContentIndexId = @contentId AND IndexTypeId = @indexTypeId",
                        new { contentId = request.contentId, indexTypeId = request.indexTypeId });

                        contentCode = chapter?.ChapterCode;
                        //subjectId = chapter?.SubjectId ?? 0;
                    }
                    else if (request.indexTypeId == 2)
                    {
                        // Fetch Topic Data
                        var topic = await _connection.QueryFirstOrDefaultAsync("SELECT TopicCode FROM tblContentIndexTopics WHERE ContInIdTopic = @contentId",
                        new { contentId = request.contentId });

                        contentCode = topic?.TopicCode;
                        //subjectId = topic?.SubjectId ?? 0;
                    }
                    else if (request.indexTypeId == 3)
                    {
                        // Fetch SubTopic Data
                        var subTopic = await _connection.QueryFirstOrDefaultAsync("SELECT SubTopicCode FROM tblContentIndexSubTopics WHERE ContInIdSubTopic = @contentId",
                        new { contentId = request.contentId });

                        contentCode = subTopic?.SubTopicCode;
                        //subjectId = subTopic?.SubjectId ?? 0;
                    }


                    int maxOptions = 0;
                    int optionsToAdd = Math.Max(4, maxOptions);


                    // Create a worksheet for Questions
                    var worksheet = package.Workbook.Worksheets.Add("Questions");

                    // Add headers for Questions
                    worksheet.Cells[1, 1].Value = "CategoryId";
                    worksheet.Cells[1, 2].Value = "SubjectId";
                    worksheet.Cells[1, 3].Value = "Chapter/Concept/Sub-ConceptId";
                    worksheet.Cells[1, 4].Value = "QuestionTypeId";
                    worksheet.Cells[1, 5].Value = "ParagraphId";
                    worksheet.Cells[1, 6].Value = "ParagraphQuestionTypeId";
                    worksheet.Cells[1, 7].Value = "Question";
                    worksheet.Cells[1, 8].Value = "Answer";

                    worksheet.Cells[2, 1].Value = request.CategoryId;
                    worksheet.Cells[2, 2].Value = subjectId; // Fetched subjectId
                    worksheet.Cells[2, 3].Value = contentCode;
                    // Add dynamic columns for options starting from column 6
                    int optionStartIndex = 9;
                    for (int i = 0; i < optionsToAdd; i++)
                    {
                        worksheet.Cells[1, optionStartIndex + i].Value = $"Option{i + 1}";
                    }
                    int optionEndIndex = optionStartIndex + optionsToAdd; // End of dynamic option columns

                    // Add the fixed columns after the dynamic options
                    worksheet.Cells[1, optionEndIndex].Value = "Explanation";
                    worksheet.Cells[1, optionEndIndex + 1].Value = "Extra Information";

                    // Add dynamic course columns starting after "Extra Information"
                    var courses = (await _connection.QueryAsync<Course>("SELECT CourseId, CourseName FROM tblCourse"))
                        .ToDictionary(c => c.CourseId, c => c.CourseName);

                    int courseStartIndex = optionEndIndex + 2;
                    foreach (var course in courses)
                    {
                        worksheet.Cells[1, courseStartIndex].Value = course.Value; // course.Value is the CourseName
                        courseStartIndex++;
                    }

                    //// Leave 10 empty columns
                    //int questionCodeColumnIndex = courseStartIndex + 10;
                    //worksheet.Cells[1, questionCodeColumnIndex].Value = "QuestionCode";
                    //worksheet.Column(questionCodeColumnIndex).Hidden = true;

                    // Format headers
                    using (var range = worksheet.Cells[1, 1, 1, 1])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }
                    // Auto fit columns for better readability
                    worksheet.Cells.AutoFitColumns();

                    AddMasterDataSheets(package, request.subjectId);

                    // Convert the ExcelPackage to a byte array
                    var fileBytes = package.GetAsByteArray();

                    // Return the file as a response
                    return new ServiceResponse<byte[]>(true, "Excel file generated successfully", fileBytes, 200);
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                return new ServiceResponse<byte[]>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<string>> AddMatchThePairQuestion(MatchThePairRequest request)
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            try
            {
                string roleQuery = @"select r.RoleCode from  [tblEmployee] e 
                                                 LEFT JOIN [tblRole] r ON e.RoleID = r.RoleID
                                                 WHERE e.Employeeid = @EmployeeId";
                string GetRoleName = await _connection.QueryFirstOrDefaultAsync<string>(roleQuery, new { EmployeeId = request.ModifierId });

                if (request.EmployeeId != request.ModifierId && request.QuestionCode != "string" && request.QuestionCode != null && GetRoleName != "AD")
                {

                    int QuestionModifier = await _connection.QueryFirstOrDefaultAsync<int>(@"select ModifierId from tblQuestion where QuestionCode = @QuestionCode
                     and IsActive = 1", new { QuestionCode = request.QuestionCode });
                    if (QuestionModifier == request.ModifierId)
                    {
                        string updateQuery = @"
                UPDATE tblQuestion 
                SET 
                    QuestionTypeId = @QuestionTypeId, 
                    Status = @Status, 
                    ModifiedBy = @ModifiedBy, 
                    ModifiedOn = GETDATE(), 
                    EmployeeId = @EmployeeId, 
                    SubjectID = @SubjectID, 
                    IndexTypeId = @IndexTypeId, 
                    ContentIndexId = @ContentIndexId, 
                    ExamTypeId = @ExamTypeId, 
                    CategoryId = @CategoryId, 
                    IsRejected = @IsRejected, 
                    IsApproved = @IsApproved, 
                    Explanation = @Explanation, 
                    ExtraInformation = @ExtraInformation, 
                    IsActive = @IsActive, 
                    IsConfigure = @IsConfigure,
                    ModifierId = @ModifierId
                WHERE 
                    QuestionCode = @QuestionCode and IsActive = 1;";
                        var parameters = new
                        {
                            QuestionTypeId = request.QuestionTypeId,
                            Status = request.Status,
                            ModifiedBy = request.ModifiedBy,
                            ModifiedOn = request.ModifiedOn,
                            SubjectID = request.SubjectID,
                            EmployeeId = request.EmployeeId,
                            ModifierId = request.ModifierId,
                            IndexTypeId = request.IndexTypeId,
                            ContentIndexId = request.ContentIndexId,
                            QuestionCode = request.QuestionCode,
                            Explanation = request.Explanation,
                            ExtraInformation = request.ExtraInformation,
                            IsActive = request.IsActive,
                            IsConfigure = request.IsConfigure,
                            CategoryId = request.CategoryId,
                            request.ExamTypeId,
                            request.IsRejected,
                            request.IsApproved
                        };

                        int rowsAffected = _connection.Execute(updateQuery, parameters);
                        var insertedQuestionCode = request.QuestionCode;

                        // Handle QIDCourses mapping
                        var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);

                        var answer = await AnswerHandling(request.QuestionTypeId, request.AnswerMultipleChoiceCategories, request.QuestionId, insertedQuestionCode, null);
                        string updateMatchPairQuery = @"
                        UPDATE tblQuestionMatchThePair
                        SET 
                            PairValue = @PairValue,
                            PairColumn = @PairColumn,
                            PairRow = @PairRow
                        WHERE MatchThePairId = @MatchThePairId";

                        foreach (var pair in request.MatchPairs)
                        {
                            var rowsAffectedMatchPair = await _connection.ExecuteAsync(updateMatchPairQuery, new
                            {
                                MatchThePairId = pair.MatchThePairId,
                                PairColumn = pair.PairColumn,
                                PairRow = pair.PairRow,
                                PairValue = pair.PairValue
                            });

                            // Optional: Check if no rows were updated and log the information
                            if (rowsAffectedMatchPair == 0)
                            {
                                Console.WriteLine($"No record found for MatchThePairId: {pair.MatchThePairId}");
                            }
                        }
                        if (data > 0 && answer.Data > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Question updated Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                        }
                    }
                    else
                    {
                        int count = await _connection.QueryFirstOrDefaultAsync<int>(@"select count(*) from tblQuestionProfilerRejections where QuestionCode = @QuestionCode", new { QuestionCode = request.QuestionCode });
                        bool isRejectedQuestion = await _connection.QueryFirstOrDefaultAsync<bool>(@"select IsRejected from tblQuestion where QuestionCode = @QuestionCode and IsActive = 1", new { QuestionCode = request.QuestionCode });
                        bool isRejected = false;
                        if (count == 0 || !isRejectedQuestion)
                        {
                            isRejected = false;
                        }
                        else
                        {
                            isRejected = true;
                        }
                        // Step 1: Insert question into tblQuestion
                        string insertQuestionQuery = @"
           INSERT INTO tblQuestion 
(
    QuestionTypeId, 
    QuestionDescription,
    Status, 
    QuestionCode, 
    CreatedBy, 
    CreatedOn, 
    EmployeeId, 
    SubjectID, 
    IndexTypeId, 
    ContentIndexId, 
    ExamTypeId, 
    CategoryId, 
    IsRejected, 
    IsApproved, 
    Explanation, 
    ExtraInformation, 
    IsActive, 
    IsConfigure, ModifierId
)
VALUES 
(
    @QuestionTypeId, 
    'string',
    @Status, 
    @QuestionCode, 
    @CreatedBy, 
    GETDATE(), 
    @EmployeeId, 
    @SubjectID, 
    @IndexTypeId, 
    @ContentIndexId, 
    @ExamTypeId, 
    @CategoryId, 
    @IsRejected, 
    @IsApproved, 
    @Explanation, 
    @ExtraInformation, 
    @IsActive, 
    @IsConfigure, @ModifierId); SELECT CAST(SCOPE_IDENTITY() AS INT)";
                        string deactivateQuery = @"UPDATE tblQuestion SET IsActive = 0 WHERE QuestionCode = @QuestionCode AND IsActive = 1";
                        await _connection.ExecuteAsync(deactivateQuery, new { request.QuestionCode });
                        int insertedQuestionId = await _connection.ExecuteScalarAsync<int>(insertQuestionQuery, new
                        {
                            request.QuestionTypeId,
                            request.Status,
                            request.QuestionCode,
                            request.CreatedBy,
                            request.EmployeeId,
                            request.SubjectID,
                            request.IndexTypeId,
                            request.ContentIndexId,
                            request.ExamTypeId,
                            request.CategoryId,
                            IsRejected = isRejected,
                            request.IsApproved,
                            request.Explanation,
                            request.ExtraInformation,
                            IsActive = true,
                            IsConfigure = true,
                            request.ModifierId
                        });


                        string code = string.Empty;

                        if (string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string")
                        {
                            code = GenerateQuestionCode(request.IndexTypeId, request.ContentIndexId, insertedQuestionId);

                            string questionCodeQuery = @"
                UPDATE tblQuestion
                SET QuestionCode = @QuestionCode
                WHERE QuestionId = @QuestionId AND IsActive = 1";

                            await _connection.ExecuteAsync(questionCodeQuery, new { QuestionCode = code, QuestionId = insertedQuestionId });
                        }

                        string insertedQuestionCode = string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string" ? code : request.QuestionCode;
                        // Step 2: Insert data into tblQuestionMatchThePair

                        string insertMatchPairQuery = @"
            INSERT INTO tblQuestionMatchThePair (QuestionId, QuestionCode, PairColumn, PairRow, PairValue)
            VALUES (@QuestionId, @QuestionCode, @PairColumn, @PairRow, @PairValue)";

                        foreach (var pair in request.MatchPairs)
                        {
                            await _connection.ExecuteAsync(insertMatchPairQuery, new
                            {
                                QuestionId = insertedQuestionId,
                                QuestionCode = insertedQuestionCode,
                                PairColumn = pair.PairColumn,
                                PairRow = pair.PairRow,
                                PairValue = pair.PairValue
                            });
                        }
                        foreach (var data1 in request.QIDCourses)
                        {
                            data1.QIDCourseID = 0;
                        }
                        foreach (var data2 in request.AnswerMultipleChoiceCategories)
                        {
                            data2.Answermultiplechoicecategoryid = 0;
                        }
                        var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);

                        var answer = await AnswerHandling(request.QuestionTypeId, request.AnswerMultipleChoiceCategories, insertedQuestionId, insertedQuestionCode, null);
                        return new ServiceResponse<string>(true, "Match the Pair question added successfully", null, 200);
                    }
                }
                else if (request.EmployeeId == request.ModifierId && request.ModifierId > 0)
                {
                    var count1 = _connection.QueryFirstOrDefault<int>(@"select * from tblQuestionProfiler where QuestionCode = @QuestionCode "
                               , new { QuestionCode = request.QuestionCode, EmpId = request.ModifierId });
                    if (count1 > 0)
                    {
                        int count = await _connection.QueryFirstOrDefaultAsync<int>(@"select count(*) from tblQuestionProfilerRejections where QuestionCode = @QuestionCode", new { QuestionCode = request.QuestionCode });
                        //bool isRejectedQuestion = await _connection.QueryFirstOrDefaultAsync<bool>(@"select IsRejected from tblQuestion where QuestionCode = @QuestionCode and IsActive = 1", new { QuestionCode = request.QuestionCode });
                        bool isRejected = false;
                        if (count > 0)
                        {
                            isRejected = true;
                        }
                        // Step 1: Insert question into tblQuestion
                        string insertQuestionQuery = @"
           INSERT INTO tblQuestion 
(
    QuestionTypeId, 
    QuestionDescription,
    Status, 
    QuestionCode, 
    CreatedBy, 
    CreatedOn, 
    EmployeeId, 
    SubjectID, 
    IndexTypeId, 
    ContentIndexId, 
    ExamTypeId, 
    CategoryId, 
    IsRejected, 
    IsApproved, 
    Explanation, 
    ExtraInformation, 
    IsActive, 
    IsConfigure
)
VALUES 
(
    @QuestionTypeId, 
    'string',
    @Status, 
    @QuestionCode, 
    @CreatedBy, 
    GETDATE(), 
    @EmployeeId, 
    @SubjectID, 
    @IndexTypeId, 
    @ContentIndexId, 
    @ExamTypeId, 
    @CategoryId, 
    @IsRejected, 
    @IsApproved, 
    @Explanation, 
    @ExtraInformation, 
    @IsActive, 
    @IsConfigure); SELECT CAST(SCOPE_IDENTITY() AS INT)";
                        string deactivateQuery = @"UPDATE tblQuestion SET IsActive = 0 WHERE QuestionCode = @QuestionCode AND IsActive = 1";
                        await _connection.ExecuteAsync(deactivateQuery, new { request.QuestionCode });
                        int insertedQuestionId = await _connection.ExecuteScalarAsync<int>(insertQuestionQuery, new
                        {
                            request.QuestionTypeId,
                            request.Status,
                            request.QuestionCode,
                            request.CreatedBy,
                            request.EmployeeId,
                            request.SubjectID,
                            request.IndexTypeId,
                            request.ContentIndexId,
                            request.ExamTypeId,
                            request.CategoryId,
                            IsRejected = isRejected,
                            request.IsApproved,
                            request.Explanation,
                            request.ExtraInformation,
                            IsActive = true,
                            IsConfigure = true
                        });
                        string code = string.Empty;

                        if (string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string")
                        {
                            code = GenerateQuestionCode(request.IndexTypeId, request.ContentIndexId, insertedQuestionId);

                            string questionCodeQuery = @"
                UPDATE tblQuestion
                SET QuestionCode = @QuestionCode
                WHERE QuestionId = @QuestionId AND IsActive = 1";

                            await _connection.ExecuteAsync(questionCodeQuery, new { QuestionCode = code, QuestionId = insertedQuestionId });
                        }

                        string insertedQuestionCode = string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string" ? code : request.QuestionCode;
                        // Step 2: Insert data into tblQuestionMatchThePair
                        string insertMatchPairQuery = @"
            INSERT INTO tblQuestionMatchThePair (QuestionId, QuestionCode, PairColumn, PairRow, PairValue)
            VALUES (@QuestionId, @QuestionCode, @PairColumn, @PairRow, @PairValue)";

                        foreach (var pair in request.MatchPairs)
                        {
                            await _connection.ExecuteAsync(insertMatchPairQuery, new
                            {
                                QuestionId = insertedQuestionId,
                                QuestionCode = insertedQuestionCode,
                                PairColumn = pair.PairColumn,
                                PairRow = pair.PairRow,
                                PairValue = pair.PairValue
                            });
                        }
                        foreach (var data1 in request.QIDCourses)
                        {
                            data1.QIDCourseID = 0;
                        }
                        foreach (var data2 in request.AnswerMultipleChoiceCategories)
                        {
                            data2.Answermultiplechoicecategoryid = 0;
                        }
                        var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);

                        var answer = await AnswerHandling(request.QuestionTypeId, request.AnswerMultipleChoiceCategories, insertedQuestionId, insertedQuestionCode, null);
                        return new ServiceResponse<string>(true, "Match the Pair question added successfully", null, 200);
                    }
                    else
                    {
                        string updateQuery = @"
                UPDATE tblQuestion 
                SET 
                    QuestionTypeId = @QuestionTypeId, 
                    Status = @Status, 
                    ModifiedBy = @ModifiedBy, 
                    ModifiedOn = GETDATE(), 
                    EmployeeId = @EmployeeId, 
                    SubjectID = @SubjectID, 
                    IndexTypeId = @IndexTypeId, 
                    ContentIndexId = @ContentIndexId, 
                    ExamTypeId = @ExamTypeId, 
                    CategoryId = @CategoryId, 
                    IsRejected = @IsRejected, 
                    IsApproved = @IsApproved, 
                    Explanation = @Explanation, 
                    ExtraInformation = @ExtraInformation, 
                    IsActive = @IsActive, 
                    IsConfigure = @IsConfigure,
                    ModifierId = @ModifierId
                WHERE 
                    QuestionCode = @QuestionCode and IsActive = 1;";
                        var parameters = new
                        {
                            QuestionTypeId = request.QuestionTypeId,
                            Status = request.Status,
                            ModifiedBy = request.ModifiedBy,
                            ModifiedOn = request.ModifiedOn,
                            SubjectID = request.SubjectID,
                            EmployeeId = request.EmployeeId,
                            ModifierId = request.ModifierId,
                            IndexTypeId = request.IndexTypeId,
                            ContentIndexId = request.ContentIndexId,
                            QuestionCode = request.QuestionCode,
                            Explanation = request.Explanation,
                            ExtraInformation = request.ExtraInformation,
                            IsActive = request.IsActive,
                            IsConfigure = request.IsConfigure,
                            CategoryId = request.CategoryId,
                            request.ExamTypeId,
                            request.IsRejected,
                            request.IsApproved
                        };

                        int rowsAffected = _connection.Execute(updateQuery, parameters);
                        var insertedQuestionCode = request.QuestionCode;

                        // Handle QIDCourses mapping
                        var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);

                        var answer = await AnswerHandling(request.QuestionTypeId, request.AnswerMultipleChoiceCategories, request.QuestionId, insertedQuestionCode, null);
                        string updateMatchPairQuery = @"
                        UPDATE tblQuestionMatchThePair
                        SET 
                            PairValue = @PairValue,
                            PairColumn = @PairColumn,
                            PairRow = @PairRow
                        WHERE MatchThePairId = @MatchThePairId";

                        foreach (var pair in request.MatchPairs)
                        {
                            var rowsAffectedMatchPair = await _connection.ExecuteAsync(updateMatchPairQuery, new
                            {
                                MatchThePairId = pair.MatchThePairId,
                                PairColumn = pair.PairColumn,
                                PairRow = pair.PairRow,
                                PairValue = pair.PairValue
                            });

                            // Optional: Check if no rows were updated and log the information
                            if (rowsAffectedMatchPair == 0)
                            {
                                Console.WriteLine($"No record found for MatchThePairId: {pair.MatchThePairId}");
                            }
                        }
                        if (data > 0 && answer.Data > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Question updated Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                        }
                    }
                }
                else if (request.QuestionCode != null && request.QuestionCode != "string" && GetRoleName != "AD")
                {
                    return new ServiceResponse<string>(true, "Operation Successful", string.Empty, 200);
                }
                else if (GetRoleName == "AD")
                {
                    bool isLive = await _connection.QueryFirstOrDefaultAsync<bool>(@"select IsLive from tblQuestion where QuestionCode = @QuestionCode", new { QuestionCode = request.QuestionCode });
                    if (isLive)
                    {
                        string updateQuery = @"
                UPDATE tblQuestion 
                SET 
                    QuestionTypeId = @QuestionTypeId, 
                    Status = @Status, 
                    ModifiedBy = @ModifiedBy, 
                    ModifiedOn = GETDATE(), 
                    EmployeeId = @EmployeeId, 
                    SubjectID = @SubjectID, 
                    IndexTypeId = @IndexTypeId, 
                    ContentIndexId = @ContentIndexId, 
                    ExamTypeId = @ExamTypeId, 
                    CategoryId = @CategoryId, 
                    IsRejected = @IsRejected, 
                    IsApproved = @IsApproved, 
                    Explanation = @Explanation, 
                    ExtraInformation = @ExtraInformation, 
                    IsActive = @IsActive, 
                    IsConfigure = @IsConfigure,
                    ModifierId = @ModifierId,
                WHERE 
                    QuestionCode = @QuestionCode;";
                        var parameters = new
                        {
                            QuestionTypeId = request.QuestionTypeId,
                            Status = request.Status,
                            ModifiedBy = request.ModifiedBy,
                            ModifiedOn = request.ModifiedOn,
                            SubjectID = request.SubjectID,
                            EmployeeId = request.EmployeeId,
                            ModifierId = request.ModifierId,
                            IndexTypeId = request.IndexTypeId,
                            ContentIndexId = request.ContentIndexId,
                            QuestionCode = request.QuestionCode,
                            Explanation = request.Explanation,
                            ExtraInformation = request.ExtraInformation,
                            IsActive = request.IsActive,
                            IsConfigure = request.IsConfigure,
                            CategoryId = request.CategoryId
                        };

                        int rowsAffected = _connection.Execute(updateQuery, parameters);
                        var insertedQuestionCode = request.QuestionCode;

                        // Handle QIDCourses mapping
                        var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);

                        var answer = await AnswerHandling(request.QuestionTypeId, request.AnswerMultipleChoiceCategories, request.QuestionId, insertedQuestionCode, null);
                        string updateMatchPairQuery = @"
                        UPDATE tblQuestionMatchThePair
                        SET 
                            PairValue = @PairValue,
                            PairColumn = @PairColumn,
                            PairRow = @PairRow
                        WHERE MatchThePairId = @MatchThePairId";

                        foreach (var pair in request.MatchPairs)
                        {
                            var rowsAffectedMatchPair = await _connection.ExecuteAsync(updateMatchPairQuery, new
                            {
                                MatchThePairId = pair.MatchThePairId,
                                PairColumn = pair.PairColumn,
                                PairRow = pair.PairRow,
                                PairValue = pair.PairValue
                            });

                            // Optional: Check if no rows were updated and log the information
                            if (rowsAffectedMatchPair == 0)
                            {
                                Console.WriteLine($"No record found for MatchThePairId: {pair.MatchThePairId}");
                            }
                        }
                        if (data > 0 && answer.Data > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Question updated Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Cannot modify question", string.Empty, 500);
                    }
                }
                else
                {
                    // Step 1: Insert question into tblQuestion
                    string insertQuestionQuery = @"
           INSERT INTO tblQuestion 
(
    QuestionTypeId, 
    QuestionDescription,
    Status, 
    QuestionCode, 
    CreatedBy, 
    CreatedOn, 
    EmployeeId, 
    SubjectID, 
    IndexTypeId, 
    ContentIndexId, 
    ExamTypeId, 
    CategoryId, 
    IsRejected, 
    IsApproved, 
    Explanation, 
    ExtraInformation, 
    IsActive, 
    IsConfigure
)
VALUES 
(
    @QuestionTypeId,
    'string',
    @Status, 
    @QuestionCode, 
    @CreatedBy, 
    GETDATE(), 
    @EmployeeId, 
    @SubjectID, 
    @IndexTypeId, 
    @ContentIndexId, 
    @ExamTypeId, 
    @CategoryId, 
    @IsRejected, 
    @IsApproved, 
    @Explanation, 
    @ExtraInformation, 
    @IsActive, 
    @IsConfigure); SELECT CAST(SCOPE_IDENTITY() AS INT)";
                    string deactivateQuery = @"UPDATE tblQuestion SET IsActive = 0 WHERE QuestionCode = @QuestionCode AND IsActive = 1";
                    await _connection.ExecuteAsync(deactivateQuery, new { request.QuestionCode });
                    int insertedQuestionId = await _connection.ExecuteScalarAsync<int>(insertQuestionQuery, new
                    {
                        request.QuestionTypeId,
                        request.Status,
                        request.QuestionCode,
                        request.CreatedBy,
                        request.EmployeeId,
                        request.SubjectID,
                        request.IndexTypeId,
                        request.ContentIndexId,
                        request.ExamTypeId,
                        request.CategoryId,
                        IsRejected = false,
                        IsApproved = false,
                        request.Explanation,
                        request.ExtraInformation,
                        request.IsActive,
                        request.IsConfigure
                    });

                    string code = string.Empty;

                    if (string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string")
                    {
                        code = GenerateQuestionCode(request.IndexTypeId, request.ContentIndexId, insertedQuestionId);

                        string questionCodeQuery = @"
                UPDATE tblQuestion
                SET QuestionCode = @QuestionCode
                WHERE QuestionId = @QuestionId AND IsActive = 1";

                        await _connection.ExecuteAsync(questionCodeQuery, new { QuestionCode = code, QuestionId = insertedQuestionId });
                    }

                    string insertedQuestionCode = string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string" ? code : request.QuestionCode;
                    // Step 2: Insert data into tblQuestionMatchThePair
                    string insertMatchPairQuery = @"
            INSERT INTO tblQuestionMatchThePair (QuestionId, QuestionCode, PairColumn, PairRow, PairValue)
            VALUES (@QuestionId, @QuestionCode, @PairColumn, @PairRow, @PairValue)";

                    foreach (var pair in request.MatchPairs)
                    {
                        await _connection.ExecuteAsync(insertMatchPairQuery, new
                        {
                            QuestionId = insertedQuestionId,
                            QuestionCode = insertedQuestionCode,
                            PairColumn = pair.PairColumn,
                            PairRow = pair.PairRow,
                            PairValue = pair.PairValue
                        });
                    }

                    var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);

                    var answer = await AnswerHandling(request.QuestionTypeId, request.AnswerMultipleChoiceCategories, insertedQuestionId, insertedQuestionCode, null);
                    return new ServiceResponse<string>(true, "Match the Pair question added successfully", null, 200);
                }

            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, "An error occurred while adding the question: " + ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<string>> AddOrUpdateMatchThePairType2(MatchThePair2Request request)
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            try
            {
                string roleQuery = @"select r.RoleCode from  [tblEmployee] e 
                                                 LEFT JOIN [tblRole] r ON e.RoleID = r.RoleID
                                                 WHERE e.Employeeid = @EmployeeId";
                string GetRoleName = await _connection.QueryFirstOrDefaultAsync<string>(roleQuery, new { EmployeeId = request.ModifierId });

                if (request.EmployeeId != request.ModifierId && request.QuestionCode != "string" && request.QuestionCode != null && GetRoleName != "AD")
                {
                    int QuestionModifier = await _connection.QueryFirstOrDefaultAsync<int>(@"select ModifierId from tblQuestion where QuestionCode = @QuestionCode
                     and IsActive = 1", new { QuestionCode = request.QuestionCode });
                    if (QuestionModifier == request.ModifierId)
                    {
                        string updateQuery = @"
                UPDATE tblQuestion 
                SET 
                    QuestionTypeId = @QuestionTypeId, 
                    Status = @Status, 
                    ModifiedBy = @ModifiedBy, 
                    ModifiedOn = GETDATE(), 
                    EmployeeId = @EmployeeId, 
                    SubjectID = @SubjectID, 
                    IndexTypeId = @IndexTypeId, 
                    ContentIndexId = @ContentIndexId, 
                    ExamTypeId = @ExamTypeId, 
                    CategoryId = @CategoryId, 
                    IsRejected = @IsRejected, 
                    IsApproved = @IsApproved, 
                    Explanation = @Explanation, 
                    ExtraInformation = @ExtraInformation, 
                    IsActive = @IsActive, 
                    IsConfigure = @IsConfigure,
                    ModifierId = @ModifierId
                WHERE 
                    QuestionCode = @QuestionCode and IsActive = 1;";
                        var parameters = new
                        {
                            QuestionTypeId = request.QuestionTypeId,
                            Status = request.Status,
                            ModifiedBy = request.ModifiedBy,
                            ModifiedOn = request.ModifiedOn,
                            SubjectID = request.SubjectID,
                            EmployeeId = request.EmployeeId,
                            ModifierId = request.ModifierId,
                            IndexTypeId = request.IndexTypeId,
                            ContentIndexId = request.ContentIndexId,
                            QuestionCode = request.QuestionCode,
                            Explanation = request.Explanation,
                            ExtraInformation = request.ExtraInformation,
                            IsActive = request.IsActive,
                            IsConfigure = request.IsConfigure,
                            CategoryId = request.CategoryId,
                            request.ExamTypeId,
                            request.IsRejected,
                            request.IsApproved
                        };

                        int rowsAffected = _connection.Execute(updateQuery, parameters);
                        var insertedQuestionCode = request.QuestionCode;

                        // Handle QIDCourses mapping
                        var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);
                        string updateMatchPairQuery = @"
                        UPDATE tblQuestionMatchThePair
                        SET 
                            PairValue = @PairValue,
                            PairColumn = @PairColumn,
                            PairRow = @PairRow
                        WHERE MatchThePairId = @MatchThePairId";

                        foreach (var pair in request.MatchPairs)
                        {
                            var rowsAffectedMatchPair = await _connection.ExecuteAsync(updateMatchPairQuery, new
                            {
                                MatchThePairId = pair.MatchThePairId,
                                PairColumn = pair.PairColumn,
                                PairRow = pair.PairRow,
                                PairValue = pair.PairValue
                            });

                            // Optional: Check if no rows were updated and log the information
                            if (rowsAffectedMatchPair == 0)
                            {
                                Console.WriteLine($"No record found for MatchThePairId: {pair.MatchThePairId}");
                            }
                        }

                        string getQuesType = @"SELECT * FROM tblQBQuestionType WHERE QuestionTypeID = @QuestionTypeID;";
                        var questTypedata = await _connection.QueryFirstOrDefaultAsync<QuestionTypes>(getQuesType, new { QuestionTypeID = request.QuestionTypeId });

                        int answer = 0;
                        int Answerid = 0;

                        // Check if the answer already exists in AnswerMaster
                        string getAnswerQuery = @"SELECT Answerid FROM tblAnswerMaster WHERE QuestionCode = @QuestionCode AND QuestionId = @QuestionId;";
                        Answerid = await _connection.QueryFirstOrDefaultAsync<int>(getAnswerQuery, new { request.QuestionCode, request.QuestionId });

                        if (Answerid == 0) // If no entry exists, insert a new one
                        {
                            string insertAnswerMasterQuery = @"
            INSERT INTO [tblAnswerMaster] (Questionid, QuestionTypeid, QuestionCode)
            VALUES (@Questionid, @QuestionTypeid, @QuestionCode);
            SELECT CAST(SCOPE_IDENTITY() as int);";

                            Answerid = await _connection.QuerySingleAsync<int>(insertAnswerMasterQuery, new
                            {
                                Questionid = request.QuestionId,
                                QuestionTypeid = questTypedata?.QuestionTypeID,
                                QuestionCode = request.QuestionCode
                            });
                        }
                        string updateMatchPairQuery1 = @"
        UPDATE tblOptionsMatchThePair2
        SET 
            PairColumn = @PairColumn,
            PairRow = @PairRow
        WHERE MatchThePair2Id = @MatchThePair2Id";

                        foreach (var answerDetail in request.MatchThePairAnswers)
                        {
                            var rowsAffectedMatchPair = await _connection.ExecuteAsync(updateMatchPairQuery1, new
                            {
                                MatchThePair2Id = answerDetail.MatchThePair2Id,
                                PairColumn = answerDetail.PairColumn,
                                PairRow = answerDetail.PairRow
                            });

                            if (rowsAffectedMatchPair == 0)
                            {
                                Console.WriteLine($"No record found for MatchThePair2Id: {answerDetail.MatchThePair2Id}");
                            }
                        }
                        if (data > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Question updated Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                        }
                    }
                    else
                    {
                        int count = await _connection.QueryFirstOrDefaultAsync<int>(@"select count(*) from tblQuestionProfilerRejections where QuestionCode = @QuestionCode", new { QuestionCode = request.QuestionCode });
                        bool isRejectedQuestion = await _connection.QueryFirstOrDefaultAsync<bool>(@"select IsRejected from tblQuestion where QuestionCode = @QuestionCode and IsActive = 1", new { QuestionCode = request.QuestionCode });
                        bool isRejected = false;
                        if (count == 0 || !isRejectedQuestion)
                        {
                            isRejected = false;
                        }
                        else
                        {
                            isRejected = true;
                        }
                        // Step 1: Insert question into tblQuestion
                        string insertQuestionQuery = @"
           INSERT INTO tblQuestion 
(
    QuestionTypeId, 
    QuestionDescription,
    Status, 
    QuestionCode, 
    CreatedBy, 
    CreatedOn, 
    EmployeeId, 
    SubjectID, 
    IndexTypeId, 
    ContentIndexId, 
    ExamTypeId, 
    CategoryId, 
    IsRejected, 
    IsApproved, 
    Explanation, 
    ExtraInformation, 
    IsActive, 
    IsConfigure
)
VALUES 
(
    @QuestionTypeId,
    'string',
    @Status, 
    @QuestionCode, 
    @CreatedBy, 
    GETDATE(), 
    @EmployeeId, 
    @SubjectID, 
    @IndexTypeId, 
    @ContentIndexId, 
    @ExamTypeId, 
    @CategoryId, 
    @IsRejected, 
    @IsApproved, 
    @Explanation, 
    @ExtraInformation, 
    @IsActive, 
    @IsConfigure)  SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        string deactivateQuery = @"UPDATE tblQuestion SET IsActive = 0 WHERE QuestionCode = @QuestionCode AND IsActive = 1";
                        await _connection.ExecuteAsync(deactivateQuery, new { request.QuestionCode });
                        int insertedQuestionId = await _connection.ExecuteScalarAsync<int>(insertQuestionQuery, new
                        {
                            request.QuestionTypeId,
                            request.Status,
                            request.QuestionCode,
                            request.CreatedBy,
                            request.EmployeeId,
                            request.SubjectID,
                            request.IndexTypeId,
                            request.ContentIndexId,
                            request.ExamTypeId,
                            request.CategoryId,
                            IsRejected = isRejected,
                            request.IsApproved,
                            request.Explanation,
                            request.ExtraInformation,
                            request.IsActive,
                            request.IsConfigure
                        });


                        string code = string.Empty;
                        if (string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string")
                        {
                            code = GenerateQuestionCode(request.IndexTypeId, request.ContentIndexId, insertedQuestionId);

                            string questionCodeQuery = @"
                UPDATE tblQuestion
                SET QuestionCode = @QuestionCode
                WHERE QuestionId = @QuestionId AND IsActive = 1";

                            await _connection.ExecuteAsync(questionCodeQuery, new { QuestionCode = code, QuestionId = insertedQuestionId });
                        }
                        string insertedQuestionCode = string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string" ? code : request.QuestionCode;
                        // Step 2: Insert data into tblQuestionMatchThePair
                        string insertMatchPairQuery = @"
            INSERT INTO tblQuestionMatchThePair (QuestionId, QuestionCode, PairColumn, PairRow, PairValue)
            VALUES (@QuestionId, @QuestionCode, @PairColumn, @PairRow, @PairValue)";

                        foreach (var pair in request.MatchPairs)
                        {
                            await _connection.ExecuteAsync(insertMatchPairQuery, new
                            {
                                QuestionId = insertedQuestionId,
                                QuestionCode = insertedQuestionCode,
                                PairColumn = pair.PairColumn,
                                PairRow = pair.PairRow,
                                PairValue = pair.PairValue
                            });
                        }

                        foreach (var record in request.QIDCourses)
                        {
                            record.QIDCourseID = 0;
                        }
                        var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);
                        string getQuesType = @"SELECT * FROM tblQBQuestionType WHERE QuestionTypeID = @QuestionTypeID;";
                        var questTypedata = await _connection.QueryFirstOrDefaultAsync<QuestionTypes>(getQuesType, new { QuestionTypeID = request.QuestionTypeId });

                        int answer = 0;
                        int Answerid = 0;

                        // Check if the answer already exists in AnswerMaster
                        string getAnswerQuery = @"SELECT Answerid FROM tblAnswerMaster WHERE QuestionCode = @QuestionCode AND QuestionId = @QuestionId;";
                        Answerid = await _connection.QueryFirstOrDefaultAsync<int>(getAnswerQuery, new { QuestionCode = insertedQuestionCode, QuestionId = insertedQuestionId });

                        if (Answerid == 0) // If no entry exists, insert a new one
                        {
                            string insertAnswerMasterQuery = @"
            INSERT INTO [tblAnswerMaster] (Questionid, QuestionTypeid, QuestionCode)
            VALUES (@Questionid, @QuestionTypeid, @QuestionCode);
            SELECT CAST(SCOPE_IDENTITY() as int);";

                            Answerid = await _connection.QuerySingleAsync<int>(insertAnswerMasterQuery, new
                            {
                                Questionid = insertedQuestionId,
                                QuestionTypeid = questTypedata?.QuestionTypeID,
                                QuestionCode = insertedQuestionCode
                            });
                        }

                        string insertAnswerQuery = @"
            INSERT INTO tblOptionsMatchThePair2 (AnswerId, PairColumn, PairRow)
            VALUES (@AnswerId, @PairColumn, @PairRow)";

                        foreach (var answerdetail in request.MatchThePairAnswers)
                        {
                            await _connection.ExecuteAsync(insertAnswerQuery, new
                            {
                                AnswerId = Answerid,
                                answerdetail.PairColumn,
                                answerdetail.PairRow
                            });
                        }
                        return new ServiceResponse<string>(true, "Match the Pair question added successfully", null, 200);
                    }
                }
                else if (request.EmployeeId == request.ModifierId && request.ModifierId > 0)
                {
                    var count1 = _connection.QueryFirstOrDefault<int>(@"select * from tblQuestionProfiler where QuestionCode = @QuestionCode "
                             , new { QuestionCode = request.QuestionCode, EmpId = request.ModifierId });
                    if (count1 > 0)
                    {
                        int count = await _connection.QueryFirstOrDefaultAsync<int>(@"select count(*) from tblQuestionProfilerRejections where QuestionCode = @QuestionCode", new { QuestionCode = request.QuestionCode });
                        //bool isRejectedQuestion = await _connection.QueryFirstOrDefaultAsync<bool>(@"select IsRejected from tblQuestion where QuestionCode = @QuestionCode and IsActive = 1", new { QuestionCode = request.QuestionCode });
                        bool isRejected = false;
                        if (count > 0)
                        {
                            isRejected = true;
                        }
                        // Step 1: Insert question into tblQuestion
                        string insertQuestionQuery = @"
           INSERT INTO tblQuestion 
(
    QuestionTypeId, 
    QuestionDescription,
    Status, 
    QuestionCode, 
    CreatedBy, 
    CreatedOn, 
    EmployeeId, 
    SubjectID, 
    IndexTypeId, 
    ContentIndexId, 
    ExamTypeId, 
    CategoryId, 
    IsRejected, 
    IsApproved, 
    Explanation, 
    ExtraInformation, 
    IsActive, 
    IsConfigure
)
VALUES 
(
    @QuestionTypeId,
    'string',
    @Status, 
    @QuestionCode, 
    @CreatedBy, 
    GETDATE(), 
    @EmployeeId, 
    @SubjectID, 
    @IndexTypeId, 
    @ContentIndexId, 
    @ExamTypeId, 
    @CategoryId, 
    @IsRejected, 
    @IsApproved, 
    @Explanation, 
    @ExtraInformation, 
    @IsActive, 
    @IsConfigure)  SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        string deactivateQuery = @"UPDATE tblQuestion SET IsActive = 0 WHERE QuestionCode = @QuestionCode AND IsActive = 1";
                        await _connection.ExecuteAsync(deactivateQuery, new { request.QuestionCode });
                        int insertedQuestionId = await _connection.ExecuteScalarAsync<int>(insertQuestionQuery, new
                        {
                            request.QuestionTypeId,
                            request.Status,
                            request.QuestionCode,
                            request.CreatedBy,
                            request.EmployeeId,
                            request.SubjectID,
                            request.IndexTypeId,
                            request.ContentIndexId,
                            request.ExamTypeId,
                            request.CategoryId,
                            IsRejected = isRejected,
                            request.IsApproved,
                            request.Explanation,
                            request.ExtraInformation,
                            request.IsActive,
                            request.IsConfigure
                        });


                        string code = string.Empty;
                        if (string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string")
                        {
                            code = GenerateQuestionCode(request.IndexTypeId, request.ContentIndexId, insertedQuestionId);

                            string questionCodeQuery = @"
                UPDATE tblQuestion
                SET QuestionCode = @QuestionCode
                WHERE QuestionId = @QuestionId AND IsActive = 1";

                            await _connection.ExecuteAsync(questionCodeQuery, new { QuestionCode = code, QuestionId = insertedQuestionId });
                        }
                        string insertedQuestionCode = string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string" ? code : request.QuestionCode;
                        // Step 2: Insert data into tblQuestionMatchThePair
                        string insertMatchPairQuery = @"
            INSERT INTO tblQuestionMatchThePair (QuestionId, QuestionCode, PairColumn, PairRow, PairValue)
            VALUES (@QuestionId, @QuestionCode, @PairColumn, @PairRow, @PairValue)";

                        foreach (var pair in request.MatchPairs)
                        {
                            await _connection.ExecuteAsync(insertMatchPairQuery, new
                            {
                                QuestionId = insertedQuestionId,
                                QuestionCode = insertedQuestionCode,
                                PairColumn = pair.PairColumn,
                                PairRow = pair.PairRow,
                                PairValue = pair.PairValue
                            });
                        }

                        foreach (var record in request.QIDCourses)
                        {
                            record.QIDCourseID = 0;
                        }
                        var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);
                        string getQuesType = @"SELECT * FROM tblQBQuestionType WHERE QuestionTypeID = @QuestionTypeID;";
                        var questTypedata = await _connection.QueryFirstOrDefaultAsync<QuestionTypes>(getQuesType, new { QuestionTypeID = request.QuestionTypeId });

                        int answer = 0;
                        int Answerid = 0;

                        // Check if the answer already exists in AnswerMaster
                        string getAnswerQuery = @"SELECT Answerid FROM tblAnswerMaster WHERE QuestionCode = @QuestionCode AND QuestionId = @QuestionId;";
                        Answerid = await _connection.QueryFirstOrDefaultAsync<int>(getAnswerQuery, new { QuestionCode = insertedQuestionCode, QuestionId = insertedQuestionId });

                        if (Answerid == 0) // If no entry exists, insert a new one
                        {
                            string insertAnswerMasterQuery = @"
            INSERT INTO [tblAnswerMaster] (Questionid, QuestionTypeid, QuestionCode)
            VALUES (@Questionid, @QuestionTypeid, @QuestionCode);
            SELECT CAST(SCOPE_IDENTITY() as int);";

                            Answerid = await _connection.QuerySingleAsync<int>(insertAnswerMasterQuery, new
                            {
                                Questionid = insertedQuestionId,
                                QuestionTypeid = questTypedata?.QuestionTypeID,
                                QuestionCode = insertedQuestionCode
                            });
                        }

                        string insertAnswerQuery = @"
            INSERT INTO tblOptionsMatchThePair2 (AnswerId, PairColumn, PairRow)
            VALUES (@AnswerId, @PairColumn, @PairRow)";

                        foreach (var answerdetail in request.MatchThePairAnswers)
                        {
                            await _connection.ExecuteAsync(insertAnswerQuery, new
                            {
                                AnswerId = Answerid,
                                answerdetail.PairColumn,
                                answerdetail.PairRow
                            });
                        }
                        return new ServiceResponse<string>(true, "Match the Pair question added successfully", null, 200);
                    }
                    else
                    {
                        string updateQuery = @"
                UPDATE tblQuestion 
                SET 
                    QuestionTypeId = @QuestionTypeId, 
                    Status = @Status, 
                    ModifiedBy = @ModifiedBy, 
                    ModifiedOn = GETDATE(), 
                    EmployeeId = @EmployeeId, 
                    SubjectID = @SubjectID, 
                    IndexTypeId = @IndexTypeId, 
                    ContentIndexId = @ContentIndexId, 
                    ExamTypeId = @ExamTypeId, 
                    CategoryId = @CategoryId, 
                    IsRejected = @IsRejected, 
                    IsApproved = @IsApproved, 
                    Explanation = @Explanation, 
                    ExtraInformation = @ExtraInformation, 
                    IsActive = @IsActive, 
                    IsConfigure = @IsConfigure,
                    ModifierId = @ModifierId
                WHERE 
                    QuestionCode = @QuestionCode and IsActive = 1;";
                        var parameters = new
                        {
                            QuestionTypeId = request.QuestionTypeId,
                            Status = request.Status,
                            ModifiedBy = request.ModifiedBy,
                            ModifiedOn = request.ModifiedOn,
                            SubjectID = request.SubjectID,
                            EmployeeId = request.EmployeeId,
                            ModifierId = request.ModifierId,
                            IndexTypeId = request.IndexTypeId,
                            ContentIndexId = request.ContentIndexId,
                            QuestionCode = request.QuestionCode,
                            Explanation = request.Explanation,
                            ExtraInformation = request.ExtraInformation,
                            IsActive = request.IsActive,
                            IsConfigure = request.IsConfigure,
                            CategoryId = request.CategoryId,
                            request.ExamTypeId,
                            request.IsRejected,
                            request.IsApproved
                        };

                        int rowsAffected = _connection.Execute(updateQuery, parameters);
                        var insertedQuestionCode = request.QuestionCode;

                        // Handle QIDCourses mapping
                        var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);
                        string updateMatchPairQuery = @"
                        UPDATE tblQuestionMatchThePair
                        SET 
                            PairValue = @PairValue,
                            PairColumn = @PairColumn,
                            PairRow = @PairRow
                        WHERE MatchThePairId = @MatchThePairId";

                        foreach (var pair in request.MatchPairs)
                        {
                            var rowsAffectedMatchPair = await _connection.ExecuteAsync(updateMatchPairQuery, new
                            {
                                MatchThePairId = pair.MatchThePairId,
                                PairColumn = pair.PairColumn,
                                PairRow = pair.PairRow,
                                PairValue = pair.PairValue
                            });

                            // Optional: Check if no rows were updated and log the information
                            if (rowsAffectedMatchPair == 0)
                            {
                                Console.WriteLine($"No record found for MatchThePairId: {pair.MatchThePairId}");
                            }
                        }

                        string getQuesType = @"SELECT * FROM tblQBQuestionType WHERE QuestionTypeID = @QuestionTypeID;";
                        var questTypedata = await _connection.QueryFirstOrDefaultAsync<QuestionTypes>(getQuesType, new { QuestionTypeID = request.QuestionTypeId });

                        int answer = 0;
                        int Answerid = 0;

                        // Check if the answer already exists in AnswerMaster
                        string getAnswerQuery = @"SELECT Answerid FROM tblAnswerMaster WHERE QuestionCode = @QuestionCode AND QuestionId = @QuestionId;";
                        Answerid = await _connection.QueryFirstOrDefaultAsync<int>(getAnswerQuery, new { request.QuestionCode, request.QuestionId });

                        if (Answerid == 0) // If no entry exists, insert a new one
                        {
                            string insertAnswerMasterQuery = @"
            INSERT INTO [tblAnswerMaster] (Questionid, QuestionTypeid, QuestionCode)
            VALUES (@Questionid, @QuestionTypeid, @QuestionCode);
            SELECT CAST(SCOPE_IDENTITY() as int);";

                            Answerid = await _connection.QuerySingleAsync<int>(insertAnswerMasterQuery, new
                            {
                                Questionid = request.QuestionId,
                                QuestionTypeid = questTypedata?.QuestionTypeID,
                                QuestionCode = request.QuestionCode
                            });
                        }
                        string updateMatchPairQuery1 = @"
        UPDATE tblOptionsMatchThePair2
        SET 
            PairColumn = @PairColumn,
            PairRow = @PairRow
        WHERE MatchThePair2Id = @MatchThePair2Id";

                        foreach (var answerDetail in request.MatchThePairAnswers)
                        {
                            var rowsAffectedMatchPair = await _connection.ExecuteAsync(updateMatchPairQuery1, new
                            {
                                MatchThePair2Id = answerDetail.MatchThePair2Id,
                                PairColumn = answerDetail.PairColumn,
                                PairRow = answerDetail.PairRow
                            });

                            if (rowsAffectedMatchPair == 0)
                            {
                                Console.WriteLine($"No record found for MatchThePair2Id: {answerDetail.MatchThePair2Id}");
                            }
                        }
                        if (data > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Question updated Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                        }
                    }
                }
                else if (request.QuestionCode != null && request.QuestionCode != "string" && GetRoleName != "AD")
                {
                    return new ServiceResponse<string>(true, "Operation Successful", string.Empty, 200);
                }
                else if (GetRoleName == "AD")
                {
                    bool isLive = await _connection.QueryFirstOrDefaultAsync<bool>(@"select IsLive from tblQuestion where QuestionCode = @QuestionCode", new { QuestionCode = request.QuestionCode });
                    if (isLive)
                    {
                        string updateQuery = @"
                UPDATE tblQuestion 
                SET 
                    QuestionTypeId = @QuestionTypeId, 
                    Status = @Status, 
                    ModifiedBy = @ModifiedBy, 
                    ModifiedOn = GETDATE(), 
                    EmployeeId = @EmployeeId, 
                    SubjectID = @SubjectID, 
                    IndexTypeId = @IndexTypeId, 
                    ContentIndexId = @ContentIndexId, 
                    ExamTypeId = @ExamTypeId, 
                    CategoryId = @CategoryId, 
                    IsRejected = @IsRejected, 
                    IsApproved = @IsApproved, 
                    Explanation = @Explanation, 
                    ExtraInformation = @ExtraInformation, 
                    IsActive = @IsActive, 
                    IsConfigure = @IsConfigure,
                    ModifierId = @ModifierId
                WHERE 
                    QuestionCode = @QuestionCode and IsActive = 1;";
                        var parameters = new
                        {
                            QuestionTypeId = request.QuestionTypeId,
                            Status = request.Status,
                            ModifiedBy = request.ModifiedBy,
                            ModifiedOn = request.ModifiedOn,
                            SubjectID = request.SubjectID,
                            EmployeeId = request.EmployeeId,
                            ModifierId = request.ModifierId,
                            IndexTypeId = request.IndexTypeId,
                            ContentIndexId = request.ContentIndexId,
                            QuestionCode = request.QuestionCode,
                            Explanation = request.Explanation,
                            ExtraInformation = request.ExtraInformation,
                            IsActive = request.IsActive,
                            IsConfigure = request.IsConfigure,
                            CategoryId = request.CategoryId,
                            request.ExamTypeId,
                            request.IsRejected,
                            request.IsApproved
                        };

                        int rowsAffected = _connection.Execute(updateQuery, parameters);
                        var insertedQuestionCode = request.QuestionCode;

                        // Handle QIDCourses mapping
                        var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);
                        string updateMatchPairQuery = @"
                        UPDATE tblQuestionMatchThePair
                        SET 
                            PairValue = @PairValue,
                            PairColumn = @PairColumn,
                            PairRow = @PairRow
                        WHERE MatchThePairId = @MatchThePairId";

                        foreach (var pair in request.MatchPairs)
                        {
                            var rowsAffectedMatchPair = await _connection.ExecuteAsync(updateMatchPairQuery, new
                            {
                                MatchThePairId = pair.MatchThePairId,
                                PairColumn = pair.PairColumn,
                                PairRow = pair.PairRow,
                                PairValue = pair.PairValue
                            });

                            // Optional: Check if no rows were updated and log the information
                            if (rowsAffectedMatchPair == 0)
                            {
                                Console.WriteLine($"No record found for MatchThePairId: {pair.MatchThePairId}");
                            }
                        }

                        string getQuesType = @"SELECT * FROM tblQBQuestionType WHERE QuestionTypeID = @QuestionTypeID;";
                        var questTypedata = await _connection.QueryFirstOrDefaultAsync<QuestionTypes>(getQuesType, new { QuestionTypeID = request.QuestionTypeId });

                        int answer = 0;
                        int Answerid = 0;

                        // Check if the answer already exists in AnswerMaster
                        string getAnswerQuery = @"SELECT Answerid FROM tblAnswerMaster WHERE QuestionCode = @QuestionCode AND QuestionId = @QuestionId;";
                        Answerid = await _connection.QueryFirstOrDefaultAsync<int>(getAnswerQuery, new { request.QuestionCode, request.QuestionId });

                        if (Answerid == 0) // If no entry exists, insert a new one
                        {
                            string insertAnswerMasterQuery = @"
            INSERT INTO [tblAnswerMaster] (Questionid, QuestionTypeid, QuestionCode)
            VALUES (@Questionid, @QuestionTypeid, @QuestionCode);
            SELECT CAST(SCOPE_IDENTITY() as int);";

                            Answerid = await _connection.QuerySingleAsync<int>(insertAnswerMasterQuery, new
                            {
                                Questionid = request.QuestionId,
                                QuestionTypeid = questTypedata?.QuestionTypeID,
                                QuestionCode = request.QuestionCode
                            });
                        }
                        string updateMatchPairQuery1 = @"
        UPDATE tblOptionsMatchThePair2
        SET 
            PairColumn = @PairColumn,
            PairRow = @PairRow
        WHERE MatchThePair2Id = @MatchThePair2Id";

                        foreach (var answerDetail in request.MatchThePairAnswers)
                        {
                            var rowsAffectedMatchPair = await _connection.ExecuteAsync(updateMatchPairQuery1, new
                            {
                                MatchThePair2Id = answerDetail.MatchThePair2Id,
                                PairColumn = answerDetail.PairColumn,
                                PairRow = answerDetail.PairRow
                            });

                            if (rowsAffectedMatchPair == 0)
                            {
                                Console.WriteLine($"No record found for MatchThePair2Id: {answerDetail.MatchThePair2Id}");
                            }
                        }
                        if (data > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Question updated Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Cannot modify question", string.Empty, 500);
                    }
                }
                else
                {
                    // Step 1: Insert question into tblQuestion
                    string insertQuestionQuery = @"
           INSERT INTO tblQuestion 
(
    QuestionTypeId, 
    QuestionDescription,
    Status, 
    QuestionCode, 
    CreatedBy, 
    CreatedOn, 
    EmployeeId, 
    SubjectID, 
    IndexTypeId, 
    ContentIndexId, 
    ExamTypeId, 
    CategoryId, 
    IsRejected, 
    IsApproved, 
    Explanation, 
    ExtraInformation, 
    IsActive, 
    IsConfigure
)
VALUES 
(
    @QuestionTypeId, 
    'string',
    @Status, 
    @QuestionCode, 
    @CreatedBy, 
    GETDATE(), 
    @EmployeeId, 
    @SubjectID, 
    @IndexTypeId, 
    @ContentIndexId, 
    @ExamTypeId, 
    @CategoryId, 
    @IsRejected, 
    @IsApproved, 
    @Explanation, 
    @ExtraInformation, 
    @IsActive, 
    @IsConfigure)  SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    string deactivateQuery = @"UPDATE tblQuestion SET IsActive = 0 WHERE QuestionCode = @QuestionCode AND IsActive = 1";
                    await _connection.ExecuteAsync(deactivateQuery, new { request.QuestionCode });
                    int insertedQuestionId = await _connection.ExecuteScalarAsync<int>(insertQuestionQuery, new
                    {
                        request.QuestionTypeId,
                        request.Status,
                        request.QuestionCode,
                        request.CreatedBy,
                        request.EmployeeId,
                        request.SubjectID,
                        request.IndexTypeId,
                        request.ContentIndexId,
                        request.ExamTypeId,
                        request.CategoryId,
                        IsRejected = false,
                        IsApproved = false,
                        request.Explanation,
                        request.ExtraInformation,
                        request.IsActive,
                        request.IsConfigure
                    });

                    string code = string.Empty;
                    if (string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string")
                    {
                        code = GenerateQuestionCode(request.IndexTypeId, request.ContentIndexId, insertedQuestionId);

                        string questionCodeQuery = @"
                UPDATE tblQuestion
                SET QuestionCode = @QuestionCode
                WHERE QuestionId = @QuestionId AND IsActive = 1";

                        await _connection.ExecuteAsync(questionCodeQuery, new { QuestionCode = code, QuestionId = insertedQuestionId });
                    }

                    // Step 2: Insert data into tblQuestionMatchThePair
                    string insertMatchPairQuery = @"
            INSERT INTO tblQuestionMatchThePair (QuestionId, QuestionCode, PairColumn, PairRow, PairValue)
            VALUES (@QuestionId, @QuestionCode, @PairColumn, @PairRow, @PairValue)";

                    foreach (var pair in request.MatchPairs)
                    {
                        await _connection.ExecuteAsync(insertMatchPairQuery, new
                        {
                            QuestionId = insertedQuestionId,
                            QuestionCode = code,
                            PairColumn = pair.PairColumn,
                            PairRow = pair.PairRow,
                            PairValue = pair.PairValue
                        });
                    }

                    foreach (var record in request.QIDCourses)
                    {
                        record.QIDCourseID = 0;
                    }
                    var data = await AddUpdateQIDCourses(request.QIDCourses, code);
                    string getQuesType = @"SELECT * FROM tblQBQuestionType WHERE QuestionTypeID = @QuestionTypeID;";
                    var questTypedata = await _connection.QueryFirstOrDefaultAsync<QuestionTypes>(getQuesType, new { QuestionTypeID = request.QuestionTypeId });

                    int answer = 0;
                    int Answerid = 0;

                    // Check if the answer already exists in AnswerMaster
                    string getAnswerQuery = @"SELECT Answerid FROM tblAnswerMaster WHERE QuestionCode = @QuestionCode AND QuestionId = @QuestionId;";
                    Answerid = await _connection.QueryFirstOrDefaultAsync<int>(getAnswerQuery, new { QuestionCode = code, QuestionId = insertedQuestionId });

                    if (Answerid == 0) // If no entry exists, insert a new one
                    {
                        string insertAnswerMasterQuery = @"
            INSERT INTO [tblAnswerMaster] (Questionid, QuestionTypeid, QuestionCode)
            VALUES (@Questionid, @QuestionTypeid, @QuestionCode);
            SELECT CAST(SCOPE_IDENTITY() as int);";

                        Answerid = await _connection.QuerySingleAsync<int>(insertAnswerMasterQuery, new
                        {
                            Questionid = insertedQuestionId,
                            QuestionTypeid = questTypedata?.QuestionTypeID,
                            QuestionCode = code
                        });
                    }

                    string insertAnswerQuery = @"
            INSERT INTO tblOptionsMatchThePair2 (AnswerId, PairColumn, PairRow)
            VALUES (@AnswerId, @PairColumn, @PairRow)";

                    foreach (var answerdetail in request.MatchThePairAnswers)
                    {
                        await _connection.ExecuteAsync(insertAnswerQuery, new
                        {
                            AnswerId = Answerid,
                            answerdetail.PairColumn,
                            answerdetail.PairRow
                        });
                    }
                    return new ServiceResponse<string>(true, "Match the Pair question added successfully", null, 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, "An error occurred while saving Match the Pair Type 2.", ex.Message, 500);
            }
        }
        private void AddMasterDataSheets(ExcelPackage package, int subjectId)
        {
            // Create worksheets for master data
            var subjectWorksheet = package.Workbook.Worksheets.Add("Subjects");
            var contentWorksheet = package.Workbook.Worksheets.Add("Content");
            var difficultyLevelWorksheet = package.Workbook.Worksheets.Add("Difficulty Levels");
            var questionTypeWorksheet = package.Workbook.Worksheets.Add("Question Types");
            var coursesWorksheet = package.Workbook.Worksheets.Add("Courses");
            var categoryWorksheet = package.Workbook.Worksheets.Add("Category");
            var paragraphWorksheet = package.Workbook.Worksheets.Add("Paragraph");

            categoryWorksheet.Cells[1, 1].Value = "CategoryId";
            categoryWorksheet.Cells[1, 2].Value = "Category";

            var category = _connection.Query<dynamic>(@"select * from tblCategory where Status = 1");
            int categoryRow = 2;
            foreach (var data in category)
            {
                categoryWorksheet.Cells[categoryRow, 1].Value = data.APId;
                categoryWorksheet.Cells[categoryRow, 2].Value = data.APName;
                categoryRow++;
            }
            // Populate data for Subjects
            subjectWorksheet.Cells[1, 1].Value = "SubjectId";
            subjectWorksheet.Cells[1, 2].Value = "SubjectCode";
            subjectWorksheet.Cells[1, 3].Value = "SubjectName";

            var subjects = GetSubjects(subjectId);
            int subjectRow = 2;
            foreach (var subject in subjects)
            {
                subjectWorksheet.Cells[subjectRow, 1].Value = subject.SubjectId;
                subjectWorksheet.Cells[subjectRow, 2].Value = subject.SubjectCode;
                subjectWorksheet.Cells[subjectRow, 3].Value = subject.SubjectName;
                subjectRow++;
            }

            // Add headers for the content
            contentWorksheet.Cells[1, 1].Value = "Chapter/Concept/Sub-Concept Name";
            contentWorksheet.Cells[1, 2].Value = "Type";
            contentWorksheet.Cells[1, 3].Value = "Chapter/Concept/Sub-Concept ID";

            var chapters = GetChapters(subjectId);
            int currentRow = 2;
            foreach (var chapter in chapters)
            {
                // Add chapter details to the worksheet
                contentWorksheet.Cells[currentRow, 1].Value = chapter.ContentName_Chapter;
                contentWorksheet.Cells[currentRow, 2].Value = "Chapter";  // Mark it as Chapter
                contentWorksheet.Cells[currentRow, 3].Value = chapter.ChapterCode;
                currentRow++;

                // Get Topics for each Chapter
                var topics = GetTopics(chapter.ContentIndexId);
                foreach (var topic in topics)
                {
                    // Add topic (concept) details under the corresponding chapter
                    contentWorksheet.Cells[currentRow, 1].Value = topic.ContentName_Topic;
                    contentWorksheet.Cells[currentRow, 2].Value = "Concept";  // Mark it as Concept
                    contentWorksheet.Cells[currentRow, 3].Value = topic.TopicCode;
                    currentRow++;

                    // Get SubTopics for each Topic
                    var subTopics = GetSubTopics(topic.ContInIdTopic);
                    foreach (var subTopic in subTopics)
                    {
                        // Add sub-topic (sub-concept) details under the corresponding topic
                        contentWorksheet.Cells[currentRow, 1].Value = subTopic.ContentName_SubTopic;
                        contentWorksheet.Cells[currentRow, 2].Value = "Sub-Concept";  // Mark it as Sub-Concept
                        contentWorksheet.Cells[currentRow, 3].Value = subTopic.SubTopicCode;
                        currentRow++;
                    }
                }
            }


            // Populate data for Difficulty Levels
            difficultyLevelWorksheet.Cells[1, 1].Value = "LevelId";
            difficultyLevelWorksheet.Cells[1, 2].Value = "LevelName";
            difficultyLevelWorksheet.Cells[1, 3].Value = "LevelCode";

            var difficultyLevels = GetDifficultyLevels();
            int levelRow = 2;
            foreach (var level in difficultyLevels)
            {
                difficultyLevelWorksheet.Cells[levelRow, 1].Value = level.LevelId;
                difficultyLevelWorksheet.Cells[levelRow, 2].Value = level.LevelName;
                difficultyLevelWorksheet.Cells[levelRow, 3].Value = level.LevelCode;
                levelRow++;
            }

            // Populate data for Question Types
            questionTypeWorksheet.Cells[1, 1].Value = "QuestionTypeID";
            questionTypeWorksheet.Cells[1, 2].Value = "QuestionType";

            var questionTypes = GetQuestionTypes();
            int typeRow = 2;
            foreach (var type in questionTypes)
            {
                questionTypeWorksheet.Cells[typeRow, 1].Value = type.QuestionTypeID;
                questionTypeWorksheet.Cells[typeRow, 2].Value = type.QuestionType;
                typeRow++;
            }
            coursesWorksheet.Cells[1, 1].Value = "CourseName";
            coursesWorksheet.Cells[1, 2].Value = "CourseCode";

            var courses = GetCourses();
            int courseRow = 2;
            foreach (var course in courses)
            {
                coursesWorksheet.Cells[courseRow, 1].Value = course.CourseName;
                coursesWorksheet.Cells[courseRow, 2].Value = course.CourseCode;
                courseRow++;
            }


            paragraphWorksheet.Cells[1, 1].Value = "ParagraphId";
            paragraphWorksheet.Cells[1, 2].Value = "Paragraph";

            int courseColumnStartIndex = 3; // Start after ParagraphId and Paragraph

            foreach (var course in courses)
            {
                paragraphWorksheet.Cells[1, courseColumnStartIndex].Value = course.CourseName;
                courseColumnStartIndex++;
            }

            // AutoFit columns
            subjectWorksheet.Cells[subjectWorksheet.Dimension.Address].AutoFitColumns();
            contentWorksheet.Cells[contentWorksheet.Dimension.Address].AutoFitColumns();
            difficultyLevelWorksheet.Cells[difficultyLevelWorksheet.Dimension.Address].AutoFitColumns();
            questionTypeWorksheet.Cells[questionTypeWorksheet.Dimension.Address].AutoFitColumns();
            coursesWorksheet.Cells[coursesWorksheet.Dimension.Address].AutoFitColumns();
            categoryWorksheet.Cells[categoryWorksheet.Dimension.Address].AutoFitColumns();
            paragraphWorksheet.Cells[paragraphWorksheet.Dimension.Address].AutoFitColumns();
        }
        private IEnumerable<Subject> GetSubjects(int subjectId)
        {
            var query = "SELECT * FROM tblSubject WHERE SubjectId = @subjectId AND Status = 1";
            var result = _connection.Query<dynamic>(query, new { subjectId = subjectId });
            var resposne = result.Select(item => new Subject
            {
                SubjectCode = item.SubjectCode,
                SubjectId = item.SubjectId,
                SubjectName = item.SubjectName,
                SubjectType = item.SubjectType
            });
            return resposne;
        }
        private IEnumerable<Chapters> GetChapters(int subjectId)
        {
            var query = "SELECT * FROM tblContentIndexChapters WHERE IndexTypeId = 1 AND IsActive = 1 AND SubjectId = @subjectId";
            return _connection.Query<Chapters>(query, new { subjectId });
        }
        private IEnumerable<Topics> GetTopics(int chapterId)
        {
            var query = "SELECT * FROM tblContentIndexTopics WHERE IndexTypeId = 2 AND IsActive = 1 AND ContentIndexId = @chapterId";
            return _connection.Query<Topics>(query, new { chapterId });
        }
        private IEnumerable<SubTopic> GetSubTopics(int TopicId)
        {
            var query = "SELECT * FROM tblContentIndexSubTopics WHERE IndexTypeId = 3 AND IsActive = 1 AND ContInIdTopic = @TopicId";
            return _connection.Query<SubTopic>(query, new { TopicId });
        }
        private IEnumerable<DifficultyLevel> GetDifficultyLevels()
        {
            var query = "SELECT [LevelId], [LevelName], [LevelCode], [Status], [NoofQperLevel], [SuccessRate], [createdon], [patterncode], [modifiedon], [modifiedby], [createdby], [EmployeeID], [EmpFirstName] FROM [tbldifficultylevel]";
            return _connection.Query<DifficultyLevel>(query);
        }
        private IEnumerable<Course> GetCourses()
        {
            var query = "SELECT CourseName, CourseCode FROM [tblCourse]";
            return _connection.Query<Course>(query);
        }
        private IEnumerable<Questiontype> GetQuestionTypes()
        {
            var query = "SELECT [QuestionTypeID], [QuestionType], [Code], [Status], [MinNoOfOptions], [modifiedon], [modifiedby], [createdon], [createdby], [EmployeeID], [EmpFirstName], [TypeOfOption], [Question] FROM [tblQBQuestionType]";
            return _connection.Query<Questiontype>(query);
        }
        public async Task<ServiceResponse<string>> UploadQuestionsFromExcel(IFormFile file, int EmployeeId)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var questions = new List<QuestionDTO>();
            List<ComprehensiveQuestionRequest> comprehensiveQuestions = new List<ComprehensiveQuestionRequest>();
            Dictionary<string, int> courseCodeDictionary = new Dictionary<string, int>();
            Dictionary<string, int> subjectDictionary = new Dictionary<string, int>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using (var package = new ExcelPackage(stream))
                {
                    // Process main worksheet for questions
                    var worksheet = package.Workbook.Worksheets["Questions"];
                    var rowCount = worksheet.Dimension.Rows;
                    var courseSheet = package.Workbook.Worksheets["Courses"];
                    var subjectSheet = package.Workbook.Worksheets["Subjects"];
                    LoadCourseCodes(courseSheet, courseCodeDictionary);
                    LoadSubjectCodes(subjectSheet, subjectDictionary);

                    for (int row = 2; row <= rowCount; row++) // Skip header row
                    {

                        int questionTypeId = Convert.ToInt32(worksheet.Cells[row, 4].Text);

                        if (questionTypeId == 11) // Handle paragraph type
                        {

                            var paragraphIdPrevious = (row > 2 && int.TryParse(worksheet.Cells[row - 1, 5].Text, out int previousId)) ? previousId : 0;

                            var paragraphId = Convert.ToInt32(worksheet.Cells[row, 5].Text); // Assuming ParagraphId is in column 6

                            if (paragraphId != paragraphIdPrevious)
                            {
                                var paragraphSheet = package.Workbook.Worksheets["Paragraph"];
                                var paragraph = GetParagraphById(paragraphSheet, paragraphId);

                                if (string.IsNullOrEmpty(paragraph))
                                {
                                    return new ServiceResponse<string>(false, $"Paragraph not found for ParagraphId {paragraphId} at row {row}.", string.Empty, 400);
                                }

                                // Fetch chapter, topic, and subtopic names
                                string contentCode = worksheet.Cells[row, 3].Text;  // Fetch Content Code from the sheet
                                int indexTypeId = 0;
                                int contentIndexId = 0;

                                // Determine the type of content based on the structure of ContentCode
                                if (!string.IsNullOrEmpty(contentCode) && contentCode.Contains("C") && !contentCode.Contains("T"))
                                {
                                    // It's a Chapter Code
                                    indexTypeId = 1;
                                    contentIndexId = await GetChapterIdByCode(contentCode);
                                }
                                else if (!string.IsNullOrEmpty(contentCode) && contentCode.Contains("T") && !contentCode.Contains("ST"))
                                {
                                    // It's a Concept/Topic Code
                                    indexTypeId = 2;
                                    contentIndexId = await GetTopicIdByCode(contentCode);
                                }
                                else if (!string.IsNullOrEmpty(contentCode) && contentCode.Contains("ST"))
                                {
                                    // It's a Sub-Concept Code
                                    indexTypeId = 3;
                                    contentIndexId = await GetSubTopicIdByCode(contentCode);
                                }
                                else
                                {
                                    return new ServiceResponse<string>(false, $"Invalid data at row {row}: Must have at least a chapter name.", string.Empty, 400);
                                }

                                if (contentIndexId == 0)
                                {
                                    return new ServiceResponse<string>(false, $"Content index not found for the specified names at row {row}.", string.Empty, 400);
                                }
                                // Fetch the SubjectID from the row
                                int subjectIDFromRow = Convert.ToInt32(worksheet.Cells[row, 2].Text);
                                int validSubjectId = subjectDictionary.ContainsValue(subjectIDFromRow) ? subjectIDFromRow : 0;
                                // Subject ID validation - if it doesn't match, skip this record
                                if (subjectIDFromRow != validSubjectId)
                                {
                                    // Log the skipped question if needed
                                    Console.WriteLine($"Skipped question at row {row}: Subject ID {subjectIDFromRow} does not match {validSubjectId}.");
                                    continue; // Skip to the next question
                                }
                                var comprehensiveQuestionRequest = new ComprehensiveQuestionRequest
                                {
                                    Paragraph = paragraph,
                                    QuestionTypeId = questionTypeId,
                                    Status = true, // Set as required
                                    CategoryId = Convert.ToInt32(worksheet.Cells[row, 1].Text),
                                    CreatedBy = "YourUsername",
                                    CreatedOn = DateTime.UtcNow,
                                    subjectID = Convert.ToInt32(worksheet.Cells[row, 2].Text),
                                    EmployeeId = EmployeeId,
                                    IndexTypeId = indexTypeId, // Populate as required
                                    ContentIndexId = contentIndexId, // Populate as required
                                    IsRejected = false,
                                    IsApproved = false,
                                    QuestionCode = "string", // Generate or populate as required
                                    IsActive = true,
                                    IsConfigure = true,
                                    QIDCourses = ExtractQIDCourses(paragraphSheet, courseCodeDictionary, 2), // Populate from the row
                                    Questions = GetChildQuestions(worksheet, rowCount, courseCodeDictionary, subjectDictionary, EmployeeId, paragraphId, row), // Populate child questions as needed
                                };

                                var comprehensiveResponse = await AddUpdateComprehensiveQuestion(comprehensiveQuestionRequest);
                                // Update the previousParagraphId to the current one after processing
                                paragraphIdPrevious = paragraphId;
                                if (!comprehensiveResponse.Success)
                                {
                                    return new ServiceResponse<string>(false, $"Failed to add/update comprehensive question at row {row}: {comprehensiveResponse.Message}", string.Empty, 500);
                                }
                            }
                            else
                            {
                                // Skip processing, as it's the same paragraph as the previous question
                                continue;
                            }
                        }
                        else
                        {
                            int courseStartCol = 0;
                            for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                            {
                                if (worksheet.Cells[1, col].Text.Equals("Extra Information", StringComparison.OrdinalIgnoreCase))
                                {
                                    courseStartCol = col + 1; // Start right after "Extra Information"
                                    break;
                                }
                            }

                            if (courseStartCol == 0)
                            {
                                return new ServiceResponse<string>(false, "Extra Information column not found.", string.Empty, 400);
                            }

                            var qidCourses = new List<QIDCourse>();

                            for (int col = courseStartCol; col <= courseStartCol + courseCodeDictionary.Count; col++)
                            {
                                var courseName = worksheet.Cells[1, col].Text; // Assuming course names are in the first row (header row)

                                if (string.IsNullOrEmpty(courseName)) continue; // Skip if the course name is empty

                                int courseId = courseCodeDictionary.ContainsKey(courseName) ? courseCodeDictionary[courseName] : 0;
                                if (courseId == 0)
                                {
                                    return new ServiceResponse<string>(false, $"Course name '{courseName}' not found in the header at column {col}.", string.Empty, 400);
                                }

                                // Get the difficulty level ID from the current cell
                                string difficultyCellText = worksheet.Cells[row, col].Text;
                                if (string.IsNullOrEmpty(difficultyCellText)) continue; // If the cell is empty, skip this course mapping

                                int diffiId = int.Parse(difficultyCellText); // Convert difficulty level to int (or handle parsing errors as needed)

                                // Create and add the QIDCourse entry
                                qidCourses.Add(new QIDCourse
                                {
                                    QIDCourseID = 0, // Assuming you want to set this later or handle it in the AddUpdateQuestion method
                                    QID = 0, // Populate this as needed
                                    QuestionCode = "string",//string.IsNullOrEmpty(worksheet.Cells[row, 27].Text) ? null : worksheet.Cells[row, 27].Text, // Assuming QuestionCode is in column 27
                                    CourseID = courseId,
                                    LevelId = diffiId, // Difficulty level ID fetched from the current cell
                                    Status = true, // Set as needed
                                    CreatedBy = "YourUsername", // Set the creator's username or similar info
                                    CreatedDate = DateTime.UtcNow, // Use the current date and time
                                    ModifiedBy = "YourUsername", // Set as needed
                                    ModifiedDate = DateTime.UtcNow // Set as needed
                                });
                            }

                            // Fetch chapter, topic, and subtopic names
                            string contentCode = worksheet.Cells[row, 3].Text;  // Fetch Content Code from the sheet
                            int indexTypeId = 0;
                            int contentIndexId = 0;

                            // Determine the type of content based on the structure of ContentCode
                            if (!string.IsNullOrEmpty(contentCode) && contentCode.Contains("C") && !contentCode.Contains("T"))
                            {
                                // It's a Chapter Code
                                indexTypeId = 1;
                                contentIndexId = await GetChapterIdByCode(contentCode);
                            }
                            else if (!string.IsNullOrEmpty(contentCode) && contentCode.Contains("T") && !contentCode.Contains("ST"))
                            {
                                // It's a Concept/Topic Code
                                indexTypeId = 2;
                                contentIndexId = await GetTopicIdByCode(contentCode);
                            }
                            else if (!string.IsNullOrEmpty(contentCode) && contentCode.Contains("ST"))
                            {
                                // It's a Sub-Concept Code
                                indexTypeId = 3;
                                contentIndexId = await GetSubTopicIdByCode(contentCode);
                            }
                            else
                            {
                                return new ServiceResponse<string>(false, $"Invalid data at row {row}: Must have at least a chapter name.", string.Empty, 400);
                            }

                            if (contentIndexId == 0)
                            {
                                return new ServiceResponse<string>(false, $"Content index not found for the specified names at row {row}.", string.Empty, 400);
                            }
                            string extraInfo = worksheet.Cells[row, courseStartCol - 1].Text; // Assuming extra info column is just before courses
                            int explanationCol = 0;
                            for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                            {
                                if (worksheet.Cells[1, col].Text.Equals("Explanation", StringComparison.OrdinalIgnoreCase))
                                {
                                    explanationCol = col;
                                    break;
                                }
                            }

                            string explanation = explanationCol > 0 ? worksheet.Cells[row, explanationCol].Text : null;
                            // Fetch the SubjectID from the row
                            int subjectIDFromRow = Convert.ToInt32(worksheet.Cells[row, 2].Text);
                            int validSubjectId = subjectDictionary.ContainsValue(subjectIDFromRow) ? subjectIDFromRow : 0;
                            // Subject ID validation - if it doesn't match, skip this record
                            if (subjectIDFromRow != validSubjectId)
                            {
                                // Log the skipped question if needed
                                Console.WriteLine($"Skipped question at row {row}: Subject ID {subjectIDFromRow} does not match {validSubjectId}.");
                                continue; // Skip to the next question
                            }
                            // Create the question DTO
                            var question = new QuestionDTO
                            {
                                QuestionDescription = worksheet.Cells[row, 7].Text,
                                QuestionTypeId = Convert.ToInt32(worksheet.Cells[row, 4].Text),
                                subjectID = Convert.ToInt32(worksheet.Cells[row, 2].Text),
                                IndexTypeId = indexTypeId,
                                Explanation = explanation,
                                QuestionCode = "string",//string.IsNullOrEmpty(worksheet.Cells[row, 27].Text) ? null : worksheet.Cells[row, 27].Text,
                                ContentIndexId = contentIndexId,
                                AnswerMultipleChoiceCategories = GetAnswerMultipleChoiceCategories(worksheet, row),
                                Answersingleanswercategories = GetAnswerSingleAnswerCategories(worksheet, row, Convert.ToInt32(worksheet.Cells[row, 4].Text)),
                                QIDCourses = qidCourses,
                                IsActive = true,
                                IsConfigure = true,
                                EmployeeId = EmployeeId,
                                CategoryId = Convert.ToInt32(worksheet.Cells[row, 1].Text),
                                ExtraInformation = extraInfo
                            };

                            // Add question to the list for bulk processing
                            questions.Add(question);

                            // Call AddUpdateQuestion for each question
                            var response = await AddUpdateQuestion(question);
                            if (!response.Success)
                            {
                                return new ServiceResponse<string>(false, $"Failed to add/update question at row {row}: {response.Message}", string.Empty, 500);
                            }
                        }
                    }
                }
            }
            return new ServiceResponse<string>(true, "All questions uploaded successfully.", "Data uploaded successfully.", 200);
        }
        private List<QIDCourse> ExtractQIDCourses(
    ExcelWorksheet paragraphSheet,
    Dictionary<string, int> courseCodeDictionary,
    int paragraphRow)
        {

            // Step 2: Identify the starting column for dynamic course data
            int courseStartCol = 3;
            // Step 3: Extract QIDCourse mappings for the given paragraph
            var qidCourses = new List<QIDCourse>();
            for (int col = courseStartCol; col <= paragraphSheet.Dimension.Columns; col++)
            {
                string courseName = paragraphSheet.Cells[1, col].Text; // Course name in header row
                if (string.IsNullOrEmpty(courseName)) continue; // Skip empty columns

                if (!courseCodeDictionary.TryGetValue(courseName, out int courseId)) continue; // Skip if course is not mapped

                string difficultyText = paragraphSheet.Cells[paragraphRow, col].Text; // Difficulty ID for the paragraph
                if (string.IsNullOrEmpty(difficultyText)) continue;

                if (!int.TryParse(difficultyText, out int difficultyId)) continue; // Skip non-numeric difficulty levels

                // Create QIDCourse entry
                qidCourses.Add(new QIDCourse
                {
                    QIDCourseID = 0, // To be set later, if needed
                    QID = 0, // To be set later, if needed
                    QuestionCode = "string", // Modify based on your requirements
                    CourseID = courseId,
                    LevelId = difficultyId,
                    Status = true,
                    CreatedBy = "YourUsername", // Replace with actual creator
                    CreatedDate = DateTime.UtcNow,
                    ModifiedBy = "YourUsername", // Replace with actual modifier
                    ModifiedDate = DateTime.UtcNow
                });
            }

            return qidCourses;
        }
        private string GetParagraphById(ExcelWorksheet paragraphSheet, int paragraphId)
        {
            if (paragraphSheet == null) return null;

            var rowCount = paragraphSheet.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++) // Assuming the header is in row 1
            {
                if (Convert.ToInt32(paragraphSheet.Cells[row, 1].Text) == paragraphId) // Assuming ParagraphId is in column 1
                {
                    return paragraphSheet.Cells[row, 2].Text; // Assuming Paragraph text is in column 2
                }
            }

            return null; // Return null if no matching ParagraphId is found
        }
        private List<ParagraphQuestionDTO> GetChildQuestions(ExcelWorksheet worksheet, int rowCount,
        Dictionary<string, int> courseCodeDictionary,
        Dictionary<string, int> subjectDictionary, int EmployeeId, int ParagraphId, int rowNumber)
        {
            var questions = new List<ParagraphQuestionDTO>();
            for (int row = rowNumber; row <= rowCount; row++) // Skip header row
            {

                var paragraphID = string.IsNullOrWhiteSpace(worksheet.Cells[row, 5].Text)
       ? 0
       : Convert.ToInt32(worksheet.Cells[row, 5].Text);
                if (paragraphID != ParagraphId)
                {
                    break;
                }
                int questionTypeId = Convert.ToInt32(worksheet.Cells[row, 4].Text);
                int courseStartCol = 0;
                for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                {
                    if (worksheet.Cells[1, col].Text.Equals("Extra Information", StringComparison.OrdinalIgnoreCase))
                    {
                        courseStartCol = col + 1; // Start right after "Extra Information"
                        break;
                    }
                }

                if (courseStartCol == 0)
                {
                    return [];
                }

                var qidCourses = new List<QIDCourse>();

                for (int col = courseStartCol; col <= courseStartCol + courseCodeDictionary.Count; col++)
                {
                    var courseName = worksheet.Cells[1, col].Text; // Assuming course names are in the first row (header row)

                    if (string.IsNullOrEmpty(courseName)) continue; // Skip if the course name is empty

                    int courseId = courseCodeDictionary.ContainsKey(courseName) ? courseCodeDictionary[courseName] : 0;
                    if (courseId == 0)
                    {
                        return [];
                    }

                    // Get the difficulty level ID from the current cell
                    string difficultyCellText = worksheet.Cells[row, col].Text;
                    if (string.IsNullOrEmpty(difficultyCellText)) continue; // If the cell is empty, skip this course mapping

                    int diffiId = int.Parse(difficultyCellText); // Convert difficulty level to int (or handle parsing errors as needed)

                    // Create and add the QIDCourse entry
                    qidCourses.Add(new QIDCourse
                    {
                        QIDCourseID = 0, // Assuming you want to set this later or handle it in the AddUpdateQuestion method
                        QID = 0, // Populate this as needed
                        QuestionCode = "string",//string.IsNullOrEmpty(worksheet.Cells[row, 27].Text) ? null : worksheet.Cells[row, 27].Text, // Assuming QuestionCode is in column 27
                        CourseID = courseId,
                        LevelId = diffiId, // Difficulty level ID fetched from the current cell
                        Status = true, // Set as needed
                        CreatedBy = "YourUsername", // Set the creator's username or similar info
                        CreatedDate = DateTime.UtcNow, // Use the current date and time
                        ModifiedBy = "YourUsername", // Set as needed
                        ModifiedDate = DateTime.UtcNow // Set as needed
                    });
                }

                // Fetch chapter, topic, and subtopic names
                string contentCode = worksheet.Cells[row, 3].Text;  // Fetch Content Code from the sheet
                int indexTypeId = 0;
                int contentIndexId = 0;

                // Determine the type of content based on the structure of ContentCode
                if (!string.IsNullOrEmpty(contentCode) && contentCode.Contains("C") && !contentCode.Contains("T"))
                {
                    // It's a Chapter Code
                    indexTypeId = 1;
                    contentIndexId = _connection.QueryFirstOrDefault<int>(@"SELECT TOP 1 ContentIndexId FROM tblContentIndexChapters WHERE ChapterCode = @ChapterCode AND IsActive = 1",
                        new { ChapterCode = contentCode });

                }
                else if (!string.IsNullOrEmpty(contentCode) && contentCode.Contains("T") && !contentCode.Contains("ST"))
                {
                    // It's a Concept/Topic Code
                    indexTypeId = 2;
                    contentIndexId = _connection.QueryFirstOrDefault<int>(@"SELECT TOP 1 ContentIndexId FROM tblContentIndexTopics WHERE TopicCode = @TopicCode AND IsActive = 1",
                        new { TopicCode = contentCode });
                }
                else if (!string.IsNullOrEmpty(contentCode) && contentCode.Contains("ST"))
                {
                    // It's a Sub-Concept Code
                    indexTypeId = 3;
                    contentIndexId = _connection.QueryFirstOrDefault<int>(@"SELECT TOP 1 ContInIdSubTopic FROM tblContentIndexSubTopics WHERE SubTopicCode = @SubTopicCode AND IsActive = 1",
                        new { SubTopicCode = contentCode });
                }
                else
                {
                    return [];
                }

                if (contentIndexId == 0)
                {
                    return [];
                }
                string extraInfo = worksheet.Cells[row, courseStartCol - 1].Text; // Assuming extra info column is just before courses
                int explanationCol = 0;
                for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                {
                    if (worksheet.Cells[1, col].Text.Equals("Explanation", StringComparison.OrdinalIgnoreCase))
                    {
                        explanationCol = col;
                        break;
                    }
                }

                string explanation = explanationCol > 0 ? worksheet.Cells[row, explanationCol].Text : null;
                // Fetch the SubjectID from the row
                int subjectIDFromRow = Convert.ToInt32(worksheet.Cells[row, 2].Text);
                int validSubjectId = subjectDictionary.ContainsValue(subjectIDFromRow) ? subjectIDFromRow : 0;
                // Subject ID validation - if it doesn't match, skip this record
                if (subjectIDFromRow != validSubjectId)
                {
                    // Log the skipped question if needed
                    Console.WriteLine($"Skipped question at row {row}: Subject ID {subjectIDFromRow} does not match {validSubjectId}.");
                    continue; // Skip to the next question
                }
                // Create the question DTO
                var question = new ParagraphQuestionDTO
                {
                    QuestionDescription = worksheet.Cells[row, 7].Text,
                    QuestionTypeId = Convert.ToInt32(worksheet.Cells[row, 4].Text),
                    // subjectID = Convert.ToInt32(worksheet.Cells[row, 2].Text),
                    // IndexTypeId = indexTypeId,
                    Explanation = explanation,
                    QuestionCode = "string",//string.IsNullOrEmpty(worksheet.Cells[row, 27].Text) ? null : worksheet.Cells[row, 27].Text,
                    // ContentIndexId = contentIndexId,
                    AnswerMultipleChoiceCategories = GetAnswerMultipleChoiceCategories(worksheet, row),
                    Answersingleanswercategories = GetAnswerSingleAnswerCategories(worksheet, row, Convert.ToInt32(worksheet.Cells[row, 4].Text)),
                    IsActive = true,
                    IsConfigure = true,
                    //EmployeeId = EmployeeId,
                    // CategoryId = Convert.ToInt32(worksheet.Cells[row, 1].Text),
                    ExtraInformation = extraInfo,
                    Status = true,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = ""
                };

                // Add question to the list for bulk processing
                questions.Add(question);
            }
            return questions;
        }
        private async Task<int> GetChapterIdByCode(string chapterCode)
        {
            string query = "SELECT TOP 1 ContentIndexId FROM tblContentIndexChapters WHERE ChapterCode = @ChapterCode AND IsActive = 1";
            return await _connection.QueryFirstOrDefaultAsync<int>(query, new { ChapterCode = chapterCode });
        }
        private async Task<int> GetTopicIdByCode(string topicCode)
        {
            string query = "SELECT TOP 1 ContentIndexId FROM tblContentIndexTopics WHERE TopicCode = @TopicCode AND IsActive = 1";
            return await _connection.QueryFirstOrDefaultAsync<int>(query, new { TopicCode = topicCode });
        }
        private async Task<int> GetSubTopicIdByCode(string subTopicCode)
        {
            string query = "SELECT TOP 1 ContInIdSubTopic FROM tblContentIndexSubTopics WHERE SubTopicCode = @SubTopicCode AND IsActive = 1";
            return await _connection.QueryFirstOrDefaultAsync<int>(query, new { SubTopicCode = subTopicCode });
        }

        // Helper method to load subject codes and their corresponding IDs
        private void LoadSubjectCodes(ExcelWorksheet sheet, Dictionary<string, int> dictionary)
        {
            int rowCount = sheet.Dimension.Rows;
            var query = "SELECT SubjectId FROM tblSubject WHERE [SubjectName] = @subjectName";
            for (int row = 2; row <= rowCount; row++) // Assuming the first row contains headers
            {
                var subjectName = sheet.Cells[row, 3].Text; // Assuming subject codes are in the second column
                var subjectId = _connection.QuerySingleOrDefault<int>(query, new { subjectName = subjectName });

                if (!string.IsNullOrEmpty(subjectName) && !dictionary.ContainsKey(subjectName))
                {
                    dictionary.Add(subjectName, subjectId);
                }
            }
        }
        private void LoadCourseCodes(ExcelWorksheet sheet, Dictionary<string, int> dictionary)
        {
            int rowCount = sheet.Dimension.Rows;
            var query = "SELECT CourseId FROM tblCourse WHERE [CourseName] = @CourseName";
            for (int row = 2; row <= rowCount; row++) // Assuming the first row contains headers
            {
                var courseName = sheet.Cells[row, 1].Text; // Assuming subject codes are in the second column
                var courseId = _connection.QuerySingleOrDefault<int>(query, new { CourseName = courseName });

                if (!string.IsNullOrEmpty(courseName) && !dictionary.ContainsKey(courseName))
                {
                    dictionary.Add(courseName, courseId);
                }
            }
        }
        private List<AnswerMultipleChoiceCategory> GetAnswerMultipleChoiceCategories(ExcelWorksheet worksheet, int row)
        {
            var categories = new List<AnswerMultipleChoiceCategory>();

            // Get the correct answers from cell 5 (comma-separated for multiple answers)
            var correctAnswers = worksheet.Cells[row, 8].Text.Split(',').Select(a => a.Trim()).ToList();

            int optionColumn = 9; // Start from column 6 where the options begin

            // Loop through the columns until the Explanation column is found
            while (true)
            {
                var currentCellValue = worksheet.Cells[row, optionColumn].Text;

                // Check if the current column is the "Explanation" column
                if (worksheet.Cells[1, optionColumn].Text.Equals("Explanation", StringComparison.OrdinalIgnoreCase))
                {
                    // Stop processing when Explanation column is found
                    break;
                }

                // If it's not the Explanation column and there's an option present in the cell
                if (!string.IsNullOrWhiteSpace(currentCellValue))
                {
                    var answer = currentCellValue; // Get the answer from the current option column

                    // Check if the current option is one of the correct answers
                    bool isCorrect = correctAnswers.Any(ca => ca.Equals(answer, StringComparison.OrdinalIgnoreCase));

                    categories.Add(new AnswerMultipleChoiceCategory
                    {
                        Answer = answer,
                        Iscorrect = isCorrect, // Set isCorrect to true if it's one of the correct answers
                        Matchid = 0 // Assuming MatchId is still 0
                    });
                }

                optionColumn++; // Move to the next column
            }

            return categories;
        }
        private Answersingleanswercategory GetAnswerSingleAnswerCategories(ExcelWorksheet worksheet, int row, int questionTypeId)
        {
            string answer = null;
            // Define which question types are considered descriptive
            if (questionTypeId == 7 || questionTypeId == 8 || questionTypeId == 3)
            {

                //// Loop through the columns to find the "Explanation" column
                //for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                //{
                //    var headerText = worksheet.Cells[1, col].Text;

                //    if (headerText.Equals("Explanation", StringComparison.OrdinalIgnoreCase))
                //    {
                //        answer = worksheet.Cells[row, col].Text; // Fetch the explanation text as the answer
                //        break;
                //    }
                //}
                int explanationCol = 0;
                for (int col = 1; col <= worksheet.Dimension.Columns; col++)
                {
                    if (worksheet.Cells[1, col].Text.Equals("Explanation", StringComparison.OrdinalIgnoreCase))
                    {
                        explanationCol = col;
                        break;
                    }
                }

                answer = explanationCol > 0 ? worksheet.Cells[row, explanationCol].Text : null;
            }
            // Return the answer for single-answer category questions
            return new Answersingleanswercategory
            {
                Answer = answer
            };
        }

        // Helper method to fetch questions based on parameters
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
        private async Task<int> AddUpdateQIDCourses(List<QIDCourse>? request, string questionCode)
        {
            int rowsAffected = 0;
            if (request != null)
            {
                // Use questionCode to get questionId
                string getQuestionIdQuery = "SELECT QuestionID FROM tblQuestion WHERE QuestionCode = @QuestionCode AND IsActive = 1";
                int questionId = await _connection.QuerySingleOrDefaultAsync<int>(getQuestionIdQuery, new { QuestionCode = questionCode });

                if (questionId > 0)
                {
                    foreach (var data in request)
                    {
                        var newQIDCourse = new QIDCourse
                        {
                            QID = questionId,
                            CourseID = data.CourseID,
                            LevelId = data.LevelId,
                            Status = true,
                            // CreatedBy = 1,
                            CreatedDate = DateTime.Now,
                            //  ModifiedBy = 1,
                            ModifiedDate = DateTime.Now,
                            QIDCourseID = data.QIDCourseID,
                            QuestionCode = questionCode
                        };
                        if (data.QIDCourseID == 0)
                        {
                            string insertQuery = @"
                            INSERT INTO tblQIDCourse (QID, CourseID, LevelId, Status, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, QuestionCode)
                            VALUES (@QID, @CourseID, @LevelId, @Status, @CreatedBy, @CreatedDate, @ModifiedBy, @ModifiedDate, @QuestionCode)";

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
                               ModifiedDate = @ModifiedDate,
                               QuestionCode = @QuestionCode
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
        private async Task<int> AddUpdateQuestionSubjectMap(List<QuestionSubjectMapping>? request, string questionCode)
        {
            if (request != null)
            {
                // Use questionCode to get questionId
                string getQuestionIdQuery = "SELECT QuestionID FROM tblQuestion WHERE QuestionCode = @QuestionCode AND IsActive = 1";
                int questionId = await _connection.QuerySingleOrDefaultAsync<int>(getQuestionIdQuery, new { QuestionCode = questionCode });

                if (questionId > 0)
                {
                    foreach (var data in request)
                    {
                        data.QuestionCode = questionCode;
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
                            string insertQuery = @"INSERT INTO tblQuestionSubjectMapping (ContentIndexId, Indexid, questionid, QuestionCode) 
                            VALUES (@ContentIndexId, @Indexid, @questionid, @QuestionCode)";
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
                        string insertQuery = @"INSERT INTO tblQuestionSubjectMapping (ContentIndexId, Indexid, questionid, QuestionCode) 
                        VALUES (@ContentIndexId, @Indexid, @questionid, @QuestionCode)";
                        var valuesInserted = await _connection.ExecuteAsync(insertQuery, request);
                        return valuesInserted;
                    }
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
        private List<QIDCourseResponse> GetListOfQIDCourse(string QuestionCode)
        {
            // Get active question IDs
            var activeQuestionIds = GetActiveQuestionIds(QuestionCode);

            // If no active question IDs found, return an empty list
            if (!activeQuestionIds.Any())
            {
                return new List<QIDCourseResponse>();
            }

            var query = @"
    SELECT qc.*, c.CourseName, l.LevelName
    FROM [tblQIDCourse] qc
    LEFT JOIN tblCourse c ON qc.CourseID = c.CourseID
    LEFT JOIN tbldifficultylevel l ON qc.LevelId = l.LevelId
    WHERE qc.QuestionCode = @QuestionCode
      AND qc.QID IN @ActiveQuestionIds";

            var data = _connection.Query<QIDCourseResponse>(query, new { QuestionCode, ActiveQuestionIds = activeQuestionIds });
            return data.ToList();
        }
        private Reference GetQuestionReference(int questionId)
        {
            var boardquery = @"SELECT * FROM [tblReference] WHERE QuestionId = @questionId;";

            var data = _connection.QueryFirstOrDefault<Reference>(boardquery, new { questionId });
            return data ?? new Reference();
        }
        private List<QuestionSubjectMappingResponse> GetListOfQuestionSubjectMapping(string QuestionCode)
        {
            // Get active question IDs
            var activeQuestionIds = GetActiveQuestionIds(QuestionCode);

            // If no active question IDs found, return an empty list
            if (!activeQuestionIds.Any())
            {
                return new List<QuestionSubjectMappingResponse>();
            }

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
            WHERE qsm.QuestionCode = @QuestionCode
              AND qsm.questionid IN @ActiveQuestionIds";

            var data = _connection.Query<QuestionSubjectMappingResponse>(boardquery, new { QuestionCode, ActiveQuestionIds = activeQuestionIds });
            return data.ToList();
        }
        private Answersingleanswercategory GetSingleAnswer(string QuestionCode, int QuestionId)
        {
            var answerMaster = _connection.QueryFirstOrDefault<AnswerMaster>(@"
        SELECT * FROM tblAnswerMaster WHERE QuestionCode = @QuestionCode and Questionid = @Questionid", new { QuestionCode, Questionid = QuestionId });

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
        private List<AnswerMultipleChoiceCategory> GetMultipleAnswers(string QuestionCode)
        {
            var answerMaster = _connection.QueryFirstOrDefault<AnswerMaster>(@"
         SELECT TOP 1 * FROM tblAnswerMaster WHERE QuestionCode = @QuestionCode ORDER BY AnswerId DESC", new { QuestionCode });

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
        private async Task<QuestionResponseDTO> CreateQuestionResponseDTO(dynamic item)
        {
            var questionResponse = new QuestionResponseDTO
            {
                QuestionId = item.QuestionId,
                QuestionDescription = item.QuestionDescription,
                SubjectName = item.SubjectName,
                EmployeeName = item.EmpFirstName,
                IndexTypeName = item.IndexTypeName,
                ContentIndexName = item.ContentIndexName,
                QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                //QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
                ContentIndexId = item.ContentIndexId,
                CreatedBy = item.CreatedBy,
                CreatedOn = item.CreatedOn,
                EmployeeId = item.EmployeeId,
                IndexTypeId = item.IndexTypeId,
                subjectID = item.subjectID,
                ModifiedOn = item.ModifiedOn,
                QuestionTypeId = item.QuestionTypeId,
                QuestionTypeName = item.QuestionTypeName,
                QuestionCode = item.QuestionCode,
                Explanation = item.Explanation,
                ExtraInformation = item.ExtraInformation,
                IsActive = item.IsActive,
                Answersingleanswercategories = GetSingleAnswer(item.QuestionCode, item.QuestionId),
                AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode)
            };
            return questionResponse;
        }
        private List<int> GetActiveQuestionIds(string QuestionCode)
        {
            var query = @"
            SELECT q.QuestionId
            FROM tblQuestion q
            WHERE q.QuestionCode = @QuestionCode
              AND q.IsActive = 1 AND q.IsConfigure = 1";

            var questionIds = _connection.Query<int>(query, new { QuestionCode });
            return questionIds.ToList();
        }
        public string GenerateQuestionCode(int indexTypeId, int contentId, int questionId)
        {
            string questionCode = "";
            int subjectId = 0;
            int chapterId = 0;
            int topicId = 0;
            int subTopicId = 0;

            // Fetch subject ID and related hierarchy based on indexTypeId
            if (indexTypeId == 1)  // Chapter
            {
                // Fetch subject directly from chapter
                var chapter = _connection.QueryFirstOrDefault("SELECT SubjectId, ContentIndexId FROM tblContentIndexChapters WHERE ContentIndexId = @contentId", new { contentId });
                if (chapter != null)
                {
                    subjectId = chapter.SubjectId;
                    chapterId = chapter.ContentIndexId;
                }
            }
            else if (indexTypeId == 2)  // Topic
            {
                // Fetch parent chapter from topic, then get subject from the chapter
                var topic = _connection.QueryFirstOrDefault("SELECT ContentIndexId, ContInIdTopic FROM tblContentIndexTopics WHERE ContInIdTopic = @contentId", new { contentId });
                if (topic != null)
                {
                    topicId = topic.ContInIdTopic;
                    chapterId = topic.ContentIndexId;

                    // Now fetch the subject from the parent chapter
                    var chapter = _connection.QueryFirstOrDefault("SELECT SubjectId FROM tblContentIndexChapters WHERE ContentIndexId = @chapterId", new { chapterId });
                    if (chapter != null)
                    {
                        subjectId = chapter.SubjectId;
                    }
                }
            }
            else if (indexTypeId == 3)  // SubTopic
            {
                // Fetch parent topic from subtopic, then get the chapter, and then the subject
                var subTopic = _connection.QueryFirstOrDefault("SELECT ContInIdTopic, ContInIdSubTopic FROM tblContentIndexSubTopics WHERE ContInIdSubTopic = @contentId", new { contentId });
                if (subTopic != null)
                {
                    subTopicId = subTopic.ContInIdSubTopic;
                    topicId = subTopic.ContInIdTopic;

                    // Now fetch the chapter from the parent topic
                    var topic = _connection.QueryFirstOrDefault("SELECT ContentIndexId FROM tblContentIndexTopics WHERE ContInIdTopic = @topicId", new { topicId });
                    if (topic != null)
                    {
                        chapterId = topic.ContentIndexId;

                        // Now fetch the subject from the parent chapter
                        var chapter = _connection.QueryFirstOrDefault("SELECT SubjectId FROM tblContentIndexChapters WHERE ContentIndexId = @chapterId", new { chapterId });
                        if (chapter != null)
                        {
                            subjectId = chapter.SubjectId;
                        }
                    }
                }
            }
            // Construct the question code based on IndexTypeId and IDs
            if (indexTypeId == 1)  // Chapter
            {
                questionCode = $"S{subjectId}C{chapterId}Q{questionId}";
            }
            else if (indexTypeId == 2)  // Topic
            {
                questionCode = $"S{subjectId}C{chapterId}T{topicId}Q{questionId}";
            }
            else if (indexTypeId == 3)  // SubTopic
            {
                questionCode = $"S{subjectId}C{chapterId}T{topicId}ST{subTopicId}Q{questionId}";
            }

            return questionCode;
        }
        private string? GetRoleName(int roleId)
        {
            // Fetch role details based on the roleId
            var role = _connection.QuerySingleOrDefault<dynamic>("SELECT RoleName FROM tblRole WHERE RoleID = @RoleId", new { RoleId = roleId });
            return role?.RoleName; // Return the role name or null if not found
        }
        private async Task<string> GetSubjectiveAnswer(int questionId)
        {
            string query = @"
            SELECT sa.Answer 
            FROM tblAnswersingleanswercategory sa
            INNER JOIN tblAnswerMaster am ON sa.Answerid = am.Answerid
            WHERE am.Questionid = @QuestionId AND am.QuestionTypeid = 2 AND am.IsActive = 1";

            return await _connection.QuerySingleOrDefaultAsync<string>(query, new { QuestionId = questionId });
        }
        public async Task<List<Option>> GetOptionsForQuestion(string questionId)
        {
            string query = @"
            SELECT mc.Answer, mc.Iscorrect 
            FROM tblAnswerMultipleChoiceCategory mc
            INNER JOIN tblAnswerMaster am ON mc.Answerid = am.Answerid
            WHERE am.QuestionCode = @QuestionId AND am.QuestionTypeid = 1";

            var options = await _connection.QueryAsync<Option>(query, new { QuestionId = questionId });

            return options.ToList();
        }

        // Supporting DTO
        public class Option
        {
            public string Answer { get; set; }
            public bool IsCorrect { get; set; }
        }
        public class ContentIndexData
        {
            public int ParentId { get; set; }
            public int ChildId { get; set; }
            public string ContentName { get; set; } = string.Empty;
        }
        public enum QuestionTypesEnum
        {
            MCQ = 1,
            TF = 2,
            SA = 3,
            FB = 4,
            MT = 5,
            MAQ = 6,
            LA = 7,
            VSA = 8,
            MT2 = 9,
            AR = 10,
            NMR = 11,
            CMPR = 12
        }
        public class QuestionUploadData
        {
            public string SubjectName { get; set; }
            public string ChapterName { get; set; }
            public string ConceptName { get; set; }
            public string SubConceptName { get; set; }
            public string QuestionType { get; set; }
            public string QuestionDescription { get; set; }
            public string CourseName { get; set; }
            public string DifficultyLevel { get; set; }
            public string Solution { get; set; }
            public string Explanation { get; set; }
            public string QuestionCode { get; set; }
        }
        private string FileUpload(string base64String)
        {
            if (string.IsNullOrEmpty(base64String) || base64String == "string")
            {
                return string.Empty;
            }
            if (base64String == string.Empty)
            {
                return string.Empty;
            }
            byte[] data = Convert.FromBase64String(base64String);
            string directoryPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "RejectedQuestions");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fileExtension = IsJpeg(data) == true ? ".jpg" : IsPng(data) == true ?
                ".png" : IsGif(data) == true ? ".gif" : IsPdf(data) == true ? ".pdf" : string.Empty;

            string fileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(directoryPath, fileName);
            if (string.IsNullOrEmpty(fileExtension))
            {
                throw new InvalidOperationException("Incorrect file uploaded");
            }
            // Write the byte array to the image file
            File.WriteAllBytes(filePath, data);
            return filePath;
        }
        private string GetFile(string Filename)
        {
            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "RejectedQuestions", Filename);

            if (!File.Exists(filePath))
            {
                return string.Empty;
            }
            byte[] fileBytes = File.ReadAllBytes(filePath);
            string base64String = Convert.ToBase64String(fileBytes);
            return base64String;
        }
        private bool IsJpeg(byte[] bytes)
        {
            // JPEG magic number: 0xFF, 0xD8
            return bytes.Length > 1 && bytes[0] == 0xFF && bytes[1] == 0xD8;
        }
        private bool IsPng(byte[] bytes)
        {
            // PNG magic number: 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A
            return bytes.Length > 7 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47
                && bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A;
        }
        private bool IsGif(byte[] bytes)
        {
            // GIF magic number: "GIF"
            return bytes.Length > 2 && bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46;
        }
        private bool IsPdf(byte[] bytes)
        {
            // PDF magic number: "%PDF"
            return bytes.Length > 4 &&
                   bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46;
        }
    }
}