using backend_disc.Dtos.Auth;
using backend_disc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_disc.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.Login(dto);
            if(result == null)
            {
                return Unauthorized("Invalid username or password.");
            }
            return Ok(result);
        }

        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPost("error")]
        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Error([FromBody] LoginDto dto)
        {
            throw new Exception("This is a test exception for global error handling. need smal code change for sonar");
        }
    
    }
}
