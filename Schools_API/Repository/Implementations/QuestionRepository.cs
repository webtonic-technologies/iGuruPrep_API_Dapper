using Dapper;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Schools_API.DTOs.Requests;
using Schools_API.DTOs.Response;
using Schools_API.DTOs.ServiceResponse;
using Schools_API.Models;
using Schools_API.Repository.Interfaces;
using System.Data;
using System.Drawing;

namespace Schools_API.Repository.Implementations
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly IDbConnection _connection;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public QuestionRepository(IDbConnection connection, IWebHostEnvironment hostingEnvironment)
        {
            _connection = connection;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<ServiceResponse<string>> AddUpdateQuestion(QuestionDTO request)
        {
            try
            {
                // Generate QuestionCode if not present
                if (string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string")
                {
                    request.QuestionCode = GenerateCode();
                }

                // Check for existing entries with the same QuestionCode and deactivate them
                string deactivateQuery = @"
                UPDATE tblQuestion
                SET IsActive = 0
                WHERE QuestionCode = @QuestionCode AND IsActive = 1";

                await _connection.ExecuteAsync(deactivateQuery, new { request.QuestionCode });

                // Prepare new question entry
                var question = new Question
                {
                    QuestionDescription = request.QuestionDescription,
                    QuestionTypeId = request.QuestionTypeId,
                    Status = true,
                    CreatedBy = request.CreatedBy,
                    CreatedOn = DateTime.Now,
                    subjectID = request.subjectID,
                    ContentIndexId = request.ContentIndexId,
                    EmployeeId = request.EmployeeId,
                    IndexTypeId = request.IndexTypeId,
                    IsApproved = false,
                    IsRejected = false,
                    QuestionCode = request.QuestionCode,
                    Explanation = request.Explanation,
                    ExtraInformation = request.ExtraInformation,
                    IsActive = true
                };

                string insertQuery = @"
                INSERT INTO tblQuestion (
                    QuestionDescription,
                    QuestionTypeId,
                    Status,
                    CreatedBy,
                    CreatedOn,
                    subjectID,
                    EmployeeId,
                    IndexTypeId,
                    ContentIndexId,
                    IsRejected,
                    IsApproved,
                    QuestionCode,
                    Explanation,
                    ExtraInformation,
                    IsActive
                ) VALUES (
                    @QuestionDescription,
                    @QuestionTypeId,
                    @Status,
                    @CreatedBy,
                    @CreatedOn,
                    @subjectID,
                    @EmployeeId,
                    @IndexTypeId,
                    @ContentIndexId,
                    @IsRejected,
                    @IsApproved,
                    @QuestionCode,
                    @Explanation,
                    @ExtraInformation,
                    @IsActive
                );
                SELECT QuestionCode FROM tblQuestion WHERE QuestionCode = @QuestionCode AND IsActive = 1;";

                // Retrieve the QuestionCode after insertion
                var insertedQuestionCode = await _connection.QuerySingleOrDefaultAsync<string>(insertQuery, question);
                if (!string.IsNullOrEmpty(insertedQuestionCode))
                {
                    // Handle QIDCourses mapping
                    var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);

                    // Handle QuestionSubjectMappings
                    var quesSub = await AddUpdateQuestionSubjectMap(request.QuestionSubjectMappings, insertedQuestionCode);

                    // Handle Answer mappings
                    string getQuesType = @"SELECT * FROM tblQBQuestionType WHERE QuestionTypeID = @QuestionTypeID;";
                    var questTypedata = await _connection.QueryFirstOrDefaultAsync<QuestionTypes>(getQuesType, new { QuestionTypeID = request.QuestionTypeId });
                    var answer = 0;

                    string insertAnswerQuery = @"
                    INSERT INTO [tblAnswerMaster] (Questionid, QuestionTypeid, QuestionCode)
                    VALUES (@Questionid, @QuestionTypeid, @QuestionCode);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                    // Use QuestionCode to insert into AnswerMaster
                    var Answerid = await _connection.QuerySingleAsync<int>(insertAnswerQuery, new
                    {
                        Questionid = 0, // Set to 0 or remove if QuestionId is not required
                        QuestionTypeid = questTypedata?.QuestionTypeID,
                        QuestionCode = insertedQuestionCode
                    });

                    if (questTypedata != null)
                    {
                        if (questTypedata.Code.Trim() == "MCQ" || questTypedata.Code.Trim() == "TF" || questTypedata.Code.Trim() == "MT1" ||
                            questTypedata.Code.Trim() == "MAQ" || questTypedata.Code.Trim() == "MT2" || questTypedata.Code.Trim() == "AR" || questTypedata.Code.Trim() == "C")
                        {
                            if (request.AnswerMultipleChoiceCategories != null)
                            {
                                foreach (var item in request.AnswerMultipleChoiceCategories)
                                {
                                    item.Answerid = Answerid;
                                }
                                string insertAnsQuery = @"
                INSERT INTO tblAnswerMultipleChoiceCategory
                (Answerid, Answer, Iscorrect, Matchid) 
                VALUES (@AnswerId, @Answer, @IsCorrect, @MatchId);";
                                answer = await _connection.ExecuteAsync(insertAnsQuery, request.AnswerMultipleChoiceCategories);
                            }
                        }
                        else
                        {
                            string sql = @"
            INSERT INTO tblAnswersingleanswercategory (Answerid, Answer)
            VALUES (@AnswerId, @Answer);";
                            if (request.Answersingleanswercategories != null)
                            {
                                request.Answersingleanswercategories.Answerid = Answerid;
                                answer = await _connection.ExecuteAsync(sql, request.Answersingleanswercategories);
                            }
                        }
                    }

                    if (data > 0 && quesSub > 0 && answer > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Question Added Successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                    }
                }
                else
                {
                    return new ServiceResponse<string>(false, "Some error occurred", string.Empty, 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetAllLiveQuestionsList(int SubjectId)
        {
            try
            {
                string sql = @"
        SELECT q.*, 
               c.CourseName, 
               b.BoardName, 
               cl.ClassName, 
               s.SubjectName,
               et.ExamTypeName,
               e.EmpFirstName, 
               qt.QuestionType as QuestionTypeName,
               it.IndexType as IndexTypeName,
               CASE 
                   WHEN q.IndexTypeId = 1 THEN ci.ContentName_Chapter
                   WHEN q.IndexTypeId = 2 THEN ct.ContentName_Topic
                   WHEN q.IndexTypeId = 3 THEN cst.ContentName_SubTopic
               END AS ContentIndexName
        FROM tblQuestion q
        LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
        LEFT JOIN tblCourse c ON q.courseid = c.CourseID
        LEFT JOIN tblBoard b ON q.boardid = b.BoardID
        LEFT JOIN tblClass cl ON q.classid = cl.ClassID
        LEFT JOIN tblSubject s ON q.subjectID = s.SubjectID
        LEFT JOIN tblExamType et ON q.ExamTypeId = et.ExamTypeId
        LEFT JOIN tblEmployee e ON q.EmployeeId = e.Employeeid
        LEFT JOIN tblQBIndexType it ON q.IndexTypeId = it.IndexId
        LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
        LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
        LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
        WHERE q.IsApproved = 1
          AND q.IsRejected = 0
          AND q.IsActive = 1
          AND q.SubjectID = @SubjectId
          AND q.IsLive = 1"; // Adjusted to ensure the questions are live

                var parameters = new
                {
                    SubjectId
                };

                var data = await _connection.QueryAsync<dynamic>(sql, parameters);

                if (data != null)
                {
                    var response = data.Select(item => new QuestionResponseDTO
                    {
                        QuestionId = item.QuestionId,
                        QuestionDescription = item.QuestionDescription,
                        QuestionTypeId = item.QuestionTypeId,
                        Status = item.Status,
                        CreatedBy = item.CreatedBy,
                        CreatedOn = item.CreatedOn,
                        ModifiedBy = item.ModifiedBy,
                        ModifiedOn = item.ModifiedOn,
                        subjectID = item.subjectID,
                        SubjectName = item.SubjectName,
                        EmployeeId = item.EmployeeId,
                        EmployeeName = item.EmpFirstName,
                        IndexTypeId = item.IndexTypeId,
                        IndexTypeName = item.IndexTypeName,
                        ContentIndexId = item.ContentIndexId,
                        ContentIndexName = item.ContentIndexName,
                        QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                        QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
                        Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
                        AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode),
                        IsApproved = item.IsApproved,
                        IsRejected = item.IsRejected,
                        QuestionTypeName = item.QuestionTypeName,
                        QuestionCode = item.QuestionCode,
                        Explanation = item.Explanation,
                        ExtraInformation = item.ExtraInformation,
                        IsActive = item.IsActive
                    }).ToList();

                    if (response.Count != 0)
                    {
                        return new ServiceResponse<List<QuestionResponseDTO>>(true, "Operation Successful", response, 200);
                    }
                    else
                    {
                        return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
                    }
                }
                else
                {
                    return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(false, ex.Message, new List<QuestionResponseDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<string>> MarkQuestionLive(string questionCode)
        {
            try
            {
                // Check if the question is approved and not rejected
                string checkSql = @"
            SELECT COUNT(*)
            FROM [tblQuestion]
            WHERE QuestionCode = @QuestionCode AND IsApproved = 1 AND IsRejected = 0 AND IsActive = 1";

                var exists = await _connection.ExecuteScalarAsync<int>(checkSql, new { QuestionCode = questionCode });

                if (exists > 0)
                {
                    // Update the question to mark it as live
                    string updateSql = @"
                UPDATE [tblQuestion]
                SET IsLive = 1
                WHERE QuestionCode = @QuestionCode";

                    await _connection.ExecuteAsync(updateSql, new { QuestionCode = questionCode });

                    return new ServiceResponse<string>(true, "Question marked as live successfully", string.Empty, 200);
                }
                else
                {
                    return new ServiceResponse<string>(false, "Question is either not approved, rejected, or not active", string.Empty, 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<List<EmployeeListAssignedQuestionCount>>> GetAssignedQuestionsCount(int EmployeeId)
        {
            try
            {
                // Initialize the list to hold the employee details with assigned question count
                List<EmployeeListAssignedQuestionCount> employeeList = new List<EmployeeListAssignedQuestionCount>();

                // SQL query to get the role of the given EmployeeId
                string employeeRoleQuery = @"
            SELECT e.RoleID, r.RoleCode
            FROM [tblEmployee] e
            JOIN [tblRole] r ON e.RoleID = r.RoleID
            WHERE e.EmployeeId = @EmployeeId";

                // Get the RoleID of the given EmployeeId
                var employeeRole = await _connection.QueryFirstOrDefaultAsync(employeeRoleQuery, new { EmployeeId });
                if (employeeRole == null)
                {
                    return new ServiceResponse<List<EmployeeListAssignedQuestionCount>>(false, "Employee not found.", employeeList, 404);
                }

                int roleID = employeeRole.RoleID;
                List<int> targetRoleIDs = new List<int>();

                // Determine the target roles based on the current role
                if (employeeRole.RoleCode == "TR")
                {
                    string sql = "SELECT [RoleID] FROM [tblRole] WHERE [RoleCode] = @RoleCode;";
                    var roleId = await _connection.QueryFirstOrDefaultAsync<int>(sql, new { RoleCode = "SM" });

                    // If the current role is Transcriber, target employees with SME role
                    targetRoleIDs.Add(roleId); // Assume RoleID for SME is 3
                }
                else if (employeeRole.RoleCode == "SM")
                {
                    string sql = "SELECT [RoleID] FROM [tblRole] WHERE [RoleCode] = @RoleCode;";
                    var roleId = await _connection.QueryFirstOrDefaultAsync<int>(sql, new { RoleCode = "PR" });
                    // If the current role is SME, target employees with Proofer and Transcriber roles
                    targetRoleIDs.Add(roleId); // Assume RoleID for Proofer is 4
                    var roleId1 = await _connection.QueryFirstOrDefaultAsync<int>(sql, new { RoleCode = "TR" });
                    targetRoleIDs.Add(roleId1); // Assume RoleID for Transcriber is 5
                }
                else if (employeeRole.RoleCode == "PR")
                {
                    string sql = "SELECT [RoleID] FROM [tblRole] WHERE [RoleCode] = @RoleCode;";
                    var roleId = await _connection.QueryFirstOrDefaultAsync<int>(sql, new { RoleCode = "SM" });
                    // If the current role is Proofer, target employees with SME role
                    targetRoleIDs.Add(roleId); // Assume RoleID for SME is 3
                }
                else
                {
                    return new ServiceResponse<List<EmployeeListAssignedQuestionCount>>(false, "Invalid role for the operation.", employeeList, 400);
                }

                // SQL query to fetch employees and their assigned question counts
                string assignedQuestionsCountQuery = @"
            SELECT e.EmployeeId, 
                   CONCAT(e.EmpFirstName, ' ', e.EmpMiddleName, ' ', e.EmpLastName) AS EmployeeName,
                   COUNT(qp.Questionid) AS Count
            FROM [tblEmployee] e
            LEFT JOIN [tblQuestionProfiler] qp ON e.EmployeeId = qp.EmpId AND qp.Status = 1
            WHERE e.RoleID IN @TargetRoleIDs
            GROUP BY e.EmployeeId, e.EmpFirstName, e.EmpMiddleName, e.EmpLastName
            ORDER BY EmployeeName";

                // Execute query to get the list of employees with the count of assigned questions
                employeeList = (await _connection.QueryAsync<EmployeeListAssignedQuestionCount>(assignedQuestionsCountQuery, new { TargetRoleIDs = targetRoleIDs })).ToList();

                return new ServiceResponse<List<EmployeeListAssignedQuestionCount>>(true, "Employee list with assigned questions count retrieved successfully.", employeeList, 200);
            }
            catch (Exception ex)
            {
                // Return failure response with error message
                return new ServiceResponse<List<EmployeeListAssignedQuestionCount>>(false, ex.Message, new List<EmployeeListAssignedQuestionCount>(), 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetAllQuestionsList(GetAllQuestionListRequest request)
        {
            try
            {
                // Initialize a list to hold all question codes to fetch
                List<string> assignedQuestionCodes = new List<string>();

                // Step 1: Fetch list of QuestionCodes assigned to the given employee if EmployeeId is provided
                if (request.EmployeeId > 0)
                {
                    string fetchAssignedQuestionsSql = @"
                    SELECT QuestionCode 
                    FROM tblQuestionProfiler 
                    WHERE EmpId = @EmployeeId AND Status = 1";

                    assignedQuestionCodes = (await _connection.QueryAsync<string>(fetchAssignedQuestionsSql, new { EmployeeId = request.EmployeeId })).ToList();
                }

                // Step 2: Fetch questions based on the provided filters
                string fetchQuestionsSql = @"
                SELECT q.*, 
                       c.CourseName, 
                       b.BoardName, 
                       cl.ClassName, 
                       s.SubjectName,
                       et.ExamTypeName,
                       e.EmpFirstName, 
                       qt.QuestionType as QuestionTypeName,
                       it.IndexType as IndexTypeName,
                       CASE 
                           WHEN q.IndexTypeId = 1 THEN ci.ContentName_Chapter
                           WHEN q.IndexTypeId = 2 THEN ct.ContentName_Topic
                           WHEN q.IndexTypeId = 3 THEN cst.ContentName_SubTopic
                       END AS ContentIndexName
                FROM tblQuestion q
                LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
                LEFT JOIN tblCourse c ON q.courseid = c.CourseID
                LEFT JOIN tblBoard b ON q.boardid = b.BoardID
                LEFT JOIN tblClass cl ON q.classid = cl.ClassID
                LEFT JOIN tblSubject s ON q.subjectID = s.SubjectID
                LEFT JOIN tblExamType et ON q.ExamTypeId = et.ExamTypeId
                LEFT JOIN tblEmployee e ON q.EmployeeId = e.Employeeid
                LEFT JOIN tblQBIndexType it ON q.IndexTypeId = it.IndexId
                LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
                LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
                LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
                WHERE (@ContentIndexId = 0 OR q.ContentIndexId = @ContentIndexId)
                  AND (@IndexTypeId = 0 OR q.IndexTypeId = @IndexTypeId)
                  AND q.IsRejected = 0
                  AND q.IsApproved = 0
                  AND (q.EmployeeId = @EmployeeId OR q.QuestionCode IN @QuestionCodes)
                  AND q.IsActive = 1
                  AND IsLive = 0";

                var parameters = new
                {
                    ContentIndexId = request.ContentIndexId,
                    IndexTypeId = request.IndexTypeId,
                    EmployeeId = request.EmployeeId,
                    QuestionCodes = assignedQuestionCodes
                };

                var data = await _connection.QueryAsync<dynamic>(fetchQuestionsSql, parameters);

                if (data != null)
                {
                    // Convert the data to a list of DTOs
                    var response = data.Select(item => new QuestionResponseDTO
                    {
                        QuestionId = item.QuestionId,
                        QuestionDescription = item.QuestionDescription,
                        QuestionTypeId = item.QuestionTypeId,
                        Status = item.Status,
                        CreatedBy = item.CreatedBy,
                        CreatedOn = item.CreatedOn,
                        ModifiedBy = item.ModifiedBy,
                        ModifiedOn = item.ModifiedOn,
                        subjectID = item.subjectID,
                        SubjectName = item.SubjectName,
                        EmployeeId = item.EmployeeId,
                        EmployeeName = item.EmpFirstName,
                        IndexTypeId = item.IndexTypeId,
                        IndexTypeName = item.IndexTypeName,
                        ContentIndexId = item.ContentIndexId,
                        ContentIndexName = item.ContentIndexName,
                        IsRejected = item.IsRejected,
                        IsApproved = item.IsApproved,
                        QuestionTypeName = item.QuestionTypeName,
                        QuestionCode = item.QuestionCode,
                        Explanation = item.Explanation,
                        ExtraInformation = item.ExtraInformation,
                        IsActive = item.IsActive,
                        QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                        QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
                        Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
                        AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode)
                    }).ToList();

                    // Step 3: Filter out questions assigned to other employees
                    var questionCodesCreatedByEmployee = response.Where(r => r.EmployeeId == request.EmployeeId).Select(r => r.QuestionCode).ToList();
                    string fetchAssignedToOthersSql = @"
                    SELECT DISTINCT QuestionCode 
                    FROM tblQuestionProfiler 
                    WHERE QuestionCode IN @QuestionCodes 
                    AND EmpId != @EmployeeId 
                    AND Status = 1";

                    var assignedToOthersQuestionCodes = (await _connection.QueryAsync<string>(fetchAssignedToOthersSql, new
                    {
                        QuestionCodes = questionCodesCreatedByEmployee,
                        EmployeeId = request.EmployeeId
                    })).ToList();

                    response = response.Where(r => !assignedToOthersQuestionCodes.Contains(r.QuestionCode)).ToList();

                    // Adding role information and pagination logic
                    foreach (var record in response)
                    {
                        var employeeRoleId = _connection.QuerySingleOrDefault<int?>("SELECT RoleID FROM tblEmployee WHERE EmployeeID = @EmployeeId", new { EmployeeId = record.EmployeeId });
                        record.userRole = employeeRoleId.HasValue ? GetRoleName(employeeRoleId.Value) : string.Empty;
                    }

                    int totalCount = response.Count;

                    var paginatedList = response.Skip((request.PageNumber - 1) * request.PageSize)
                                                .Take(request.PageSize)
                                                .ToList();

                    if (paginatedList.Count != 0)
                    {
                        return new ServiceResponse<List<QuestionResponseDTO>>(true, "Operation Successful", paginatedList, 200, totalCount);
                    }
                    else
                    {
                        return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
                    }
                }
                else
                {
                    return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(false, ex.Message, new List<QuestionResponseDTO>(), 500);
            }
        }

        //public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetAllQuestionsList(GetAllQuestionListRequest request)
        //{
        //    try
        //    {
        //        // Initialize a list to hold all question codes to fetch
        //        List<string> assignedQuestionCodes = new List<string>();

        //        // Step 1: Fetch list of QuestionCodes assigned to the given employee if EmployeeId is provided
        //        if (request.EmployeeId > 0)
        //        {
        //            string fetchAssignedQuestionsSql = @"
        //    SELECT QuestionCode 
        //    FROM tblQuestionProfiler 
        //    WHERE EmpId = @EmployeeId AND Status = 1";

        //            assignedQuestionCodes = (await _connection.QueryAsync<string>(fetchAssignedQuestionsSql, new { EmployeeId = request.EmployeeId })).ToList();
        //        }

        //        // Step 2: Fetch questions based on the provided filters
        //        string fetchQuestionsSql = @"
        //SELECT q.*, 
        //       c.CourseName, 
        //       b.BoardName, 
        //       cl.ClassName, 
        //       s.SubjectName,
        //       et.ExamTypeName,
        //       e.EmpFirstName, 
        //       qt.QuestionType as QuestionTypeName,
        //       it.IndexType as IndexTypeName,
        //       CASE 
        //           WHEN q.IndexTypeId = 1 THEN ci.ContentName_Chapter
        //           WHEN q.IndexTypeId = 2 THEN ct.ContentName_Topic
        //           WHEN q.IndexTypeId = 3 THEN cst.ContentName_SubTopic
        //       END AS ContentIndexName
        //FROM tblQuestion q
        //LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
        //LEFT JOIN tblCourse c ON q.courseid = c.CourseID
        //LEFT JOIN tblBoard b ON q.boardid = b.BoardID
        //LEFT JOIN tblClass cl ON q.classid = cl.ClassID
        //LEFT JOIN tblSubject s ON q.subjectID = s.SubjectID
        //LEFT JOIN tblExamType et ON q.ExamTypeId = et.ExamTypeId
        //LEFT JOIN tblEmployee e ON q.EmployeeId = e.Employeeid
        //LEFT JOIN tblQBIndexType it ON q.IndexTypeId = it.IndexId
        //LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
        //LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
        //LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
        //WHERE (@ContentIndexId = 0 OR q.ContentIndexId = @ContentIndexId)
        //  AND (@IndexTypeId = 0 OR q.IndexTypeId = @IndexTypeId)
        //  AND q.IsRejected = 0
        //  AND q.IsApproved = 0
        //  AND (q.EmployeeId = @EmployeeId OR q.QuestionCode IN @QuestionCodes)
        //  AND q.IsActive = 1
        //  AND IsLive = 0";

        //        var parameters = new
        //        {
        //            ContentIndexId = request.ContentIndexId,
        //            IndexTypeId = request.IndexTypeId,
        //            EmployeeId = request.EmployeeId,
        //            QuestionCodes = assignedQuestionCodes
        //        };

        //        var data = await _connection.QueryAsync<dynamic>(fetchQuestionsSql, parameters);

        //        if (data != null)
        //        {
        //            var response = data.Select(item => new QuestionResponseDTO
        //            {
        //                QuestionId = item.QuestionId,
        //                QuestionDescription = item.QuestionDescription,
        //                QuestionTypeId = item.QuestionTypeId,
        //                Status = item.Status,
        //                CreatedBy = item.CreatedBy,
        //                CreatedOn = item.CreatedOn,
        //                ModifiedBy = item.ModifiedBy,
        //                ModifiedOn = item.ModifiedOn,
        //                subjectID = item.subjectID,
        //                SubjectName = item.SubjectName,
        //                EmployeeId = item.EmployeeId,
        //                EmployeeName = item.EmpFirstName,
        //                IndexTypeId = item.IndexTypeId,
        //                IndexTypeName = item.IndexTypeName,
        //                ContentIndexId = item.ContentIndexId,
        //                ContentIndexName = item.ContentIndexName,
        //                IsRejected = item.IsRejected,
        //                IsApproved = item.IsApproved,
        //                QuestionTypeName = item.QuestionTypeName,
        //                QuestionCode = item.QuestionCode,
        //                Explanation = item.Explanation,
        //                ExtraInformation = item.ExtraInformation,
        //                IsActive = item.IsActive,
        //                QIDCourses = GetListOfQIDCourse(item.QuestionCode),
        //                QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
        //                Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
        //                AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode)
        //            }).ToList();
        //            foreach (var record in response)
        //            {
        //                // Get role based on EmployeeId
        //                var employeeRoleId = _connection.QuerySingleOrDefault<int?>("SELECT RoleID FROM tblEmployee WHERE EmployeeID = @EmployeeId", new { EmployeeId = record.EmployeeId });
        //                record.userRole = employeeRoleId.HasValue ? GetRoleName(employeeRoleId.Value) : string.Empty; // Map the role name
        //            }
        //            int totalCount = response.Count;

        //            var paginatedList = response.Skip((request.PageNumber - 1) * request.PageSize)
        //                                        .Take(request.PageSize)
        //                                        .ToList();

        //            if (paginatedList.Count != 0)
        //            {
        //                return new ServiceResponse<List<QuestionResponseDTO>>(true, "Operation Successful", paginatedList, 200, totalCount);
        //            }
        //            else
        //            {
        //                return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
        //            }
        //        }
        //        else
        //        {
        //            return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<List<QuestionResponseDTO>>(false, ex.Message, new List<QuestionResponseDTO>(), 500);
        //    }
        //}
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetAssignedQuestionsList(int employeeId)
        {
            try
            {
                string query = @"
        SELECT q.*, s.SubjectName, e.EmpFirstName + ' ' + e.EmpLastName AS EmployeeName,
               it.IndexType as IndexTypeName,
               CASE 
                   WHEN q.IndexTypeId = 1 THEN ci.ContentName_Chapter
                   WHEN q.IndexTypeId = 2 THEN ct.ContentName_Topic
                   WHEN q.IndexTypeId = 3 THEN cst.ContentName_SubTopic
               END AS ContentIndexName,
               qt.QuestionType as QuestionTypeName
        FROM tblQuestion q
        LEFT JOIN tblSubject s ON q.subjectID = s.SubjectID
        LEFT JOIN tblEmployee e ON q.EmployeeId = e.Employeeid
        LEFT JOIN tblQBIndexType it ON q.IndexTypeId = it.IndexId
        LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
        LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
        LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
        LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
        WHERE q.EmployeeId = @EmployeeId AND q.IsActive = 1 AND IsLive = 0";

                var questions = await _connection.QueryAsync<QuestionResponseDTO>(query, new { EmployeeId = employeeId });

                // Fetch additional details for each question
                foreach (var question in questions)
                {
                    question.QIDCourses = GetListOfQIDCourse(question.QuestionCode);
                    question.QuestionSubjectMappings = GetListOfQuestionSubjectMapping(question.QuestionCode);
                    question.AnswerMultipleChoiceCategories = GetMultipleAnswers(question.QuestionCode);
                    question.Answersingleanswercategories = GetSingleAnswer(question.QuestionCode);
                }

                return new ServiceResponse<List<QuestionResponseDTO>>(true, "Questions fetched successfully", questions.ToList(), 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(false, ex.Message, null, 500);
            }
        }
        //public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetApprovedQuestionsList(GetAllQuestionListRequest request)
        //{
        //    try
        //    {
        //        string sql = @"
        //        SELECT q.*, 
        //               c.CourseName, 
        //               b.BoardName, 
        //               cl.ClassName, 
        //               s.SubjectName,
        //               et.ExamTypeName,
        //               e.EmpFirstName, 
        //               qt.QuestionType as QuestionTypeName,
        //               it.IndexType as IndexTypeName,
        //               CASE 
        //                   WHEN q.IndexTypeId = 1 THEN ci.ContentName_Chapter
        //                   WHEN q.IndexTypeId = 2 THEN ct.ContentName_Topic
        //                   WHEN q.IndexTypeId = 3 THEN cst.ContentName_SubTopic
        //               END AS ContentIndexName
        //        FROM tblQuestion q
        //        LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
        //        LEFT JOIN tblCourse c ON q.courseid = c.CourseID
        //        LEFT JOIN tblBoard b ON q.boardid = b.BoardID
        //        LEFT JOIN tblClass cl ON q.classid = cl.ClassID
        //        LEFT JOIN tblSubject s ON q.subjectID = s.SubjectID
        //        LEFT JOIN tblExamType et ON q.ExamTypeId = et.ExamTypeId
        //        LEFT JOIN tblEmployee e ON q.EmployeeId = e.Employeeid
        //        LEFT JOIN tblQBIndexType it ON q.IndexTypeId = it.IndexId
        //        LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
        //        LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
        //        LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
        //        WHERE (@ContentIndexId = 0 OR q.ContentIndexId = @ContentIndexId)
        //          AND (@IndexTypeId = 0 OR q.IndexTypeId = @IndexTypeId)
        //          AND q.IsApproved = 1
        //          AND q.IsRejected = 0
        //          AND q.IsActive = 1
        //          AND (@EmployeeId = 0 OR q.EmployeeId = @EmployeeId)
        //          AND IsLive = 0";

        //        var parameters = new
        //        {
        //            ContentIndexId = request.ContentIndexId,
        //            IndexTypeId = request.IndexTypeId,
        //            request.EmployeeId
        //        };

        //        var data = await _connection.QueryAsync<dynamic>(sql, parameters);

        //        if (data != null)
        //        {
        //            var response = data.Select(item => new QuestionResponseDTO
        //            {
        //                QuestionId = item.QuestionId,
        //                QuestionDescription = item.QuestionDescription,
        //                QuestionTypeId = item.QuestionTypeId,
        //                Status = item.Status,
        //                CreatedBy = item.CreatedBy,
        //                CreatedOn = item.CreatedOn,
        //                ModifiedBy = item.ModifiedBy,
        //                ModifiedOn = item.ModifiedOn,
        //                subjectID = item.subjectID,
        //                SubjectName = item.SubjectName,
        //                EmployeeId = item.EmployeeId,
        //                EmployeeName = item.EmpFirstName,
        //                IndexTypeId = item.IndexTypeId,
        //                IndexTypeName = item.IndexTypeName,
        //                ContentIndexId = item.ContentIndexId,
        //                ContentIndexName = item.ContentIndexName,
        //                QIDCourses = GetListOfQIDCourse(item.QuestionCode),
        //                QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
        //                Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
        //                AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode),
        //                IsApproved = item.IsApproved,
        //                IsRejected = item.IsRejected,
        //                QuestionTypeName = item.QuestionTypeName,
        //                QuestionCode = item.QuestionCode,
        //                Explanation = item.Explanation,
        //                ExtraInformation = item.ExtraInformation,
        //                IsActive = item.IsActive
        //            }).ToList();

        //            var paginatedList = response.Skip((request.PageNumber - 1) * request.PageSize)
        //                                        .Take(request.PageSize)
        //                                        .ToList();

        //            if (paginatedList.Count != 0)
        //            {
        //                return new ServiceResponse<List<QuestionResponseDTO>>(true, "Operation Successful", paginatedList, 200);
        //            }
        //            else
        //            {
        //                return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
        //            }
        //        }
        //        else
        //        {
        //            return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<List<QuestionResponseDTO>>(false, ex.Message, new List<QuestionResponseDTO>(), 500);
        //    }
        //}
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetApprovedQuestionsList(GetAllQuestionListRequest request)
        {
            try
            {
                // Initialize a list to hold question codes that are assigned to other employees for review
                List<string> assignedToOtherEmployeesQuestionCodes = new List<string>();

                // Fetch the list of question codes that were created by the given employee but are assigned to other employees for review
                if (request.EmployeeId > 0)
                {
                    string fetchAssignedQuestionsSql = @"
            SELECT DISTINCT QuestionCode 
            FROM tblQuestionProfiler 
            WHERE QuestionCode IS NOT NULL 
              AND Status = 1 
              AND QuestionCode IN (SELECT QuestionCode FROM tblQuestion WHERE EmployeeId = @EmployeeId) 
              AND EmpId != @EmployeeId";

                    assignedToOtherEmployeesQuestionCodes = (await _connection.QueryAsync<string>(
                        fetchAssignedQuestionsSql, new { EmployeeId = request.EmployeeId })).ToList();
                }

                // Fetch approved questions based on the provided filters
                string sql = @"
        SELECT q.*, 
               c.CourseName, 
               b.BoardName, 
               cl.ClassName, 
               s.SubjectName,
               et.ExamTypeName,
               e.EmpFirstName, 
               qt.QuestionType as QuestionTypeName,
               it.IndexType as IndexTypeName,
               CASE 
                   WHEN q.IndexTypeId = 1 THEN ci.ContentName_Chapter
                   WHEN q.IndexTypeId = 2 THEN ct.ContentName_Topic
                   WHEN q.IndexTypeId = 3 THEN cst.ContentName_SubTopic
               END AS ContentIndexName
        FROM tblQuestion q
        LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
        LEFT JOIN tblCourse c ON q.courseid = c.CourseID
        LEFT JOIN tblBoard b ON q.boardid = b.BoardID
        LEFT JOIN tblClass cl ON q.classid = cl.ClassID
        LEFT JOIN tblSubject s ON q.subjectID = s.SubjectID
        LEFT JOIN tblExamType et ON q.ExamTypeId = et.ExamTypeId
        LEFT JOIN tblEmployee e ON q.EmployeeId = e.Employeeid
        LEFT JOIN tblQBIndexType it ON q.IndexTypeId = it.IndexId
        LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
        LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
        LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
        WHERE (@ContentIndexId = 0 OR q.ContentIndexId = @ContentIndexId)
          AND (@IndexTypeId = 0 OR q.IndexTypeId = @IndexTypeId)
          AND q.IsApproved = 1
          AND q.IsRejected = 0
          AND q.IsActive = 1
          AND (@EmployeeId = 0 OR q.EmployeeId = @EmployeeId)
          AND IsLive = 0
          AND (q.QuestionCode IS NULL OR q.QuestionCode NOT IN @AssignedToOtherEmployeesQuestionCodes)"; // Exclude questions assigned to other employees

                var parameters = new
                {
                    ContentIndexId = request.ContentIndexId,
                    IndexTypeId = request.IndexTypeId,
                    request.EmployeeId,
                    AssignedToOtherEmployeesQuestionCodes = assignedToOtherEmployeesQuestionCodes
                };

                var data = await _connection.QueryAsync<dynamic>(sql, parameters);

                if (data != null)
                {
                    var response = data.Select(item => new QuestionResponseDTO
                    {
                        QuestionId = item.QuestionId,
                        QuestionDescription = item.QuestionDescription,
                        QuestionTypeId = item.QuestionTypeId,
                        Status = item.Status,
                        CreatedBy = item.CreatedBy,
                        CreatedOn = item.CreatedOn,
                        ModifiedBy = item.ModifiedBy,
                        ModifiedOn = item.ModifiedOn,
                        subjectID = item.subjectID,
                        SubjectName = item.SubjectName,
                        EmployeeId = item.EmployeeId,
                        EmployeeName = item.EmpFirstName,
                        IndexTypeId = item.IndexTypeId,
                        IndexTypeName = item.IndexTypeName,
                        ContentIndexId = item.ContentIndexId,
                        ContentIndexName = item.ContentIndexName,
                        QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                        QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
                        Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
                        AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode),
                        IsApproved = item.IsApproved,
                        IsRejected = item.IsRejected,
                        QuestionTypeName = item.QuestionTypeName,
                        QuestionCode = item.QuestionCode,
                        Explanation = item.Explanation,
                        ExtraInformation = item.ExtraInformation,
                        IsActive = item.IsActive
                    }).ToList();

                    var paginatedList = response.Skip((request.PageNumber - 1) * request.PageSize)
                                                .Take(request.PageSize)
                                                .ToList();

                    if (paginatedList.Count != 0)
                    {
                        return new ServiceResponse<List<QuestionResponseDTO>>(true, "Operation Successful", paginatedList, 200);
                    }
                    else
                    {
                        return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
                    }
                }
                else
                {
                    return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(false, ex.Message, new List<QuestionResponseDTO>(), 500);
            }
        }
        //public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetRejectedQuestionsList(GetAllQuestionListRequest request)
        //{
        //    try
        //    {
        //        string sql = @"
        //        SELECT q.*, 
        //               c.CourseName, 
        //               b.BoardName, 
        //               cl.ClassName, 
        //               s.SubjectName,
        //               et.ExamTypeName,
        //               e.EmpFirstName, 
        //               qt.QuestionType as QuestionTypeName,
        //               it.IndexType as IndexTypeName,
        //           CASE 
        //           WHEN q.IndexTypeId = 1 THEN ci.ContentName_Chapter
        //           WHEN q.IndexTypeId = 2 THEN ct.ContentName_Topic
        //           WHEN q.IndexTypeId = 3 THEN cst.ContentName_SubTopic
        //           END AS ContentIndexName
        //        FROM tblQuestion q
        //        LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
        //        LEFT JOIN tblCourse c ON q.courseid = c.CourseID
        //        LEFT JOIN tblBoard b ON q.boardid = b.BoardID
        //        LEFT JOIN tblClass cl ON q.classid = cl.ClassID
        //        LEFT JOIN tblSubject s ON q.subjectID = s.SubjectID
        //        LEFT JOIN tblExamType et ON q.ExamTypeId = et.ExamTypeId
        //        LEFT JOIN tblEmployee e ON q.EmployeeId = e.Employeeid
        //        LEFT JOIN tblQBIndexType it ON q.IndexTypeId = it.IndexId
        //        LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
        //        LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
        //        LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
        //        WHERE (@ContentIndexId = 0 OR q.ContentIndexId = @ContentIndexId)
        //          AND (@IndexTypeId = 0 OR q.IndexTypeId = @IndexTypeId)
        //          AND q.IsRejected = 1
        //          AND q.IsApproved = 0
        //          AND q.IsActive = 1
        //          AND (@EmployeeId = 0 OR q.EmployeeId = @EmployeeId)
        //          AND IsLive = 0";

        //        var parameters = new
        //        {
        //            ContentIndexId = request.ContentIndexId,
        //            IndexTypeId = request.IndexTypeId,
        //            request.EmployeeId
        //        };

        //        var data = await _connection.QueryAsync<dynamic>(sql, parameters);

        //        if (data != null)
        //        {
        //            var response = data.Select(item => new QuestionResponseDTO
        //            {
        //                QuestionId = item.QuestionId,
        //                QuestionDescription = item.QuestionDescription,
        //                QuestionTypeId = item.QuestionTypeId,
        //                Status = item.Status,
        //                CreatedBy = item.CreatedBy,
        //                CreatedOn = item.CreatedOn,
        //                ModifiedBy = item.ModifiedBy,
        //                ModifiedOn = item.ModifiedOn,
        //                subjectID = item.subjectID,
        //                SubjectName = item.SubjectName,
        //                EmployeeId = item.EmployeeId,
        //                EmployeeName = item.EmpFirstName,
        //                IndexTypeId = item.IndexTypeId,
        //                IndexTypeName = item.IndexTypeName,
        //                ContentIndexId = item.ContentIndexId,
        //                ContentIndexName = item.ContentIndexName,
        //                QIDCourses = GetListOfQIDCourse(item.QuestionCode),
        //                QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
        //                Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
        //                AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode),
        //                IsApproved = item.IsApproved,
        //                IsRejected = item.IsRejected,
        //                QuestionTypeName = item.QuestionTypeName,
        //                QuestionCode = item.QuestionCode,
        //                Explanation = item.Explanation,
        //                ExtraInformation = item.ExtraInformation,
        //                IsActive = item.IsActive
        //            }).ToList();

        //            var paginatedList = response.Skip((request.PageNumber - 1) * request.PageSize)
        //                                        .Take(request.PageSize)
        //                                        .ToList();

        //            if (paginatedList.Count != 0)
        //            {
        //                return new ServiceResponse<List<QuestionResponseDTO>>(true, "Operation Successful", paginatedList, 200);
        //            }
        //            else
        //            {
        //                return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
        //            }
        //        }
        //        else
        //        {
        //            return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<List<QuestionResponseDTO>>(false, ex.Message, new List<QuestionResponseDTO>(), 500);
        //    }
        //}
        public async Task<ServiceResponse<List<QuestionResponseDTO>>> GetRejectedQuestionsList(GetAllQuestionListRequest request)
        {
            try
            {
                string sql = @"
        SELECT q.*, 
               c.CourseName, 
               b.BoardName, 
               cl.ClassName, 
               s.SubjectName,
               et.ExamTypeName,
               e.EmpFirstName, 
               qt.QuestionType as QuestionTypeName,
               it.IndexType as IndexTypeName,
               CASE 
               WHEN q.IndexTypeId = 1 THEN ci.ContentName_Chapter
               WHEN q.IndexTypeId = 2 THEN ct.ContentName_Topic
               WHEN q.IndexTypeId = 3 THEN cst.ContentName_SubTopic
               END AS ContentIndexName
        FROM tblQuestion q
        LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
        LEFT JOIN tblCourse c ON q.courseid = c.CourseID
        LEFT JOIN tblBoard b ON q.boardid = b.BoardID
        LEFT JOIN tblClass cl ON q.classid = cl.ClassID
        LEFT JOIN tblSubject s ON q.subjectID = s.SubjectID
        LEFT JOIN tblExamType et ON q.ExamTypeId = et.ExamTypeId
        LEFT JOIN tblEmployee e ON q.EmployeeId = e.Employeeid
        LEFT JOIN tblQBIndexType it ON q.IndexTypeId = it.IndexId
        LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
        LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
        LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
        WHERE (@ContentIndexId = 0 OR q.ContentIndexId = @ContentIndexId)
          AND (@IndexTypeId = 0 OR q.IndexTypeId = @IndexTypeId)
          AND q.IsRejected = 1
          AND q.IsApproved = 0
          AND q.IsActive = 1
          AND (@EmployeeId = 0 OR q.EmployeeId = @EmployeeId)
          AND IsLive = 0
          AND NOT EXISTS (
              SELECT 1
              FROM tblQuestionProfiler qp
              WHERE qp.QuestionCode = q.QuestionCode
                AND qp.Status = 1
                AND qp.EmpId != @EmployeeId
          )";

                var parameters = new
                {
                    ContentIndexId = request.ContentIndexId,
                    IndexTypeId = request.IndexTypeId,
                    request.EmployeeId
                };

                var data = await _connection.QueryAsync<dynamic>(sql, parameters);

                if (data != null)
                {
                    var response = data.Select(item => new QuestionResponseDTO
                    {
                        QuestionId = item.QuestionId,
                        QuestionDescription = item.QuestionDescription,
                        QuestionTypeId = item.QuestionTypeId,
                        Status = item.Status,
                        CreatedBy = item.CreatedBy,
                        CreatedOn = item.CreatedOn,
                        ModifiedBy = item.ModifiedBy,
                        ModifiedOn = item.ModifiedOn,
                        subjectID = item.subjectID,
                        SubjectName = item.SubjectName,
                        EmployeeId = item.EmployeeId,
                        EmployeeName = item.EmpFirstName,
                        IndexTypeId = item.IndexTypeId,
                        IndexTypeName = item.IndexTypeName,
                        ContentIndexId = item.ContentIndexId,
                        ContentIndexName = item.ContentIndexName,
                        QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                        QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
                        Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
                        AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode),
                        IsApproved = item.IsApproved,
                        IsRejected = item.IsRejected,
                        QuestionTypeName = item.QuestionTypeName,
                        QuestionCode = item.QuestionCode,
                        Explanation = item.Explanation,
                        ExtraInformation = item.ExtraInformation,
                        IsActive = item.IsActive
                    }).ToList();

                    var paginatedList = response.Skip((request.PageNumber - 1) * request.PageSize)
                                                .Take(request.PageSize)
                                                .ToList();

                    if (paginatedList.Count != 0)
                    {
                        return new ServiceResponse<List<QuestionResponseDTO>>(true, "Operation Successful", paginatedList, 200);
                    }
                    else
                    {
                        return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
                    }
                }
                else
                {
                    return new ServiceResponse<List<QuestionResponseDTO>>(false, "No records found", new List<QuestionResponseDTO>(), 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<QuestionResponseDTO>>(false, ex.Message, new List<QuestionResponseDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<QuestionResponseDTO>> GetQuestionByCode(string questionCode)
        {
            try
            {
                string sql = @"
                SELECT q.*, 
                       c.CourseName, 
                       b.BoardName, 
                       cl.ClassName, 
                       s.SubjectName,
                       et.ExamTypeName,
                       e.EmpFirstName,
                       qt.QuestionType as QuestionTypeName,
                       it.IndexType as IndexTypeName,
                       CASE 
                           WHEN q.IndexTypeId = 1 THEN ci.ContentName_Chapter
                           WHEN q.IndexTypeId = 2 THEN ct.ContentName_Topic
                           WHEN q.IndexTypeId = 3 THEN cst.ContentName_SubTopic
                       END AS ContentIndexName
                FROM tblQuestion q
                LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
                LEFT JOIN tblCourse c ON q.courseid = c.CourseID
                LEFT JOIN tblBoard b ON q.boardid = b.BoardID
                LEFT JOIN tblClass cl ON q.classid = cl.ClassID
                LEFT JOIN tblSubject s ON q.subjectID = s.SubjectID
                LEFT JOIN tblExamType et ON q.ExamTypeId = et.ExamTypeId
                LEFT JOIN tblEmployee e ON q.EmployeeId = e.EmployeeId
                LEFT JOIN tblQBIndexType it ON q.IndexTypeId = it.IndexId
                LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
                LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
                LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
                WHERE q.QuestionCode = @QuestionCode AND q.IsActive = 1 AND IsLive = 0";

                var parameters = new { QuestionCode = questionCode };

                var item = await _connection.QueryFirstOrDefaultAsync<dynamic>(sql, parameters);

                if (item != null)
                {
                    var questionResponse = new QuestionResponseDTO
                    {
                        QuestionId = item.QuestionId,
                        QuestionDescription = item.QuestionDescription,
                        SubjectName = item.SubjectName,
                        EmployeeName = item.EmpFirstName,
                        IndexTypeName = item.IndexTypeName,
                        ContentIndexName = item.ContentIndexName,
                        QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                        QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
                        Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
                        AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode),
                        ContentIndexId = item.ContentIndexId,
                        CreatedBy = item.CreatedBy,
                        CreatedOn = item.CreatedOn,
                        EmployeeId = item.EmployeeId,
                        IndexTypeId = item.IndexTypeId,
                        subjectID = item.subjectID,
                        ModifiedOn = item.ModifiedOn,
                        QuestionTypeId = item.QuestionTypeId,
                        QuestionTypeName = item.QuestionTypeName,
                        QuestionCode = item.QuestionCode,
                        Explanation = item.Explanation,
                        ExtraInformation = item.ExtraInformation,
                        IsActive = item.IsActive
                    };

                    return new ServiceResponse<QuestionResponseDTO>(true, "Operation Successful", questionResponse, 200);
                }
                else
                {
                    return new ServiceResponse<QuestionResponseDTO>(false, "No records found", new QuestionResponseDTO(), 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<QuestionResponseDTO>(false, ex.Message, new QuestionResponseDTO(), 500);
            }
        }
        public async Task<ServiceResponse<List<QuestionComparisonDTO>>> CompareQuestionAsync(QuestionCompareRequest newQuestion)
        {
            // Fetch only active questions
            string query = "SELECT QuestionCode, QuestionId, QuestionDescription FROM tblQuestion WHERE IsActive = 1";
            var existingQuestions = await _connection.QueryAsync<Question>(query);

            // Calculate similarity and create comparison objects
            var comparisons = existingQuestions.Select(q => new QuestionComparisonDTO
            {
                QuestionCode = q.QuestionCode,
                QuestionID = q.QuestionId,
                QuestionText = q.QuestionDescription,
                Similarity = CalculateSimilarity(newQuestion.NewQuestion, q.QuestionDescription)
            })
            .Where(c => c.Similarity > 59.0 && c.Similarity < 101.0) // Filter based on similarity
            .OrderByDescending(c => c.Similarity) // Order by similarity in descending order
            .Take(10) // Take top 10 results
            .ToList();

            return new ServiceResponse<List<QuestionComparisonDTO>>(true, "Comparison results", comparisons, 200, comparisons.Count);
        }
        public async Task<ServiceResponse<string>> RejectQuestion(QuestionRejectionRequestDTO request)
        {
            try
            {
                string updateSql = @"
        UPDATE [tblQuestion]
        SET 
           IsRejected = @IsRejected,
           IsApproved = 0 
        WHERE
           QuestionCode = @QuestionCode AND IsActive = 1";

                var parameters = new
                {
                    request.QuestionCode,
                    IsRejected = true
                };

                var affectedRows = await _connection.ExecuteAsync(updateSql, parameters);
                if (affectedRows > 0)
                {
                    string sql = @"
            INSERT INTO [tblQuestionProfilerRejections]
            ([Questionid], [CreatedDate], [QuestionRejectReason], [RejectedBy], QuestionCode)
            VALUES (@QuestionId, @CreatedDate, @QuestionRejectReason, @RejectedBy, @QuestionCode);

            SELECT CAST(SCOPE_IDENTITY() as int)";

                    var questionId = await _connection.QueryFirstOrDefaultAsync<int>(@"
                SELECT QuestionId FROM [tblQuestion] 
                WHERE QuestionCode = @QuestionCode AND IsActive = 1",
                        new { request.QuestionCode });

                    if (questionId > 0)
                    {
                        var newId = await _connection.ExecuteScalarAsync<int>(sql, new
                        {
                            QuestionId = questionId,
                            CreatedDate = request.RejectedDate,
                            QuestionRejectReason = request.RejectedReason,
                            request.Rejectedby,
                            request.QuestionCode
                        });

                        if (newId > 0)
                        {
                            return new ServiceResponse<string>(true, "Question rejected successfully", "Success", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Some error occurred", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Question not found", "Failure", 404);
                    }
                }
                else
                {
                    return new ServiceResponse<string>(false, "Question not found or already inactive", "Failure", 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<string>> ApproveQuestion(QuestionApprovalRequestDTO request)
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }

                using (var transaction = _connection.BeginTransaction())
                {
                    // Update the question status to approved
                    string updateQuestionSql = @"
            UPDATE [tblQuestion]
            SET IsApproved = @IsApproved, IsRejected = 0
            WHERE QuestionCode = @QuestionCode AND IsActive = 1";

                    var updateParameters = new
                    {
                        request.QuestionCode,
                        IsApproved = true
                    };

                    var affectedRows = await _connection.ExecuteAsync(updateQuestionSql, updateParameters, transaction);
                    if (affectedRows == 0)
                    {
                        return new ServiceResponse<string>(false, "Question not found or already inactive", "Failure", 404);
                    }

                    // Get the QuestionId based on QuestionCode and IsActive = 1
                    var questionId = await _connection.QueryFirstOrDefaultAsync<int>(@"
            SELECT QuestionId FROM [tblQuestion]
            WHERE QuestionCode = @QuestionCode AND IsActive = 1",
                    new { request.QuestionCode }, transaction);

                    if (questionId == 0)
                    {
                        return new ServiceResponse<string>(false, "Question not found or already inactive", "Failure", 404);
                    }

                    // Insert into tblQuestionProfilerApproval
                    string insertApprovalSql = @"
            INSERT INTO [tblQuestionProfilerApproval]
            ([QuestionId], [ApprovedBy], [ApprovedDate], QuestionCode)
            VALUES (@QuestionId, @ApprovedBy, @ApprovedDate, @QuestionCode)";

                    var insertApprovalParameters = new
                    {
                        QuestionId = questionId,
                        request.ApprovedBy,
                        ApprovedDate = request.ApprovedDate ?? DateTime.UtcNow,
                        QuestionCode = request.QuestionCode
                    };

                    await _connection.ExecuteAsync(insertApprovalSql, insertApprovalParameters, transaction);

                    // Update tblQuestionProfiler to set status of the current profiler to inactive
                    string updateProfilerSql = @"
            UPDATE tblQuestionProfiler
            SET Status = 0
            WHERE Status = 1 AND QuestionCode = @QuestionCode";

                    await _connection.ExecuteAsync(updateProfilerSql, new { request.QuestionCode }, transaction);

                    // Commit the transaction
                    transaction.Commit();
                }

                return new ServiceResponse<string>(true, "Question approved successfully", "Success", 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
            }
        }
        public async Task<ServiceResponse<string>> AssignQuestionToProfiler(QuestionProfilerRequest request)
        {
            try
            {
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }

                using (var transaction = _connection.BeginTransaction())
                {
                    // Check if the question is already assigned to a profiler with active status based on QuestionCode
                    string checkSql = @"
                SELECT QPID
                FROM tblQuestionProfiler
                WHERE QuestionCode = @QuestionCode AND Status = 1";

                    var existingProfiler = await _connection.QueryFirstOrDefaultAsync<int?>(checkSql, new { request.QuestionCode }, transaction);

                    // If the question is already assigned, update the status of the current profiler to false
                    if (existingProfiler.HasValue)
                    {
                        string updateSql = @"
                    UPDATE tblQuestionProfiler
                    SET Status = 0
                    WHERE QPID = @QPID";

                        await _connection.ExecuteAsync(updateSql, new { QPID = existingProfiler.Value }, transaction);
                    }

                    // Fetch the QuestionId from the main table using QuestionCode and IsActive = 1
                    string fetchQuestionIdSql = @"
                SELECT QuestionId
                FROM tblQuestion
                WHERE QuestionCode = @QuestionCode AND IsActive = 1";

                    var questionId = await _connection.QueryFirstOrDefaultAsync<int?>(fetchQuestionIdSql, new { request.QuestionCode }, transaction);

                    if (!questionId.HasValue)
                    {
                        return new ServiceResponse<string>(false, "Question not found or inactive", string.Empty, 404);
                    }

                    // Update the tblQuestion to set IsRejected and IsApproved to 0
                    string updateQuestionSql = @"
                UPDATE tblQuestion
                SET IsRejected = 0, IsApproved = 0
                WHERE QuestionId = @QuestionId";

                    await _connection.ExecuteAsync(updateQuestionSql, new { QuestionId = questionId.Value }, transaction);

                    // Insert a new record for the new profiler with ApprovedStatus = false and Status = true
                    string insertSql = @"
                INSERT INTO tblQuestionProfiler (Questionid, QuestionCode, EmpId, ApprovedStatus, Status, AssignedDate)
                VALUES (@Questionid, @QuestionCode, @EmpId, 0, 1, @AssignedDate)";

                    await _connection.ExecuteAsync(insertSql, new { Questionid = questionId.Value, request.QuestionCode, request.EmpId, AssignedDate = DateTime.Now }, transaction);

                    // Commit the transaction
                    transaction.Commit();
                }

                return new ServiceResponse<string>(true, "Question successfully assigned to profiler", string.Empty, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
            finally
            {
                _connection.Close();
            }
        }
        public async Task<ServiceResponse<List<ContentIndexResponses>>> GetSyllabusDetailsBySubject(SyllabusDetailsRequest request)
        {
            try
            {
                // SQL Query
                string sql = @"
            SELECT sd.*, s.*
            FROM [iGuruPrep].[dbo].[tblSyllabus] s
            JOIN [iGuruPrep].[dbo].[tblSyllabusDetails] sd ON s.SyllabusId = sd.SyllabusID
            WHERE s.APID = @APId
            AND (s.BoardID = @BoardId OR @BoardId = 0)
            AND (s.ClassId = @ClassId OR @ClassId = 0)
            AND (s.CourseId = @CourseId OR @CourseId = 0)
            AND (sd.SubjectId = @SubjectId OR @SubjectId = 0)";

                var syllabusDetails = await _connection.QueryAsync<dynamic>(sql, new
                {
                    request.APId,
                    request.BoardId,
                    request.ClassId,
                    request.CourseId,
                    request.SubjectId
                });

                // Process the results to create a hierarchical structure
                var contentIndexResponse = new List<ContentIndexResponses>();

                foreach (var detail in syllabusDetails)
                {
                    int indexTypeId = detail.IndexTypeId;

                    // Query to count questions
                    string countQuestionsSql = @"
                SELECT COUNT(*) 
                FROM [iGuruPrep].[dbo].[tblQuestion] 
                WHERE IndexTypeId = @IndexTypeId AND ContentIndexId = @ContentIndexId AND IsActive = 1";

                    int questionCount = await _connection.ExecuteScalarAsync<int>(countQuestionsSql, new
                    {
                        IndexTypeId = indexTypeId,
                        ContentIndexId = detail.ContentIndexId
                    });

                    if (indexTypeId == 1) // Chapter
                    {
                        string getchapter = @"SELECT * FROM tblContentIndexChapters WHERE ContentIndexId = @ContentIndexId;";
                        var data = await _connection.QueryFirstOrDefaultAsync<ContentIndexResponses>(getchapter, new { ContentIndexId = detail.ContentIndexId });

                        var chapter = new ContentIndexResponses
                        {
                            ContentIndexId = data.ContentIndexId,
                            SubjectId = data.SubjectId,
                            ContentName_Chapter = data.ContentName_Chapter,
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
                            DisplayName = data.DisplayName,
                            DisplayOrder = data.DisplayOrder,
                            Count = questionCount, // Adding question count here
                            ContentIndexTopics = new List<ContentIndexTopicsResponse>()
                        };

                        // Add to response list
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
                            ContentName_Topic = data.ContentName_Topic,
                            Status = data.Status,
                            IndexTypeId = indexTypeId,
                            CreatedOn = data.CreatedOn,
                            CreatedBy = data.CreatedBy,
                            ModifiedOn = data.ModifiedOn,
                            ModifiedBy = data.ModifiedBy,
                            EmployeeId = data.EmployeeId,
                            IsActive = data.Status,
                            TopicCode = data.TopicCode,
                            DisplayName = data.DisplayName,
                            DisplayOrder = data.DisplayOrder,
                            ChapterCode = data.ChapterCode,
                            Count = questionCount, // Adding question count here
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
                            ContentName_SubTopic = data.ContentName_SubTopic,
                            Status = data.Status,
                            IndexTypeId = indexTypeId,
                            CreatedOn = data.CreatedOn,
                            CreatedBy = data.CreatedBy,
                            ModifiedOn = data.ModifiedOn,
                            ModifiedBy = data.ModifiedBy,
                            EmployeeId = data.EmployeeId,
                            IsActive = data.Status,
                            SubTopicCode = data.SubTopicCode,
                            DisplayName = data.DisplayName,
                            DisplayOrder = data.DisplayOrder,
                            TopicCode = data.TopicCode,
                            Count = questionCount // Adding question count here
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

                return new ServiceResponse<List<ContentIndexResponses>>(true, "Syllabus details retrieved successfully", contentIndexResponse, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ContentIndexResponses>>(false, ex.Message, new List<ContentIndexResponses>(), 500);
            }
        }
        //public async Task<ServiceResponse<string>> AssignQuestionToProfiler(QuestionProfilerRequest request)
        //{
        //    try
        //    {
        //        if (_connection.State != ConnectionState.Open)
        //        {
        //            _connection.Open();
        //        }
        //        using (var transaction = _connection.BeginTransaction())
        //        {
        //            // Check if the question is already assigned to a profiler with active status based on QuestionCode
        //            string checkSql = @"
        //    SELECT QPID
        //    FROM tblQuestionProfiler
        //    WHERE QuestionCode = @QuestionCode AND Status = 1";

        //            var existingProfiler = await _connection.QueryFirstOrDefaultAsync<int?>(checkSql, new { request.QuestionCode }, transaction);

        //            // If the question is already assigned, update the status of the current profiler to false
        //            if (existingProfiler.HasValue)
        //            {
        //                string updateSql = @"
        //        UPDATE tblQuestionProfiler
        //        SET Status = 0
        //        WHERE QPID = @QPID";

        //                await _connection.ExecuteAsync(updateSql, new { QPID = existingProfiler.Value }, transaction);
        //            }
        //            // Fetch the Questionid from the main table using QuestionCode and IsActive = 1
        //            string fetchQuestionIdSql = @"
        //    SELECT QuestionId
        //    FROM tblQuestion
        //    WHERE QuestionCode = @QuestionCode AND IsActive = 1";

        //            var questionId = await _connection.QueryFirstOrDefaultAsync<int?>(fetchQuestionIdSql, new { request.QuestionCode }, transaction);

        //            if (!questionId.HasValue)
        //            {
        //                return new ServiceResponse<string>(false, "Question not found or inactive", string.Empty, 404);
        //            }
        //            // Insert a new record for the new profiler with ApprovedStatus = false and Status = true
        //            string insertSql = @"
        //    INSERT INTO tblQuestionProfiler (Questionid, QuestionCode, EmpId, ApprovedStatus, Status, AssignedDate)
        //    VALUES (@Questionid, @QuestionCode, @EmpId, 0, 1, @AssignedDate)";

        //            await _connection.ExecuteAsync(insertSql, new { Questionid = questionId.Value, request.QuestionCode, request.EmpId, AssignedDate = DateTime.Now }, transaction);

        //            // Commit the transaction
        //            transaction.Commit();
        //        }

        //        return new ServiceResponse<string>(true, "Question successfully assigned to profiler", string.Empty, 200);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
        //    }
        //    finally
        //    {
        //        _connection.Close();
        //    }
        //}
        public async Task<ServiceResponse<QuestionProfilerResponse>> GetQuestionProfilerDetails(string QuestionCode)
        {
            try
            {

                string fetchQuestionIdSql = @"
            SELECT QuestionId
            FROM tblQuestion
            WHERE QuestionCode = @QuestionCode AND IsActive = 1"
                ;

                var questionId = await _connection.QueryFirstOrDefaultAsync<int?>(fetchQuestionIdSql, new { QuestionCode });

                if (!questionId.HasValue)
                {
                    return new ServiceResponse<QuestionProfilerResponse>(false, "Question not found or inactive", new QuestionProfilerResponse(), 404);
                }

                string sql = @"
        SELECT qp.QPID, qp.Questionid, qp.EmpId, qp.ApprovedStatus, qp.AssignedDate,
               e.EmpFirstName + ' ' + e.EmpLastName AS EmpName, r.RoleName AS Role, r.RoleID,
               qr.QuestionProfilerRejectionsid AS RejectionId, qr.CreatedDate AS RejectedDate, 
               qr.QuestionRejectReason AS RejectedReason, qr.RejectedBy,
               c.CourseName, c.CourseId, qc.QIDCourseID, qc.LevelId, l.LevelName,
               qc.Status, qc.CreatedBy, qc.CreatedDate, qc.ModifiedBy, qc.ModifiedDate
        FROM tblQuestionProfiler qp
        LEFT JOIN tblEmployee e ON qp.EmpId = e.Employeeid
        LEFT JOIN tblRole r ON e.RoleID = r.RoleID
        LEFT JOIN tblQuestionProfilerRejections qr ON qp.Questionid = qr.Questionid
        LEFT JOIN tblQIDCourse qc ON qp.Questionid = qc.QID
        LEFT JOIN tblCourse c ON qc.CourseID = c.CourseId
        LEFT JOIN tbldifficultylevel l ON qc.LevelId = l.LevelId
        WHERE qp.QuestionCode = @QuestionCode";

                var parameters = new { QuestionCode };

                var data = (await _connection.QueryAsync<dynamic>(sql, parameters)).ToList();

                if (data != null && data.Any())
                {
                    var firstRecord = data.First();

                    var response = new QuestionProfilerResponse
                    {
                        QPID = firstRecord.QPID,
                        Questionid = firstRecord.Questionid,
                        ApprovedStatus = firstRecord.ApprovedStatus,
                        Proofers = data.GroupBy(d => new { d.EmpId, d.EmpName, d.Role, d.RoleID })
                                       .Select(g => g.First())
                                       .Select(g => new ProoferList
                                       {
                                           QPId = g.QPID,
                                           EmpId = g.EmpId,
                                           EmpName = g.EmpName,
                                           Role = g.Role,
                                           RoleId = g.RoleID,
                                           QuestionCode = g.QuestionCode,
                                           AssignedDate = g.AssignedDate,
                                       }).ToList(),
                        QIDCourses = data.GroupBy(d => new { d.QIDCourseID, d.CourseId, d.CourseName, d.LevelId, d.LevelName, d.Status, d.CreatedBy, d.CreatedDate, d.ModifiedBy, d.ModifiedDate })
                                         .Select(g => g.First())
                                         .Select(g => new QIDCourseResponse
                                         {
                                             QIDCourseID = g.QIDCourseID,
                                             QID = firstRecord.QPID,
                                             CourseID = g.CourseId,
                                             CourseName = g.CourseName,
                                             LevelId = g.LevelId,
                                             LevelName = g.LevelName,
                                             Status = g.Status,
                                             CreatedBy = g.CreatedBy,
                                             CreatedDate = g.CreatedDate,
                                             ModifiedBy = g.ModifiedBy,
                                             ModifiedDate = g.ModifiedDate,
                                             QuestionCode = g.QuestionsCode
                                         }).ToList(),
                        QuestionRejectionResponseDTOs = data.Where(d => d.RejectionId != null)
                                                            .GroupBy(d => new { d.RejectionId, d.Questionid, d.RejectedBy, d.RejectedDate, d.RejectedReason })
                                                            .Select(g => g.First())
                                                            .Select(g => new QuestionRejectionResponseDTO
                                                            {
                                                                RejectionId = g.RejectionId,
                                                                QuestionId = g.Questionid,
                                                                EmpId = g.RejectedBy,
                                                                EmpName = g.EmpName,
                                                                RejectedDate = g.RejectedDate,
                                                                RejectedReason = g.RejectedReason,
                                                                QuestionCode = g.QuestionCode,
                                                            }).ToList()
                    };

                    return new ServiceResponse<QuestionProfilerResponse>(true, "Operation Successful", response, 200);
                }
                else
                {
                    return new ServiceResponse<QuestionProfilerResponse>(false, "No records found", new QuestionProfilerResponse(), 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<QuestionProfilerResponse>(false, ex.Message, new QuestionProfilerResponse(), 500);
            }
        }
        public async Task<ServiceResponse<object>> CompareQuestionVersions(string questionCode)
        {
            try
            {
                string sql = @"
        SELECT q.*, 
               c.CourseName, 
               b.BoardName, 
               cl.ClassName, 
               s.SubjectName,
               et.ExamTypeName,
               e.EmpFirstName,
               qt.QuestionType as QuestionTypeName,
               it.IndexType as IndexTypeName,
               CASE 
                   WHEN q.IndexTypeId = 1 THEN ci.ContentName_Chapter
                   WHEN q.IndexTypeId = 2 THEN ct.ContentName_Topic
                   WHEN q.IndexTypeId = 3 THEN cst.ContentName_SubTopic
               END AS ContentIndexName,
               q.IsActive
        FROM tblQuestion q
        LEFT JOIN tblQBQuestionType qt ON q.QuestionTypeId = qt.QuestionTypeID
        LEFT JOIN tblCourse c ON q.courseid = c.CourseID
        LEFT JOIN tblBoard b ON q.boardid = b.BoardID
        LEFT JOIN tblClass cl ON q.classid = cl.ClassID
        LEFT JOIN tblSubject s ON q.subjectID = s.SubjectID
        LEFT JOIN tblExamType et ON q.ExamTypeId = et.ExamTypeId
        LEFT JOIN tblEmployee e ON q.EmployeeId = e.EmployeeId
        LEFT JOIN tblQBIndexType it ON q.IndexTypeId = it.IndexId
        LEFT JOIN tblContentIndexChapters ci ON q.ContentIndexId = ci.ContentIndexId AND q.IndexTypeId = 1
        LEFT JOIN tblContentIndexTopics ct ON q.ContentIndexId = ct.ContInIdTopic AND q.IndexTypeId = 2
        LEFT JOIN tblContentIndexSubTopics cst ON q.ContentIndexId = cst.ContInIdSubTopic AND q.IndexTypeId = 3
        WHERE q.QuestionCode = @QuestionCode
        ORDER BY q.ModifiedOn DESC";

                var parameters = new { QuestionCode = questionCode };

                var items = (await _connection.QueryAsync<dynamic>(sql, parameters)).ToList();

                var inactiveItems = items.Where(x => x.IsActive == false).ToList();
                var activeItems = items.Where(x => x.IsActive == true).ToList();

                if (inactiveItems.Count == 0 || activeItems.Count == 0)
                {
                    return new ServiceResponse<object>(false, "Required versions not found", null, 404);
                }

                var originalVersion = await CreateQuestionResponseDTO(inactiveItems.First());
                var finalVersion = await CreateQuestionResponseDTO(activeItems.First());

                var response = new
                {
                    OriginalVersion = originalVersion,
                    FinalVersion = finalVersion
                };

                return new ServiceResponse<object>(true, "Operation Successful", response, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<object>(false, ex.Message, null, 500);
            }
        }
        private double CalculateSimilarity(string question1, string question2)
        {
            int maxLen = Math.Max(question1.Length, question2.Length);
            if (maxLen == 0) return 100.0;

            int distance = ComputeLevenshteinDistance(question1, question2);
            return (1.0 - (double)distance / maxLen) * 100;
        }
        private int ComputeLevenshteinDistance(string s, string t)
        {
            int[,] d = new int[s.Length + 1, t.Length + 1];

            // Step 1: Initialize the matrix
            for (int i = 0; i <= s.Length; i++)
            {
                d[i, 0] = i;
            }
            for (int j = 0; j <= t.Length; j++)
            {
                d[0, j] = j;
            }

            // Step 2: Fill the matrix
            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[s.Length, t.Length];
        }
        private async Task<int> AddUpdateQIDCourses(List<QIDCourse>? request, string questionCode)
        {
            int rowsAffected = 0;
            if (request != null)
            {
                // Use questionCode to get questionId
                string getQuestionIdQuery = "SELECT QuestionID FROM tblQuestion WHERE QuestionCode = @QuestionCode AND IsActive = 1";
                int questionId = await _connection.QuerySingleOrDefaultAsync<int>(getQuestionIdQuery, new { QuestionCode = questionCode });

                if (questionId > 0)
                {
                    foreach (var data in request)
                    {
                        var newQIDCourse = new QIDCourse
                        {
                            QID = questionId,
                            CourseID = data.CourseID,
                            LevelId = data.LevelId,
                            Status = true,
                            CreatedBy = 1,
                            CreatedDate = DateTime.Now,
                            ModifiedBy = 1,
                            ModifiedDate = DateTime.Now,
                            QIDCourseID = data.QIDCourseID,
                            QuestionCode = questionCode
                        };
                        if (data.QIDCourseID == 0)
                        {
                            string insertQuery = @"
                            INSERT INTO tblQIDCourse (QID, CourseID, LevelId, Status, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, QuestionCode)
                            VALUES (@QID, @CourseID, @LevelId, @Status, @CreatedBy, @CreatedDate, @ModifiedBy, @ModifiedDate, @QuestionCode)";

                            rowsAffected = await _connection.ExecuteAsync(insertQuery, newQIDCourse);
                        }
                        else
                        {
                            string updateQuery = @"
                           UPDATE tblQIDCourse
                           SET QID = @QID,
                               CourseID = @CourseID,
                               LevelId = @LevelId,
                               Status = @Status,
                               CreatedBy = @CreatedBy,
                               CreatedDate = @CreatedDate,
                               ModifiedBy = @ModifiedBy,
                               ModifiedDate = @ModifiedDate,
                               QuestionCode = @QuestionCode
                           WHERE QIDCourseID = @QIDCourseID";
                            rowsAffected = await _connection.ExecuteAsync(updateQuery, newQIDCourse);
                        }
                    }
                    return rowsAffected;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
        private async Task<int> AddUpdateReference(Reference? request, int questionId)
        {
            if (request != null)
            {
                var newReference = new Reference
                {
                    ReferenceNotes = request.ReferenceNotes,
                    ReferenceURL = request.ReferenceURL,
                    QuestionId = questionId,
                    Status = true,
                    CreatedBy = 1,
                    CreatedOn = DateTime.Now,
                    ModifiedBy = 1,
                    ModifiedOn = DateTime.Now,
                    ReferenceId = request.ReferenceId,
                };
                int rowsAffected;
                if (request.ReferenceId == 0)
                {
                    string insertQuery = @"
                    INSERT INTO tblReference (ReferenceNotes, ReferenceURL, QuestionId,
                                            Status, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
                    VALUES (@ReferenceNotes, @ReferenceURL, @QuestionId,
                            @Status, @CreatedBy, @CreatedOn, @ModifiedBy, @ModifiedOn)";

                    rowsAffected = await _connection.ExecuteAsync(insertQuery, newReference);
                }
                else
                {
                    string updateQuery = @"
                    UPDATE tblReference
                    SET ReferenceNotes = @ReferenceNotes,
                        ReferenceURL = @ReferenceURL,
                        QuestionId = @QuestionId,
                        Status = @Status,
                        CreatedBy = @CreatedBy,
                        CreatedOn = @CreatedOn,
                        ModifiedBy = @ModifiedBy,
                        ModifiedOn = @ModifiedOn
                    WHERE ReferenceId = @ReferenceId";
                    rowsAffected = await _connection.ExecuteAsync(updateQuery, newReference);
                }
                return rowsAffected;
            }
            else
            {
                return 0;
            }
        }
        private async Task<int> AddUpdateQuestionSubjectMap(List<QuestionSubjectMapping>? request, string questionCode)
        {
            if (request != null)
            {
                // Use questionCode to get questionId
                string getQuestionIdQuery = "SELECT QuestionID FROM tblQuestion WHERE QuestionCode = @QuestionCode AND IsActive = 1";
                int questionId = await _connection.QuerySingleOrDefaultAsync<int>(getQuestionIdQuery, new { QuestionCode = questionCode });

                if (questionId > 0)
                {
                    foreach (var data in request)
                    {
                        data.QuestionCode = questionCode;
                        data.questionid = questionId;
                    }
                    string query = "SELECT COUNT(*) FROM [tblQuestionSubjectMapping] WHERE [questionid] = @questionId";
                    int count = await _connection.QueryFirstOrDefaultAsync<int>(query, new { questionId });
                    if (count > 0)
                    {
                        var deleteDuery = @"DELETE FROM [tblQuestionSubjectMapping] WHERE [questionid] = @questionId;";
                        var rowsAffected = await _connection.ExecuteAsync(deleteDuery, new { questionId });
                        if (rowsAffected > 0)
                        {
                            string insertQuery = @"INSERT INTO tblQuestionSubjectMapping (ContentIndexId, Indexid, questionid, QuestionCode) 
                            VALUES (@ContentIndexId, @Indexid, @questionid, @QuestionCode)";
                            var valuesInserted = await _connection.ExecuteAsync(insertQuery, request);
                            return valuesInserted;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                    else
                    {
                        string insertQuery = @"INSERT INTO tblQuestionSubjectMapping (ContentIndexId, Indexid, questionid, QuestionCode) 
                        VALUES (@ContentIndexId, @Indexid, @questionid, @QuestionCode)";
                        var valuesInserted = await _connection.ExecuteAsync(insertQuery, request);
                        return valuesInserted;
                    }
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
        private List<QIDCourseResponse> GetListOfQIDCourse(string QuestionCode)
        {
            // Get active question IDs
            var activeQuestionIds = GetActiveQuestionIds(QuestionCode);

            // If no active question IDs found, return an empty list
            if (!activeQuestionIds.Any())
            {
                return new List<QIDCourseResponse>();
            }

            var query = @"
    SELECT qc.*, c.CourseName, l.LevelName
    FROM [tblQIDCourse] qc
    LEFT JOIN tblCourse c ON qc.CourseID = c.CourseID
    LEFT JOIN tbldifficultylevel l ON qc.LevelId = l.LevelId
    WHERE qc.QuestionCode = @QuestionCode
      AND qc.QID IN @ActiveQuestionIds";

            var data = _connection.Query<QIDCourseResponse>(query, new { QuestionCode, ActiveQuestionIds = activeQuestionIds });
            return data.ToList();
        }
        private Reference GetQuestionReference(int questionId)
        {
            var boardquery = @"SELECT * FROM [tblReference] WHERE QuestionId = @questionId;";

            var data = _connection.QueryFirstOrDefault<Reference>(boardquery, new { questionId });
            return data ?? new Reference();
        }
        private List<QuestionSubjectMappingResponse> GetListOfQuestionSubjectMapping(string QuestionCode)
        {
            // Get active question IDs
            var activeQuestionIds = GetActiveQuestionIds(QuestionCode);

            // If no active question IDs found, return an empty list
            if (!activeQuestionIds.Any())
            {
                return new List<QuestionSubjectMappingResponse>();
            }

            var boardquery = @"
            SELECT qsm.*, it.IndexType as IndexTypeName,
            CASE 
                WHEN qsm.Indexid = 1 THEN ci.ContentName_Chapter
                WHEN qsm.Indexid = 2 THEN ct.ContentName_Topic
                WHEN qsm.Indexid = 3 THEN cst.ContentName_SubTopic
            END AS ContentIndexName
            FROM [tblQuestionSubjectMapping] qsm
            LEFT JOIN tblQBIndexType it ON qsm.Indexid = it.IndexId
            LEFT JOIN tblContentIndexChapters ci ON qsm.ContentIndexId = ci.ContentIndexId AND qsm.Indexid = 1
            LEFT JOIN tblContentIndexTopics ct ON qsm.ContentIndexId = ct.ContInIdTopic AND qsm.Indexid = 2
            LEFT JOIN tblContentIndexSubTopics cst ON qsm.ContentIndexId = cst.ContInIdSubTopic AND qsm.Indexid = 3
            WHERE qsm.QuestionCode = @QuestionCode
              AND qsm.questionid IN @ActiveQuestionIds";

            var data = _connection.Query<QuestionSubjectMappingResponse>(boardquery, new { QuestionCode, ActiveQuestionIds = activeQuestionIds });
            return data.ToList();
        }
        private Answersingleanswercategory GetSingleAnswer(string QuestionCode)
        {
            var answerMaster = _connection.QueryFirstOrDefault<AnswerMaster>(@"
        SELECT * FROM tblAnswerMaster WHERE QuestionCode = @QuestionCode", new { QuestionCode });

            if (answerMaster != null)
            {
                string getQuery = @"
            SELECT * FROM [tblAnswersingleanswercategory] WHERE [Answerid] = @Answerid";

                var response = _connection.QueryFirstOrDefault<Answersingleanswercategory>(getQuery, new { answerMaster.Answerid });
                return response ?? new Answersingleanswercategory();
            }
            else
            {
                return new Answersingleanswercategory();
            }
        }
        private List<AnswerMultipleChoiceCategory> GetMultipleAnswers(string QuestionCode)
        {
            var answerMaster = _connection.QueryFirstOrDefault<AnswerMaster>(@"
        SELECT * FROM tblAnswerMaster WHERE QuestionCode = @QuestionCode", new { QuestionCode });

            if (answerMaster != null)
            {
                string getQuery = @"
            SELECT * FROM [tblAnswerMultipleChoiceCategory] WHERE [Answerid] = @Answerid";

                var response = _connection.Query<AnswerMultipleChoiceCategory>(getQuery, new { answerMaster.Answerid });
                return response.AsList() ?? new List<AnswerMultipleChoiceCategory>();
            }
            else
            {
                return new List<AnswerMultipleChoiceCategory>();
            }
        }
        private async Task<QuestionResponseDTO> CreateQuestionResponseDTO(dynamic item)
        {
            var questionResponse = new QuestionResponseDTO
            {
                QuestionId = item.QuestionId,
                QuestionDescription = item.QuestionDescription,
                SubjectName = item.SubjectName,
                EmployeeName = item.EmpFirstName,
                IndexTypeName = item.IndexTypeName,
                ContentIndexName = item.ContentIndexName,
                QIDCourses = GetListOfQIDCourse(item.QuestionCode),
                QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
                ContentIndexId = item.ContentIndexId,
                CreatedBy = item.CreatedBy,
                CreatedOn = item.CreatedOn,
                EmployeeId = item.EmployeeId,
                IndexTypeId = item.IndexTypeId,
                subjectID = item.subjectID,
                ModifiedOn = item.ModifiedOn,
                QuestionTypeId = item.QuestionTypeId,
                QuestionTypeName = item.QuestionTypeName,
                QuestionCode = item.QuestionCode,
                Explanation = item.Explanation,
                ExtraInformation = item.ExtraInformation,
                IsActive = item.IsActive,
                Answersingleanswercategories = GetSingleAnswer(item.QuestionCode),
                AnswerMultipleChoiceCategories = GetMultipleAnswers(item.QuestionCode)
            };
            return questionResponse;
        }
        private List<int> GetActiveQuestionIds(string QuestionCode)
        {
            var query = @"
            SELECT q.QuestionId
            FROM tblQuestion q
            WHERE q.QuestionCode = @QuestionCode
              AND q.IsActive = 1";

            var questionIds = _connection.Query<int>(query, new { QuestionCode });
            return questionIds.ToList();
        }
        private string GenerateCode()
        {
            return DateTime.Now.ToString("yyyyMMddHHmmssff");
        }
        private string? GetRoleName(int roleId)
        {
            // Fetch role details based on the roleId
            var role = _connection.QuerySingleOrDefault<dynamic>("SELECT RoleName FROM tblRole WHERE RoleID = @RoleId", new { RoleId = roleId });
            return role?.RoleName; // Return the role name or null if not found
        }
    }
}