using Packages_API.DTOs.Requests;

namespace Packages_API.DTOs.Response
{
    public class SubscriptionDTO
    {
        public int SubscriptionID { get; set; }
        public int CountryID { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }  // Added
        public int BoardID { get; set; }
        public string BoardName { get; set; }  // Added
        public int ClassID { get; set; }
        public string ClassName { get; set; }  // Added
        public int CourseID { get; set; }
        public string CourseName { get; set; }  // Added
        public string PackageName { get; set; }
        public int ValidityDays { get; set; }
        public decimal MRP { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalPrice { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public List<SubjectWiseDiscountDTO> SubjectWiseDiscounts { get; set; }
    }

    public class SubjectWiseDiscountDTO
    {
        public int SWDID { get; set; }
        public int SubscriptionID { get; set; }
        public int NoOfSubject { get; set; }
        public decimal Discount { get; set; }
    }

}
