using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlPanel_API.Models
{
    [Table("tblSOTD")]
    public class StoryOfTheDay
    {
        [Key]
        public int StoryId {  get; set; }
        //public int APID { get; set; }
        public int EventTypeID { get; set; }
        //public string BoardID { get; set; } = string.Empty;
        //public string ClassID { get; set; } = string.Empty;
        //public string BoardName { get; set; } = string.Empty;
        //public string ClassName { get; set; } = string.Empty;
        //public string EventName {  get; set; } = string.Empty;
        public string Event1Posttime { get; set; } = string.Empty;
        public DateTime? Event1PostDate { get; set; }
        public DateTime? Event2PostDate { get; set; }
        public string Event2Posttime { get; set; } = string.Empty;
        public string modifiedby { get; set; } = string.Empty;
        public string createdby { get; set; } = string.Empty;
        public string eventname { get; set; } = string.Empty;
        public DateTime? modifiedon {  get; set; }
        public DateTime? createdon {  get; set; }
        //public string APName {  get; set; } = string.Empty;
        public bool Status { get; set; }
        public int EmployeeID {  get; set; }
        public string Filename1 {  get; set; } = string.Empty;
        public string Filename2 { get; set; } = string.Empty;
        //public string EmpFirstName { get; set; } = string.Empty;
    }
}