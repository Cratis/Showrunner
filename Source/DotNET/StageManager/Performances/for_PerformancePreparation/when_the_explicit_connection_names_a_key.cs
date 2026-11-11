// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Performances.given;

namespace Cratis.Showrunner.StageManager.Performances.for_PerformancePreparation;

public class when_the_explicit_connection_names_a_key : a_preparation
{
    void Establish()
    {
        _performance.Spec.Chronicle!.ConnectionSecretRef.Key = "dsn";
        _cluster.SecretValues["apps/chronicle-connection/dsn"] = PerformanceSamples.ConnectionString;
    }

    Task Because() => Prepare();

    [Fact] void should_prepare_the_performance() => _prepared.ShouldNotBeNull();
    [Fact] void should_use_the_named_key() => _prepared!.Connection!.Key.ShouldEqual("dsn");
}
#endif
