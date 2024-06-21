using System.ComponentModel.DataAnnotations.Schema;

namespace ControlPanel_API.Models
{
    [Table("tblEmployee")]
    public class Employee
    {
        public int Employeeid { get; set; }
        public string Usercode { get; set; } = string.Empty;
        public int RoleID { get; set; }
        public int DesignationID { get; set; }
        public string EmpFirstName { get; set; } = string.Empty;
        public string EmpLastName { get; set; } = string.Empty;
        public string EMPPhoneNumber { get; set; } = string.Empty;
        public string EMPEmail { get; set; } = string.Empty;
        public DateTime? EMPDOB { get; set; }
        public string ZipCode { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public int SubjectID { get; set; }
        public string StateName { get; set; } = string.Empty;
        public string VcName { get; set; } = string.Empty;
        //public string Rolename { get; set; } = string.Empty;
        //public string Designationname { get; set; } = string.Empty;
        public DateTime? Modifiedon { get; set; }
        public string Modifiedby { get; set; } = string.Empty;
        public DateTime? Createdon { get; set; }
        public string Createdby { get; set; } = string.Empty;
        public bool? Status { get; set; }
    }
}
