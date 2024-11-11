namespace StudentApp_API.DTOs.Responses
{
    public class AssignScholarshipResponse
    {
        public int RegistrationID { get; set; }
        public bool IsAssigned { get; set; }
        public string Message { get; set; }
    }
}
