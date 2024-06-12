using Config_API.DTOs.Requests;
using Config_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Config_API.Controllers
{
    [Route("iGuru/Configure/[controller]")]
    [ApiController]
    public class BoardController : ControllerBase
    {
        private readonly IBoardServices _boardService;

        public BoardController(IBoardServices boardService)
        {
            _boardService = boardService;
        }
        [HttpPost("AddUpdate")]
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
        [HttpPost("GetAllBoards")]
        public async Task<IActionResult> GetAllBoardsList(GetAllBoardsRequest request)
        {
            try
            {
                var data = await _boardService.GetAllBoards(request);
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
        [HttpGet("GetBoardById/{BoardId}")]
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
        [HttpGet("GetAllBoardsMasters")]
        public async Task<IActionResult> GetAllBoardsMasters()
        {
            try
            {
                var data = await _boardService.GetAllBoardsMaster();
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
