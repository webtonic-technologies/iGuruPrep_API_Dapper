namespace StudentApp_API.DTOs.Responses
{
    public class AssignClassResponse
    {
        public int RegistrationID { get; set; }
        public int CourseID { get; set; }
        public int ClassID { get; set; }
        public bool IsClassAssigned { get; set; }
        public string Message { get; set; }
    }
}
