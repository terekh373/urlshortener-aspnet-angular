using UrlShortener.Core.Entities;

namespace UrlShortener.Core.Interfaces;

public interface IUrlRepository
{
    Task<IEnumerable<ShortenedUrl>> GetAllAsync();
    Task<ShortenedUrl?> GetByIdAsync(int id);
    Task<ShortenedUrl?> GetByOriginalUrlAsync(string originalUrl);
    Task<ShortenedUrl?> GetByShortCodeAsync(string shortCode);
    Task AddAsync(ShortenedUrl url);
    Task DeleteAsync(int id);
    Task IncrementClickCountAsync(string shortCode);
    Task SaveChangesAsync();
}