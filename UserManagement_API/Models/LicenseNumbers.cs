namespace UserManagement_API.Models
{
    public class LicenseNumbers
    {
        public int LicenseNumbersId { get; set; }
        public int LicenseDetailID { get; set; }
        public string LicenseNo { get; set; } = string.Empty;
        public string LicensePassword { get; set; } = string.Empty;
    }
}
