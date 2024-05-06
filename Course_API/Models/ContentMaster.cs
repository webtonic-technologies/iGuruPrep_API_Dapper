namespace Course_API.Models
{
    public class ContentMaster
    {
        public int contentid { get; set; }
        public int subjectindexid { get; set; }
        public int boardId { get; set; }
        public int classId { get; set; }
        public int courseId { get; set; }
        public int subjectId { get; set; }
        public string fileName { get; set; } = string.Empty;
        public string PathURL { get; set; } = string.Empty;
        public DateTime? createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
    }
}