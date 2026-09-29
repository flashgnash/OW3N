using System.Net;

namespace OW3N.Tests.TestSupport;

/// <summary>
/// A test double for <see cref="HttpMessageHandler"/> that records every request it receives
/// and returns a response produced by a caller-supplied delegate. Lets tests assert on the
/// outgoing payload (Discord webhooks) or stub the external roll service.
/// </summary>
public sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

    public List<HttpRequestMessage> Requests { get; } = new();
    public List<string> RequestBodies { get; } = new();

    public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responder = responder;
    }

    /// <summary>Always responds with the given status code and (optional) JSON body.</summary>
    public static StubHttpMessageHandler WithResponse(HttpStatusCode status, string? jsonBody = null)
        => new(_ => new HttpResponseMessage(status)
        {
            Content = new StringContent(jsonBody ?? "", System.Text.Encoding.UTF8, "application/json"),
        });

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);
        RequestBodies.Add(request.Content == null
            ? ""
            : await request.Content.ReadAsStringAsync(cancellationToken));
        return _responder(request);
    }
}
