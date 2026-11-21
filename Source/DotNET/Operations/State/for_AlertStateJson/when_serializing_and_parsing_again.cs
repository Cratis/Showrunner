// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Text.Json.Nodes;

namespace Cratis.Showrunner.Operations.State.for_AlertStateJson;

public class when_serializing_and_parsing_again : Specification
{
    static readonly DateTimeOffset At = new(2026, 7, 1, 12, 0, 0, TimeSpan.Zero);

    AlertStateSnapshot _original;
    string _json;
    AlertStateSnapshot _parsed;

    void Establish() => _original = new AlertStateSnapshot(
        AlertStateSnapshot.Empty.LastAlerted.Add("a", At),
        AlertStateSnapshot.Empty.FirstSeen.Add("a", At.AddHours(-3)),
        At,
        AlertStateSnapshot.Empty.SummaryStreaks.Add("b", 1),
        ["job-1"],
        """{"x":1}""");

    void Because()
    {
        _json = AlertStateJson.Serialize(_original);
        _parsed = AlertStateJson.Parse(_json);
    }

    [Fact] void should_keep_the_alert_times() => _parsed.LastAlerted["a"].ShouldEqual(At);
    [Fact] void should_keep_the_first_seen_times() => _parsed.FirstSeen["a"].ShouldEqual(At.AddHours(-3));
    [Fact] void should_keep_the_last_check() => _parsed.LastCheck.ShouldEqual(At);
    [Fact] void should_keep_the_streaks() => _parsed.SummaryStreaks["b"].ShouldEqual(1);
    [Fact] void should_keep_the_reported_jobs() => _parsed.ReportedFailedJobs.ShouldContainOnly("job-1");
    [Fact] void should_keep_the_actions_audit() => _parsed.ActionsAudit.ShouldEqual("""{"x":1}""");
    [Fact] void should_write_the_finding_as_epoch_seconds_at_the_top_level_like_the_python_did() => ((double)JsonNode.Parse(_json)!["a"]!).ShouldEqual(At.ToUnixTimeSeconds());
}
#endif
