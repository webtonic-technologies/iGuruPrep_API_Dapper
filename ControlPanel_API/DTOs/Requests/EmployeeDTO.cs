﻿using System.ComponentModel.DataAnnotations;

namespace ControlPanel_API.DTOs.Requests
{
    public class EmployeeDTO
    {
        public int Employeeid { get; set; }
        public string Usercode { get; set; } = string.Empty;
        [Required(ErrorMessage = "Role name cannot be empty")]
        public int RoleID { get; set; }
        [Required(ErrorMessage = "Designation name cannot be empty")]
        public int DesignationID { get; set; }
        [Required(ErrorMessage = "First name cannot be empty")]
        public string EmpFirstName { get; set; } = string.Empty;
        [Required(ErrorMessage = "BoLastard name cannot be empty")]
        public string EmpLastName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Phone number cannot be empty")]
        public string EMPPhoneNumber { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email cannot be empty")]
        public string EMPEmail { get; set; } = string.Empty;
        public DateTime? EMPDOB { get; set; }
        [Required(ErrorMessage = "Zip code cannot be empty")]
        public string ZipCode { get; set; } = string.Empty;
        [Required(ErrorMessage = "District name cannot be empty")]
        public string DistrictName { get; set; } = string.Empty;
        public List<EmployeeSubject>? EmployeeSubjects { get; set; }
        [Required(ErrorMessage = "State name cannot be empty")]
        public string StateName { get; set; } = string.Empty;
        public string VcName { get; set; } = string.Empty;
        //public string Rolename { get; set; } = string.Empty;
        //public string Designationname { get; set; } = string.Empty;
        public DateTime? Modifiedon { get; set; }
        public string Modifiedby { get; set; } = string.Empty;
        public DateTime? Createdon { get; set; }
        public string Createdby { get; set; } = string.Empty;
        public bool? Status { get; set; }
        public string Password { get; set; } = string.Empty;
        public string EmpMiddleName { get; set; } = string.Empty;
        public bool IsSuperAdmin { get; set; }
    }
    public class EmployeeSubject
    {
        public int EmpSubId { get; set; }
        public int SubjectID { get; set; }
       // public string SubjectName { get; set; } = string.Empty;
        public int Employeeid { get; set; }
    }
    public class EmployeeLoginRequest
    {
        public string EmpEmailOrPhoneNumber { get; set; } = string.Empty;
        public string Password {  set; get; } = string.Empty;
    }
}
