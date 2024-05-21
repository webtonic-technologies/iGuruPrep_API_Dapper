using Course_API.DTOs;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;
using Course_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace Course_API.Repository.Implementations
{
    public class ContentMasterRepository : IContentMasterRepository
    {
        private readonly IDbConnection _connection;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public ContentMasterRepository(IDbConnection connection, IWebHostEnvironment hostingEnvironment)
        {
            _connection = connection;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<ServiceResponse<string>> AddUpdateContent(ContentMaster request)
        {
            try
            {
                if (request.contentid == 0)
                {
                    var insertQuery = @"INSERT INTO tblContentMaster (subjectindexid, boardId, classId, courseId, subjectId, fileName, PathURL, createdon, createdby)
                VALUES (@subjectindexid, @boardId, @classId, @courseId, @subjectId, @fileName, @PathURL, @createdon, @createdby);";
                    var content = new ContentMaster
                    {
                        boardId = request.boardId,
                        classId = request.classId,
                        courseId = request.courseId,
                        fileName = PDFUpload(request.fileName),
                        PathURL = VideoUpload(request.PathURL),
                        subjectindexid = request.subjectindexid,
                        subjectId = request.subjectId,
                        createdby = request.createdby,
                        createdon = DateTime.Now
                    };

                    int rowsAffected = await _connection.ExecuteAsync(insertQuery, content);
                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Content Added Successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                    }
                }
                else
                {
                    var updateQuery = @"UPDATE tblContentMaster
                SET subjectindexid = @subjectindexid,
                    boardId = @boardId,
                    classId = @classId,
                    courseId = @courseId,
                    subjectId = @subjectId,
                    fileName = @fileName,
                    PathURL = @PathURL,
                    modifiedon = @modifiedon,
                    modifiedby = @modifiedby
                WHERE contentid = @contentid;";
                    var content = new ContentMaster
                    {
                        boardId = request.boardId,
                        classId = request.classId,
                        courseId = request.courseId,
                        fileName = PDFUpload(request.fileName),
                        PathURL = VideoUpload(request.PathURL),
                        subjectindexid = request.subjectindexid,
                        subjectId = request.subjectId,
                        modifiedby = request.modifiedby,
                        modifiedon = DateTime.Now,
                        contentid = request.contentid
                    };
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, content);
                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Content Updated Successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<ContentMaster>> GetContentById(int ContentId)
        {
            try
            {
                string selectQuery = @"
            SELECT *
            FROM tblContentMaster
            WHERE contentid = @ContentId";
                var data = await _connection.QuerySingleOrDefaultAsync<ContentMaster>(selectQuery, new { ContentId });
                if (data != null)
                {
                    data.PathURL = GetVideo(data.PathURL);
                    data.fileName = GetPDF(data.fileName);
                    return new ServiceResponse<ContentMaster>(true, "Operation Successful", data, 200);
                }
                else
                {
                    return new ServiceResponse<ContentMaster>(false, "Opertion Failed", new ContentMaster(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ContentMaster>(false, ex.Message, new ContentMaster(), 500);
            }
        }
        public async Task<ServiceResponse<List<ContentMaster>>> GetContentList()
        {
            try
            {
                string selectQuery = @"
            SELECT *
            FROM tblContentMaster";
                var data = await _connection.QueryAsync<ContentMaster>(selectQuery);
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        item.fileName = GetPDF(item.fileName);
                        item.PathURL = GetVideo(item.PathURL);
                    }
                    return new ServiceResponse<List<ContentMaster>>(true, "Operation Successful", data.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<ContentMaster>>(false, "Opertion Failed", new List<ContentMaster>(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ContentMaster>>(false, ex.Message, new List<ContentMaster>(), 500);
            }
        }
        public async Task<ServiceResponse<List<SubjectContentIndexDTO>>> GetListOfSubjectContent(SubjectContentIndexRequestDTO request)
        {
            try
            {
                List<SubjectContentIndexDTO> resposne = [];
                string query = @" SELECT * FROM tblQBSubjectContentIndex WHERE SubjectId = @SubjectId 
                                AND classid = @ClassId AND courseid = @CourseId AND boardid = @BoardId";
                var data = await _connection.QueryAsync(query, request);
                if(data != null)
                {
                    foreach (var item in data)
                    {
                        var responseData = new SubjectContentIndexDTO
                        {
                            boardid = item.boardid,
                            classid = item.classid,
                            ContentName = item.contentname,
                            courseid = item.courseid,
                            DisplayOrder = item.displayorder,
                            IndexTypeId = item.indexid,
                            IsSubjective = item.issubjective,
                            ParentLevel = item.parentlevel,
                            SubjectId = item.subjectid,
                            SubjectIndexId = item.subjectindexid,
                        };
                        resposne.Add(responseData);
                    }
                    return new ServiceResponse<List<SubjectContentIndexDTO>>(true, "Records found", resposne, 200);
                }
                else
                {
                    return new ServiceResponse<List<SubjectContentIndexDTO>>(false, " No Records found", resposne, 204);
                }
               
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<SubjectContentIndexDTO>>(false, ex.Message, new List<SubjectContentIndexDTO>(), 500);
            }
        }
        private string PDFUpload(string pdf)
        {
            if (string.IsNullOrEmpty(pdf) || pdf == "string")
            {
                return string.Empty;
            }
            byte[] imageData = Convert.FromBase64String(pdf);
            string directoryPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "ContentMaster");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fileName = Guid.NewGuid().ToString() + ".pdf";
            string filePath = Path.Combine(directoryPath, fileName);

            // Write the byte array to the image file
            File.WriteAllBytes(filePath, imageData);
            return filePath;
        }
        private string VideoUpload(string data)
        {
            if (string.IsNullOrEmpty(data) || data == "string")
            {
                return string.Empty;
            }
            byte[] bytes = Convert.FromBase64String(data);
            string directoryPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "ContentMasterVideo");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fileExtension = IsMp4(bytes) == true ? ".mp4" : IsMov(bytes) == true ? ".mov" : IsAvi(bytes) == true ? ".avi" : string.Empty;

            string fileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(directoryPath, fileName);

            // Write the byte array to the image file
            File.WriteAllBytes(filePath, bytes);
            return filePath;
        }
        private bool IsMp4(byte[] bytes)
        {
            // MP4 magic number: 0x00, 0x00, 0x00, 0x20, 0x66, 0x74, 0x79, 0x70
            return bytes.Length > 7 &&
                   bytes[0] == 0x00 && bytes[1] == 0x00 && bytes[2] == 0x00 && bytes[3] == 0x20 &&
                   bytes[4] == 0x66 && bytes[5] == 0x74 && bytes[6] == 0x79 && bytes[7] == 0x70;
        }
        private bool IsAvi(byte[] bytes)
        {
            // AVI magic number: "RIFF"
            return bytes.Length > 3 &&
                   bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46;
        }
        private bool IsMov(byte[] bytes)
        {
            // MOV magic number: "moov"
            return bytes.Length > 3 &&
                   bytes[0] == 0x6D && bytes[1] == 0x6F && bytes[2] == 0x6F && bytes[3] == 0x76;
        }
        private string GetVideo(string Filename)
        {
            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "ContentMasterVideo", Filename);

            if (!File.Exists(filePath))
            {
                return string.Empty;
            }
            byte[] fileBytes = File.ReadAllBytes(filePath);
            string base64String = Convert.ToBase64String(fileBytes);
            return base64String;
        }
        private string GetPDF(string Filename)
        {
            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "ContentMaster", Filename);

            if (!File.Exists(filePath))
            {
                return string.Empty;
            }
            byte[] fileBytes = File.ReadAllBytes(filePath);
            string base64String = Convert.ToBase64String(fileBytes);
            return base64String;
        }
    }
}
