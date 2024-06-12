using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlPanel_API.Models
{
    [Table("tblMagazine")]
    public class Magazine
    {
        public int MagazineId { get; set; }
        public DateTime? Date { get; set; }
        public string Time { get; set; } = string.Empty;
        public string PathURL { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string MagazineTitle { get; set; } = string.Empty;
        //public int CourseId {  get; set; }
        //public int ClassId {  get; set; }
        //public int Boardid {  get; set; }
        //public int APID {  get; set; }
        //public int ExamTypeId {  get; set; }
        //public string classname { get; set; } = string.Empty;
        //public string coursename { get; set; } = string.Empty;
        public bool? Status {  get; set; }
        //public string boardname { get; set; } = string.Empty;
        //public string examtypename { get; set; } = string.Empty;
        //public string APname {  get; set; } = string.Empty;
        public DateTime? modifiedon {  get; set; }
        public string modifiedby {  get; set; } = string.Empty;
        public DateTime? createdon { get; set; }
        public string createdby { get; set; } = string.Empty;
        public int EmployeeID {  get; set; }
        //public string EmpFirstName { get; set; } = string.Empty;
    }
}