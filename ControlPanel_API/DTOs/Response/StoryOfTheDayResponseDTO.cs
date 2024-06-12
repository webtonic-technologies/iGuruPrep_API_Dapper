namespace ControlPanel_API.DTOs.Response
{
    public class StoryOfTheDayResponseDTO
    {

        public int StoryId { get; set; }
        public int EventTypeID { get; set; }
        public string Event1Posttime { get; set; } = string.Empty;
        public DateTime? Event1PostDate { get; set; }
        public DateTime? Event2PostDate { get; set; }
        public string Event2Posttime { get; set; } = string.Empty;
        public string modifiedby { get; set; } = string.Empty;
        public string createdby { get; set; } = string.Empty;
        public string eventtypename { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public DateTime? createdon { get; set; }
        public bool Status { get; set; }
        public int EmployeeID { get; set; }
        public string Filename1 { get; set; } = string.Empty;
        public string Filename2 { get; set; } = string.Empty;
        public string EmpFirstName { get; set; } = string.Empty;
        public List<SOTDCategoryResponse>? SOTDCategories { get; set; }
        public List<SOTDBoardResponse>? SOTDBoards { get; set; }
        public List<SOTDClassResponse>? SOTDClasses { get; set; }
        public List<SOTDCourseResponse>? SOTDCourses { get; set; }
        public List<SOTDExamTypeResponse>? SOTDExamTypes { get; set; }
    }
    public class SOTDCategoryResponse
    {
        public int SOTDCategoryID { get; set; }
        public int SOTDID { get; set; }
        public int APID { get; set; } //Academic-Professional Id
        public string APIDName { get; set; } = string.Empty;
    }
    public class SOTDBoardResponse
    {
        public int tblSOTDBoardID { get; set; }
        public int SOTDID { get; set; }
        public int BoardID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class SOTDClassResponse
    {
        public int tblSOTDClassID { get; set; }
        public int SOTDID { get; set; }
        public int ClassID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class SOTDCourseResponse
    {
        public int SOTDCourseID { get; set; }
        public int SOTDID { get; set; }
        public int CourseID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class SOTDExamTypeResponse
    {
        public int SOTDExamTypeID { get; set; }
        public int SOTDID { get; set; }
        public int ExamTypeID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

