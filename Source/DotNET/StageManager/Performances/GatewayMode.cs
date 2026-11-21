// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The gateway mode of a <see cref="Performance"/>: AuthProxy in front of an existing Service, with no application of its own.
/// </summary>
public static class GatewayMode
{
    /// <summary>
    /// Determines whether the Performance is a gateway.
    /// </summary>
    /// <param name="spec">The spec.</param>
    /// <returns>True when the AuthProxy names an upstream.</returns>
    public static bool IsGateway(PerformanceSpec spec) => spec.AuthProxy.Upstream is not null;

    /// <summary>
    /// Determines whether the Cratis environment is read: always, except for a gateway that names none and needs no key from it.
    /// </summary>
    /// <param name="spec">The spec.</param>
    /// <returns>True when the environment file is read.</returns>
    public static bool NeedsEnvironment(PerformanceSpec spec) =>
        !IsGateway(spec) || EnvironmentKeys.RequiredBy(spec).Count > 0 || !string.IsNullOrWhiteSpace(spec.Environment.ConfigMapRef.Name);
}
