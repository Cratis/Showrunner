// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.given;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Showrunner.Operations.Probe.for_ProbeRunner;

public class when_running_the_probe : a_probe
{
    RecordingChannel _channel;
    StubHttpHandler _pings;
    readonly List<string> _log = [];
    (int ExitCode, string Summary) _down;
    (int ExitCode, string Summary) _healthy;
    int _alertsWhenHealthy;

    ProbeRunner Runner(StubHttpHandler? sinks = null, bool failingChannel = false)
    {
        _channel = new RecordingChannel { Fail = failingChannel };
        _pings = sinks ?? StubHttpHandler.Answering(HttpStatusCode.OK);
        return new ProbeRunner(
            Evaluator,
            new AlertDelivery([_channel], NullLogger<AlertDelivery>.Instance),
            new HealthchecksProbeSink(new HttpClient(_pings), new Uri("https://hc.example/uuid")),
            null,
            "cratis",
            _log.Add);
    }

    async Task Because()
    {
        _status = request => request.RequestUri!.Host == "app.cratis.direct" ? 503 : 401;
        _down = await Runner(failingChannel: true).Run(Targets, TimeSpan.Zero, CancellationToken.None);

        _status = request => request.RequestUri!.Host == "app.cratis.direct" ? 200 : 401;
        _log.Clear();
        _healthy = await Runner().Run(Targets, TimeSpan.Zero, CancellationToken.None);
        _alertsWhenHealthy = _channel.Delivered.Count;
    }

    [Fact] void should_exit_with_failure_when_something_is_down() => _down.ExitCode.ShouldEqual(1);
    [Fact] void should_exit_cleanly_when_everything_answers() => _healthy.ExitCode.ShouldEqual(0);
    [Fact] void should_write_a_markdown_summary() => _down.Summary.ShouldContain("## Public endpoint probe: down");
    [Fact] void should_list_every_request_in_the_summary() => _down.Summary.ShouldContain("| direct/health | 503 | failed |");
    [Fact] void should_not_alert_when_everything_answers() => _alertsWhenHealthy.ShouldEqual(0);
    [Fact] void should_say_ok_in_the_summary() => _healthy.Summary.ShouldContain("## Public endpoint probe: ok");
    [Fact] void should_log_only_which_sinks_delivered() => _log.ShouldContain("healthchecks: delivered");
    [Fact] void should_say_a_sink_is_disabled() => _log.ShouldContain("issues: disabled");
}
#endif
