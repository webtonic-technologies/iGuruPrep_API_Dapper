using Config_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Config_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class BoardController : ControllerBase
    {
        private readonly IBoardServices _boardService;

        public BoardController(IBoardServices boardService)
        {
            _boardService = boardService;
        }
        [HttpPost]
        public async Task<IActionResult> AddUpdateBoard(Board request)
        {
            try
            {
                var data = await _boardService.AddUpdateBoard(request);
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
        [HttpGet("GetAllBoards")]
        public async Task<IActionResult> GetAllBoardsList()
        {
            try
            {
                var data = await _boardService.GetAllBoards();
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
        [HttpGet("GetBoard/{BoardId}")]
        public async Task<IActionResult> GetBoardById(int BoardId)
        {
            try
            {
                var data = await _boardService.GetBoardById(BoardId);
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
        [HttpPut("Status/{BoardId}")]
        public async Task<IActionResult> StatusActiveInactive(int BoardId)
        {
            try
            {
                var data = await _boardService.StatusActiveInactive(BoardId);
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