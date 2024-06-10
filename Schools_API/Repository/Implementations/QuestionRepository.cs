using Dapper;
using Schools_API.DTOs.ServiceResponse;
using Schools_API.Models;
using Schools_API.Repository.Interfaces;
using System.Data;

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
                    var question = new Question
                    {
                        QuestionDescription = request.QuestionDescription,
                        QuestionFormula = request.QuestionFormula,
                        QuestionImage = FileUpload(request.QuestionImage),
                        DifficultyLevelId = request.DifficultyLevelId,
                        QuestionTypeId = request.QuestionTypeId,
                        SubjectIndexId = request.SubjectIndexId,
                        Duration = request.Duration,
                        Occurrence = request.Occurrence,
                        ApprovedStatus = request.ApprovedStatus,
                        ApprovedBy = request.ApprovedBy,
                        ReasonNote = request.ReasonNote,
                        ActualOption = request.ActualOption,
                        Status = true,
                        CreatedBy = request.CreatedBy,
                        CreatedOn = DateTime.Now,
                        Verified = request.Verified,
                        courseid = request.courseid,
                        boardid = request.boardid,
                        classid = request.classid,
                        subjectID = request.subjectID,
                        userid = request.userid,
                        Rejectedby = request.Rejectedby,
                        RejectedReason = request.RejectedReason,
                        APName = request.APName,
                        BoardName = request.BoardName,
                        ClassName = request.ClassName,
                        CourseName = request.CourseName,
                        SubjectName = request.SubjectName
                    };

                    string insertQuery = @"
                INSERT INTO tblQuestion (
                    QuestionDescription, QuestionFormula, QuestionImage, DifficultyLevelId, QuestionTypeId, 
                    SubjectIndexId, Duration, Occurrence, ApprovedStatus, ApprovedBy, ReasonNote, 
                    ActualOption, Status, CreatedBy, CreatedOn,
                    Verified, CourseId, BoardId, ClassId, SubjectId, UserId, 
                    RejectedBy, RejectedReason, APName, BoardName, ClassName, CourseName, SubjectName
                )
                VALUES (
                    @QuestionDescription, @QuestionFormula, @QuestionImage, @DifficultyLevelId, @QuestionTypeId, 
                    @SubjectIndexId, @Duration, @Occurrence, @ApprovedStatus, @ApprovedBy, @ReasonNote, 
                    @ActualOption, @Status, @CreatedBy, @CreatedOn, 
                    @Verified, @CourseId, @BoardId, @ClassId, @SubjectId, @UserId, 
                    @RejectedBy, @RejectedReason, @APName, @BoardName, @ClassName, @CourseName, @SubjectName
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
                            if (questTypedata.Code == "MCQ" || questTypedata.Code == "TF" || questTypedata.Code == "MT1" || questTypedata.Code == "MAQ"
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
                                    request.Answersingleanswercategories.Answerid = Answerid;
                                answer = await _connection.ExecuteAsync(sql, request.Answersingleanswercategories);
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
                        QuestionFormula = request.QuestionFormula,
                        QuestionImage = FileUpload(request.QuestionImage),
                        DifficultyLevelId = request.DifficultyLevelId,
                        QuestionTypeId = request.QuestionTypeId,
                        SubjectIndexId = request.SubjectIndexId,
                        Duration = request.Duration,
                        Occurrence = request.Occurrence,
                        ApprovedStatus = request.ApprovedStatus,
                        ApprovedBy = request.ApprovedBy,
                        ReasonNote = request.ReasonNote,
                        ActualOption = request.ActualOption,
                        Status = request.Status,
                        ModifiedBy = request.ModifiedBy,
                        ModifiedOn = DateTime.Now,
                        Verified = request.Verified,
                        courseid = request.courseid,
                        boardid = request.boardid,
                        classid = request.classid,
                        subjectID = request.subjectID,
                        userid = request.userid,
                        Rejectedby = request.Rejectedby,
                        RejectedReason = request.RejectedReason,
                        APName = request.APName,
                        BoardName = request.BoardName,
                        ClassName = request.ClassName,
                        CourseName = request.CourseName,
                        SubjectName = request.SubjectName
                    };

                    string updateQuery = @"
                UPDATE Questions
                SET
                    QuestionDescription = @QuestionDescription,
                    QuestionFormula = @QuestionFormula,
                    QuestionImage = @QuestionImage,
                    DifficultyLevelId = @DifficultyLevelId,
                    QuestionTypeId = @QuestionTypeId,
                    SubjectIndexId = @SubjectIndexId,
                    Duration = @Duration,
                    Occurrence = @Occurrence,
                    ApprovedStatus = @ApprovedStatus,
                    ApprovedBy = @ApprovedBy,
                    ReasonNote = @ReasonNote,
                    ActualOption = @ActualOption,
                    Status = @Status,
                    ModifiedBy = @ModifiedBy,
                    ModifiedOn = @ModifiedOn,
                    Verified = @Verified,
                    CourseId = @CourseId,
                    BoardId = @BoardId,
                    ClassId = @ClassId,
                    SubjectId = @SubjectId,
                    UserId = @UserId,
                    RejectedBy = @RejectedBy,
                    RejectedReason = @RejectedReason,
                    APName = @APName,
                    BoardName = @BoardName,
                    ClassName = @ClassName,
                    CourseName = @CourseName,
                    SubjectName = @SubjectName
                WHERE QuestionId = @QuestionId";

                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, question);
                    if (rowsAffected > 0)
                    {
                        int count = await _connection.ExecuteScalarAsync<int>("SELECT COUNT(*) " +
                            "FROM tblQIDCourse WHERE QuestionId = @QuestionId", new { request.QuestionId });
                        if (count > 0)
                        {
                            string deleteQuery = @"DELETE FROM tblQIDCourse WHERE QuestionId = @QuestionId";
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
                        string selectQuery = @"SELECT * FROM AnswerMaster WHERE Questionid = @Questionid";

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
                            if (questTypedata.Code == "MCQ" || questTypedata.Code == "TF" || questTypedata.Code == "MT1" || questTypedata.Code == "MAQ"
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
        public async Task<ServiceResponse<List<QuestionDTO>>> GetAllQuestionsList(GetAllQuestionListRequest request)
        {
            try
            {
                string sql = @"SELECT TOP (100) * FROM tblQuestion";
                var data = await _connection.QueryAsync<Question>(sql);
                if (data != null)
                {
                    var response = data.Select(item => new QuestionDTO
                    {
                        QuestionId = item.QuestionId,
                        QuestionDescription = item.QuestionDescription,
                        QuestionFormula = item.QuestionFormula,
                        QuestionImage = GetFile(item.QuestionImage),
                        DifficultyLevelId = item.DifficultyLevelId,
                        QuestionTypeId = item.QuestionTypeId,
                        SubjectIndexId = item.SubjectIndexId,
                        Duration = item.Duration,
                        Occurrence = item.Occurrence,
                        ApprovedStatus = item.ApprovedStatus,
                        ApprovedBy = item.ApprovedBy,
                        ReasonNote = item.ReasonNote,
                        ActualOption = item.ActualOption,
                        Status = item.Status,
                        CreatedBy = item.CreatedBy,
                        CreatedOn = item.CreatedOn,
                        ModifiedBy = item.ModifiedBy,
                        ModifiedOn = item.ModifiedOn,
                        Verified = item.Verified,
                        courseid = item.courseid,
                        boardid = item.boardid,
                        classid = item.classid,
                        subjectID = item.subjectID,
                        userid = item.userid,
                        Rejectedby = item.Rejectedby,
                        RejectedReason = item.RejectedReason,
                        APName = item.APName,
                        BoardName = item.BoardName,
                        ClassName = item.ClassName,
                        CourseName = item.CourseName,
                        SubjectName = item.SubjectName,
                        QIDCourses = GetListOfQIDCourse(item.QuestionId),
                        QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionId),
                        References = GetQuestionReference(item.QuestionId),
                        Answersingleanswercategories = GetSingleAnswer(item.QuestionId),
                        AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionId)
                    }).ToList();
                    var paginatedList = response.Skip((request.PageNumber - 1) * request.PageSize)
                     .Take(request.PageSize)
                     .ToList();
                    if (paginatedList.Count != 0)
                    {
                        return new ServiceResponse<List<QuestionDTO>>(true, "Operation Successful", paginatedList, 200);
                    }
                    else
                    {
                        return new ServiceResponse<List<QuestionDTO>>(false, "No records found", [], 404);
                    }
                }
                else
                {
                    return new ServiceResponse<List<QuestionDTO>>(false, "no records found", [], 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionDTO>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<QuestionDTO>> GetQuestionById(int questionId)
        {
            try
            {
                var response = new QuestionDTO();
                string sql = @"SELECT * FROM tblQuestion WHERE QuestionId = @QuestionId";

                var item = await _connection.QueryFirstOrDefaultAsync<Question>(sql, new { QuestionId = questionId });
                if (item != null)
                {
                    response = new QuestionDTO
                    {
                        QuestionId = item.QuestionId,
                        QuestionDescription = item.QuestionDescription,
                        QuestionFormula = item.QuestionFormula,
                        QuestionImage = GetFile(item.QuestionImage),
                        DifficultyLevelId = item.DifficultyLevelId,
                        QuestionTypeId = item.QuestionTypeId,
                        SubjectIndexId = item.SubjectIndexId,
                        Duration = item.Duration,
                        Occurrence = item.Occurrence,
                        ApprovedStatus = item.ApprovedStatus,
                        ApprovedBy = item.ApprovedBy,
                        ReasonNote = item.ReasonNote,
                        ActualOption = item.ActualOption,
                        Status = item.Status,
                        CreatedBy = item.CreatedBy,
                        CreatedOn = item.CreatedOn,
                        ModifiedBy = item.ModifiedBy,
                        ModifiedOn = item.ModifiedOn,
                        Verified = item.Verified,
                        courseid = item.courseid,
                        boardid = item.boardid,
                        classid = item.classid,
                        subjectID = item.subjectID,
                        userid = item.userid,
                        Rejectedby = item.Rejectedby,
                        RejectedReason = item.RejectedReason,
                        APName = item.APName,
                        BoardName = item.BoardName,
                        ClassName = item.ClassName,
                        CourseName = item.CourseName,
                        SubjectName = item.SubjectName,
                        QIDCourses = GetListOfQIDCourse(item.QuestionId),
                        QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionId),
                        References = GetQuestionReference(item.QuestionId),
                        Answersingleanswercategories = GetSingleAnswer(item.QuestionId),
                        AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionId)
                    };
                    return new ServiceResponse<QuestionDTO>(true, "Operation Successful", response, 200);
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
                    var deleteDuery = @"DELETE FROM [tblQuestionSubjectMapping]
                          WHERE [questionid] = @questionId;";
                    var rowsAffected = await _connection.ExecuteAsync(deleteDuery, new { questionId });
                    if (rowsAffected > 0)
                    {
                        string insertQuery = @"INSERT INTO tblQuestionSubjectMapping (SubjectIndexId, Indexid, questionid, Levelid) 
                           VALUES (@SubjectIndexId, @Indexid, @questionid, @Levelid)";
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
                    string insertQuery = @"INSERT INTO tblQuestionSubjectMapping (SubjectIndexId, Indexid, questionid, Levelid) 
                           VALUES (@SubjectIndexId, @Indexid, @questionid, @Levelid)";
                    var valuesInserted = await _connection.ExecuteAsync(insertQuery, request);
                    return valuesInserted;
                }
            }
            else
            {
                return 0;
            }
        }
        private List<QIDCourse> GetListOfQIDCourse(int questionId)
        {
            var boardquery = @"SELECT * FROM [tblQIDCourse] WHERE QID = @questionId;";

            var data = _connection.Query<QIDCourse>(boardquery, new { questionId });
            return data != null ? data.AsList() : [];
        }
        private Reference GetQuestionReference(int questionId)
        {
            var boardquery = @"SELECT * FROM [tblReference] WHERE QuestionId = @questionId;";

            var data = _connection.QueryFirstOrDefault<Reference>(boardquery, new { questionId });
            return data ?? new Reference();
        }
        private List<QuestionSubjectMapping> GetListOfQuestionSubjectMapping(int questionId)
        {
            var boardquery = @"SELECT * FROM [tblQuestionSubjectMapping] WHERE questionid = @questionId;";

            var data = _connection.Query<QuestionSubjectMapping>(boardquery, new { questionId });
            return data != null ? data.AsList() : [];
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
            string directoryPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "QuestionImages");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fileExtension = IsJpeg(data) == true ? ".jpg" : IsPng(data) == true ? ".png" : IsGif(data) == true ? ".gif" : string.Empty;

            string fileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(directoryPath, fileName);

            // Write the byte array to the image file
            File.WriteAllBytes(filePath, data);
            return filePath;
        }
        private string GetFile(string Filename)
        {
            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "QuestionImages", Filename);

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
        private Answersingleanswercategory GetSingleAnswer(int QuestionId)
        {
            var answerMaster = _connection.QueryFirstOrDefault<AnswerMaster>(@"Select * from tblAnswerMaster where Questionid = @Questionid", new { Questionid = QuestionId });
            if (answerMaster != null)
            {
                string getQuery = @"Select * from [tblAnswersingleanswercategory] where [Answerid] = @Answerid;";
                var response = _connection.QueryFirstOrDefault<Answersingleanswercategory>(getQuery, new { answerMaster.Answerid });
                return response ?? new Answersingleanswercategory();
            }
            else
            {
                return new Answersingleanswercategory();
            }
        }
        private List<AnswerMultipleChoiceCategory> GetMultipleAnswers (int QuestionId)
        {
            var answerMaster = _connection.QueryFirstOrDefault<AnswerMaster>(@"Select * from tblAnswerMaster where Questionid = @Questionid", new { Questionid = QuestionId });
            if (answerMaster != null)
            {
                string getQuery = @"Select * from [tblAnswerMultipleChoiceCategory] where [Answerid] = @Answerid;";
                var response = _connection.Query<AnswerMultipleChoiceCategory>(getQuery, new { answerMaster.Answerid });
                return response.AsList() ?? [];
            }
            else
            {
                return [];
            }
        }
    }
}
