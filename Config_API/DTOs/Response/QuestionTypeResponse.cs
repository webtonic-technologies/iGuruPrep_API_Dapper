namespace Config_API.DTOs.Response
{
    public class QuestionTypeResponse { 
        public int QuestionTypeID { get; set; }
        public string QuestionType { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public bool Status { get; set; }
        public int MinNoOfOptions { get; set; }
        public DateTime modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public int TypeOfOptionId { get; set; }
        public string TypeOfOptionName { get; set; } = string.Empty;

    }
}
