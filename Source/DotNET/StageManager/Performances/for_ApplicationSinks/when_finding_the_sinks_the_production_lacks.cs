// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Performances.for_ApplicationSinks;

public class when_finding_the_sinks_the_production_lacks : Specification
{
    IReadOnlyList<string> _used;
    IReadOnlyList<string> _missing;

    void Because()
    {
        _used = ApplicationSinks.UsedBy(new() { Sinks = ["Reporting", "audit", "reporting"] }, ["people", "ledger"]);
        _missing = ApplicationSinks.MissingFrom(_used, ProductionSamples.WithSinks());
    }

    [Fact] void should_use_each_name_once_without_regard_to_case() => _used.ShouldContainOnly(["Reporting", "audit", "people", "ledger"]);
    [Fact] void should_name_the_sinks_the_production_does_not_configure() => _missing.ShouldContainOnly(["audit", "ledger"]);
}
#endif
