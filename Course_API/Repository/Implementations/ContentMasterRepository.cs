using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
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
                    var insertQuery = @"
                INSERT INTO tblContentMaster (
                    boardId, classId, courseId, subjectId, fileName, PathURL, createdon, createdby, IndexTypeId, ExamTypeId, APId, EmployeeId, ContentIndexId)
                VALUES (
                    @boardId, @classId, @courseId, @subjectId, @fileName, @PathURL, @createdon, @createdby, @IndexTypeId, @ExamTypeId, @APId, @EmployeeId, @ContentIndexId);";

                    var content = new ContentMaster
                    {
                        boardId = request.boardId,
                        classId = request.classId,
                        courseId = request.courseId,
                        subjectId = request.subjectId,
                        fileName = PDFUpload(request.fileName),
                        PathURL = VideoUpload(request.PathURL),
                        createdon = DateTime.Now,
                        createdby = request.createdby,
                        IndexTypeId = request.IndexTypeId,
                        ExamTypeId = request.ExamTypeId,
                        APId = request.APId,
                        EmployeeId = request.EmployeeId,
                        ContentIndexId = request.ContentIndexId
                    };

                    int rowsAffected = await _connection.ExecuteAsync(insertQuery, content);
                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Content Added Successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                    }
                }
                else
                {
                    var updateQuery = @"
                UPDATE tblContentMaster
                SET boardId = @boardId,
                    classId = @classId,
                    courseId = @courseId,
                    subjectId = @subjectId,
                    fileName = @fileName,
                    PathURL = @PathURL,
                    modifiedon = @modifiedon,
                    modifiedby = @modifiedby,
                    IndexTypeId = @IndexTypeId,
                    ExamTypeId = @ExamTypeId,
                    APId = @APId,
                    EmployeeId = @EmployeeId,
                    ContentIndexId = @ContentIndexId
                WHERE contentid = @contentid;";

                    var content = new ContentMaster
                    {
                        contentid = request.contentid,
                        boardId = request.boardId,
                        classId = request.classId,
                        courseId = request.courseId,
                        subjectId = request.subjectId,
                        fileName = PDFUpload(request.fileName),
                        PathURL = VideoUpload(request.PathURL),
                        modifiedon = DateTime.Now,
                        modifiedby = request.modifiedby,
                        IndexTypeId = request.IndexTypeId,
                        ExamTypeId = request.ExamTypeId,
                        APId = request.APId,
                        EmployeeId = request.EmployeeId,
                        ContentIndexId = request.ContentIndexId
                    };

                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, content);
                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Content Updated Successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<ContentMasterResponseDTO>> GetContentById(int ContentId)
        {
            try
            {
                string selectQuery = @"
            SELECT cm.contentid, cm.boardId, b.BoardName, cm.classId, cl.ClassName, cm.courseId, c.CourseName, cm.subjectId, s.SubjectName, cm.fileName, cm.PathURL, cm.createdon, cm.createdby, cm.modifiedon, cm.modifiedby,
                   cm.IndexTypeId, it.IndexType as IndexTypeName, cm.ExamTypeId, et.ExamTypeName, cm.APId, a.APName, cm.EmployeeId, e.EmpFirstName as EmployeeName , cm.ContentIndexId,
                   CASE 
                       WHEN cm.IndexTypeId = 1 THEN ci.ContentName_Chapter
                       WHEN cm.IndexTypeId = 2 THEN ct.ContentName_Topic
                       WHEN cm.IndexTypeId = 3 THEN cst.ContentName_SubTopic
                   END AS ContentIndexName
            FROM tblContentMaster cm
            LEFT JOIN tblBoard b ON cm.boardId = b.BoardId
            LEFT JOIN tblClass cl ON cm.classId = cl.ClassId
            LEFT JOIN tblCourse c ON cm.courseId = c.CourseId
            LEFT JOIN tblSubject s ON cm.subjectId = s.SubjectId
            LEFT JOIN tblExamType et ON cm.ExamTypeId = et.ExamTypeId
            LEFT JOIN tblCategory a ON cm.APId = a.APId
            LEFT JOIN tblEmployee e ON cm.EmployeeId = e.EmployeeId
            LEFT JOIN tblQBIndexType it ON cm.IndexTypeId = it.IndexId
            LEFT JOIN tblContentIndexChapters ci ON cm.ContentIndexId = ci.ContentIndexId AND cm.IndexTypeId = 1
            LEFT JOIN tblContentIndexTopics ct ON cm.ContentIndexId = ct.ContInIdTopic AND cm.IndexTypeId = 2
            LEFT JOIN tblContentIndexSubTopics cst ON cm.ContentIndexId = cst.ContInIdSubTopic AND cm.IndexTypeId = 3
            WHERE cm.contentid = @ContentId";

                var data = await _connection.QuerySingleOrDefaultAsync<ContentMasterResponseDTO>(selectQuery, new { ContentId });
                if (data != null)
                {
                    data.PathURL = GetVideo(data.PathURL);
                    data.fileName = GetPDF(data.fileName);
                    return new ServiceResponse<ContentMasterResponseDTO>(true, "Operation Successful", data, 200);
                }
                else
                {
                    return new ServiceResponse<ContentMasterResponseDTO>(false, "Operation Failed", new ContentMasterResponseDTO(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ContentMasterResponseDTO>(false, ex.Message, new ContentMasterResponseDTO(), 500);
            }
        }
        public async Task<ServiceResponse<List<ContentMasterResponseDTO>>> GetContentList(GetAllContentListRequest request)
        {
            try
            {
                // Base query
                string baseQuery = @"
        SELECT cm.contentid, cm.boardId, b.BoardName, cm.classId, cl.ClassName, cm.courseId, c.CourseName, cm.subjectId, s.SubjectName, cm.fileName, cm.PathURL, cm.createdon, cm.createdby, cm.modifiedon, cm.modifiedby,
               cm.IndexTypeId, it.IndexType as IndexTypeName, cm.ExamTypeId, et.ExamTypeName, cm.APId, a.APName, cm.EmployeeId, e.EmpFirstName as EmployeeName , cm.ContentIndexId,
               CASE 
                   WHEN cm.IndexTypeId = 1 THEN ci.ContentName_Chapter
                   WHEN cm.IndexTypeId = 2 THEN ct.ContentName_Topic
                   WHEN cm.IndexTypeId = 3 THEN cst.ContentName_SubTopic
               END AS ContentIndexName
        FROM tblContentMaster cm
        LEFT JOIN tblBoard b ON cm.boardId = b.BoardId
        LEFT JOIN tblClass cl ON cm.classId = cl.ClassId
        LEFT JOIN tblCourse c ON cm.courseId = c.CourseId
        LEFT JOIN tblSubject s ON cm.subjectId = s.SubjectId
        LEFT JOIN tblExamType et ON cm.ExamTypeId = et.ExamTypeId
        LEFT JOIN tblCategory a ON cm.APId = a.APId
        LEFT JOIN tblEmployee e ON cm.EmployeeId = e.EmployeeId
        LEFT JOIN tblQBIndexType it ON cm.IndexTypeId = it.IndexId
        LEFT JOIN tblContentIndexChapters ci ON cm.ContentIndexId = ci.ContentIndexId AND cm.IndexTypeId = 1
        LEFT JOIN tblContentIndexTopics ct ON cm.ContentIndexId = ct.ContInIdTopic AND cm.IndexTypeId = 2
        LEFT JOIN tblContentIndexSubTopics cst ON cm.ContentIndexId = cst.ContInIdSubTopic AND cm.IndexTypeId = 3
        WHERE 1=1";

                // Applying filters
                if (request.ClassID > 0)
                {
                    baseQuery += " AND cm.classId = @ClassID";
                }
                if (request.BoardID > 0)
                {
                    baseQuery += " AND cm.boardId = @BoardID";
                }
                if (request.CourseID > 0)
                {
                    baseQuery += " AND cm.courseId = @CourseID";
                }
                if (request.ExamTypeID > 0)
                {
                    baseQuery += " AND cm.ExamTypeId = @ExamTypeID";
                }
                if (request.APId > 0)
                {
                    baseQuery += " AND cm.APId = @APId";
                }
                if (request.SubjectID > 0)
                {
                    baseQuery += " AND cm.subjectId = @SubjectID";
                }

                // Execute the query without pagination
                var data = (await _connection.QueryAsync<ContentMasterResponseDTO>(baseQuery, new
                {
                    ClassID = request.ClassID,
                    BoardID = request.BoardID,
                    CourseID = request.CourseID,
                    ExamTypeID = request.ExamTypeID,
                    APId = request.APId,
                    SubjectID = request.SubjectID
                })).ToList();

                // Update fileName and PathURL
                if (data != null && data.Any())
                {
                    foreach (var item in data)
                    {
                        item.fileName = GetPDF(item.fileName);
                        item.PathURL = GetVideo(item.PathURL);
                    }

                    // Apply pagination logic after fetching the records
                    int totalCount = data.Count;
                    var paginatedData = data.Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToList();

                    return new ServiceResponse<List<ContentMasterResponseDTO>>(true, "Operation Successful", paginatedData, 200, totalCount);
                }
                else
                {
                    return new ServiceResponse<List<ContentMasterResponseDTO>>(false, "No Records Found", new List<ContentMasterResponseDTO>(), 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ContentMasterResponseDTO>>(false, ex.Message, new List<ContentMasterResponseDTO>(), 500);
            }
        }

        //public async Task<ServiceResponse<List<ContentMasterResponseDTO>>> GetContentList(GetAllContentListRequest request)
        //{
        //    try
        //    {
        //        string countSql = @"SELECT COUNT(*) FROM [tblContentMaster]";
        //        int totalCount = await _connection.ExecuteScalarAsync<int>(countSql);

        //        string selectQuery = @"
        //        SELECT cm.contentid, cm.boardId, b.BoardName, cm.classId, cl.ClassName, cm.courseId, c.CourseName, cm.subjectId, s.SubjectName, cm.fileName, cm.PathURL, cm.createdon, cm.createdby, cm.modifiedon, cm.modifiedby,
        //               cm.IndexTypeId, it.IndexType as IndexTypeName, cm.ExamTypeId, et.ExamTypeName, cm.APId, a.APName, cm.EmployeeId, e.EmpFirstName as EmployeeName , cm.ContentIndexId,
        //               CASE 
        //                   WHEN cm.IndexTypeId = 1 THEN ci.ContentName_Chapter
        //                   WHEN cm.IndexTypeId = 2 THEN ct.ContentName_Topic
        //                   WHEN cm.IndexTypeId = 3 THEN cst.ContentName_SubTopic
        //               END AS ContentIndexName
        //        FROM tblContentMaster cm
        //        LEFT JOIN tblBoard b ON cm.boardId = b.BoardId
        //        LEFT JOIN tblClass cl ON cm.classId = cl.ClassId
        //        LEFT JOIN tblCourse c ON cm.courseId = c.CourseId
        //        LEFT JOIN tblSubject s ON cm.subjectId = s.SubjectId
        //        LEFT JOIN tblExamType et ON cm.ExamTypeId = et.ExamTypeId
        //        LEFT JOIN tblCategory a ON cm.APId = a.APId
        //        LEFT JOIN tblEmployee e ON cm.EmployeeId = e.EmployeeId
        //        LEFT JOIN tblQBIndexType it ON cm.IndexTypeId = it.IndexId
        //        LEFT JOIN tblContentIndexChapters ci ON cm.ContentIndexId = ci.ContentIndexId AND cm.IndexTypeId = 1
        //        LEFT JOIN tblContentIndexTopics ct ON cm.ContentIndexId = ct.ContInIdTopic AND cm.IndexTypeId = 2
        //        LEFT JOIN tblContentIndexSubTopics cst ON cm.ContentIndexId = cst.ContInIdSubTopic AND cm.IndexTypeId = 3
        //        ORDER BY cm.contentid
        //        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        //        int offset = (request.PageNumber - 1) * request.PageSize;

        //        var data = await _connection.QueryAsync<ContentMasterResponseDTO>(selectQuery, new { Offset = offset, request.PageSize });

        //        if (data != null && data.Any())
        //        {
        //            foreach (var item in data)
        //            {
        //                item.fileName = GetPDF(item.fileName);
        //                item.PathURL = GetVideo(item.PathURL);
        //            }

        //            return new ServiceResponse<List<ContentMasterResponseDTO>>(true, "Operation Successful", data.ToList(), 200, totalCount);
        //        }
        //        else
        //        {
        //            return new ServiceResponse<List<ContentMasterResponseDTO>>(false, "No Records Found", [], 204);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<List<ContentMasterResponseDTO>>(false, ex.Message, [], 500);
        //    }
        //}
        public async Task<ServiceResponse<List<ContentIndexResponse>>> GetAllContentIndexList(ContentIndexRequestDTO request)
        {
            try
            {
                // Base query to fetch the syllabus based on filters
                string syllabusQuery = @"
            SELECT * 
            FROM [tblSyllabus] 
            WHERE [ClassId] = @ClassId AND [CourseId] = @CourseId AND [BoardID] = @BoardId";

                // Fetch the syllabus based on the given filters
                var syllabus = await _connection.QueryAsync<Syllabus>(syllabusQuery, new
                {
                    ClassId = request.classid,
                    CourseId = request.courseid,
                    BoardId = request.boardid
                });

                if (!syllabus.Any())
                {
                    return new ServiceResponse<List<ContentIndexResponse>>(false, "No syllabus found", new List<ContentIndexResponse>(), StatusCodes.Status204NoContent);
                }

                // Prepare a list to hold content index responses
                var contentIndexResponses = new List<ContentIndexResponse>();

                foreach (var item in syllabus)
                {
                    // Fetch syllabus details for each syllabus ID
                    string syllabusDetailQuery = @"
                SELECT * 
                FROM [tblSyllabusDetails] 
                WHERE [SyllabusID] = @SyllabusID AND [SubjectId] = @SubjectId";

                    var syllabusDetails = await _connection.QueryAsync<SyllabusDetail>(syllabusDetailQuery, new
                    {
                        SyllabusID = item.SyllabusId,
                        SubjectId = request.SubjectId
                    });

                    foreach (var detail in syllabusDetails)
                    {
                        // Fetch the content index record based on ContentIndexId
                        string contentIndexQuery = @"
                    SELECT * 
                    FROM [tblContentIndexChapters] 
                    WHERE [ContentIndexId] = @ContentIndexId";

                        var chapterRecord = await _connection.QuerySingleOrDefaultAsync<ContentIndexResponse>(contentIndexQuery, new
                        {
                            ContentIndexId = detail.ContentIndexId
                        });

                        if (chapterRecord != null)
                        {
                            // Map the common fields from the chapter record
                            var contentIndexResponse = new ContentIndexResponse
                            {
                                ContentIndexId = chapterRecord.ContentIndexId,
                                SubjectId = detail.SubjectId,
                                APID = item.APID,
                                CreatedOn = chapterRecord.CreatedOn,
                                CreatedBy = chapterRecord.CreatedBy,
                                ModifiedOn = chapterRecord.ModifiedOn,
                                ModifiedBy = chapterRecord.ModifiedBy,
                                EmployeeId = chapterRecord.EmployeeId,
                                Status = chapterRecord.Status,
                                ChapterCode = chapterRecord.ChapterCode,
                                ContentName_Chapter = chapterRecord.ContentName_Chapter,
                                IndexTypeId = chapterRecord.IndexTypeId // Set index type as chapter
                            };

                            // Fetch topics for the current chapter
                            string topicsQuery = @"
                        SELECT * 
                        FROM [tblContentIndexTopics] 
                        WHERE [ContentIndexId] = @ContentIndexId";

                            var topics = await _connection.QueryAsync<ContentIndexTopics>(topicsQuery, new
                            {
                                ContentIndexId = chapterRecord.ContentIndexId
                            });

                            // Map topics and their subtopics
                            contentIndexResponse.ContentIndexTopics = new List<ContentIndexTopics>();
                            foreach (var topic in topics)
                            {
                                // Fetch subtopics for the current topic
                                string subTopicsQuery = "SELECT * FROM [tblContentIndexSubTopics] WHERE [ContInIdTopic] = @ContInIdTopic";
                                var subTopics = await _connection.QueryAsync<ContentIndexSubTopic>(subTopicsQuery, new
                                {
                                    ContInIdTopic = topic.ContInIdTopic
                                });

                                // Map the topic properties including subtopics
                                topic.ContentIndexSubTopics = subTopics.ToList();
                                contentIndexResponse.ContentIndexTopics.Add(topic);
                            }

                            contentIndexResponses.Add(contentIndexResponse);
                        }
                    }
                }

                if (contentIndexResponses.Any())
                {
                    return new ServiceResponse<List<ContentIndexResponse>>(true, "Records found", contentIndexResponses, StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<List<ContentIndexResponse>>(false, "Records not found", new List<ContentIndexResponse>(), StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ContentIndexResponse>>(false, ex.Message, new List<ContentIndexResponse>(), StatusCodes.Status500InternalServerError);
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
            string fileExtension = IsPdf(imageData) == true ? ".pdf" : string.Empty;
            string fileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(directoryPath, fileName);
            if (string.IsNullOrEmpty(fileExtension))
            {
                throw new InvalidOperationException("Incorrect file uploaded");
            }
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
            if (string.IsNullOrEmpty(fileExtension))
            {
                throw new InvalidOperationException("Incorrect file uploaded");
            }
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
        private bool IsPdf(byte[] fileData)
        {
            return fileData.Length > 4 &&
                   fileData[0] == 0x25 && fileData[1] == 0x50 && fileData[2] == 0x44 && fileData[3] == 0x46;
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
