using System.Data;
using Dapper;
using Packages_API.DTOs.Requests;
using Packages_API.DTOs.Response;
using Packages_API.DTOs.ServiceResponse;
using Packages_API.Models;
using Packages_API.Repository.Interfaces;

namespace Packages_API.Repository.Implementations
{
    public class SubscriptionPackageRepository: ISubscriptionPackageRepository
    {
        private readonly IDbConnection _connection;

        public SubscriptionPackageRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<bool>> AddUpdateSubscription(AddUpdateSubscriptionRequest subscription)
        {
            try
            {
                if (_connection.State == ConnectionState.Closed)
                    _connection.Open();

                using (var transaction = _connection.BeginTransaction())
                {
                    try
                    {
                        string query;
                        if (subscription.SubscriptionID == 0)
                        {
                            // Insert new subscription
                            query = @"INSERT INTO tblSubscriptionPackage (CountryID, CategoryID, BoardID, ClassID, CourseID, PackageName, ValidityDays, 
                              MRP, Discount, FinalPrice, IsActive, IsDeleted)
                              VALUES (@CountryID, @CategoryID, @BoardID, @ClassID, @CourseID, @PackageName, @ValidityDays, 
                              @MRP, @Discount, @FinalPrice, @IsActive, 0);
                              SELECT CAST(SCOPE_IDENTITY() as int);";

                            subscription.SubscriptionID = await _connection.ExecuteScalarAsync<int>(query, subscription, transaction);
                        }
                        else
                        {
                            // Update existing subscription
                            query = @"UPDATE tblSubscriptionPackage 
                              SET CountryID = @CountryID, CategoryID = @CategoryID, BoardID = @BoardID, ClassID = @ClassID, 
                              CourseID = @CourseID, PackageName = @PackageName, ValidityDays = @ValidityDays, 
                              MRP = @MRP, Discount = @Discount, FinalPrice = @FinalPrice, IsActive = @IsActive
                              WHERE SubscriptionID = @SubscriptionID AND IsDeleted = 0";

                            await _connection.ExecuteAsync(query, subscription, transaction);

                            // Delete existing subject-wise discounts
                            string deleteQuery = "DELETE FROM tblSubjectWiseDiscount WHERE SubscriptionID = @SubscriptionID";
                            await _connection.ExecuteAsync(deleteQuery, new { subscription.SubscriptionID }, transaction);
                        }

                        // Insert new subject-wise discounts
                        if (subscription.SubjectWiseDiscounts != null && subscription.SubjectWiseDiscounts.Any())
                        {
                            string insertSWDQuery = @"INSERT INTO tblSubjectWiseDiscount (SubscriptionID, NoOfSubject, Discount) 
                                              VALUES (@SubscriptionID, @NoOfSubject, @Discount)";

                            foreach (var discount in subscription.SubjectWiseDiscounts)
                            {
                                discount.SubscriptionID = subscription.SubscriptionID;
                                await _connection.ExecuteAsync(insertSWDQuery, discount, transaction);
                            }
                        }

                        transaction.Commit();
                        return new ServiceResponse<bool>(true, "Subscription package saved successfully.", true, 200);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return new ServiceResponse<bool>(false, ex.Message, false, 500);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }
        public async Task<ServiceResponse<List<SubscriptionDTO>>> GetAllSubscriptions()
        {
            try
            {
                string query = @"
            SELECT sp.*, 
                   b.BoardName, 
                   c.ClassName, 
                   cr.CourseName, 
                   cat.APName AS CategoryName
            FROM tblSubscriptionPackage sp
            LEFT JOIN tblBoard b ON sp.BoardID = b.BoardId
            LEFT JOIN tblClass c ON sp.ClassID = c.ClassId
            LEFT JOIN tblCourse cr ON sp.CourseID = cr.CourseId
            LEFT JOIN tblCategory cat ON sp.CategoryID = cat.APId
            WHERE sp.IsDeleted = 0";

                var subscriptions = (await _connection.QueryAsync<SubscriptionDTO>(query)).ToList();

                return new ServiceResponse<List<SubscriptionDTO>>(true, "Subscriptions fetched successfully.", subscriptions, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<SubscriptionDTO>>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<List<CountryDTO>>> GetAllCountry()
        {
            try
            {
                string query = "SELECT CountryID, CountryName FROM tblCountries";
                var countries = (await _connection.QueryAsync<CountryDTO>(query)).ToList();

                if (!countries.Any())
                    return new ServiceResponse<List<CountryDTO>>(false, "No countries found.", null, 404);

                return new ServiceResponse<List<CountryDTO>>(true, "Countries retrieved successfully.", countries, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<CountryDTO>>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<List<SubjectDTO>>> GetSubjectsByBoardClassCourse(SubjectRequestDTO request)
        {
            try
            {
                // Step 1: Get Syllabus mapped to Board, Class, and Course
                string syllabusQuery = @"SELECT SyllabusId FROM tblSyllabus 
                                 WHERE BoardID = @BoardID AND CourseID = @CourseID AND ClassID = @ClassID";

                var syllabusIds = (await _connection.QueryAsync<int>(syllabusQuery,
                                    new { BoardID = request.BoardID, CourseID = request.CourseID, ClassID = request.ClassID }))
                                    .ToList();

                if (!syllabusIds.Any())
                {
                    return new ServiceResponse<List<SubjectDTO>>(false, "No syllabus found for the given Board, Class, and Course.", [], 404);
                }

                // Step 2: Get Subject IDs mapped to these syllabi
                string subjectMappingQuery = @"SELECT DISTINCT SubjectID FROM tblSyllabusSubjects 
                                       WHERE SyllabusID IN @SyllabusIDs AND Status = 1";

                var subjectIds = (await _connection.QueryAsync<int>(subjectMappingQuery, new { SyllabusIDs = syllabusIds })).ToList();

                if (!subjectIds.Any())
                {
                    return new ServiceResponse<List<SubjectDTO>>(false, "No subjects found for the mapped syllabi.", [], 404);
                }

                // Step 3: Get Subject Names
                string subjectQuery = @"SELECT SubjectId, SubjectName FROM tblSubject WHERE SubjectId IN @SubjectIDs";

                var subjects = (await _connection.QueryAsync<SubjectDTO>(subjectQuery, new { SubjectIDs = subjectIds })).ToList();

                return new ServiceResponse<List<SubjectDTO>>(true, "Subjects fetched successfully.", subjects, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<SubjectDTO>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<SubscriptionDTO>> GetSubscriptionByID(int subscriptionID)
        {
            try
            {
                string query = @"
            SELECT sp.*, 
                   b.BoardName, 
                   c.ClassName, 
                   cr.CourseName, 
                   cat.APName AS CategoryName
            FROM tblSubscriptionPackage sp
            LEFT JOIN tblBoard b ON sp.BoardID = b.BoardId
            LEFT JOIN tblClass c ON sp.ClassID = c.ClassId
            LEFT JOIN tblCourse cr ON sp.CourseID = cr.CourseId
            LEFT JOIN tblCategory cat ON sp.CategoryID = cat.APId
            WHERE sp.SubscriptionID = @SubscriptionID AND sp.IsDeleted = 0";

                var subscription = await _connection.QueryFirstOrDefaultAsync<SubscriptionDTO>(query, new { SubscriptionID = subscriptionID });

                if (subscription == null)
                    return new ServiceResponse<SubscriptionDTO>(false, "Subscription not found.", null, 404);

                // Fetch subject-wise discounts
                string discountQuery = "SELECT * FROM tblSubjectWiseDiscount WHERE SubscriptionID = @SubscriptionID";
                subscription.SubjectWiseDiscounts = (await _connection.QueryAsync<DTOs.Response.SubjectWiseDiscountDTO>(discountQuery, new { SubscriptionID = subscriptionID })).ToList();

                return new ServiceResponse<SubscriptionDTO>(true, "Subscription details fetched successfully.", subscription, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<SubscriptionDTO>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<bool>> DeleteSubscription(int subscriptionID)
        {
            try
            {
                string query = "UPDATE tblSubscriptionPackage SET IsDeleted = 1 WHERE SubscriptionID = @SubsciptionID";
                int rowsAffected = await _connection.ExecuteAsync(query, new { SubsciptionID = subscriptionID });

                if (rowsAffected == 0)
                    return new ServiceResponse<bool>(false, "Subscription not found.", false, 404);

                return new ServiceResponse<bool>(true, "Subscription deleted successfully.", true, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }
        public async Task<ServiceResponse<bool>> SubscriptionStatus(int subscriptionID)
        {
            try
            {
                string query = "SELECT IsActive FROM tblSubscriptionPackage WHERE SubscriptionID = @SubsciptionID AND IsDeleted = 0";
                bool? isActive = await _connection.QueryFirstOrDefaultAsync<bool?>(query, new { SubsciptionID = subscriptionID });

                if (isActive == null)
                    return new ServiceResponse<bool>(false, "Subscription not found.", false, 404);

                bool newStatus = !isActive.Value;
                string updateQuery = "UPDATE tblSubscriptionPackage SET IsActive = @IsActive WHERE SubscriptionID = @SubsciptionID";
                await _connection.ExecuteAsync(updateQuery, new { SubsciptionID = subscriptionID, IsActive = newStatus });

                return new ServiceResponse<bool>(true, "Subscription status updated successfully.", true, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }
    }
}
