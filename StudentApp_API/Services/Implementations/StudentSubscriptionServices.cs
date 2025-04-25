using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Response;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Implementations;
using StudentApp_API.Repository.Interfaces;
using StudentApp_API.Services.Interfaces;

namespace StudentApp_API.Services.Implementations
{
    public class StudentSubscriptionServices : IStudentSubscriptionServices
    {
        private readonly IStudentSubscriptionRepository _studentSubscriptionRepository;

        public StudentSubscriptionServices(IStudentSubscriptionRepository studentSubscriptionRepository)
        {
            _studentSubscriptionRepository = studentSubscriptionRepository;
        }

        public async Task<ServiceResponse<string>> CoinTransactionAsync(CoinTransactionRequestDTO request)
        {
            return await _studentSubscriptionRepository.CoinTransactionAsync(request);
        }

        public async Task<ServiceResponse<StudentCoinTransactionResult>> GetStudentCoinTransactionsAsync(int studentId)
        {
            return await _studentSubscriptionRepository.GetStudentCoinTransactionsAsync(studentId);
        }

        public async Task<ServiceResponse<List<SubscriptionResponseDTO>>> GetSubscriptionPackages(SubscriptionRequestDTO request)
        {
            return await _studentSubscriptionRepository.GetSubscriptionPackages(request);
        }

        public async Task<ServiceResponse<StudentSubscriptionRepository.TransactionDetailsResponse>> GetTransactionDetailsByOrderIdAsync(string orderId)
        {
            return await _studentSubscriptionRepository.GetTransactionDetailsByOrderIdAsync(orderId);
        }

        public async Task<ServiceResponse<StudentSubscriptionRepository.InitiateRazorpayResponse>> InitiateTransactionAsync(InitiateTransactionRequest request)
        {
            return await _studentSubscriptionRepository.InitiateTransactionAsync(request);
        }

        public async Task<ServiceResponse<string>> InsertStudentSubscriptionAsync(StudentSubscriptionInsertRequest request)
        {
            return await _studentSubscriptionRepository.InsertStudentSubscriptionAsync(request);
        }
    }
}
