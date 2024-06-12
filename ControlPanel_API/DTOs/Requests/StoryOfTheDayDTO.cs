using System.ComponentModel.DataAnnotations;

namespace ControlPanel_API.DTOs.Requests
{
    public class StoryOfTheDayDTO
    {

        public int StoryId { get; set; }
        [Required(ErrorMessage = "Event type cannot be empty")]
        public int EventTypeID { get; set; }
       // public string EventName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Post time cannot be empty")]
        public string Event1Posttime { get; set; } = string.Empty;
        [Required(ErrorMessage = "Date cannot be empty")]
        public DateTime? Event1PostDate { get; set; }
        [Required(ErrorMessage = "Date cannot be empty")]
        public DateTime? Event2PostDate { get; set; }
        [Required(ErrorMessage = "Post time cannot be empty")]
        public string Event2Posttime { get; set; } = string.Empty;
        public string modifiedby { get; set; } = string.Empty;
        public string createdby { get; set; } = string.Empty;
        [Required(ErrorMessage = "Event name cannot be empty")]
        public string eventtypename { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public DateTime? createdon { get; set; }
        public bool Status { get; set; }
        public int EmployeeID { get; set; }
        public string Filename1 { get; set; } = string.Empty;
        public string Filename2 { get; set; } = string.Empty;
        // public string EmpFirstName { get; set; } = string.Empty;
        public List<SOTDCategory>? SOTDCategories { get; set; }
        public List<SOTDBoard>? SOTDBoards { get; set; }
        public List<SOTDClass>? SOTDClasses { get; set; }
        public List<SOTDCourse>? SOTDCourses { get; set; }
        public List<SOTDExamType>? SOTDExamTypes { get; set; }
    }
    public class SOTDCategory
    {
        public int SOTDCategoryID { get; set; }
        public int SOTDID { get; set; }
        public int APID { get; set; } //Academic-Professional Id
        //public string APIDName { get; set; } = string.Empty;
    }
    public class SOTDBoard
    {
        public int tblSOTDBoardID { get; set; }
        public int SOTDID { get; set; }
        public int BoardID { get; set; }
        // public string Name { get; set; } = string.Empty;
    }
    public class SOTDClass
    {
        public int tblSOTDClassID { get; set; }
        public int SOTDID { get; set; }
        public int ClassID { get; set; }
        //  public string Name { get; set; } = string.Empty;
    }
    public class SOTDCourse
    {
        public int SOTDCourseID { get; set; }
        public int SOTDID { get; set; }
        public int CourseID { get; set; }
        //  public string Name { get; set; } = string.Empty;
    }
    public class SOTDExamType
    {
        public int SOTDExamTypeID { get; set; }
        public int SOTDID { get; set; }
        public int ExamTypeID { get; set; }
        // public string Name { get; set; } = string.Empty;
    }
    public class SOTDListDTO
    {
        public int APID { get; set; }
        public int BoardID { get; set; }
        public int ClassID { get; set; }
        public int CourseID { get; set; }
        public int ExamTypeID { get; set; }
        public int EventTypeID { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
