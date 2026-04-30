using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Interfaces;

namespace UrlShortener.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UrlsController : ControllerBase
{
    private readonly IUrlRepository _repo;
    private readonly IHttpClientFactory _httpClientFactory;


    public UrlsController(IUrlRepository repo, IHttpClientFactory httpClientFactory)
    {
        _repo = repo;
        _httpClientFactory = httpClientFactory;
    }


    // GET api/urls
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var urls = await _repo.GetAllAsync();
        var result = urls.Select(u => new
        {
            u.Id,
            u.OriginalUrl,
            u.ShortCode,
            ShortUrl = $"{Request.Scheme}://{Request.Host}/s/{u.ShortCode}",
            u.CreatedAt,
            u.ClickCount,
            CreatedBy = u.CreatedBy?.UserName
        });
        return Ok(result);
    }

    // GET api/urls/5
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var url = await _repo.GetByIdAsync(id);
        if (url == null) return NotFound();

        return Ok(new
        {
            url.Id,
            url.OriginalUrl,
            url.ShortCode,
            ShortUrl = $"{Request.Scheme}://{Request.Host}/s/{url.ShortCode}",
            url.CreatedAt,
            url.ClickCount,
            CreatedBy = url.CreatedBy?.UserName
        });
    }

    // POST api/urls
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUrlDto dto)
    {
        if (!Uri.TryCreate(dto.OriginalUrl, UriKind.Absolute, out var uriResult)
            || (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
        {
            return BadRequest(new { message = "Invalid URL format. Must start with http:// or https://" });
        }

        var existing = await _repo.GetByOriginalUrlAsync(dto.OriginalUrl);
        if (existing != null)
            return BadRequest(new { message = "This URL already exists" });

        var urlExists = await CheckUrlExistsAsync(dto.OriginalUrl);
        if (!urlExists)
            return BadRequest(new { message = "URL is not reachable. Please check the URL and try again." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var shortened = new ShortenedUrl
        {
            OriginalUrl = dto.OriginalUrl,
            ShortCode = GenerateShortCode(),
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(shortened);
        await _repo.SaveChangesAsync();

        var created = await _repo.GetByIdAsync(shortened.Id);

        return Ok(new
        {
            created!.Id,
            created.OriginalUrl,
            created.ShortCode,
            ShortUrl = $"{Request.Scheme}://{Request.Host}/s/{created.ShortCode}",
            created.CreatedAt,
            CreatedBy = created.CreatedBy?.UserName
        });
    }

    // DELETE api/urls/5
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var url = await _repo.GetByIdAsync(id);
        if (url == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole("Admin");

        if (!isAdmin && url.CreatedById != userId)
            return Forbid();

        await _repo.DeleteAsync(id);
        await _repo.SaveChangesAsync();
        return NoContent();
    }


    // Checks if the URL is reachable by sending a HEAD request. This is more efficient than a GET request because it doesn't download the entire content of the page, just checks if the server responds with a success status code.
    private async Task<bool> CheckUrlExistsAsync(string url)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("UrlChecker");
            var request = new HttpRequestMessage(HttpMethod.Head, url);
            var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // Algorithm to generate a unique short code for the URL. In a real application, you would want to ensure uniqueness and handle potential collisions, but for simplicity, we just generate a random string here.
    private static string GenerateShortCode()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)])
            .ToArray());
    }
}

public record CreateUrlDto(string OriginalUrl);