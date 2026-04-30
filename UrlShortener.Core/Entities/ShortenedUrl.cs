namespace UrlShortener.Core.Entities;

public class ShortenedUrl
{
    public int Id { get; set; }
    public string OriginalUrl { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int ClickCount { get; set; } = 0;
    public string CreatedById { get; set; } = string.Empty;
    public AppUser CreatedBy { get; set; } = null!;
}