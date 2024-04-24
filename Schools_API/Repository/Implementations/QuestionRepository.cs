using Dapper;
using Schools_API.DTOs.ServiceResponse;
using Schools_API.Models;
using Schools_API.Repository.Interfaces;
using System.Data;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        public async Task<ServiceResponse<string>> AddQuestion(QuestionDTO request)
        {
            try
            {
                string imagePath = string.Empty;
                if (request.QuestionId == 0)
                {
                    if (request.QuestionImage != null)
                    {
                        var folderName = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "QuestionImages");
                        if (!Directory.Exists(folderName))
                        {
                            Directory.CreateDirectory(folderName);
                        }
                        var fileName = Path.GetFileNameWithoutExtension(request.QuestionImage.FileName) + "_" + Guid.NewGuid().ToString() + Path.GetExtension(request.QuestionImage.FileName);
                        var filePath = Path.Combine(folderName, fileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            request.QuestionImage.CopyTo(fileStream);
                        }
                        imagePath = fileName; // Set the path where the image is saved
                    }
                    else
                    {
                        imagePath = string.Empty;
                    }

                    var question = new Question
                    {
                        Status = request.Status,
                        CreatedBy = request.CreatedBy,
                        CreatedOn = request.CreatedOn,
                        ModifiedBy = request.ModifiedBy,
                        ModifiedOn = request.ModifiedOn,
                        QuestionDescription = request.QuestionDescription,
                        QuestionFormula = request.QuestionFormula,
                        QuestionImage = imagePath,
                        DifficultyLevelId = request.DifficultyLevelId,
                        QuestionTypeId = request.QuestionTypeId,
                        SubjectIndexId = request.SubjectIndexId,
                        courseid = request.courseid,
                        boardid = request.boardid,
                        classid = request.classid
                    };

                    string insertQuery = @"
            INSERT INTO tblQuestion (QuestionDescription, QuestionFormula, QuestionImage, DifficultyLevelId,
                                   QuestionTypeId, SubjectIndexId, Duration, Occurrence, ComprehensiveId,
                                   ApprovedStatus, ApprovedBy, ReasonNote, ActualOption, Status, CreatedBy,
                                   CreatedOn, ModifiedBy, ModifiedOn, Verified, courseid, boardid, classid)
            VALUES (@QuestionDescription, @QuestionFormula, @QuestionImage, @DifficultyLevelId,
                    @QuestionTypeId, @SubjectIndexId, @Duration, @Occurrence, @ComprehensiveId,
                    @ApprovedStatus, @ApprovedBy, @ReasonNote, @ActualOption, @Status, @CreatedBy,
                    @CreatedOn, @ModifiedBy, @ModifiedOn, @Verified, @courseid, @boardid, @classid);

            SELECT SCOPE_IDENTITY()";

                    int questionId = await _connection.QuerySingleOrDefaultAsync<int>(insertQuery, question);
                    if (questionId != 0)
                    {
                        //courses and difficulty mapping
                        var data = await AddUpdateQIDCourses(request.QIDCourses, questionId);

                        // reference mapping
                        var rowsAffected = await AddUpdateReference(request.References, questionId);

                        //subject details
                        var quesSub = await AddUpdateQuestionSubjectMap(request.QuestionSubjectMapping, questionId);

                        string getQuesType = @"select * from tblQBQuestionType where QuestionTypeID = @QuestionTypeID;";

                        var questTypedata = await _connection.QuerySingleAsync<QuestionTypes>(getQuesType, new { QuestionTypeID = request.QuestionTypeId });
                        var answer = 0;
                        if (questTypedata != null)
                        {
                            if (questTypedata.AnswerCode == "MCC" || questTypedata.AnswerCode == "MTC")
                            {
                                if (request.AnswerMultipleChoiceCategory != null)
                                {
                                    string insertAnsQuery = @"INSERT INTO tblAnswerMultipleChoiceCategory
                           (Answerid, Answer, Iscorrect, Matchid) 
                           VALUES (@AnswerId, @Answer, @IsCorrect, @MatchId);";
                                    answer = await _connection.ExecuteAsync(insertAnsQuery, request.AnswerMultipleChoiceCategory);

                                }
                            }
                            else
                            {
                                string sql = @"INSERT INTO tblAnswersingleanswercategory 
                                             ( Answerid, Answer)
                                             VALUES ( @AnswerId, @Answer);";
                                answer = await _connection.ExecuteAsync(sql, request.Answersingleanswercategory);
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
                        Status = request.Status,
                        CreatedBy = request.CreatedBy,
                        CreatedOn = request.CreatedOn,
                        ModifiedBy = request.ModifiedBy,
                        ModifiedOn = request.ModifiedOn,
                        QuestionDescription = request.QuestionDescription,
                        QuestionFormula = request.QuestionFormula,
                        QuestionImage = imagePath,
                        DifficultyLevelId = request.DifficultyLevelId,
                        QuestionTypeId = request.QuestionTypeId,
                        SubjectIndexId = request.SubjectIndexId,
                        courseid = request.courseid,
                        boardid = request.boardid,
                        classid = request.classid,
                        QuestionId = request.QuestionId
                    };

                    string updateQuery = @"UPDATE tblQuestion
                           SET QuestionDescription = @QuestionDescription,
                               QuestionFormula = @QuestionFormula,
                               QuestionImage = @QuestionImage,
                               DifficultyLevelId = @DifficultyLevelId,
                               QuestionTypeId = @QuestionTypeId,
                               SubjectIndexId = @SubjectIndexId,
                               Duration = @Duration,
                               Occurrence = @Occurrence,
                               ComprehensiveId = @ComprehensiveId,
                               ApprovedStatus = @ApprovedStatus,
                               ApprovedBy = @ApprovedBy,
                               ReasonNote = @ReasonNote,
                               ActualOption = @ActualOption,
                               Status = @Status,
                               CreatedBy = @CreatedBy,
                               CreatedOn = @CreatedOn,
                               ModifiedBy = @ModifiedBy,
                               ModifiedOn = @ModifiedOn,
                               Verified = @Verified,
                               courseid = @CourseId,
                               boardid = @BoardId,
                               classid = @ClassId
                           WHERE QuestionId = @QuestionId";

                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, question);
                    if (rowsAffected > 0)
                    {
                        int count = await _connection.ExecuteScalarAsync<int>("SELECT COUNT(*) " +
                            "FROM tblQIDCourse WHERE QuestionId = @QuestionId", new { request.QuestionId });
                        if (count > 0)
                        {
                            string deleteQuery = @"
        DELETE FROM tblQIDCourse
        WHERE QuestionId = @QuestionId";
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
                        var quesSub = await AddUpdateQuestionSubjectMap(request.QuestionSubjectMapping, request.QuestionId);

                        if (rowsAffected2 > 0 && quesSub > 0)
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
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<List<Question>>> GetAllQuestionsList()
        {
            try
            {
                string sql = @"
                SELECT QuestionId, QuestionDescription, QuestionFormula,
                       DifficultyLevelId, QuestionTypeId, SubjectIndexId,
                       Duration, Occurrence, ComprehensiveId, ApprovedStatus,
                       ApprovedBy, ReasonNote, ActualOption, Status,
                       CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, Verified,
                       courseid, boardid, classid
                FROM tblQuestion";

                
                var data = await _connection.QueryAsync<Question>(sql);
                if(data != null)
                {
                    return new ServiceResponse<List<Question>>(true, "Operation Successful", data.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<Question>>(false, "no records found", [], 500);
                }
            }
            catch(Exception ex)
            {
                return new ServiceResponse<List<Question>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<QuestionDTO>> GetQuestionById(int questionId)
        {
            try
            {
                var response = new QuestionDTO();
                string sql = @"
                SELECT QuestionId, QuestionDescription, QuestionFormula,
                       DifficultyLevelId, QuestionTypeId, SubjectIndexId,
                       Duration, Occurrence, ComprehensiveId, ApprovedStatus,
                       ApprovedBy, ReasonNote, ActualOption, Status,
                       CreatedBy, CreatedOn, ModifiedBy, ModifiedOn, Verified,
                       courseid, boardid, classid
                FROM tblQuestion
                WHERE QuestionId = @QuestionId";

                var data = await _connection.QueryFirstOrDefaultAsync<QuestionDTO>(sql, new { QuestionId = questionId });
                if (data != null)
                {
                    var refData = await _connection.QueryFirstOrDefaultAsync<Reference>("SELECT * from tblReference where QuestionId = @QuestionId", new { QuestionId = questionId });
                    if (refData != null)
                    {
                        response.References = refData;
                    }
                    var diffCour = await _connection.QueryAsync<QIDCourseDTO>("SELECT * from tblQIDCourse where QuestionId = @QuestionId", new { QuestionId = questionId });
                    if (diffCour != null)
                    {
                        response.QIDCourses = diffCour.AsList();
                    }
                    return new ServiceResponse<QuestionDTO>(true, "Operation Successful", data, 200);
                }
                else
                {

                    return new ServiceResponse<QuestionDTO>(false, "no records found", new QuestionDTO(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<QuestionDTO>(false, ex.Message, new QuestionDTO(), 500);
            }
        }
        public async Task<ServiceResponse<string>> UpdateQuestionImageFile(QuestionImageDTO request)
        {
            try
            {
                var questionData = await _connection.QueryFirstOrDefaultAsync<Question>(
                    "SELECT QuestionImage FROM tblQuestion WHERE QuestionId = @QuestionId",
                    new { request.QuestionId });

                if (questionData == null)
                {
                    throw new Exception("Question not found");
                }

                var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "QuestionImages", questionData.QuestionImage);
                if (File.Exists(filePath) && !string.IsNullOrWhiteSpace(questionData.QuestionImage))
                {
                    File.Delete(filePath);
                }

                if (questionData.QuestionImage != null)
                {
                    var uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "QuestionImages");
                    if (!Directory.Exists(uploads))
                    {
                        Directory.CreateDirectory(uploads);
                    }
                    if(request.QuestionImage != null)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.QuestionImage.FileName);
                        var newFilePath = Path.Combine(uploads, fileName);
                        using (var fileStream = new FileStream(newFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await request.QuestionImage.CopyToAsync(fileStream);
                        }
                        questionData.QuestionImage = fileName;
                    }
                }

                int rowsAffected = await _connection.ExecuteAsync(
                    "UPDATE tblQuestion SET QuestionImage = @QuestionImage WHERE QuestionId = @QuestionId",
                    new { questionData.QuestionImage, request.QuestionId });
                if (rowsAffected > 0)
                {
                    return new ServiceResponse<string>(true, "Operation Successful", "Question Updated Successfully", 200);
                }
                else
                {
                    return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        private async Task<int> AddUpdateQIDCourses(List<QIDCourseDTO>? request, int questionId)
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
                    SubjectIndexId = request.SubjectIndexId,
                    Type = request.Type,
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
                    INSERT INTO tblReference (SubjectIndexId, Type, ReferenceNotes, ReferenceURL, QuestionId,
                                            Status, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
                    VALUES (@SubjectIndexId, @Type, @ReferenceNotes, @ReferenceURL, @QuestionId,
                            @Status, @CreatedBy, @CreatedOn, @ModifiedBy, @ModifiedOn)";

                    rowsAffected = await _connection.ExecuteAsync(insertQuery, newReference);
                }
                else
                {
                    string updateQuery = @"
                    UPDATE tblReference
                    SET SubjectIndexId = @SubjectIndexId,
                        Type = @Type,
                        ReferenceNotes = @ReferenceNotes,
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
        private async Task<int> AddUpdateQuestionSubjectMap(QuestionSubjectMapping? request, int questionId)
        {
            if (request != null)
            {
                var newMapping = new QuestionSubjectMapping
                {
                    Indexid = request.Indexid,
                    Levelid = request.Levelid,
                    questionid = questionId,
                    QuestionSubjectid = request.QuestionSubjectid,
                    SubjectIndexId = request.Indexid
                };
                int rowsAffected;
                if (request.QuestionSubjectid == 0)
                {
                    string insertQuery = @"INSERT INTO tblQuestionSubjectMapping (SubjectIndexId, Indexid, questionid, Levelid) 
                           VALUES (@SubjectIndexId, @Indexid, @questionid, @Levelid)";
                    rowsAffected = await _connection.ExecuteAsync(insertQuery, newMapping);
                }
                else
                {
                    string updateQuery = @"UPDATE tblQuestionSubjectMapping 
                           SET SubjectIndexId = @SubjectIndexId,
                               Indexid = @Indexid,
                               questionid = @questionid,
                               Levelid = @Levelid
                           WHERE QuestionSubjectid = @QuestionSubjectid";
                    rowsAffected = await _connection.ExecuteAsync(updateQuery, newMapping);
                }
                return rowsAffected;
            }
            else
            {
                return 0;
            }
        }
    }
}
