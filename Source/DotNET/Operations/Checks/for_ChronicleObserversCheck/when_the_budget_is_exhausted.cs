// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_ChronicleObserversCheck;

public class when_the_budget_is_exhausted : a_chronicle_observers_check
{
    async Task Because() => await Run(budget: TimeSpan.Zero);

    [Fact] void should_be_a_finding_and_not_a_pass() => _findings.Select(finding => finding.Key).ShouldContainOnly("chronicle:api");
    [Fact] void should_say_the_check_did_not_finish() => _findings.Single().Text.ShouldContain("not finished within 45s");
    [Fact] void should_not_ask_chronicle_for_anything() => _chronicle.ListedStores.ShouldEqual(0);
}
#endif
