using System.ComponentModel.DataAnnotations.Schema;

namespace ControlPanel_API.Models
{
    [Table("tblEmployee")]
    public class Employee
    {
        public string EmployeeCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int PinCode { get; set; }
        public string State { get; set; }
        public string District { get; set; }
        public string City { get; set; }
        public string Role { get; set; }
        public string Designation { get; set; }
        public string Subjects { get; set; }
    }
}
