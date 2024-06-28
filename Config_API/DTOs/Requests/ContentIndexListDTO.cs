namespace Config_API.DTOs.Requests
{
    public class ContentIndexListDTO
    {
        public int APID { get; set; }
        public int SubjectId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    public class ContentIndexMastersDTO
    {
        public int APID { get; set; }
        public int SubjectId { get; set; }
    }
}
