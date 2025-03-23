using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.Protected;
using NatechCats.Controllers;
using NatechCats.Entities;
using Newtonsoft.Json;
using Xunit;

namespace NatechCats.Tests;

internal class CatApiResponseItem
{
    public CatApiResponseItem(string id, string url, int width, int height, List<Breed> breeds)
    {
        this.id = id;
        this.url = url;
        this.width = width;
        this.height = height;
        this.breeds = breeds;
    }

    public string id { get; set; }
    public string url { get; set; }
    public int width { get; set; }
    public int height { get; set; }
    public List<Breed> breeds { get; set; }

    public class Breed
    {
        public string temperament { get; set; }
    }
}
public class CatsControllerTests
{
    private DbContextOptions<AppDbContext> CreateInMemoryDatabaseOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task GetCat_ReturnsCat_WhenCatExists()
    {
        // Arrange
        await using var context = new AppDbContext(CreateInMemoryDatabaseOptions());
        var cat = new Cat { Id = 1, CatId = "abc", Width = 100, Height = 100, Image = [], Created = DateTime.UtcNow };
        context.Cats.Add(cat);
        await context.SaveChangesAsync();

        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient());
        var controller = new CatsController(context, mockHttpClientFactory.Object);

        // Act
        var result = await controller.GetCat(1);

        // Assert
        var actionResult = Assert.IsType<ActionResult<Cat>>(result);
        var actualCat = Assert.IsType<Cat>(actionResult.Value);
        Assert.Equal(cat.Id, actualCat.Id);
    }

    [Fact]
    public async Task GetCat_ReturnsNotFound_WhenCatDoesNotExist()
    {
        // Arrange
        await using var context = new AppDbContext(CreateInMemoryDatabaseOptions());
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient());
        var controller = new CatsController(context, mockHttpClientFactory.Object);

        // Act
        var result = await controller.GetCat(1);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetCats_ReturnsCats_WithPaging()
    {
        // Arrange
        await using var context = new AppDbContext(CreateInMemoryDatabaseOptions());
        var cats = new List<Cat>();
        for (var i = 1; i <= 25; i++)
        {
            cats.Add(new Cat { Id = i, CatId = $"cat{i}", Width = 100, Height = 100, Image = [], Created = DateTime.UtcNow });
        }
        context.Cats.AddRange(cats);
        await context.SaveChangesAsync();

        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient());
        var controller = new CatsController(context, mockHttpClientFactory.Object);

        // Act
        var result = await controller.GetCats(page: 2, pageSize: 10);

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<Cat>>>(result);
        var actualCats = Assert.IsType<IEnumerable<Cat>>(actionResult.Value, exactMatch: false).ToList();
        Assert.Equal(10, actualCats.Count);
        Assert.Equal(11, actualCats.First().Id);
        Assert.Equal(20, actualCats.Last().Id);
    }

    [Fact]
    public async Task GetCats_ReturnsCats_WithTagFilter()
    {
        // Arrange
        await using var context = new AppDbContext(CreateInMemoryDatabaseOptions());
        var tag1 = new Tag { Id = 1, Name = "Playful", Created = DateTime.UtcNow };
        var tag2 = new Tag { Id = 2, Name = "Lazy", Created = DateTime.UtcNow };
        context.Tags.AddRange(tag1, tag2);

        var cat1 = new Cat { Id = 1, CatId = "cat1", Width = 100, Height = 100, Image = [], Created = DateTime.UtcNow };
        var cat2 = new Cat { Id = 2, CatId = "cat2", Width = 100, Height = 100, Image = [], Created = DateTime.UtcNow };
        context.Cats.AddRange(cat1, cat2);

        context.CatTags.AddRange(
            new CatTag { CatId = 1, TagId = 1 },
            new CatTag { CatId = 2, TagId = 2 }
        );
        await context.SaveChangesAsync();

        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(new HttpClient());
        var controller = new CatsController(context, mockHttpClientFactory.Object);

        // Act
        var result = await controller.GetCats(tag: "Playful");

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<Cat>>>(result);
        var actualCats = Assert.IsAssignableFrom<IEnumerable<Cat>>(actionResult.Value).ToList();
        Assert.Single(actualCats);
        Assert.Equal(1, actualCats.First().Id);
    }
        
        

    [Fact]
    public async Task FetchCats_ReturnsOk_AndSavesCats()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new AppDbContext(options);

        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

        var catsApiResponse = new List<CatApiResponseItem>
        {
            new CatApiResponseItem(id: "cat1", url: "http://example.com/cat1.jpg", width: 100, height: 100,
                breeds: [new CatApiResponseItem.Breed { temperament = "Playful, Energetic" }]),
            new CatApiResponseItem(id: "cat2", url: "http://example.com/cat2.jpg", width: 200, height: 200,
                breeds: [new CatApiResponseItem.Breed { temperament = "Lazy" }]),
            new CatApiResponseItem(id: "cat2", url: "http://example.com/cat2.jpg", width: 200, height: 200,
                breeds: [new CatApiResponseItem.Breed { temperament = "Lazy" }])
        };

        var catsApiJson = JsonConvert.SerializeObject(catsApiResponse);

        mockHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(catsApiJson, Encoding.UTF8, "application/json")
            });

        var mockHttpClient = new HttpClient(mockHttpMessageHandler.Object);
        mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(mockHttpClient);

        var controller = new CatsController(context, mockHttpClientFactory.Object);

        // Mocking the image download
        var mockImageHttpMessageHandler = new Mock<HttpMessageHandler>();
        mockImageHttpMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent([0x01, 0x02, 0x03])
            });

        var mockImageHttpClient = new HttpClient(mockImageHttpMessageHandler.Object);
        mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(mockImageHttpClient);

        // Act
        var result = await controller.FetchCats();

        // Assert
        Assert.IsType<OkObjectResult>(result);
        Assert.Equal(2, context.Cats.Count());
        Assert.Equal(3, context.Tags.Count()); //Playful, Energetic, Lazy
        Assert.Equal(3, context.CatTags.Count());
    }
}