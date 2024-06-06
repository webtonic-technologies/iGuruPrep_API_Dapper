namespace ControlPanel_API.Models
{
    public class ContactUs
    {
        public int ContactusID { get; set; }
        public int boardid { get; set; }
        public int classid { get; set; }
        public int courseid { get; set; }
        public int Querytype { get; set; }
        public string QuerytypeDescription { get; set; } = string.Empty;
        public DateTime? DateTime { get; set; }
        public bool? RQSID { get; set; }
        public int APID { get; set; }
        public int EmployeeID { get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
        public string boardame { get; set; } = string.Empty;
        public string classname { get; set; } = string.Empty;
        public string coursename { get; set; } = string.Empty;
        public string APName { get; set; } = string.Empty;
        public string phonenumber { get; set; } = string.Empty;
        public string RQSName { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
    }
}
