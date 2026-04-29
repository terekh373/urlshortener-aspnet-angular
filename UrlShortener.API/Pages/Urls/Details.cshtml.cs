using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UrlShortener.Core.Interfaces;

namespace UrlShortener.API.Pages.Urls;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly IUrlRepository _repo;

    public DetailsModel(IUrlRepository repo) => _repo = repo;

    public UrlDetailViewModel? UrlItem { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var url = await _repo.GetByIdAsync(id);
        if (url == null) return NotFound();

        UrlItem = new UrlDetailViewModel
        {
            OriginalUrl = url.OriginalUrl,
            ShortUrl = $"{Request.Scheme}://{Request.Host}/s/{url.ShortCode}",
            CreatedBy = url.CreatedBy?.UserName ?? "Unknown",
            CreatedAt = url.CreatedAt
        };

        return Page();
    }
}

public class UrlDetailViewModel
{
    public string OriginalUrl { get; set; } = string.Empty;
    public string ShortUrl { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}