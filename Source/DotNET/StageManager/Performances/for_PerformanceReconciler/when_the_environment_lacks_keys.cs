// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_the_environment_lacks_keys : a_reconciliation
{
    protected override ResolvedEnvironment CreateEnvironment() => new("production", PerformanceSamples.Environment().Values
        .Where(pair => pair.Key != "authentication.providers.EntraId.clientSecret" && pair.Key != "ante.mongodb.server")
        .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal));

    [Fact] void should_be_blocked() => _phase.ShouldEqual(PerformancePhase.Blocked);
    [Fact] void should_apply_nothing() => _cluster.Applied.Count.ShouldEqual(0);
    [Fact] void should_report_the_missing_client_secret() => _performance.Status.MissingEnvironmentKeys.Contains("authentication.providers.EntraId.clientSecret").ShouldBeTrue();
    [Fact] void should_report_the_missing_mongodb_server() => _performance.Status.MissingEnvironmentKeys.Contains("ante.mongodb.server").ShouldBeTrue();
    [Fact] void should_still_write_the_status() => _cluster.StatusWritten.ShouldBeSame(_performance);
    [Fact] void should_not_put_a_secret_value_in_the_status() => PerformanceSamples.SecretValues.Any(StatusAsJson.Contains).ShouldBeFalse();
}
#endif
