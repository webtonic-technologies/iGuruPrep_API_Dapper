public class Board
{
    public int BoardId { get; set; }
    public string BoardName { get; set; } = string.Empty;
    public string BoardCode { get; set; } = string.Empty;
    public bool? Status { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? CreatedOn { get; set; }
    public int? ModifiedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public bool? ShowCourse { get; set; }
}