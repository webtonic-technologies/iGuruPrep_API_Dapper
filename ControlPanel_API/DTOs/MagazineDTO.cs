namespace ControlPanel_API.DTOs
{
    public class MagazineDTO
    {
        public int? MagazineId { get; set; }
        public string MagazineName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public IFormFile? File { get; set; } = null;
    }

    public class UpdateMagazineDTO
    {
        public int? MagazineId { get; set; }
        public string MagazineName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
    }
}
