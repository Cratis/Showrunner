// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_BackupPlanner.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_BackupPlanner;

/// <summary>
/// A namespace that has no read models has no read models database yet. It is claimed, so it is not unclaimed, and there is nothing to dump.
/// </summary>
public class when_a_claimed_database_does_not_exist_yet : a_cluster
{
    void Establish() => _existing.Remove("Billing");

    void Because() => Plan();

    [Fact] void should_still_claim_it() => Claim("Billing").Namespace.ShouldEqual("Default");
    [Fact] void should_not_mark_it_present() => Claim("Billing").Present.ShouldBeFalse();
    [Fact] void should_not_report_it_unclaimed() => _plan.Unclaimed.ShouldBeEmpty();
}
#endif
