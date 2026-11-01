// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Resolves a Cratis environment, secrets included, from the environment file and the secrets provider it names.
/// </summary>
public interface IEnvironmentResolver
{
    /// <summary>
    /// Resolves the environment.
    /// </summary>
    /// <param name="name">The name of the environment.</param>
    /// <param name="environmentFile">The content of the environment file.</param>
    /// <param name="passphrase">The passphrase of a passphrase environment, when there is one.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resolution.</returns>
    Task<EnvironmentResolution> ResolveAsync(string name, string environmentFile, string? passphrase, CancellationToken cancellationToken);
}
