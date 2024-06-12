using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using ControlPanel_API.Services.Interfaces;

namespace ControlPanel_API.Services.Implementations
{
    public class HelpFAQServices : IHelpFAQServices
    {
        private readonly IHelpFAQRepository _helpFAQRepository;
        public HelpFAQServices(IHelpFAQRepository helpFAQRepository)
        {
            _helpFAQRepository = helpFAQRepository;
        }
        public async Task<ServiceResponse<string>> AddUpdateFAQ(HelpFAQ request)
        {
            try
            {
                return await _helpFAQRepository.AddUpdateFAQ(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<HelpFAQ>> GetFAQById(int faqId)
        {
            try
            {
                return await _helpFAQRepository.GetFAQById(faqId);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<HelpFAQ>(false, ex.Message, new HelpFAQ(), 500);
            }
        }

        public async Task<ServiceResponse<List<HelpFAQ>>> GetFAQList(GetAllFAQRequest request)
        {
            try
            {
                return await _helpFAQRepository.GetFAQList(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<HelpFAQ>>(false, ex.Message, [], 500);
            }
        }

        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                return await _helpFAQRepository.StatusActiveInactive(id);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }
    }
}
