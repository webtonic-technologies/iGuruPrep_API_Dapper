namespace StudentApp_API.DTOs.Response
{
    public class ForgetPasswordResponse
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserType { get; set; }
        public string Email { get; set; }
        public string Token { get; set; } = string.Empty;
    }
}
