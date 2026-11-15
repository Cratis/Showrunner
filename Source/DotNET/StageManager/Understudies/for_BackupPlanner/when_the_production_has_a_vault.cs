// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_BackupPlanner.given;

namespace Cratis.Showrunner.StageManager.Understudies.for_BackupPlanner;

public class when_the_production_has_a_vault : a_cluster
{
    protected override bool HasVault => true;

    void Because() => Plan();

    [Fact] void should_plan_the_keys_of_every_namespace() => _plan.Keys.Select(keys => keys.Path).ShouldContainOnly(["Studio/Default", "Studio/acme", "Billing/Default"]);
}
#endif
