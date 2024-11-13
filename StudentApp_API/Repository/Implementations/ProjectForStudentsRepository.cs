using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using System.Data;
using System.Data.SqlClient;

namespace StudentApp_API.Repository.Implementations
{
    public class ProjectForStudentsRepository : IProjectForStudentsRepository
    {
        private readonly IDbConnection _connection;

        public ProjectForStudentsRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<List<ProjectForStudentsResponse>>> GetAllProjects(ProjectForStudentsRequest request)
        {
            try
            {
                var data = await _connection.QueryFirstOrDefaultAsync(@"select * from tblStudentClassCourseMapping where RegistrationID = 
                @RegistrationID", new { RegistrationID = request.RegistrationId });
                // Base query to retrieve projects based on filters
                var query = @"
                SELECT 
                    p.ProjectId,
                    p.ProjectName,
                    p.ProjectDescription,
                    p.Image,
                    p.ReferenceLink,
                    p.status,
                    p.createdon,
                    p.createdby,
                    p.modifiedon,
                    p.modifiedby,
                    p.EmployeeID,
                    p.pdfVideoFile,
                    emp.EmpFirstName AS EmpFirstName
                FROM tblProject p
                LEFT JOIN tblEmployee emp ON p.EmployeeID = emp.EmployeeID
                -- Join the mapping tables for Board, Class, Course, and Subject
                LEFT JOIN tblProjectBoard pb ON p.ProjectId = pb.ProjectID
                LEFT JOIN tblProjectClass pc ON p.ProjectId = pc.ProjectID
                LEFT JOIN tblProjectCourse prc ON p.ProjectId = prc.ProjectID
                LEFT JOIN tblProjectSubject ps ON p.ProjectId = ps.ProjectID
                WHERE 
                    (@CourseID = 0 OR prc.CourseID = @CourseID) AND
                    (@ClassID = 0 OR pc.ClassID = @ClassID) AND
                    (@BoardID = 0 OR pb.BoardID = @BoardID)
                    -- Include this condition only if a SubjectID is provided
                    AND (@SubjectID = 0 OR ps.SubjectID = @SubjectID)";



                // Prepare parameters
                var parameters = new
                {
                    CourseID = data.CourseID,
                    ClassID = data.ClassID,
                    BoardID = data.BoardId,
                    request.SubjectID
                };

                // Fetch the filtered list of projects
                var projectList = await _connection.QueryAsync<ProjectForStudentsResponse>(query, parameters);
                // Fetch related categories, boards, classes, courses, subjects, and exam types for each project
                
                int totalCount = projectList.Count();

                var paginatedResults = projectList
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();
                return new ServiceResponse<List<ProjectForStudentsResponse>>(true, "Records found", paginatedResults, 200);
            }
           
            catch(Exception ex)
            {
                return new ServiceResponse<List<ProjectForStudentsResponse>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<List<ProjectSubjectCountResponse>>> GetSubjectProjectCounts(ProjectForStudentRequest request)
        {
            try
            {
                var data = await _connection.QueryFirstOrDefaultAsync(@"select * from tblStudentClassCourseMapping where RegistrationID = 
                @RegistrationID", new { RegistrationID = request.RegistrationId });
               
                var subjectCountQuery = @"
    SELECT ps.SubjectID, s.SubjectName, COUNT(p.ProjectId) AS ProjectCount
    FROM tblProject p
    INNER JOIN tblProjectSubject ps ON p.ProjectId = ps.ProjectID
    INNER JOIN tblSubject s ON ps.SubjectID = s.SubjectID
    -- Join the mapping tables for Board, Class, and Course
    INNER JOIN tblProjectBoard pb ON pb.ProjectID = p.ProjectId
    INNER JOIN tblProjectClass pc ON pc.ProjectID = p.ProjectId
    INNER JOIN tblProjectCourse pcou ON pcou.ProjectID = p.ProjectId
    WHERE 
        (@CourseID IS NULL OR pcou.CourseID = @CourseID) AND
        (@ClassID IS NULL OR pc.ClassID = @ClassID) AND
        (@BoardID IS NULL OR pb.BoardID = @BoardID)
    GROUP BY ps.SubjectID, s.SubjectName";

                var subjectCounts = await _connection.QueryAsync<ProjectSubjectCountResponse>(subjectCountQuery, new
                {
                    data.CourseID,
                    data.ClassID,
                    BoardID = data.BoardId
                });


                return new ServiceResponse<List<ProjectSubjectCountResponse>>(true, "Records found", subjectCounts.ToList(), 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ProjectSubjectCountResponse>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<ProjectForStudentsResponse>> GetProjectByIdAsync(int projectId)
        {
            try
            {
                var response = new ProjectForStudentsResponse();

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
                    //response.ProjectCourses = GetListOfProjectCourse(response.ProjectId);
                    //response.ProjectClasses = GetListOfProjectClass(response.ProjectId);
                    //response.ProjectExamTypes = GetListOfProjectExamType(response.ProjectId);
                    //response.ProjectBoards = GetListOfProjectBoards(response.ProjectId);
                    //response.ProjectSubjects = GetListOfProjectSubject(response.ProjectId);
                    //response.ProjectCategories = GetListOfProjectCategory(response.ProjectId);
                    return new ServiceResponse<ProjectForStudentsResponse>(true, "Operation Successful", response, 200);
                }
                else
                {
                    return new ServiceResponse<ProjectForStudentsResponse>(false, "Opertion Failed", new ProjectForStudentsResponse(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ProjectForStudentsResponse>(false, ex.Message, new ProjectForStudentsResponse(), 500);
            }
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
    WHERE pc.ProjectID = @ProjectID;"; // Check for active classes
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
