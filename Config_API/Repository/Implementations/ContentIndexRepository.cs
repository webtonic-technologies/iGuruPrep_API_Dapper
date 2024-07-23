using Config_API.DTOs.Requests;
using Config_API.DTOs.Response;
using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
using Config_API.Repository.Interfaces;
using Dapper;
using iGuruPrep.Models;
using OfficeOpenXml;
using System.Data;

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
                                .OrderBy(subTopic => subTopic.DisplayOrder)
                                .ThenBy(subTopic => subTopic.DisplayName)
                                .ToList()
                            })
                            .OrderBy(topic => topic.DisplayOrder)
                            .ThenBy(topic => topic.DisplayName)
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
                string topicsSql = @"SELECT * FROM [tblContentIndexTopics] WHERE [ChapterCode] = @chapterCode AND [IsActive] = 1";
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
                                TopicCode = st.TopicCode,
                            })
                            .OrderBy(st => st.DisplayOrder)
                            .ThenBy(st => st.DisplayName)
                            .ToList()
                    })
                    .OrderBy(topic => topic.DisplayOrder)
                    .ThenBy(topic => topic.DisplayName)
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

            using var transaction = _connection.BeginTransaction();
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
                    }, transaction);

                    if (insertedId > 0)
                    {
                        await InsertOrUpdateContentIndexTopics(newChapterCode, request.ContentIndexTopics, transaction);
                        transaction.Commit();
                        return new ServiceResponse<string>(true, "Operation Successful", "Content Index Added Successfully", StatusCodes.Status201Created);
                    }
                    else
                    {
                        transaction.Rollback();
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
                    }, transaction);

                    if (rowsAffected > 0)
                    {
                        string chapterCode = await _connection.QuerySingleOrDefaultAsync<string>(request.ChapterCode, new { request.ContentIndexId }, transaction);

                        await InsertOrUpdateContentIndexTopics(chapterCode, request.ContentIndexTopics, transaction);
                        transaction.Commit();
                        return new ServiceResponse<string>(true, "Operation Successful", "Content Index Updated Successfully", StatusCodes.Status200OK);
                    }
                    else
                    {
                        transaction.Rollback();
                        return new ServiceResponse<string>(false, "Operation Failed", string.Empty, StatusCodes.Status404NotFound);
                    }
                }
            }
            catch (Exception ex)
            {
                transaction.Rollback();
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
                query += " ORDER BY [DisplayOrder]";

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
                    ORDER BY [DisplayOrder]";
                        var topics = (await _connection.QueryAsync<ContentIndexTopicsResponse>(topicsSql, new { contentIndexId = contentIndex.ContentIndexId })).ToList();

                        string subTopicsSql = @"
                    SELECT st.* 
                    FROM [tblContentIndexSubTopics] st
                    INNER JOIN [tblContentIndexTopics] t ON st.ContInIdTopic = t.ContInIdTopic
                    WHERE t.ContentIndexId = @contentIndexId AND st.[IsActive] = 1
                    ORDER BY st.[DisplayOrder]";
                        var subTopics = (await _connection.QueryAsync<ContentIndexSubTopicResponse>(subTopicsSql, new { contentIndexId = contentIndex.ContentIndexId })).ToList();

                        // Assign subtopics to topics
                        foreach (var topic in topics)
                        {
                            topic.ContentIndexSubTopics = subTopics
                                .Where(st => st.ContInIdTopic == topic.ContInIdTopic)
                                .OrderBy(st => st.DisplayOrder)
                                .ThenBy(st => st.DisplayName)
                                .ToList();
                        }

                        // Assign topics to content index
                        contentIndex.ContentIndexTopics = topics
                            .OrderBy(topic => topic.DisplayOrder)
                            .ThenBy(topic => topic.DisplayName)
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
        WHERE [SubjectId] = @subjectId AND [IsActive] = 1 
        ORDER BY [DisplayOrder]";
                var contentIndexes = await _connection.QueryAsync<ContentIndexResponse>(contentIndexSql, new { subjectId });

                if (!contentIndexes.Any())
                {
                    return new ServiceResponse<byte[]>(false, "No records found", null, StatusCodes.Status204NoContent);
                }

                var contentIndexList = contentIndexes.ToList();
                var exportData = new List<dynamic>();

                foreach (var contentIndex in contentIndexList)
                {
                    // Fetch topics for each chapter
                    string topicsSql = @"
            SELECT * 
            FROM [tblContentIndexTopics] 
            WHERE [ChapterCode] = @chapterCode AND [IsActive] = 1 
            ORDER BY [DisplayOrder]";
                    var topics = await _connection.QueryAsync<ContentIndexTopicsResponse>(topicsSql, new { chapterCode = contentIndex.ChapterCode });

                    if (!topics.Any())
                    {
                        // Add chapter-only rows
                        exportData.Add(new
                        {
                            subjectcode = subject,  // Use the fetched SubjectCode
                            chapter = string.IsNullOrEmpty(contentIndex.DisplayName) ? contentIndex.ContentName_Chapter : contentIndex.DisplayName,
                            topic = "",
                            subtopic = "",
                            displayorder = ""
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
                    WHERE [TopicCode] = @topicCode AND [IsActive] = 1 
                    ORDER BY [DisplayOrder]";
                            var subTopics = await _connection.QueryAsync<ContentIndexSubTopicResponse>(subTopicsSql, new { topicCode = topic.TopicCode });

                            if (!subTopics.Any())
                            {
                                // Add topic-only rows
                                exportData.Add(new
                                {
                                    subjectcode = subject,  // Use the fetched SubjectCode
                                    chapter = string.IsNullOrEmpty(contentIndex.DisplayName) ? contentIndex.ContentName_Chapter : contentIndex.DisplayName,
                                    topic = string.IsNullOrEmpty(topic.DisplayName) ? topic.ContentName_Topic : topic.DisplayName,
                                    subtopic = "",
                                    displayorder = ""
                                });
                            }
                            else
                            {
                                foreach (var subTopic in subTopics)
                                {
                                    // Add rows for each subtopic
                                    var row = new
                                    {
                                        subjectcode = subject,  // Use the fetched SubjectCode
                                        chapter = string.IsNullOrEmpty(contentIndex.DisplayName) ? contentIndex.ContentName_Chapter : contentIndex.DisplayName,
                                        topic = string.IsNullOrEmpty(topic.DisplayName) ? topic.ContentName_Topic : topic.DisplayName,
                                        subtopic = string.IsNullOrEmpty(subTopic.DisplayName) ? subTopic.ContentName_SubTopic : subTopic.DisplayName,
                                        displayorder = subTopic.DisplayOrder
                                    };
                                    exportData.Add(row);
                                }
                            }
                        }
                    }
                }

                // Convert to Excel
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("ContentIndex");

                    // Add header
                    worksheet.Cells[1, 1].Value = "subjectcode";
                    worksheet.Cells[1, 2].Value = "chapter";
                    worksheet.Cells[1, 3].Value = "topic";
                    worksheet.Cells[1, 4].Value = "subtopic";
                    worksheet.Cells[1, 5].Value = "displayorder";

                    // Add rows
                    for (int i = 0; i < exportData.Count; i++)
                    {
                        worksheet.Cells[i + 2, 1].Value = exportData[i].subjectcode;
                        worksheet.Cells[i + 2, 2].Value = exportData[i].chapter;
                        worksheet.Cells[i + 2, 3].Value = exportData[i].topic;
                        worksheet.Cells[i + 2, 4].Value = exportData[i].subtopic;
                        worksheet.Cells[i + 2, 5].Value = exportData[i].displayorder;
                    }

                    return new ServiceResponse<byte[]>(true, "Records found", package.GetAsByteArray(), StatusCodes.Status200OK);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<byte[]>(false, ex.Message, null, StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<string>> UploadContentIndex(IFormFile file)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                var contentIndexes = new List<ContentIndexSheet>();
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension.Rows;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            var subjectCode = worksheet.Cells[row, 1].Text.Trim();
                            var chapter = worksheet.Cells[row, 2].Text.Trim();
                            var topic = worksheet.Cells[row, 3].Text.Trim();
                            var subTopic = worksheet.Cells[row, 4].Text.Trim();
                            var displayOrderText = worksheet.Cells[row, 5].Text.Trim();

                            var subjectCode1 = worksheet.Column(1).Hidden;

                            // Validate data
                            if (string.IsNullOrEmpty(subjectCode) || string.IsNullOrEmpty(chapter) || string.IsNullOrEmpty(topic) || string.IsNullOrEmpty(subTopic) || string.IsNullOrEmpty(displayOrderText))
                            {
                                return new ServiceResponse<string>(false, $"Missing data in row {row}", null, StatusCodes.Status400BadRequest);
                            }

                            // Check if display order is a valid integer
                            if (!int.TryParse(displayOrderText, out int displayOrder))
                            {
                                return new ServiceResponse<string>(false, $"Invalid display order in row {row}", null, StatusCodes.Status400BadRequest);
                            }

                            // Check if subject exists
                            var subject = await _connection.QuerySingleOrDefaultAsync<Subject>("SELECT * FROM tblSubject WHERE SubjectCode = @subjectCode", new { subjectCode });
                            if (subject == null)
                            {
                                return new ServiceResponse<string>(false, $"Subject code {subjectCode} does not exist in row {row}", null, StatusCodes.Status400BadRequest);
                            }

                            // Process each row and add to the list
                            contentIndexes.Add(new ContentIndexSheet
                            {
                                SubjectCode = subjectCode,
                                Chapter = chapter,
                                Topic = topic,
                                SubTopic = subTopic,
                                DisplayOrder = displayOrder
                            });
                        }
                    }
                }

                // Insert/Update database
                foreach (var contentIndex in contentIndexes)
                {
                    // Fetch existing chapters for the subject
                    var existingChapters = await _connection.QueryAsync<ContentIndexResponse>(
                        "SELECT * FROM [tblContentIndexChapters] WHERE [SubjectId] = @SubjectId AND [IsActive] = 1",
                        new { SubjectId = GetSubjectIdByCode(contentIndex.SubjectCode) });

                    var existingChapter = existingChapters.FirstOrDefault(ch => ch.ContentName_Chapter == contentIndex.Chapter);

                    if (existingChapter != null)
                    {
                        // Update chapter
                        var updateChapterSql = @"UPDATE [tblContentIndexChapters] SET [ContentName_Chapter] = @ContentName_Chapter, [DisplayOrder] = @DisplayOrder, [ModifiedOn] = GETDATE() WHERE [ChapterCode] = @ChapterCode";
                        await _connection.ExecuteAsync(updateChapterSql, new { ContentName_Chapter = contentIndex.Chapter, DisplayOrder = contentIndex.DisplayOrder, ChapterCode = existingChapter.ChapterCode });
                    }
                    else
                    {
                        // Insert new chapter
                        var insertChapterSql = @"INSERT INTO [tblContentIndexChapters] ([SubjectId], [ContentName_Chapter], [ChapterCode], [DisplayOrder], [IsActive]) VALUES (@SubjectId, @ContentName_Chapter, @ChapterCode, @DisplayOrder, 1)";
                        await _connection.ExecuteAsync(insertChapterSql, new { SubjectId = GetSubjectIdByCode(contentIndex.SubjectCode), ContentName_Chapter = contentIndex.Chapter, ChapterCode = contentIndex.Chapter, DisplayOrder = contentIndex.DisplayOrder });

                        // Fetch the newly inserted chapter to get its ID
                        existingChapter = await _connection.QuerySingleOrDefaultAsync<ContentIndexResponse>(
                            "SELECT * FROM [tblContentIndexChapters] WHERE [ChapterCode] = @ChapterCode",
                            new { contentIndex.Chapter });
                    }

                    // Fetch existing topics for the chapter
                    var existingTopics = await _connection.QueryAsync<ContentIndexTopicsResponse>(
                        "SELECT * FROM [tblContentIndexTopics] WHERE [ContentIndexId] = @ContentIndexId AND [IsActive] = 1",
                        new { ContentIndexId = existingChapter.ContentIndexId });

                    var existingTopic = existingTopics.FirstOrDefault(t => t.ContentName_Topic == contentIndex.Topic);

                    if (existingTopic != null)
                    {
                        // Update topic
                        var updateTopicSql = @"UPDATE [tblContentIndexTopics] SET [ContentName_Topic] = @ContentName_Topic, [DisplayOrder] = @DisplayOrder, [ModifiedOn] = GETDATE() WHERE [TopicCode] = @TopicCode";
                        await _connection.ExecuteAsync(updateTopicSql, new { ContentName_Topic = contentIndex.Topic, DisplayOrder = contentIndex.DisplayOrder, TopicCode = existingTopic.TopicCode });
                    }
                    else
                    {
                        // Insert new topic
                        var insertTopicSql = @"INSERT INTO [tblContentIndexTopics] ([ContentIndexId], [ContentName_Topic], [TopicCode], [DisplayOrder], [IsActive]) VALUES (@ContentIndexId, @ContentName_Topic, @TopicCode, @DisplayOrder, 1)";
                        await _connection.ExecuteAsync(insertTopicSql, new { ContentIndexId = existingChapter.ContentIndexId, ContentName_Topic = contentIndex.Topic, TopicCode = contentIndex.Topic, DisplayOrder = contentIndex.DisplayOrder });

                        // Fetch the newly inserted topic to get its ID
                        existingTopic = await _connection.QuerySingleOrDefaultAsync<ContentIndexTopicsResponse>(
                            "SELECT * FROM [tblContentIndexTopics] WHERE [TopicCode] = @TopicCode",
                            new { contentIndex.Topic });
                    }

                    // Fetch existing subtopics for the topic
                    var existingSubTopics = await _connection.QueryAsync<ContentIndexSubTopicResponse>(
                        "SELECT * FROM [tblContentIndexSubTopics] WHERE [ContInIdTopic] = @ContInIdTopic AND [IsActive] = 1",
                        new { ContInIdTopic = existingTopic.ContInIdTopic });

                    var existingSubTopic = existingSubTopics.FirstOrDefault(st => st.ContentName_SubTopic == contentIndex.SubTopic);

                    if (existingSubTopic != null)
                    {
                        // Update subtopic
                        var updateSubTopicSql = @"UPDATE [tblContentIndexSubTopics] SET [ContentName_SubTopic] = @ContentName_SubTopic, [DisplayOrder] = @DisplayOrder, [ModifiedOn] = GETDATE() WHERE [SubTopicCode] = @SubTopicCode";
                        await _connection.ExecuteAsync(updateSubTopicSql, new { ContentName_SubTopic = contentIndex.SubTopic, DisplayOrder = contentIndex.DisplayOrder, SubTopicCode = existingSubTopic.SubTopicCode });
                    }
                    else
                    {
                        // Insert new subtopic
                        var insertSubTopicSql = @"INSERT INTO [tblContentIndexSubTopics] ([ContInIdTopic], [ContentName_SubTopic], [SubTopicCode], [DisplayOrder], [IsActive]) VALUES (@ContInIdTopic, @ContentName_SubTopic, @SubTopicCode, @DisplayOrder, 1)";
                        await _connection.ExecuteAsync(insertSubTopicSql, new { ContInIdTopic = existingTopic.ContInIdTopic, ContentName_SubTopic = contentIndex.SubTopic, SubTopicCode = contentIndex.SubTopic, DisplayOrder = contentIndex.DisplayOrder });
                    }
                }

                return new ServiceResponse<string>(true, "Records uploaded successfully", null, StatusCodes.Status200OK);
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
        private async Task InsertOrUpdateContentIndexTopics(string chapterCode, List<ContentIndexTopics>? topics, IDbTransaction transaction)
        {
            if (topics != null)
            {
                foreach (var topic in topics)
                {
                    if (topic.ContInIdTopic == 0)
                    {
                        // Insert new topic
                        string insertTopicQuery = @"
                INSERT INTO tblContentIndexTopics (ContentName_Topic, Status, IndexTypeId, CreatedOn, CreatedBy, EmployeeId, TopicCode, DisplayName, DisplayOrder, IsActive, ChapterCode)
                VALUES (@ContentName_Topic, @Status, @IndexTypeId, @CreatedOn, @CreatedBy, @EmployeeId, @TopicCode, @DisplayName, @DisplayOrder, @IsActive, @ChapterCode);
                SELECT CAST(SCOPE_IDENTITY() as int);";

                        string newTopicCode = GenerateCode();

                        await _connection.ExecuteAsync(insertTopicQuery, new
                        {
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
                            topic.IsActive
                        }, transaction);

                        await InsertOrUpdateContentIndexSubTopics(newTopicCode, topic.ContentIndexSubTopics, transaction);
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
                    IsActive = @IsActive
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
                            topic.TopicCode
                        }, transaction);

                        await InsertOrUpdateContentIndexSubTopics(topic.TopicCode, topic.ContentIndexSubTopics, transaction);
                    }
                }
            }
        }
        private async Task InsertOrUpdateContentIndexSubTopics(string topicCode, List<ContentIndexSubTopic>? subTopics, IDbTransaction transaction)
        {
            if (subTopics != null)
            {
                foreach (var subTopic in subTopics)
                {
                    if (subTopic.ContInIdSubTopic == 0)
                    {
                        // Insert new subtopic
                        string insertSubTopicQuery = @"
                INSERT INTO tblContentIndexSubTopics (ContentName_SubTopic, Status, IndexTypeId, CreatedOn, CreatedBy, EmployeeId, SubTopicCode, DisplayName, DisplayOrder, IsActive, TopicCode)
                VALUES (@ContentName_SubTopic, @Status, @IndexTypeId, @CreatedOn, @CreatedBy, @EmployeeId, @SubTopicCode, @DisplayName, @DisplayOrder, @IsActive, @TopicCode);";

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
                            subTopic.IsActive
                        }, transaction);
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
                    IsActive = @IsActive
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
                            subTopic.IsActive
                        }, transaction);
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
        public async Task<int?> GetContentIndexIdByChapterCode(string chapterCode)
        {
            string query = @"
            SELECT TOP 1 ContentIndexId
            FROM tblContentIndexChapters
            WHERE ChapterCode = @ChapterCode AND IsActive = 1";

            return await _connection.QueryFirstOrDefaultAsync<int?>(query, new { ChapterCode = chapterCode });
        }
        public async Task<int?> GetContInIdTopicByTopicCode(string topicCode)
        {
            string query = @"
            SELECT TOP 1 ContInIdTopic
            FROM tblContentIndexTopics
            WHERE TopicCode = @TopicCode AND IsActive = 1";

            return await _connection.QueryFirstOrDefaultAsync<int?>(query, new { TopicCode = topicCode });
        }

    }
}
