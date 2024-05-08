namespace Config_API.Models
{
    public class Questiontype
    {
        public int QuestionTypeID { get; set; }
        public string QuestionType { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool Status { get; set; }
        public int MinNoOfOptions {  get; set; }
        public DateTime modifiedon {  get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime createdon {  get; set; }
        public string createdby { get; set; } = string.Empty;
        public int EmployeeID {  get; set; }
        public int TypeOfOption {  get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
    }
}