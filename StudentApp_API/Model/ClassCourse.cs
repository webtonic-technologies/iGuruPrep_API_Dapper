namespace StudentApp_API.Models
{
    public class ClassCourse
    {
        public int CourseClassMappingID { get; set; }
        public int CourseID { get; set; }
        public int ClassID { get; set; }
        public string ClassName { get; set; }
        public string CourseName { get; set; }
        public bool Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public int EmployeeID { get; set; }
        public string EmpFirstName { get; set; }
    }
}
