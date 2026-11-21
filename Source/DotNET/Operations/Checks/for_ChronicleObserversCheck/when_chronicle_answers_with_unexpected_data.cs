// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.given;

namespace Cratis.Showrunner.Operations.Checks.for_ChronicleObserversCheck;

public class when_chronicle_answers_with_unexpected_data : a_chronicle_observers_check
{
    void Establish() => _chronicle.MalformedNamespaces = true;

    async Task Because() => await Run();

    [Fact] void should_be_a_finding_and_not_a_pass() => Keys.ShouldContainOnly("chronicle:api");
    [Fact] void should_name_every_namespace_it_could_not_read() => _findings.Single().Text.ShouldContain("for 2 event store(s) or namespace(s)");
}
#endif
