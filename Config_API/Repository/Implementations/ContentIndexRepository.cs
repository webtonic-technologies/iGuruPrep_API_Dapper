using Config_API.DTOs.Requests;
using Config_API.DTOs.Response;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
using Config_API.Repository.Interfaces;
using Dapper;
using OfficeOpenXml;
using System.Data;
using System.Data.SqlClient;

namespace Config_API.Repository.Implementations
{
    public class ContentIndexRepository : IContentIndexRepository
    {

        private readonly IDbConnection _connection;
        public ContentIndexRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<List<ContentIndexResponse>>> GetAllContentIndexList(ContentIndexListDTO request)
        {
            try
            {
                // Base query to fetch the content indexes
                string query = @"SELECT * FROM [tblContentIndexChapters] WHERE [IsActive] = 1";

                // Add filters based on DTO properties
                if (request.APID > 0)
                {
                    query += " AND [APID] = @APID";
                }
                if (request.SubjectId > 0)
                {
                    query += " AND [SubjectId] = @SubjectId";
                }

                // Fetch all matching records
                var contentIndexes = (await _connection.QueryAsync<ContentIndexResponse>(query, new { request.APID, request.SubjectId })).ToList();

                if (contentIndexes.Any())
                {
                    // Fetch related topics and subtopics for each content index
                    foreach (var contentIndex in contentIndexes)
                    {
                        string topicsSql = @"SELECT * FROM [tblContentIndexTopics] WHERE [ChapterCode] = @chapterCode AND [IsActive] = 1";
                        var topics = (await _connection.QueryAsync<ContentIndexTopicsResponse>(topicsSql, new { chapterCode = contentIndex.ChapterCode })).ToList();

                        string subTopicsSql = @"
                SELECT st.*
                FROM [tblContentIndexSubTopics] st
                INNER JOIN [tblContentIndexTopics] t ON st.TopicCode = t.TopicCode
                WHERE t.ChapterCode = @chapterCode AND st.[IsActive] = 1";
                        var subTopics = (await _connection.QueryAsync<ContentIndexSubTopicResponse>(subTopicsSql, new { chapterCode = contentIndex.ChapterCode })).ToList();

                        // Assign the subtopics to the respective topics
                        foreach (var topic in topics)
                        {
                            topic.ContentIndexSubTopics = subTopics
                                .Where(st => st.TopicCode == topic.TopicCode)
                                .OrderBy(st => st.DisplayOrder == 0 ? 1 : 0) // Prioritize non-zero display orders
                                .ThenBy(st => st.DisplayOrder) // Sort by DisplayOrder
                                .ThenBy(st => st.DisplayName) // Sort alphabetically if DisplayOrder is 0 or same
                                .ToList();
                        }

                        // Assign the topics to the content index
                        contentIndex.ContentIndexTopics = topics
                            .OrderBy(topic => topic.DisplayOrder == 0 ? 1 : 0) // Prioritize non-zero display orders
                            .ThenBy(topic => topic.DisplayOrder) // Sort by DisplayOrder
                            .ThenBy(topic => topic.DisplayName) // Sort alphabetically if DisplayOrder is 0 or same
                            .ToList();
                    }

                    // Order the content indexes by DisplayOrder and DisplayName
                    var orderedContentIndexes = contentIndexes
                        .OrderBy(c => c.DisplayOrder == 0 ? 1 : 0) // Prioritize non-zero display orders
                        .ThenBy(c => c.DisplayOrder) // Sort by DisplayOrder
                        .ThenBy(c => c.DisplayName) // Sort alphabetically if DisplayOrder is 0 or same
                        .ToList();

                    // Paginate the content indexes
                    var paginatedContentIndexes = orderedContentIndexes
                        .Skip((request.PageNumber - 1) * request.PageSize)
                        .Take(request.PageSize)
                        .Select(ci => new ContentIndexResponse
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
                                .OrderBy(subTopic => subTopic.DisplayOrder == 0 ? 1 : 0) // Prioritize non-zero display orders
                                .ThenBy(subTopic => subTopic.DisplayOrder) // Sort by DisplayOrder
                                .ThenBy(subTopic => subTopic.DisplayName) // Sort alphabetically if DisplayOrder is 0 or same
                                .ToList()
                            })
                            .OrderBy(topic => topic.DisplayOrder == 0 ? 1 : 0) // Prioritize non-zero display orders
                            .ThenBy(topic => topic.DisplayOrder) // Sort by DisplayOrder
                            .ThenBy(topic => topic.DisplayName) // Sort alphabetically if DisplayOrder is 0 or same
                            .ToList()
                        })
                        .ToList();

                    return new ServiceResponse<List<ContentIndexResponse>>(true, "Records found", paginatedContentIndexes, StatusCodes.Status302Found, contentIndexes.Count);
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
        public async Task<ServiceResponse<ContentIndexResponse>> GetContentIndexById(int id)
        {
            try
            {
                // Fetch the main content index
                string contentIndexSql = @"SELECT * FROM [tblContentIndexChapters] WHERE [ContentIndexId] = @id AND [IsActive] = 1";
                var contentIndex = await _connection.QueryFirstOrDefaultAsync<ContentIndexResponse>(contentIndexSql, new { id });

                if (contentIndex == null)
                {
                    return new ServiceResponse<ContentIndexResponse>(false, "Records Not Found", new ContentIndexResponse(), StatusCodes.Status204NoContent);
                }

                // Fetch the related topics by ChapterCode
                string topicsSql = @"SELECT * FROM [tblContentIndexTopics] WHERE [ChapterCode] = @chapterCode AND IsActive = 1";
                var topics = await _connection.QueryAsync<ContentIndexTopicsResponse>(topicsSql, new { chapterCode = contentIndex.ChapterCode });

                // Fetch the related subtopics by TopicCodes
                string subTopicsSql = @"
            SELECT st.* 
            FROM [tblContentIndexSubTopics] st 
            INNER JOIN [tblContentIndexTopics] t ON st.TopicCode = t.TopicCode 
            WHERE t.ChapterCode = @chapterCode AND st.[IsActive] = 1";
                var subTopics = await _connection.QueryAsync<ContentIndexSubTopicResponse>(subTopicsSql, new { chapterCode = contentIndex.ChapterCode });

                // Create a ContentIndexResponse object and assign the fetched data
                var contentIndexResponse = new ContentIndexResponse
                {
                    ContentIndexId = contentIndex.ContentIndexId,
                    SubjectId = contentIndex.SubjectId,
                    ContentName_Chapter = contentIndex.ContentName_Chapter,
                    Status = contentIndex.Status,
                    IndexTypeId = contentIndex.IndexTypeId,
                    BoardId = contentIndex.BoardId,
                    ClassId = contentIndex.ClassId,
                    CourseId = contentIndex.CourseId,
                    APID = contentIndex.APID,
                    CreatedOn = contentIndex.CreatedOn,
                    CreatedBy = contentIndex.CreatedBy,
                    ModifiedOn = contentIndex.ModifiedOn,
                    ModifiedBy = contentIndex.ModifiedBy,
                    EmployeeId = contentIndex.EmployeeId,
                    ExamTypeId = contentIndex.ExamTypeId,
                    IsActive = contentIndex.IsActive,
                    ChapterCode = contentIndex.ChapterCode,
                    DisplayName = contentIndex.DisplayName,
                    DisplayOrder = contentIndex.DisplayOrder,
                    ContentIndexTopics = topics.Select(topic => new ContentIndexTopicsResponse
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
                        DisplayName = topic.DisplayName,
                        ChapterCode = topic.ChapterCode,
                        DisplayOrder = topic.DisplayOrder,
                        ContentIndexSubTopics = subTopics
                            .Where(st => st.TopicCode == topic.TopicCode)
                            .Select(st => new ContentIndexSubTopicResponse
                            {
                                ContInIdSubTopic = st.ContInIdSubTopic,
                                ContInIdTopic = st.ContInIdTopic,
                                ContentName_SubTopic = st.ContentName_SubTopic,
                                Status = st.Status,
                                IndexTypeId = st.IndexTypeId,
                                CreatedOn = st.CreatedOn,
                                CreatedBy = st.CreatedBy,
                                ModifiedOn = st.ModifiedOn,
                                ModifiedBy = st.ModifiedBy,
                                EmployeeId = st.EmployeeId,
                                IsActive = st.IsActive,
                                SubTopicCode = st.SubTopicCode,
                                DisplayName = st.DisplayName,
                                DisplayOrder = st.DisplayOrder,
                            })
                            .OrderBy(st => st.DisplayOrder == 0 ? 1 : 0) // Prioritize non-zero display orders
                            .ThenBy(st => st.DisplayOrder) // Sort by DisplayOrder
                            .ThenBy(st => st.DisplayName) // Sort alphabetically if DisplayOrder is 0 or same
                            .ToList()
                    })
                    .OrderBy(topic => topic.DisplayOrder == 0 ? 1 : 0) // Prioritize non-zero display orders
                    .ThenBy(topic => topic.DisplayOrder) // Sort by DisplayOrder
                    .ThenBy(topic => topic.DisplayName) // Sort alphabetically if DisplayOrder is 0 or same
                    .ToList()
                };

                return new ServiceResponse<ContentIndexResponse>(true, "Records Found", contentIndexResponse, StatusCodes.Status302Found);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ContentIndexResponse>(false, ex.Message, new ContentIndexResponse(), StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            using var transaction = _connection.BeginTransaction();
            try
            {
                string contentIndexSql = @"SELECT * FROM [tblContentIndexChapters] WHERE [ContentIndexId] = @id";
                var data = await _connection.QueryFirstOrDefaultAsync<ContentIndexChapters>(contentIndexSql, new { id }, transaction);

                if (data != null)
                {
                    // Toggle the status
                    data.Status = !data.Status;

                    // Update the main content index status
                    string updateContentIndexQuery = @"
                UPDATE [tblContentIndexChapters] 
                SET Status = @Status, 
                    ModifiedOn = @ModifiedOn 
                WHERE [ContentIndexId] = @Id";

                    int rowsAffected = await _connection.ExecuteAsync(updateContentIndexQuery, new
                    {
                        data.Status,
                        ModifiedOn = DateTime.Now,
                        Id = id
                    }, transaction);

                    if (rowsAffected > 0)
                    {
                        // Fetch and update related topics status
                        string updateTopicsQuery = @"
                    UPDATE [tblContentIndexTopics] 
                    SET Status = @Status, 
                        ModifiedOn = @ModifiedOn 
                    WHERE [ContentIndexId] = @ContentIndexId";

                        await _connection.ExecuteAsync(updateTopicsQuery, new
                        {
                            data.Status,
                            ModifiedOn = DateTime.Now,
                            ContentIndexId = id
                        }, transaction);

                        // Fetch and update related subtopics status
                        string updateSubTopicsQuery = @"
                    UPDATE st
                    SET st.Status = @Status,
                        st.ModifiedOn = @ModifiedOn
                    FROM [tblContentIndexSubTopics] st
                    INNER JOIN [tblContentIndexTopics] t ON st.ContInIdTopic = t.ContInIdTopic
                    WHERE t.ContentIndexId = @ContentIndexId";

                        await _connection.ExecuteAsync(updateSubTopicsQuery, new
                        {
                            data.Status,
                            ModifiedOn = DateTime.Now,
                            ContentIndexId = id
                        }, transaction);

                        transaction.Commit();
                        return new ServiceResponse<bool>(true, "Operation Successful", true, StatusCodes.Status200OK);
                    }
                    else
                    {
                        transaction.Rollback();
                        return new ServiceResponse<bool>(false, "Operation Failed", false, StatusCodes.Status304NotModified);
                    }
                }
                else
                {
                    return new ServiceResponse<bool>(false, "Record Not Found", false, StatusCodes.Status404NotFound);
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return new ServiceResponse<bool>(false, ex.Message, false, StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task<ServiceResponse<string>> AddUpdateContentIndex(ContentIndexRequest request)
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }
            try
            {
                if (request.ContentIndexId == 0)
                {
                    // Insert new content index
                    string insertQuery = @"
                INSERT INTO tblContentIndexChapters (SubjectId, ContentName_Chapter, IndexTypeId, Status, ClassId, BoardId, APID, CreatedOn, CreatedBy, CourseId, EmployeeId, ExamTypeId, ChapterCode, DisplayName, DisplayOrder, IsActive)
                VALUES (@SubjectId, @ContentName_Chapter, @IndexTypeId, @Status, @ClassId, @BoardId, @APID, @CreatedOn, @CreatedBy, @CourseId, @EmployeeId, @ExamTypeId, @ChapterCode, @DisplayName, @DisplayOrder, @IsActive);
                SELECT CAST(SCOPE_IDENTITY() as int);";

                    string newChapterCode = GenerateCode();

                    int insertedId = await _connection.QuerySingleAsync<int>(insertQuery, new
                    {
                        request.SubjectId,
                        request.ContentName_Chapter,
                        request.IndexTypeId,
                        request.Status,
                        request.ClassId,
                        request.BoardId,
                        request.APID,
                        CreatedOn = DateTime.Now,
                        request.CreatedBy,
                        request.CourseId,
                        request.EmployeeId,
                        request.ExamTypeId,
                        ChapterCode = newChapterCode,
                        request.DisplayName,
                        request.DisplayOrder,
                        request.IsActive
                    });

                    if (insertedId > 0)
                    {
                        await InsertOrUpdateContentIndexTopics(newChapterCode, request.ContentIndexTopics);
                        return new ServiceResponse<string>(true, "Operation Successful", "Content Index Added Successfully", StatusCodes.Status201Created);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Operation Failed", string.Empty, StatusCodes.Status400BadRequest);
                    }
                }
                else
                {
                    // Update existing content index
                    string updateQuery = @"
                UPDATE tblContentIndexChapters SET
                    SubjectId = @SubjectId,
                    ContentName_Chapter = @ContentName_Chapter,
                    IndexTypeId = @IndexTypeId,
                    Status = @Status,
                    ClassId = @ClassId,
                    BoardId = @BoardId,
                    APID = @APID,
                    ModifiedOn = @ModifiedOn,
                    ModifiedBy = @ModifiedBy,
                    CourseId = @CourseId,
                    EmployeeId = @EmployeeId,
                    ExamTypeId = @ExamTypeId,
                    DisplayName = @DisplayName,
                    DisplayOrder = @DisplayOrder,
                    IsActive = @IsActive
                WHERE ContentIndexId = @ContentIndexId AND IsActive = 1";

                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, new
                    {
                        request.ContentIndexId,
                        request.SubjectId,
                        request.ContentName_Chapter,
                        request.IndexTypeId,
                        request.Status,
                        request.ClassId,
                        request.BoardId,
                        request.APID,
                        ModifiedOn = DateTime.Now,
                        request.ModifiedBy,
                        request.CourseId,
                        request.EmployeeId,
                        request.ExamTypeId,
                        request.DisplayName,
                        request.DisplayOrder,
                        request.IsActive
                    });

                    if (rowsAffected > 0)
                    {
                        // Get chapter code
                        string chapterCodeQuery = "SELECT ChapterCode FROM tblContentIndexChapters WHERE ContentIndexId = @ContentIndexId";
                        string chapterCode = await _connection.QuerySingleOrDefaultAsync<string>(chapterCodeQuery, new { request.ContentIndexId });

                        await InsertOrUpdateContentIndexTopics(chapterCode, request.ContentIndexTopics);
                        return new ServiceResponse<string>(true, "Operation Successful", "Content Index Updated Successfully", StatusCodes.Status200OK);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Operation Failed", string.Empty, StatusCodes.Status404NotFound);
                    }
                }
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // SQL error numbers for unique key violation
            {
                return new ServiceResponse<string>(false, "Data already exists.", string.Empty, StatusCodes.Status409Conflict);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, StatusCodes.Status500InternalServerError);
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task<ServiceResponse<List<ContentIndexResponse>>> GetAllContentIndexListMasters(ContentIndexMastersDTO request)
        {
            try
            {
                // Base query to fetch the content indexes
                string query = @"
        SELECT * FROM [tblContentIndexChapters] 
        WHERE [IsActive] = 1";

                // Add filters based on request properties
                if (request.APID > 0)
                {
                    query += " AND [APID] = @APID";
                }
                if (request.SubjectId > 0)
                {
                    query += " AND [SubjectId] = @SubjectId";
                }

                // Sort by DisplayOrder or DisplayName
                query += " ORDER BY CASE WHEN [DisplayOrder] = 0 THEN 1 ELSE 0 END, [DisplayOrder], [DisplayName]";

                // Fetch content indexes
                var contentIndexes = (await _connection.QueryAsync<ContentIndexResponse>(query, new { request.APID, request.SubjectId })).ToList();

                if (contentIndexes.Any())
                {
                    // Fetch related topics and subtopics for each content index
                    foreach (var contentIndex in contentIndexes)
                    {
                        string topicsSql = @"
                SELECT * FROM [tblContentIndexTopics] 
                WHERE [ContentIndexId] = @contentIndexId AND [IsActive] = 1 
                ORDER BY CASE WHEN [DisplayOrder] = 0 THEN 1 ELSE 0 END, [DisplayOrder], [ContentName_Topic]"; // Sort by DisplayOrder or ContentName_Topic
                        var topics = (await _connection.QueryAsync<ContentIndexTopicsResponse>(topicsSql, new { contentIndexId = contentIndex.ContentIndexId })).ToList();

                        string subTopicsSql = @"
                SELECT st.* 
                FROM [tblContentIndexSubTopics] st
                INNER JOIN [tblContentIndexTopics] t ON st.ContInIdTopic = t.ContInIdTopic
                WHERE t.ContentIndexId = @contentIndexId AND st.[IsActive] = 1
                ORDER BY CASE WHEN st.DisplayOrder = 0 THEN 1 ELSE 0 END, st.DisplayOrder, st.ContentName_SubTopic"; // Sort by DisplayOrder or ContentName_SubTopic
                        var subTopics = (await _connection.QueryAsync<ContentIndexSubTopicResponse>(subTopicsSql, new { contentIndexId = contentIndex.ContentIndexId })).ToList();

                        // Assign subtopics to topics
                        foreach (var topic in topics)
                        {
                            topic.ContentIndexSubTopics = subTopics
                                .Where(st => st.ContInIdTopic == topic.ContInIdTopic)
                                .OrderBy(st => st.DisplayOrder == 0 ? 1 : 0) // Prioritize non-zero display orders
                                .ThenBy(st => st.DisplayOrder) // Sort by DisplayOrder
                                .ThenBy(st => st.DisplayName) // Sort alphabetically if DisplayOrder is 0 or same
                                .ToList();
                        }

                        // Assign topics to content index
                        contentIndex.ContentIndexTopics = topics
                            .OrderBy(topic => topic.DisplayOrder == 0 ? 1 : 0) // Prioritize non-zero display orders
                            .ThenBy(topic => topic.DisplayOrder) // Sort by DisplayOrder
                            .ThenBy(topic => topic.DisplayName) // Sort alphabetically if DisplayOrder is 0 or same
                            .ToList();
                    }

                    return new ServiceResponse<List<ContentIndexResponse>>(true, "Records found", contentIndexes, StatusCodes.Status302Found);
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
        public async Task<ServiceResponse<string>> AddUpdateContentIndexChapter(ContentIndexRequestdto request)
        {
            try
            {
                string insertSql = @"
                INSERT INTO tblContentIndexChapters 
                (SubjectId, ContentName_Chapter, Status, IndexTypeId, BoardId, ClassId, CourseId, APID, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, EmployeeId, ExamTypeId, DisplayName, DisplayOrder, IsActive, ChapterCode) 
                VALUES 
                (@SubjectId, @ContentName_Chapter, @Status, @IndexTypeId, @BoardId, @ClassId, @CourseId, @APID, @CreatedOn, @CreatedBy, @ModifiedOn, @ModifiedBy, @EmployeeId, @ExamTypeId, @DisplayName, @DisplayOrder, @IsActive, @ChapterCode);
                SELECT @ChapterCode;";

                string updateSql = @"
                UPDATE tblContentIndexChapters SET 
                    SubjectId = @SubjectId, 
                    ContentName_Chapter = @ContentName_Chapter, 
                    Status = @Status, 
                    IndexTypeId = @IndexTypeId, 
                    BoardId = @BoardId, 
                    ClassId = @ClassId, 
                    CourseId = @CourseId, 
                    APID = @APID, 
                    CreatedOn = @CreatedOn, 
                    CreatedBy = @CreatedBy, 
                    ModifiedOn = @ModifiedOn, 
                    ModifiedBy = @ModifiedBy, 
                    EmployeeId = @EmployeeId, 
                    ExamTypeId = @ExamTypeId, 
                    DisplayName = @DisplayName, 
                    DisplayOrder = @DisplayOrder, 
                    IsActive = @IsActive 
                WHERE ChapterCode = @ChapterCode AND IsActive = 1";

                string result;
                if (request.ContentIndexId == 0)
                {
                    string chapterCode = GenerateCode();
                    result = await _connection.ExecuteScalarAsync<string>(insertSql, new
                    {
                        request.SubjectId,
                        request.ContentName_Chapter,
                        request.Status,
                        request.IndexTypeId,
                        request.BoardId,
                        request.ClassId,
                        request.CourseId,
                        request.APID,
                        request.CreatedOn,
                        request.CreatedBy,
                        request.ModifiedOn,
                        request.ModifiedBy,
                        request.EmployeeId,
                        request.ExamTypeId,
                        request.DisplayName,
                        request.DisplayOrder,
                        request.IsActive,
                        ChapterCode = chapterCode
                    });
                }
                else
                {
                    await _connection.ExecuteAsync(updateSql, request);
                    result = request.ChapterCode;
                }

                return new ServiceResponse<string>(true, "Operation successful", result, 200);
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // SQL error numbers for unique key violation
            {
                return new ServiceResponse<string>(false, "Chapter already exists.", string.Empty, StatusCodes.Status409Conflict);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<string>> AddUpdateContentIndexTopics(ContentIndexTopicsdto request)
        {
            try
            {
                // Fetch the ContentIndexId based on ChapterCode and IsActive = 1
                int? contentIndexId = await GetContentIndexIdByChapterCode(request.ChapterCode);
                if (contentIndexId == null)
                {
                    return new ServiceResponse<string>(false, "Invalid ChapterCode or Chapter is not active", string.Empty, 400);
                }
                request.ContentIndexId = contentIndexId.Value;

                string insertSql = @"
        INSERT INTO tblContentIndexTopics 
        (ContentIndexId, ContentName_Topic, Status, IndexTypeId, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, EmployeeId, DisplayName, DisplayOrder, IsActive, TopicCode, ChapterCode) 
        VALUES 
        (@ContentIndexId, @ContentName_Topic, @Status, @IndexTypeId, @CreatedOn, @CreatedBy, @ModifiedOn, @ModifiedBy, @EmployeeId, @DisplayName, @DisplayOrder, @IsActive, @TopicCode, @ChapterCode);
        SELECT @TopicCode;";

                string updateSql = @"
        UPDATE tblContentIndexTopics SET 
            ContentIndexId = @ContentIndexId, 
            ContentName_Topic = @ContentName_Topic, 
            Status = @Status, 
            IndexTypeId = @IndexTypeId, 
            CreatedOn = @CreatedOn, 
            CreatedBy = @CreatedBy, 
            ModifiedOn = @ModifiedOn, 
            ModifiedBy = @ModifiedBy, 
            EmployeeId = @EmployeeId, 
            DisplayName = @DisplayName, 
            DisplayOrder = @DisplayOrder, 
            IsActive = @IsActive 
        WHERE ContInIdTopic = @ContInIdTopic";

                string result;
                if (request.ContInIdTopic == 0)
                {
                    string topicCode = GenerateCode();
                    result = await _connection.ExecuteScalarAsync<string>(insertSql, new
                    {
                        request.ContentIndexId,
                        request.ContentName_Topic,
                        request.Status,
                        request.IndexTypeId,
                        request.CreatedOn,
                        request.CreatedBy,
                        request.ModifiedOn,
                        request.ModifiedBy,
                        request.EmployeeId,
                        request.DisplayName,
                        request.DisplayOrder,
                        request.IsActive,
                        request.ChapterCode,
                        TopicCode = topicCode
                    });
                }
                else
                {
                    await _connection.ExecuteAsync(updateSql, request);
                    result = request.TopicCode;
                }

                return new ServiceResponse<string>(true, "Operation successful", result, 200);
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // SQL error numbers for unique key violation
            {
                return new ServiceResponse<string>(false, "Topic already exists.", string.Empty, StatusCodes.Status409Conflict);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<string>> AddUpdateContentIndexSubTopics(ContentIndexSubTopic request)
        {
            try
            {
                // Fetch the ContInIdTopic based on TopicCode and IsActive = 1
                int? contInIdTopic = await GetContInIdTopicByTopicCode(request.TopicCode);
                if (contInIdTopic == null)
                {
                    return new ServiceResponse<string>(false, "Invalid TopicCode or Topic is not active", string.Empty, 400);
                }
                request.ContInIdTopic = contInIdTopic.Value;

                string insertSql = @"
        INSERT INTO tblContentIndexSubTopics 
        (ContInIdTopic, ContentName_SubTopic, Status, IndexTypeId, CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, EmployeeId, DisplayName, DisplayOrder, IsActive, SubTopicCode, TopicCode) 
        VALUES 
        (@ContInIdTopic, @ContentName_SubTopic, @Status, @IndexTypeId, @CreatedOn, @CreatedBy, @ModifiedOn, @ModifiedBy, @EmployeeId, @DisplayName, @DisplayOrder, @IsActive, @SubTopicCode, @TopicCode);
        SELECT @SubTopicCode;";

                string updateSql = @"
        UPDATE tblContentIndexSubTopics SET 
            ContInIdTopic = @ContInIdTopic, 
            ContentName_SubTopic = @ContentName_SubTopic, 
            Status = @Status, 
            IndexTypeId = @IndexTypeId, 
            CreatedOn = @CreatedOn, 
            CreatedBy = @CreatedBy, 
            ModifiedOn = @ModifiedOn, 
            ModifiedBy = @ModifiedBy, 
            EmployeeId = @EmployeeId, 
            DisplayName = @DisplayName, 
            DisplayOrder = @DisplayOrder, 
            IsActive = @IsActive 
        WHERE ContInIdSubTopic = @ContInIdSubTopic";

                string result;
                if (request.ContInIdSubTopic == 0)
                {
                    string subTopicCode = GenerateCode();
                    result = await _connection.ExecuteScalarAsync<string>(insertSql, new
                    {
                        request.ContInIdTopic,
                        request.ContentName_SubTopic,
                        request.Status,
                        request.IndexTypeId,
                        request.CreatedOn,
                        request.CreatedBy,
                        request.ModifiedOn,
                        request.ModifiedBy,
                        request.EmployeeId,
                        request.DisplayName,
                        request.DisplayOrder,
                        request.IsActive,
                        SubTopicCode = subTopicCode,
                        request.TopicCode
                    });
                }
                else
                {
                    await _connection.ExecuteAsync(updateSql, request);
                    result = request.SubTopicCode;
                }

                return new ServiceResponse<string>(true, "Operation successful", result, 200);
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // SQL error numbers for unique key violation
            {
                return new ServiceResponse<string>(false, "Sub topic already exists.", string.Empty, StatusCodes.Status409Conflict);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<byte[]>> DownloadContentIndexBySubjectId(int subjectId)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Fetch the subject information
                string subjectSql = @"
            SELECT [SubjectCode]
            FROM [tblSubject]
            WHERE [SubjectId] = @subjectId";
                var subject = await _connection.QuerySingleOrDefaultAsync<string>(subjectSql, new { subjectId });

                if (subject == null)
                {
                    return new ServiceResponse<byte[]>(false, "Subject not found", null, StatusCodes.Status204NoContent);
                }

                // Fetch the main content index chapters
                string contentIndexSql = @"
            SELECT * 
            FROM [tblContentIndexChapters] 
            WHERE [SubjectId] = @subjectId AND [IsActive] = 1";

                var contentIndexes = await _connection.QueryAsync<ContentIndexResponse>(contentIndexSql, new { subjectId });

                // Create an Excel sheet with headers
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("ContentIndex");

                    // Add header
                    worksheet.Cells[1, 1].Value = "subjectcode";
                    worksheet.Cells[1, 2].Value = "chapter";
                    worksheet.Cells[1, 3].Value = "displayorder_chapter";
                    worksheet.Cells[1, 4].Value = "topic";
                    worksheet.Cells[1, 5].Value = "displayorder_topic";
                    worksheet.Cells[1, 6].Value = "subtopic";
                    worksheet.Cells[1, 7].Value = "displayorder_subtopic";
                    worksheet.Cells[1, 27].Value = "chaptercode"; // AA column
                    worksheet.Cells[1, 28].Value = "topiccode";   // AB column
                    worksheet.Cells[1, 29].Value = "subtopiccode"; // AC column

                    if (!contentIndexes.Any())
                    {
                        worksheet.Cells[2, 1].Value = subject;  // Use the fetched SubjectCode

                        // Add the MasterData sheet
                        var masterDataSheet1 = package.Workbook.Worksheets.Add("MasterData");

                        // Add header
                        // masterDataSheet.Cells[1, 1].Value = "SubjectId";
                        masterDataSheet1.Cells[1, 1].Value = "SubjectName";
                        masterDataSheet1.Cells[1, 2].Value = "SubjectCode";

                        // Fetch all subjects for the MasterData sheet
                        string masterDataSql1 = @"
                SELECT  [SubjectName], [SubjectCode]
                FROM [tblSubject]";

                        var masterData1 = await _connection.QueryAsync(masterDataSql1);

                        // Add rows to MasterData sheet
                        int rowIndex1 = 2;
                        foreach (var row in masterData1)
                        {
                            //  masterDataSheet.Cells[rowIndex, 1].Value = row.SubjectId;
                            masterDataSheet1.Cells[rowIndex1, 1].Value = row.SubjectName;
                            masterDataSheet1.Cells[rowIndex1, 2].Value = row.SubjectCode;
                            rowIndex1++;
                        }

                        return new ServiceResponse<byte[]>(true, "No records found, but subject code provided", package.GetAsByteArray(), StatusCodes.Status200OK);
                    }

                    var contentIndexList = contentIndexes.ToList();
                    var exportData = new List<dynamic>();

                    foreach (var contentIndex in contentIndexList)
                    {
                        // Fetch topics for each chapter
                        string topicsSql = @"
                    SELECT * 
                    FROM [tblContentIndexTopics] 
                    WHERE [ChapterCode] = @chapterCode AND [IsActive] = 1";
                        var topics = await _connection.QueryAsync<ContentIndexTopicsResponse>(topicsSql, new { chapterCode = contentIndex.ChapterCode });

                        if (!topics.Any())
                        {
                            // Add chapter-only rows
                            exportData.Add(new
                            {
                                subjectcode = subject,
                                chapter = contentIndex.ContentName_Chapter,
                                displayorder_chapter = contentIndex.DisplayOrder,
                                topic = "",
                                displayorder_topic = "",
                                subtopic = "",
                                displayorder_subtopic = "",
                                chaptercode = contentIndex.ChapterCode,
                                topiccode = "",
                                subtopiccode = ""
                            });
                        }
                        else
                        {
                            foreach (var topic in topics)
                            {
                                // Fetch subtopics for each topic
                                string subTopicsSql = @"
                            SELECT * 
                            FROM [tblContentIndexSubTopics] 
                            WHERE [TopicCode] = @topicCode AND [IsActive] = 1";
                                var subTopics = await _connection.QueryAsync<ContentIndexSubTopicResponse>(subTopicsSql, new { topicCode = topic.TopicCode });

                                if (!subTopics.Any())
                                {
                                    // Add topic-only rows
                                    exportData.Add(new
                                    {
                                        subjectcode = subject,
                                        chapter = contentIndex.ContentName_Chapter,
                                        displayorder_chapter = contentIndex.DisplayOrder,
                                        topic = topic.ContentName_Topic,
                                        displayorder_topic = topic.DisplayOrder,
                                        subtopic = "",
                                        displayorder_subtopic = "",
                                        chaptercode = contentIndex.ChapterCode,
                                        topiccode = topic.TopicCode,
                                        subtopiccode = ""
                                    });
                                }
                                else
                                {
                                    foreach (var subTopic in subTopics)
                                    {
                                        // Add rows for each subtopic
                                        exportData.Add(new
                                        {
                                            subjectcode = subject,
                                            chapter = contentIndex.ContentName_Chapter,
                                            displayorder_chapter = contentIndex.DisplayOrder,
                                            topic = topic.ContentName_Topic,
                                            displayorder_topic = topic.DisplayOrder,
                                            subtopic = subTopic.ContentName_SubTopic,
                                            displayorder_subtopic = subTopic.DisplayOrder,
                                            chaptercode = contentIndex.ChapterCode,
                                            topiccode = topic.TopicCode,
                                            subtopiccode = subTopic.SubTopicCode
                                        });
                                    }
                                }
                            }
                        }
                    }

                    // Sort the export data based on display order logic
                    exportData = exportData
                        .OrderBy(e => e.displayorder_chapter == 0 ? 1 : 0) // Prioritize non-zero chapter display orders
                        .ThenBy(e => e.displayorder_chapter) // Sort by chapter display order
                        .ThenBy(e => e.chapter) // Sort alphabetically if chapter display order is 0 or same
                        .ThenBy(e => e.displayorder_topic == 0 ? 1 : 0) // Prioritize non-zero topic display orders
                        .ThenBy(e => e.displayorder_topic) // Sort by topic display order
                        .ThenBy(e => e.topic) // Sort alphabetically if topic display order is 0 or same
                        .ThenBy(e => e.displayorder_subtopic == 0 ? 1 : 0) // Prioritize non-zero subtopic display orders
                        .ThenBy(e => e.displayorder_subtopic) // Sort by subtopic display order
                        .ThenBy(e => e.subtopic) // Sort alphabetically if subtopic display order is 0 or same
                        .ToList();

                    // Add rows if there is data
                    for (int i = 0; i < exportData.Count; i++)
                    {
                        worksheet.Cells[i + 2, 1].Value = exportData[i].subjectcode;
                        worksheet.Cells[i + 2, 2].Value = exportData[i].chapter;
                        worksheet.Cells[i + 2, 3].Value = exportData[i].displayorder_chapter;
                        worksheet.Cells[i + 2, 4].Value = exportData[i].topic;
                        worksheet.Cells[i + 2, 5].Value = exportData[i].displayorder_topic;
                        worksheet.Cells[i + 2, 6].Value = exportData[i].subtopic;
                        worksheet.Cells[i + 2, 7].Value = exportData[i].displayorder_subtopic;
                        worksheet.Cells[i + 2, 27].Value = exportData[i].chaptercode;
                        worksheet.Cells[i + 2, 28].Value = exportData[i].topiccode;
                        worksheet.Cells[i + 2, 29].Value = exportData[i].subtopiccode;
                    }

                    // Hide the code columns
                    worksheet.Column(27).Hidden = true;  // chaptercode (AA)
                    worksheet.Column(28).Hidden = true;  // topiccode (AB)
                    worksheet.Column(29).Hidden = true;  // subtopiccode (AC)

                    // Add the MasterData sheet
                    var masterDataSheet = package.Workbook.Worksheets.Add("MasterData");

                    // Add header
                   // masterDataSheet.Cells[1, 1].Value = "SubjectId";
                    masterDataSheet.Cells[1, 1].Value = "SubjectName";
                    masterDataSheet.Cells[1, 2].Value = "SubjectCode";

                    // Fetch all subjects for the MasterData sheet
                    string masterDataSql = @"
                SELECT  [SubjectName], [SubjectCode]
                FROM [tblSubject]";

                    var masterData = await _connection.QueryAsync(masterDataSql);

                    // Add rows to MasterData sheet
                    int rowIndex = 2;
                    foreach (var row in masterData)
                    {
                      //  masterDataSheet.Cells[rowIndex, 1].Value = row.SubjectId;
                        masterDataSheet.Cells[rowIndex, 1].Value = row.SubjectName;
                        masterDataSheet.Cells[rowIndex, 2].Value = row.SubjectCode;
                        rowIndex++;
                    }

                    return new ServiceResponse<byte[]>(true, "Records found", package.GetAsByteArray(), StatusCodes.Status200OK);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<byte[]>(false, ex.Message, null, StatusCodes.Status500InternalServerError);
            }
        }

        //public async Task<ServiceResponse<byte[]>> DownloadContentIndexBySubjectId(int subjectId)
        //{
        //    try
        //    {
        //        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        //        // Fetch the subject information
        //        string subjectSql = @"
        //SELECT [SubjectCode]
        //FROM [tblSubject]
        //WHERE [SubjectId] = @subjectId";
        //        var subject = await _connection.QuerySingleOrDefaultAsync<string>(subjectSql, new { subjectId });

        //        if (subject == null)
        //        {
        //            return new ServiceResponse<byte[]>(false, "Subject not found", null, StatusCodes.Status204NoContent);
        //        }

        //        // Fetch the main content index chapters
        //        string contentIndexSql = @"
        //SELECT * 
        //FROM [tblContentIndexChapters] 
        //WHERE [SubjectId] = @subjectId AND [IsActive] = 1";

        //        var contentIndexes = await _connection.QueryAsync<ContentIndexResponse>(contentIndexSql, new { subjectId });

        //        // Create an Excel sheet with headers
        //        using (var package = new ExcelPackage())
        //        {
        //            var worksheet = package.Workbook.Worksheets.Add("ContentIndex");

        //            // Add header
        //            worksheet.Cells[1, 1].Value = "subjectcode";
        //            worksheet.Cells[1, 2].Value = "chapter";
        //            worksheet.Cells[1, 3].Value = "displayorder_chapter";
        //            worksheet.Cells[1, 4].Value = "topic";
        //            worksheet.Cells[1, 5].Value = "displayorder_topic";
        //            worksheet.Cells[1, 6].Value = "subtopic";
        //            worksheet.Cells[1, 7].Value = "displayorder_subtopic";
        //            worksheet.Cells[1, 27].Value = "chaptercode"; // AA column
        //            worksheet.Cells[1, 28].Value = "topiccode";   // AB column
        //            worksheet.Cells[1, 29].Value = "subtopiccode"; // AC column

        //            if (!contentIndexes.Any())
        //            {
        //                worksheet.Cells[2, 1].Value = subject;  // Use the fetched SubjectCode
        //                return new ServiceResponse<byte[]>(true, "No records found, but subject code provided", package.GetAsByteArray(), StatusCodes.Status200OK);
        //            }

        //            var contentIndexList = contentIndexes.ToList();
        //            var exportData = new List<dynamic>();

        //            foreach (var contentIndex in contentIndexList)
        //            {
        //                // Fetch topics for each chapter
        //                string topicsSql = @"
        //        SELECT * 
        //        FROM [tblContentIndexTopics] 
        //        WHERE [ChapterCode] = @chapterCode AND [IsActive] = 1";
        //                var topics = await _connection.QueryAsync<ContentIndexTopicsResponse>(topicsSql, new { chapterCode = contentIndex.ChapterCode });

        //                if (!topics.Any())
        //                {
        //                    // Add chapter-only rows
        //                    exportData.Add(new
        //                    {
        //                        subjectcode = subject,
        //                        chapter = contentIndex.ContentName_Chapter,
        //                        displayorder_chapter = contentIndex.DisplayOrder,
        //                        topic = "",
        //                        displayorder_topic = "",
        //                        subtopic = "",
        //                        displayorder_subtopic = "",
        //                        chaptercode = contentIndex.ChapterCode,
        //                        topiccode = "",
        //                        subtopiccode = ""
        //                    });
        //                }
        //                else
        //                {
        //                    foreach (var topic in topics)
        //                    {
        //                        // Fetch subtopics for each topic
        //                        string subTopicsSql = @"
        //                SELECT * 
        //                FROM [tblContentIndexSubTopics] 
        //                WHERE [TopicCode] = @topicCode AND [IsActive] = 1";
        //                        var subTopics = await _connection.QueryAsync<ContentIndexSubTopicResponse>(subTopicsSql, new { topicCode = topic.TopicCode });

        //                        if (!subTopics.Any())
        //                        {
        //                            // Add topic-only rows
        //                            exportData.Add(new
        //                            {
        //                                subjectcode = subject,
        //                                chapter = contentIndex.ContentName_Chapter,
        //                                displayorder_chapter = contentIndex.DisplayOrder,
        //                                topic = topic.ContentName_Topic,
        //                                displayorder_topic = topic.DisplayOrder,
        //                                subtopic = "",
        //                                displayorder_subtopic = "",
        //                                chaptercode = contentIndex.ChapterCode,
        //                                topiccode = topic.TopicCode,
        //                                subtopiccode = ""
        //                            });
        //                        }
        //                        else
        //                        {
        //                            foreach (var subTopic in subTopics)
        //                            {
        //                                // Add rows for each subtopic
        //                                exportData.Add(new
        //                                {
        //                                    subjectcode = subject,
        //                                    chapter = contentIndex.ContentName_Chapter,
        //                                    displayorder_chapter = contentIndex.DisplayOrder,
        //                                    topic = topic.ContentName_Topic,
        //                                    displayorder_topic = topic.DisplayOrder,
        //                                    subtopic = subTopic.ContentName_SubTopic,
        //                                    displayorder_subtopic = subTopic.DisplayOrder,
        //                                    chaptercode = contentIndex.ChapterCode,
        //                                    topiccode = topic.TopicCode,
        //                                    subtopiccode = subTopic.SubTopicCode
        //                                });
        //                            }
        //                        }
        //                    }
        //                }
        //            }

        //            // Sort the export data based on display order logic
        //            exportData = exportData
        //                .OrderBy(e => e.displayorder_chapter == 0 ? 1 : 0) // Prioritize non-zero chapter display orders
        //                .ThenBy(e => e.displayorder_chapter) // Sort by chapter display order
        //                .ThenBy(e => e.chapter) // Sort alphabetically if chapter display order is 0 or same
        //                .ThenBy(e => e.displayorder_topic == 0 ? 1 : 0) // Prioritize non-zero topic display orders
        //                .ThenBy(e => e.displayorder_topic) // Sort by topic display order
        //                .ThenBy(e => e.topic) // Sort alphabetically if topic display order is 0 or same
        //                .ThenBy(e => e.displayorder_subtopic == 0 ? 1 : 0) // Prioritize non-zero subtopic display orders
        //                .ThenBy(e => e.displayorder_subtopic) // Sort by subtopic display order
        //                .ThenBy(e => e.subtopic) // Sort alphabetically if subtopic display order is 0 or same
        //                .ToList();

        //            // Add rows if there is data
        //            for (int i = 0; i < exportData.Count; i++)
        //            {
        //                worksheet.Cells[i + 2, 1].Value = exportData[i].subjectcode;
        //                worksheet.Cells[i + 2, 2].Value = exportData[i].chapter;
        //                worksheet.Cells[i + 2, 3].Value = exportData[i].displayorder_chapter;
        //                worksheet.Cells[i + 2, 4].Value = exportData[i].topic;
        //                worksheet.Cells[i + 2, 5].Value = exportData[i].displayorder_topic;
        //                worksheet.Cells[i + 2, 6].Value = exportData[i].subtopic;
        //                worksheet.Cells[i + 2, 7].Value = exportData[i].displayorder_subtopic;
        //                worksheet.Cells[i + 2, 27].Value = exportData[i].chaptercode;
        //                worksheet.Cells[i + 2, 28].Value = exportData[i].topiccode;
        //                worksheet.Cells[i + 2, 29].Value = exportData[i].subtopiccode;
        //            }

        //            // Hide the code columns
        //            worksheet.Column(27).Hidden = true;  // chaptercode (AA)
        //            worksheet.Column(28).Hidden = true;  // topiccode (AB)
        //            worksheet.Column(29).Hidden = true;  // subtopiccode (AC)

        //            return new ServiceResponse<byte[]>(true, "Records found", package.GetAsByteArray(), StatusCodes.Status200OK);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<byte[]>(false, ex.Message, null, StatusCodes.Status500InternalServerError);
        //    }
        //}
        public async Task<ServiceResponse<string>> UploadContentIndex(IFormFile file)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension.Rows;

                        // Step 1: Import all entries into a list
                        var entries = new List<ContentIndexEntry>();

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var subjectCode = worksheet.Cells[row, 1].Text.Trim();
                            var chapter = worksheet.Cells[row, 2].Text.Trim();
                            var displayOrderChapterText = worksheet.Cells[row, 3].Text.Trim();
                            var topic = worksheet.Cells[row, 4].Text.Trim();
                            var displayOrderTopicText = worksheet.Cells[row, 5].Text.Trim();
                            var subTopic = worksheet.Cells[row, 6].Text.Trim();
                            var displayOrderSubTopicText = worksheet.Cells[row, 7].Text.Trim();
                            var chapterCode = worksheet.Cells[row, 27].Text.Trim();
                            var topicCode = worksheet.Cells[row, 28].Text.Trim();
                            var subTopicCode = worksheet.Cells[row, 29].Text.Trim();

                            // Validate data
                            if (string.IsNullOrEmpty(subjectCode) || string.IsNullOrEmpty(chapter) ||
                                string.IsNullOrEmpty(topic) || string.IsNullOrEmpty(subTopic) ||
                                string.IsNullOrEmpty(displayOrderChapterText) ||
                                string.IsNullOrEmpty(displayOrderTopicText) ||
                                string.IsNullOrEmpty(displayOrderSubTopicText))
                            {
                                return new ServiceResponse<string>(false, $"Missing data in row {row}", null, StatusCodes.Status400BadRequest);
                            }

                            // Check if display order is a valid integer
                            if (!int.TryParse(displayOrderChapterText, out int displayOrderChapter) ||
                                !int.TryParse(displayOrderTopicText, out int displayOrderTopic) ||
                                !int.TryParse(displayOrderSubTopicText, out int displayOrderSubTopic))
                            {
                                return new ServiceResponse<string>(false, $"Invalid display order in row {row}", null, StatusCodes.Status400BadRequest);
                            }

                            // Add to entries list
                            entries.Add(new ContentIndexEntry
                            {
                                SubjectCode = subjectCode,
                                Chapter = chapter,
                                DisplayOrderChapter = displayOrderChapter,
                                Topic = topic,
                                DisplayOrderTopic = displayOrderTopic,
                                SubTopic = subTopic,
                                DisplayOrderSubTopic = displayOrderSubTopic,
                                ChapterCode = chapterCode,
                                TopicCode = topicCode,
                                SubTopicCode = subTopicCode
                            });
                        }

                        // Step 2: Remove duplicates
                        var distinctEntries = entries.GroupBy(x => new { x.Chapter, x.Topic, x.SubTopic })
                                                      .Select(g => g.First())
                                                      .ToList();

                        // Step 3: Process distinct entries
                        foreach (var entry in distinctEntries)
                        {
                            // Fetch SubjectId based on SubjectCode
                            var subjectId = await _connection.QueryFirstOrDefaultAsync<int?>(
                                "SELECT SubjectId FROM tblSubject WHERE SubjectCode = @subjectCode",
                                new { subjectCode = entry.SubjectCode });

                            if (!subjectId.HasValue)
                            {
                                return new ServiceResponse<string>(false, $"Subject code {entry.SubjectCode} does not exist", null, StatusCodes.Status400BadRequest);
                            }

                            string chapterCode = entry.ChapterCode;
                            // Check if chapter exists
                            var existingChapter = await _connection.QueryFirstOrDefaultAsync<ContentIndexRequestdto>(
                                "SELECT * FROM tblContentIndexChapters WHERE (ContentName_Chapter = @Chapter OR ChapterCode = @ChapterCode) AND IsActive = 1",
                                new { Chapter = entry.Chapter, ChapterCode = entry.ChapterCode });

                            if (existingChapter != null)
                            {
                                // Update existing chapter
                                existingChapter.ContentName_Chapter = entry.Chapter;
                                existingChapter.DisplayOrder = entry.DisplayOrderChapter;
                                var chapterResponse = await AddUpdateContentIndexChapter(existingChapter);
                                chapterCode = chapterResponse.Data; // Use updated chapter code
                            }
                            else
                            {
                                // Prepare and insert new chapter
                                var chapterRequest = new ContentIndexRequestdto
                                {
                                    SubjectId = subjectId.Value,
                                    ContentName_Chapter = entry.Chapter,
                                    Status = true,
                                    IndexTypeId = 1,
                                    BoardId = 1,
                                    ClassId = 1,
                                    CourseId = 1,
                                    APID = 1,
                                    CreatedOn = DateTime.UtcNow,
                                    CreatedBy = "Admin", // Replace with actual user
                                    ModifiedOn = DateTime.UtcNow,
                                    ModifiedBy = "Admin", // Replace with actual user
                                    EmployeeId = 1,
                                    ExamTypeId = 1,
                                    DisplayOrder = entry.DisplayOrderChapter,
                                    IsActive = true,
                                    ChapterCode = entry.ChapterCode
                                };

                                // Insert new chapter
                                var chapterResponse = await AddUpdateContentIndexChapter(chapterRequest);
                                chapterCode = chapterResponse.Data; // Use newly generated chapter code
                            }

                            // Check if topic exists
                            string topicCode = entry.TopicCode;
                            var existingTopic = await _connection.QueryFirstOrDefaultAsync<ContentIndexTopicsdto>(
                                "SELECT * FROM tblContentIndexTopics WHERE (ContentName_Topic = @Topic OR TopicCode = @TopicCode) AND ChapterCode = @ChapterCode AND IsActive = 1",
                                new { Topic = entry.Topic, TopicCode = entry.TopicCode, ChapterCode = chapterCode });

                            if (existingTopic != null)
                            {
                                // Update existing topic
                                existingTopic.ContentName_Topic = entry.Topic;
                                existingTopic.DisplayOrder = entry.DisplayOrderTopic;
                                var topicResponse = await AddUpdateContentIndexTopics(existingTopic);
                                topicCode = topicResponse.Data; // Use updated topic code
                            }
                            else
                            {
                                // Prepare and insert new topic
                                var topicRequest = new ContentIndexTopicsdto
                                {
                                    ContentIndexId = 0, // Assuming this is the returned ContentIndexId
                                    ContentName_Topic = entry.Topic,
                                    Status = true,
                                    IndexTypeId = 2,
                                    CreatedOn = DateTime.UtcNow,
                                    CreatedBy = "Admin", // Replace with actual user
                                    ModifiedOn = DateTime.UtcNow,
                                    ModifiedBy = "Admin", // Replace with actual user
                                    EmployeeId = 1,
                                    DisplayName = string.Empty,
                                    DisplayOrder = entry.DisplayOrderTopic,
                                    IsActive = true,
                                    ChapterCode = chapterCode,
                                    TopicCode = entry.TopicCode
                                };

                                // Insert new topic
                                var topicResponse = await AddUpdateContentIndexTopics(topicRequest);
                                topicCode = topicResponse.Data; // Use newly generated topic code
                            }

                            // Check if subtopic exists
                            var existingSubTopic = await _connection.QueryFirstOrDefaultAsync<ContentIndexSubTopic>(
                                "SELECT * FROM tblContentIndexSubTopics WHERE (ContentName_SubTopic = @SubTopic OR SubTopicCode = @SubTopicCode) AND TopicCode = @TopicCode AND IsActive = 1",
                                new { SubTopic = entry.SubTopic, SubTopicCode = entry.SubTopicCode, TopicCode = topicCode });

                            if (existingSubTopic != null)
                            {
                                // Update existing subtopic
                                existingSubTopic.ContentName_SubTopic = entry.SubTopic;
                                existingSubTopic.DisplayOrder = entry.DisplayOrderSubTopic;
                                await AddUpdateContentIndexSubTopics(existingSubTopic);
                            }
                            else
                            {
                                // Prepare and insert new subtopic
                                var subTopicRequest = new ContentIndexSubTopic
                                {
                                    ContInIdTopic = 0, // Assuming this is the returned ContentIndexId
                                    ContentName_SubTopic = entry.SubTopic,
                                    Status = true,
                                    IndexTypeId = 3,
                                    CreatedOn = DateTime.UtcNow,
                                    CreatedBy = "Admin", // Replace with actual user
                                    ModifiedOn = DateTime.UtcNow,
                                    ModifiedBy = "Admin", // Replace with actual user
                                    EmployeeId = 1,
                                    DisplayName = string.Empty,
                                    DisplayOrder = entry.DisplayOrderSubTopic,
                                    IsActive = true,
                                    SubTopicCode = entry.SubTopicCode,
                                    TopicCode = topicCode
                                };

                                // Insert new subtopic
                                var subTopicResponse = await AddUpdateContentIndexSubTopics(subTopicRequest);
                                if (!subTopicResponse.Success)
                                {
                                    return new ServiceResponse<string>(false, subTopicResponse.Message, null, StatusCodes.Status400BadRequest);
                                }
                            }
                        }
                    }

                    return new ServiceResponse<string>(true, "Records uploaded successfully", null, StatusCodes.Status200OK);
                }
            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // SQL error numbers for unique key violation
            {
                return new ServiceResponse<string>(false, "Data already exists.", string.Empty, StatusCodes.Status409Conflict);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, null, StatusCodes.Status500InternalServerError);
            }
        }
        private string GenerateCode()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssff");
        }
        private async Task InsertOrUpdateContentIndexTopics(string chapterCode, List<ContentIndexTopics>? topics)
        {
            if (topics != null)
            {
                foreach (var topic in topics)
                {
                    if (topic.ContInIdTopic == 0)
                    {
                        // Insert new topic
                        string insertTopicQuery = @"
                    INSERT INTO tblContentIndexTopics (ContentIndexId, ContentName_Topic, Status, IndexTypeId, CreatedOn, CreatedBy, EmployeeId, TopicCode, DisplayName, DisplayOrder, IsActive, ChapterCode)
                    VALUES (@ContentIndexId, @ContentName_Topic, @Status, @IndexTypeId, @CreatedOn, @CreatedBy, @EmployeeId, @TopicCode, @DisplayName, @DisplayOrder, @IsActive, @ChapterCode);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                        string newTopicCode = GenerateCode();

                        await _connection.ExecuteAsync(insertTopicQuery, new
                        {
                            ContentIndexId = await GetContentIndexIdByChapterCode(chapterCode),
                            ChapterCode = chapterCode,
                            topic.ContentName_Topic,
                            topic.Status,
                            topic.IndexTypeId,
                            CreatedOn = DateTime.Now,
                            topic.CreatedBy,
                            topic.EmployeeId,
                            TopicCode = newTopicCode,
                            topic.DisplayName,
                            topic.DisplayOrder,
                            topic.IsActive,
                        });

                        await InsertOrUpdateContentIndexSubTopics(newTopicCode, topic.ContentIndexSubTopics);
                    }
                    else
                    {
                        // Update existing topic
                        string updateTopicQuery = @"
                    UPDATE tblContentIndexTopics SET
                        ContentName_Topic = @ContentName_Topic,
                        Status = @Status,
                        IndexTypeId = @IndexTypeId,
                        ModifiedOn = @ModifiedOn,
                        ModifiedBy = @ModifiedBy,
                        EmployeeId = @EmployeeId,
                        DisplayName = @DisplayName,
                        DisplayOrder = @DisplayOrder,
                        IsActive = @IsActive,
                        ContentIndexId = @ContentIndexId
                    WHERE TopicCode = @TopicCode AND IsActive = 1";

                        await _connection.ExecuteAsync(updateTopicQuery, new
                        {
                            topic.ContentName_Topic,
                            topic.Status,
                            topic.IndexTypeId,
                            ModifiedOn = DateTime.Now,
                            topic.ModifiedBy,
                            topic.EmployeeId,
                            topic.DisplayName,
                            topic.DisplayOrder,
                            topic.IsActive,
                            topic.TopicCode,
                            topic.ContentIndexId
                        });

                        await InsertOrUpdateContentIndexSubTopics(topic.TopicCode, topic.ContentIndexSubTopics);
                    }
                }
            }
        }
        private async Task InsertOrUpdateContentIndexSubTopics(string topicCode, List<ContentIndexSubTopic>? subTopics)
        {
            if (subTopics != null)
            {
                foreach (var subTopic in subTopics)
                {
                    if (subTopic.ContInIdSubTopic == 0)
                    {
                        // Insert new subtopic
                        string insertSubTopicQuery = @"
                    INSERT INTO tblContentIndexSubTopics (ContInIdTopic, ContentName_SubTopic, Status, IndexTypeId, CreatedOn, CreatedBy, EmployeeId, SubTopicCode, DisplayName, DisplayOrder, IsActive, TopicCode)
                    VALUES (@ContInIdTopic, @ContentName_SubTopic, @Status, @IndexTypeId, @CreatedOn, @CreatedBy, @EmployeeId, @SubTopicCode, @DisplayName, @DisplayOrder, @IsActive, @TopicCode);";

                        await _connection.ExecuteAsync(insertSubTopicQuery, new
                        {
                            TopicCode = topicCode,
                            subTopic.ContentName_SubTopic,
                            subTopic.Status,
                            subTopic.IndexTypeId,
                            CreatedOn = DateTime.Now,
                            subTopic.CreatedBy,
                            subTopic.EmployeeId,
                            SubTopicCode = GenerateCode(),
                            subTopic.DisplayName,
                            subTopic.DisplayOrder,
                            subTopic.IsActive,
                            ContInIdTopic = await GetContInIdTopicByTopicCode(topicCode)
                        });
                    }
                    else
                    {
                        // Update existing subtopic
                        string updateSubTopicQuery = @"
                    UPDATE tblContentIndexSubTopics SET
                        ContentName_SubTopic = @ContentName_SubTopic,
                        Status = @Status,
                        IndexTypeId = @IndexTypeId,
                        ModifiedOn = @ModifiedOn,
                        ModifiedBy = @ModifiedBy,
                        EmployeeId = @EmployeeId,
                        DisplayName = @DisplayName,
                        DisplayOrder = @DisplayOrder,
                        IsActive = @IsActive,
                        ContInIdTopic = @ContInIdTopic
                    WHERE SubTopicCode = @SubTopicCode AND IsActive = 1";

                        await _connection.ExecuteAsync(updateSubTopicQuery, new
                        {
                            subTopic.ContentName_SubTopic,
                            subTopic.Status,
                            subTopic.IndexTypeId,
                            ModifiedOn = DateTime.Now,
                            subTopic.ModifiedBy,
                            subTopic.EmployeeId,
                            subTopic.DisplayName,
                            subTopic.DisplayOrder,
                            subTopic.IsActive,
                            subTopic.SubTopicCode,
                            ContInIdTopic = await GetContInIdTopicByTopicCode(topicCode)
                        });
                    }
                }
            }
        }
        private int? GetSubjectIdByCode(string subjectCode)
        {
            try
            {
                // Query to fetch the subject ID based on subject code
                const string query = "SELECT [SubjectId] FROM [tblSubject] WHERE [SubjectCode] = @SubjectCode";

                // Execute the query
                var subjectId =  _connection.QuerySingleOrDefault<int?>(query, new { SubjectCode = subjectCode });

                if (subjectId == null)
                {
                    throw new Exception($"No subject found with code {subjectCode}");
                }

                return subjectId;
            }
            catch (Exception ex)
            {
                // Handle exceptions as needed
                throw new Exception($"Error fetching subject ID: {ex.Message}", ex);
            }
        }
        private async Task<int> GetContentIndexIdByChapterCode(string chapterCode)
        {
            string query = "SELECT ContentIndexId FROM tblContentIndexChapters WHERE ChapterCode = @ChapterCode AND IsActive = 1";
            return await _connection.QuerySingleOrDefaultAsync<int>(query, new { ChapterCode = chapterCode });
        }
        private async Task<int> GetContInIdTopicByTopicCode(string topicCode)
        {
            string query = "SELECT ContInIdTopic FROM tblContentIndexTopics WHERE TopicCode = @TopicCode AND IsActive = 1";
            return await _connection.QuerySingleOrDefaultAsync<int>(query, new { TopicCode = topicCode });
        }
    }
}
