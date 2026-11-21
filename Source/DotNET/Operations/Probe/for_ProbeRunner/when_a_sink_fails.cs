// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Showrunner.Operations.Probe.for_ProbeRunner;

public class when_a_sink_fails : a_probe
{
    RecordingChannel _channel;
    readonly List<string> _log = [];
    (int ExitCode, string Summary) _outcome;

    async Task Because()
    {
        _status = request => request.RequestUri!.Host == "app.cratis.direct" ? 503 : 401;
        _channel = new RecordingChannel();
        var runner = new ProbeRunner(
            Evaluator,
            new AlertDelivery([_channel], NullLogger<AlertDelivery>.Instance),
            new HealthchecksProbeSink(new HttpClient(StubHttpHandler.Answering(HttpStatusCode.InternalServerError)), new Uri("https://hc.example/uuid-capability")),
            null,
            "cratis",
            _log.Add);
        _outcome = await runner.Run(Targets, TimeSpan.Zero, CancellationToken.None);
    }

    [Fact] void should_still_report_the_outage() => _outcome.ExitCode.ShouldEqual(1);
    [Fact] void should_still_alert_the_shared_channels() => _channel.Delivered.Single().Raised.Select(finding => finding.Key).ShouldContainOnly("probe:direct/health");
    [Fact] void should_say_which_sink_failed() => _log.ShouldContain("healthchecks: failed");
    [Fact] void should_never_log_the_ping_address() => _log.Any(line => line.Contains("uuid-capability")).ShouldBeFalse();
    [Fact] void should_say_the_alerts_were_delivered() => _log.ShouldContain("alerts: delivered");
}
#endif
