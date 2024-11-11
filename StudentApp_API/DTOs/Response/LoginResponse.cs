namespace StudentApp_API.DTOs.Responses
{
    public class LoginResponse
    {
        public int RegistrationID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailID { get; set; }
        public string MobileNumber { get; set; }
        public string Location { get; set; }
        public bool IsLoginSuccessful { get; set; }
    }
}
