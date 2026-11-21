// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_ChronicleObserversCheck;

public class when_chronicle_is_slow : a_chronicle_observers_check
{
    int _requests;
    int _requestsInTheBudget;

    void Establish()
    {
        _chronicle.Stores["studio"] = Enumerable.Range(0, 30).ToDictionary(index => $"tenant{index}", _ => new List<ChronicleFailedPartition>());
        _chronicle.OnRequest = () =>
        {
            _requests++;
            _time.Advance(TimeSpan.FromSeconds(10));
        };
    }

    async Task Because()
    {
        await Run();
        _requestsInTheBudget = _requests;
    }

    [Fact] void should_stop_asking_when_the_budget_is_spent() => (_requestsInTheBudget < 15).ShouldBeTrue();
    [Fact] void should_say_it_did_not_finish() => _findings.Single(finding => finding.Key == "chronicle:api").Text.ShouldContain("not finished within 45s");
    [Fact] void should_count_the_requests_the_budget_covers_from_the_very_first() => _chronicle.Timeouts.Count.ShouldEqual(_requestsInTheBudget);
}
#endif
