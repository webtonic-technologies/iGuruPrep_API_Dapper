using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ControlPanel_API.DTOs.Requests
{
    public class TimeTablePreparationRequest
    {
        public int PreparationTimeTableId { get; set; }
        public string TTTitle { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public bool Status { get; set; }
        public DateTime modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public List<TimeTableBoard>? TimeTableBoards { get; set; }
        public List<TimeTableSubject>? TimeTableSubjects { get; set; }
        public List<TimeTableExamType>? TimeTableExamTypes { get; set; }
        public List<TimeTableCourse>? TimeTableCourses { get; set; }
        public List<TimeTableClass>? TimeTableClasses { get; set; }
        public List<TimeTableCategory>? TimeTableCategories { get; set; }
    }
    public class TimeTableBoard
    {
        public int NBTimeTableBoardId { get; set; }
        public int PreparationTimeTableId { get; set; }
        public int BoardId { get; set; }
    }
    public class TimeTableSubject
    {
        public int NBTimeTableSubjectId { get; set; }
        public int PreparationTimeTableId { get; set; }
        public int SubjectId { get; set; }
        public List<TTSubjectContentMapping>? TTSubjectContentMappings { get; set; }

    }
    public class TimeTableExamType
    {
        public int NBTimeTableExamTypeId { get; set; }
        public int PreparationTimeTableId { get; set; }
        public int ExamTypeId { get; set; }
    }
    public class TimeTableCourse
    {
        public int NBTimeTableCourseId { get; set; }
        public int PreparationTimeTableId { get; set; }
        public int CourseId { get; set; }
    }
    public class TimeTableClass
    {
        public int NBTimeTableClassId { get; set; }
        public int PreparationTimeTableId { get; set; }
        public int ClassId { get; set; }
    }
    public class TimeTableCategory
    {
        public int NBTimeTableCategoryId { get; set; }
        public int PreparationTimeTableId { get; set; }
        public int CategoryId { get; set; }
    }
    public class TTSubjectContentMapping
    {

        public int NBTTSubContMappingId { get; set; }
        public int PreparationTimeTableId { get; set; }
        public int NBTimeTableSubjectId { get; set; }
        public int IndexTypeId { get; set; }
        public int ContentIndexId { get; set; }
    }
    public class TimeTableListRequestDTO
    {
        public int APID { get; set; }
        public int BoardIDID { get; set; }
        public int ClassID { get; set; }
        public int CourseID { get; set; }
        public int ExamTypeID { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
