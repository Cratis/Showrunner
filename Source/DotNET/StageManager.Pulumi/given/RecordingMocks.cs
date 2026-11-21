// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG && NET11_0_OR_GREATER
using System.Collections.Concurrent;
using Pulumi.Testing;

namespace Cratis.Showrunner.StageManager.Pulumi.given;

/// <summary>
/// Stands in for the Pulumi engine and keeps every resource registered with it, so no cluster and no engine is needed.
/// </summary>
public sealed class RecordingMocks : IMocks
{
    readonly ConcurrentQueue<RecordedResource> _resources = new();

    /// <summary>
    /// Gets the resources registered so far.
    /// </summary>
    public IReadOnlyList<RecordedResource> Resources => [.. _resources];

    /// <inheritdoc/>
#pragma warning disable SA1316 // The element names are fixed by IMocks; the compiler rejects another casing.
    public Task<(string? id, object state)> NewResourceAsync(MockResourceArgs args)
#pragma warning restore SA1316
    {
        _resources.Enqueue(new RecordedResource(args.Type!, args.Name!, args.Inputs));
        return Task.FromResult<(string? Id, object State)>(($"{args.Name}-id", args.Inputs));
    }

    /// <inheritdoc/>
    public Task<object> CallAsync(MockCallArgs args) => Task.FromResult<object>(args.Args);
}
#endif
