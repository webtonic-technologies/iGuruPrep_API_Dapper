﻿using Dapper;
using Schools_API.DTOs.Requests;
using Schools_API.DTOs.Response;
using Schools_API.DTOs.ServiceResponse;
using Schools_API.Models;
using Schools_API.Repository.Interfaces;
using System.Data;

namespace Schools_API.Repository.Implementations
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly IDbConnection _connection;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly string? _connectionString;

        public ProjectRepository(IDbConnection connection, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
        {
            _connection = connection;
            _hostingEnvironment = hostingEnvironment;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<ServiceResponse<string>> AddProjectAsync(ProjectDTO request)
        {
            try
            {
                if (request.ProjectId == 0)
                {
                    var insertQuery = @"
                    INSERT INTO tblProject (ProjectName, ProjectDescription, Image, createdby, ReferenceLink, EmployeeID, status, createdon, pdfVideoFile)
                    VALUES (@ProjectName, @ProjectDescription, @Image, @createdby, @ReferenceLink, @EmployeeID, @status, @createdon, @pdfVideoFile);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    var project = new Project
                    {
                        ProjectName = request.ProjectName,
                        ProjectDescription = request.ProjectDescription,
                        Image = UploadImage(request.Image ??= string.Empty),
                        createdby = request.createdby,
                        ReferenceLink = request.ReferenceLink,
                        EmployeeID = request.EmployeeID,
                        status = true,
                        createdon = DateTime.Now,
                        pdfVideoFile = UploadDocumentOrVideo(request.pdfVideoFile ??= string.Empty)
                    };
                    int insertedValue = await _connection.QueryFirstOrDefaultAsync<int>(insertQuery, project);
                    if(insertedValue > 0)
                    {
                        int category = ProjectCategoryMapping(request.ProjectCategories ??= ([]), insertedValue);
                        int classes = ProjectClassMapping(request.ProjectClasses ??= ([]), insertedValue);
                        int board = ProjectBoardMapping(request.ProjectBoards ??= ([]), insertedValue);
                        int course = ProjectCourseMapping(request.ProjectCourses ??= ([]), insertedValue);
                        int exam = ProjectExamTypeMapping(request.ProjectExamTypes ??= ([]), insertedValue);
                        int subject = ProjectSubjectMapping(request.ProjectSubjects ??= ([]), insertedValue);
                        if (category > 0 && classes > 0 && board > 0 && course > 0 && exam > 0 && subject > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Project Added Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Operation failed", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Some error occured", string.Empty, 500);
                    }
                }
                else
                {
                    var updateQuery = @"
                        UPDATE tblProject
                        SET ProjectName = @ProjectName,
                        ProjectDescription = @ProjectDescription,
                        Image = @Image,
                        ReferenceLink = @ReferenceLink,
                        EmployeeID = @EmployeeID,
                        status = @status,
                        modifiedby = @modifiedby,
                        modifiedon = @modifiedon,
                        pdfVideoFile = @pdfVideoFile
                        WHERE ProjectId = @ProjectId;";
                    var project = new Project
                    {
                        ProjectName = request.ProjectName,
                        ProjectDescription = request.ProjectDescription,
                        Image = UploadImage(request.Image ??= string.Empty),
                        modifiedby = request.modifiedby,
                        ReferenceLink = request.ReferenceLink,
                        EmployeeID = request.EmployeeID,
                        status = request.status,
                        modifiedon = DateTime.Now,
                        pdfVideoFile = UploadDocumentOrVideo(request.pdfVideoFile ??= string.Empty),
                        ProjectId = request.ProjectId
                    }; ;
                   int rowsAffected = await _connection.ExecuteAsync(updateQuery, project);
                    if (rowsAffected > 0)
                    {
                        int category = ProjectCategoryMapping(request.ProjectCategories ??= ([]), request.ProjectId);
                        int classes = ProjectClassMapping(request.ProjectClasses ??= ([]), request.ProjectId);
                        int board = ProjectBoardMapping(request.ProjectBoards ??= ([]), request.ProjectId);
                        int course = ProjectCourseMapping(request.ProjectCourses ??= ([]), request.ProjectId);
                        int exam = ProjectExamTypeMapping(request.ProjectExamTypes ??= ([]), request.ProjectId);
                        int subject = ProjectSubjectMapping(request.ProjectSubjects ??= ([]), request.ProjectId);
                        if (category > 0 && classes > 0 && board > 0 && course > 0 && exam > 0 && subject > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "project updated Successfully", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Operation failed", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Some error occured", string.Empty, 500);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<List<ProjectResponseDTO>>> GetAllProjectsByFilter(ProjectFilter request)
        {
            try
            {
                var employeeRoleQuery = "SELECT e.RoleID, r.RoleCode FROM tblEmployee e INNER JOIN tblRole r ON e.RoleID = r.RoleID WHERE e.Employeeid = @EmployeeID";
                var employeeRole = await _connection.QuerySingleOrDefaultAsync<dynamic>(employeeRoleQuery, new { EmployeeID = request.EmployeeId });

                // Determine if the employee is Admin or SuperAdmin
                bool isAdminOrSuperAdmin = employeeRole != null && (employeeRole.RoleCode == "AD" || employeeRole.RoleCode == "SA");
                // Base query
                string baseQuery = @"
            SELECT
                p.ProjectId,
                p.ProjectName,
                p.ProjectDescription,
                p.Image,
                p.createdby,
                p.ReferenceLink,
                p.EmployeeID,
                p.status,
                p.modifiedby,
                p.modifiedon,
                p.createdon,
                e.EmpFirstName,
                p.pdfVideoFile
            FROM [tblProject] p
            LEFT JOIN [tblEmployee] e ON p.EmployeeID = e.Employeeid
            LEFT JOIN [tblProjectCategory] pc ON p.ProjectId = pc.ProjectId
            LEFT JOIN [tblProjectBoard] pb ON p.ProjectId = pb.ProjectID
            LEFT JOIN [tblProjectClass] pcl ON p.ProjectId = pcl.ProjectID
            LEFT JOIN [tblProjectCourse] pco ON p.ProjectId = pco.ProjectID
            LEFT JOIN [tblProjectExamType] pet ON p.ProjectId = pet.ProjectID
            LEFT JOIN [tblProjectSubject] ps ON p.ProjectId = ps.ProjectID
            LEFT JOIN [tblBoard] b ON pb.BoardID = b.BoardId AND b.Status = 1
            LEFT JOIN [tblClass] c ON pcl.ClassID = c.ClassId AND c.Status = 1
            LEFT JOIN [tblCourse] co ON pco.CourseID = co.CourseId AND co.Status = 1
            LEFT JOIN [tblSubject] s ON ps.SubjectID = s.SubjectId AND s.Status = 1
            WHERE 1=1";

                // Applying filters dynamically
                if (request.ClassID > 0)
                {
                    baseQuery += " AND pcl.ClassID = @ClassID";
                }
                if (request.BoardID > 0)
                {
                    baseQuery += " AND pb.BoardID = @BoardID";
                }
                if (request.CourseID > 0)
                {
                    baseQuery += " AND pco.CourseID = @CourseID";
                }
                if (request.ExamTypeID > 0)
                {
                    baseQuery += " AND pet.ExamTypeID = @ExamTypeID";
                }
                if (request.APID > 0)
                {
                    baseQuery += " AND pc.APID = @APID";
                }
                if (request.SubjectID > 0)
                {
                    baseQuery += " AND ps.SubjectID = @SubjectID";
                }
                if (!isAdminOrSuperAdmin)
                {
                    baseQuery += " AND s.Status = 1";
                }
                // Parameters for the query
                var parameters = new
                {
                    ClassID = request.ClassID,
                    BoardID = request.BoardID,
                    CourseID = request.CourseID,
                    ExamTypeID = request.ExamTypeID,
                    APID = request.APID,
                    SubjectID = request.SubjectID
                };

                // Fetch all filtered records
                var mainResult = await _connection.QueryAsync<dynamic>(baseQuery, parameters);

                // Filter and paginate in memory
                var filteredResults = mainResult.Select(item => new ProjectResponseDTO
                {
                    ProjectId = item.ProjectId,
                    ProjectName = item.ProjectName,
                    ProjectDescription = item.ProjectDescription,
                    Image = GetFile(item.Image ??= string.Empty),
                    createdby = item.createdby,
                    ReferenceLink = item.ReferenceLink,
                    EmployeeID = item.EmployeeID,
                    status = item.status,
                    modifiedby = item.modifiedby,
                    modifiedon = item.modifiedon,
                    createdon = item.createdon,
                    EmpFirstName = item.EmpFirstName,
                    pdfVideoFile = GetFile(item.pdfVideoFile),
                    ProjectCategories = GetListOfProjectCategory(item.ProjectId),
                    ProjectBoards = GetListOfProjectBoards(item.ProjectId),
                    ProjectClasses = GetListOfProjectClass(item.ProjectId),
                    ProjectCourses = GetListOfProjectCourse(item.ProjectId),
                    ProjectExamTypes = GetListOfProjectExamType(item.ProjectId),
                    ProjectSubjects = GetListOfProjectSubject(item.ProjectId)
                }).ToList();

                // Apply pagination after fetching
                int totalCount = filteredResults.Count;

                var paginatedResults = filteredResults
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                if (paginatedResults.Count != 0)
                {
                    return new ServiceResponse<List<ProjectResponseDTO>>(true, "Records found", paginatedResults, 200, totalCount);
                }
                else
                {
                    return new ServiceResponse<List<ProjectResponseDTO>>(false, "Records not found", new List<ProjectResponseDTO>(), 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ProjectResponseDTO>>(false, ex.Message, new List<ProjectResponseDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<ProjectResponseDTO>> GetProjectByIdAsync(int projectId)
        {
            try
            {
                var response = new ProjectResponseDTO();

                string sql = @"
                SELECT 
                    p.ProjectId,
                    p.ProjectName,
                    p.ProjectDescription,
                    p.Image,
                    p.createdby,
                    p.ReferenceLink,
                    p.EmployeeID,
                    p.status,
                    p.modifiedby,
                    p.modifiedon,
                    p.createdon,
                    e.EmpFirstName,
                    p.pdfVideoFile
                FROM tblProject p
                LEFT JOIN tblEmployee e ON p.EmployeeID = e.Employeeid
                WHERE p.ProjectId = @ProjectId";

                var data = await _connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { ProjectId = projectId });

                // If project is found, append full path to ImageName
                if (data != null && !string.IsNullOrEmpty(data?.PathURL))
                {
                    response.Image = GetFile(data?.Image);
                }
                if (data != null && !string.IsNullOrEmpty(data?.pdfVideoFile))
                {
                    response.pdfVideoFile = GetFile(data?.pdfVideoFile);
                }
                // If project is not found, throw KeyNotFoundException
                if (data == null)
                {
                    throw new KeyNotFoundException("Project not found.");
                }

                if (data != null)
                {
                    response.ProjectId = data.ProjectId;
                    response.EmpFirstName = data.EmpFirstName;
                    response.createdby = data.createdby;
                    response.createdon = data.createdon;
                    response.status = data.status;
                    response.ProjectName = data.ProjectName;
                    response.ProjectDescription = data.ProjectDescription;
                    response.ReferenceLink = data.ReferenceLink;
                    response.EmployeeID = data.EmployeeID;
                    response.modifiedby = data.modifiedby;
                    response.modifiedon = data.modifiedon;
                    response.ProjectCourses = GetListOfProjectCourse(response.ProjectId);
                    response.ProjectClasses = GetListOfProjectClass(response.ProjectId);
                    response.ProjectExamTypes = GetListOfProjectExamType(response.ProjectId);
                    response.ProjectBoards = GetListOfProjectBoards(response.ProjectId);
                    response.ProjectSubjects = GetListOfProjectSubject(response.ProjectId);
                    response.ProjectCategories = GetListOfProjectCategory(response.ProjectId);
                    return new ServiceResponse<ProjectResponseDTO>(true, "Operation Successful", response, 200);
                }
                else
                {
                    return new ServiceResponse<ProjectResponseDTO>(false, "Opertion Failed", new ProjectResponseDTO(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ProjectResponseDTO>(false, ex.Message, new ProjectResponseDTO(), 500);
            }
        }
        private string UploadImage(string base64String)
        {
            if (string.IsNullOrEmpty(base64String) || base64String == "string")
            {
                return string.Empty;
            }

            byte[] data = Convert.FromBase64String(base64String);
            string directoryPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "ProjectForStudent");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string fileExtension = IsJpeg(data) ? ".jpg" : IsPng(data) ? ".png" : IsGif(data) ? ".gif" : string.Empty;

            if (string.IsNullOrEmpty(fileExtension))
            {
                throw new InvalidOperationException("Incorrect image file uploaded");
            }

            string fileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(directoryPath, fileName);

            File.WriteAllBytes(filePath, data);
            return filePath;
        }
        private string UploadDocumentOrVideo(string base64String)
        {
            if (string.IsNullOrEmpty(base64String) || base64String == "string")
            {
                return string.Empty;
            }

            byte[] data = Convert.FromBase64String(base64String);
            string directoryPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "ProjectForStudent");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string fileExtension = IsPdf(data) ? ".pdf" : IsMov(data) ? ".mov" : IsMp4(data) ? ".mp4" : IsAvi(data) ? ".avi" : string.Empty;

            if (string.IsNullOrEmpty(fileExtension))
            {
                throw new InvalidOperationException("Incorrect document or video file uploaded");
            }

            string fileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(directoryPath, fileName);

            File.WriteAllBytes(filePath, data);
            return filePath;
        }

        //private string FileUpload(string base64String)
        //{
        //    if (string.IsNullOrEmpty(base64String) || base64String == "string")
        //    {
        //        return string.Empty;
        //    }
        //    if (base64String == string.Empty)
        //    {
        //        return string.Empty;
        //    }
        //    byte[] data = Convert.FromBase64String(base64String);
        //    string directoryPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "ProjectForStudent");

        //    if (!Directory.Exists(directoryPath))
        //    {
        //        Directory.CreateDirectory(directoryPath);
        //    }
        //    string fileExtension = IsJpeg(data) == true ? ".jpg" : IsPng(data) == true ?
        //        ".png" : IsGif(data) == true ? ".gif" : IsPdf(data) == true ? ".pdf" : IsMov(data) == true ? ".mov" :
        //    IsMp4(data) == true ? ".mp4" : IsAvi(data) == true ? ".avi" : string.Empty;

        //    string fileName = Guid.NewGuid().ToString() + fileExtension;
        //    string filePath = Path.Combine(directoryPath, fileName);
        //    if (string.IsNullOrEmpty(fileExtension))
        //    {
        //        throw new InvalidOperationException("Incorrect file uploaded");
        //    }
        //    // Write the byte array to the image file
        //    File.WriteAllBytes(filePath, data);
        //    return filePath;
        //}
        private string GetFile(string Filename)
        {
            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "ProjectForStudent", Filename);

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
        private int ProjectCategoryMapping(List<ProjectCategory> request, int ProjectId)
        {
            foreach (var data in request)
            {
                data.ProjectId = ProjectId;
            }
            string query = "SELECT COUNT(*) FROM [tblProjectCategory] WHERE [ProjectId] = @ProjectId";
            int count = _connection.QueryFirstOrDefault<int>(query, new { ProjectId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblProjectCategory]
                          WHERE [ProjectId] = @ProjectId;";
                var rowsAffected = _connection.Execute(deleteDuery, new { ProjectId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblProjectCategory] ([APId], [ProjectId])
                          VALUES (@APId, @ProjectId);";
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
                var insertquery = @"INSERT INTO [tblProjectCategory] ([APId], [ProjectId])
                          VALUES (@APId, @ProjectId);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int ProjectClassMapping(List<ProjectClass> request, int ProjectId)
        {
            foreach (var data in request)
            {
                data.ProjectID = ProjectId;
            }
            string query = "SELECT COUNT(*) FROM [tblProjectClass] WHERE [ProjectID] = @ProjectID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { ProjectID = ProjectId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblProjectClass]
                          WHERE [ProjectID] = @ProjectID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { ProjectID = ProjectId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblProjectClass] ([ProjectID], [ClassID])
                          VALUES (@ProjectID, @ClassID);";
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
                var insertquery = @"INSERT INTO [tblProjectClass] ([ProjectID], [ClassID])
                          VALUES (@ProjectID, @ClassID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int ProjectBoardMapping(List<ProjectBoard> request, int ProjectId)
        {
            foreach (var data in request)
            {
                data.ProjectID = ProjectId;
            }
            string query = "SELECT COUNT(*) FROM [tblProjectBoard] WHERE [ProjectID] = @ProjectID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { ProjectID = ProjectId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblProjectBoard]
                          WHERE [ProjectID] = @ProjectID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { ProjectID = ProjectId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblProjectBoard] ([ProjectID], [BoardID])
                          VALUES (@ProjectID, @BoardID);";
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
                var insertquery = @"INSERT INTO [tblProjectBoard] ([ProjectID], [BoardID])
                          VALUES (@ProjectID, @BoardID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int ProjectCourseMapping(List<ProjectCourse> request, int ProjectId)
        {
            foreach (var data in request)
            {
                data.ProjectID = ProjectId;
            }
            string query = "SELECT COUNT(*) FROM [tblProjectCourse] WHERE [ProjectID] = @ProjectID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { ProjectID = ProjectId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblProjectCourse]
                          WHERE [ProjectID] = @ProjectID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { ProjectID = ProjectId});
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblProjectCourse] ([ProjectID], [CourseID])
                          VALUES (@ProjectID, @CourseID);";
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
                var insertquery = @"INSERT INTO [tblProjectCourse] ([ProjectID], [CourseID])
                          VALUES (@ProjectID, @CourseID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int ProjectExamTypeMapping(List<ProjectExamType> request, int ProjectId)
        {
            foreach (var data in request)
            {
                data.ProjectID = ProjectId;
            }
            string query = "SELECT COUNT(*) FROM [tblProjectExamType] WHERE [ProjectID] = @ProjectID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { ProjectID = ProjectId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblProjectExamType]
                          WHERE [ProjectID] = @ProjectID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { ProjectID = ProjectId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblProjectExamType] ([ProjectID], [ExamTypeID])
                          VALUES (@ProjectID, @ExamTypeID);";
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
                var insertquery = @"INSERT INTO [tblProjectExamType] ([ProjectID], [ExamTypeID])
                          VALUES (@ProjectID, @ExamTypeID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int ProjectSubjectMapping(List<ProjectSubject> request, int ProjectId)
        {
            foreach (var data in request)
            {
                data.ProjectID = ProjectId;
            }
            string query = "SELECT COUNT(*) FROM [tblProjectSubject] WHERE [ProjectID] = @ProjectID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { ProjectID = ProjectId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [tblProjectSubject]
                          WHERE [ProjectID] = @ProjectID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { ProjectID = ProjectId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [tblProjectSubject] ([ProjectID], [SubjectID])
                          VALUES (@ProjectID, @SubjectID);";
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
                var insertquery = @"INSERT INTO [tblProjectSubject] ([ProjectID], [SubjectID])
                          VALUES (@ProjectID, @SubjectID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private bool IsPdf(byte[] bytes)
        {
            // PDF magic number: "%PDF"
            return bytes.Length > 4 &&
                   bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46;
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
        private List<ProjectBoardResponse> GetListOfProjectBoards(int ProjectId)
        {
            var boardquery = @"
            SELECT pb.*, b.BoardName as Name
            FROM tblProjectBoard pb
            LEFT JOIN tblBoard b ON pb.BoardID = b.BoardId
            WHERE pb.ProjectID = @ProjectID
              AND b.Status = 1;"; // Check for active boards
            var data = _connection.Query<ProjectBoardResponse>(boardquery, new { ProjectID = ProjectId });
            return data != null ? data.AsList() : new List<ProjectBoardResponse>();
        }
        private List<ProjectCategoryResponse> GetListOfProjectCategory(int ProjectId)
        {
            var query = @"
            SELECT pc.*, ap.APName
            FROM tblProjectCategory pc
            LEFT JOIN tblCategory ap ON pc.APID = ap.APId
            WHERE pc.ProjectId = @ProjectId;";
            var data = _connection.Query<ProjectCategoryResponse>(query, new { ProjectId });
            return data != null ? data.AsList() : [];
        }
        private List<ProjectClassResponse> GetListOfProjectClass(int ProjectId)
        {
            var query = @"
    SELECT pc.*, c.ClassName as Name
    FROM tblProjectClass pc
    LEFT JOIN tblClass c ON pc.ClassID = c.ClassId
    WHERE pc.ProjectID = @ProjectID
      AND c.Status = 1;"; // Check for active classes
            var data = _connection.Query<ProjectClassResponse>(query, new { ProjectID = ProjectId });
            return data != null ? data.AsList() : new List<ProjectClassResponse>();
        }
        private List<ProjectCourseResponse> GetListOfProjectCourse(int ProjectId)
        {
            var query = @"
    SELECT pc.*, c.CourseName as Name
    FROM tblProjectCourse pc
    LEFT JOIN tblCourse c ON pc.CourseID = c.CourseId
    WHERE pc.ProjectID = @ProjectID
      AND c.Status = 1;"; // Check for active courses
            var data = _connection.Query<ProjectCourseResponse>(query, new { ProjectID = ProjectId });
            return data != null ? data.AsList() : new List<ProjectCourseResponse>();
        }
        private List<ProjectExamTypeResponse> GetListOfProjectExamType(int ProjectId)
        {
            var query = @"
            SELECT pet.*, et.ExamTypeName as Name
            FROM tblProjectExamType pet
            LEFT JOIN tblExamType et ON pet.ExamTypeID = et.ExamTypeID
            WHERE pet.ProjectID = @ProjectID;";
            var data = _connection.Query<ProjectExamTypeResponse>(query, new { ProjectID = ProjectId });
            return data != null ? data.AsList() : [];
        }
        private List<ProjectSubjectResponse> GetListOfProjectSubject(int ProjectId)
        {
            var query = @"
    SELECT ps.*, s.SubjectName as Name
    FROM tblProjectSubject ps
    LEFT JOIN tblSubject s ON ps.SubjectID = s.SubjectId
    WHERE ps.ProjectID = @ProjectID
      AND s.Status = 1;"; // Check for active subjects
            var data = _connection.Query<ProjectSubjectResponse>(query, new { ProjectID = ProjectId });
            return data != null ? data.AsList() : new List<ProjectSubjectResponse>();
        }

    }
}
