// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformancePreparation;

public class when_the_environment_lacks_keys : a_preparation
{
    protected override EnvironmentResolution CreateResolution() => EnvironmentResolution.Resolved(new("production", PerformanceSamples.Environment().Values
        .Where(pair => pair.Key != "authentication.providers.EntraId.clientSecret" && pair.Key != "ante.mongodb.server")
        .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal)));

    Task Because() => Prepare();

    [Fact] void should_not_prepare_the_performance() => _prepared.ShouldBeNull();
    [Fact] void should_be_blocked() => _status.Phase.ShouldEqual(PerformancePhase.Blocked);
    [Fact] void should_record_the_incomplete_environment_last() => Conditions.ShouldEqual("SpecValid=True/Valid | ChronicleConnection=True/Resolved | EnvironmentResolved=True/Resolved | EnvironmentComplete=False/MissingEnvironmentKeys");
    [Fact] void should_count_and_name_the_missing_keys() => Condition("EnvironmentComplete").Message.ShouldEqual("The environment 'production' is missing 2 key(s): authentication.providers.EntraId.clientSecret, ante.mongodb.server.");
    [Fact] void should_report_the_missing_keys_in_the_status() => _performance.Status.MissingEnvironmentKeys.Count.ShouldEqual(2);
}
#endif
