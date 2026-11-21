// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// A remedy that restarts one Deployment the watchdog is explicitly configured and RBAC'd to manage. It cannot delete anything,
/// scale, or touch another Deployment, and a restart that is repeated only restarts it again.
/// </summary>
/// <param name="binding">The Deployment the remedy restarts.</param>
/// <param name="deployments">The Kubernetes access.</param>
public sealed class DeploymentRestartRemedy(RemedyBinding binding, IDeployments deployments) : IRemedy
{
    /// <inheritdoc/>
    public string Name => binding.Remedy;

    /// <inheritdoc/>
    public async Task<RemedyResult> Execute(RemedyInvocation invocation, CancellationToken cancellationToken)
    {
        try
        {
            await deployments.RolloutRestart(binding.Namespace, binding.Deployment, cancellationToken);
            return new RemedyResult(true, 0, $"requested a rolling restart of deployment {binding.Namespace}/{binding.Deployment}");
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return new RemedyResult(false, 0, $"could not restart deployment {binding.Namespace}/{binding.Deployment}: {error.GetType().Name}");
        }
    }
}
