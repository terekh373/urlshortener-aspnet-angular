using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using UrlShortener.Core.Interfaces;

namespace UrlShortener.API.Pages.Urls;

public class DetailsModel : PageModel
{
    private readonly IUrlRepository _repo;
    private readonly IConfiguration _config;

    public DetailsModel(IUrlRepository repo, IConfiguration config)
    {
        _repo = repo;
        _config = config;
    }

    public UrlDetailViewModel? UrlItem { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id, string? token)
    {
        if (string.IsNullOrEmpty(token) || !IsValidToken(token))
        {
            return RedirectToPage("/Login");
        }

        var url = await _repo.GetByIdAsync(id);
        if (url == null) return NotFound();

        UrlItem = new UrlDetailViewModel
        {
            OriginalUrl = url.OriginalUrl,
            ShortUrl = $"{Request.Scheme}://{Request.Host}/s/{url.ShortCode}",
            CreatedBy = url.CreatedBy?.UserName ?? "Unknown",
            CreatedAt = url.CreatedAt,
            ClickCount = url.ClickCount
        };

        return Page();
    }

    private bool IsValidToken(string token)
    {
        try
        {
            var jwt = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Secret"]!));

            var handler = new JwtSecurityTokenHandler();
            handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwt["Issuer"],
                ValidAudience = jwt["Audience"],
                IssuerSigningKey = key
            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class UrlDetailViewModel
{
    public string OriginalUrl { get; set; } = string.Empty;
    public string ShortUrl { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int ClickCount { get; set; }
}