using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;
using Course_API.Repository.Interfaces;
using Dapper;
using System.Data;
namespace Course_API.Repository.Implementations
{
    public class SyllabusRepository : ISyllabusRepository
    {
        private readonly IDbConnection _connection;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public SyllabusRepository(IDbConnection connection, IWebHostEnvironment hostingEnvironment)
        {
            _connection = connection;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<ServiceResponse<int>> AddUpdateSyllabus(SyllabusDTO request)
        {
            try
            {
                if (request.SyllabusId == 0)
                {
                    // Insert new syllabus
                    string insertQuery = @"
                INSERT INTO tblSyllabus (BoardID, CourseId, ClassId, SyllabusName, Status, createdby, createdon, APID, EmployeeID, ExamTypeId)
                VALUES (@BoardID, @CourseId, @ClassId, @SyllabusName, @Status, @createdby, @createdon, @APID, @EmployeeID, @ExamTypeId);
                SELECT CAST(SCOPE_IDENTITY() as int);";

                    var syllabusId = await _connection.ExecuteScalarAsync<int>(insertQuery, new
                    {
                        request.BoardID,
                        request.CourseId,
                        request.ClassId,
                        request.SyllabusName,
                        Status = true,
                        request.createdby,
                        createdon = DateTime.Now,
                        request.APID,
                        request.EmployeeID,
                        request.ExamTypeId
                    });
                    if (syllabusId != 0)
                    {
                        int rowsAffected = SyllabusSubjectMapping(request.SyllabusSubjects ?? [], syllabusId);
                        if (rowsAffected > 0)
                        {
                            return new ServiceResponse<int>(true, "Operation Successful", syllabusId, 200);
                        }
                        else
                        {
                            return new ServiceResponse<int>(false, "Operation Failed", 0, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<int>(false, "Operation Failed", 0, 500);
                    }
                }
                else
                {
                    // Update existing syllabus
                    string updateQuery = @"
                    UPDATE tblSyllabus
                    SET 
                        BoardID = @BoardID,
                        CourseId = @CourseId,
                        ClassId = @ClassId,
                        SyllabusName = @SyllabusName,
                        Status = @Status,
                        modifiedby = @modifiedby,
                        modifiedon = @modifiedon,
                        APID = @APID,
                        EmployeeID = @EmployeeID,
                        ExamTypeId = @ExamTypeId
                    WHERE SyllabusId = @SyllabusId;";

                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, new
                    {
                        request.BoardID,
                        request.CourseId,
                        request.ClassId,
                        request.SyllabusName,
                        request.Status,
                        request.modifiedby,
                        modifiedon = DateTime.Now,
                        request.APID,
                        request.EmployeeID,
                        request.ExamTypeId,
                        request.SyllabusId
                    });
                    if (rowsAffected > 0)
                    {
                        string selectQuery = "SELECT * FROM tblSyllabus WHERE SyllabusId = @SyllabusId";
                        var data = await _connection.QuerySingleOrDefaultAsync<dynamic>(selectQuery, new { request.SyllabusId });

                        if (data != null)
                        {
                            int affectedRows = SyllabusSubjectMapping(request.SyllabusSubjects ?? [], request.SyllabusId);
                            if (affectedRows > 0)
                            {
                                return new ServiceResponse<int>(true, "Operation Successful", request.SyllabusId, 200);
                            }
                            else
                            {
                                return new ServiceResponse<int>(false, "Operation Failed", 0, 500);
                            }
                        }
                        else
                        {
                            return new ServiceResponse<int>(false, "Operation Failed", 0, 204);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<int>(false, "Operation Failed", 0, 500);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<int>(false, ex.Message, 0, 500);
            }
        }
        public async Task<ServiceResponse<SyllabusResponseDTO>> GetSyllabusById(int syllabusId)
        {
            try
            {
                string selectQuery = @"
        SELECT 
            s.SyllabusId, s.BoardID, b.BoardName, s.CourseId, c.CourseName, s.ClassId, cl.ClassName, 
            s.SyllabusName, s.Status, s.createdby, s.createdon, s.modifiedby, s.modifiedon, 
            s.APID, a.APName, s.EmployeeID, e.EmpFirstName as EmpFirstName, s.ExamTypeId, et.ExamTypeName
        FROM 
            tblSyllabus s
        LEFT JOIN 
            tblBoard b ON s.BoardID = b.BoardId
        LEFT JOIN 
            tblClass cl ON s.ClassId = cl.ClassId
        LEFT JOIN 
            tblCourse c ON s.CourseId = c.CourseId
        LEFT JOIN 
            tblEmployee e ON s.EmployeeID = e.Employeeid
        LEFT JOIN 
            tblExamType et ON s.ExamTypeId = et.ExamTypeId
        LEFT JOIN 
            tblCategory a ON s.APID = a.APId
        WHERE 
            s.SyllabusId = @SyllabusId;

        SELECT 
            ss.SyllabusSubjectID, ss.SyllabusID, ss.SubjectID, sub.SubjectName, 
            ss.Status, ss.CreatedBy, ss.CreatedDate, ss.ModifiedBy, ss.ModifiedDate
        FROM 
            tblSyllabusSubjects ss
        LEFT JOIN 
            tblSubject sub ON ss.SubjectID = sub.SubjectId
        WHERE 
            ss.SyllabusID = @SyllabusId;";

                using (var multi = await _connection.QueryMultipleAsync(selectQuery, new { SyllabusId = syllabusId }))
                {
                    var syllabus = await multi.ReadSingleOrDefaultAsync<SyllabusResponseDTO>();

                    if (syllabus != null)
                    {
                        syllabus.SyllabusSubjects = (await multi.ReadAsync<SyllabusSubjectResponse>()).ToList();
                        return new ServiceResponse<SyllabusResponseDTO>(true, "Operation Successful", syllabus, 200);
                    }
                    else
                    {
                        return new ServiceResponse<SyllabusResponseDTO>(false, "Record not found", new SyllabusResponseDTO(), 404);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<SyllabusResponseDTO>(false, ex.Message, new SyllabusResponseDTO(), 500);
            }
        }
        public async Task<ServiceResponse<List<SyllabusResponseDTO>>> GetSyllabusList(GetAllSyllabusList request)
        {
            try
            {
                // Adjusted query to include more filters
                string selectQuery = @"
            SELECT 
                s.SyllabusId, s.BoardID, b.BoardName, s.CourseId, c.CourseName, 
                s.ClassId, cl.ClassName, s.SyllabusName, s.Status, 
                s.createdby, s.createdon, s.modifiedby, s.modifiedon, 
                s.APID, a.APName, s.EmployeeID, e.EmpFirstName as EmpFirstName, 
                s.ExamTypeId, et.ExamTypeName
            FROM 
                tblSyllabus s
            LEFT JOIN 
                tblBoard b ON s.BoardID = b.BoardId
            LEFT JOIN 
                tblClass cl ON s.ClassId = cl.ClassId
            LEFT JOIN 
                tblCourse c ON s.CourseId = c.CourseId
            LEFT JOIN 
                tblEmployee e ON s.EmployeeID = e.Employeeid
            LEFT JOIN 
                tblExamType et ON s.ExamTypeId = et.ExamTypeId
            LEFT JOIN 
                tblCategory a ON s.APID = a.APId
            WHERE 
                (@BoardId IS NULL OR s.BoardID = @BoardId) AND 
                (@CourseId IS NULL OR s.CourseId = @CourseId) AND
                (@ClassId IS NULL OR s.ClassId = @ClassId) AND
                (@APID IS NULL OR s.APID = @APID) AND
                (@ExamTypeId IS NULL OR s.ExamTypeId = @ExamTypeId)";

                var syllabusList = await _connection.QueryAsync<SyllabusResponseDTO>(selectQuery, new
                {
                    BoardId = request.BoardId > 0 ? (int?)request.BoardId : null,
                    CourseId = request.CourseId > 0 ? (int?)request.CourseId : null,
                    ClassId = request.ClassId > 0 ? (int?)request.ClassId : null,
                    APID = request.APID > 0 ? (int?)request.APID : null,
                    ExamTypeId = request.ExamTypeId > 0 ? (int?)request.ExamTypeId : null
                });

                return new ServiceResponse<List<SyllabusResponseDTO>>(true, "Operation Successful", syllabusList.ToList(), 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<SyllabusResponseDTO>>(false, ex.Message, new List<SyllabusResponseDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<string>> AddUpdateSyllabusDetails(SyllabusDetailsDTO request)
        {
            try
            {
                // Validate PDF format
                foreach (var detail in request.SyllabusDetails)
                {
                    if (string.IsNullOrEmpty(detail.Synopsis) || detail.Synopsis == "string")
                    {
                        detail.Synopsis = string.Empty;
                    }
                    else
                    {
                        if (!IsPdf(Convert.FromBase64String(detail.Synopsis)))
                        {
                            throw new Exception("The provided synopsis is not a valid PDF file.");
                        }
                    }
                    // If valid, you can optionally reassign the base64 string back
                    detail.Synopsis = PDFUpload(detail.Synopsis); // If you still need to process the PDF
                    detail.SyllabusID = request.SyllabusId;
                    detail.SubjectId = request.SubjectId;
                }

                string insertQuery = @"
        INSERT INTO tblSyllabusDetails (SyllabusID, ContentIndexId, IndexTypeId, Status, IsVerson, Synopsis, SubjectId)
        VALUES (@SyllabusID, @ContentIndexId, @IndexTypeId, @Status, @IsVerson, @Synopsis, @SubjectId)";

                string deleteQuery = "DELETE FROM tblSyllabusDetails WHERE SyllabusID = @SyllabusID";

                // Check if the syllabus details exist
                int count = await _connection.ExecuteScalarAsync<int>(
                    "SELECT COUNT(*) FROM tblSyllabusDetails WHERE SyllabusID = @SyllabusID",
                    new { SyllabusID = request.SyllabusId }
                );

                if (count > 0)
                {
                    // Delete existing details
                    int rowsAffected = await _connection.ExecuteAsync(deleteQuery, new { SyllabusID = request.SyllabusId });
                    if (rowsAffected <= 0)
                    {
                        return new ServiceResponse<string>(false, "Operation Failed to delete existing details", string.Empty, 500);
                    }
                }

                // Insert new or updated details
                int addedRecords = await _connection.ExecuteAsync(insertQuery, request.SyllabusDetails);

                if (addedRecords > 0)
                {
                    return new ServiceResponse<string>(true, "Operation Successful", "Syllabus Details Added Successfully", 200);
                }
                else
                {
                    return new ServiceResponse<string>(false, "Operation Failed to add new details", string.Empty, 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<SyllabusDetailsResponseDTO>> GetSyllabusDetailsById(int syllabusId, int subjectId)
        {
            try
            {
                string selectQuery = @"
            SELECT 
                sd.SyllabusDetailID, 
                sd.SyllabusID, 
                sd.ContentIndexId, 
                sd.IndexTypeId, 
                sd.Status, 
                sd.IsVerson,
                sd.Synopsis,
                cic.ContentName_Chapter,
                cit.ContentName_Topic,
                cist.ContentName_SubTopic,
                cic.ChapterCode AS ChapterCode,
                cit.TopicCode AS TopicCode,
                cic.CreatedOn AS ChapterCreatedOn,
                cic.CreatedBy AS ChapterCreatedBy,
                cic.ModifiedOn AS ChapterModifiedOn,
                cic.ModifiedBy AS ChapterModifiedBy,
                cit.CreatedOn AS TopicCreatedOn,
                cit.CreatedBy AS TopicCreatedBy,
                cit.ModifiedOn AS TopicModifiedOn,
                cit.ModifiedBy AS TopicModifiedBy,
                cist.CreatedOn AS SubTopicCreatedOn,
                cist.CreatedBy AS SubTopicCreatedBy,
                cist.ModifiedOn AS SubTopicModifiedOn,
                cist.ModifiedBy AS SubTopicModifiedBy
            FROM 
                tblSyllabusDetails sd
            LEFT JOIN 
                tblContentIndexChapters cic ON sd.ContentIndexId = cic.ContentIndexId AND sd.IndexTypeId = 1
            LEFT JOIN 
                tblContentIndexTopics cit ON sd.ContentIndexId = cit.ContInIdTopic AND sd.IndexTypeId = 2
            LEFT JOIN 
                tblContentIndexSubTopics cist ON sd.ContentIndexId = cist.ContInIdSubTopic AND sd.IndexTypeId = 3
            WHERE 
                sd.SyllabusID = @SyllabusId";

                var syllabusDetails = await _connection.QueryAsync<dynamic>(selectQuery, new { SyllabusId = syllabusId });

                var syllabusDetailsResponseDTO = new SyllabusDetailsResponseDTO
                {
                    SyllabusId = syllabusId,
                    SubjectId = subjectId,
                    SyllabusDetails = new List<SyllabusDetailsResponse>()
                };

                // List to store the chapters, topics, and subtopics
                var contentIndexResponses = new List<ContentIndexResponse>();

                if (syllabusDetails != null && syllabusDetails.Any())
                {
                    // Iterate through the results and populate the list
                    foreach (var detail in syllabusDetails)
                    {
                        // Check for Chapters
                        if (detail.IndexTypeId == 1) // Chapter
                        {
                            var chapterResponse = new ContentIndexResponse
                            {
                                ContentIndexId = detail.ContentIndexId,
                                SubjectId = subjectId,
                                ContentName_Chapter = detail.ContentName_Chapter,
                                Status = detail.Status,
                                IndexTypeId = detail.IndexTypeId,
                                CreatedOn = detail.ChapterCreatedOn,
                                CreatedBy = detail.ChapterCreatedBy,
                                ModifiedOn = detail.ChapterModifiedOn,
                                ModifiedBy = detail.ChapterModifiedBy,
                                ChapterCode = detail.ChapterCode,
                                ContentIndexTopics = new List<ContentIndexTopics>()
                            };

                            // Add chapter to the list
                            contentIndexResponses.Add(chapterResponse);
                        }
                        // Check for Topics
                        else if (detail.IndexTypeId == 2) // Topic
                        {
                            // Find the corresponding chapter
                            var chapterResponse = contentIndexResponses
                                .FirstOrDefault(c => c.ContentIndexId == detail.ContentIndexId);

                            if (chapterResponse == null)
                            {
                                // Create a new chapter response with null values for topic
                                chapterResponse = new ContentIndexResponse
                                {
                                    ContentIndexId = detail.ContentIndexId,
                                    SubjectId = subjectId,
                                    ContentName_Chapter = null, // No chapter
                                    Status = detail.Status,
                                    IndexTypeId = 1, // Set as chapter
                                    CreatedOn = null,
                                    CreatedBy = null,
                                    ModifiedOn = null,
                                    ModifiedBy = null,
                                    ChapterCode = null,
                                    ContentIndexTopics = new List<ContentIndexTopics>()
                                };

                                // Add the chapter response to the list
                                contentIndexResponses.Add(chapterResponse);
                            }

                            var topicResponse = new ContentIndexTopics
                            {
                                ContInIdTopic = detail.ContInIdTopic,
                                ContentIndexId = detail.ContentIndexId,
                                ContentName_Topic = detail.ContentName_Topic,
                                Status = detail.Status,
                                IndexTypeId = detail.IndexTypeId,
                                CreatedOn = detail.TopicCreatedOn,
                                CreatedBy = detail.TopicCreatedBy,
                                ModifiedOn = detail.TopicModifiedOn,
                                ModifiedBy = detail.TopicModifiedBy,
                                ChapterCode = detail.ChapterCode,
                                TopicCode = detail.TopicCode,
                                ContentIndexSubTopics = new List<ContentIndexSubTopic>()
                            };

                            // Add topic to the corresponding chapter
                            chapterResponse.ContentIndexTopics.Add(topicResponse);
                        }
                        // Check for Subtopics
                        else if (detail.IndexTypeId == 3) // Subtopic
                        {
                            var topicResponse = contentIndexResponses
                                .SelectMany(c => c.ContentIndexTopics)
                                .FirstOrDefault(t => t.ContInIdTopic == detail.ContInIdTopic);

                            if (topicResponse == null)
                            {
                                // Create a new topic response with null values for subtopic
                                topicResponse = new ContentIndexTopics
                                {
                                    ContInIdTopic = detail.ContInIdTopic,
                                    ContentIndexId = detail.ContentIndexId,
                                    ContentName_Topic = null, // No topic name
                                    Status = detail.Status,
                                    IndexTypeId = 2, // Set as topic
                                    ChapterCode = detail.ChapterCode,
                                    TopicCode = detail.TopicCode,
                                    ContentIndexSubTopics = new List<ContentIndexSubTopic>()
                                };

                                // Add the topic to the chapter (if found) or to the list
                                var chapterResponse = contentIndexResponses
                                    .FirstOrDefault(c => c.ContentIndexId == detail.ContentIndexId);

                                if (chapterResponse != null)
                                {
                                    chapterResponse.ContentIndexTopics.Add(topicResponse);
                                }
                                else
                                {
                                    contentIndexResponses.Add(new ContentIndexResponse
                                    {
                                        ContentIndexId = detail.ContentIndexId,
                                        SubjectId = subjectId,
                                        ContentName_Chapter = null, // No chapter
                                        Status = detail.Status,
                                        IndexTypeId = 1, // Set as chapter
                                        CreatedOn = null,
                                        CreatedBy = null,
                                        ModifiedOn = null,
                                        ModifiedBy = null,
                                        ChapterCode = null,
                                        ContentIndexTopics = new List<ContentIndexTopics> { topicResponse }
                                    });
                                }
                            }

                            if (topicResponse != null)
                            {
                                var subTopicResponse = new ContentIndexSubTopic
                                {
                                    ContInIdSubTopic = detail.ContInIdSubTopic,
                                    ContInIdTopic = detail.ContInIdTopic,
                                    ContentName_SubTopic = detail.ContentName_SubTopic,
                                    Status = detail.Status,
                                    IndexTypeId = detail.IndexTypeId,
                                    CreatedOn = detail.SubTopicCreatedOn,
                                    CreatedBy = detail.SubTopicCreatedBy,
                                    ModifiedOn = detail.SubTopicModifiedOn,
                                    ModifiedBy = detail.SubTopicModifiedBy,
                                    TopicCode = detail.TopicCode,
                                    SubTopicCode = detail.SubTopicCode
                                };

                                // Add subtopic to the corresponding topic
                                topicResponse.ContentIndexSubTopics.Add(subTopicResponse);
                            }
                        }
                    }

                    // Add the content index responses to syllabus details
                    syllabusDetailsResponseDTO.SyllabusDetails.Add(new SyllabusDetailsResponse
                    {
                        SyllabusID = syllabusId,
                        ContentIndexResponses = contentIndexResponses
                    });

                    return new ServiceResponse<SyllabusDetailsResponseDTO>(true, "Operation Successful", syllabusDetailsResponseDTO, 200);
                }
                else
                {
                    return new ServiceResponse<SyllabusDetailsResponseDTO>(false, "No records found for the provided syllabus ID.", syllabusDetailsResponseDTO, 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<SyllabusDetailsResponseDTO>(false, ex.Message, new SyllabusDetailsResponseDTO(), 500);
            }
        }
        public async Task<ServiceResponse<string>> UpdateContentIndexName(UpdateContentIndexNameDTO request)
        {
            try
            {
                string updateQuery = string.Empty;

                // Determine which table to update based on the IndexTypeId
                if (request.IndexTypeId == 1)
                {
                    updateQuery = @"
            UPDATE tblContentIndexChapters
            SET DisplayName = @NewContentIndexName
            WHERE ChapterCode = @ContentCode AND IsActive = 1";
                }
                else if (request.IndexTypeId == 2)
                {
                    updateQuery = @"
            UPDATE tblContentIndexTopics
            SET DisplayName = @NewContentIndexName
            WHERE TopicCode = @ContentCode AND IsActive = 1";
                }
                else if (request.IndexTypeId == 3)
                {
                    updateQuery = @"
            UPDATE tblContentIndexSubTopics
            SET DisplayName = @NewContentIndexName
            WHERE SubTopicCode = @ContentCode AND IsActive = 1";
                }
                else
                {
                    return new ServiceResponse<string>(false, "Invalid IndexTypeId", string.Empty, 400);
                }

                int rowsAffected = await _connection.ExecuteAsync(updateQuery, new { request.NewContentIndexName, request.ContentCode });

                if (rowsAffected > 0)
                {
                    return new ServiceResponse<string>(true, "Operation Successful", "Content Index Name Updated Successfully", 200);
                }
                else
                {
                    return new ServiceResponse<string>(false, "Operation Failed", "No records were updated", 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        private int SyllabusSubjectMapping(List<SyllabusSubject> request, int SyllabusId)
        {
            foreach (var data in request)
            {
                data.SyllabusID = SyllabusId;
            }
            string query = "SELECT COUNT(*) FROM [tblSyllabusSubjects] WHERE [SyllabusID] = @SyllabusID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { SyllabusID = SyllabusId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblSyllabusSubjects] WHERE [SyllabusID] = @SyllabusID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { SyllabusID = SyllabusId });
                if (rowsAffected > 0)
                {
                    string insertQuery = @"
                INSERT INTO tblSyllabusSubjects (SyllabusID, SubjectID, Status, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@SyllabusID, @SubjectID, @Status, @CreatedBy, @CreatedDate, @ModifiedBy, @ModifiedDate);";
                    var valuesInserted = _connection.Execute(insertQuery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                string insertQuery = @"
                INSERT INTO tblSyllabusSubjects (SyllabusID, SubjectID, Status, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate)
                VALUES (@SyllabusID, @SubjectID, @Status, @CreatedBy, @CreatedDate, @ModifiedBy, @ModifiedDate);";
                var valuesInserted = _connection.Execute(insertQuery, request);
                return valuesInserted;
            }
        }
        private string PDFUpload(string pdf)
        {
            if (string.IsNullOrEmpty(pdf) || pdf == "string")
            {
                return string.Empty;
            }
            byte[] imageData = Convert.FromBase64String(pdf);
            string directoryPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "Syllabus");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fileExtension = IsPdf(imageData) == true ? ".pdf" : string.Empty;
            if (string.IsNullOrEmpty(fileExtension))
            {
                throw new InvalidOperationException("Incorrect file uploaded");
            }
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
        private bool IsPdf(byte[] fileData)
        {
            return fileData.Length > 4 &&
                   fileData[0] == 0x25 && fileData[1] == 0x50 && fileData[2] == 0x44 && fileData[3] == 0x46;
        }
        private string GetPDF(string Filename)
        {
            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "Syllabus", Filename);

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