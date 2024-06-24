using Course_API.DTOs.Requests;
using Course_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Course_API.Controllers
{
    [Route("iGuru/Course/[controller]")]
    [ApiController]
    public class BookController : ControllerBase
    {
        private readonly IBookServices _bookServices;

        public BookController(IBookServices bookServices)
        {
            _bookServices = bookServices;
        }
        [HttpPost("GetAllBook")]
        public async Task<IActionResult> GetListOfBooks(BookListDTO request)
        {
            try
            {
                var data = await _bookServices.GetAll(request);
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
        [HttpGet("GetBookById/{BookId}")]
        public async Task<IActionResult> GetBookById(int BookId)
        {
            try
            {
                var data = await _bookServices.Get(BookId);
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
        [HttpPost("AddBook")]
        public async Task<IActionResult> AddBook([FromBody] BookDTO bookDTO)
        {
            try
            {
                var data = await _bookServices.Add(bookDTO);
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
        [HttpPut("UpdateBook")]
        public async Task<IActionResult> UpdateBook([FromBody] BookDTO bookDTO)
        {
            try
            {
                var data = await _bookServices.Update(bookDTO);
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
