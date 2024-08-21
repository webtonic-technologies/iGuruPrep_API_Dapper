using ControlPanel_API.Models;

namespace ControlPanel_API.DTOs.Response
{
    public class NotificationResponseDTO
    {
        public int NBNotificationID { get; set; }
        public string NotificationTitle { get; set; } = string.Empty;
        public string PDF { get; set; } = string.Empty;
        public bool status { get; set; }
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime? createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
        public List<NbNotificationCategoryResponse>? NbNotificationCategories { get; set; }
        public List<NbNotificationBoardResponse>? NbNotificationBoards { get; set; }
        public List<NbNotificationClassResponse>? NbNotificationClasses { get; set; }
        public List<NbNotificationCourseResponse>? NbNotificationCourses { get; set; }
        public List<NbNotificationExamTypeResponse>? NbNotificationExamTypes { get; set; }
        public List<NotificationLinkMaster>? NotificationLinkMasters { get; set; }
        public List<NotificationDetail>? NotificationDetails { get; set; }
    }
    public class NbNotificationCategoryResponse
    {
        public int NbNotificationCategoryId { get; set; }
        public int APID { get; set; }
        public int NBNotificationID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class NbNotificationBoardResponse
    {
        public int NbNotificationBoardId { get; set; }
        public int NBNotificationID { get; set; }
        public int BoardID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class NbNotificationClassResponse
    {
        public int NbNotificationClassId { get; set; }
        public int NBNotificationID { get; set; }
        public int ClassID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class NbNotificationCourseResponse
    {
        public int NbNotificationCourseId { get; set; }
        public int NBNotificationID { get; set; }
        public int CourseID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class NbNotificationExamTypeResponse
    {
        public int NbNotificationExamTypeId { get; set; }
        public int NBNotificationID { get; set; }
        public int ExamTypeID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
