using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;

namespace ControlPanel_API.Repository.Interfaces
{
    public interface ITicketRepository
    {
        Task<ServiceResponse<string>> AddTicket(Ticket request);
        Task<ServiceResponse<List<Ticket>>> GetAllTicketsList(GeAllTicketsRequest request);
    }
}
