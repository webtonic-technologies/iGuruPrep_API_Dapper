namespace ControlPanel_API.DTOs
{
    public class GetEmployeeListDTO
    {
        public int RoleId {  get; set; }
        public int DesignationId {  get; set; }
        public string SearchText { get; set; } = string.Empty;
    }
}
