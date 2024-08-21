namespace ControlPanel_API.Models
{
    public class Notification
    {
        public int NBNotificationID { get; set; }
        public string NotificationTitle { get; set; } = string.Empty;
        //public int CourseID { get; set; }
        public string PDF { get; set; } = string.Empty;
        //public int ClassID { get; set; }
        //public int BoardId { get; set; }
        //public int NBNID { get; set; }
        //public int NBNLID { get; set; }
        public bool status { get; set; }
        //public int APID { get; set; }
        //public string boardname { get; set; } = string.Empty;
        //public string classname { get; set; } = string.Empty;
        //public string coursename { get; set; } = string.Empty;
        //public string APname { get; set; } = string.Empty;
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime? createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        //public string EmpFirstName { get; set; } = string.Empty;
    }
}