using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace Config_API.DTOs.Response
{
    public class PartialMarksResponse
    {
        public int RuleId { get; set; }
        public int QuestionTypeId { get; set; }
        public string QuestionTypeName {  get; set; }
        public string RuleName { get; set; }
        public List<PartialMarksMappings> PartialMarks { get; set; }
    }
    public class PartialMarksMappings
    {
        public int MappingId { get; set; }
        public int PartialMarksId { get; set; }
        public decimal MarksPerQuestion { get; set; }
        public int NoOfCorrectOptions { get; set; }
        public int NoOfOptionsSelected { get; set; }
        public int SuccessRate { get; set; }
    }
}
