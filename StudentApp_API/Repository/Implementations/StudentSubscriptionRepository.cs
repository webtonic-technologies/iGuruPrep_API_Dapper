using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using System.Data;
using System.Data.SqlClient;

namespace StudentApp_API.Repository.Implementations
{
    public class StudentSubscriptionRepository : IStudentSubscriptionRepository
    {
        private readonly IDbConnection _connection;

        public StudentSubscriptionRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<List<SubscriptionResponseDTO>>> GetSubscriptionPackages(SubscriptionRequestDTO request)
        {
            try
            {
                // Step 1: Fetch matching syllabus
                string syllabusQuery = request.CategoryID == 1
                    ? @"SELECT SyllabusId FROM tblSyllabus 
                WHERE BoardID = @BoardID AND ClassId = @ClassID AND CourseId = @CourseID AND Status = 1"
                    : @"SELECT SyllabusId FROM tblSyllabus 
                WHERE ExamTypeId = @ExamTypeID AND Status = 1";

                var syllabusId = await _connection.QueryFirstOrDefaultAsync<int?>(syllabusQuery, request);
                if (syllabusId == null)
                    return new ServiceResponse<List<SubscriptionResponseDTO>>(false, "No syllabus found", [], 404);

                // Step 2: Fetch mapped subjects
                string subjectQuery = @"SELECT SubjectID FROM tblSyllabusSubjects WHERE SyllabusID = @SyllabusID AND Status = 1";
                var subjectIds = (await _connection.QueryAsync<int>(subjectQuery, new { SyllabusID = syllabusId })).ToList();

                if (!subjectIds.Any())
                    return new ServiceResponse<List<SubscriptionResponseDTO>>(false, "No subjects mapped to syllabus", [], 404);

                // Step 3: Fetch subscriptions
                string subscriptionQuery = request.CategoryID == 1
                    ? @"SELECT * FROM tblSubscriptionPackage 
                WHERE CategoryID = @CategoryID AND BoardID = @BoardID AND ClassID = @ClassID AND CourseID = @CourseID AND IsActive = 1 AND IsDeleted = 0"
                    : @"SELECT * FROM tblSubscriptionPackage 
                WHERE CategoryID = @CategoryID AND ExamTypeID = @ExamTypeID AND IsActive = 1 AND IsDeleted = 0";

                var subscriptions = (await _connection.QueryAsync<SubscriptionResponseDTO>(subscriptionQuery, request)).ToList();

                if (!subscriptions.Any())
                    return new ServiceResponse<List<SubscriptionResponseDTO>>(false, "No subscriptions found", [], 404);

                // Step 4: Fetch chapter and topic counts only from syllabus-mapped content
                var subjectSummaries = new List<SubjectDetailsDTO>();

                foreach (var subjectId in subjectIds)
                {
                    // Get Chapter Codes mapped to syllabus
                    var chapterCodes = await _connection.QueryAsync<string>(
                        @"SELECT c.ChapterCode 
          FROM tblSyllabusDetails sd
          INNER JOIN tblContentIndexChapters c ON sd.ContentIndexId = c.ContentIndexId
          WHERE sd.SyllabusID = @SyllabusID 
            AND sd.SubjectId = @SubjectId 
            AND sd.Status = 1 
            AND sd.IndexTypeId = 1 
            AND c.IsActive = 1",
                        new { SyllabusID = syllabusId, SubjectId = subjectId });

                    var chapterCodeList = chapterCodes.ToList();

                    // Get topic count that are mapped to syllabus and belong to above chapters
                    var topicCount = await _connection.QuerySingleAsync<int>(
                        @"SELECT COUNT(*) 
          FROM tblSyllabusDetails sd
          INNER JOIN tblContentIndexTopics t ON sd.ContentIndexId = t.ContInIdTopic
          WHERE sd.SyllabusID = @SyllabusID 
            AND sd.SubjectId = @SubjectId 
            AND sd.Status = 1 
            AND sd.IndexTypeId = 2 
            AND t.IsActive = 1
            AND t.ChapterCode IN @ChapterCodes",
                        new { SyllabusID = syllabusId, SubjectId = subjectId, ChapterCodes = chapterCodeList });
                    var subject = await _connection.QueryFirstAsync<string>(@"select SubjectName from tblSubject where SubjectId = @SubjectId", new { SubjectId = subjectId });
                    subjectSummaries.Add(new SubjectDetailsDTO
                    {
                        SubjectId = subjectId,
                        SubjectName = subject,
                        ChapterCount = chapterCodeList.Count,
                        ConceptCount = topicCount
                    });
                }

                // Step 5: Attach subject summary to each subscription
                foreach (var subscription in subscriptions)
                {
                    subscription.Subjects = subjectSummaries;
                }

                // Step 6: Fetch modules
                string moduleQuery = @"SELECT * FROM tblModule WHERE Status = 1";
                var modules = (await _connection.QueryAsync<ModuleDTO>(moduleQuery)).ToList();

                // Step 7: Fetch module configurations
                string configQuery = @"SELECT * FROM tblModulewiseConfiguration";
                var configs = (await _connection.QueryAsync<tblModulewiseConfiguration>(configQuery)).ToList();


                foreach (var module in modules)
                {
                    var config = configs.FirstOrDefault(c => c.ModuleID == module.ModuleID);
                    if (config != null)
                    {
                        module.IsFree = config.IsFree;
                        module.IsSubscription = config.IsSubscription;
                        module.DiscountOnFinalPrice = config.DiscountOnFinalPrice;
                    }
                }

                // Step 8: Add modules to each subscription
                foreach (var subscription in subscriptions)
                {
                    subscription.Modules = modules;
                }

                return new ServiceResponse<List<SubscriptionResponseDTO>>(true, "Data fetched successfully", subscriptions, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<SubscriptionResponseDTO>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<string>> InsertStudentSubscriptionAsync(StudentSubscriptionInsertRequest request)
        {
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                _connection.Open();
            }

            using var transaction = _connection.BeginTransaction();
            try
            {
                // Step 1: Validate subject selection
                if (request.SubjectIds == null || !request.SubjectIds.Any(s => s != 0))
                    return new ServiceResponse<string>(false, "At least one subject must be selected.", null, 400);

                // Step 2: Get all module configurations
                string moduleQuery = @"
        SELECT m.ModuleID, mwc.IsFree
        FROM tblModule m
        INNER JOIN tblModulewiseConfiguration mwc ON m.ModuleID = mwc.ModuleID";

                var allModules = (await _connection.QueryAsync<(int ModuleID, bool IsFree)>(moduleQuery, transaction: transaction)).ToList();

                // Step 3: Check if selected paid module IDs contain at least one valid non-free module
                var paidModuleIdsFromRequest = request.SelectedPaidModuleIds?.Where(id => id != 0).Distinct().ToList() ?? new List<int>();

                var actualPaidModules = allModules
                    .Where(m => paidModuleIdsFromRequest.Contains(m.ModuleID) && !m.IsFree)
                    .Select(m => m.ModuleID)
                    .ToList();

                if (!actualPaidModules.Any())
                    return new ServiceResponse<string>(false, "At least one valid paid module must be selected.", null, 400);

                // Step 4: Get all free module IDs
                var freeModuleIds = allModules
                    .Where(m => m.IsFree)
                    .Select(m => m.ModuleID)
                    .Distinct()
                    .ToList();

                // Step 5: Final module ID list (paid + free)
                var finalModuleIds = actualPaidModules.Concat(freeModuleIds).Distinct().ToList();

                // Step 6: Prepare CSV strings
                string subjectCsv = string.Join(",", request.SubjectIds.Distinct());
                string moduleCsv = string.Join(",", finalModuleIds);

                // Step 7: Insert into tblStudentSubscriptionMapping
                string insertQuery = @"
        INSERT INTO tblStudentSubscriptionMapping (StudentID, SubscriptionID, ModuleID, SubjectID)
        VALUES (@StudentID, @SubscriptionID, @ModuleID, @SubjectID);";

                await _connection.ExecuteAsync(insertQuery, new
                {
                    StudentID = request.StudentId,
                    SubscriptionID = request.SubscriptionId,
                    ModuleID = moduleCsv,
                    SubjectID = subjectCsv
                }, transaction);

                transaction.Commit();
                return new ServiceResponse<string>(true, "Subscription saved successfully with selected modules and subjects.", null, 200);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return new ServiceResponse<string>(false, $"Error: {ex.Message}", null, 500);
            }
        }
        public async Task<ServiceResponse<StudentCoinTransactionResult>> GetStudentCoinTransactionsAsync(int studentId)
        {
            try
            {
                string transactionQuery = @" SELECT 
     CT.CTID, CT.StudentID, CT.CoinCategoryTypeID, CT.Coins, CT.JournalTypeID,
     CT.ReferenceID, CT.TransactionTime, CT.IndexTypeId, CT.ContentId,
     CASE 
         WHEN CT.CoinCategoryTypeID = 10 THEN C.ChallengeName
         WHEN CT.CoinCategoryTypeID = 8 AND CT.IndexTypeId = 1 THEN CH.ContentName_Chapter
         WHEN CT.CoinCategoryTypeID = 8 AND CT.IndexTypeId = 2 THEN T.ContentName_Topic
         WHEN CT.CoinCategoryTypeID = 8 AND CT.IndexTypeId = 3 THEN S.ContentName_SubTopic
         ELSE NULL
     END AS ReferenceName
 FROM tblCoinTransaction CT
 LEFT JOIN tblCYOT C ON CT.CoinCategoryTypeID = 10 AND CT.ReferenceID = C.CYOTID
 LEFT JOIN tblContentIndexChapters CH ON CT.CoinCategoryTypeID = 8 AND CT.IndexTypeId = 1 AND CT.ContentId = CH.ContentIndexId
 LEFT JOIN tblContentIndexTopics T ON CT.CoinCategoryTypeID = 8 AND CT.IndexTypeId = 2 AND CT.ContentId = T.ContInIdTopic
 LEFT JOIN tblContentIndexSubTopics S ON CT.CoinCategoryTypeID = 8 AND CT.IndexTypeId = 3 AND CT.ContentId = S.ContInIdSubTopic
 WHERE CT.StudentID = @StudentID
 ORDER BY CT.TransactionTime DESC";

                var transactions = (await _connection.QueryAsync<CoinTransactionResponse>(transactionQuery, new { StudentID = studentId })).ToList();

                string balanceQuery = @"
        SELECT ISNULL(SUM(CASE WHEN JournalTypeID = 1 THEN Coins ELSE -Coins END), 0)
        FROM tblCoinTransaction
        WHERE StudentID = @StudentID";

                int balance = await _connection.ExecuteScalarAsync<int>(balanceQuery, new { StudentID = studentId });

                var result = new StudentCoinTransactionResult
                {
                    TotalRemainingCoins = balance,
                    Transactions = transactions
                };

                return new ServiceResponse<StudentCoinTransactionResult>(true, "Coin transactions fetched", result, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<StudentCoinTransactionResult>(false, $"Error: {ex.Message}", null, 500);
            }
        }
        public async Task<ServiceResponse<string>> CoinTransactionAsync(CoinTransactionRequestDTO request)
        {
            var result = await HandleCoinTransactionAsync(
                request.StudentID,
                request.CoinCategoryTypeID,
                request.Coins,
                request.JournalTypeID,
                request.ReferenceID, request.ItemId, request.IndexTypeId, request.ContentId);

            return result.Success
                ? new ServiceResponse<string>(true, $"Coins successfully {(request.JournalTypeID == 2 ? "debited" : "credited")}.", null, 200)
                : new ServiceResponse<string>(false, result.Message, null, result.StatusCode);
        }
        private async Task<ServiceResponse<bool>> HandleCoinTransactionAsync(int studentId, int coinCategoryTypeId, int coins, int journalTypeId, int referenceId, int ItemId, int? IndexTypeId, int? ContentId)
        {
            if (studentId <= 0 || coins <= 0)
                return new ServiceResponse<bool>(false, "Invalid student ID or coin amount", false, 400);

            string validateCategoryQuery = @"SELECT COUNT(1) FROM tblCoinModules WHERE CoinCategoryTypeID = @CoinCategoryTypeID";
            var categoryExists = await _connection.ExecuteScalarAsync<bool>(validateCategoryQuery, new { CoinCategoryTypeID = coinCategoryTypeId });

            if (!categoryExists)
                return new ServiceResponse<bool>(false, "Invalid coin category type", false, 400);

           
                string subQuery = @"SELECT FinalPrice FROM tblSubscriptionPackage 
                                WHERE SubscriptionID = @ItemId AND IsActive = 1 AND IsDeleted = 0";

               var amount = await _connection.ExecuteScalarAsync<decimal>(subQuery, new { ItemId = ItemId });

                if (amount <= 0)
                {
                    return new ServiceResponse<bool>(
                        false, "Invalid subscription or price not found", false, 400);
                }
            string mockOrderId = (Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")).Substring(0, 40);
            if (coins > 0)
                {
                    string coinBalanceQuery = @"SELECT ISNULL(SUM(CASE WHEN JournalTypeID = 1 THEN Coins ELSE -Coins END), 0)
                                            FROM tblCoinTransaction
                                            WHERE StudentID = @StudentID";

                    int coinsAvailable = await _connection.ExecuteScalarAsync<int>(coinBalanceQuery, new { StudentID = studentId });

                    if (coins > coinsAvailable)
                    {
                        return new ServiceResponse<bool>(
                            false, "You don't have enough coins", false, 400);
                    }

                    amount -= coins;
                    if (amount < 0) amount = 0;

                    string logCoinTxn = @"INSERT INTO tblCoinTransaction 
                                      (StudentID, CoinCategoryTypeID, Coins, JournalTypeID, ReferenceID, TransactionTime, IndexTypeId, ContentId)
                                      VALUES 
                                      (@StudentID, @CoinCategoryTypeID, @Coins, 2, @ReferenceID, GETDATE(), @IndexTypeId, @ContentId)"; // 2 = Debit

                    await _connection.ExecuteAsync(logCoinTxn, new
                    {
                        StudentID = studentId,
                        CoinCategoryTypeID = coinCategoryTypeId, // 1 = Subscription
                        Coins = coins,
                        ReferenceID = referenceId,
                        IndexTypeId = IndexTypeId,
                        ContentId = ContentId
                    });
                }

                string insertQuery = @"INSERT INTO tblPaymentTransaction 
                                   (OrderID, SubscriptionID, StudentID, OrderDate, OrderAmount, Status, PurchaseType, PaymentUrl)
                                   VALUES 
                                   (@OrderID, @SubscriptionID, @StudentID, GETDATE(), @OrderAmount, 'Initiated', 'Subscription', @PaymentUrl)";

                string paymentLink = $"https://pay.razorpay.in/pay/{mockOrderId}";

                await _connection.ExecuteAsync(insertQuery, new
                {
                    OrderID = mockOrderId,
                    SubscriptionID = ItemId,
                    StudentID = studentId,
                    OrderAmount = amount,
                    PaymentUrl = paymentLink
                });
            return new ServiceResponse<bool>(true, "Coin transaction recorded", true, 200);
        }
        public async Task<ServiceResponse<InitiateRazorpayResponse>> InitiateTransactionAsync(InitiateTransactionRequest request)
        {
            try
            {
                decimal amount = 0;
                string mockOrderId = (Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N")).Substring(0, 40);

                if (request.PurchaseType.ToLower() == "subscription")
                {
                    string subQuery = @"SELECT FinalPrice FROM tblSubscriptionPackage 
                                WHERE SubscriptionID = @ItemId AND IsActive = 1 AND IsDeleted = 0";

                    amount = await _connection.ExecuteScalarAsync<decimal>(subQuery, new { ItemId = request.ItemId });

                    if (amount <= 0)
                    {
                        return new ServiceResponse<InitiateRazorpayResponse>(
                            false, "Invalid subscription or price not found", null, 400);
                    }

                    if (request.UseCoins && request.CoinsToUse > 0)
                    {
                        string coinBalanceQuery = @"SELECT ISNULL(SUM(CASE WHEN JournalTypeID = 1 THEN Coins ELSE -Coins END), 0)
                                            FROM tblCoinTransaction
                                            WHERE StudentID = @StudentID";

                        int coinsAvailable = await _connection.ExecuteScalarAsync<int>(coinBalanceQuery, new { StudentID = request.StudentId });

                        if (request.CoinsToUse > coinsAvailable)
                        {
                            return new ServiceResponse<InitiateRazorpayResponse>(
                                false, "You don't have enough coins", null, 400);
                        }

                        amount -= request.CoinsToUse;
                        if (amount < 0) amount = 0;

                        string logCoinTxn = @"INSERT INTO tblCoinTransaction 
                                      (StudentID, CoinCategoryTypeID, Coins, JournalTypeID, ReferenceID, TransactionTime)
                                      VALUES 
                                      (@StudentID, @CoinCategoryTypeID, @Coins, 2, @ReferenceID, GETDATE())"; // 2 = Debit

                        await _connection.ExecuteAsync(logCoinTxn, new
                        {
                            StudentID = request.StudentId,
                            CoinCategoryTypeID = 11, // 1 = Subscription
                            Coins = request.CoinsToUse,
                            ReferenceID = mockOrderId
                        });
                    }

                    string insertQuery = @"INSERT INTO tblPaymentTransaction 
                                   (OrderID, SubscriptionID, StudentID, OrderDate, OrderAmount, Status, PurchaseType, PaymentUrl)
                                   VALUES 
                                   (@OrderID, @SubscriptionID, @StudentID, GETDATE(), @OrderAmount, 'Initiated', 'Subscription', @PaymentUrl)";

                    string paymentLink = $"https://pay.razorpay.in/pay/{mockOrderId}";

                    await _connection.ExecuteAsync(insertQuery, new
                    {
                        OrderID = mockOrderId,
                        SubscriptionID = request.ItemId,
                        StudentID = request.StudentId,
                        OrderAmount = amount,
                        PaymentUrl = paymentLink
                    });

                    return new ServiceResponse<InitiateRazorpayResponse>(
                        true, "Transaction initialized successfully",
                        new InitiateRazorpayResponse
                        {
                            RazorpayOrderId = mockOrderId,
                            PaymentLink = paymentLink,
                            Amount = amount
                        },
                        200
                    );
                }
                else if (request.PurchaseType.ToLower() == "coins")
                {
                    string coinQuery = @"SELECT Price, Coins FROM tblCoins 
                                 WHERE CoinID = @ItemId AND IsActive = 1 AND IsDeleted = 0";

                    var coinData = await _connection.QueryFirstOrDefaultAsync<(decimal Price, int Coins)>(coinQuery, new { ItemId = request.ItemId });

                    if (coinData.Price <= 0 || coinData.Coins <= 0)
                    {
                        return new ServiceResponse<InitiateRazorpayResponse>(
                            false, "Invalid coin package or price not found", null, 400);
                    }

                    string paymentLink = $"https://pay.razorpay.in/pay/{mockOrderId}";

                    string insertTxnQuery = @"INSERT INTO tblPaymentTransaction 
                                      (OrderID, CoinID, StudentID, OrderDate, OrderAmount, Status, PurchaseType, PaymentUrl)
                                      VALUES 
                                      (@OrderID, @CoinID, @StudentID, GETDATE(), @OrderAmount, 'Initiated', 'Coins', @PaymentUrl)";

                    await _connection.ExecuteAsync(insertTxnQuery, new
                    {
                        OrderID = mockOrderId,
                        CoinID = request.ItemId,
                        StudentID = request.StudentId,
                        OrderAmount = coinData.Price,
                        PaymentUrl = paymentLink
                    });

                    string insertCoinTxn = @"INSERT INTO tblCoinTransaction 
                                     (StudentID, CoinCategoryTypeID, Coins, JournalTypeID, ReferenceID, TransactionTime)
                                     VALUES 
                                     (@StudentID, @CoinCategoryTypeID, @Coins, 1, @ReferenceID, GETDATE())"; // 1 = Credit

                    await _connection.ExecuteAsync(insertCoinTxn, new
                    {
                        StudentID = request.StudentId,
                        CoinCategoryTypeID = 28, // 2 = Coin Purchase
                        Coins = coinData.Coins,
                        ReferenceID = mockOrderId
                    });

                    return new ServiceResponse<InitiateRazorpayResponse>(
                        true, "Coin purchase transaction initialized",
                        new InitiateRazorpayResponse
                        {
                            RazorpayOrderId = mockOrderId,
                            PaymentLink = paymentLink,
                            Amount = coinData.Price
                        },
                        200
                    );
                }
                else
                {
                    return new ServiceResponse<InitiateRazorpayResponse>(
                        false, "Invalid Purchase Type", null, 400);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<InitiateRazorpayResponse>(
                    false, $"Error initiating transaction: {ex.Message}", null, 500);
            }
        }
        public async Task<ServiceResponse<TransactionDetailsResponse>> GetTransactionDetailsByOrderIdAsync(string orderId)
        {
            try
            {
                string query = @"SELECT 
                            OrderID,
                            TransactionID,
                            OrderAmount AS Amount,
                            StudentID,
                            PurchaseType,
                            ISNULL(SubscriptionID, CoinID) AS ItemID,
                            PaymentUrl,
                            Status
                        FROM tblPaymentTransaction
                        WHERE OrderID = @OrderID";

                var result = await _connection.QueryFirstOrDefaultAsync<TransactionDetailsResponse>(query, new { OrderID = orderId });

                if (result == null)
                {
                    return new ServiceResponse<TransactionDetailsResponse>(
                        false, "Transaction not found", null, 404);
                }

                // If needed, you can also add RazorpayKey or Currency (e.g., hardcoded "INR")
                result.Currency = "INR";
                result.RazorpayKey = ""; // Not used in mock

                return new ServiceResponse<TransactionDetailsResponse>(
                    true, "Transaction retrieved successfully", result, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<TransactionDetailsResponse>(
                    false, $"Error retrieving transaction: {ex.Message}", null, 500);
            }
        }
        public class InitiateRazorpayResponse
        {
            public string RazorpayOrderId { get; set; }
            public string PaymentLink { get; set; }
            public decimal Amount { get; set; }
        }
        public class TransactionDetailsResponse
        {
            public string OrderID { get; set; }
            public string TransactionID { get; set; }
            public decimal Amount { get; set; }
            public string Currency { get; set; }
            public int StudentID { get; set; }
            public string PurchaseType { get; set; }
            public int ItemID { get; set; }
            public string PaymentUrl { get; set; }
            public string RazorpayKey { get; set; }
            public string Status { get; set; }
        }
    }
}