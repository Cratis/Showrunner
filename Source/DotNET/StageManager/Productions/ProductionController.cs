// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Productions;

/// <summary>
/// Reconciles <see cref="Production"/> resources. Everything a Production creates carries an owner reference, so deleting it removes them; the volumes of its managed database and graph stay.
/// </summary>
/// <param name="reconciler">The reconciler.</param>
/// <param name="logger">The logger.</param>
public class ProductionController(ProductionReconciler reconciler, ILogger<ProductionController> logger) : IEntityController<Production>
{
    static readonly TimeSpan _whileProgressing = TimeSpan.FromSeconds(15);
    static readonly TimeSpan _whileBlocked = TimeSpan.FromMinutes(1);
    static readonly TimeSpan _whenReady = TimeSpan.FromMinutes(5);

    /// <inheritdoc/>
    public async Task<ReconciliationResult<Production>> ReconcileAsync(Production entity, CancellationToken cancellationToken)
    {
        try
        {
            var phase = await reconciler.ReconcileAsync(entity, cancellationToken);
            var requeue = phase switch
            {
                ProductionPhase.Ready => _whenReady,
                ProductionPhase.Blocked => _whileBlocked,
                _ => _whileProgressing
            };
            return ReconciliationResult<Production>.Success(entity, requeue);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // The exception goes to the log by type and stack only; messages can echo what the API said.
            logger.ReconciliationFailed(entity.Metadata.NamespaceProperty, entity.Metadata.Name, exception);
            return ReconciliationResult<Production>.Failure(entity, "Reconciliation failed. See the operator log.", exception, _whileBlocked);
        }
    }

    /// <inheritdoc/>
    public Task<ReconciliationResult<Production>> DeletedAsync(Production entity, CancellationToken cancellationToken) =>
        Task.FromResult(ReconciliationResult<Production>.Success(entity));
}
