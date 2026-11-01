// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageOperator.Configuration;
using Cratis.Showrunner.StageOperator.given;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.for_ReconcilePlanner;

public class when_the_lifetime_ends_later : Specification
{
    ReconcilePlan _result = null!;

    void Because() => _result = ReconcilePlanner.Plan(Applications.Context(
        Applications.Create(version: "1.0.0", image: Applications.OldImage, status: Applications.Running(), permanent: false, until: Applications.Now.AddHours(2)),
        Applications.Ready(Applications.OldImage)));

    [Fact] void should_keep_running() => Assert.Equal(StageApplicationPhases.Running, _result.Status.Phase);
    [Fact] void should_report_when_it_expires() => Assert.Equal(Rfc3339.Format(Applications.Now.AddHours(2)), _result.Status.ExpiresAt);
    [Fact] void should_look_again_at_the_expiry() => Assert.Equal(TimeSpan.FromHours(2), _result.RequeueAfter);
}
#endif
