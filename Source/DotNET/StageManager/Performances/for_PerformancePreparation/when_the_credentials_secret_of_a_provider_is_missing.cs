// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformancePreparation;

public class when_the_credentials_secret_of_a_provider_is_missing : a_preparation
{
    protected override Performance CreatePerformance()
    {
        var performance = PerformanceSamples.Performance(onboarding: false);
        performance.Spec.AuthProxy.Providers = [new() { Kind = AuthProxyProviderKind.GitHub, Identity = "GitHub", CredentialsSecretRef = new() { Name = "crm-github" } }];
        return performance;
    }

    protected override EnvironmentResolution CreateResolution() => EnvironmentResolution.Resolved(new("production", new Dictionary<string, string>(StringComparer.Ordinal)));

    Task Because() => Prepare();

    [Fact] void should_not_prepare_the_performance() => _prepared.ShouldBeNull();
    [Fact] void should_be_blocked() => _status.Phase.ShouldEqual(PerformancePhase.Blocked);
    [Fact] void should_record_why() => Condition("ProviderCredentialsResolved").Reason.ShouldEqual("ProviderCredentialsNotFound");
    [Fact] void should_name_the_secret_and_the_provider() => Condition("ProviderCredentialsResolved").Message.ShouldEqual("The Secret 'crm-github' of the provider 'GitHub' does not exist or lacks the key 'clientId' or 'clientSecret'.");
}
#endif
