// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Net;
using Cratis.Showrunner.Operations.Configuration;
using Cratis.Showrunner.Operations.given;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Showrunner.Operations.Watchdog.for_WatchdogCommands;

public class when_running_the_external_probe : Specification
{
    const string Targets = "[{\"name\":\"direct\",\"baseUrl\":\"https://app.cratis.direct\",\"checks\":[{\"id\":\"health\",\"path\":\"/health\",\"expectStatus\":200}]}]";

    readonly List<string> _log = [];
    string _summaryFile;
    StubHttpHandler _public;
    StubHttpHandler _sinks;
    int _down;
    int _up;
    string _summary;

    WatchdogFactory Factory(HttpStatusCode status)
    {
        _public = StubHttpHandler.Answering(status);
        _sinks = StubHttpHandler.Answering(HttpStatusCode.OK);
        return new WatchdogFactory(
            WatchdogSettingsReader.FromEnvironment(
                new Dictionary<string, string>
                {
                    ["PROBE_TARGETS"] = Targets,
                    ["PROBE_ROUND_GAP_SECONDS"] = "0",
                    ["PROBE_PING_URL"] = "https://hc-ping.example/uuid",
                    ["DISCORD_WEBHOOK_URL"] = "https://discord.example.com/api/webhooks/1/x",
                    ["WATCHDOG_CHECKS"] = "probe"
                },
                "operations"),
            new FakeClusterReader(),
            new FakeConfigMapStore(),
            new HttpClient(_sinks),
            null,
            new ManualTimeProvider { AutoFireWithin = TimeSpan.FromMinutes(1) },
            NullLoggerFactory.Instance,
            probeHttp: new HttpClient(_public));
    }

    async Task Because()
    {
        _summaryFile = Path.Combine(Path.GetTempPath(), $"probe-summary-{Guid.NewGuid():N}.md");
        try
        {
            _down = await WatchdogCommands.Run(WatchdogCommand.ProbeExternal, Factory(HttpStatusCode.ServiceUnavailable), CancellationToken.None, _log.Add, _summaryFile);
            _summary = await File.ReadAllTextAsync(_summaryFile);
            _up = await WatchdogCommands.Run(WatchdogCommand.ProbeExternal, Factory(HttpStatusCode.OK), CancellationToken.None, _log.Add);
        }
        finally
        {
            File.Delete(_summaryFile);
        }
    }

    [Fact] void should_exit_with_failure_when_a_public_endpoint_is_down() => _down.ShouldEqual(1);
    [Fact] void should_exit_cleanly_when_every_endpoint_answers() => _up.ShouldEqual(0);
    [Fact] void should_append_the_markdown_summary_for_the_ci_job() => _summary.ShouldContain("## Public endpoint probe: down");
    [Fact] void should_call_the_public_endpoint_with_the_probe_client() => _public.Requests.All(request => request.Uri.Host == "app.cratis.direct").ShouldBeTrue();
    [Fact] void should_say_the_healthchecks_ping_was_delivered() => _log.ShouldContain("healthchecks: delivered");
}
#endif
