namespace ControlPanel_API.DTOs.Requests
{
    public class GetEmployeeListDTO
    {
        public int RoleId { get; set; }
        public int DesignationId { get; set; }
        public string SearchText { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}