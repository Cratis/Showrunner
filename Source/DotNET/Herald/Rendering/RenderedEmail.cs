// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald.Rendering;

/// <summary>
/// Represents an email that is ready to send.
/// </summary>
/// <param name="Kind">The kind of email.</param>
/// <param name="Language">The language of the template used.</param>
/// <param name="Subject">The subject, as plain text.</param>
/// <param name="Html">The email-safe HTML, with its styles inlined and its images referred to as <c>cid:</c>.</param>
/// <param name="Text">The plain-text alternative.</param>
/// <param name="Images">The images the HTML refers to.</param>
public record RenderedEmail(EmailKind Kind, EmailLanguage Language, string Subject, string Html, string Text, IReadOnlyList<EmailImage> Images);
