// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_ChronicleObserversCheck;

public class when_the_credentials_are_rejected : a_chronicle_observers_check
{
    IReadOnlyCollection<Alerts.Finding> _rejected;

    async Task Because()
    {
        _chronicle.RejectCredentials = true;
        await Run();
        _rejected = _findings;
        _chronicle.RejectCredentials = false;
        await Run();
    }

    [Fact] void should_be_a_finding_and_not_a_pass() => _rejected.Select(finding => finding.Key).ShouldContainOnly("chronicle:api");
    [Fact] void should_say_what_could_not_be_read() => _rejected.Single().Text.ShouldContain("could not read Chronicle's observer state");
    [Fact] void should_read_again_on_the_recheck_so_a_blip_does_not_survive() => _findings.ShouldBeEmpty();
}
#endif
