// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageOperator.Building;
using Cratis.Showrunner.StageOperator.Metering;
using Cratis.Showrunner.StageOperator.Reconciling;
using Cratis.Showrunner.StageOperator.Resources;

namespace Cratis.Showrunner.StageOperator.Kubernetes;

/// <summary>
/// The narrow view of the cluster the operator needs. The reconcile loop is written against this, so its decisions
/// are specified with fakes and only the implementation touches the Kubernetes API.
/// </summary>
public interface IKubernetesGateway
{
    /// <summary>Lists the applications in a namespace, or in every namespace when none is given.</summary>
    /// <param name="namespaceName">The namespace, or null for all.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The applications.</returns>
    Task<IReadOnlyList<StageApplication>> ListApplications(string? namespaceName, CancellationToken cancellationToken);

    /// <summary>Watches the applications of a namespace, or of every namespace when none is given, until the stream ends.</summary>
    /// <param name="namespaceName">The namespace, or null for all.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The applications as they change.</returns>
    IAsyncEnumerable<StageApplication> WatchApplications(string? namespaceName, CancellationToken cancellationToken);

    /// <summary>Gets an application, or null when it no longer exists.</summary>
    /// <param name="namespaceName">The namespace.</param>
    /// <param name="name">The name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The application.</returns>
    Task<StageApplication?> GetApplication(string namespaceName, string name, CancellationToken cancellationToken);

    /// <summary>Observes what the cluster holds for an application.</summary>
    /// <param name="application">The application.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The observed state.</returns>
    Task<ObservedState> Observe(StageApplication application, CancellationToken cancellationToken);

    /// <summary>Creates or updates the objects of an application so they equal the desired ones.</summary>
    /// <param name="desired">The desired objects.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when every object is applied.</returns>
    Task Apply(DesiredObjects desired, CancellationToken cancellationToken);

    /// <summary>Scales the Deployment and the MongoDB StatefulSet of an application to zero, where they exist.</summary>
    /// <param name="application">The application.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when scaled.</returns>
    Task StopWorkloads(StageApplication application, CancellationToken cancellationToken);

    /// <summary>Removes the Ingress of an application, where it exists.</summary>
    /// <param name="application">The application.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when removed.</returns>
    Task DeleteIngress(StageApplication application, CancellationToken cancellationToken);

    /// <summary>Removes the MongoDB StatefulSet and its volumes, where they exist.</summary>
    /// <param name="application">The application.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when removed.</returns>
    Task DeleteData(StageApplication application, CancellationToken cancellationToken);

    /// <summary>Adds the retention finalizer to an application.</summary>
    /// <param name="application">The application.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when added.</returns>
    Task AddFinalizer(StageApplication application, CancellationToken cancellationToken);

    /// <summary>Removes the retention finalizer from an application, which lets its deletion complete.</summary>
    /// <param name="application">The application.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when removed.</returns>
    Task RemoveFinalizer(StageApplication application, CancellationToken cancellationToken);

    /// <summary>
    /// Updates the status of an application. The change is applied to the latest status, so the reconcile loop and the
    /// usage sampler never overwrite each other.
    /// </summary>
    /// <param name="application">The application.</param>
    /// <param name="change">Computes the new status from the latest one.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when written.</returns>
    Task UpdateStatus(StageApplication application, Func<StageApplicationStatus, StageApplicationStatus> change, CancellationToken cancellationToken);

    /// <summary>Reads the metrics of the containers of an application's pods.</summary>
    /// <param name="application">The application.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The usage per container, or null when metrics are not available.</returns>
    Task<IReadOnlyList<ContainerUsage>?> GetContainerUsage(StageApplication application, CancellationToken cancellationToken);
}
