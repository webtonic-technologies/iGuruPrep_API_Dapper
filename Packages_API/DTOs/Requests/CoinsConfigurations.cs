namespace Packages_API.DTOs.Requests
{
    public class CoinsConfigurations
    {
    }
    public class CoinConfigurationDTO
    {
        public int CCID { get; set; }
        public int CoinCategoryID { get; set; }
        public string Name { get; set; }
        public decimal? Rupees { get; set; }  // Only for Basic
        public int? NoOfCoins { get; set; }   // For Basic & LeaderBoard
        public int? StartRange { get; set; }  // Only for Badges
        public int? EndRange { get; set; }    // Only for Badges
        public bool IsActive { get; set; }
    }

    public class AddUpdateCoinConfigurationRequest
    {
        public int? CCID { get; set; } // Null for add, value for update
        public int CoinCategoryID { get; set; }
        public string Name { get; set; }
        public decimal? Rupees { get; set; }  // Only for Basic
        public int? NoOfCoins { get; set; }   // For Basic & LeaderBoard
        public int? StartRange { get; set; }  // Only for Badges
        public int? EndRange { get; set; }    // Only for Badges
    }

}
