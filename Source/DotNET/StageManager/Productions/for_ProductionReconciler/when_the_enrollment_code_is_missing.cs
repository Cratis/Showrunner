// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_enrollment_code_is_missing : a_production_reconciliation
{
    protected override ProductionSpec CreateSpec()
    {
        var spec = ProductionSamples.External();
        spec.Federation.Dial = new() { Enabled = true, EventStore = "ledger", EnrollmentCodeSecretRef = new() { Name = "direct-enrollment" } };
        return spec;
    }

    Task Because() => Reconcile();

    [Fact] void should_be_blocked() => _phase.ShouldEqual(ProductionPhase.Blocked);
    [Fact] void should_apply_nothing() => _cluster.Applied.ShouldBeEmpty();
    [Fact] void should_name_the_secret_and_the_key() => Condition(ProductionConditions.FederationConfigured).Message.ShouldContain("'direct-enrollment' does not exist or has no key 'code'");
}
#endif
