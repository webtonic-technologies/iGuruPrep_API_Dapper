namespace Course_API.DTOs.Response
{
    public class SyllabusResponseDTO
    {
        public int SyllabusId { get; set; }
        public int BoardID { get; set; }
        public int CourseId { get; set; }
        public int ClassId { get; set; }
        public string SyllabusName { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public string? createdby { get; set; }
        public DateTime? createdon { get; set; }
        public string? modifiedby { get; set; }
        public DateTime? modifiedon { get; set; }
        public int? APID { get; set; }
        public int? empid { get; set; }
        public string boardname { get; set; } = string.Empty;
        public string classname { get; set; } = string.Empty;
        public string coursename { get; set; } = string.Empty;
        //public string subjectname { get; set; } = string.Empty;
        public string APname { get; set; } = string.Empty;
        public int? EmployeeID { get; set; }
        public int ExamTypeId { get; set; }
        public string ExamTypeName {  get; set; } = string.Empty;
        public string EmpFirstName { get; set; } = string.Empty;
        public List<SyllabusSubjectResponse>? SyllabusSubjects { get; set; }
    }
    public class SyllabusSubjectResponse
    {
        public int SyllabusSubjectID { get; set; }
        public int SyllabusID { get; set; }
        public int? SubjectID { get; set; }
        public string SubjectName {  get; set; } = string.Empty;
        public bool? Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
    public class SyllabusDetailsResponseDTO
    {
        public int SyllabusId { get; set; }
        public List<SyllabusDetailsResponse>? SyllabusDetails { get; set; }
    }
    public class SyllabusDetailsResponse
    {
        public int SyllabusDetailID { get; set; }
        public int SyllabusID { get; set; }
        public int ContentIndexId { get; set; }
        public string ContentIndexName { get; set; } = string.Empty;
        public int IndexTypeId { get; set; }
        public string IndexTypeName { get; set; } = string.Empty;
        public int Status { get; set; }
        public int? IsVerson { get; set; }
    }
}
