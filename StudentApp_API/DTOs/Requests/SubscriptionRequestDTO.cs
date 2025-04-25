namespace StudentApp_API.DTOs.Requests
{
    public class SubscriptionRequestDTO
    {
        public int CategoryID { get; set; } // 1=Academic, 2=Professional
        public int BoardID { get; set; }
        public int ClassID { get; set; }
        public int CourseID { get; set; }
        public int ExamTypeID { get; set; }
    }
    public class StudentSubscriptionInsertRequest
    {
        public int StudentId { get; set; }
        public int SubscriptionId { get; set; }
        public List<int> SubjectIds { get; set; }
        public List<int> SelectedPaidModuleIds { get; set; }
    }
    public class CoinTransactionRequestDTO
    {
        public int StudentID { get; set; }
        public int CoinCategoryTypeID { get; set; }  // e.g., 1 = Test Series, 2 = Rank, 3 = Subscription
        public int Coins { get; set; }               // Coins to credit or debit
        public int JournalTypeID { get; set; }       // 1 = Debit, 2 = Credit
        public int ReferenceID { get; set; }
        public int ItemId { get; set; }// Optional reference (e.g., Test ID)
        public int? IndexTypeId {  get; set; }
        public int? ContentId {  get; set; }
    }
    public class InitiateTransactionRequest
    {
        public int StudentId { get; set; }
        public int ItemId { get; set; }
        public string PurchaseType { get; set; } // "subscription" or "coins"
        public bool UseCoins { get; set; } = false;
        public int CoinsToUse { get; set; } = 0; // Optional; applies only for subscription
    }

}
