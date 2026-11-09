// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_certificate_is_not_issued_yet : a_production_reconciliation
{
    void Establish()
    {
        _cluster.SecretValues.Remove("apps/ledger-chronicle-tls/keystore.p12");
        _cluster.SecretValues.Remove("apps/ledger-chronicle-tls/ca.crt");
    }

    Task Because() => Reconcile();

    [Fact] void should_be_progressing() => _phase.ShouldEqual(ProductionPhase.Progressing);
    [Fact] void should_request_the_certificates() => _cluster.AppliedOf<CertManagerCertificate>().Count().ShouldEqual(2);
    [Fact] void should_not_run_chronicle_yet() => Chronicle.ShouldBeNull();
    [Fact] void should_not_write_the_connection_secret_yet() => _cluster.AppliedNamed<V1Secret>("ledger-chronicle-connection").ShouldBeNull();
    [Fact] void should_report_no_connection_secret() => _production.Status.ConnectionSecretName.ShouldEqual(string.Empty);
    [Fact] void should_say_the_certificate_is_not_issued() => Condition(ProductionConditions.CertificatesReady).Reason.ShouldEqual("NotIssued");
}
#endif
