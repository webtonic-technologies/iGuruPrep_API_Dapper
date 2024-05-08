using Dapper;
using Schools_API.DTOs;
using Schools_API.DTOs.ServiceResponse;
using Schools_API.Models;
using Schools_API.Repository.Interfaces;
using System.Data;
using System.Text;

namespace Schools_API.Repository.Implementations
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly IDbConnection _connection;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ProjectRepository(IDbConnection connection, IWebHostEnvironment hostingEnvironment)
        {
            _connection = connection;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<ServiceResponse<string>> AddProjectAsync(ProjectDTO request)
        {
            try
            {
                if (request.ProjectId == 0)
                {
                    var insertQuery = @"
                    INSERT INTO Project (ProjectName, ProjectDescription, PathURL, createdby, ReferenceLink, EmployeeID, status, createdon, EmpFirstName, pdfVideoFile)
                    VALUES (@ProjectName, @ProjectDescription, @PathURL, @createdby, @ReferenceLink, @EmployeeID, @status, @createdon, @EmpFirstName, @pdfVideoFile);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    var project = new Project
                    {
                        ProjectName = request.ProjectName,
                        ProjectDescription = request.ProjectDescription,
                        PathURL = FileUpload(request.PathURL ??= string.Empty),
                        createdby = request.createdby,
                        ReferenceLink = request.ReferenceLink,
                        EmployeeID = request.EmployeeID,
                        status = true,
                        createdon = DateTime.Now,
                        EmpFirstName = request.EmpFirstName,
                        pdfVideoFile = FileUpload(request.pdfVideoFile ??= string.Empty)
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
                        UPDATE Project
                        SET ProjectName = @ProjectName,
                        ProjectDescription = @ProjectDescription,
                        PathURL = @PathURL,
                        ReferenceLink = @ReferenceLink,
                        EmployeeID = @EmployeeID,
                        status = @status,
                        modifiedby = @modifiedby,
                        modifiedon = @modifiedon,
                        EmpFirstName = @EmpFirstName,
                        pdfVideoFile = @pdfVideoFile
                        WHERE ProjectId = @ProjectId;";
                    var project = new Project
                    {
                        ProjectName = request.ProjectName,
                        ProjectDescription = request.ProjectDescription,
                        PathURL = FileUpload(request.PathURL ??= string.Empty),
                        modifiedby = request.modifiedby,
                        ReferenceLink = request.ReferenceLink,
                        EmployeeID = request.EmployeeID,
                        status = request.status,
                        modifiedon = DateTime.Now,
                        EmpFirstName = request.EmpFirstName,
                        pdfVideoFile = FileUpload(request.pdfVideoFile ??= string.Empty),
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
        public async Task<ServiceResponse<IEnumerable<ProjectDTO>>> GetAllProjectsByFilter(ProjectFilter filter)
        {
            try
            {
                var queryBuilder = new StringBuilder("SELECT * FROM tblproject WHERE 1 = 1");
                var parameters = new DynamicParameters();

                if (filter.BoardId.HasValue)
                {
                    queryBuilder.Append(" AND BoardId = @BoardId");
                    parameters.Add("@BoardId", filter.BoardId.Value);
                }
                if (filter.ClassId.HasValue)
                {
                    queryBuilder.Append(" AND ClassId = @ClassId");
                    parameters.Add("@ClassId", filter.ClassId.Value);
                }
                if (filter.CourseId.HasValue)
                {
                    queryBuilder.Append(" AND CourseId = @CourseId");
                    parameters.Add("@CourseId", filter.CourseId.Value);
                }
                if (filter.SubjectId.HasValue)
                {
                    queryBuilder.Append(" AND SubjectId = @SubjectId");
                    parameters.Add("@SubjectId", filter.SubjectId.Value);
                }

                // Execute the SQL query using Dapper
                var projects = await _connection.QueryAsync<ProjectDTO>(queryBuilder.ToString(), parameters);

                // Process the PathURL if necessary
                foreach (var project in projects)
                {
                    if (project.PathURL != null)
                    {
                        project.PathURL = Path.Combine(_hostingEnvironment.ContentRootPath, "ProjectImages", project.PathURL);
                    }
                }

                if (projects != null)
                {
                    return new ServiceResponse<IEnumerable<ProjectDTO>>(true, "Operation Successful", projects.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<IEnumerable<ProjectDTO>>(false, "Opertion Failed", new List<ProjectDTO>(), 500);
                }

            }
            catch (Exception ex)
            {
                return new ServiceResponse<IEnumerable<ProjectDTO>>(false, ex.Message, new List<ProjectDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<ProjectDTO>> GetProjectByIdAsync(int projectId)
        {
            try
            {
                var response = new ProjectDTO();

                string sql = @"
            SELECT ProjectId,
                   ProjectName,
                   ProjectDescription,
                   PathURL,
                   createdby,
                   ReferenceLink,
                   EmployeeID,
                   status,
                   modifiedby,
                   modifiedon,
                   createdon,
                   EmpFirstName,
                   pdfVideoFile
            FROM tblproject
            WHERE ProjectId = @ProjectId";

                var data = await _connection.QueryFirstOrDefaultAsync<Project>(sql, new { ProjectId = projectId });

                // If project is found, append full path to ImageName
                if (data != null && !string.IsNullOrEmpty(data.PathURL))
                {
                    response.PathURL = GetFile(data.PathURL);
                }
                if (data != null && !string.IsNullOrEmpty(data.pdfVideoFile))
                {
                    response.pdfVideoFile = GetFile(data.pdfVideoFile);
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
                    return new ServiceResponse<ProjectDTO>(true, "Operation Successful", response, 200);
                }
                else
                {
                    return new ServiceResponse<ProjectDTO>(false, "Opertion Failed", new ProjectDTO(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ProjectDTO>(false, ex.Message, new ProjectDTO(), 500);
            }
        }
        private string FileUpload(string base64String)
        {
            if (base64String == string.Empty)
            {
                return string.Empty;
            }
            byte[] data = Convert.FromBase64String(base64String);
            string directoryPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "ProjectForStudent");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fileExtension = IsJpeg(data) == true ? ".jpg" : IsPng(data) == true ?
                ".png" : IsGif(data) == true ? ".gif" : IsPdf(data) == true ? ".pdf" : IsMov(data) == true ? ".mov" :
            IsMp4(data) == true ? ".mp4" : IsAvi(data) == true ? ".avi" : string.Empty;

            string fileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(directoryPath, fileName);

            // Write the byte array to the image file
            File.WriteAllBytes(filePath, data);
            return filePath;
        }
        private string GetFile(string Filename)
        {
            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "ProjectForStudent", Filename);

            if (!File.Exists(filePath))
            {
                throw new Exception("File not found");
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
            string query = "SELECT COUNT(*) FROM [dbo].[tblProjectCategory] WHERE [ProjectId] = @ProjectId";
            int count = _connection.QueryFirstOrDefault<int>(query, new { ProjectId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [iGuruPrep].[dbo].[tblProjectCategory]
                          WHERE [ProjectId] = @ProjectId;";
                var rowsAffected = _connection.Execute(deleteDuery, new { ProjectId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblProjectCategory] ([APId], [ProjectId], [APName])
                          VALUES (@APId, @ProjectId, @APName);";
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
                var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblProjectCategory] ([APId], [ProjectId], [APName])
                          VALUES (@APId, @ProjectId, @APName);";
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
            string query = "SELECT COUNT(*) FROM [dbo].[tblProjectClass] WHERE [ProjectID] = @ProjectID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { ProjectID = ProjectId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [iGuruPrep].[dbo].[tblProjectClass]
                          WHERE [ProjectID] = @ProjectID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { ProjectID = ProjectId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblProjectClass] ([ProjectID], [ClassID])
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
                var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblProjectClass] ([ProjectID], [ClassID])
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
            string query = "SELECT COUNT(*) FROM [dbo].[tblProjectBoard] WHERE [ProjectID] = @ProjectID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { ProjectID = ProjectId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [iGuruPrep].[dbo].[tblProjectBoard]
                          WHERE [ProjectID] = @ProjectID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { ProjectID = ProjectId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblProjectBoard] ([ProjectID], [BoardID])
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
                var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblProjectBoard] ([ProjectID], [BoardID])
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
            string query = "SELECT COUNT(*) FROM [dbo].[tblProjectCourse] WHERE [ProjectID] = @ProjectID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { ProjectID = ProjectId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [iGuruPrep].[dbo].[tblProjectCourse]
                          WHERE [ProjectID] = @ProjectID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { ProjectID = ProjectId});
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblProjectCourse] ([ProjectID], [CourseID])
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
                var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblProjectCourse] ([ProjectID], [CourseID])
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
            string query = "SELECT COUNT(*) FROM [dbo].[tblProjectExamType] WHERE [ProjectID] = @ProjectID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { ProjectID = ProjectId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [iGuruPrep].[dbo].[tblProjectExamType]
                          WHERE [ProjectID] = @ProjectID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { ProjectID = ProjectId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblProjectExamType] ([ProjectID], [ExamTypeID])
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
                var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblProjectExamType] ([ProjectID], [ExamTypeID])
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
            string query = "SELECT COUNT(*) FROM [dbo].[tblProjectSubject] WHERE [ProjectID] = @ProjectID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { ProjectID = ProjectId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [iGuruPrep].[dbo].[tblProjectSubject]
                          WHERE [ProjectID] = @ProjectID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { ProjectID = ProjectId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblProjectSubject] ([ProjectID], [SubjectID])
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
                var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblProjectSubject] ([ProjectID], [SubjectID])
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
        private List<ProjectBoard> GetListOfProjectBoards(int ProjectId)
        {
            var boardquery = @"SELECT * FROM [iGuruPrep].[dbo].[tblProjectBoard] WHERE ProjectID = @ProjectID;";

            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<ProjectBoard>(boardquery, new { ProjectID = ProjectId });
            return data != null ? data.AsList() : [];
        }
        private List<ProjectCategory> GetListOfProjectCategory(int ProjectId)
        {
            var query = @"SELECT * FROM [iGuruPrep].[dbo].[tblProjectCategory] WHERE  ProjectId = @ProjectId;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<ProjectCategory>(query, new { ProjectId });
            return data != null ? data.AsList() : [];
        }
        private List<ProjectClass> GetListOfProjectClass(int ProjectId)
        {
            var query = @"SELECT * FROM [iGuruPrep].[dbo].[tblProjectClass] WHERE  ProjectID = @ProjectID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<ProjectClass>(query, new { ProjectID = ProjectId });
            return data != null ? data.AsList() : [];
        }
        private List<ProjectCourse> GetListOfProjectCourse(int ProjectId)
        {
            var query = @"SELECT * FROM [iGuruPrep].[dbo].[tblProjectCourse] WHERE  ProjectID = @ProjectID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<ProjectCourse>(query, new { ProjectID = ProjectId });
            return data != null ? data.AsList() : [];
        }
        private List<ProjectExamType> GetListOfProjectExamType(int ProjectId)
        {
            var query = @"SELECT * FROM [iGuruPrep].[dbo].[tblProjectExamType] WHERE  bookID = @bookID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<ProjectExamType>(query, new { ProjectID = ProjectId });
            return data != null ? data.AsList() : [];
        }
        private List<ProjectSubject> GetListOfProjectSubject(int ProjectId)
        {
            var query = @"SELECT * FROM [iGuruPrep].[dbo].[tblProjectSubject] WHERE  ProjectID = @ProjectID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<ProjectSubject>(query, new { ProjectID = ProjectId });
            return data != null ? data.AsList() : [];
        }

    }
}
