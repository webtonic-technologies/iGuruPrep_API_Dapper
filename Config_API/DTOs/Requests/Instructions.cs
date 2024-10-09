namespace Config_API.DTOs.Requests
{
    public class Instructions
    {
        public int InstructionId { get; set; }
        public string InstructionName { get; set; } = string.Empty;
        public string InstructionsDescription { get; set; } = string.Empty;
    }
}
