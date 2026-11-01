// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Restores;

/// <summary>
/// What a <see cref="Restore"/> asks for. Schema only: the reconciliation that acts on it is a separate piece of work.
/// </summary>
public class RestoreSpec
{
    /// <summary>
    /// Gets or sets the name of the Understudy whose backup is restored.
    /// </summary>
    [Required]
    [Description("The name of the Understudy whose backup is restored.")]
    public string UnderstudyRef { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the event store to restore.
    /// </summary>
    [Description("The event store to restore.")]
    public string EventStore { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the namespace to restore. Empty restores the whole event store.
    /// </summary>
    [Description("The namespace to restore. Empty restores the whole event store.")]
    public string Namespace { get; set; } = string.Empty;
}
