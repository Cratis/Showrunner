// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi.Operators;

/// <summary>
/// The Percona Operator for MongoDB a <see cref="StageManagerOperator"/> installs for Productions with <c>database.mode: Managed</c>.
/// </summary>
public sealed class StageManagerPerconaArgs
{
    /// <summary>
    /// Gets the version of the Percona <c>psmdb-operator</c> chart, or null for the latest the repository serves. Pin it for a
    /// production environment.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// Gets the namespace the Percona operator runs in. It watches every namespace, so Productions can live anywhere. Defaults to <c>percona-operator</c>.
    /// </summary>
    public string Namespace { get; init; } = StageManagerOperator.DefaultPerconaNamespace;

    /// <summary>
    /// Gets the Helm repository serving the chart. Defaults to Percona's own.
    /// </summary>
    public string Repository { get; init; } = StageManagerOperator.DefaultPerconaRepository;

    /// <summary>
    /// Gets extra values of the Percona chart, merged over the ones the component sets.
    /// </summary>
    public IDictionary<string, object> Values { get; init; } = new Dictionary<string, object>(StringComparer.Ordinal);
}
