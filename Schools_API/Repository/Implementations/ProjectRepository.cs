using Schools_API.DTOs;
using Schools_API.DTOs.ServiceResponse;
using Schools_API.Models;
using Schools_API.Repository.Interfaces;
using System.Data;
using Dapper;
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
        public async Task<ServiceResponse<string>> AddProjectAsync(ProjectDTO projectDTO)
        {
            try
            {
                string imagePath = string.Empty;
                if (projectDTO.image != null)
                {
                    var folderName = Path.Combine(_hostingEnvironment.ContentRootPath, "ProjectImages");
                    if (!Directory.Exists(folderName))
                    {
                        Directory.CreateDirectory(folderName);
                    }
                    var fileName = Path.GetFileNameWithoutExtension(projectDTO.image.FileName) + "_" + Guid.NewGuid().ToString() + Path.GetExtension(projectDTO.image.FileName);
                    var filePath = Path.Combine(folderName, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        projectDTO.image.CopyTo(fileStream);
                    }
                    imagePath = fileName; // Set the path where the image is saved
                }
                else
                {
                    imagePath = string.Empty;
                }

                var project = new Project
                {
                    ProjectName = projectDTO.ProjectName,
                    ProjectDescription = projectDTO.ProjectDescription,
                    PathURL = imagePath,
                    BoardId = projectDTO.BoardId,
                    ClassId = projectDTO.ClassId,
                    CourseId = projectDTO.CourseId,
                    SubjectId = projectDTO.SubjectId,
                    CreatedBy = projectDTO.CreatedBy,
                    modifiedby = projectDTO.modifiedby,
                    modifiedon = projectDTO.modifiedon,
                    ReferenceLink = projectDTO.ReferenceLink,
                    status = projectDTO.status,
                    UserID = projectDTO.UserID
                };
                var query = @"
            INSERT INTO tblProject (ProjectName, ProjectDescription, PathURL, CourseId, ClassId, BoardId, SubjectId, CreatedBy, ReferenceLink, UserID, status, modifiedby, modifiedon)
            VALUES (@ProjectName, @ProjectDescription, @PathURL, @CourseId, @ClassId, @BoardId, @SubjectId, @CreatedBy, @ReferenceLink, @UserID, @status, @modifiedby, @modifiedon);";

                int rowsAffected = await _connection.ExecuteAsync(query, project);

                if (rowsAffected > 0)
                {
                    return new ServiceResponse<string>(true, "Operation Successful", "Project Added Successfully", 200);
                }
                else
                {
                    return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
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

        public async Task<ServiceResponse<ProjectDetailsDTO>> GetProjectByIdAsync(int projectId)
        {
            try
            {
                    var project = await _connection.QueryFirstOrDefaultAsync<ProjectDetailsDTO>(
                        @"SELECT *
                  FROM tblproject p
                  INNER JOIN tblCourse c ON p.CourseId = c.CourseId
                  INNER JOIN tblClass cl ON p.ClassId = cl.ClassId
                  INNER JOIN tblBoard b ON p.BoardId = b.BoardId
                  INNER JOIN tblSubject s ON p.SubjectId = s.SubjectId
                  WHERE p.ProjectId = @ProjectId",
                        new { ProjectId = projectId });

                    // If project is found, append full path to ImageName
                    if (project != null && !string.IsNullOrEmpty(project.ImageName))
                    {
                        project.PathURL = Path.Combine(_hostingEnvironment.ContentRootPath, "ProjectImages", project.ImageName);
                    }

                    // If project is not found, throw KeyNotFoundException
                    if (project == null)
                    {
                        throw new KeyNotFoundException("Project not found.");
                    }


                if (project != null)
                {
                    return new ServiceResponse<ProjectDetailsDTO>(true, "Operation Successful", project, 200);
                }
                else
                {
                    return new ServiceResponse<ProjectDetailsDTO>(false, "Opertion Failed", new ProjectDetailsDTO(), 500);
                }

            }
            catch (Exception ex)
            {
                return new ServiceResponse<ProjectDetailsDTO>(false, ex.Message, new ProjectDetailsDTO(), 500);
            }
        }
    }
}
