using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using OW3N.Tests.TestSupport;
using Xunit;

namespace OW3N.Tests.Unit;

/// <summary>
/// Tests for <see cref="DiscordService"/>: it posts message/embed payloads to the configured
/// webhook URL, and derives a deterministic embed colour from the title when none is supplied.
/// </summary>
public class DiscordServiceTests
{
    private const string WebhookUrl = "https://example.test/webhook";

    private static (DiscordService service, StubHttpMessageHandler handler) CreateService()
    {
        var handler = StubHttpMessageHandler.WithResponse(HttpStatusCode.NoContent);
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["DiscordWebhookUrl"] = WebhookUrl })
            .Build();
        return (new DiscordService(config, new HttpClient(handler)), handler);
    }

    [Fact]
    public async Task SendMessageAsync_posts_content_payload_to_webhook()
    {
        var (service, handler) = CreateService();

        await service.SendMessageAsync("hello world");

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal(WebhookUrl, request.RequestUri!.ToString());

        using var doc = JsonDocument.Parse(handler.RequestBodies.Single());
        Assert.Equal("hello world", doc.RootElement.GetProperty("content").GetString());
    }

    [Fact]
    public async Task SendEmbedAsync_uses_explicit_colour()
    {
        var (service, handler) = CreateService();

        await service.SendEmbedAsync("Title", "Description", "#FF0000");

        using var doc = JsonDocument.Parse(handler.RequestBodies.Single());
        var embed = doc.RootElement.GetProperty("embeds")[0];
        Assert.Equal("Title", embed.GetProperty("title").GetString());
        Assert.Equal("Description", embed.GetProperty("description").GetString());
        Assert.Equal(0xFF0000, embed.GetProperty("color").GetInt32());
    }

    [Fact]
    public async Task SendEmbedAsync_derives_a_deterministic_colour_from_the_title()
    {
        var (service, handler) = CreateService();

        await service.SendEmbedAsync("Consistent", "first");
        await service.SendEmbedAsync("Consistent", "second");

        var colours = handler.RequestBodies
            .Select(b => JsonDocument.Parse(b).RootElement.GetProperty("embeds")[0].GetProperty("color").GetInt32())
            .ToList();

        // Same title -> same colour, and it must be a valid 24-bit RGB value.
        Assert.Equal(colours[0], colours[1]);
        Assert.InRange(colours[0], 0, 0xFFFFFF);
    }
}
