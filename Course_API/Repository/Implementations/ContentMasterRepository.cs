using Course_API.DTOs;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;
using Course_API.Repository.Interfaces;
using Dapper;
using System.Collections.Generic;
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
        public async Task<ServiceResponse<string>> AddUpdateContent(ContentMasterDTO request)
        {
            try
            {
                if (request.Content_Id == 0)
                {
                    string insertQuery = @"
            INSERT INTO tblContentMaster (SubjectIndexId, Board_Id, Class_Id, Course_Id, Subject_Id, NameOfFile, PathUrl)
            VALUES (@SubjectIndexId, @Board_Id, @Class_Id, @Course_Id, @Subject_Id, @NameOfFile, @PathUrl);";
                    var content = new ContentMaster
                    {
                        Board_Id = request.Board_Id,
                        Class_Id = request.Class_Id,
                        Course_Id = request.Course_Id,
                        NameOfFile = await GetPath(request.NameOfFile),
                        PathUrl = await GetPath(request.PathUrl),
                        SubjectIndexId = request.SubjectIndexId,
                        Subject_Id = request.Subject_Id
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
                    string updateQuery = @"
            UPDATE tblContentMaster 
            SET SubjectIndexId = @SubjectIndexId, 
                Board_Id = @Board_Id, 
                Class_Id = @Class_Id, 
                Course_Id = @Course_Id, 
                Subject_Id = @Subject_Id, 
                NameOfFile = @NameOfFile, 
                PathUrl = @PathUrl 
            WHERE Content_Id = @Content_Id";
                    var content = new ContentMaster
                    {
                        Board_Id = request.Board_Id,
                        Class_Id = request.Class_Id,
                        Course_Id = request.Course_Id,
                        NameOfFile = await GetPath(request.NameOfFile),
                        PathUrl = await GetPath(request.PathUrl),
                        SubjectIndexId = request.SubjectIndexId,
                        Subject_Id = request.Subject_Id,
                        Content_Id = request.Content_Id
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
        public async Task<ServiceResponse<byte[]>> GetContentFileById(int ContentId)
        {
            try
            {
                var data = await _connection.QueryFirstOrDefaultAsync<ContentMaster>(
                    "SELECT * FROM tblContentMaster WHERE Content_Id = @Content_Id",
                    new { Content_Id = ContentId });

                if (data == null)
                {
                    throw new Exception("Data not found");
                }

                var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "ContentMaster", data.NameOfFile);

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
        public async Task<ServiceResponse<byte[]>> GetContentFilePathUrlById(int ContentId)
        {
            try
            {
                var data = await _connection.QueryFirstOrDefaultAsync<ContentMaster>(
                    "SELECT * FROM tblContentMaster WHERE Content_Id = @Content_Id",
                    new { Content_Id = ContentId });

                if (data == null)
                {
                    throw new Exception("Data not found");
                }

                var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "ContentMaster", data.PathUrl);

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
        public async Task<ServiceResponse<ContentMaster>> GetContentById(int ContentId)
        {
            try
            {
                string selectQuery = @"
            SELECT Content_Id, SubjectIndexId, Board_Id, Class_Id, Course_Id, Subject_Id
            FROM tblContentMaster
            WHERE Content_Id = @ContentId";
                var data = await _connection.QuerySingleOrDefaultAsync<ContentMaster>(selectQuery, new { ContentId });
                if (data != null)
                {
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
            SELECT Content_Id, SubjectIndexId, Board_Id, Class_Id, Course_Id, Subject_Id
            FROM tblContentMaster";
                var data = await _connection.QueryAsync<ContentMaster>(selectQuery);
                if (data != null)
                {
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
                List<SubjectContentIndexDTO> resposne = new List<SubjectContentIndexDTO>();
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
        private async Task<string> GetPath(IFormFile? file)
        {
            if (file != null)
            {
                var uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "ContentMaster");
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploads, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await file.CopyToAsync(fileStream);
                }
                string imageUrl = fileName;
                return imageUrl;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
