// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.State;

/// <summary>
/// An <see cref="IAlertState"/> that keeps the state in memory, for specifications and for dry runs.
/// </summary>
public sealed class InMemoryAlertState : IAlertState
{
    /// <summary>
    /// Gets or sets the state that is held.
    /// </summary>
    public AlertStateSnapshot Current { get; set; } = AlertStateSnapshot.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether loading throws, to simulate an unreadable store.
    /// </summary>
    public bool FailLoading { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether saving throws, to simulate an unwritable store.
    /// </summary>
    public bool FailSaving { get; set; }

    /// <summary>
    /// Gets the number of times the state was loaded.
    /// </summary>
    public int Loads { get; private set; }

    /// <summary>
    /// Gets the number of times the state was saved.
    /// </summary>
    public int Saves { get; private set; }

    /// <inheritdoc/>
    public Task<AlertStateSnapshot> Load(CancellationToken cancellationToken)
    {
        Loads++;
        return FailLoading ? throw new InvalidOperationException("The state store is unreadable.") : Task.FromResult(Current);
    }

    /// <inheritdoc/>
    public Task Save(AlertStateSnapshot snapshot, CancellationToken cancellationToken)
    {
        Saves++;
        if (FailSaving)
        {
            throw new InvalidOperationException("The state store is unwritable.");
        }

        Current = snapshot;
        return Task.CompletedTask;
    }
}
