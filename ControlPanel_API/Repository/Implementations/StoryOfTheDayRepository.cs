using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using Dapper;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace ControlPanel_API.Repository.Implementations
{
    public class StoryOfTheDayRepository : IStoryOfTheDayRepository
    {
        private readonly IDbConnection _connection;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public StoryOfTheDayRepository(IDbConnection connection, IWebHostEnvironment hostingEnvironment)
        {
            _connection = connection;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<ServiceResponse<string>> AddNewStoryOfTheDay(StoryOfTheDayDTO storyOfTheDayDTO)
        {
            try
            {
                var storyOfTheDay = new StoryOfTheDay
                {
                    Questionid = storyOfTheDayDTO.Questionid,
                    QuestionName = storyOfTheDayDTO.QuestionName,
                    BoardID = storyOfTheDayDTO.BoardID,
                    ClassID = storyOfTheDayDTO.ClassID,
                    BoardName = storyOfTheDayDTO.BoardName,
                    ClassName = storyOfTheDayDTO.ClassName,
                    PostTime = storyOfTheDayDTO.PostTime,
                    DateAndTime = storyOfTheDayDTO.PostTime,
                    Answer = storyOfTheDayDTO.Answer,
                    AnswerRevealTime = storyOfTheDayDTO.AnswerRevealTime != null ? new TimeSpan(0, storyOfTheDayDTO.AnswerRevealTime.Value.Hours, storyOfTheDayDTO.AnswerRevealTime.Value.Minutes, storyOfTheDayDTO.AnswerRevealTime.Value.Seconds, storyOfTheDayDTO.AnswerRevealTime.Value.Microseconds) : null,
                    Status = storyOfTheDayDTO.Status,
                    UploadImage = string.Empty // Default to empty string if no image is uploaded
                };

                if (storyOfTheDayDTO.UploadImage != null)
                {
                    var uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "StoryOfTheDay");
                    if (!Directory.Exists(uploads))
                    {
                        Directory.CreateDirectory(uploads);
                    }
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(storyOfTheDayDTO.UploadImage.FileName);
                    var filePath = Path.Combine(uploads, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await storyOfTheDayDTO.UploadImage.CopyToAsync(fileStream);
                    }
                    storyOfTheDay.UploadImage = fileName;
                }

                int rowsAffected = await _connection.ExecuteAsync(
                     @"INSERT INTO tblSOTD (Questionid, QuestionName, BoardID, ClassID, BoardName, ClassName, PostTime, DateAndTime, Answer, AnswerRevealTime, Status, UploadImage) 
                VALUES (@Questionid, @QuestionName, @BoardID, @ClassID, @BoardName, @ClassName, @PostTime, @DateAndTime, @Answer, @AnswerRevealTime, @Status, @UploadImage)",
                     storyOfTheDay);

                if (rowsAffected > 0)
                {
                    return new ServiceResponse<string>(true, "Operation Successful", "SOTD Added Successfully", 200);
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

        public async Task<ServiceResponse<bool>> DeleteStoryOfTheDay(int id)
        {
            try
            {
                var storyOfTheDay = await _connection.QueryFirstOrDefaultAsync<StoryOfTheDay>(
                    "SELECT UploadImage FROM tblSOTD WHERE QuestionId = @QuestionId",
                    new { QuestionId = id });

                if (storyOfTheDay == null)
                {
                    throw new Exception("Story of the day not found");
                }

                var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "StoryOfTheDay", storyOfTheDay.UploadImage);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                int rowsAffected = await _connection.ExecuteAsync(
                    "DELETE FROM tblSOTD WHERE QuestionId = @QuestionId",
                    new { QuestionId = id });

                if (rowsAffected > 0)
                {
                    return new ServiceResponse<bool>(true, "Operation Successful", true, 200);
                }
                else
                {
                    return new ServiceResponse<bool>(false, "Opertion Failed", false, 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }

        public async Task<ServiceResponse<IEnumerable<StoryOfTheDayDTO>>> GetAllStoryOfTheDay()
        {
            try
            {
                var storyOfTheDays = await _connection.QueryAsync<StoryOfTheDayDTO>(
                    @"SELECT StoryId, Questionid, QuestionName, BoardID, ClassID, BoardName, ClassName, PostTime, DateAndTime, Answer, AnswerRevealTime, Status FROM tblSOTD");
                if (storyOfTheDays != null)
                {
                    return new ServiceResponse<IEnumerable<StoryOfTheDayDTO>>(true, "Records Found", storyOfTheDays.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<IEnumerable<StoryOfTheDayDTO>>(false, "Records Not Found", new List<StoryOfTheDayDTO>(), 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<IEnumerable<StoryOfTheDayDTO>>(false, ex.Message, new List<StoryOfTheDayDTO>(), 500);
            }
        }

        public async Task<ServiceResponse<StoryOfTheDayDTO>> GetStoryOfTheDayById(int id)
        {
            try
            {
                var storyOfTheDay = await _connection.QueryFirstOrDefaultAsync<StoryOfTheDayDTO>(
                    @"SELECT StoryId, Questionid, QuestionName, BoardID, ClassID, BoardName, ClassName, PostTime, DateAndTime, Answer, AnswerRevealTime, Status FROM tblSOTD WHERE StoryId = @StoryId",
                    new { StoryId = id });

                if (storyOfTheDay != null)
                {
                    return new ServiceResponse<StoryOfTheDayDTO>(true, "Operation Successful", storyOfTheDay, 200);
                }
                else
                {
                    return new ServiceResponse<StoryOfTheDayDTO>(false, "Opertion Failed", new StoryOfTheDayDTO(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<StoryOfTheDayDTO>(false, ex.Message, new StoryOfTheDayDTO(), 500);
            }
        }

        public async Task<ServiceResponse<byte[]>> GetStoryOfTheDayFileById(int id)
        {
            try
            {
                var storyOfTheDay = await _connection.QueryFirstOrDefaultAsync<StoryOfTheDay>(
                    "SELECT UploadImage FROM tblSOTD WHERE StoryId = @StoryId",
                    new { StoryId = id });

                if (storyOfTheDay == null)
                {
                    throw new Exception("Story of the day not found");
                }

                var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "StoryOfTheDay", storyOfTheDay.UploadImage);

                if (!File.Exists(filePath))
                {
                    throw new Exception("File not found");
                }
                var fileBytes = await File.ReadAllBytesAsync(filePath);

                return new ServiceResponse<byte[]>(true, "Record Found", fileBytes, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<byte[]>(false, ex.Message, Array.Empty<byte>(), 500);
            }
        }

        public async Task<ServiceResponse<string>> UpdateStoryOfTheDay(UpdateStoryOfTheDayDTO storyOfTheDayDTO)
        {
            try
            {
                var storyOfTheDay = await _connection.QueryFirstOrDefaultAsync<StoryOfTheDay>(
                    @"SELECT * FROM tblSOTD WHERE StoryId = @StoryId",
                    new { storyOfTheDayDTO.StoryId });

                if (storyOfTheDay == null)
                    throw new Exception("Story of the day not found");

                storyOfTheDay.Questionid = storyOfTheDayDTO.Questionid;
                storyOfTheDay.QuestionName = storyOfTheDayDTO.QuestionName;
                storyOfTheDay.BoardID = storyOfTheDayDTO.BoardID;
                storyOfTheDay.ClassID = storyOfTheDayDTO.ClassID;
                storyOfTheDay.BoardName = storyOfTheDayDTO.BoardName;
                storyOfTheDay.ClassName = storyOfTheDayDTO.ClassName;
                storyOfTheDay.PostTime = storyOfTheDayDTO.PostTime;
                storyOfTheDay.DateAndTime = storyOfTheDayDTO.PostTime;
                storyOfTheDay.Answer = storyOfTheDayDTO.Answer;
                storyOfTheDay.AnswerRevealTime = storyOfTheDayDTO.AnswerRevealTime != null ? new TimeSpan(0, storyOfTheDayDTO.AnswerRevealTime.Value.Hours, storyOfTheDayDTO.AnswerRevealTime.Value.Minutes, storyOfTheDayDTO.AnswerRevealTime.Value.Seconds, storyOfTheDayDTO.AnswerRevealTime.Value.Microseconds) : null;
                storyOfTheDay.Status = storyOfTheDayDTO.Status;

                int rowsAffected = await _connection.ExecuteAsync(
                     @"UPDATE tblSOTD 
            SET BoardName = @BoardName, ClassName = @ClassName, PostTime = @PostTime, DateAndTime = @DateAndTime, 
            Answer = @Answer, AnswerRevealTime = @AnswerRevealTime, Status = @Status, Questionid = @Questionid, QuestionName = @QuestionName, BoardID = @BoardID, ClassID = @ClassID 
            WHERE StoryId = @StoryId",
                     storyOfTheDay);

                if (rowsAffected > 0)
                {
                    return new ServiceResponse<string>(true, "Operation Successful", "SOTD Updated Successfully", 200);
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

        public async Task<ServiceResponse<string>> UpdateStoryOfTheDayFile(StoryOfTheDayIdAndFileDTO storyOfTheDayDTO)
        {
            try
            {
                var storyOfTheDay = await _connection.QueryFirstOrDefaultAsync<StoryOfTheDay>(
                    "SELECT UploadImage FROM tblSOTD WHERE StoryId = @StoryId",
                    new { storyOfTheDayDTO.StoryId });

                if (storyOfTheDay == null)
                {
                    throw new Exception("Story of the day not found");
                }

                var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "StoryOfTheDay", storyOfTheDay.UploadImage);
                if (File.Exists(filePath) && !string.IsNullOrWhiteSpace(storyOfTheDay.UploadImage))
                {
                    File.Delete(filePath);
                }

                if (storyOfTheDayDTO.UploadImage != null)
                {
                    var uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "StoryOfTheDay");
                    if (!Directory.Exists(uploads))
                    {
                        Directory.CreateDirectory(uploads);
                    }
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(storyOfTheDayDTO.UploadImage.FileName);
                    var newFilePath = Path.Combine(uploads, fileName);
                    using (var fileStream = new FileStream(newFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await storyOfTheDayDTO.UploadImage.CopyToAsync(fileStream);
                    }
                    storyOfTheDay.UploadImage = fileName;
                }

                int rowsAffected = await _connection.ExecuteAsync(
                    "UPDATE tblSOTD SET UploadImage = @UploadImage WHERE StoryId = @StoryId",
                    new { storyOfTheDay.UploadImage, storyOfTheDayDTO.StoryId });
                if (rowsAffected > 0)
                {
                    return new ServiceResponse<string>(true, "Operation Successful", "SOTD Updated Successfully", 200);
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
        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                var sotd = await GetStoryOfTheDayById(id);

                if (sotd.Data != null)
                {
                    sotd.Data.Status = sotd.Data.Status == 1 ? sotd.Data.Status = 0 : sotd.Data.Status = 1;

                    string sql = "UPDATE [tblSOTD] SET Status = @Status WHERE [StoryId] = @StoryId";

                    int rowsAffected = await _connection.ExecuteAsync(sql, new { sotd.Data.Status, StoryId = id });
                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<bool>(true, "Operation Successful", true, 200);
                    }
                    else
                    {
                        return new ServiceResponse<bool>(false, "Opertion Failed", false, 500);
                    }
                }
                else
                {
                    return new ServiceResponse<bool>(false, "Record not Found", false, 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }
    }
}
