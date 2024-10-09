namespace Course_API.Models
{
    public class TestSeriesInstructions
    {
        public int TestInstructionsId { get; set; }
        public string Instructions { get; set; } = string.Empty;
        public int TestSeriesID { get; set; }
        public string InstructionName { get; set; } = string.Empty;
        public int InstructionId { get; set; }
    }
}
