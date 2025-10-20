using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    public static User user = new();

    [HttpPost("register")]
    public async Task<ActionResult<User>> Register(UserDto request)
    {
        var user = await authService.RegisterAsync(request);

        if(user is null)
        {
            return BadRequest("Username already exists.");
        }

        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponseDto>> Login(UserDto request)
    {
        var token = await authService.LoginAsync(request);

        if (token is null)
        {
            return BadRequest("Invalid username or password");
        }

        return Ok(token);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenDto dirtyRequest)
    {
        if (!Guid.TryParse(dirtyRequest.UserId, out var parsedUserId))
        {
            return Unauthorized("Invalid refresh token");
        }
        var request = new RefreshTokenRequestDto
        {
            UserId = parsedUserId,
            RefreshToken = dirtyRequest.RefreshToken
        };

        var result = await authService.RefreshTokensAsync(request);
        if (result is null || result.AccessToken is null || result.RefreshToken is null)
            return Unauthorized("Invalid refresh token");

        return Ok(result);
    }

    [Authorize]
    [HttpGet]
    public IActionResult AuthenticatedOnlyEndPoint()
    {
        return Ok("You are authenticated!");
    }

    public class RefreshTokenDto{
        public required string UserId { get; set; }
        public required string RefreshToken { get; set; }
    }
}