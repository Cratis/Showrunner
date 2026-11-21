// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Pipeline;

/// <summary>
/// A check that keeps state of its own between runs, next to the cooldowns, as JSON. The run hands the state over before the checks
/// run and saves what the check says afterwards, whatever else happens to the run.
/// </summary>
public interface IPersistentCheck : IWatchdogCheck
{
    /// <summary>
    /// Takes over the state the previous run left, or <see langword="null"/> when there is none.
    /// </summary>
    /// <param name="json">The saved state.</param>
    void Restore(string? json);

    /// <summary>
    /// Gets the state to save, or <see langword="null"/> when the check has nothing to keep.
    /// </summary>
    /// <returns>The state as JSON.</returns>
    string? Persist();
}
