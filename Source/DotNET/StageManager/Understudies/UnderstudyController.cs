// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies;

/// <summary>
/// Reconciles <see cref="Understudy"/> resources. Everything an Understudy creates carries an owner reference, so deleting it removes the CronJobs and Jobs;
/// the backups in object storage stay.
/// </summary>
/// <param name="reconciler">The reconciler.</param>
/// <param name="logger">The logger.</param>
public class UnderstudyController(UnderstudyReconciler reconciler, ILogger<UnderstudyController> logger) : IEntityController<Understudy>
{
    static readonly TimeSpan _whileBlocked = TimeSpan.FromMinutes(1);
    static readonly TimeSpan _whenScheduled = TimeSpan.FromMinutes(5);

    /// <inheritdoc/>
    public async Task<ReconciliationResult<Understudy>> ReconcileAsync(Understudy entity, CancellationToken cancellationToken)
    {
        try
        {
            var phase = await reconciler.ReconcileAsync(entity, cancellationToken);
            return ReconciliationResult<Understudy>.Success(entity, phase == UnderstudyPhase.Blocked ? _whileBlocked : _whenScheduled);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // The exception goes to the log by type and stack only; messages can echo what the API said.
            logger.ReconciliationFailed(entity.Metadata.NamespaceProperty, entity.Metadata.Name, exception);
            return ReconciliationResult<Understudy>.Failure(entity, "Reconciliation failed. See the operator log.", exception, _whileBlocked);
        }
    }

    /// <inheritdoc/>
    public Task<ReconciliationResult<Understudy>> DeletedAsync(Understudy entity, CancellationToken cancellationToken) =>
        Task.FromResult(ReconciliationResult<Understudy>.Success(entity));
}
