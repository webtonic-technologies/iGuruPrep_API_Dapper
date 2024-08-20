using System.ComponentModel.DataAnnotations;

namespace Schools_API.DTOs.Requests
{
    public class ProjectDTO
    {
        public int ProjectId { get; set; }
        [Required(ErrorMessage = "Project Title cannot be empty")]
        public string ProjectName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Project Description cannot be empty")]
        public string ProjectDescription { get; set; } = string.Empty;
        public string? Image { get; set; }
        public string createdby { get; set; } = string.Empty;
        public string ReferenceLink { get; set; } = string.Empty;
        public int? EmployeeID { get; set; }
        public bool? status { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public DateTime? createdon { get; set; }
        // public string EmpFirstName { get; set; } = string.Empty;
        public string pdfVideoFile { get; set; } = string.Empty;
        public List<ProjectCategory>? ProjectCategories { get; set; }
        public List<ProjectBoard>? ProjectBoards { get; set; }
        public List<ProjectClass>? ProjectClasses { get; set; }
        public List<ProjectCourse>? ProjectCourses { get; set; }
        public List<ProjectExamType>? ProjectExamTypes { get; set; }
        public List<ProjectSubject>? ProjectSubjects { get; set; }
    }
    public class ProjectCategory
    {
        public int ProjectCategoryId { get; set; }
        public int APID { get; set; }
        public int ProjectId { get; set; } //Academic-Professional Id
        //public string APName { get; set; } = string.Empty;
    }
    public class ProjectBoard
    {
        public int ProjectBoardID { get; set; }
        public int ProjectID { get; set; }
        public int BoardID { get; set; }
    }
    public class ProjectClass
    {
        public int ProjectClassID { get; set; }
        public int ProjectID { get; set; }
        public int ClassID { get; set; }
    }
    public class ProjectCourse
    {
        public int ProjectCourseID { get; set; }
        public int ProjectID { get; set; }
        public int CourseID { get; set; }
    }
    public class ProjectExamType
    {
        public int ProjectExamTypeID { get; set; }
        public int ProjectID { get; set; }
        public int ExamTypeID { get; set; }
    }
    public class ProjectSubject
    {
        public int ProjectSubjectID { get; set; }
        public int ProjectID { get; set; }
        [Required(ErrorMessage = "Subject name cannot be empty")]
        public int SubjectID { get; set; }
    }
}
