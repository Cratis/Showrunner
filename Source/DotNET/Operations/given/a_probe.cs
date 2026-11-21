// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.Probe;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// A probe of the three Cratis sites against an HTTP handler the specification scripts, on a manual clock.
/// </summary>
public abstract class a_probe : Specification
{
    protected ManualTimeProvider _time;
    protected StubHttpHandler _handler;
    protected Func<HttpRequestMessage, int> _status = _ => 200;
    protected Dictionary<string, int> _calls;
    protected Func<HttpRequestMessage, Exception?> _failure = _ => null;

    protected static IReadOnlyList<ProbeTarget> Targets =>
    [
        new("studio", "https://app.cratis.studio/", [new("health", "GET", "/health", "application/json", 401), new("subscribe", "POST", "/.cratis/queries/sse/subscribe", "application/json", 401, "{}")]),
        new("direct", "https://app.cratis.direct", [new("health", "GET", "/health", "application/json", 200)])
    ];

    protected ProbeEvaluator Evaluator => new(new HttpClient(_handler), _time);

    protected static string Key(HttpRequestMessage request) => $"{request.RequestUri!.Host}{request.RequestUri.AbsolutePath}";

    void Establish()
    {
        _time = new ManualTimeProvider { AutoFireWithin = TimeSpan.FromMinutes(1) };
        _calls = [];
        _handler = new StubHttpHandler(request =>
        {
            _calls[Key(request)] = _calls.GetValueOrDefault(Key(request)) + 1;
            return _failure(request) is { } error ? throw error : new HttpResponseMessage((HttpStatusCode)_status(request));
        });
    }
}
#endif
