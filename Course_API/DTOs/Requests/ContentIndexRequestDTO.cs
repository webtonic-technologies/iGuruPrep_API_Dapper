namespace Course_API.DTOs.Requests
{
    public class ContentIndexRequestDTO
    {
        public int APId {  get; set; }
        public int ExamTypeId {  get; set; }
        public int SubjectId { get; set; }
        public int classid { get; set; }
        public int courseid { get; set; }
        public int boardid { get; set; }
        public int PageSize {  get; set; }
        public int PageNumber {  get; set; }
    }
}
