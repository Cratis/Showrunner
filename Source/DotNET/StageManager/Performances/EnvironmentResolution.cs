// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The outcome of resolving a Cratis environment: its values, or why they could not be resolved. The failure never contains a secret value.
/// </summary>
/// <param name="Environment">The resolved environment, when resolving succeeded.</param>
/// <param name="Failure">Why resolving failed, otherwise empty.</param>
public record EnvironmentResolution(ResolvedEnvironment? Environment, string Failure)
{
    /// <summary>
    /// Creates a resolution that succeeded.
    /// </summary>
    /// <param name="environment">The resolved environment.</param>
    /// <returns>The resolution.</returns>
    public static EnvironmentResolution Resolved(ResolvedEnvironment environment) => new(environment, string.Empty);

    /// <summary>
    /// Creates a resolution that failed.
    /// </summary>
    /// <param name="failure">Why it failed, without any secret value.</param>
    /// <returns>The resolution.</returns>
    public static EnvironmentResolution Failed(string failure) => new(null, failure);
}
