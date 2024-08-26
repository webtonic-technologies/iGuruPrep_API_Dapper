using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;
using Course_API.Repository.Interfaces;
using Dapper;
using OfficeOpenXml;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
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
                // Validate PDF format and set default values
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
                    detail.Synopsis = PDFUpload(detail.Synopsis);
                    detail.SyllabusID = request.SyllabusId;
                    detail.SubjectId = request.SubjectId;
                }

                // List to store new syllabus details for child topics and subtopics
                var additionalDetails = new List<SyllabusDetails>();

                // If IndexTypeId is 1 (Chapter), fetch and add child topics and subtopics
                foreach (var detail in request.SyllabusDetails.ToList()) // Convert to a list to avoid modification issues
                {
                    if (detail.IndexTypeId == 1) // Chapter
                    {
                        // Fetch child topics for this chapter
                        var topicsQuery = @"
                SELECT ContInIdTopic AS ContentIndexId, 2 AS IndexTypeId, ContentName_Topic AS ContentName, DisplayOrder 
                FROM tblContentIndexTopics 
                WHERE ContentIndexId = @ContentIndexId AND IsActive = 1";

                        var topics = await _connection.QueryAsync<SyllabusDetails>(topicsQuery, new { ContentIndexId = detail.ContentIndexId });

                        foreach (var topic in topics)
                        {
                            topic.SyllabusID = request.SyllabusId;
                            topic.SubjectId = request.SubjectId;
                            topic.Status = 1; // Or any status you want to assign
                            topic.Synopsis = string.Empty; // No synopsis for topic-level by default
                            topic.IsVerson = detail.IsVerson;
                            additionalDetails.Add(topic);

                            // Fetch child subtopics for each topic
                            var subtopicsQuery = @"
                    SELECT ContInIdSubTopic AS ContentIndexId, 3 AS IndexTypeId, ContentName_SubTopic AS ContentName, DisplayOrder 
                    FROM tblContentIndexSubTopics 
                    WHERE ContInIdTopic = @ContInIdTopic AND IsActive = 1";

                            var subtopics = await _connection.QueryAsync<SyllabusDetails>(subtopicsQuery, new { ContInIdTopic = topic.ContentIndexId });

                            foreach (var subtopic in subtopics)
                            {
                                subtopic.SyllabusID = request.SyllabusId;
                                subtopic.SubjectId = request.SubjectId;
                                subtopic.Status = 1; // Or any status you want to assign
                                subtopic.Synopsis = string.Empty; // No synopsis for subtopic-level by default
                                subtopic.IsVerson = detail.IsVerson;
                                additionalDetails.Add(subtopic);
                            }
                        }
                    }
                }

                // Add all additional syllabus details to the main list after enumeration
                request.SyllabusDetails.AddRange(additionalDetails);

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
        //public async Task<ServiceResponse<string>> AddUpdateSyllabusDetails(SyllabusDetailsDTO request)
        //{
        //    try
        //    {
        //        // Validate PDF format
        //        foreach (var detail in request.SyllabusDetails)
        //        {
        //            if (string.IsNullOrEmpty(detail.Synopsis) || detail.Synopsis == "string")
        //            {
        //                detail.Synopsis = string.Empty;
        //            }
        //            else
        //            {
        //                if (!IsPdf(Convert.FromBase64String(detail.Synopsis)))
        //                {
        //                    throw new Exception("The provided synopsis is not a valid PDF file.");
        //                }
        //            }
        //            // If valid, you can optionally reassign the base64 string back
        //            detail.Synopsis = PDFUpload(detail.Synopsis); // If you still need to process the PDF
        //            detail.SyllabusID = request.SyllabusId;
        //            detail.SubjectId = request.SubjectId;
        //        }

        //        string insertQuery = @"
        //INSERT INTO tblSyllabusDetails (SyllabusID, ContentIndexId, IndexTypeId, Status, IsVerson, Synopsis, SubjectId)
        //VALUES (@SyllabusID, @ContentIndexId, @IndexTypeId, @Status, @IsVerson, @Synopsis, @SubjectId)";

        //        string deleteQuery = "DELETE FROM tblSyllabusDetails WHERE SyllabusID = @SyllabusID";

        //        // Check if the syllabus details exist
        //        int count = await _connection.ExecuteScalarAsync<int>(
        //            "SELECT COUNT(*) FROM tblSyllabusDetails WHERE SyllabusID = @SyllabusID",
        //            new { SyllabusID = request.SyllabusId }
        //        );

        //        if (count > 0)
        //        {
        //            // Delete existing details
        //            int rowsAffected = await _connection.ExecuteAsync(deleteQuery, new { SyllabusID = request.SyllabusId });
        //            if (rowsAffected <= 0)
        //            {
        //                return new ServiceResponse<string>(false, "Operation Failed to delete existing details", string.Empty, 500);
        //            }
        //        }

        //        // Insert new or updated details
        //        int addedRecords = await _connection.ExecuteAsync(insertQuery, request.SyllabusDetails);

        //        if (addedRecords > 0)
        //        {
        //            return new ServiceResponse<string>(true, "Operation Successful", "Syllabus Details Added Successfully", 200);
        //        }
        //        else
        //        {
        //            return new ServiceResponse<string>(false, "Operation Failed to add new details", string.Empty, 500);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
        //    }
        //}
        public async Task<ServiceResponse<SyllabusDetailsResponse>> GetSyllabusDetailsById(int syllabusId, int subjectId)
        {
            try
            {
                // SQL Query
                string sql = @"
        SELECT sd.*, s.*
        FROM [tblSyllabus] s
        JOIN [tblSyllabusDetails] sd ON s.SyllabusId = sd.SyllabusID
        WHERE s.SyllabusId = @SyllabusId
        AND sd.SubjectId = @SubjectId";

                var syllabusDetails = await _connection.QueryAsync<dynamic>(sql, new { SyllabusId = syllabusId, SubjectId = subjectId });

                // Process the results to create a hierarchical structure
                var contentIndexResponse = new List<ContentIndexResponses>();

                foreach (var detail in syllabusDetails)
                {
                    int indexTypeId = detail.IndexTypeId;
                    if (indexTypeId == 1) // Chapter
                    {
                        string getchapter = @"SELECT * FROM tblContentIndexChapters WHERE ContentIndexId = @ContentIndexId;";
                        var data = await _connection.QueryFirstOrDefaultAsync<ContentIndexResponses>(getchapter, new { ContentIndexId = detail.ContentIndexId });

                        var chapter = new ContentIndexResponses
                        {
                            ContentIndexId = data.ContentIndexId,
                            SubjectId = data.SubjectId,
                            ContentName_Chapter = string.IsNullOrEmpty(data.DisplayName) ? data.ContentName_Chapter : data.DisplayName,//data.ContentName_Chapter,
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
                           // DisplayName = data.DisplayName,
                            DisplayOrder = data.DisplayOrder,
                            ContentIndexTopics = new List<ContentIndexTopicsResponse>()
                        };

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
                            ContentName_Topic = string.IsNullOrEmpty(data.DisplayName) ? data.ContentName_Topic : data.DisplayName,//data.ContentName_Topic,
                            Status = data.Status,
                            IndexTypeId = indexTypeId,
                            CreatedOn = data.CreatedOn,
                            CreatedBy = data.CreatedBy,
                            ModifiedOn = data.ModifiedOn,
                            ModifiedBy = data.ModifiedBy,
                            EmployeeId = data.EmployeeId,
                            IsActive = data.Status,
                            TopicCode = data.TopicCode,
                           // DisplayName = data.DisplayName,
                            DisplayOrder = data.DisplayOrder,
                            ChapterCode = data.ChapterCode,
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
                            ContentName_SubTopic = string.IsNullOrEmpty(data.DisplayName) ? data.ContentName_SubTopic : data.DisplayName,//data.ContentName_SubTopic,
                            Status = data.Status,
                            IndexTypeId = indexTypeId,
                            CreatedOn = data.CreatedOn,
                            CreatedBy = data.CreatedBy,
                            ModifiedOn = data.ModifiedOn,
                            ModifiedBy = data.ModifiedBy,
                            EmployeeId = data.EmployeeId,
                            IsActive = data.Status,
                            SubTopicCode = data.SubTopicCode,
                           // DisplayName = data.DisplayName,
                            DisplayOrder = data.DisplayOrder,
                            TopicCode = data.TopicCode
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

                // Create response object with syllabusId and subjectId
                var response = new SyllabusDetailsResponse
                {
                    SyllabusId = syllabusId,
                    SubjectId = subjectId,
                    ContentIndexResponses = contentIndexResponse
                };

                return new ServiceResponse<SyllabusDetailsResponse>(true, "Syllabus details retrieved successfully", response, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<SyllabusDetailsResponse>(false, ex.Message, new SyllabusDetailsResponse(), 500);
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
        public async Task<ServiceResponse<List<ContentIndexResponses>>> GetAllContentIndexList(int SubjectId)
        {
            try
            {
                // Base query to fetch the content indexes
                string query = @"SELECT * FROM [tblContentIndexChapters] WHERE [IsActive] = 1 AND SubjectId = @SubjectId";

                // Fetch all matching records
                var contentIndexes = (await _connection.QueryAsync<ContentIndexResponses>(query, new { SubjectId })).ToList();

                if (contentIndexes.Any())
                {
                    // Fetch related topics and subtopics for each content index
                    foreach (var contentIndex in contentIndexes)
                    {
                        string topicsSql = @"SELECT * FROM [tblContentIndexTopics] WHERE [ChapterCode] = @chapterCode AND [IsActive] = 1
                                     ORDER BY CASE WHEN [DisplayOrder] = 0 THEN [ContentName_Topic] ELSE [DisplayOrder] END,
                                              [ContentName_Topic]"; // Sort alphabetically if DisplayOrder is 0 or same
                        var topics = (await _connection.QueryAsync<ContentIndexTopicsResponse>(topicsSql, new { chapterCode = contentIndex.ChapterCode })).ToList();

                        string subTopicsSql = @"
            SELECT st.*
            FROM [tblContentIndexSubTopics] st
            INNER JOIN [tblContentIndexTopics] t ON st.TopicCode = t.TopicCode
            WHERE t.ChapterCode = @chapterCode AND st.[IsActive] = 1
            ORDER BY CASE WHEN st.DisplayOrder = 0 THEN st.ContentName_SubTopic ELSE st.DisplayOrder END,
                     st.ContentName_SubTopic"; // Sort alphabetically if DisplayOrder is 0 or same
                        var subTopics = (await _connection.QueryAsync<ContentIndexSubTopicResponse>(subTopicsSql, new { chapterCode = contentIndex.ChapterCode })).ToList();

                        // Assign the subtopics to the respective topics
                        foreach (var topic in topics)
                        {
                            topic.ContentIndexSubTopics = subTopics
                                .Where(st => st.TopicCode == topic.TopicCode)
                                .OrderBy(st => st.DisplayOrder)
                                .ThenBy(st => st.DisplayName)
                                .ToList();
                        }

                        // Assign the topics to the content index
                        contentIndex.ContentIndexTopics = topics
                            .OrderBy(topic => topic.DisplayOrder)
                            .ThenBy(topic => topic.DisplayName)
                            .ToList();
                    }

                    // Order the content indexes by DisplayOrder and DisplayName
                    var orderedContentIndexes = contentIndexes
                        .OrderBy(c => c.DisplayOrder)
                        .ThenBy(c => c.DisplayName)
                        .ToList();

                    // Paginate the content indexes
                    var paginatedContentIndexes = orderedContentIndexes
                        .Select(ci => new ContentIndexResponses
                        {
                            ContentIndexId = ci.ContentIndexId,
                            SubjectId = ci.SubjectId,
                            ContentName_Chapter = ci.ContentName_Chapter,
                            Status = ci.Status,
                            IndexTypeId = ci.IndexTypeId,
                            BoardId = ci.BoardId,
                            ClassId = ci.ClassId,
                            CourseId = ci.CourseId,
                            APID = ci.APID,
                            CreatedOn = ci.CreatedOn,
                            CreatedBy = ci.CreatedBy,
                            ModifiedOn = ci.ModifiedOn,
                            ModifiedBy = ci.ModifiedBy,
                            EmployeeId = ci.EmployeeId,
                            ExamTypeId = ci.ExamTypeId,
                            IsActive = ci.IsActive,
                            ChapterCode = ci.ChapterCode,
                            DisplayName = ci.DisplayName,
                            DisplayOrder = ci.DisplayOrder,
                            ContentIndexTopics = ci?.ContentIndexTopics?.Select(topic => new ContentIndexTopicsResponse
                            {
                                ContInIdTopic = topic.ContInIdTopic,
                                ContentIndexId = topic.ContentIndexId,
                                ContentName_Topic = topic.ContentName_Topic,
                                Status = topic.Status,
                                IndexTypeId = topic.IndexTypeId,
                                CreatedOn = topic.CreatedOn,
                                CreatedBy = topic.CreatedBy,
                                ModifiedOn = topic.ModifiedOn,
                                ModifiedBy = topic.ModifiedBy,
                                EmployeeId = topic.EmployeeId,
                                IsActive = topic.IsActive,
                                TopicCode = topic.TopicCode,
                                ChapterCode = topic.ChapterCode,
                                DisplayName = topic.DisplayName,
                                DisplayOrder = topic.DisplayOrder,
                                ContentIndexSubTopics = topic?.ContentIndexSubTopics?.Select(subTopic => new ContentIndexSubTopicResponse
                                {
                                    ContInIdSubTopic = subTopic.ContInIdSubTopic,
                                    ContInIdTopic = subTopic.ContInIdTopic,
                                    ContentName_SubTopic = subTopic.ContentName_SubTopic,
                                    Status = subTopic.Status,
                                    IndexTypeId = subTopic.IndexTypeId,
                                    CreatedOn = subTopic.CreatedOn,
                                    CreatedBy = subTopic.CreatedBy,
                                    ModifiedOn = subTopic.ModifiedOn,
                                    ModifiedBy = subTopic.ModifiedBy,
                                    EmployeeId = subTopic.EmployeeId,
                                    IsActive = subTopic.IsActive,
                                    TopicCode = subTopic.TopicCode,
                                    SubTopicCode = subTopic.SubTopicCode,
                                    DisplayName = subTopic.DisplayName,
                                    DisplayOrder = subTopic.DisplayOrder
                                })
                                .OrderBy(subTopic => subTopic.DisplayOrder)
                                .ThenBy(subTopic => subTopic.DisplayName)
                                .ToList()
                            })
                            .OrderBy(topic => topic.DisplayOrder)
                            .ThenBy(topic => topic.DisplayName)
                            .ToList()
                        })
                        .ToList();

                    return new ServiceResponse<List<ContentIndexResponses>>(true, "Records found", paginatedContentIndexes, StatusCodes.Status302Found, contentIndexes.Count);
                }
                else
                {
                    return new ServiceResponse<List<ContentIndexResponses>>(false, "Records not found", new List<ContentIndexResponses>(), StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ContentIndexResponses>>(false, ex.Message, new List<ContentIndexResponses>(), StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<byte[]>> DownloadExcelFile(int SyllabusId)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Fetch syllabus details
                var syllabusQuery = @"
            SELECT [SyllabusId], [BoardID], [CourseId], [ClassId], [SyllabusName], [Status], [APID], [modifiedon], [modifiedby], [createdon], [createdby], [EmployeeID], [ExamTypeId]
            FROM [tblSyllabus]
            WHERE [SyllabusId] = @SyllabusId";

                var syllabus = await _connection.QueryFirstOrDefaultAsync(syllabusQuery, new { SyllabusId });

                if (syllabus == null)
                {
                    return new ServiceResponse<byte[]>(false, "Syllabus not found", Array.Empty<byte>(), 500);
                }

                // Fetch syllabus details
                var detailsQuery = @"
            SELECT [SyllabusDetailID], [SyllabusID], [Status], [IsVerson], [ContentIndexId], [IndexTypeId], [Synopsis], [SubjectId]
            FROM [tblSyllabusDetails]
            WHERE [SyllabusID] = @SyllabusId";

                var syllabusDetails = (await _connection.QueryAsync(detailsQuery, new { SyllabusId })).ToList();

                // Prepare mapping for chapters, topics, and subtopics
                var chaptersQuery = @"
            SELECT [ContentIndexId], [SubjectId], [ContentName_Chapter], [IndexTypeId], [DisplayOrder]
            FROM [tblContentIndexChapters]";

                var topicsQuery = @"
            SELECT [ContInIdTopic], [ContentIndexId], [ContentName_Topic], [IndexTypeId], [DisplayOrder]
            FROM [tblContentIndexTopics]";

                var subTopicsQuery = @"
            SELECT [ContInIdSubTopic], [ContInIdTopic], [ContentName_SubTopic], [IndexTypeId], [DisplayOrder]
            FROM [tblContentIndexSubTopics]";

                var chapters = (await _connection.QueryAsync(chaptersQuery)).ToList();
                var topics = (await _connection.QueryAsync(topicsQuery)).ToList();
                var subTopics = (await _connection.QueryAsync(subTopicsQuery)).ToList();

                // Create Excel package
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("SyllabusData");

                    // Add header
                    worksheet.Cells[1, 1].Value = "SyllabusID";
                    worksheet.Cells[1, 2].Value = "IndexTypeId";
                    worksheet.Cells[1, 3].Value = "ContentId";
                    worksheet.Cells[1, 4].Value = "SubjectName";
                    worksheet.Cells[1, 5].Value = "ChapterName";
                    worksheet.Cells[1, 6].Value = "Concept Name";
                    worksheet.Cells[1, 7].Value = "Sub-Concept Name";
                    worksheet.Cells[1, 8].Value = "DisplayOrder";

                    // Remove duplicates by using a HashSet to track ContentIndexIds
                    var seenContentIndexIds = new HashSet<int>();

                    int row = 2;
                    foreach (var detail in syllabusDetails)
                    {
                        // Skip if ContentIndexId is already processed
                        if (seenContentIndexIds.Contains(detail.ContentIndexId))
                            continue;

                        var contentRow = new ContentRow
                        {
                            ChapterName = string.Empty,
                            TopicName = string.Empty,
                            SubTopicName = string.Empty,
                            DisplayOrder = 0
                        };

                        if (detail.IndexTypeId == 1) // Chapter
                        {
                            var chapter = chapters.FirstOrDefault(c => c.ContentIndexId == detail.ContentIndexId);
                            contentRow = new ContentRow
                            {
                                ChapterName = chapter?.ContentName_Chapter,
                                TopicName = string.Empty,
                                SubTopicName = string.Empty,
                                DisplayOrder = chapter?.DisplayOrder
                            };
                        }
                        else if (detail.IndexTypeId == 2) // Topic
                        {
                            var topic = topics.FirstOrDefault(t => t.ContInIdTopic == detail.ContentIndexId);
                            contentRow = new ContentRow
                            {
                                ChapterName = chapters.FirstOrDefault(c => c.ContentIndexId == topic?.ContentIndexId)?.ContentName_Chapter,
                                TopicName = topic?.ContentName_Topic,
                                SubTopicName = string.Empty,
                                DisplayOrder = topic?.DisplayOrder
                            };
                        }
                        else if (detail.IndexTypeId == 3) // SubTopic
                        {
                            var subTopic = subTopics.FirstOrDefault(st => st.ContInIdSubTopic == detail.ContentIndexId);
                            var topic = topics.FirstOrDefault(t => t.ContInIdTopic == subTopic?.ContInIdTopic);
                            var chapter = chapters.FirstOrDefault(c => c.ContentIndexId == topic?.ContentIndexId);
                            contentRow = new ContentRow
                            {
                                ChapterName = chapter?.ContentName_Chapter,
                                TopicName = topic?.ContentName_Topic,
                                SubTopicName = subTopic?.ContentName_SubTopic,
                                DisplayOrder = subTopic?.DisplayOrder
                            };
                        }

                        // Add to HashSet to avoid duplicates
                        seenContentIndexIds.Add(detail.ContentIndexId);
                        //a query to fetch subject name using detail.SubjectId
                        var subjectQuery = @"
                                    SELECT [SubjectName]
                                    FROM [tblSubject]
                                    WHERE [SubjectId] = @SubjectId";

                        var subjectName = await _connection.QueryFirstOrDefaultAsync<string>(subjectQuery, new { SubjectId = detail.SubjectId });

                        worksheet.Cells[row, 1].Value = detail.SyllabusID;
                        worksheet.Cells[row, 2].Value = detail.IndexTypeId;
                        worksheet.Cells[row, 3].Value = detail.ContentIndexId;
                        worksheet.Cells[row, 4].Value = subjectName;
                        worksheet.Cells[row, 5].Value = contentRow.ChapterName;
                        worksheet.Cells[row, 6].Value = contentRow.TopicName;
                        worksheet.Cells[row, 7].Value = contentRow.SubTopicName;
                        worksheet.Cells[row, 8].Value = contentRow.DisplayOrder;
                        row++;
                    }

                    // Convert to byte array
                    var fileBytes = package.GetAsByteArray();

                    return new ServiceResponse<byte[]>(true, "Excel generated successfully", fileBytes, 200);
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                return new ServiceResponse<byte[]>(false, ex.Message, Array.Empty<byte>(), 500);
            }
        }
        public async Task<ServiceResponse<string>> UploadSyllabusDetails(IFormFile file)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                // Check if file is not null and has content
                if (file == null || file.Length == 0)
                {
                    return new ServiceResponse<string>(false, "Invalid file. Please upload a valid syllabus details file.", string.Empty, 400);
                }

                // Read the file content
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0;

                // Use OfficeOpenXml to read the Excel file
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0]; // Assume data is in the first worksheet
                var rowCount = worksheet.Dimension.Rows;

                var syllabusDetails = new List<SyllabusDetails>();
                var headerRow = 1; // Assuming the first row is header

                int syllabusIdToDelete = 0; // Will be used to store the syllabus ID for deletion

                for (int row = headerRow + 1; row <= rowCount; row++)
                {
                    var syllabusIdCell = worksheet.Cells[row, 1].Text.Trim();
                    var indexTypeIdCell = worksheet.Cells[row, 2].Text.Trim();
                    var contentIdCell = worksheet.Cells[row, 3].Text.Trim();
                    var subjectName = worksheet.Cells[row, 4].Text.Trim(); // Use a query to fetch subject ID using subject name

                    // Validate data
                    if (string.IsNullOrEmpty(indexTypeIdCell) || string.IsNullOrEmpty(contentIdCell) || string.IsNullOrEmpty(syllabusIdCell))
                    {
                        continue; // Skip rows with missing essential data
                    }

                    // Parse integer values
                    if (!int.TryParse(indexTypeIdCell, out int indexTypeId) ||
                        !int.TryParse(contentIdCell, out int contentIndexId) ||
                        !int.TryParse(syllabusIdCell, out int syllabusId))
                    {
                        return new ServiceResponse<string>(false, $"Invalid data at row {row}", null, 400);
                    }

                    if (syllabusIdToDelete == 0) // Set syllabus ID to delete records
                    {
                        syllabusIdToDelete = syllabusId;
                    }

                    var subjectQuery = "SELECT SubjectId FROM tblSubject WHERE SubjectName = @SubjectName";
                    var subjectId = await _connection.QueryFirstOrDefaultAsync<int?>(subjectQuery, new { SubjectName = subjectName });

                    if (subjectId == null)
                    {
                        return new ServiceResponse<string>(false, $"Subject '{subjectName}' not found at row {row}", null, 404);
                    }

                    // Create SyllabusDetails object
                    var detail = new SyllabusDetails
                    {
                        IndexTypeId = indexTypeId,
                        ContentIndexId = contentIndexId,
                        SyllabusID = syllabusId,
                        Status = 1, // Default or set based on your requirements
                        IsVerson = 1, // Default version or set based on your requirements
                        SubjectId = subjectId.Value
                    };

                    syllabusDetails.Add(detail);
                }

                if (syllabusDetails.Count == 0)
                {
                    return new ServiceResponse<string>(false, "No valid data found in the file.", string.Empty, 400);
                }

                // Delete existing details
                string deleteQuery = "DELETE FROM tblSyllabusDetails WHERE SyllabusID = @SyllabusID";
                int rowsDeleted = await _connection.ExecuteAsync(deleteQuery, new { SyllabusID = syllabusIdToDelete });

                if (rowsDeleted < 0)
                {
                    return new ServiceResponse<string>(false, "Failed to delete existing syllabus details.", string.Empty, 500);
                }

                // Define the SQL insert query
                string insertQuery = @"
        INSERT INTO tblSyllabusDetails (SyllabusID, ContentIndexId, IndexTypeId, Status, IsVerson, SubjectId)
        VALUES (@SyllabusID, @ContentIndexId, @IndexTypeId, @Status, @IsVerson, @SubjectId)";

                // Insert the parsed data into the database
                int addedRecords = await _connection.ExecuteAsync(insertQuery, syllabusDetails);

                if (addedRecords > 0)
                {
                    return new ServiceResponse<string>(true, "Operation Successful", "Syllabus details uploaded successfully.", 200);
                }
                else
                {
                    return new ServiceResponse<string>(false, "Operation Failed to insert data", string.Empty, 500);
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
        public class ContentRow
        {
            public string ChapterName { get; set; }
            public string TopicName { get; set; }
            public string SubTopicName { get; set; }
            public int DisplayOrder { get; set; }
        }
    }
}