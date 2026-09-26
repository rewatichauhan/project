using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace apiproject.Tests;

[TestFixture]
public class GistsApiTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    // ── Happy path ───────────────────────────────────────────────────────────

    [Test]
    public async Task GetGists_KnownUser_ReturnsOk()
    {
        var response = await _client.GetAsync("/octocat?page=1&perPage=10");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetGists_KnownUser_ReturnsJsonArray()
    {
        var response = await _client.GetAsync("/octocat?page=1&perPage=10");
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(body.TrimStart(), Does.StartWith("["),
            "Response body should be a JSON array");
    }

    [Test]
    public async Task GetGists_KnownUser_GistsContainExpectedFields()
    {
        var response = await _client.GetAsync("/octocat?page=1&perPage=10");
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(body, Does.Contain("\"id\""));
        Assert.That(body, Does.Contain("\"url\""));
        Assert.That(body, Does.Contain("\"description\""));
    }

    // ── Pagination ───────────────────────────────────────────────────────────

    [Test]
    public async Task GetGists_PerPageOne_ReturnsSingleItem()
    {
        var response = await _client.GetAsync("/octocat?page=1&perPage=1");
        var body = await response.Content.ReadAsStringAsync();

        var array = System.Text.Json.JsonDocument.Parse(body).RootElement;
        Assert.That(array.ValueKind, Is.EqualTo(System.Text.Json.JsonValueKind.Array));
        Assert.That(array.GetArrayLength(), Is.EqualTo(1), "Expected exactly one gist when perPage=1");
    }

    [Test]
    public async Task GetGists_DifferentPages_ReturnDifferentResults()
    {
        var page1 = await _client.GetAsync("/octocat?page=1&perPage=1");
        var page2 = await _client.GetAsync("/octocat?page=2&perPage=1");

        var body1 = await page1.Content.ReadAsStringAsync();
        var body2 = await page2.Content.ReadAsStringAsync();

        Assert.That(body1, Is.Not.EqualTo(body2),
            "Page 1 and page 2 should return different gists");
    }

    // ── Caching ──────────────────────────────────────────────────────────────

    [Test]
    public async Task GetGists_SameRequest_SecondCallIsFasterDueToCache()
    {
        // Warm the cache
        await _client.GetAsync("/octocat?page=1&perPage=5");

        var sw = System.Diagnostics.Stopwatch.StartNew();
        await _client.GetAsync("/octocat?page=1&perPage=5");
        sw.Stop();

        // Cached response should come back well under 100 ms (no network round-trip)
        Assert.That(sw.ElapsedMilliseconds, Is.LessThan(100),
            "Second identical request should be served from cache");
    }

    // ── Error cases ──────────────────────────────────────────────────────────

    [Test]
    public async Task GetGists_NonExistentUser_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/this-user-definitely-does-not-exist-xyzzy9999?page=1&perPage=10");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetGists_RootPath_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}
