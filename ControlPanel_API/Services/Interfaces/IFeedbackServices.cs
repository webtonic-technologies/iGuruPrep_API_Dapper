﻿using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;

namespace ControlPanel_API.Services.Interfaces
{
    public interface IFeedbackServices
    {
        Task<ServiceResponse<string>> AddFeedBack(Feedback request);
        Task<ServiceResponse<string>> UpdateFeedback(Feedback request);
        Task<ServiceResponse<string>> AddSyllabus(Syllabus request);
        Task<ServiceResponse<List<GetAllFeedbackResponse>>> GetAllFeedBackList(GetAllFeedbackRequest request);
    }
}