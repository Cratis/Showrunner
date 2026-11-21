// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Pipeline;

/// <summary>
/// How a run ended.
/// </summary>
/// <param name="Failure">The original failure, or <see langword="null"/> when the run succeeded.</param>
public sealed record WatchdogOutcome(Exception? Failure = null)
{
    /// <summary>
    /// Gets a value indicating whether the run succeeded. A run with findings that were alerted is a success; a run that could
    /// not collect, alert or save is not.
    /// </summary>
    public bool Succeeded => Failure is null;

    /// <summary>
    /// Gets the process exit code: 0 when the run succeeded, 1 when it failed.
    /// </summary>
    public int ExitCode => Succeeded ? 0 : 1;
}
