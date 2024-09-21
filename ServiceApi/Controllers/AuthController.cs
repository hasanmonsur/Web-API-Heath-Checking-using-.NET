using Microsoft.AspNetCore.Mvc;
using ServiceApi.Contacts;

namespace ServiceApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _authService;

        public AuthController(IAuthRepository authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel login)
        {
            // In real-world, you'd validate user credentials against a database
            if (login.Username == "user" && login.Password == "password")
            {
                var token = _authService.GenerateJwtToken(login.Username);
                return Ok(new { Token = token });
            }

            return Unauthorized();
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
