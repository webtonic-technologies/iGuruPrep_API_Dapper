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
                if (request.QuestionId == 0)
                {
                    string imagePath = string.Empty;
                    if (request.QuestionImage != null)
                    {
                        var folderName = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets","QuestionImages");
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
                    };

                    string insertQuery = @"
            INSERT INTO tblQuestion (QuestionDescription, QuestionFormula, QuestionImage, DifficultyLevelId,
                                   QuestionTypeId, SubjectIndexId, Duration, Occurrence, ComprehensiveId,
                                   ApprovedStatus, ApprovedBy, ReasonNote, ActualOption, Status, CreatedBy,
                                   CreatedOn, ModifiedBy, ModifiedOn, Verified)
            VALUES (@QuestionDescription, @QuestionFormula, @QuestionImage, @DifficultyLevelId,
                    @QuestionTypeId, @SubjectIndexId, @Duration, @Occurrence, @ComprehensiveId,
                    @ApprovedStatus, @ApprovedBy, @ReasonNote, @ActualOption, @Status, @CreatedBy,
                    @CreatedOn, @ModifiedBy, @ModifiedOn, @Verified);

            SELECT SCOPE_IDENTITY()";

                    int questionId = await _connection.QuerySingleOrDefaultAsync<int>(insertQuery, question);
                    if (questionId != 0)
                    {
                        var data = await AddUpdateQIDCourses(request.QIDCourses, questionId);

                        var rowsAffected = await AddUpdateReference(request.References, questionId);
                        if (rowsAffected > 0 && data > 0)
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
                    return new ServiceResponse<string>(true, string.Empty, string.Empty, 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
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
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.QuestionImage.FileName);
                    var newFilePath = Path.Combine(uploads, fileName);
                    using (var fileStream = new FileStream(newFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await request.QuestionImage.CopyToAsync(fileStream);
                    }
                    questionData.QuestionImage = fileName;
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

            string insertQuery = @"
            INSERT INTO QIDCourses (QID, CourseID, LevelId, Status, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
            VALUES (@QID, @CourseID, @LevelId, @Status, @CreatedBy, @CreatedDate, @ModifiedBy, @ModifiedDate)";

            string updateQuery = @"
            UPDATE QIDCourses
            SET QID = @QID,
                CourseID = @CourseID,
                LevelId = @LevelId,
                Status = @Status,
                CreatedBy = @CreatedBy,
                CreatedDate = @CreatedDate,
                ModifiedBy = @ModifiedBy,
                ModifiedDate = @ModifiedDate
            WHERE QIDCourseID = @QIDCourseID";
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
                    ModifiedDate = DateTime.Now
                };
                if (data.QIDCourseID == 0)
                {
                     rowsAffected = await _connection.ExecuteAsync(insertQuery, newQIDCourse);
                }
                else
                {
                     rowsAffected = await _connection.ExecuteAsync(updateQuery, newQIDCourse);
                }
            }
            return rowsAffected;
        }
        private async Task<int> AddUpdateReference(Reference? request, int questionId)
        {
            string insertQuery = @"
                    INSERT INTO References (SubjectIndexId, Type, ReferenceNotes, ReferenceURL, QuestionId,
                                            Status, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
                    VALUES (@SubjectIndexId, @Type, @ReferenceNotes, @ReferenceURL, @QuestionId,
                            @Status, @CreatedBy, @CreatedOn, @ModifiedBy, @ModifiedOn)";

            string updateQuery = @"
                    UPDATE References
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
                ModifiedOn = DateTime.Now
            };
            int rowsAffected;
            if (request.ReferenceId == 0)
            {
                rowsAffected = await _connection.ExecuteAsync(insertQuery, newReference);
            }
            else
            {
                rowsAffected = await _connection.ExecuteAsync(updateQuery, newReference);
            }
            return rowsAffected;
        }
    }
}
