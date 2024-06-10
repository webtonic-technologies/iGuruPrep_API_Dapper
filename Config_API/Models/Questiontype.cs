using System.ComponentModel.DataAnnotations;

namespace Config_API.Models
{
    public class Questiontype
    {
        public int QuestionTypeID { get; set; }
        [Required(ErrorMessage = "Question type cannot be empty")]
        public string QuestionType { get; set; } = string.Empty;
        [Required(ErrorMessage = "Code cannot be empty")]
        public string Code { get; set; } = string.Empty;
        [Required(ErrorMessage = "Question cannot be empty")]
        public string Question { get; set; } = string.Empty;
        public bool Status { get; set; }
        [Required(ErrorMessage = "Number of options cannot be empty")]
        public int MinNoOfOptions {  get; set; }
        public DateTime modifiedon {  get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime createdon {  get; set; }
        public string createdby { get; set; } = string.Empty;
        public int EmployeeID {  get; set; }
        [Required(ErrorMessage = "Type of options cannot be empty")]
        public int TypeOfOption {  get; set; }
        //public string EmpFirstName { get; set; } = string.Empty;
    }
}