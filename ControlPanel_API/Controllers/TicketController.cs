using ControlPanel_API.DTOs;
using ControlPanel_API.Models;
using ControlPanel_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ControlPanel_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class TicketController : ControllerBase
    {

        private readonly ITicketServices _ticketService;

        public TicketController(ITicketServices ticketServices)
        {
            _ticketService = ticketServices;
        }


        [HttpPost("AddTicket")]
        public async Task<IActionResult> AddTicket(Ticket request)
        {
            try
            {
                var data = await _ticketService.AddTicket(request);
                if (data != null)
                {
                    return Ok(data);

                }
                else
                {
                    return BadRequest("Bad Request");
                }

            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }

        }
        [HttpPost("GetAllTickets")]
        public async Task<IActionResult> GetAllTickets(GeAllTicketsRequest request)
        {
            try
            {
                var data = await _ticketService.GetAllTicketsList(request);
                if (data != null)
                {
                    return Ok(data);

                }
                else
                {
                    return BadRequest("Bad Request");
                }

            }
            catch (Exception e)
            {
                return this.BadRequest(e.Message);
            }

        }
    }
}
