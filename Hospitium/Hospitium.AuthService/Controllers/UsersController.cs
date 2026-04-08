using Hospitium.AuthService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Hospitium.AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;

        public UsersController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("managers")]
        public async Task<IActionResult> GetManagers()
        {
            var result = await _authService.GetAllManagersAsync();
            return Ok(result);
        }
    }
}
