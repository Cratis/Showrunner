// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Creates the Secrets a Performance mounts into its workloads.
/// </summary>
public static class SecretFactory
{
    /// <summary>
    /// Creates an Opaque Secret from files.
    /// </summary>
    /// <param name="performance">The Performance that owns it.</param>
    /// <param name="name">The name of the Secret.</param>
    /// <param name="component">The component it belongs to.</param>
    /// <param name="files">The file contents by file name.</param>
    /// <returns>The Secret.</returns>
    public static V1Secret Create(Performance performance, string name, string component, IReadOnlyDictionary<string, string> files) => new()
    {
        ApiVersion = "v1",
        Kind = "Secret",
        Metadata = PerformanceMetadata.Create(performance, name, component),
        Type = "Opaque",
        Data = files.ToDictionary(pair => pair.Key, pair => Encoding.UTF8.GetBytes(pair.Value), StringComparer.Ordinal)
    };
}
