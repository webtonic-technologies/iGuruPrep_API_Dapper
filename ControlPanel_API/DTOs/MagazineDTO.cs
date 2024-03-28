namespace ControlPanel_API.DTOs
{
    public class MagazineDTO
    {
        public int? MagazineId { get; set; }
        public string MagazineName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public IFormFile? File { get; set; } = null;
        public string MagazineTitle { get; set; } = string.Empty;
        public bool? Status { get; set; }
    }

    public class UpdateMagazineDTO
    {
        public int? MagazineId { get; set; }
        public string MagazineName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string MagazineTitle { get; set; } = string.Empty;
        public bool? Status { get; set; }
    }
}
