// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_ChronicleObserversCheck;

public class when_the_state_is_read_twice_in_one_run : a_chronicle_observers_check
{
    IReadOnlyCollection<Alerts.Finding> _first;
    IReadOnlyCollection<Alerts.Finding> _second;

    void Establish() => _chronicle.Stores["studio"]["acme"] = [Partition("sender", 60, quarantined: true)];

    async Task Because()
    {
        await Run();
        _first = _findings;
        await Run();
        _second = _findings;
    }

    [Fact] void should_answer_both_passes_the_same() => _second.ShouldEqual(_first);
    [Fact] void should_ask_chronicle_only_once() => _chronicle.ListedStores.ShouldEqual(1);
}
#endif
