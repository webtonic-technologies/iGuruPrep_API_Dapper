namespace Config_API.DTOs.Requests
{
    public class PartialMarksRequest
    {
        public int QuestionTypeId { get; set; }
        public string RuleName {  get; set; }
    }
    public class GetListRequest
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}