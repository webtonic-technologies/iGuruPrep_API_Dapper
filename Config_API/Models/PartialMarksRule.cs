namespace Config_API.Models
{
    public class PartialMarksRule
    {
        public int QuestionTypeId { get; set; }
        public string RuleName { get; set; }
        public decimal MarksPerQuestion { get; set; }
        public int NoOfCorrectOptions { get; set; }
        public int NumberOfOptionsSelected { get; set; }
        public int SuccessRate { get; set; }
    }

}
