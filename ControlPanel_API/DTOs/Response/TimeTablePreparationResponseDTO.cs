namespace ControlPanel_API.DTOs.Response
{
    public class TimeTablePreparationResponseDTO
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
        public string EmployeeName { get; set; } = string.Empty;
        public List<TimeTableBoardResponse>? TimeTableBoards { get; set; }
        public List<TimeTableSubjectResponse>? TimeTableSubjects { get; set; }
        public List<TimeTableExamTypeResponse>? TimeTableExamTypes { get; set; }
        public List<TimeTableCourseResponse>? TimeTableCourses { get; set; }
        public List<TimeTableClassResponse>? TimeTableClasses { get; set; }
        public List<TimeTableCategoryResponse>? TimeTableCategories { get; set; }
    }
    public class TimeTableBoardResponse
    {
        public int NBTimeTableBoardId { get; set; }
        public int PreparationTimeTableId { get; set; }
        public int BoardId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class TimeTableSubjectResponse
    {
        public int NBTimeTableSubjectId { get; set; }
        public int PreparationTimeTableId { get; set; }
        public int SubjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<TTSubjectContentMappingResponse>? TTSubjectContentMappings { get; set; }
    }
    public class TTSubjectContentMappingResponse
    {

        public int NBTTSubContMappingId { get; set; }
        public int PreparationTimeTableId { get; set; }
        public int NBTimeTableSubjectId { get; set; }
        public int IndexTypeId { get; set; }
        public string IndexTypeName { get; set; } = string.Empty;
        public int ContentIndexId { get; set; }
        public string ContentIndexName { get; set; } = string.Empty;
    }
    public class TimeTableExamTypeResponse
    {
        public int NBTimeTableExamTypeId { get; set; }
        public int PreparationTimeTableId { get; set; }
        public int ExamTypeId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class TimeTableCourseResponse
    {
        public int NBTimeTableCourseId { get; set; }
        public int PreparationTimeTableId { get; set; }
        public int CourseId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class TimeTableClassResponse
    {
        public int NBTimeTableClassId { get; set; }
        public int PreparationTimeTableId { get; set; }
        public int ClassId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class TimeTableCategoryResponse
    {
        public int NBTimeTableCategoryId { get; set; }
        public int PreparationTimeTableId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}