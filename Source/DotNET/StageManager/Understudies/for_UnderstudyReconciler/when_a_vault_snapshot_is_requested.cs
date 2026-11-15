// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Jobs;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler;

public class when_a_vault_snapshot_is_requested : an_understudy_reconciliation
{
    protected override bool Vault => true;

    protected override UnderstudySpec CreateSpec()
    {
        var spec = UnderstudySamples.Spec();
        spec.Keys = UnderstudySamples.Keys();
        return spec;
    }

    void Establish() => _understudy.Metadata.Annotations = new Dictionary<string, string> { [UnderstudyMetadata.VaultSnapshotAnnotation] = "2026-03-15T12:00" };

    async Task Because()
    {
        await Reconcile();
        await Reconcile();
    }

    [Fact] void should_create_one_job() => _cluster.Created.OfType<V1Job>().Count().ShouldEqual(1);
    [Fact] void should_start_the_snapshot_job() => _cluster.Created.OfType<V1Job>().Single().Spec.Template.Spec.Containers.Single().Args.ShouldContain("vault-snapshot");
    [Fact] void should_own_the_job() => _cluster.Created.OfType<V1Job>().Single().Metadata.OwnerReferences.Single().Name.ShouldEqual("ledger-backups");
    [Fact] void should_remember_the_request() => _understudy.Status.LastVaultSnapshotRequest.ShouldEqual("2026-03-15T12:00");
}
#endif
