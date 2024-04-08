using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;

namespace ControlPanel_API.Services.Interfaces
{
    public interface ITicketServices
    {
        Task<ServiceResponse<string>> AddTicket(Ticket request);
        Task<ServiceResponse<List<Ticket>>> GetAllTicketsList(GeAllTicketsRequest request);
    }
}
