// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_BackupPlanner.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_BackupPlanner;

public class when_a_prefix_is_configured : a_cluster
{
    protected override string Prefix => "prod-";

    void Because() => Plan();

    [Fact] void should_claim_the_prefixed_cluster_database() => Claim("prod-chronicle+main").Present.ShouldBeTrue();
    [Fact] void should_claim_the_prefixed_events_database() => Claim("prod-Studio+es+acme").Namespace.ShouldEqual("acme");
    [Fact] void should_claim_the_prefixed_read_models_database() => Claim("prod-Studio").Namespace.ShouldEqual("Default");
    [Fact] void should_leave_nothing_unclaimed() => _plan.Unclaimed.ShouldBeEmpty();
}
#endif
