using System.ComponentModel.DataAnnotations;

namespace ControlPanel_API.DTOs.Response
{
    public class MagazineResponseDTO
    {
        public int MagazineId { get; set; }
        [Required(ErrorMessage = "Date cannot be empty")]
        public DateTime? Date { get; set; }
        [Required(ErrorMessage = "Time cannot be empty")]
        public string Time { get; set; } = string.Empty;
        [Required(ErrorMessage = "url cannot be empty")]
        public string PathURL { get; set; } = string.Empty;
        [Required(ErrorMessage = "file cannot be empty")]
        public string Link { get; set; } = string.Empty;
        [Required(ErrorMessage = "Title cannot be empty")]
        public string MagazineTitle { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime? createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
        public List<MagazineCategoryResponse>? MagazineCategories { get; set; }
        public List<MagazineBoardResponse>? MagazineBoards { get; set; }
        public List<MagazineClassResponse>? MagazineClasses { get; set; }
        public List<MagazineCourseResponse>? MagazineCourses { get; set; }
        public List<MagazineExamTypeResponse>? MagazineExamTypes { get; set; }
    }
    public class MagazineCategoryResponse
    {
        public int MgCategoryID { get; set; }
        public int APID { get; set; }
        public int MagazineId { get; set; }
        public string APIDName { get; set; } = string.Empty;
    }
    public class MagazineBoardResponse
    {
        public int MagazineBoardId { get; set; }
        public int MagazineID { get; set; }
        public int BoardID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class MagazineClassResponse
    {
        public int MagazineClassId { get; set; }
        public int MagazineID { get; set; }
        public int ClassID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class MagazineCourseResponse
    {
        public int MagazineCourseID { get; set; }
        public int MagazineID { get; set; }
        public int CourseID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class MagazineExamTypeResponse
    {
        public int MagazineExamTypeID { get; set; }
        public int MagazineID { get; set; }
        public int ExamTypeID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
