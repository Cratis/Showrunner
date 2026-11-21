// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// What the link remembers between passes that is not a credential: observations, remedy attempts and what is waiting for
/// verification. Kept in the link's ConfigMap.
/// </summary>
/// <param name="Observations">The last observation of every check by name.</param>
/// <param name="Attempts">When each remedy's attempts started, within the attempt window.</param>
/// <param name="AwaitingVerification">The remedies that ran and wait for their verification check to pass, with when they ran.</param>
/// <param name="Handled">The remedy requests and denials already dealt with, so a repeat is idempotent.</param>
/// <param name="Escalated">The remedies already escalated in the current failure episode.</param>
public sealed record LinkMemory(
    IReadOnlyDictionary<string, CheckObservation> Observations,
    IReadOnlyDictionary<string, IReadOnlyList<DateTimeOffset>> Attempts,
    IReadOnlyDictionary<string, DateTimeOffset> AwaitingVerification,
    IReadOnlyList<string> Handled,
    IReadOnlyList<string> Escalated)
{
    /// <summary>
    /// Gets the memory of a link that has not run.
    /// </summary>
    public static LinkMemory Empty { get; } = new(
        new Dictionary<string, CheckObservation>(),
        new Dictionary<string, IReadOnlyList<DateTimeOffset>>(),
        new Dictionary<string, DateTimeOffset>(),
        [],
        []);
}
