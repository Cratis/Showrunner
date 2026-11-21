// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Operations.Alerts;
using Cratis.Showrunner.Operations.Pipeline;

namespace Cratis.Showrunner.Operations.given;

/// <summary>
/// An <see cref="IWatchdogCheck"/> whose findings the specification decides, pass by pass.
/// </summary>
/// <param name="name">The name of the check.</param>
/// <param name="collect">What the check does on each pass; the argument is the zero-based pass number.</param>
public sealed class StubCheck(string name, Func<int, CheckContext, CancellationToken, Task<IReadOnlyCollection<Finding>>> collect) : IWatchdogCheck
{
    /// <summary>
    /// Gets how many times the check ran.
    /// </summary>
    public int Passes { get; private set; }

    /// <inheritdoc/>
    public string Name => name;

    /// <summary>
    /// Creates a check that finds the same findings on every pass.
    /// </summary>
    /// <param name="name">The name of the check.</param>
    /// <param name="findings">The findings.</param>
    /// <returns>The check.</returns>
    public static StubCheck Finding(string name, params Finding[] findings) =>
        new(name, (_, _, _) => Task.FromResult<IReadOnlyCollection<Finding>>(findings));

    /// <summary>
    /// Creates a check that finds <paramref name="findings"/> on the first pass and nothing after it.
    /// </summary>
    /// <param name="name">The name of the check.</param>
    /// <param name="findings">The findings of the first pass.</param>
    /// <returns>The check.</returns>
    public static StubCheck FindingOnlyOnce(string name, params Finding[] findings) =>
        new(name, (pass, _, _) => Task.FromResult<IReadOnlyCollection<Finding>>(pass == 0 ? findings : []));

    /// <inheritdoc/>
    public Task<IReadOnlyCollection<Finding>> Collect(CheckContext context, CancellationToken cancellationToken) =>
        collect(Passes++, context, cancellationToken);
}
#endif
