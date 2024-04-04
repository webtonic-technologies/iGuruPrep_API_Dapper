namespace Course_API.DTOs
{
    public class SubjectContentIndexDTO
    {
        public int SubjectIndexId { get; set; }
        public int SubjectId { get; set; }
        public string ContentName { get; set; } = string.Empty;
        public int IndexTypeId { get; set; }
        public int ParentLevel { get; set; }
        public int DisplayOrder { get; set; }
        public int IsSubjective { get; set; }
        public int classid { get; set; }
        public int courseid { get; set; }
        public int boardid { get; set; }
    }
    public class SubjectContentIndexRequestDTO
    {
        public int SubjectId { get; set; }
        public int classid { get; set; }
        public int courseid { get; set; }
        public int boardid { get; set; }
    }
}
