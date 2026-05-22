using EnvComplianceTracker.Api.Auth;
using EnvComplianceTracker.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnvComplianceTracker.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    public record LoginRequest(string Username, string Password);
    public record LoginResponse(string Token, string Username, string Role);

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest req,
        [FromServices] ComplianceDbContext db, [FromServices] JwtTokenService tokens)
    {
        var user = await db.Users.SingleOrDefaultAsync(u => u.Username == req.Username);
        if (user is null || !PasswordHasher.Verify(req.Password, user.PasswordHash))
            return Unauthorized();
        return new LoginResponse(tokens.Issue(user), user.Username, user.Role);
    }
}
