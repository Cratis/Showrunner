// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Probe.for_ProbeEvaluator;

public class when_an_endpoint_never_answers : Specification
{
    sealed class Hanging : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return new HttpResponseMessage();
        }
    }

    ProbeResult _result;
    Exception _cancelled;

    async Task Because()
    {
        var evaluator = new ProbeEvaluator(new HttpClient(new Hanging()), TimeProvider.System);
        var target = new ProbeTarget("slow", "https://slow.example", []);
        _result = await evaluator.Check(target, new ProbeEndpoint("sse", "GET", "/stream", "text/event-stream", 401, TimeoutMs: 50), CancellationToken.None);
        using var cancel = new CancellationTokenSource();
        await cancel.CancelAsync();
        _cancelled = await Record.ExceptionAsync(() => evaluator.Check(target, new ProbeEndpoint("sse", "GET", "/stream", "text/event-stream", 401), cancel.Token));
    }

    [Fact] void should_give_up_at_the_checks_own_timeout() => _result.Error.ShouldEqual("TimeoutError");
    [Fact] void should_not_be_ok() => _result.Ok.ShouldBeFalse();
    [Fact] void should_describe_the_failure_without_an_address() => _result.Description.ShouldEqual("slow/sse: TimeoutError");
    [Fact] void should_let_a_real_cancellation_through() => (_cancelled is OperationCanceledException).ShouldBeTrue();
}
#endif
