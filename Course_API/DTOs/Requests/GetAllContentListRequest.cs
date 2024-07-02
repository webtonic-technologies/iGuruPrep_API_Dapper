namespace Course_API.DTOs.Requests
{
    public class GetAllContentListRequest
    {
        public int APId { get; set; }
        public int BoardID { get; set; }
        public int ClassID { get; set; }
        public int CourseID { get; set; }
        public int SubjectID { get; set; }
        public int ExamTypeID { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
