using Config_API.DTOs.ServiceResponse;
using Config_API.Models;
using Config_API.Repository.Interfaces;
using System.Data;
using Dapper;
using Config_API.DTOs.Requests;

namespace Config_API.Repository.Implementations
{
    public class ContentIndexRepository : IContentIndexRepository
    {

        private readonly IDbConnection _connection;

        public ContentIndexRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<ServiceResponse<List<ContentIndexRequest>>> GetAllContentIndexList(ContentIndexListDTO request)
        {
            try
            {

                string countSql = @"SELECT COUNT(*) FROM [tblContentIndexChapters]";
                int totalCount = await _connection.ExecuteScalarAsync<int>(countSql);
                // Base query to fetch the content indexes
                string query = @"SELECT * FROM [tblContentIndexChapters] WHERE 1 = 1";

                // Add filters based on DTO properties
                if (request.APID > 0)
                {
                    query += " AND [APID] = @APID";
                }
                if (request.SubjectId > 0)
                {
                    query += " AND [SubjectId] = @SubjectId";
                }

                // Apply pagination
                int offset = (request.PageNumber - 1) * request.PageSize;
                query += " ORDER BY [ContentIndexId] OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                var contentIndexes = await _connection.QueryAsync<ContentIndexRequest>(query, new { request.APID, request.SubjectId, Offset = offset, PageSize = request.PageSize });

                if (contentIndexes.Any())
                {
                    // Fetch related topics and subtopics for each content index
                    foreach (var contentIndex in contentIndexes)
                    {
                        string topicsSql = @"SELECT * FROM [tblContentIndexTopics] WHERE [ContentIndexId] = @contentIndexId";
                        var topics = (await _connection.QueryAsync<ContentIndexTopics>(topicsSql, new { contentIndexId = contentIndex.ContentIndexId })).ToList();

                        string subTopicsSql = @"
                    SELECT st.*
                    FROM [tblContentIndexSubTopics] st
                    INNER JOIN [tblContentIndexTopics] t ON st.ContInIdTopic = t.ContInIdTopic
                    WHERE t.ContentIndexId = @contentIndexId";
                        var subTopics = (await _connection.QueryAsync<ContentIndexSubTopic>(subTopicsSql, new { contentIndexId = contentIndex.ContentIndexId })).ToList();

                        // Assign the subtopics to the respective topics
                        foreach (var topic in topics)
                        {
                            topic.ContentIndexSubTopics = subTopics.Where(st => st.ContInIdTopic == topic.ContInIdTopic).ToList();
                        }

                        // Assign the topics to the content index
                        contentIndex.ContentIndexTopics = topics;
                    }

                    return new ServiceResponse<List<ContentIndexRequest>>(true, "Records found", contentIndexes.AsList(), StatusCodes.Status302Found, totalCount);
                }
                else
                {
                    return new ServiceResponse<List<ContentIndexRequest>>(false, "Records not found", [], StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ContentIndexRequest>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<ContentIndexRequest>> GetContentIndexById(int id)
        {
            try
            {
                // Fetch the main content index
                string contentIndexSql = @"SELECT * FROM [tblContentIndexChapters] WHERE [ContentIndexId] = @id";
                var contentIndex = await _connection.QueryFirstOrDefaultAsync<ContentIndexChapters>(contentIndexSql, new { id });

                if (contentIndex == null)
                {
                    return new ServiceResponse<ContentIndexRequest>(false, "Records Not Found", new ContentIndexRequest(), StatusCodes.Status204NoContent);
                }

                // Fetch the related topics
                string topicsSql = @"SELECT * FROM [tblContentIndexTopics] WHERE [ContentIndexId] = @contentIndexId";
                var topics = await _connection.QueryAsync<ContentIndexTopics>(topicsSql, new { contentIndexId = contentIndex.ContentIndexId });

                // Fetch the related subtopics
                string subTopicsSql = @"
            SELECT st.*
            FROM [tblContentIndexSubTopics] st
            INNER JOIN [tblContentIndexTopics] t ON st.ContInIdTopic = t.ContInIdTopic
            WHERE t.ContentIndexId = @contentIndexId";
                var subTopics = await _connection.QueryAsync<ContentIndexSubTopic>(subTopicsSql, new { contentIndexId = contentIndex.ContentIndexId });

                // Create a ContentIndexRequest object and assign the fetched data
                var contentIndexRequest = new ContentIndexRequest
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
                    ContentIndexTopics = topics.ToList()
                };

                // Assign the subtopics to the respective topics
                foreach (var topic in contentIndexRequest.ContentIndexTopics)
                {
                    topic.ContentIndexSubTopics = subTopics.Where(st => st.ContInIdTopic == topic.ContInIdTopic).ToList();
                }

                return new ServiceResponse<ContentIndexRequest>(true, "Records Found", contentIndexRequest, StatusCodes.Status302Found);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ContentIndexRequest>(false, ex.Message, new ContentIndexRequest(), StatusCodes.Status500InternalServerError);
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
                INSERT INTO tblContentIndexChapters (SubjectId, ContentName_Chapter, IndexTypeId, Status, ClassId, BoardId, APID, CreatedOn, CreatedBy, CourseId, EmployeeId, ExamTypeId)
                VALUES (@SubjectId, @ContentName_Chapter, @IndexTypeId, @Status, @ClassId, @BoardId, @APID, @CreatedOn, @CreatedBy, @CourseId, @EmployeeId, @ExamTypeId);
                SELECT CAST(SCOPE_IDENTITY() as int);";

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
                        request.ExamTypeId
                    }, transaction);

                    if (insertedId > 0)
                    {
                        await InsertOrUpdateContentIndexTopics(insertedId, request.ContentIndexTopics, transaction);
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
                    ExamTypeId = @ExamTypeId
                WHERE ContentIndexId = @ContentIndexId";

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
                        request.ExamTypeId
                    }, transaction);

                    if (rowsAffected > 0)
                    {
                        await InsertOrUpdateContentIndexTopics(request.ContentIndexId, request.ContentIndexTopics, transaction);
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
        private async Task InsertOrUpdateContentIndexTopics(int contentIndexId, List<ContentIndexTopics>? topics, IDbTransaction transaction)
        {
            if (topics != null)
            {
                foreach (var topic in topics)
                {
                    if (topic.ContInIdTopic == 0)
                    {
                        // Insert new topic
                        string insertTopicQuery = @"
                    INSERT INTO tblContentIndexTopics (ContentIndexId, ContentName_Topic, Status, IndexTypeId, CreatedOn, CreatedBy, EmployeeId)
                    VALUES (@ContentIndexId, @ContentName_Topic, @Status, @IndexTypeId, @CreatedOn, @CreatedBy, @EmployeeId);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                        int topicId = await _connection.QuerySingleAsync<int>(insertTopicQuery, new
                        {
                            ContentIndexId = contentIndexId,
                            topic.ContentName_Topic,
                            topic.Status,
                            topic.IndexTypeId,
                            CreatedOn = DateTime.Now,
                            topic.CreatedBy,
                            topic.EmployeeId
                        }, transaction);

                        await InsertOrUpdateContentIndexSubTopics(topicId, topic.ContentIndexSubTopics, transaction);
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
                        EmployeeId = @EmployeeId
                    WHERE ContInIdTopic = @ContInIdTopic";

                        await _connection.ExecuteAsync(updateTopicQuery, new
                        {
                            topic.ContInIdTopic,
                            topic.ContentName_Topic,
                            topic.Status,
                            topic.IndexTypeId,
                            ModifiedOn = DateTime.Now,
                            topic.ModifiedBy,
                            topic.EmployeeId
                        }, transaction);

                        await InsertOrUpdateContentIndexSubTopics(topic.ContInIdTopic, topic.ContentIndexSubTopics, transaction);
                    }
                }
            }
        }
        private async Task InsertOrUpdateContentIndexSubTopics(int topicId, List<ContentIndexSubTopic>? subTopics, IDbTransaction transaction)
        {
            if (subTopics != null)
            {
                foreach (var subTopic in subTopics)
                {
                    if (subTopic.ContInIdSubTopic == 0)
                    {
                        // Insert new subtopic
                        string insertSubTopicQuery = @"
                    INSERT INTO tblContentIndexSubTopics (ContInIdTopic, ContentName_SubTopic, Status, IndexTypeId, CreatedOn, CreatedBy, EmployeeId)
                    VALUES (@ContInIdTopic, @ContentName_SubTopic, @Status, @IndexTypeId, @CreatedOn, @CreatedBy, @EmployeeId);";

                        await _connection.ExecuteAsync(insertSubTopicQuery, new
                        {
                            ContInIdTopic = topicId,
                            subTopic.ContentName_SubTopic,
                            subTopic.Status,
                            subTopic.IndexTypeId,
                            CreatedOn = DateTime.Now,
                            subTopic.CreatedBy,
                            subTopic.EmployeeId
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
                        EmployeeId = @EmployeeId
                    WHERE ContInIdSubTopic = @ContInIdSubTopic";

                        await _connection.ExecuteAsync(updateSubTopicQuery, new
                        {
                            subTopic.ContInIdSubTopic,
                            subTopic.ContentName_SubTopic,
                            subTopic.Status,
                            subTopic.IndexTypeId,
                            ModifiedOn = DateTime.Now,
                            subTopic.ModifiedBy,
                            subTopic.EmployeeId
                        }, transaction);
                    }
                }
            }
        }
    }
}