// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi;

/// <summary>
/// A reference to a key of an existing Kubernetes Secret, in the namespace of the resource that names it.
/// </summary>
public sealed class SecretReferenceArgs
{
    /// <summary>
    /// Gets the name of the Secret.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the key within the Secret, or null to use the key the operator reads by default.
    /// </summary>
    public string? Key { get; init; }
}
