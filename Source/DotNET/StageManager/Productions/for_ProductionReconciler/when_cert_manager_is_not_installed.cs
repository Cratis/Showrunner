// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_cert_manager_is_not_installed : a_production_reconciliation
{
    void Establish() => _cluster.Definitions.Remove(CertManagerCertificate.DefinitionName);

    Task Because() => Reconcile();

    [Fact] void should_be_blocked() => _phase.ShouldEqual(ProductionPhase.Blocked);
    [Fact] void should_apply_nothing() => _cluster.Applied.ShouldBeEmpty();
    [Fact] void should_say_cert_manager_is_missing() => Condition(ProductionConditions.CertManagerInstalled).Reason.ShouldEqual("CertManagerNotInstalled");
}
#endif
