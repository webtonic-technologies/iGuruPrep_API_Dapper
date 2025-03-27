namespace Packages_API.DTOs.Requests
{
    public class AddUpdateSubscriptionRequest
    {
        public int SubscriptionID { get; set; }
        public int CountryID { get; set; }
        public int CategoryID { get; set; }
        public int BoardID { get; set; }
        public int ClassID { get; set; }
        public int CourseID { get; set; }
        public string PackageName { get; set; }
        public int ValidityDays { get; set; }
        public decimal MRP { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalPrice { get; set; }
        public bool IsActive { get; set; }

        // List of subject-wise discounts
        public List<SubjectWiseDiscountDTO> SubjectWiseDiscounts { get; set; }
    }
    public class SubjectWiseDiscountDTO
    {
        public int SWDID { get; set; }
        public int SubscriptionID { get; set; }
        public int NoOfSubject { get; set; }
        public decimal Discount { get; set; }
    }
    public class SubjectRequestDTO
    {
        public int BoardID { get; set; }
        public int ClassID { get; set; }
        public int CourseID { get; set; }
    }

}
