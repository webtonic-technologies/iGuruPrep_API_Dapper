namespace ControlPanel_API.DTOs
{
    public class GeAllTicketsRequest
    {
        public int? boardid { get; set; }
        public int? ClassId { get; set; }
        public int? TicketNo { get; set; } //ticket no is courseID (foreign key)
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? Today { get; set; }
    }
}
