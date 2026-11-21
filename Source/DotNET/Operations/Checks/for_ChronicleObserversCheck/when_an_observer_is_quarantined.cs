// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Cluster;
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_ChronicleObserversCheck;

public class when_an_observer_is_quarantined : a_chronicle_observers_check
{
    void Establish() => _chronicle.ObserversOf[("studio", "globex")] = [new ChronicleObserver("inbox", 5), new ChronicleObserver("outbox", 1)];

    async Task Because() => await Run();

    [Fact] void should_report_only_the_quarantined_observer() => _findings.Select(finding => finding.Key).ShouldContainOnly("chronicle:studio/globex/inbox:ObserverQuarantined");
    [Fact] void should_say_how_to_clear_it() => _findings.Single().Text.ShouldContain("clear-quarantine");
}
#endif
