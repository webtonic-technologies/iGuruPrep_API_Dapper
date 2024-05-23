using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;

namespace ControlPanel_API.Services.Interfaces
{
    public interface IHelpFAQServices
    {
        Task<ServiceResponse<List<HelpFAQ>>> GetFAQList();
        Task<ServiceResponse<HelpFAQ>> GetFAQById(int faqId);
        Task<ServiceResponse<string>> AddUpdateFAQ(HelpFAQ request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
