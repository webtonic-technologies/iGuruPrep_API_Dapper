using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.Response;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace ControlPanel_API.Repository.Implementations
{
    public class TimeTablePreparationRepository : ITimeTablePreparationRepository
    {
        private readonly IDbConnection _connection;
        public TimeTablePreparationRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateTimeTable(TimeTablePreparationRequest request)
        {
            try
            {
                // Insert Operation
                if (request.PreparationTimeTableId == 0)
                {
                    string insertTimeTableQuery = @"
                    INSERT INTO [tblNBPreparationTimeTable] 
                    ([TTTitle], [Date], [Status], [createdon], [createdby], [EmployeeID]) 
                    VALUES 
                    (@TTTitle, @Date, @Status, @createdon, @createdby, @EmployeeID);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

                    int newId = await _connection.QuerySingleAsync<int>(insertTimeTableQuery, new
                    {
                        request.TTTitle,
                        request.Date,
                        request.createdby,
                        Status = true,
                        createdon = DateTime.Now,
                        request.EmployeeID
                    });
                    if (newId > 0)
                    {
                        int category = TimeTableCategoryMapping(request.TimeTableCategories ??= ([]), newId);
                        int classes = TimeTableClassMapping(request.TimeTableClasses ??= ([]), newId);
                        int board = TimeTableBoardMapping(request.TimeTableBoards ??= ([]), newId);
                        int course = TimeTableCourseMapping(request.TimeTableCourses ??= ([]), newId);
                        int exam = TimeTableExamTypeMapping(request.TimeTableExamTypes ??= ([]), newId);
                        int subject = TimeTableSubjectMapping(request.TimeTableSubjects ??= ([]), newId);
                        if (category > 0 && classes > 0 && board > 0 && course > 0 && exam > 0 && subject > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "TimeTable Added Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Operation failed", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                    }
                }
                // Update Operation
                else
                {
                    string updateTimeTableQuery = @"
                    UPDATE [tblNBPreparationTimeTable] 
                    SET [TTTitle] = @TTTitle, [Date] = @Date, [Status] = @Status, [modifiedon] = @modifiedon, [modifiedby] = @modifiedby, 
                        [EmployeeID] = @EmployeeID 
                    WHERE [PreparationTimeTableId] = @PreparationTimeTableId;";

                    int rowsAffected = await _connection.ExecuteAsync(updateTimeTableQuery, new
                    {
                        request.TTTitle,
                        request.Date,
                        request.Status,
                        request.modifiedby,
                        modifiedon = DateTime.Now,
                        request.EmployeeID,
                        request.PreparationTimeTableId
                    });

                    if (rowsAffected > 0)
                    {
                        int category = TimeTableCategoryMapping(request.TimeTableCategories ??= ([]), request.PreparationTimeTableId);
                        int classes = TimeTableClassMapping(request.TimeTableClasses ??= ([]), request.PreparationTimeTableId);
                        int board = TimeTableBoardMapping(request.TimeTableBoards ??= ([]), request.PreparationTimeTableId);
                        int course = TimeTableCourseMapping(request.TimeTableCourses ??= ([]), request.PreparationTimeTableId);
                        int exam = TimeTableExamTypeMapping(request.TimeTableExamTypes ??= ([]), request.PreparationTimeTableId);
                        int subject = TimeTableSubjectMapping(request.TimeTableSubjects ??= ([]), request.PreparationTimeTableId);
                        if (category > 0 && classes > 0 && board > 0 && course > 0 && exam > 0 && subject > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "TimeTable Updated Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Operation failed", string.Empty, 500);
                        }
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
        public async Task<ServiceResponse<List<TimeTablePreparationResponseDTO>>> GetAllTimeTableList(TimeTableListRequestDTO request)
        {
            try
            {
                var employeeRoleQuery = "SELECT e.RoleID, r.RoleCode FROM tblEmployee e INNER JOIN tblRole r ON e.RoleID = r.RoleID WHERE e.Employeeid = @EmployeeID";
                var employeeRole = await _connection.QuerySingleOrDefaultAsync<dynamic>(employeeRoleQuery, new { EmployeeID = request.EmployeeId });

                // Determine if the employee is Admin or SuperAdmin
                bool isAdminOrSuperAdmin = employeeRole != null && (employeeRole.RoleCode == "AD" || employeeRole.RoleCode == "SA");
                // Base query to fetch all matching records
                string baseQuery = @"
        SELECT 
            tt.PreparationTimeTableId, 
            tt.TTTitle, 
            tt.Date, 
            tt.Status, 
            tt.modifiedon, 
            tt.modifiedby, 
            tt.createdon, 
            tt.createdby, 
            tt.EmployeeID, 
            e.EmpFirstName AS EmployeeName
        FROM tblNBPreparationTimeTable tt
        LEFT JOIN tblEmployee e ON tt.EmployeeID = e.Employeeid
        LEFT JOIN tblNBTimeTableClass tc ON tt.PreparationTimeTableId = tc.PreparationTimeTableId
        LEFT JOIN tblNBTimeTableBoard tb ON tt.PreparationTimeTableId = tb.PreparationTimeTableId
        LEFT JOIN tblNBTimeTableCourse tco ON tt.PreparationTimeTableId = tco.PreparationTimeTableId
        LEFT JOIN tblNBTimeTableExamType tet ON tt.PreparationTimeTableId = tet.PreparationTimeTableId
        LEFT JOIN tblNBTimeTableCategory tca ON tt.PreparationTimeTableId = tca.PreparationTimeTableId
        WHERE 1=1";

                // Applying filters
                //if (request.ClassID > 0)
                //{
                //    baseQuery += " AND tc.ClassId = @ClassID";
                //}
                //if (request.BoardIDID > 0)
                //{
                //    baseQuery += " AND tb.BoardId = @BoardIDID";
                //}
                //if (request.CourseID > 0)
                //{
                //    baseQuery += " AND tco.CourseId = @CourseID";
                //}
                if (request.ClassID > 0)
                {
                    baseQuery += @"
    AND tc.ClassId = @ClassID 
    AND tc.PreparationTimeTableId IN (
        SELECT tc.[PreparationTimeTableId] 
        FROM [tblNBTimeTableClass] tc 
        INNER JOIN [tblClass] c ON tc.[ClassId] = c.[ClassId] 
        WHERE c.[Status] = 1
    )";
                }
                if (request.BoardIDID > 0)
                {
                    baseQuery += @"
    AND tb.BoardId = @BoardIDID 
    AND tb.PreparationTimeTableId IN (
        SELECT tb.[PreparationTimeTableId] 
        FROM [tblNBTimeTableBoard] tb 
        INNER JOIN [tblBoard] b ON tb.[BoardId] = b.[BoardId] 
        WHERE b.[Status] = 1
    )";
                }
                if (request.CourseID > 0)
                {
                    baseQuery += @"
    AND tco.CourseId = @CourseID 
    AND tco.PreparationTimeTableId IN (
        SELECT tco.[PreparationTimeTableId] 
        FROM [tblNBTimeTableCourse] tco 
        INNER JOIN [tblCourse] co ON tco.[CourseId] = co.[CourseId] 
        WHERE co.[Status] = 1
    )";
                }
                if (request.ExamTypeID > 0)
                {
                    baseQuery += " AND tet.ExamTypeId = @ExamTypeID";
                }
                if (request.APID > 0)
                {
                    baseQuery += " AND tca.CategoryId = @APID";
                }
                if (!isAdminOrSuperAdmin)
                {
                    baseQuery += " AND s.Status = 1";
                }
                // Parameters for the query
                var parameters = new
                {
                    ClassID = request.ClassID,
                    BoardIDID = request.BoardIDID,
                    CourseID = request.CourseID,
                    ExamTypeID = request.ExamTypeID,
                    APID = request.APID
                };

                // Fetch all matching records
                var mainResult = (await _connection.QueryAsync<TimeTablePreparationResponseDTO>(baseQuery, parameters)).ToList();

                // Map results to response DTO
                var resultList = mainResult.Select(result => new TimeTablePreparationResponseDTO
                {
                    PreparationTimeTableId = result.PreparationTimeTableId,
                    TTTitle = result.TTTitle,
                    Date = result.Date,
                    Status = result.Status,
                    modifiedon = result.modifiedon,
                    modifiedby = result.modifiedby,
                    createdon = result.createdon,
                    createdby = result.createdby,
                    EmployeeID = result.EmployeeID,
                    EmployeeName = result.EmployeeName,
                    TimeTableBoards = GetListOfTimeTableBoards(result.PreparationTimeTableId),
                    TimeTableSubjects = GetListOfTimeTableSubjects(result.PreparationTimeTableId),
                    TimeTableExamTypes = GetListOfTimeTableExamType(result.PreparationTimeTableId),
                    TimeTableCourses = GetListOfTimeTableCourse(result.PreparationTimeTableId),
                    TimeTableClasses = GetListOfTimeTableClass(result.PreparationTimeTableId),
                    TimeTableCategories = GetListOfTimeTableCategory(result.PreparationTimeTableId)
                }).ToList();

                // Total count before pagination
                int totalCount = resultList.Count;

                // Apply logical pagination
                var paginatedResponse = resultList
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                // Check if there are records
                if (paginatedResponse.Any())
                {
                    return new ServiceResponse<List<TimeTablePreparationResponseDTO>>(true, "Success", paginatedResponse, 200, totalCount);
                }
                else
                {
                    return new ServiceResponse<List<TimeTablePreparationResponseDTO>>(false, "No records found", new List<TimeTablePreparationResponseDTO>(), 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<TimeTablePreparationResponseDTO>>(false, ex.Message, new List<TimeTablePreparationResponseDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<TimeTablePreparationResponseDTO>> GetTimeTableById(int PreparationTimeTableId)
        {
            try
            {
                string mainQuery = @"
        SELECT 
            tt.PreparationTimeTableId, 
            tt.TTTitle, 
            tt.Date, 
            tt.Status, 
            tt.modifiedon, 
            tt.modifiedby, 
            tt.createdon, 
            tt.createdby, 
            tt.EmployeeID, 
            e.EmpFirstName AS EmployeeName
        FROM tblNBPreparationTimeTable tt
        LEFT JOIN tblEmployee e ON tt.EmployeeID = e.Employeeid
        WHERE tt.PreparationTimeTableId = @PreparationTimeTableId;";

                var mainResult = await _connection.QuerySingleOrDefaultAsync<TimeTablePreparationResponseDTO>(mainQuery, new { PreparationTimeTableId });

                if (mainResult == null)
                {
                    return new ServiceResponse<TimeTablePreparationResponseDTO>(false, "TimeTable not found", new TimeTablePreparationResponseDTO(), 404);
                }

                mainResult.TimeTableBoards = GetListOfTimeTableBoards(PreparationTimeTableId);
                mainResult.TimeTableSubjects = GetListOfTimeTableSubjects(PreparationTimeTableId);
                mainResult.TimeTableExamTypes = GetListOfTimeTableExamType(PreparationTimeTableId);
                mainResult.TimeTableCourses = GetListOfTimeTableCourse(PreparationTimeTableId);
                mainResult.TimeTableClasses = GetListOfTimeTableClass(PreparationTimeTableId);
                mainResult.TimeTableCategories = GetListOfTimeTableCategory(PreparationTimeTableId);

                return new ServiceResponse<TimeTablePreparationResponseDTO>(true, "Success", mainResult, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<TimeTablePreparationResponseDTO>(false, ex.Message, new TimeTablePreparationResponseDTO(), 500);
            }
        }
        private int TimeTableBoardMapping(List<TimeTableBoard> request, int PreparationTimeTableId)
        {
            foreach (var data in request)
            {
                data.PreparationTimeTableId = PreparationTimeTableId;
            }
            string query = "SELECT COUNT(*) FROM [tblNBTimeTableBoard] WHERE [PreparationTimeTableId] = @PreparationTimeTableId";
            int count = _connection.QueryFirstOrDefault<int>(query, new { PreparationTimeTableId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblNBTimeTableBoard] WHERE [PreparationTimeTableId] = @PreparationTimeTableId;";
                var rowsAffected = _connection.Execute(deleteDuery, new { PreparationTimeTableId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblNBTimeTableBoard] ([BoardId], [PreparationTimeTableId])
                          VALUES (@BoardId, @PreparationTimeTableId);";
                    var valuesInserted = _connection.Execute(insertquery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var insertquery = @"INSERT INTO [tblNBTimeTableBoard] ([BoardId], [PreparationTimeTableId])
                          VALUES (@BoardId, @PreparationTimeTableId);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int TimeTableClassMapping(List<TimeTableClass> request, int PreparationTimeTableId)
        {
            foreach (var data in request)
            {
                data.PreparationTimeTableId = PreparationTimeTableId;
            }
            string query = "SELECT COUNT(*) FROM [tblNBTimeTableClass] WHERE [PreparationTimeTableId] = @PreparationTimeTableId";
            int count = _connection.QueryFirstOrDefault<int>(query, new { PreparationTimeTableId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblNBTimeTableClass]
                          WHERE [PreparationTimeTableId] = @PreparationTimeTableId;";
                var rowsAffected = _connection.Execute(deleteDuery, new { PreparationTimeTableId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblNBTimeTableClass] ([PreparationTimeTableId], [ClassId])
                          VALUES (@PreparationTimeTableId, @ClassId);";
                    var valuesInserted = _connection.Execute(insertquery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var insertquery = @"INSERT INTO [tblNBTimeTableClass] ([PreparationTimeTableId], [ClassId])
                          VALUES (@PreparationTimeTableId, @ClassId);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int TimeTableCategoryMapping(List<TimeTableCategory> request, int PreparationTimeTableId)
        {
            foreach (var data in request)
            {
                data.PreparationTimeTableId = PreparationTimeTableId;
            }
            string query = "SELECT COUNT(*) FROM [tblNBTimeTableCategory] WHERE [PreparationTimeTableId] = @PreparationTimeTableId";
            int count = _connection.QueryFirstOrDefault<int>(query, new { PreparationTimeTableId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblNBTimeTableCategory]
                          WHERE [PreparationTimeTableId] = @PreparationTimeTableId;";
                var rowsAffected = _connection.Execute(deleteDuery, new { PreparationTimeTableId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblNBTimeTableCategory] ([PreparationTimeTableId], [CategoryId])
                          VALUES (@PreparationTimeTableId, @CategoryId);";
                    var valuesInserted = _connection.Execute(insertquery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var insertquery = @"INSERT INTO [tblNBTimeTableCategory] ([PreparationTimeTableId], [CategoryId])
                          VALUES (@PreparationTimeTableId, @CategoryId);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int TimeTableCourseMapping(List<TimeTableCourse> request, int PreparationTimeTableId)
        {
            foreach (var data in request)
            {
                data.PreparationTimeTableId = PreparationTimeTableId;
            }
            string query = "SELECT COUNT(*) FROM [tblNBTimeTableCourse] WHERE [PreparationTimeTableId] = @PreparationTimeTableId";
            int count = _connection.QueryFirstOrDefault<int>(query, new { PreparationTimeTableId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblNBTimeTableCourse]
                          WHERE [PreparationTimeTableId] = @PreparationTimeTableId;";
                var rowsAffected = _connection.Execute(deleteDuery, new { PreparationTimeTableId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblNBTimeTableCourse] ([PreparationTimeTableId], [CourseId])
                          VALUES (@PreparationTimeTableId, @CourseId);";
                    var valuesInserted = _connection.Execute(insertquery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var insertquery = @"INSERT INTO [tblNBTimeTableCourse] ([PreparationTimeTableId], [CourseId])
                          VALUES (@PreparationTimeTableId, @CourseId);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int TimeTableExamTypeMapping(List<TimeTableExamType> request, int PreparationTimeTableId)
        {
            foreach (var data in request)
            {
                data.PreparationTimeTableId = PreparationTimeTableId;
            }
            string query = "SELECT COUNT(*) FROM [tblNBTimeTableExamType] WHERE [PreparationTimeTableId] = @PreparationTimeTableId";
            int count = _connection.QueryFirstOrDefault<int>(query, new { PreparationTimeTableId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblNBTimeTableExamType]
                          WHERE [PreparationTimeTableId] = @PreparationTimeTableId;";
                var rowsAffected = _connection.Execute(deleteDuery, new { PreparationTimeTableId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblNBTimeTableExamType] ([PreparationTimeTableId], [ExamTypeId])
                          VALUES (@PreparationTimeTableId, @ExamTypeId);";
                    var valuesInserted = _connection.Execute(insertquery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var insertquery = @"INSERT INTO [tblNBTimeTableExamType] ([PreparationTimeTableId], [ExamTypeId])
                          VALUES (@PreparationTimeTableId, @ExamTypeId);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int TimeTableSubjectMapping(List<TimeTableSubject> request, int PreparationTimeTableId)
        {
            foreach (var data in request)
            {
                data.PreparationTimeTableId = PreparationTimeTableId;
            }
            string query = "SELECT COUNT(*) FROM [tblNBTimeTableSubject] WHERE [PreparationTimeTableId] = @PreparationTimeTableId";
            int count = _connection.QueryFirstOrDefault<int>(query, new { PreparationTimeTableId });
            if (count > 0)
            {
                var deleteQuery = @"DELETE FROM [tblNBTimeTableSubject] WHERE [PreparationTimeTableId] = @PreparationTimeTableId;";
                var rowsAffected = _connection.Execute(deleteQuery, new { PreparationTimeTableId });
                if (rowsAffected > 0)
                {
                    var insertQuery = @"INSERT INTO [tblNBTimeTableSubject] ([PreparationTimeTableId], [SubjectId])
                  VALUES (@PreparationTimeTableId, @SubjectId);";
                    var valuesInserted = _connection.Execute(insertQuery, request);

                    foreach (var subject in request)
                    {
                        var contentMapping = TTSubjectContentMapping(subject.TTSubjectContentMappings ?? new List<TTSubjectContentMapping>(), subject.NBTimeTableSubjectId, PreparationTimeTableId);
                        if (contentMapping < 0) return 0;
                    }

                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var insertQuery = @"INSERT INTO [tblNBTimeTableSubject] ([PreparationTimeTableId], [SubjectId])
                  VALUES (@PreparationTimeTableId, @SubjectId);";
                var valuesInserted = _connection.Execute(insertQuery, request);

                foreach (var subject in request)
                {
                    var contentMapping = TTSubjectContentMapping(subject.TTSubjectContentMappings ?? new List<TTSubjectContentMapping>(), subject.NBTimeTableSubjectId, PreparationTimeTableId);
                    if (contentMapping < 0) return 0;
                }

                return valuesInserted;
            }
        }
        private int TTSubjectContentMapping(List<TTSubjectContentMapping> request, int NBTimeTableSubjectId, int PreparationTimeTableId)
        {
            foreach (var data in request)
            {
                data.PreparationTimeTableId = PreparationTimeTableId;
                data.NBTimeTableSubjectId = NBTimeTableSubjectId;
            }
            string query = "SELECT COUNT(*) FROM [tblNBTTSubjectContentMapping] WHERE [NBTimeTableSubjectId] = @NBTimeTableSubjectId";
            int count = _connection.QueryFirstOrDefault<int>(query, new { NBTimeTableSubjectId });
            if (count > 0)
            {
                var deleteQuery = @"DELETE FROM [tblNBTTSubjectContentMapping]
                  WHERE [NBTimeTableSubjectId] = @NBTimeTableSubjectId;";
                var rowsAffected = _connection.Execute(deleteQuery, new { NBTimeTableSubjectId });
                if (rowsAffected > 0)
                {
                    var insertQuery = @"INSERT INTO [tblNBTTSubjectContentMapping] ([PreparationTimeTableId], [NBTimeTableSubjectId], [IndexTypeId], [ContentIndexId])
                  VALUES (@PreparationTimeTableId, @NBTimeTableSubjectId, @IndexTypeId, @ContentIndexId);";
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
                var insertQuery = @"INSERT INTO [tblNBTTSubjectContentMapping] ([PreparationTimeTableId], [NBTimeTableSubjectId], [IndexTypeId], [ContentIndexId])
                  VALUES (@PreparationTimeTableId, @NBTimeTableSubjectId, @IndexTypeId, @ContentIndexId);";
                var valuesInserted = _connection.Execute(insertQuery, request);
                return valuesInserted;
            }
        }
        private List<TimeTableBoardResponse> GetListOfTimeTableBoards(int PreparationTimeTableId)
        {
            string Query = @"
    SELECT tb.[NBTimeTableBoardId], tb.[PreparationTimeTableId], tb.[BoardId], b.[BoardName] AS Name
    FROM [tblNBTimeTableBoard] tb
    LEFT JOIN [tblBoard] b ON tb.BoardId = b.BoardId
    WHERE tb.[PreparationTimeTableId] = @PreparationTimeTableId
      AND b.Status = 1;"; // Ensure board is active

            var data = _connection.Query<TimeTableBoardResponse>(Query, new { PreparationTimeTableId });
            return data != null ? data.AsList() : new List<TimeTableBoardResponse>();
        }
        private List<TimeTableClassResponse> GetListOfTimeTableClass(int PreparationTimeTableId)
        {
            string Query = @"
    SELECT tc.[NBTimeTableClassId], tc.[PreparationTimeTableId], tc.[ClassId], c.[ClassName] AS Name
    FROM [tblNBTimeTableClass] tc
    LEFT JOIN [tblClass] c ON tc.ClassId = c.ClassId
    WHERE tc.[PreparationTimeTableId] = @PreparationTimeTableId
      AND c.Status = 1;"; // Ensure class is active

            var data = _connection.Query<TimeTableClassResponse>(Query, new { PreparationTimeTableId });
            return data != null ? data.AsList() : new List<TimeTableClassResponse>();
        }
        private List<TimeTableCategoryResponse> GetListOfTimeTableCategory(int PreparationTimeTableId)
        {
            string Query = @"
            SELECT tca.*, c.[APName] AS Name
            FROM [tblNBTimeTableCategory] tca
            LEFT JOIN [tblCategory] c ON tca.CategoryId = c.APId
            WHERE tca.[PreparationTimeTableId] = @PreparationTimeTableId";
            var data = _connection.Query<TimeTableCategoryResponse>(Query, new { PreparationTimeTableId });
            return data != null ? data.AsList() : [];
        }
        private List<TimeTableCourseResponse> GetListOfTimeTableCourse(int PreparationTimeTableId)
        {
            string Query = @"
    SELECT tc.[NBTimeTableCourseId], tc.[PreparationTimeTableId], tc.[CourseId], c.[CourseName] AS Name
    FROM [tblNBTimeTableCourse] tc
    LEFT JOIN [tblCourse] c ON tc.CourseId = c.CourseId
    WHERE tc.[PreparationTimeTableId] = @PreparationTimeTableId
      AND c.Status = 1;"; // Ensure course is active

            var data = _connection.Query<TimeTableCourseResponse>(Query, new { PreparationTimeTableId });
            return data != null ? data.AsList() : new List<TimeTableCourseResponse>();
        }
        private List<TimeTableExamTypeResponse> GetListOfTimeTableExamType(int PreparationTimeTableId)
        {
            string examTypeQuery = @"
            SELECT tet.*, et.[ExamTypeName] AS Name
            FROM [tblNBTimeTableExamType] tet
            LEFT JOIN [tblExamType] et ON tet.ExamTypeId = et.ExamTypeID
            WHERE tet.[PreparationTimeTableId] = @PreparationTimeTableId";

            var data = _connection.Query<TimeTableExamTypeResponse>(examTypeQuery, new { PreparationTimeTableId });
            return data != null ? data.AsList() : [];
        }
        private List<TimeTableSubjectResponse> GetListOfTimeTableSubjects(int PreparationTimeTableId)
        {
            string query = @"
    SELECT 
        ts.NBTimeTableSubjectId, 
        ts.PreparationTimeTableId, 
        ts.SubjectId, 
        s.SubjectName AS Name
    FROM tblNBTimeTableSubject ts
    LEFT JOIN tblSubject s ON ts.SubjectId = s.SubjectId
    WHERE ts.PreparationTimeTableId = @PreparationTimeTableId
      AND s.Status = 1;"; // Ensure subject is active

            var data = _connection.Query<TimeTableSubjectResponse>(query, new { PreparationTimeTableId }).ToList();

            foreach (var subject in data)
            {
                subject.TTSubjectContentMappings = GetListOfTTSubjectContentMappings(subject.NBTimeTableSubjectId);
            }

            return data;
        }
        private List<TTSubjectContentMappingResponse> GetListOfTTSubjectContentMappings(int NBTimeTableSubjectId)
        {
            string query = @"
    SELECT 
        tscm.NBTTSubContMappingId, 
        tscm.PreparationTimeTableId, 
        tscm.NBTimeTableSubjectId, 
        tscm.IndexTypeId, 
        it.IndexType AS IndexTypeName, 
        tscm.ContentIndexId, 
        CASE 
            WHEN tscm.IndexTypeId = 1 THEN ci.ContentName_Chapter
            WHEN tscm.IndexTypeId = 2 THEN ct.ContentName_Topic
            WHEN tscm.IndexTypeId = 3 THEN cst.ContentName_SubTopic
        END AS ContentIndexName
    FROM tblNBTTSubContMapping tscm
    LEFT JOIN tblQBIndexType it ON tscm.IndexTypeId = it.IndexId
    LEFT JOIN tblContentIndexChapters ci ON tscm.ContentIndexId = ci.ContentIndexId AND tscm.IndexTypeId = 1
    LEFT JOIN tblContentIndexTopics ct ON tscm.ContentIndexId = ct.ContInIdTopic AND tscm.IndexTypeId = 2
    LEFT JOIN tblContentIndexSubTopics cst ON tscm.ContentIndexId = cst.ContInIdSubTopic AND tscm.IndexTypeId = 3
    WHERE tscm.NBTimeTableSubjectId = @NBTimeTableSubjectId";

            var data = _connection.Query<TTSubjectContentMappingResponse>(query, new { NBTimeTableSubjectId });
            return data.ToList();
        }
    }
}
