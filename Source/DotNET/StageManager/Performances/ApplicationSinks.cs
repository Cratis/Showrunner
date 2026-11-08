// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.StageManager.Productions;

namespace Cratis.Showrunner.StageManager.Performances;

/// <summary>
/// Finds the sinks the application of a Performance uses, and which of them its Production does not configure.
/// </summary>
/// <remarks>
/// A traditional application lists its sinks in <c>application.sinks</c>. A published Stage application declares them in its package's
/// <c>manifest.json</c> (Cratis/Stage#1014); those names are passed to <see cref="UsedBy"/> as <c>packageSinks</c>.
/// </remarks>
public static class ApplicationSinks
{
    /// <summary>
    /// Gets the names of the sinks an application uses, each once, without regard to case as Chronicle matches them.
    /// </summary>
    /// <param name="application">The application.</param>
    /// <param name="packageSinks">The sinks a published Stage package declares in its manifest, when there is one.</param>
    /// <returns>The names, in the order they are first declared.</returns>
    public static IReadOnlyList<string> UsedBy(ApplicationSpec application, IEnumerable<string>? packageSinks = null) =>
        [.. application.Sinks.Concat(packageSinks ?? []).Where(name => !string.IsNullOrWhiteSpace(name)).Distinct(StringComparer.OrdinalIgnoreCase)];

    /// <summary>
    /// Gets the sinks an application uses that a Production does not configure.
    /// </summary>
    /// <param name="used">The names of the sinks the application uses.</param>
    /// <param name="production">The spec of the Production.</param>
    /// <returns>The names the Production lacks, empty when it configures every one.</returns>
    public static IReadOnlyList<string> MissingFrom(IEnumerable<string> used, ProductionSpec production) =>
        [.. used.Where(name => !production.Sinks.Configures(name))];
}
