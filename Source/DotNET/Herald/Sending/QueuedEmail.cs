// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.Keys;
using Cratis.Chronicle.Projections.ModelBound;

namespace Cratis.Showrunner.Herald.Sending;

/// <summary>
/// An email that has been accepted and has not yet been sent or failed. It only says where to look: the delivery reads
/// the email itself, and whether it is still waiting, from the event log.
/// </summary>
/// <param name="EmailId">The email.</param>
/// <param name="About">Whatever the email is about, whose event source it was recorded against.</param>
/// <param name="QueuedAt">When it was accepted.</param>
[FromEvent<EmailQueued>(key: nameof(EmailQueued.EmailId))]
[RemovedWith<EmailSent>(key: nameof(EmailSent.EmailId))]
[RemovedWith<EmailFailed>(key: nameof(EmailFailed.EmailId))]
public record QueuedEmail(
    [property: Key] EmailId EmailId,
    [property: SetFromContext<EmailQueued>(nameof(EventContext.EventSourceId))] EventSourceId About,
    DateTimeOffset QueuedAt);
