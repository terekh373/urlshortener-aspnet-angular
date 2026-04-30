using Microsoft.EntityFrameworkCore;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Interfaces;
using UrlShortener.Infrastructure.Data;

namespace UrlShortener.Infrastructure.Repositories;

public class UrlRepository : IUrlRepository
{
    private readonly AppDbContext _db;

    public UrlRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<ShortenedUrl>> GetAllAsync() =>
        await _db.ShortenedUrls.Include(x => x.CreatedBy).ToListAsync();

    public async Task<ShortenedUrl?> GetByIdAsync(int id) =>
        await _db.ShortenedUrls.Include(x => x.CreatedBy)
                               .FirstOrDefaultAsync(x => x.Id == id);

    public async Task<ShortenedUrl?> GetByOriginalUrlAsync(string originalUrl) =>
        await _db.ShortenedUrls.FirstOrDefaultAsync(x => x.OriginalUrl == originalUrl);

    public async Task<ShortenedUrl?> GetByShortCodeAsync(string shortCode) =>
        await _db.ShortenedUrls.FirstOrDefaultAsync(x => x.ShortCode == shortCode);

    public async Task AddAsync(ShortenedUrl url) => await _db.ShortenedUrls.AddAsync(url);

    public async Task DeleteAsync(int id)
    {
        var url = await _db.ShortenedUrls.FindAsync(id);
        if (url != null) _db.ShortenedUrls.Remove(url);
    }

    public async Task IncrementClickCountAsync(string shortCode)
    {
        var url = await _db.ShortenedUrls
                           .FirstOrDefaultAsync(x => x.ShortCode == shortCode);
        if (url != null)
        {
            url.ClickCount++;
            await _db.SaveChangesAsync();
        }
    }

    public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
}