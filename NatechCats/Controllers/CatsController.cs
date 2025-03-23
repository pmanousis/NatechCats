using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NatechCats.Entities;
using Newtonsoft.Json;

namespace NatechCats.Controllers;

[ApiController]
[Route("api/cats")]
public class CatsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;

    public CatsController(AppDbContext context, IHttpClientFactory httpClientFactory)
    {
        _context = context;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://api.thecatapi.com/v1/");
        _httpClient.DefaultRequestHeaders.Add("x-api-key", "live_IeQNddWllNCWN4sIN7EiUmhlFNxHge4In7KMGLb2Dfb9ohAa7MldHuNzkfamPL3x"); // my API key
    }
    
    /// <summary>
    /// Fetches at most 25 cats that have breeds from https://api.thecatapi.com/
    /// </summary>
    /// <remarks>If a cat is already in the DB, it is not stored/updated.</remarks>
    /// <response code="200">Cats fetched and saved</response>
    /// <response code="500">Error in fetching or inserting in DB</response>
    [HttpPost("fetch")]
    public async Task<IActionResult> FetchCats()
    {
        var response = await _httpClient.GetAsync("images/search?limit=25&has_breeds=1");
        if (!response.IsSuccessStatusCode) return BadRequest("Failed to fetch cats from API.");

        var content = await response.Content.ReadAsStringAsync();
        var cats = JsonConvert.DeserializeObject<List<dynamic>>(content);
        if (cats == null || cats.Count == 0) return BadRequest("Failed to fetch cats from API.");

        foreach (var cat in cats)
        {
            var catIdString = (string)cat.id;
            if (_context.Cats.Any(c => c.CatId == catIdString)) continue;

            var imageResponse = await _httpClient.GetAsync((string)cat.url);
            var imageBytes = await imageResponse.Content.ReadAsByteArrayAsync();
            
            var newCat = new Cat
            {
                CatId = (string)cat.id,
                Width = (int)cat.width,
                Height = (int)cat.height,
                Image = imageBytes,
                Created = DateTime.UtcNow,
                CatTags = new HashSet<CatTag>()
            };
            _context.Cats.Add(newCat);
            await _context.SaveChangesAsync();

            if (cat.breeds != null && ((Newtonsoft.Json.Linq.JArray)cat.breeds).Count > 0)
            {
                var breed = cat.breeds[0];
                if (breed.temperament != null)
                {
                    var temperaments = ((string)breed.temperament).Split(",");
                    foreach (var temperamentToTrim in temperaments)
                    {
                        var temperament = temperamentToTrim.Trim();
                        var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == temperament);
                        if (tag == null)
                        {
                            tag = new Tag {
                                Name = temperament,
                                Created = DateTime.UtcNow,
                                CatTags = new HashSet<CatTag>()
                            };
                            _context.Tags.Add(tag);
                            await _context.SaveChangesAsync();
                        }
                        var newCatTag = new CatTag { CatId = newCat.Id, TagId = tag.Id }; 
                        _context.CatTags.Add(newCatTag);
                        
                        // let newCat and tag have in their catTags the newCatTag association
                        newCat.CatTags.Add(newCatTag);
                        tag.CatTags!.Add(newCatTag);
                        await _context.SaveChangesAsync();
                    }
                }
            }
            
        }
        await _context.SaveChangesAsync();
        return Ok("Cats fetched and saved successfully.");
    }
    
    /// <summary>
    /// Gets a specific cat with a given id from our DB
    /// </summary>
    /// <param name="id">The id of the cat you are looking for</param>
    /// <response code="200">Cat fetched successfully</response>
    /// <response code="404">Cat was not found</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<Cat>> GetCat(int id)
    {
        var cat = await _context.Cats.FindAsync(id);

        if (cat == null)
        {
            return NotFound();
        }

        return cat;
    }
    
    /// <summary>
    /// Gets a number of cats, paginated from our DB, ordered by their DB id. You may apply a criterion (having a specific tag) or not.
    /// </summary>
    /// <remarks>An empty array is a valid answer if no cats with specific criteria were not found.</remarks>
    /// <param name="page">The page number of cats</param>
    /// <param name="pageSize">The number of cats per page</param>
    /// <param name="tag">The specific tag that the cats you are looking for must have</param>
    /// <response code="200">Page with cats fetched successfully</response>
    /// <response code="500">A DB error occured during cat search</response>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cat>>> GetCats(int page = 1, int pageSize = 10, string? tag = null)
    {
        var query = _context.Cats.AsQueryable();

        if (!string.IsNullOrEmpty(tag))
        {
            query = query.Where(c => c.CatTags!.Any(ct => ct.Tag.Name.ToUpper() == tag.ToUpper()));
        }

        var cats = await query
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return cats;
    }
}
