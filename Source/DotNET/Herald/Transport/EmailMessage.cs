// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Herald.Rendering;

namespace Cratis.Showrunner.Herald.Transport;

/// <summary>
/// Represents a message ready to hand to a transport.
/// </summary>
/// <param name="From">Who it comes from.</param>
/// <param name="To">Who it is for.</param>
/// <param name="ReplyTo">Where replies go, when not to <paramref name="From"/>.</param>
/// <param name="Subject">The subject.</param>
/// <param name="Html">The HTML, whose images are referred to as <c>cid:{Name}</c>.</param>
/// <param name="Text">The plain-text alternative.</param>
/// <param name="Images">The images to embed.</param>
public record EmailMessage(EmailAddress From, EmailAddress To, EmailAddress? ReplyTo, string Subject, string Html, string Text, IReadOnlyList<EmailImage> Images);
