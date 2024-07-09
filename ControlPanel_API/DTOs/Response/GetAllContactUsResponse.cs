namespace ControlPanel_API.DTOs.Response
{
    public class GetAllContactUsResponse
    {
        public int ContactusID { get; set; }
        public int Querytype {  get; set; }
        public string QueryTypeName { get; set; } = string.Empty;
        public string QuerytypeDescription { get; set; } = string.Empty;
        public int EmployeeID {  get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
        public string phonenumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int boardid { get; set; }
        public string Board { get; set; } = string.Empty;
        public int classid {  get; set; }
        public string Class { get; set; } = string.Empty;
        public int courseid {  get; set; }
        public string Course { get; set; } = string.Empty;
        public int APID {  get; set; }
        public string Category { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public DateTime? DateTime { get; set; }
        public int RQSID {  get; set; }
        public string RQSName { get; set; } = string.Empty;
        public int ExamTypeId {  get; set; }
        public string ExamTypeName {  get; set; } = string.Empty;
    }
}