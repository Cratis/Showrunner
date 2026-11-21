// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_ChronicleObserversCheck;

public class when_a_namespace_cannot_be_read : a_chronicle_observers_check
{
    void Establish()
    {
        _chronicle.Stores["studio"]["acme"] = [Partition("sender", 60, quarantined: true)];
        _chronicle.Broken.Add(("studio", "globex"));
    }

    async Task Because() => await Run();

    [Fact] void should_not_hide_the_namespaces_that_could_be_read() => Keys.ShouldContain("chronicle:studio/acme/sender:PartitionsQuarantined");
    [Fact] void should_report_that_chronicle_could_not_be_read() => Keys.ShouldContain("chronicle:api");
    [Fact] void should_name_the_namespace_and_the_reason() => _findings.Single(finding => finding.Key == "chronicle:api").Text.ShouldContain("studio/globex: Event store does not exist.");
    [Fact] void should_count_what_could_not_be_read() => _findings.Single(finding => finding.Key == "chronicle:api").Text.ShouldContain("for 1 event store(s) or namespace(s)");
}
#endif
