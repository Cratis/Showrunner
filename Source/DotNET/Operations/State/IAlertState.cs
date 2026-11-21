// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.State;

/// <summary>
/// Where the watchdog keeps what it remembers between runs.
/// </summary>
public interface IAlertState
{
    /// <summary>
    /// Loads the state. A store that has never been written yields <see cref="AlertStateSnapshot.Empty"/>.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The state. It throws when the store cannot be read.</returns>
    Task<AlertStateSnapshot> Load(CancellationToken cancellationToken);

    /// <summary>
    /// Saves the state.
    /// </summary>
    /// <param name="snapshot">The state to keep.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the state is stored. It throws when the store cannot be written.</returns>
    Task Save(AlertStateSnapshot snapshot, CancellationToken cancellationToken);
}
