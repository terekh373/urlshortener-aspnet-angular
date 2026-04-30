using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Moq.Protected;
using System.Security.Claims;
using UrlShortener.API.Controllers;
using UrlShortener.Core.Entities;
using UrlShortener.Core.Interfaces;

namespace UrlShortener.Tests.Controllers;

public class UrlsControllerTests
{
    private UrlsController CreateController(
    IUrlRepository repo,
    string userId = "user-1",
    string role = "User")
    {
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();

        var controller = new UrlsController(repo, mockHttpClientFactory.Object);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        return controller;
    }

    [Fact]
    public async Task GetAll_ShouldReturnOk()
    {
        var mockRepo = new Mock<IUrlRepository>();
        mockRepo.Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<ShortenedUrl>());

        var controller = CreateController(mockRepo.Object);
        var result = await controller.GetAll();

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenUrlDoesNotExist()
    {
        var mockRepo = new Mock<IUrlRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((ShortenedUrl?)null);

        var controller = CreateController(mockRepo.Object);
        var result = await controller.GetById(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenUrlAlreadyExists()
    {
        var mockRepo = new Mock<IUrlRepository>();
        mockRepo.Setup(r => r.GetByOriginalUrlAsync(It.IsAny<string>()))
                .ReturnsAsync(new ShortenedUrl { OriginalUrl = "https://exists.com" });

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        var httpClient = new HttpClient(mockHandler.Object);

        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        mockHttpClientFactory
            .Setup(f => f.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var controller = new UrlsController(mockRepo.Object, mockHttpClientFactory.Object);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "user-1"),
            new Claim(ClaimTypes.Name, "testuser"),
            new Claim(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        var result = await controller.Create(new CreateUrlDto("https://exists.com"));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Create_ShouldReturnBadRequest_WhenUrlFormatIsInvalid()
    {
        var mockRepo = new Mock<IUrlRepository>();
        var controller = CreateController(mockRepo.Object);

        var result = await controller.Create(new CreateUrlDto("not-a-url"));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnForbid_WhenUserDeletesOthersUrl()
    {
        var mockRepo = new Mock<IUrlRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new ShortenedUrl
                {
                    Id = 1,
                    CreatedById = "other-user-id"
                });

        var controller = CreateController(mockRepo.Object, userId: "user-1", role: "User");
        var result = await controller.Delete(1);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent_WhenAdminDeletesAnyUrl()
    {
        var mockRepo = new Mock<IUrlRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new ShortenedUrl
                {
                    Id = 1,
                    CreatedById = "other-user-id"
                });
        mockRepo.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);
        mockRepo.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var controller = CreateController(mockRepo.Object, userId: "admin-1", role: "Admin");
        var result = await controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ShouldReturnNotFound_WhenUrlDoesNotExist()
    {
        var mockRepo = new Mock<IUrlRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((ShortenedUrl?)null);

        var controller = CreateController(mockRepo.Object);
        var result = await controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }
}