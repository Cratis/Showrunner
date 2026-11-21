// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// Everything one pass of the link knows and changes: the loaded state and how the conversation with Direct is going.
/// </summary>
internal sealed class LinkContext
{
    /// <summary>
    /// Gets or sets the identity, or <see langword="null"/> when the watchdog has not been enrolled.
    /// </summary>
    internal LinkIdentity? Identity { get; set; }

    /// <summary>
    /// Gets or sets the outbox.
    /// </summary>
    internal Outbox Outbox { get; set; } = null!;

    /// <summary>
    /// Gets or sets the memory.
    /// </summary>
    internal LinkMemory Memory { get; set; } = LinkMemory.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the state was loaded and the link may work at all.
    /// </summary>
    internal bool Ready { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Direct confirmed the claim this pass and is still answering.
    /// </summary>
    internal bool Connected { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether Direct fenced this watchdog: it must stop acting through the link.
    /// </summary>
    internal bool Fenced { get; set; }

    /// <summary>
    /// Gets or sets how the link stands.
    /// </summary>
    internal LinkConnection Connection { get; set; } = LinkConnection.NotEnrolled;

    /// <summary>
    /// Gets the problems of the pass, without secrets.
    /// </summary>
    internal List<string> Problems { get; } = [];

    /// <summary>
    /// Gets what remedies did this pass.
    /// </summary>
    internal List<string> RemedyActions { get; } = [];

    /// <summary>
    /// Gets or sets how many events the pass delivered.
    /// </summary>
    internal int Delivered { get; set; }

    /// <summary>
    /// Gets a value indicating whether the watchdog holds an instance credential.
    /// </summary>
    internal bool Enrolled => Identity is { Credential.Length: > 0 };

    /// <summary>
    /// Gets the credential and epoch every request after the claim presents.
    /// </summary>
    internal DirectCredentials Credentials => new(Identity!.Credential, Identity.Epoch);

    /// <summary>
    /// Gets the configuration that may be acted on now: held, signed under the pinned key and unexpired.
    /// </summary>
    /// <param name="now">The current time.</param>
    /// <returns>The configuration, or <see langword="null"/> when none may be acted on.</returns>
    internal SignedWatchdogConfiguration? Active(DateTimeOffset now) =>
        Identity?.Held is { } held && held.Snapshot.ExpiresAt > now && ConfigurationSignature.Verify(held, Identity.SigningKey) ? held : null;

    /// <summary>
    /// Interprets an answer: records what a failure means for the link.
    /// </summary>
    /// <typeparam name="T">The response type.</typeparam>
    /// <param name="answer">The answer.</param>
    /// <param name="what">What was being asked, for the problem text.</param>
    /// <returns>True when the request succeeded.</returns>
    internal bool Succeeded<T>(DirectAnswer<T> answer, string what)
    {
        switch (answer.Outcome)
        {
            case DirectOutcome.Ok:
            case DirectOutcome.NoContent:
                return true;
            case DirectOutcome.Fenced:
                Fenced = true;
                Connected = false;
                Connection = LinkConnection.Fenced;
                Problems.Add($"fenced while {what}: {answer.Reason}");
                return false;
            case DirectOutcome.Unauthorized:
                Connected = false;
                Connection = LinkConnection.Unauthorized;
                Problems.Add($"the credential was not accepted while {what}");
                return false;
            case DirectOutcome.Rejected:
                Problems.Add($"Direct refused {what}: {answer.Reason}");
                return false;
            default:
                Connected = false;
                Connection = LinkConnection.Offline;
                Problems.Add($"Direct unreachable while {what}: {answer.Reason}");
                return false;
        }
    }
}
