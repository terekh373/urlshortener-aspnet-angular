using Microsoft.AspNetCore.Mvc;
using UrlShortener.Core.Interfaces;

namespace UrlShortener.API.Controllers;

[ApiController]
[Route("s")]
public class RedirectController : ControllerBase
{
    private readonly IUrlRepository _repo;

    public RedirectController(IUrlRepository repo) => _repo = repo;

    // GET /s/aB3xZ1
    [HttpGet("{shortCode}")]
    public async Task<IActionResult> RedirectToOriginal(string shortCode)
    {
        var url = await _repo.GetByShortCodeAsync(shortCode);
        if (url == null) return NotFound("Short URL not found");

        await _repo.IncrementClickCountAsync(shortCode);

        return Redirect(url.OriginalUrl);
    }
}