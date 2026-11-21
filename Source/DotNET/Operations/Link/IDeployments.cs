// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// The one Kubernetes write the link can make: a rolling restart of a named Deployment.
/// </summary>
public interface IDeployments
{
    /// <summary>
    /// Triggers a rolling restart of a Deployment, as <c>kubectl rollout restart</c> does.
    /// </summary>
    /// <param name="namespace">The namespace of the Deployment.</param>
    /// <param name="name">The name of the Deployment.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the restart was requested.</returns>
    Task RolloutRestart(string @namespace, string name, CancellationToken cancellationToken);
}
