namespace Config_API.DTOs.Requests
{
    public class PartialMarksRequest
    {
        public int QuestionTypeId { get; set; }
        public string RuleName {  get; set; }
    }
}