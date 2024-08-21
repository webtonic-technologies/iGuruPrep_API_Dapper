using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.Response;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Repository.Interfaces;
using ControlPanel_API.Services.Interfaces;

namespace ControlPanel_API.Services.Implementations
{
    public class ContactUsServices : IContactUsServices
    {
        private readonly IContactUsRepository _contactUsRepository;

        public ContactUsServices(IContactUsRepository contactUsRepository)
        {
            _contactUsRepository = contactUsRepository;
        }

        public async Task<ServiceResponse<string>> ChangeStatus(ChangeStatusRequest request)
        {
            try
            {
                return await _contactUsRepository.ChangeStatus(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<List<GetAllContactUsResponse>>> GetAllContactUs(GeAllContactUsRequest request)
        {
            try
            {
                return await _contactUsRepository.GetAllContactUs(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<GetAllContactUsResponse>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<GetAllContactUsResponse>> GetContactUsById(int contactusId)
        {
            try
            {
                return await _contactUsRepository.GetContactUsById(contactusId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<GetAllContactUsResponse>(false, ex.Message, new GetAllContactUsResponse(), 500);
            }
        }
    }
}
