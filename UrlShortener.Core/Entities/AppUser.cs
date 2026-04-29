using Microsoft.AspNetCore.Identity;

namespace UrlShortener.Core.Entities;

public class AppUser : IdentityUser
{
    public ICollection<ShortenedUrl> ShortenedUrls { get; set; } = new List<ShortenedUrl>();
}