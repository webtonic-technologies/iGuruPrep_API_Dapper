using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ControlPanel_API.Models
{
    public class Ticket
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TicketNo { get; set; }
        public string QueryType { get; set; } = string.Empty;
        public int TicketID { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
        public string Boardname { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string QueryInfo { get; set; } = string.Empty;
        public DateTime? DateAndTime { get; set; }
        public int? Status { get; set; }
    }
}
