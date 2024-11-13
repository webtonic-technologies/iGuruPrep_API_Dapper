namespace StudentApp_API.DTOs.Requests
{
    public class ProjectForStudentsRequest
    {
        public int? RegistrationId {  get; set; }
        public int? SubjectID { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    public class ProjectForStudentRequest
    {
      public int RegistrationId { get; set; }
    }

}
