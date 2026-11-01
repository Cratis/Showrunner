// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// The names of everything a <see cref="Performance"/> owns, all derived from the Performance's name.
/// </summary>
/// <param name="Performance">The name of the Performance.</param>
public readonly record struct PerformanceNames(string Performance)
{
    /// <summary>
    /// Gets the name of the application workload.
    /// </summary>
    public string Application => $"{Performance}-app";

    /// <summary>
    /// Gets the name of the AuthProxy workload.
    /// </summary>
    public string AuthProxy => $"{Performance}-authproxy";

    /// <summary>
    /// Gets the name of the Ante workload.
    /// </summary>
    public string Ante => $"{Performance}-ante";

    /// <summary>
    /// Gets the name of the Secret mounted into the application.
    /// </summary>
    public string ApplicationSecret => $"{Performance}-app-environment";

    /// <summary>
    /// Gets the name of the Secret mounted into AuthProxy.
    /// </summary>
    public string AuthProxySecret => $"{Performance}-authproxy-environment";

    /// <summary>
    /// Gets the name of the Secret mounted into Ante.
    /// </summary>
    public string AnteSecret => $"{Performance}-ante-secrets";

    /// <summary>
    /// Gets the name of the Secret that keeps the generated signing keys.
    /// </summary>
    public string SigningKeysSecret => $"{Performance}-signing-keys";

    /// <summary>
    /// Gets the name of the NetworkPolicy of a published application.
    /// </summary>
    public string NetworkPolicy => $"{Performance}-app";

    /// <summary>
    /// Gets the name of the Ingress.
    /// </summary>
    public string Ingress => Performance;

    /// <summary>
    /// Gets the name of the Secret cert-manager writes the certificate to.
    /// </summary>
    public string TlsSecret => $"{Performance}-tls";

    /// <summary>
    /// Gets the names for a Performance.
    /// </summary>
    /// <param name="performance">The Performance.</param>
    /// <returns>The names.</returns>
    public static PerformanceNames For(Performance performance) => new(performance.Metadata.Name);
}
