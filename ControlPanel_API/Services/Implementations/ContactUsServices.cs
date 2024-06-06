using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
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
        public async Task<ServiceResponse<string>> AddTicket(ContactUs request)
        {
            try
            {
                return await _contactUsRepository.AddTicket(request);
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
    }
}
