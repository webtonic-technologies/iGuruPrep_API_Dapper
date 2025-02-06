using Course_API.DTOs.Requests;

namespace Course_API.DTOs.Response
{
    public class ScholarshipTestResponseDTO
    {
        public int ScholarshipTestId { get; set; }
        public int APID { get; set; }
        public string APName { get; set; } = string.Empty;
        public int ExamTypeId { get; set; }
        public string ExamTypeName { get; set; } = string.Empty;
        public string PatternName { get; set; } = string.Empty;
        public int TotalNumberOfQuestions { get; set; }
        public string Duration { get; set; } = string.Empty;
        public bool Status { get; set; }
        public DateTime? createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
        public List<ScholarshipBoardsResponse>? ScholarshipBoards { get; set; }
        public List<ScholarshipClassResponse>? ScholarshipClasses { get; set; }
        public List<ScholarshipCourseResponse>? ScholarshipCourses { get; set; }
        public List<ScholarshipSubjectsResponse>? ScholarshipSubjects { get; set; }
        public List<ScholarshipSubjectDetails>? ScholarshipSubjectDetails { get; set; }
        // public List<ScholarshipQuestions>?  ScholarshipQuestions { get; set; }
        public List<ScholarshipTestInstructions>? ScholarshipTestInstructions { get; set; }
        public List<ScholarshipTestDiscountScheme>? ScholarshipTestDiscountSchemes { get; set; }
        public List<ScholarshipTestQuestion>? ScholarshipTestQuestions { get; set; }
    }
    public class ScholarshipBoardsResponse
    {
        public int SSTBoardId { get; set; }
        public int ScholarshipTestId { get; set; }
        public int BoardId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class ScholarshipClassResponse
    {
        public int SSTClassId { get; set; }
        public int ScholarshipTestId { get; set; }
        public int ClassId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class ScholarshipCourseResponse
    {
        public int SSTCourseId { get; set; }
        public int ScholarshipTestId { get; set; }
        public int CourseId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class ScholarshipSubjectsResponse
    {
        public int SSTSubjectId { get; set; }
        public int ScholarshipTestId { get; set; }
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
    }
    public class ScholarshipContentIndexResponse
    {

        public int IndexTypeId { get; set; }
        public string IndexTypeName { get; set; } = string.Empty;
        public int ContentIndexId { get; set; }
        public string ContentIndexName { get; set; } = string.Empty;
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public int SSTContIndId { get; set; }
        public int ScholarshipTestId { get; set; }
    }
    public class ScholarshipSubjectDetails
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public List<ScholarshipContentIndexResponse>? ScholarshipContentIndexResponses { get; set; }
        public List<ScholarshipQuestionSection>? ScholarshipQuestionSections { get; set; }
    }
    public class ScholarshipDetailsDTO
    {
        public int ScholarshipTestId { get; set; }
        public string PatternName { get; set; }
        public int TotalNumberOfQuestions { get; set; }
        public string Duration { get; set; } // In minutes
        public string APName { get; set; }
        public string BoardNames { get; set; }
        public string ClassNames { get; set; }
        public string CourseNames { get; set; }
        public List<StudentDetailsDTO> Students { get; set; }
    }

    public class StudentDetailsDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailID { get; set; }
        public string MobileNumber { get; set; }
    }
    public class ScholarshipTestStatusResponse
    {
        public int ScholarshipTestId { get; set; }
        public bool Status { get; set; }
    }

}
