// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Productions;

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Binds the read side of the named sinks and the graph of a Production to the application of a Performance, and says in the status what it bound and what the application cannot read.
/// </summary>
/// <remarks>
/// The conditions are <c>SinksBound</c> and, when the application reads the graph, <c>GraphBound</c>: when one is false the Performance is blocked, because a Secret it must mount is not in its namespace.
/// <c>SinksReadable</c> is a warning: false names the sinks Cratis Arc cannot read, and the Performance still rolls out.
/// </remarks>
/// <param name="cluster">The cluster.</param>
public class ApplicationReadSideBinding(IClusterResources cluster)
{
    /// <summary>
    /// Binds the read side.
    /// </summary>
    /// <param name="performance">The Performance, with a <c>productionRef</c>.</param>
    /// <param name="production">The spec of its Production, which configures every sink the application uses.</param>
    /// <param name="status">The status to record conditions in.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The read side, or null when a Secret it needs is not in the namespace of the Performance.</returns>
    public async Task<ApplicationReadSide?> BindAsync(Performance performance, ProductionSpec production, PerformanceStatusBuilder status, CancellationToken cancellationToken)
    {
        var reference = performance.Spec.ProductionRef!;
        var @namespace = performance.Metadata.NamespaceProperty;
        var names = new ProductionNames(reference.Name, reference.Namespace ?? @namespace);
        var readSide = ApplicationReadSide.For(performance.Spec.Application, ApplicationSinks.UsedBy(performance.Spec.Application), production, names);

        var readable = readSide.Unreadable.Count == 0;
        var readableMessage = readable
            ? "Cratis Arc can read every named sink the application uses."
            : $"Cratis Arc cannot read {readSide.Unreadable.Count} sink(s) the application uses: {string.Join(", ", readSide.Unreadable)}. Chronicle still writes there, but queries over those read models read the default storage instead. Arc reads named MongoDB sinks and the graph of the Production only.";
        status.Set("SinksReadable", readable, readable ? "Readable" : "NotReadableByArc", readableMessage);

        var sinksBound = await BindSinksAsync(@namespace, readSide, status, cancellationToken);
        var graphBound = await BindGraphAsync(@namespace, reference.Name, readSide, status, cancellationToken);
        return sinksBound && graphBound ? readSide : null;
    }

    static string Describe(ResolvedSecretRef secret) =>
        $"'{secret.SecretName}' (key '{secret.Key}')";

    async Task<IReadOnlyList<ResolvedSecretRef>> MissingAsync(string @namespace, IEnumerable<ResolvedSecretRef> secrets, CancellationToken cancellationToken)
    {
        var missing = new List<ResolvedSecretRef>();
        foreach (var secret in secrets)
        {
            if (await cluster.GetSecretValueAsync(@namespace, secret.SecretName, secret.Key, cancellationToken) is null)
            {
                missing.Add(secret);
            }
        }

        return missing;
    }

    async Task<bool> BindSinksAsync(string @namespace, ApplicationReadSide readSide, PerformanceStatusBuilder status, CancellationToken cancellationToken)
    {
        var sinks = readSide.MongoDBSinks;
        var missing = await MissingAsync(@namespace, sinks.Select(sink => sink.Connection), cancellationToken);
        var (reason, message) = (sinks.Count, missing.Count) switch
        {
            (0, _) => ("NoMongoDBSinks", "The application uses no named MongoDB sink."),
            (_, 0) => ("Bound", $"Bound the named MongoDB sink(s) {string.Join(", ", sinks.Select(sink => sink.Name))} for Cratis Arc to read, each from a mounted file."),
            _ => ("SecretNotInNamespace", $"The namespace '{@namespace}' of the Performance has no {string.Join(", ", missing.Select(Describe))}, holding the connection of a named MongoDB sink the application uses. A Performance mounts Secrets from its own namespace only: put it in the namespace of its Production.")
        };
        status.Set("SinksBound", missing.Count == 0, reason, message);
        return missing.Count == 0;
    }

    async Task<bool> BindGraphAsync(string @namespace, string production, ApplicationReadSide readSide, PerformanceStatusBuilder status, CancellationToken cancellationToken)
    {
        if (!readSide.GraphWanted)
        {
            return true;
        }

        if (readSide.Graph is not { } graph)
        {
            status.Set("GraphBound", false, "ProductionHasNoGraph", $"The application reads the graph of the Production '{production}', but its graph.mode is None.");
            return false;
        }

        var missing = await MissingAsync(@namespace, graph.Secrets, cancellationToken);
        var (reason, message) = (missing.Count, graph.SharedCredential) switch
        {
            (> 0, _) => ("SecretNotInNamespace", $"The namespace '{@namespace}' of the Performance has no {string.Join(", ", missing.Select(Describe))}, holding the graph of the Production '{production}'. A Performance reads Secrets from its own namespace only: put it in the namespace of its Production."),
            (_, true) => ("SharedCredential", $"Bound the graph of the Production '{production}' for Cratis.Arc.Neo4j. On Community there is one user, so the application reads with the credential Chronicle writes with."),
            _ => ("Bound", $"Bound the graph of the Production '{production}' for Cratis.Arc.Neo4j, with the credential Chronicle writes with.")
        };
        status.Set("GraphBound", missing.Count == 0, reason, message);
        return missing.Count == 0;
    }
}
