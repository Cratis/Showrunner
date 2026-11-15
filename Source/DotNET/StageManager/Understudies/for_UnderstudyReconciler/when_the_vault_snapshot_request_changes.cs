// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler.given;
using Cratis.Showrunner.StageManager.Understudies.given;
using Cratis.Showrunner.StageManager.Understudies.Jobs;
using Cratis.Showrunner.StageManager.Understudies.Storage;

namespace Cratis.Showrunner.StageManager.Understudies.for_UnderstudyReconciler;

public class when_the_vault_snapshot_request_changes : an_understudy_reconciliation
{
    protected override bool Vault => true;

    protected override UnderstudySpec CreateSpec()
    {
        var spec = UnderstudySamples.Spec();
        spec.Keys = UnderstudySamples.Keys();
        return spec;
    }

    async Task Because()
    {
        _understudy.Metadata.Annotations = new Dictionary<string, string> { [UnderstudyMetadata.VaultSnapshotAnnotation] = "first" };
        await Reconcile();
        _understudy.Metadata.Annotations[UnderstudyMetadata.VaultSnapshotAnnotation] = "second";
        await Reconcile();
    }

    [Fact] void should_take_a_new_snapshot() => _cluster.Created.OfType<V1Job>().Count().ShouldEqual(2);
    [Fact] void should_name_each_job_after_its_request() => _cluster.Created.OfType<V1Job>().Select(job => job.Metadata.Name).Distinct().Count().ShouldEqual(2);
}
#endif
