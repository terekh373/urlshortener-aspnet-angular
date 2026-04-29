using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UrlShortener.Core.Entities;

namespace UrlShortener.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IConfiguration _config;

    public AuthController(UserManager<AppUser> userManager, IConfiguration config)
    {
        _userManager = userManager;
        _config = config;
    }

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByNameAsync(dto.Username);
        if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(new { message = "Invalid username or password" });

        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateToken(user, roles);

        return Ok(new
        {
            token,
            username = user.UserName,
            role = roles.FirstOrDefault()
        });
    }

    // POST api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var user = new AppUser { UserName = dto.Username, Email = dto.Email };
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        await _userManager.AddToRoleAsync(user, "User");
        return Ok(new { message = "Registered successfully" });
    }

    private string GenerateToken(AppUser user, IList<string> roles)
    {
        var jwt = _config.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"]!));

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.Role, roles.FirstOrDefault() ?? "User")
        };

        var token = new JwtSecurityToken(
            issuer: jwt["Issuer"],
            audience: jwt["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(int.Parse(jwt["ExpiryDays"]!)),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

// DTO classes for login and registration
public record LoginDto(string Username, string Password);
public record RegisterDto(string Username, string Email, string Password);