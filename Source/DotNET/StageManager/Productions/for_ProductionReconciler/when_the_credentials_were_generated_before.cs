// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_credentials_were_generated_before : a_production_reconciliation
{
    void Establish()
    {
        _cluster.SecretValues["apps/ledger-chronicle-credentials/clientId"] = "showrunner-applications";
        _cluster.SecretValues["apps/ledger-chronicle-credentials/clientSecret"] = "kept-client-secret";
        _cluster.SecretValues["apps/ledger-chronicle-credentials/adminPassword"] = "kept-admin-password";
    }

    Task Because() => Reconcile();

    [Fact] void should_not_generate_new_credentials() => _cluster.AppliedNamed<V1Secret>("ledger-chronicle-credentials").ShouldBeNull();
    [Fact] void should_connect_with_the_kept_secret() => _cluster.AppliedSecretValue("ledger-chronicle-connection", "connectionString")!.ShouldContain(":kept-client-secret@");
}
#endif
