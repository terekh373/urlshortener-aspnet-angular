using Microsoft.EntityFrameworkCore;
using UrlShortener.Core.Entities;
using UrlShortener.Infrastructure.Data;
using UrlShortener.Infrastructure.Repositories;

namespace UrlShortener.Tests.Repositories;

public class UrlRepositoryTests
{
    private AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    private AppUser CreateTestUser() => new AppUser
    {
        Id = "user-1",
        UserName = "testuser"
    };

    [Fact]
    public async Task AddAsync_ShouldAddUrl()
    {
        using var context = CreateContext("AddTest");
        var user = CreateTestUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repo = new UrlRepository(context);
        var url = new ShortenedUrl
        {
            OriginalUrl = "https://google.com",
            ShortCode = "abc123",
            CreatedById = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        await repo.AddAsync(url);
        await repo.SaveChangesAsync();

        var result = await repo.GetAllAsync();
        Assert.Single(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectUrl()
    {
        using var context = CreateContext("GetByIdTest");
        var user = CreateTestUser();
        context.Users.Add(user);
        var url = new ShortenedUrl
        {
            OriginalUrl = "https://example.com",
            ShortCode = "xyz789",
            CreatedById = user.Id,
            CreatedAt = DateTime.UtcNow
        };
        context.ShortenedUrls.Add(url);
        await context.SaveChangesAsync();

        var repo = new UrlRepository(context);
        var result = await repo.GetByIdAsync(url.Id);

        Assert.NotNull(result);
        Assert.Equal("https://example.com", result.OriginalUrl);
    }

    [Fact]
    public async Task GetByOriginalUrlAsync_ShouldReturnUrl_WhenExists()
    {
        using var context = CreateContext("GetByOriginalTest");
        var user = CreateTestUser();
        context.Users.Add(user);
        context.ShortenedUrls.Add(new ShortenedUrl
        {
            OriginalUrl = "https://github.com",
            ShortCode = "gh1234",
            CreatedById = user.Id,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var repo = new UrlRepository(context);
        var result = await repo.GetByOriginalUrlAsync("https://github.com");

        Assert.NotNull(result);
        Assert.Equal("gh1234", result.ShortCode);
    }

    [Fact]
    public async Task GetByOriginalUrlAsync_ShouldReturnNull_WhenNotExists()
    {
        using var context = CreateContext("GetByOriginalNullTest");
        var repo = new UrlRepository(context);

        var result = await repo.GetByOriginalUrlAsync("https://notexists.com");

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveUrl()
    {
        using var context = CreateContext("DeleteTest");
        var user = CreateTestUser();
        context.Users.Add(user);
        var url = new ShortenedUrl
        {
            OriginalUrl = "https://delete-me.com",
            ShortCode = "del123",
            CreatedById = user.Id,
            CreatedAt = DateTime.UtcNow
        };
        context.ShortenedUrls.Add(url);
        await context.SaveChangesAsync();

        var repo = new UrlRepository(context);
        await repo.DeleteAsync(url.Id);
        await repo.SaveChangesAsync();

        var result = await repo.GetAllAsync();
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByShortCodeAsync_ShouldReturnCorrectUrl()
    {
        using var context = CreateContext("GetByShortCodeTest");
        var user = CreateTestUser();
        context.Users.Add(user);
        context.ShortenedUrls.Add(new ShortenedUrl
        {
            OriginalUrl = "https://shortcode.com",
            ShortCode = "sc9999",
            CreatedById = user.Id,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var repo = new UrlRepository(context);
        var result = await repo.GetByShortCodeAsync("sc9999");

        Assert.NotNull(result);
        Assert.Equal("https://shortcode.com", result.OriginalUrl);
    }
}