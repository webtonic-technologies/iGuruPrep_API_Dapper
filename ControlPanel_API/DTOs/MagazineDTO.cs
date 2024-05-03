namespace ControlPanel_API.DTOs
{
    public class MagazineDTO
    {
        public int MagazineId { get; set; }
        public DateTime? Date { get; set; }
        public string Time { get; set; } = string.Empty;
        public string PathURL { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string MagazineTitle { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime? createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
        public List<MagazineCategory>? MagazineCategories { get; set; }
        public List<MagazineBoard>? MagazineBoards { get; set; }
        public List<MagazineClass>? MagazineClasses { get; set; }
        public List<MagazineCourse>? MagazineCourses { get; set; }
        public List<MagazineExamType>? MagazineExamTypes { get; set; }
    }
    public class MagazineCategory
    {
        public int MgCategoryID { get; set; }
        public int APID { get; set; }
        public int MagazineId { get; set; } //Academic-Professional Id
        public string APIDName { get; set; } = string.Empty;
    }
    public class MagazineBoard
    {
        public int MagazineBoardId { get; set; }
        public int MagazineID { get; set; }
        public int BoardIDID { get; set; }
    }
    public class MagazineClass
    {
        public int MagazineClassId { get; set; }
        public int MagazineID { get; set; }
        public int ClassID { get; set; }
    }
    public class MagazineCourse
    {
        public int MagazineCourseID { get; set; }
        public int MagazineID { get; set; }
        public int CourseID { get; set; }
    }
    public class MagazineExamType
    {
        public int MagazineExamTypeID { get; set; }
        public int MagazineID { get; set; }
        public int ExamTypeID { get; set; }
    }
    public class MagazineListDTO
    {
        public int APID { get; set; }
        public int BoardId { get; set; }
        public int ClassId { get; set; }
        public int CourseId { get; set; }
        public int EventTypeId { get; set; }
        public int ExamType { get; set; }
    }
}
