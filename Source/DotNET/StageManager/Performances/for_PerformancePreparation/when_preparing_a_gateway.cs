// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformancePreparation;

public class when_preparing_a_gateway : a_preparation
{
    protected override Performance CreatePerformance() => ProxySamples.Gateway();

    void Establish()
    {
        _cluster.SecretValues["apps/console-github/clientId"] = "fake-console-client-id";
        _cluster.SecretValues["apps/console-github/clientSecret"] = ProxySamples.GitHubClientSecret;
    }

    Task Because() => Prepare();

    [Fact] void should_prepare_the_performance() => _prepared.ShouldNotBeNull();
    [Fact] void should_connect_to_no_chronicle() => _prepared!.Connection.ShouldBeNull();
    [Fact] void should_not_read_an_environment_file() => _resolver.Called.ShouldBeFalse();
    [Fact] void should_hand_over_the_client_secret_of_the_provider() => _prepared!.Environment.Values["authentication.providers.GitHub.clientSecret"].ShouldEqual(ProxySamples.GitHubClientSecret);
    [Fact] void should_record_only_the_conditions_that_apply() => Conditions.ShouldEqual("SpecValid=True/Valid | EnvironmentResolved=True/NotNeeded | ProviderCredentialsResolved=True/Resolved | EnvironmentComplete=True/Complete");
}
#endif
