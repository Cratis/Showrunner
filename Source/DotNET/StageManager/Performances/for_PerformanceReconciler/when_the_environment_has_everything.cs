// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformanceReconciler;

public class when_the_environment_has_everything : a_reconciliation
{
    string[] ConditionTypes => [.. _performance.Status.Conditions.Select(condition => condition.Type)];

    [Fact] void should_be_progressing_until_the_deployments_are_ready() => _phase.ShouldEqual(PerformancePhase.Progressing);
    [Fact] void should_apply_the_application_authproxy_and_ante() => string.Join(',', _cluster.AppliedOf<V1Deployment>().Select(deployment => deployment.Metadata.Name).Order(StringComparer.Ordinal)).ShouldEqual("crm-ante,crm-app,crm-authproxy");
    [Fact] void should_apply_one_ingress() => _cluster.AppliedOf<V1Ingress>().Count().ShouldEqual(1);
    [Fact] void should_keep_the_signing_keys_in_a_secret() => _cluster.AppliedOf<V1Secret>().Any(secret => secret.Metadata.Name == "crm-signing-keys").ShouldBeTrue();
    [Fact] void should_report_the_public_url() => _performance.Status.PublicUrl.ShouldEqual("https://crm.example.com");
    [Fact] void should_report_no_missing_keys() => _performance.Status.MissingEnvironmentKeys.Count.ShouldEqual(0);
    [Fact] void should_write_the_status() => _cluster.StatusWritten.ShouldBeSame(_performance);
    [Fact] void should_report_the_application_condition() => ConditionTypes.Contains("ApplicationReady").ShouldBeTrue();
    [Fact] void should_report_the_authproxy_condition() => ConditionTypes.Contains("AuthProxyReady").ShouldBeTrue();
    [Fact] void should_report_the_ante_condition() => ConditionTypes.Contains("AnteReady").ShouldBeTrue();
    [Fact] void should_not_put_a_secret_value_in_the_status() => PerformanceSamples.SecretValues.Any(StatusAsJson.Contains).ShouldBeFalse();
}
#endif
