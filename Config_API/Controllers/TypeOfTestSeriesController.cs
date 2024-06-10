using Config_API.DTOs.Requests;
using Config_API.Models;
using Config_API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Config_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class TypeOfTestSeriesController : ControllerBase
    {
        private readonly ITypeOfTestSeriesServices _typeOfTestSeriesServices;

        public TypeOfTestSeriesController(ITypeOfTestSeriesServices typeOfTestSeriesServices)
        {
            _typeOfTestSeriesServices = typeOfTestSeriesServices;
        }
        [HttpPost]
        public async Task<IActionResult> AddUpdateTypeOfTestSeries(TypeOfTestSeries request)
        {
            try
            {
                var data = await _typeOfTestSeriesServices.AddUpdateTestSeries(request);
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
        [HttpPost("GetListOfTestSeries")]
        public async Task<IActionResult> GetListOfTestSeries(GetAllTestSeriesTypesRequest request)
        {
            try
            {
                var data = await _typeOfTestSeriesServices.GetListOfTestSeries(request);
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
        [HttpGet("GetListOfTestSeriesMasters")]
        public async Task<IActionResult> GetListOfTestSeriesMasters()
        {
            try
            {
                var data = await _typeOfTestSeriesServices.GetListOfTestSeriesMasters();
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
        [HttpGet("GetTestSeries/{Id}")]
        public async Task<IActionResult> GetTestSeriesById(int Id)
        {
            try
            {
                var data = await _typeOfTestSeriesServices.GetTestSeriesById(Id);
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
        [HttpPut("Status/{Id}")]
        public async Task<IActionResult> StatusActiveInactive(int Id)
        {
            try
            {
                var data = await _typeOfTestSeriesServices.StatusActiveInactive(Id);
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
