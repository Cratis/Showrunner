// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Playbill.given;

/// <summary>
/// A call to one of the endpoints, with the response it gave captured.
/// </summary>
public class an_endpoint_call : Specification
{
    protected IReleaseNotes _notes;
    protected DefaultHttpContext _http;
    protected MemoryStream _body;

    /// <summary>
    /// Gets the status code of the response.
    /// </summary>
    protected int StatusCode => _http.Response.StatusCode;

    /// <summary>
    /// Gets the body of the response as text.
    /// </summary>
    protected string Text => Encoding.UTF8.GetString(_body.ToArray());

    void Establish()
    {
        _notes = ReleaseHistory.Of(
            new FakeReleaseNoteFiles()
                .Release("1.0.0", "## Added\n\n- The first release. (#1)\n")
                .Release("1.1.0", "Faster startup.\n\n## Fixed\n\n- A crash is gone.\n")
                .With("1.1.0/1.1.0.json", "{ \"releasedAt\": \"2026-03-02T10:00:00Z\", \"pull\": 12 }")
                .With("1.1.0/hero.png", "not really a png"),
            "1.1.0");
        _body = new();
        _http = new DefaultHttpContext { RequestServices = new ServiceCollection().AddLogging().BuildServiceProvider() };
        _http.Response.Body = _body;
    }

    /// <summary>
    /// Runs the result an endpoint gave against the response.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns>An awaitable task.</returns>
    protected Task Execute(IResult result) => result.ExecuteAsync(_http);
}
#endif
