// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Reconciles <see cref="Performance"/> resources. Everything a Performance owns carries an owner reference, so deleting it removes them.
/// </summary>
/// <param name="reconciler">The reconciler.</param>
/// <param name="time">The time provider.</param>
/// <param name="logger">The logger.</param>
public class PerformanceController(PerformanceReconciler reconciler, TimeProvider time, ILogger<PerformanceController> logger) : IEntityController<Performance>
{
    static readonly TimeSpan _whileProgressing = TimeSpan.FromSeconds(15);
    static readonly TimeSpan _whileBlocked = TimeSpan.FromMinutes(1);
    static readonly TimeSpan _whenSettled = TimeSpan.FromMinutes(10);
    static readonly TimeSpan _whileMeasuring = TimeSpan.FromSeconds(30);

    /// <inheritdoc/>
    public async Task<ReconciliationResult<Performance>> ReconcileAsync(Performance entity, CancellationToken cancellationToken)
    {
        try
        {
            var phase = await reconciler.ReconcileAsync(entity, cancellationToken);
            var requeue = phase switch
            {
                PerformancePhase.Ready => entity.Spec.Application.Published is null ? _whenSettled : _whileMeasuring,
                PerformancePhase.Expired => _whenSettled,
                PerformancePhase.Blocked => _whileBlocked,
                _ => _whileProgressing
            };

            // A lifetime that ends before the next round is met on time.
            if (phase != PerformancePhase.Expired && LifetimeClock.ExpiresAt(entity.Spec) is { } expiresAt)
            {
                var untilExpiry = expiresAt - time.GetUtcNow() + TimeSpan.FromSeconds(1);
                requeue = untilExpiry < requeue ? TimeSpan.FromTicks(Math.Max(untilExpiry.Ticks, TimeSpan.FromSeconds(1).Ticks)) : requeue;
            }

            return ReconciliationResult<Performance>.Success(entity, requeue);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // The exception goes to the log by type and stack only; messages can echo what a provider said.
            logger.ReconciliationFailed(entity.Metadata.NamespaceProperty, entity.Metadata.Name, exception);
            return ReconciliationResult<Performance>.Failure(entity, "Reconciliation failed. See the operator log.", exception, _whileBlocked);
        }
    }

    /// <inheritdoc/>
    public Task<ReconciliationResult<Performance>> DeletedAsync(Performance entity, CancellationToken cancellationToken) =>
        Task.FromResult(ReconciliationResult<Performance>.Success(entity));
}
