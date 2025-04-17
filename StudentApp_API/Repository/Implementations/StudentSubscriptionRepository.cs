using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.DTOs.Requests;
using System.Data;
using Dapper;
using StudentApp_API.Repository.Interfaces;

namespace StudentApp_API.Repository.Implementations
{
    public class StudentSubscriptionRepository: IStudentSubscriptionRepository
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
        public async Task<ServiceResponse<string>> AddCoinTransactionAsync(CoinTransactionRequestDTO request)
        {
            try
            {
                if (request.StudentID <= 0 || request.Coins <= 0)
                    return new ServiceResponse<string>(false, "Invalid student ID or coin amount", null, 400);

                // Optional: Check if CoinCategoryTypeID exists in tblCoinModules
                string validateCategoryQuery = @"SELECT COUNT(1) FROM tblCoinModules WHERE CoinCategoryTypeID = @CoinCategoryTypeID";
                var categoryExists = await _connection.ExecuteScalarAsync<bool>(validateCategoryQuery, new { request.CoinCategoryTypeID });

                if (!categoryExists)
                    return new ServiceResponse<string>(false, "Invalid coin category type", null, 400);

                // Insert into tblCoinTransaction
                string insertQuery = @"
            INSERT INTO tblCoinTransaction 
            (StudentID, CoinCategoryTypeID, Coins, JournalTypeID, ReferenceID, TransactionTime)
            VALUES 
            (@StudentID, @CoinCategoryTypeID, @Coins, @JournalTypeID, @ReferenceID, GETDATE())";

                await _connection.ExecuteAsync(insertQuery, request);

                string action = request.JournalTypeID == 2 ? "credited" : "debited";
                return new ServiceResponse<string>(true, $"Coins successfully {action}.", null, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, $"Error: {ex.Message}", null, 500);
            }
        }
    }
}
