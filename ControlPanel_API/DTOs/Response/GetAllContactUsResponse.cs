namespace ControlPanel_API.DTOs.Response
{
    public class GetAllContactUsResponse
    {
        public int ContactusID { get; set; }
        public string QueryTypeName { get; set; } = string.Empty;
        public string QuerytypeDescription { get; set; } = string.Empty;
        public string EmpFirstName { get; set; } = string.Empty;
        public string phonenumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Board { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public DateTime? DateTime { get; set; }
    }
}
