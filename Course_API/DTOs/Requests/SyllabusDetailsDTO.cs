using Course_API.Models;
using System.ComponentModel.DataAnnotations;

namespace Course_API.DTOs.Requests
{
    public class SyllabusDetailsDTO
    {
        public int SyllabusId { get; set; }
        public int SubjectId {  get; set; }
        public List<SyllabusDetails>? SyllabusDetails { get; set; }
    }
    public class SyllabusDTO
    {
        public int SyllabusId { get; set; }
        public int BoardID { get; set; }
        public int CourseId { get; set; }
        public int ClassId { get; set; }
        [Required(ErrorMessage = "Syllabus name cannot be empty")]
        public string SyllabusName { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public string? createdby { get; set; }
        public DateTime? createdon { get; set; }
        public string? modifiedby { get; set; }
        public DateTime? modifiedon { get; set; }
        public int? APID { get; set; }
        public int? EmployeeID { get; set; }
        public int ExamTypeId { get; set; }
        public List<SyllabusSubject>? SyllabusSubjects { get; set; }
    }
    public class UpdateContentIndexNameDTO
    {
        public int ContentIndexId { get; set; }
        public int IndexTypeId { get; set; }
        public string ContentCode { get; set; } = string.Empty;
        public string NewContentIndexName { get; set; } = string.Empty;
    }
    public class GetAllSyllabusList
    {
        public int APID { get; set; }
        public int ClassId { get; set; }
        public int CourseId {  get; set; }
        public int BoardId {  get; set; }
        public int ExamTypeId { get; set; }
    }
}
