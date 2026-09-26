using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/{username}", async (
    string username,
    int page,
    int perPage,
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache) =>
{
    // clamp inputs so callers can't request absurd page sizes
    page = Math.Max(1, page);
    perPage = Math.Clamp(perPage, 1, 100);

    var cacheKey = $"gists:{username}:page={page}:per_page={perPage}";

    if (!cache.TryGetValue(cacheKey, out string? cached))
    {
        try
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("User-Agent", "GistFetcher");

            var response = await client.GetAsync(
                $"https://api.github.com/users/{username}/gists?page={page}&per_page={perPage}");

            if (!response.IsSuccessStatusCode)
            {
                return Results.NotFound(new { error = $"User '{username}' not found or has no public gists" });
            }

            cached = await response.Content.ReadAsStringAsync();

            cache.Set(cacheKey, cached, TimeSpan.FromMinutes(5));
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    return Results.Content(cached, "application/json");
})
.WithName("GetUserGists")
.WithOpenApi();

app.Run();

public partial class Program { }
