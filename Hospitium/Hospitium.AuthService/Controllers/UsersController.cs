using Hospitium.AuthService.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospitium.AuthService.Controllers
{
    /// <summary>
    /// Controller for managing user-related operations, primarily for administrative purposes.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class.
        /// </summary>
        /// <param name="authService">The authentication service.</param>
        public UsersController(IAuthService authService)
        {
            _authService = authService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("managers")]
        public async Task<IActionResult> GetManagers()
        {
            var result = await _authService.GetAllManagersAsync();
            return Ok(result);
        }
    }
}
