namespace Packages_API.Models
{
    public class Coins
    {
        public int CoinID { get; set; }
        public int NoOfCoins { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}