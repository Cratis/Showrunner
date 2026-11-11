// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Productions.given;

namespace Cratis.Showrunner.StageManager.Productions.for_ProductionReconciler;

public class when_the_keystore_password_and_a_partial_credential_set_exist : a_production_reconciliation
{
    void Establish()
    {
        _cluster.SecretValues[$"apps/ledger-chronicle-keystore/{ProductionCertificates.KeystorePasswordKey}"] = "existing-keystore-password";
        _cluster.SecretValues[$"apps/ledger-chronicle-credentials/{ChronicleCredentials.ClientIdKey}"] = "existing-client-id";
    }

    Task Because() => Reconcile();

    [Fact] void should_not_replace_the_keystore_password() => _cluster.AppliedNamed<V1Secret>("ledger-chronicle-keystore").ShouldBeNull();
    [Fact] void should_generate_a_new_credential_set_because_it_is_incomplete() => _cluster.AppliedNamed<V1Secret>("ledger-chronicle-credentials").ShouldNotBeNull();
    [Fact] void should_not_keep_the_old_client_id() => _cluster.AppliedSecretValue("ledger-chronicle-credentials", ChronicleCredentials.ClientIdKey).ShouldNotEqual("existing-client-id");
}
#endif
