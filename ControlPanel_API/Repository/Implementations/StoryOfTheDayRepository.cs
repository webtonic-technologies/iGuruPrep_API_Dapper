using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using Dapper;
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
                    Question = storyOfTheDayDTO.Question,
                    BoardName = storyOfTheDayDTO.BoardName,
                    ClassName = storyOfTheDayDTO.ClassName,
                    PostTime = storyOfTheDayDTO.PostTime,
                    DateAndTime = storyOfTheDayDTO.PostTime,
                    Answer = storyOfTheDayDTO.Answer,
                    AnswerRevealTime = new TimeSpan(0, storyOfTheDayDTO.AnswerRevealTime.Value.Hours, storyOfTheDayDTO.AnswerRevealTime.Value.Minutes, storyOfTheDayDTO.AnswerRevealTime.Value.Seconds, storyOfTheDayDTO.AnswerRevealTime.Value.Microseconds),
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
                        @"INSERT INTO tblSOTD (Question, BoardName, ClassName, PostTime, DateAndTime, Answer, AnswerRevealTime, Status, UploadImage) 
                VALUES (@Question, @BoardName, @ClassName, @PostTime, @DateAndTime, @Answer, @AnswerRevealTime, @Status, @UploadImage)",
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
                    @"SELECT * FROM tblSOTD");

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
                return new ServiceResponse<IEnumerable<StoryOfTheDayDTO>> (false, ex.Message, new List<StoryOfTheDayDTO>(), 500);
            }
        }

        public async Task<ServiceResponse<StoryOfTheDayDTO>> GetStoryOfTheDayById(int id)
        {
            try
            {
                var storyOfTheDay = await _connection.QueryFirstOrDefaultAsync<StoryOfTheDayDTO>(
                    @"SELECT * FROM tblSOTD WHERE QuestionId = @QuestionId",
                    new { QuestionId = id });

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
                    "SELECT UploadImage FROM tblSOTD WHERE QuestionId = @QuestionId",
                    new { QuestionId = id });

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
                    @"SELECT * FROM tblSOTD WHERE QuestionId = @QuestionId",
                    new { storyOfTheDayDTO.QuestionId });

                if (storyOfTheDay == null)
                    throw new Exception("Story of the day not found");

                storyOfTheDay.Question = storyOfTheDayDTO.Question;
                storyOfTheDay.BoardName = storyOfTheDayDTO.BoardName;
                storyOfTheDay.ClassName = storyOfTheDayDTO.ClassName;
                storyOfTheDay.PostTime = storyOfTheDayDTO.PostTime;
                storyOfTheDay.DateAndTime = storyOfTheDayDTO.PostTime;
                storyOfTheDay.Answer = storyOfTheDayDTO.Answer;
                storyOfTheDay.AnswerRevealTime = new TimeSpan(0, storyOfTheDayDTO.AnswerRevealTime.Value.Hours, storyOfTheDayDTO.AnswerRevealTime.Value.Minutes, storyOfTheDayDTO.AnswerRevealTime.Value.Seconds, storyOfTheDayDTO.AnswerRevealTime.Value.Microseconds);
                storyOfTheDay.Status = storyOfTheDayDTO.Status;

               int rowsAffected = await _connection.ExecuteAsync(
                    @"UPDATE tblSOTD 
            SET Question = @Question, BoardName = @BoardName, ClassName = @ClassName, PostTime = @PostTime, DateAndTime = @DateAndTime, 
            Answer = @Answer, AnswerRevealTime = @AnswerRevealTime, Status = @Status 
            WHERE QuestionId = @QuestionId",
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
                    "SELECT UploadImage FROM tblSOTD WHERE QuestionId = @QuestionId",
                    new { storyOfTheDayDTO.QuestionId });

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
                    "UPDATE tblSOTD SET UploadImage = @UploadImage WHERE QuestionId = @QuestionId",
                    new { storyOfTheDay.UploadImage, storyOfTheDayDTO.QuestionId });
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
    }
}
