using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using static StudentApp_API.Repository.Implementations.StudentSubscriptionRepository;
namespace StudentApp_API.Repository.Interfaces
{
    public interface IStudentSubscriptionRepository
    {
        Task<ServiceResponse<List<SubscriptionResponseDTO>>> GetSubscriptionPackages(SubscriptionRequestDTO request);
        Task<ServiceResponse<string>> InsertStudentSubscriptionAsync(StudentSubscriptionInsertRequest request);
        Task<ServiceResponse<InitiateRazorpayResponse>> InitiateTransactionAsync(InitiateTransactionRequest request);
        Task<ServiceResponse<TransactionDetailsResponse>> GetTransactionDetailsByOrderIdAsync(string orderId);
        Task<ServiceResponse<StudentCoinTransactionResult>> GetStudentCoinTransactionsAsync(int studentId);
        Task<ServiceResponse<string>> CoinTransactionAsync(CoinTransactionRequestDTO request);
    }
}
