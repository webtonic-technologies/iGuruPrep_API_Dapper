using ControlPanel_API.Models;

namespace ControlPanel_API.DTOs
{
    public class NotificationDTO
    {
        public int NBNotificationID { get; set; }
        public string NotificationTitle { get; set; } = string.Empty;
        public string PathURL { get; set; } = string.Empty;
        public bool status { get; set; }
        public DateTime? modifiedon { get; set; }
        public string modifiedby { get; set; } = string.Empty;
        public DateTime? createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public int EmployeeID { get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
        public List<NbNotificationCategory>? NbNotificationCategories { get; set; }
        public List<NbNotificationBoard>? NbNotificationBoards { get; set; }
        public List<NbNotificationClass>? NbNotificationClasses { get; set; }
        public List<NbNotificationCourse>? NbNotificationCourses { get; set; }
        public List<NbNotificationExamType>? NbNotificationExamTypes { get; set; }
        public List<NotificationLinkMaster>? NotificationLinkMasters { get; set; }
        public List<NotificationDetail>? NotificationDetails { get; set; }
    }
    public class NbNotificationCategory
    {
        public int NbNotificationCategoryId { get; set; }
        public int APID { get; set; }
        public int NBNotificationID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class NbNotificationBoard
    {
        public int NbNotificationBoardId { get; set; }
        public int NBNotificationID { get; set; }
        public int BoardID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class NbNotificationClass
    {
        public int NbNotificationClassId { get; set; }
        public int NBNotificationID { get; set; }
        public int ClassID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class NbNotificationCourse
    {
        public int NbNotificationCourseId { get; set; }
        public int NBNotificationID { get; set; }
        public int CourseID { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    public class NbNotificationExamType
    {
        public int NbNotificationExamTypeId { get; set; }
        public int NBNotificationID { get; set; }
        public int ExamTypeID { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class NotificationsListDTO
    {
        public int APId { get; set; }
        public int BoardID { get; set; }
        public int ClassID { get; set; }
        public int CourseID { get; set; }
        public int SubjectID { get; set; }
        public int ExamTypeID { get; set; }
    }
}
