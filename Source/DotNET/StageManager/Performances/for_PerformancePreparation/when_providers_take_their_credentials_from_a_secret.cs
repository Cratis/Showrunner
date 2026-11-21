// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformancePreparation;

public class when_providers_take_their_credentials_from_a_secret : a_preparation
{
    protected override Performance CreatePerformance()
    {
        var performance = PerformanceSamples.Performance(onboarding: false);
        performance.Spec.AuthProxy.Providers = [new() { Kind = AuthProxyProviderKind.GitHub, Identity = "GitHub", CredentialsSecretRef = new() { Name = "crm-github" } }];
        return performance;
    }

    protected override EnvironmentResolution CreateResolution() => EnvironmentResolution.Resolved(new("production", new Dictionary<string, string>(StringComparer.Ordinal)));

    void Establish()
    {
        _cluster.SecretValues["apps/crm-github/clientId"] = "fake-github-client-id";
        _cluster.SecretValues["apps/crm-github/clientSecret"] = PerformanceSamples.GitHubClientSecret;
    }

    Task Because() => Prepare();

    [Fact] void should_prepare_the_performance() => _prepared.ShouldNotBeNull();
    [Fact] void should_hand_over_the_client_id_as_the_environment_would_have() => _prepared!.Environment.Values["authentication.providers.GitHub.clientId"].ShouldEqual("fake-github-client-id");
    [Fact] void should_hand_over_the_client_secret_as_the_environment_would_have() => _prepared!.Environment.Values["authentication.providers.GitHub.clientSecret"].ShouldEqual(PerformanceSamples.GitHubClientSecret);
    [Fact] void should_record_the_credentials_as_resolved() => Condition("ProviderCredentialsResolved").Status.ShouldEqual("True");
    [Fact] void should_not_put_the_client_secret_in_the_status() => Conditions.Contains(PerformanceSamples.GitHubClientSecret, StringComparison.Ordinal).ShouldBeFalse();
}
#endif
