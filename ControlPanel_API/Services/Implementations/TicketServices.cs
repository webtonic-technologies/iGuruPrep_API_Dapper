using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using ControlPanel_API.Services.Interfaces;

namespace ControlPanel_API.Services.Implementations
{
    public class TicketServices : ITicketServices
    {
        private readonly ITicketRepository _ticketRepository;

        public TicketServices(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }
        public async Task<ServiceResponse<string>> AddTicket(Ticket request)
        {
            try
            {
                return await _ticketRepository.AddTicket(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<List<Ticket>>> GetAllTicketsList(GeAllTicketsRequest request)
        {
            try
            {
                return await _ticketRepository.GetAllTicketsList(request);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Ticket>>(false, ex.Message, new List<Ticket>(), 500);
            }
        }
    }
}
