using Dapper;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Schools_API.DTOs.Requests;
using Schools_API.DTOs.Response;
using Schools_API.DTOs.ServiceResponse;
using Schools_API.Models;
using Schools_API.Repository.Interfaces;
using System.Data;
using System.Data.SqlClient;
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
                if (!string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode != "string")
                {
                    //request.QuestionCode = GenerateCode();
                    // Check for existing entries with the same QuestionCode and deactivate them
                    string deactivateQuery = @"
                UPDATE tblQuestion
                SET IsActive = 0
                WHERE QuestionCode = @QuestionCode AND IsActive = 1";

                    await _connection.ExecuteAsync(deactivateQuery, new { request.QuestionCode });

                }
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
  
  -- Fetch the QuestionId of the newly inserted row
  SELECT CAST(SCOPE_IDENTITY() AS INT);";

                //string insertQuery = @"
                //INSERT INTO tblQuestion (
                //    QuestionDescription,
                //    QuestionTypeId,
                //    Status,
                //    CreatedBy,
                //    CreatedOn,
                //    subjectID,
                //    EmployeeId,
                //    IndexTypeId,
                //    ContentIndexId,
                //    IsRejected,
                //    IsApproved,
                //    QuestionCode,
                //    Explanation,
                //    ExtraInformation,
                //    IsActive
                //) VALUES (
                //    @QuestionDescription,
                //    @QuestionTypeId,
                //    @Status,
                //    @CreatedBy,
                //    @CreatedOn,
                //    @subjectID,
                //    @EmployeeId,
                //    @IndexTypeId,
                //    @ContentIndexId,
                //    @IsRejected,
                //    @IsApproved,
                //    @QuestionCode,
                //    @Explanation,
                //    @ExtraInformation,
                //    @IsActive
                //);
                //SELECT QuestionCode FROM tblQuestion WHERE QuestionCode = @QuestionCode AND IsActive = 1;";

                // Retrieve the QuestionCode after insertion
                // var insertedQuestionCode = await _connection.QuerySingleOrDefaultAsync<string>(insertQuery, question);
                var insertedQuestionId = await _connection.QuerySingleOrDefaultAsync<int>(insertQuery, question);
                string code = string.Empty;
                if (string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string")
                {
                    code = GenerateQuestionCode(request.IndexTypeId, request.ContentIndexId, insertedQuestionId);

                    string questionCodeQuery = @"
                UPDATE tblQuestion
                SET QuestionCode = @QuestionCode
                WHERE QuestionId = @QuestionId AND IsActive = 1";

                    await _connection.ExecuteAsync(questionCodeQuery, new { QuestionCode = code, QuestionId = insertedQuestionId });
                }
                string insertedQuestionCode = string.IsNullOrEmpty(request.QuestionCode) || request.QuestionCode == "string" ? code : request.QuestionCode;

                if (!string.IsNullOrEmpty(insertedQuestionCode))
                {
                    // Handle QIDCourses mapping
                    var data = await AddUpdateQIDCourses(request.QIDCourses, insertedQuestionCode);

                    // Handle Answer mappings
                    string getQuesType = @"SELECT * FROM tblQBQuestionType WHERE QuestionTypeID = @QuestionTypeID;";
                    var questTypedata = await _connection.QueryFirstOrDefaultAsync<QuestionTypes>(getQuesType, new { QuestionTypeID = request.QuestionTypeId });

                    int answer = 0;
                    int Answerid = 0;

                    // Check if the answer already exists in AnswerMaster
                    string getAnswerQuery = @"SELECT Answerid FROM tblAnswerMaster WHERE QuestionCode = @QuestionCode;";
                    Answerid = await _connection.QueryFirstOrDefaultAsync<int>(getAnswerQuery, new { QuestionCode = insertedQuestionCode });

                    if (Answerid == 0)  // If no entry exists, insert a new one
                    {
                        string insertAnswerQuery = @"
        INSERT INTO [tblAnswerMaster] (Questionid, QuestionTypeid, QuestionCode)
        VALUES (@Questionid, @QuestionTypeid, @QuestionCode);
        SELECT CAST(SCOPE_IDENTITY() as int);";

                        Answerid = await _connection.QuerySingleAsync<int>(insertAnswerQuery, new
                        {
                            Questionid = 0, // Set to 0 or remove if QuestionId is not required
                            QuestionTypeid = questTypedata?.QuestionTypeID,
                            QuestionCode = insertedQuestionCode
                        });
                    }

                    // If the question type supports multiple-choice or similar categories
                    if (questTypedata != null)
                    {
                        if (questTypedata.Code.Trim() == "MCQ" || questTypedata.Code.Trim() == "TF" || questTypedata.Code.Trim() == "MT1" ||
                            questTypedata.Code.Trim() == "MAQ" || questTypedata.Code.Trim() == "MT2" || questTypedata.Code.Trim() == "AR" || questTypedata.Code.Trim() == "C")
                        {
                            if (request.AnswerMultipleChoiceCategories != null)
                            {
                                // First, delete existing multiple-choice entries if present
                                string deleteMCQQuery = @"DELETE FROM tblAnswerMultipleChoiceCategory WHERE Answerid = @Answerid;";
                                await _connection.ExecuteAsync(deleteMCQQuery, new { Answerid });

                                // Insert new multiple-choice answers
                                foreach (var item in request.AnswerMultipleChoiceCategories)
                                {
                                    item.Answerid = Answerid;
                                }
                                string insertMCQQuery = @"
                    INSERT INTO tblAnswerMultipleChoiceCategory
                    (Answerid, Answer, Iscorrect, Matchid) 
                    VALUES (@Answerid, @Answer, @Iscorrect, @Matchid);";
                                answer = await _connection.ExecuteAsync(insertMCQQuery, request.AnswerMultipleChoiceCategories);
                            }
                        }
                        else  // Handle single-answer category
                        {
                            string sql = @"
                INSERT INTO tblAnswersingleanswercategory (Answerid, Answer)
                VALUES (@Answerid, @Answer);";

                            if (request.Answersingleanswercategories != null)
                            {
                                // First, delete existing single-answer entries if present
                                string deleteSingleQuery = @"DELETE FROM tblAnswersingleanswercategory WHERE Answerid = @Answerid;";
                                await _connection.ExecuteAsync(deleteSingleQuery, new { Answerid });

                                // Insert new single-answer answers
                                request.Answersingleanswercategories.Answerid = Answerid;
                                answer = await _connection.ExecuteAsync(sql, request.Answersingleanswercategories);
                            }
                        }
                    }

                    if (data > 0 && answer > 0)
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
                        //QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
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
        public async Task<ServiceResponse<List<EmployeeListAssignedQuestionCount>>> GetAssignedQuestionsCount(int EmployeeId, int SubjectId)
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
                                               //var roleId1 = await _connection.QueryFirstOrDefaultAsync<int>(sql, new { RoleCode = "TR" });
                                               //targetRoleIDs.Add(roleId1); // Assume RoleID for Transcriber is 5
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

                // SQL query to fetch employees and their assigned question counts based on SubjectId
                string assignedQuestionsCountQuery = @"
        SELECT e.EmployeeId, 
               CONCAT(e.EmpFirstName, ' ', e.EmpMiddleName, ' ', e.EmpLastName) AS EmployeeName,
               COUNT(qp.Questionid) AS Count
        FROM [tblEmployee] e
        LEFT JOIN [tblQuestionProfiler] qp ON e.EmployeeId = qp.EmpId AND qp.Status = 1
        JOIN [tblEmployeeSubject] es ON e.EmployeeId = es.Employeeid
        WHERE e.RoleID IN @TargetRoleIDs
        AND es.SubjectID = @SubjectId
        GROUP BY e.EmployeeId, e.EmpFirstName, e.EmpMiddleName, e.EmpLastName
        ORDER BY EmployeeName";

                // Execute query to get the list of employees with the count of assigned questions, filtered by SubjectId
                employeeList = (await _connection.QueryAsync<EmployeeListAssignedQuestionCount>(
                    assignedQuestionsCountQuery, new { TargetRoleIDs = targetRoleIDs, SubjectId }
                )).ToList();

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
                        //QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
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
                   // question.QuestionSubjectMappings = GetListOfQuestionSubjectMapping(question.QuestionCode);
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
                        //QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
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
                        //QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
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
                        //QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
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
            ([Questionid], [CreatedDate], [QuestionRejectReason], [RejectedBy], QuestionCode, FileUpload)
            VALUES (@QuestionId, @CreatedDate, @QuestionRejectReason, @RejectedBy, @QuestionCode, @FileUpload);

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
                            request.QuestionCode,
                            FileUpload = FileUpload(request.FileUpload)
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
            FROM [tblSyllabus] s
            JOIN [tblSyllabusDetails] sd ON s.SyllabusId = sd.SyllabusID
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
                FROM [tblQuestion] 
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
        public async Task<ServiceResponse<byte[]>> GenerateExcelFile(DownExcelRequest request)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                // Create an ExcelPackage
                using (var package = new ExcelPackage())
                {
                    // Create a worksheet for Questions
                    var worksheet = package.Workbook.Worksheets.Add("Questions");

                    // Add headers for Questions
                    worksheet.Cells[1, 1].Value = "SubjectName";
                    worksheet.Cells[1, 2].Value = "ChapterName";
                    worksheet.Cells[1, 3].Value = "ConceptName"; // Only relevant if IndexTypeId >= 2
                    worksheet.Cells[1, 4].Value = "SubConceptName"; // Only relevant if IndexTypeId == 3
                    worksheet.Cells[1, 5].Value = "QuestionType";
                    worksheet.Cells[1, 6].Value = "QuestionDescription";
                    worksheet.Cells[1, 7].Value = "CourseName";
                    worksheet.Cells[1, 8].Value = "DifficultyLevel";
                    worksheet.Cells[1, 9].Value = "Solution";
                    worksheet.Cells[1, 10].Value = "Explanation";
                    worksheet.Cells[1, 27].Value = "QuestionCode";
                    worksheet.Column(27).Hidden = true;

                    // Format headers
                    using (var range = worksheet.Cells[1, 1, 1, 27])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }

                    int rowIndex = 2;
                    // Dictionaries to hold data
                    var chapters = new List<ContentIndexData>();
                    var topics = new List<ContentIndexData>();
                    var subTopics = new List<ContentIndexData>();

                    try
                    {
                        _connection.Open();

                        if (request.indexTypeId == 1)
                        {
                            // Fetch Chapters
                            var chapterQuery = "SELECT ContentIndexId AS ChildId, NULL AS ParentId, ContentName_Chapter AS ContentName FROM tblContentIndexChapters WHERE SubjectId = @SubjectId AND ContentIndexId = @ContentIndexId";
                            chapters = (await _connection.QueryAsync<ContentIndexData>(chapterQuery, new { SubjectId = request.subjectId, ContentIndexId = request.contentId })).ToList();
                        }

                        if (request.indexTypeId == 2)
                        {
                            // Fetch Topics
                            var topicQuery = "SELECT ContInIdTopic AS ChildId, ContentIndexId AS ParentId, ContentName_Topic AS ContentName FROM tblContentIndexTopics WHERE ContentIndexId = @ContentId";
                            topics = (await _connection.QueryAsync<ContentIndexData>(topicQuery, new { ContentId = request.contentId })).ToList();

                            // Fetch Chapters based on the ContentIndexId from Topics
                            var chapterQuery = "SELECT ContentIndexId AS ChildId, NULL AS ParentId, ContentName_Chapter AS ContentName FROM tblContentIndexChapters WHERE ContentIndexId IN @TopicParentIds";
                            chapters = (await _connection.QueryAsync<ContentIndexData>(chapterQuery, new { TopicParentIds = topics.Select(t => t.ParentId).Distinct().ToList() })).ToList();
                        }

                        if (request.indexTypeId == 3)
                        {
                            // Fetch SubTopics
                            var subTopicQuery = "SELECT ContInIdSubTopic AS ChildId, ContInIdTopic AS ParentId, ContentName_SubTopic AS ContentName FROM tblContentIndexSubTopics WHERE ContInIdSubTopic = @ContentId";
                            subTopics = (await _connection.QueryAsync<ContentIndexData>(subTopicQuery, new { ContentId = request.contentId })).ToList();

                            // Fetch Topics based on ContInIdTopic from SubTopics
                            var topicQuery = "SELECT ContInIdTopic AS ChildId, ContentIndexId AS ParentId, ContentName_Topic AS ContentName FROM tblContentIndexTopics WHERE ContInIdTopic IN @SubTopicParentIds";
                            topics = (await _connection.QueryAsync<ContentIndexData>(topicQuery, new { SubTopicParentIds = subTopics.Select(st => st.ParentId).Distinct().ToList() })).ToList();

                            // Fetch Chapters based on ContentIndexId from Topics
                            var chapterQuery = "SELECT ContentIndexId AS ChildId, NULL AS ParentId, ContentName_Chapter AS ContentName FROM tblContentIndexChapters WHERE ContentIndexId IN @TopicParentIds";
                            chapters = (await _connection.QueryAsync<ContentIndexData>(chapterQuery, new { TopicParentIds = topics.Select(t => t.ParentId).Distinct().ToList() })).ToList();
                        }
                    }
                    finally
                    {
                        _connection.Close();
                    }

                    // Fetch Master Data
                    var subjects = GetSubjects().ToDictionary(s => s.SubjectId, s => s.SubjectName);
                    var courses = (await _connection.QueryAsync<Course>("SELECT CourseId, CourseName FROM tblCourse")).ToDictionary(c => c.CourseId, c => c.CourseName);
                    var difficultyLevels = (await _connection.QueryAsync<DifficultyLevel>("SELECT LevelId, LevelName FROM tbldifficultylevel")).ToDictionary(dl => dl.LevelId, dl => dl.LevelName);
                    var questionTypes = GetQuestionTypes().ToDictionary(qt => qt.QuestionTypeID, qt => qt.QuestionType);

                    // Fetch Questions Data
                    var questionsData = await GetQuestionsData(request.subjectId, request.indexTypeId, request.contentId);

                    // Fetch Course and Difficulty Level from tblQIDCourse
                    var qidCourseQuery = @"SELECT QuestionCode, CourseID, LevelId 
                            FROM tblQIDCourse
                            WHERE QuestionCode IN @QuestionCodes";
                    var qidCourseMappings = (await _connection.QueryAsync<QIDCourse>(qidCourseQuery, new { QuestionCodes = questionsData.Select(q => q.QuestionCode) })).ToList();

                    int maxOptions = 0;

                    // Determine the maximum number of options
                    foreach (var question in questionsData)
                    {
                        if (question.QuestionTypeId == (int)QuestionTypesEnum.MCQ || question.QuestionTypeId == (int)QuestionTypesEnum.MAQ) // Handle multiple choice and multiple answers
                        {
                            var options = await GetOptionsForQuestion(question.QuestionCode);
                            maxOptions = Math.Max(maxOptions, options.Count);
                        }
                    }

                    // Add dynamic columns for options based on maxOptions
                    for (int i = 0; i < maxOptions; i++)
                    {
                        worksheet.Cells[1, 12 + i].Value = $"Option{i + 1}";
                    }

                    // Add data to worksheet
                    if (questionsData != null && questionsData.Any())
                    {
                        foreach (var question in questionsData)
                        {
                            worksheet.Cells[rowIndex, 1].Value = subjects.ContainsKey(question.subjectID) ? subjects[question.subjectID] : "Unknown"; // Map Subject

                            if (request.indexTypeId == 1)
                            {
                                // Map Chapter
                                var chapter = chapters.FirstOrDefault(c => c.ChildId == question.ContentIndexId);
                                worksheet.Cells[rowIndex, 2].Value = chapter != null ? chapter.ContentName : "Unknown";
                            }

                            if (request.indexTypeId == 2)
                            {
                                // Map Topic and its Parent Chapter
                                var topic = topics.FirstOrDefault(t => t.ChildId == question.ContentIndexId);
                                worksheet.Cells[rowIndex, 3].Value = topic != null ? topic.ContentName : "Unknown";

                                if (topic != null)
                                {
                                    // Get the parent chapter for the topic
                                    var chapter = chapters.FirstOrDefault(c => c.ChildId == topic.ParentId);
                                    worksheet.Cells[rowIndex, 2].Value = chapter != null ? chapter.ContentName : "Unknown";
                                }
                                else
                                {
                                    worksheet.Cells[rowIndex, 2].Value = "Unknown";
                                }
                            }

                            if (request.indexTypeId == 3)
                            {
                                // Map Sub-Topic, its Parent Topic, and Chapter
                                var subTopic = subTopics.FirstOrDefault(st => st.ChildId == question.ContentIndexId);
                                worksheet.Cells[rowIndex, 4].Value = subTopic != null ? subTopic.ContentName : "Unknown";

                                if (subTopic != null)
                                {
                                    // Get the parent topic for the sub-topic
                                    var topic = topics.FirstOrDefault(t => t.ChildId == subTopic.ParentId);
                                    worksheet.Cells[rowIndex, 3].Value = topic != null ? topic.ContentName : "Unknown";

                                    if (topic != null)
                                    {
                                        // Get the parent chapter for the topic
                                        var chapter = chapters.FirstOrDefault(c => c.ChildId == topic.ParentId);
                                        worksheet.Cells[rowIndex, 2].Value = chapter != null ? chapter.ContentName : "Unknown";
                                    }
                                    else
                                    {
                                        worksheet.Cells[rowIndex, 2].Value = "Unknown";
                                    }
                                }
                                else
                                {
                                    worksheet.Cells[rowIndex, 3].Value = "Unknown";
                                    worksheet.Cells[rowIndex, 2].Value = "Unknown";
                                }
                            }

                            worksheet.Cells[rowIndex, 5].Value = questionTypes.ContainsKey(question.QuestionTypeId) ? questionTypes[question.QuestionTypeId] : "Unknown"; // Map Question Type
                            worksheet.Cells[rowIndex, 6].Value = question.QuestionDescription;

                            // Map Course and Difficulty Level from the QIDCourse mapping table
                            var qidCourseMapping = qidCourseMappings.FirstOrDefault(q => q.QuestionCode == question.QuestionCode);
                            worksheet.Cells[rowIndex, 7].Value = qidCourseMapping != null && courses.ContainsKey(qidCourseMapping.CourseID) ? courses[qidCourseMapping.CourseID] : "Unknown";
                            worksheet.Cells[rowIndex, 8].Value = qidCourseMapping != null && difficultyLevels.ContainsKey(qidCourseMapping.LevelId) ? difficultyLevels[qidCourseMapping.LevelId] : "Unknown";

                            // Handle Options and Solution
                            if (question.QuestionTypeId == (int)QuestionTypesEnum.MCQ || question.QuestionTypeId == (int)QuestionTypesEnum.MAQ ||
                                question.QuestionTypeId == (int)QuestionTypesEnum.TF || question.QuestionTypeId == (int)QuestionTypesEnum.FB ||
                                question.QuestionTypeId == (int)QuestionTypesEnum.MT || question.QuestionTypeId == (int)QuestionTypesEnum.MT2)
                            {
                                // Fetch options for this question
                                var options = await GetOptionsForQuestion(question.QuestionCode);

                                // Populate options into dynamic columns
                                for (int i = 0; i < options.Count; i++)
                                {
                                    worksheet.Cells[rowIndex, 12 + i].Value = options[i].Answer;
                                }

                                // Map the correct answers to the Solution column
                                if (question.QuestionTypeId == (int)QuestionTypesEnum.MAQ) // Multiple Answer Question
                                {
                                    var correctAnswers = options.Where(o => o.IsCorrect).Select(o => o.Answer).ToList();
                                    worksheet.Cells[rowIndex, 9].Value = string.Join(", ", correctAnswers); // Correct answers in CSV format
                                }
                                else
                                {
                                    var correctAnswer = options.FirstOrDefault(o => o.IsCorrect)?.Answer;
                                    worksheet.Cells[rowIndex, 9].Value = correctAnswer ?? "None";
                                }
                            }
                            else if (question.QuestionTypeId == (int)QuestionTypesEnum.SA || question.QuestionTypeId == (int)QuestionTypesEnum.LA ||
                                     question.QuestionTypeId == (int)QuestionTypesEnum.VSA || question.QuestionTypeId == (int)QuestionTypesEnum.AR ||
                                     question.QuestionTypeId == (int)QuestionTypesEnum.NMR || question.QuestionTypeId == (int)QuestionTypesEnum.CMPR)
                            {
                                // For single-answer question types, use GetSingleAnswer method
                                var singleAnswer = GetSingleAnswer(question.QuestionCode);
                                worksheet.Cells[rowIndex, 9].Value = singleAnswer?.Answer ?? "None";
                            }

                            worksheet.Cells[rowIndex, 10].Value = question.Explanation;
                            worksheet.Cells[rowIndex, 27].Value = question.QuestionCode; // Hidden Question Code

                            rowIndex++;
                        }
                    }

                    // Auto fit columns for better readability
                    worksheet.Cells.AutoFitColumns();

                    AddMasterDataSheets(package, request.subjectId);

                    // Convert the ExcelPackage to a byte array
                    var fileBytes = package.GetAsByteArray();

                    // Return the file as a response
                    return new ServiceResponse<byte[]>(true, "Excel file generated successfully", fileBytes, 200);
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                return new ServiceResponse<byte[]>(false, ex.Message, [], 500);
            }
        }
        private void AddMasterDataSheets(ExcelPackage package, int subjectId)
        {
            // Create worksheets for master data
            var subjectWorksheet = package.Workbook.Worksheets.Add("Subjects");
            var chapterWorksheet = package.Workbook.Worksheets.Add("Chapters");
            var topicWorksheet = package.Workbook.Worksheets.Add("Topics");
            var subTopicWorksheet = package.Workbook.Worksheets.Add("SubTopics");
            var difficultyLevelWorksheet = package.Workbook.Worksheets.Add("Difficulty Levels");
            var questionTypeWorksheet = package.Workbook.Worksheets.Add("Question Types");

            // Populate data for Subjects
            subjectWorksheet.Cells[1, 1].Value = "SubjectName";
            subjectWorksheet.Cells[1, 2].Value = "SubjectCode";

            var subjects = GetSubjects();
            int subjectRow = 2;
            foreach (var subject in subjects)
            {
                subjectWorksheet.Cells[subjectRow, 1].Value = subject.SubjectName;
                subjectWorksheet.Cells[subjectRow, 2].Value = subject.SubjectCode;
                subjectRow++;
            }

            // Populate data for Chapters based on the selected SubjectId
            chapterWorksheet.Cells[1, 1].Value = "SubjectId";
            chapterWorksheet.Cells[1, 2].Value = "ContentIndexId";
            chapterWorksheet.Cells[1, 3].Value = "ContentName_Chapter";

            var chapters = GetChapters(subjectId);
            int chapterRow = 2;
            foreach (var chapter in chapters)
            {
                chapterWorksheet.Cells[chapterRow, 1].Value = chapter.SubjectId;
                chapterWorksheet.Cells[chapterRow, 2].Value = chapter.ContentIndexId;
                chapterWorksheet.Cells[chapterRow, 3].Value = chapter.ContentName_Chapter;
                chapterRow++;
            }

            // Populate data for Topics based on the selected ChapterId
            topicWorksheet.Cells[1, 1].Value = "ChapterId";
            topicWorksheet.Cells[1, 2].Value = "ContInIdTopic";
            topicWorksheet.Cells[1, 3].Value = "ContentName_Topic";

            int topicRow = 2;
            foreach (var chapter in chapters)
            {
                var topics = GetTopics(chapter.ContentIndexId);
                foreach (var topic in topics)
                {
                    topicWorksheet.Cells[topicRow, 1].Value = chapter.ContentIndexId;
                    topicWorksheet.Cells[topicRow, 2].Value = topic.ContInIdTopic;
                    topicWorksheet.Cells[topicRow, 3].Value = topic.ContentName_Topic;
                    topicRow++;
                }
            }

            // Populate data for SubTopics based on the selected TopicId
            subTopicWorksheet.Cells[1, 1].Value = "TopicId";
            subTopicWorksheet.Cells[1, 2].Value = "ContInIdSubTopic";
            subTopicWorksheet.Cells[1, 3].Value = "ContentName_SubTopic";

            int subTopicRow = 2;
            foreach (var chapter in chapters)
            {
                var topics = GetTopics(chapter.ContentIndexId);
                foreach (var topic in topics)
                {
                    var subTopics = GetSubTopics(topic.ContInIdTopic);
                    foreach (var subTopic in subTopics)
                    {
                        subTopicWorksheet.Cells[subTopicRow, 1].Value = topic.ContInIdTopic;
                        subTopicWorksheet.Cells[subTopicRow, 2].Value = subTopic.ContInIdSubTopic;
                        subTopicWorksheet.Cells[subTopicRow, 3].Value = subTopic.ContentName_SubTopic;
                        subTopicRow++;
                    }
                }
            }

            // Populate data for Difficulty Levels
            difficultyLevelWorksheet.Cells[1, 1].Value = "LevelId";
            difficultyLevelWorksheet.Cells[1, 2].Value = "LevelName";
            difficultyLevelWorksheet.Cells[1, 3].Value = "LevelCode";

            var difficultyLevels = GetDifficultyLevels();
            int levelRow = 2;
            foreach (var level in difficultyLevels)
            {
                difficultyLevelWorksheet.Cells[levelRow, 1].Value = level.LevelId;
                difficultyLevelWorksheet.Cells[levelRow, 2].Value = level.LevelName;
                difficultyLevelWorksheet.Cells[levelRow, 3].Value = level.LevelCode;
                levelRow++;
            }

            // Populate data for Question Types
            questionTypeWorksheet.Cells[1, 1].Value = "QuestionTypeID";
            questionTypeWorksheet.Cells[1, 2].Value = "QuestionType";

            var questionTypes = GetQuestionTypes();
            int typeRow = 2;
            foreach (var type in questionTypes)
            {
                questionTypeWorksheet.Cells[typeRow, 1].Value = type.QuestionTypeID;
                questionTypeWorksheet.Cells[typeRow, 2].Value = type.QuestionType;
                typeRow++;
            }

            // AutoFit columns
            subjectWorksheet.Cells[subjectWorksheet.Dimension.Address].AutoFitColumns();
            chapterWorksheet.Cells[chapterWorksheet.Dimension.Address].AutoFitColumns();
            topicWorksheet.Cells[topicWorksheet.Dimension.Address].AutoFitColumns();
            subTopicWorksheet.Cells[subTopicWorksheet.Dimension.Address].AutoFitColumns();
            difficultyLevelWorksheet.Cells[difficultyLevelWorksheet.Dimension.Address].AutoFitColumns();
            questionTypeWorksheet.Cells[questionTypeWorksheet.Dimension.Address].AutoFitColumns();
        }
        private IEnumerable<Subject> GetSubjects()
        {
            var query = "SELECT * FROM tblSubject WHERE Status = 1";
            var result = _connection.Query<dynamic>(query);
            var resposne = result.Select(item => new Subject
            {
                SubjectCode = item.SubjectCode,
                SubjectId = item.SubjectId,
                SubjectName = item.SubjectName,
                SubjectType = item.SubjectType
            });
            return resposne;
        }
        private IEnumerable<Chapters> GetChapters(int subjectId)
        {
            var query = "SELECT * FROM tblContentIndexChapters WHERE IndexTypeId = 1 AND IsActive = 1 AND SubjectId = @subjectId";
            return _connection.Query<Chapters>(query, new { subjectId });
        }
        private IEnumerable<Topics> GetTopics(int chapterId)
        {
            var query = "SELECT * FROM tblContentIndexTopics WHERE IndexTypeId = 2 AND IsActive = 1 AND ContentIndexId = @chapterId";
            return _connection.Query<Topics>(query, new { chapterId });
        }
        private IEnumerable<SubTopic> GetSubTopics(int TopicId)
        {
            var query = "SELECT * FROM tblContentIndexSubTopics WHERE IndexTypeId = 3 AND IsActive = 1 AND ContInIdTopic = @TopicId";
            return _connection.Query<SubTopic>(query, new { TopicId });
        }
        private IEnumerable<DifficultyLevel> GetDifficultyLevels()
        {
            var query = "SELECT [LevelId], [LevelName], [LevelCode], [Status], [NoofQperLevel], [SuccessRate], [createdon], [patterncode], [modifiedon], [modifiedby], [createdby], [EmployeeID], [EmpFirstName] FROM [tbldifficultylevel]";
            return _connection.Query<DifficultyLevel>(query);
        }
        private IEnumerable<Questiontype> GetQuestionTypes()
        {
            var query = "SELECT [QuestionTypeID], [QuestionType], [Code], [Status], [MinNoOfOptions], [modifiedon], [modifiedby], [createdon], [createdby], [EmployeeID], [EmpFirstName], [TypeOfOption], [Question] FROM [tblQBQuestionType]";
            return _connection.Query<Questiontype>(query);
        }
        public async Task<ServiceResponse<string>> UploadQuestionsFromExcel(IFormFile file)
        {
            var questions = new List<QuestionDTO>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;

                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0]; // Assumes the data is in the first worksheet
                    var rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++) // Skip header row
                    {
                        var question = new QuestionDTO
                        {
                            QuestionDescription = worksheet.Cells[row, 1].Text,
                            QuestionTypeId = int.Parse(worksheet.Cells[row, 2].Text),
                            CreatedBy = worksheet.Cells[row, 3].Text,
                            subjectID = int.Parse(worksheet.Cells[row, 4].Text),
                            ContentIndexId = int.Parse(worksheet.Cells[row, 5].Text),
                            EmployeeId = int.Parse(worksheet.Cells[row, 6].Text),
                            IndexTypeId = int.Parse(worksheet.Cells[row, 7].Text),
                            Explanation = worksheet.Cells[row, 8].Text,
                            ExtraInformation = worksheet.Cells[row, 9].Text,
                            QuestionCode = string.IsNullOrEmpty(worksheet.Cells[row, 10].Text) ? null : worksheet.Cells[row, 10].Text,
                            AnswerMultipleChoiceCategories = GetAnswerMultipleChoiceCategories(worksheet, row),
                            Answersingleanswercategories = GetAnswerSingleAnswerCategories(worksheet, row)
                        };

                        questions.Add(question);
                    }
                }
            }
            if (questions.Any())
            {
                return new ServiceResponse<string>(true, "Operation Successful", "data uploaded successfully.", 200);
            }
            else
            {
                return new ServiceResponse<string>(false, "Operation failed", string.Empty, 500);
            }
        }
        private List<AnswerMultipleChoiceCategory> GetAnswerMultipleChoiceCategories(ExcelWorksheet worksheet, int row)
        {
            var categories = new List<AnswerMultipleChoiceCategory>();

            var categoryCount = int.Parse(worksheet.Cells[row, 11].Text); // Assume number of categories is in column 11

            for (int i = 0; i < categoryCount; i++)
            {
                var answer = worksheet.Cells[row, 12 + (i * 4)].Text; // Answer
                var isCorrect = bool.Parse(worksheet.Cells[row, 13 + (i * 4)].Text); // IsCorrect
                var matchId = int.Parse(worksheet.Cells[row, 14 + (i * 4)].Text); // MatchId

                categories.Add(new AnswerMultipleChoiceCategory
                {
                    Answer = answer,
                    Iscorrect = isCorrect,
                    Matchid = matchId
                });
            }

            return categories;
        }
        private Answersingleanswercategory GetAnswerSingleAnswerCategories(ExcelWorksheet worksheet, int row)
        {
            var answer = worksheet.Cells[row, 15].Text; // Single answer category in column 15

            return new Answersingleanswercategory
            {
                Answer = answer
            };
        }
        // Helper method to fetch questions based on parameters
        private async Task ProcessUploadedData(IEnumerable<QuestionUploadData> data)
        {
            // Define your transaction scope if needed
            using (var transaction = _connection.BeginTransaction())
            {
                try
                {
                    foreach (var item in data)
                    {
                        // Example of checking if the question already exists
                        var existingQuestion = await _connection.QuerySingleOrDefaultAsync<Question>(
                            "SELECT * FROM tblQuestions WHERE QuestionCode = @QuestionCode",
                            new { QuestionCode = item.QuestionCode },
                            transaction);

                        if (existingQuestion != null)
                        {
                            // Update existing question
                            await _connection.ExecuteAsync(
                                "UPDATE tblQuestions SET QuestionDescription = @QuestionDescription, Explanation = @Explanation WHERE QuestionCode = @QuestionCode",
                                new { item.QuestionDescription, item.Explanation, item.QuestionCode },
                                transaction);
                        }
                        else
                        {
                            // Insert new question
                            await _connection.ExecuteAsync(
                                "INSERT INTO tblQuestions (SubjectName, ChapterName, ConceptName, SubConceptName, QuestionType, QuestionDescription, CourseName, DifficultyLevel, Solution, Explanation, QuestionCode) VALUES (@SubjectName, @ChapterName, @ConceptName, @SubConceptName, @QuestionType, @QuestionDescription, @CourseName, @DifficultyLevel, @Solution, @Explanation, @QuestionCode)",
                                item,
                                transaction);
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        private async Task<IEnumerable<Question>> GetQuestionsData(int subjectId, int indexTypeId, int contentId)
        {
            // Example query to fetch questions from the database
            var query = @"SELECT * FROM tblQuestion 
                  WHERE subjectID = @SubjectId 
                  AND IndexTypeId = @IndexTypeId 
                  AND [ContentIndexId] = @ContentId AND IsActive = 1";
            var result = await _connection.QueryAsync<Question>(query, new { SubjectId = subjectId, IndexTypeId = indexTypeId, ContentId = contentId });
            var resposne = result.Select(item => new Question
            {
                QuestionId = item.QuestionId,
                ContentIndexId = item.ContentIndexId,
                CreatedBy = item.CreatedBy,
                CreatedOn = item.CreatedOn,
                EmployeeId = item.EmployeeId,
                Explanation = item.Explanation,
                ExtraInformation = item.ExtraInformation,
                IndexTypeId = item.IndexTypeId,
                IsActive = item.IsActive,
                IsApproved = item.IsApproved,
                IsRejected = item.IsRejected,
                ModifiedBy = item.ModifiedBy,
                ModifiedOn = item.ModifiedOn,
                QuestionCode = item.QuestionCode,
                QuestionDescription = item.QuestionDescription,
                QuestionTypeId = item.QuestionTypeId,
                Status = item.Status,
                subjectID = item.subjectID
            });

            return resposne;
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
                           // CreatedBy = 1,
                            CreatedDate = DateTime.Now,
                          //  ModifiedBy = 1,
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
                //QuestionSubjectMappings = GetListOfQuestionSubjectMapping(item.QuestionCode),
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
        public string GenerateQuestionCode(int indexTypeId, int contentId, int questionId)
        {
            string questionCode = "";
            int subjectId = 0;
            int chapterId = 0;
            int topicId = 0;
            int subTopicId = 0;

            // Fetch subject ID and related hierarchy based on indexTypeId
            if (indexTypeId == 1)  // Chapter
            {
                // Fetch subject directly from chapter
                var chapter = _connection.QueryFirstOrDefault("SELECT SubjectId, ContentIndexId FROM tblContentIndexChapters WHERE ContentIndexId = @contentId", new { contentId });
                if (chapter != null)
                {
                    subjectId = chapter.SubjectId;
                    chapterId = chapter.ContentIndexId;
                }
            }
            else if (indexTypeId == 2)  // Topic
            {
                // Fetch parent chapter from topic, then get subject from the chapter
                var topic = _connection.QueryFirstOrDefault("SELECT ContentIndexId, ContInIdTopic FROM tblContentIndexTopics WHERE ContInIdTopic = @contentId", new { contentId });
                if (topic != null)
                {
                    topicId = topic.ContInIdTopic;
                    chapterId = topic.ContentIndexId;

                    // Now fetch the subject from the parent chapter
                    var chapter = _connection.QueryFirstOrDefault("SELECT SubjectId FROM tblContentIndexChapters WHERE ContentIndexId = @chapterId", new { chapterId });
                    if (chapter != null)
                    {
                        subjectId = chapter.SubjectId;
                    }
                }
            }
            else if (indexTypeId == 3)  // SubTopic
            {
                // Fetch parent topic from subtopic, then get the chapter, and then the subject
                var subTopic = _connection.QueryFirstOrDefault("SELECT ContInIdTopic, ContInIdSubTopic FROM tblContentIndexSubTopics WHERE ContInIdSubTopic = @contentId", new { contentId });
                if (subTopic != null)
                {
                    subTopicId = subTopic.ContInIdSubTopic;
                    topicId = subTopic.ContInIdTopic;

                    // Now fetch the chapter from the parent topic
                    var topic = _connection.QueryFirstOrDefault("SELECT ContentIndexId FROM tblContentIndexTopics WHERE ContInIdTopic = @topicId", new { topicId });
                    if (topic != null)
                    {
                        chapterId = topic.ContentIndexId;

                        // Now fetch the subject from the parent chapter
                        var chapter = _connection.QueryFirstOrDefault("SELECT SubjectId FROM tblContentIndexChapters WHERE ContentIndexId = @chapterId", new { chapterId });
                        if (chapter != null)
                        {
                            subjectId = chapter.SubjectId;
                        }
                    }
                }
            }
            // Construct the question code based on IndexTypeId and IDs
            if (indexTypeId == 1)  // Chapter
            {
                questionCode = $"S{subjectId}C{chapterId}Q{questionId}";
            }
            else if (indexTypeId == 2)  // Topic
            {
                questionCode = $"S{subjectId}C{chapterId}T{topicId}Q{questionId}";
            }
            else if (indexTypeId == 3)  // SubTopic
            {
                questionCode = $"S{subjectId}C{chapterId}T{topicId}ST{subTopicId}Q{questionId}";
            }

            return questionCode;
        }
        private string? GetRoleName(int roleId)
        {
            // Fetch role details based on the roleId
            var role = _connection.QuerySingleOrDefault<dynamic>("SELECT RoleName FROM tblRole WHERE RoleID = @RoleId", new { RoleId = roleId });
            return role?.RoleName; // Return the role name or null if not found
        }
        private async Task<string> GetSubjectiveAnswer(int questionId)
        {
            string query = @"
            SELECT sa.Answer 
            FROM tblAnswersingleanswercategory sa
            INNER JOIN tblAnswerMaster am ON sa.Answerid = am.Answerid
            WHERE am.Questionid = @QuestionId AND am.QuestionTypeid = 2 AND am.IsActive = 1";

            return await _connection.QuerySingleOrDefaultAsync<string>(query, new { QuestionId = questionId });
        }
        public async Task<List<Option>> GetOptionsForQuestion(string questionId)
        {
            string query = @"
            SELECT mc.Answer, mc.Iscorrect 
            FROM tblAnswerMultipleChoiceCategory mc
            INNER JOIN tblAnswerMaster am ON mc.Answerid = am.Answerid
            WHERE am.QuestionCode = @QuestionId AND am.QuestionTypeid = 1";

            var options = await _connection.QueryAsync<Option>(query, new { QuestionId = questionId });

            return options.ToList();
        }

        // Supporting DTO
        public class Option
        {
            public string Answer { get; set; }
            public bool IsCorrect { get; set; }
        }
        public class ContentIndexData
        {
            public int ParentId { get; set; }
            public int ChildId { get; set; }
            public string ContentName { get; set; } = string.Empty;
        }
        public enum QuestionTypesEnum
        {
            MCQ = 1,
            TF = 2,
            SA = 3,
            FB = 4,
            MT = 5,
            MAQ = 6,
            LA = 7,
            VSA = 8,
            MT2 = 9,
            AR = 10,
            NMR = 11,
            CMPR = 12
        }
        public class QuestionUploadData
        {
            public string SubjectName { get; set; }
            public string ChapterName { get; set; }
            public string ConceptName { get; set; }
            public string SubConceptName { get; set; }
            public string QuestionType { get; set; }
            public string QuestionDescription { get; set; }
            public string CourseName { get; set; }
            public string DifficultyLevel { get; set; }
            public string Solution { get; set; }
            public string Explanation { get; set; }
            public string QuestionCode { get; set; }
        }
        private string FileUpload(string base64String)
        {
            if (string.IsNullOrEmpty(base64String) || base64String == "string")
            {
                return string.Empty;
            }
            if (base64String == string.Empty)
            {
                return string.Empty;
            }
            byte[] data = Convert.FromBase64String(base64String);
            string directoryPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "RejectedQuestions");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fileExtension = IsJpeg(data) == true ? ".jpg" : IsPng(data) == true ?
                ".png" : IsGif(data) == true ? ".gif" : IsPdf(data) == true ? ".pdf" : string.Empty;

            string fileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(directoryPath, fileName);
            if (string.IsNullOrEmpty(fileExtension))
            {
                throw new InvalidOperationException("Incorrect file uploaded");
            }
            // Write the byte array to the image file
            File.WriteAllBytes(filePath, data);
            return filePath;
        }
        private string GetFile(string Filename)
        {
            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "RejectedQuestions", Filename);

            if (!File.Exists(filePath))
            {
                return string.Empty;
            }
            byte[] fileBytes = File.ReadAllBytes(filePath);
            string base64String = Convert.ToBase64String(fileBytes);
            return base64String;
        }
        private bool IsJpeg(byte[] bytes)
        {
            // JPEG magic number: 0xFF, 0xD8
            return bytes.Length > 1 && bytes[0] == 0xFF && bytes[1] == 0xD8;
        }
        private bool IsPng(byte[] bytes)
        {
            // PNG magic number: 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A
            return bytes.Length > 7 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47
                && bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A;
        }
        private bool IsGif(byte[] bytes)
        {
            // GIF magic number: "GIF"
            return bytes.Length > 2 && bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46;
        }
        private bool IsPdf(byte[] bytes)
        {
            // PDF magic number: "%PDF"
            return bytes.Length > 4 &&
                   bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46;
        }
    }
}