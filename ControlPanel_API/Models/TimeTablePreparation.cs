namespace ControlPanel_API.Models
{
    public class TimeTablePreparation
    {
        public int PreparationTimeTableId { get; set; }
        //BoardId
        // ,[ClassId]
        // ,[CourseId]
        public string TTTitle { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        // ,[SubjectId]
        // ,[SubjectIndexId]
        public bool Status { get; set; }
        // ,[APID]
        //,[boardname]
        //,[classname]
        //,[coursename]
        //,[APname]
        //,[subjectname]
        public DateTime modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        // ,[EmpFirstName]
        public int IndexTypeId { get; set; }
        public int ContentIndexId { get; set; }
    }
}
