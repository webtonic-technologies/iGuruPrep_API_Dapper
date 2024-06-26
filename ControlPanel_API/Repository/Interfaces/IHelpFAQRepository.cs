﻿using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;

namespace ControlPanel_API.Repository.Interfaces
{
    public interface IHelpFAQRepository
    {
        Task<ServiceResponse<List<HelpFAQ>>> GetFAQList(GetAllFAQRequest request);
        Task<ServiceResponse<HelpFAQ>> GetFAQById(int faqId);
        Task<ServiceResponse<string>> AddUpdateFAQ(HelpFAQ request);
        Task<ServiceResponse<bool>> StatusActiveInactive(int id);
    }
}
