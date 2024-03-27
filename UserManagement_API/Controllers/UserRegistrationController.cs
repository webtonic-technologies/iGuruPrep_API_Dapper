using Microsoft.AspNetCore.Mvc;
using UserManagement_API.DTOs.Registration;
using UserManagement_API.Repository.Interfaces;

namespace UserManagement_API.Controllers
{
    [Route("iGuru/[controller]")]
    [ApiController]
    public class UserRegistrationController : ControllerBase
    {
        private readonly IUserRegistrationServices _userRegistrationServices;

        public UserRegistrationController(IUserRegistrationServices userRegistrationServices)
        {
            _userRegistrationServices = userRegistrationServices;
        }

        [HttpPost]
        public async Task<IActionResult> UserRegistration(UserRegistrationDto request)
        {
            try
            {
                var data = await _userRegistrationServices.UserRegistration(request);
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
