namespace StudentApp_API.DTOs.Response
{
    public class ProjectForStudentsResponse
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectDescription { get; set; } = string.Empty;
        public string? Image { get; set; }
        public string createdby { get; set; } = string.Empty;
        public string ReferenceLink { get; set; } = string.Empty;
        public int? EmployeeID { get; set; }
        public bool? status { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public DateTime? createdon { get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
        public string pdfVideoFile { get; set; } = string.Empty;
        //public List<ProjectCategoryResponse>? ProjectCategories { get; set; }
        //public List<ProjectBoardResponse>? ProjectBoards { get; set; }
        //public List<ProjectClassResponse>? ProjectClasses { get; set; }
        //public List<ProjectCourseResponse>? ProjectCourses { get; set; }
        //public List<ProjectExamTypeResponse>? ProjectExamTypes { get; set; }
        //public List<ProjectSubjectResponse>? ProjectSubjects { get; set; }
    }
    public class ProjectCategoryResponse
    {
        public int ProjectCategoryId { get; set; }
        public int APID { get; set; }
        public int ProjectId { get; set; }
        public string APName { get; set; } = string.Empty;
    }
    public class ProjectBoardResponse
    {
        public int ProjectBoardID { get; set; }
        public int ProjectID { get; set; }
        public int BoardID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class ProjectClassResponse
    {
        public int ProjectClassID { get; set; }
        public int ProjectID { get; set; }
        public int ClassID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class ProjectCourseResponse
    {
        public int ProjectCourseID { get; set; }
        public int ProjectID { get; set; }
        public int CourseID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class ProjectExamTypeResponse
    {
        public int ProjectExamTypeID { get; set; }
        public int ProjectID { get; set; }
        public int ExamTypeID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class ProjectSubjectResponse
    {
        public int ProjectSubjectID { get; set; }
        public int ProjectID { get; set; }
        public int SubjectID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class ProjectSubjectCountResponse
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; }
        public int ProjectCount { get; set; }
    }
}
