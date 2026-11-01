// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Herald.Rendering;

/// <summary>
/// Represents one template: an email kind in one language.
/// </summary>
/// <param name="Kind">The kind of email.</param>
/// <param name="Language">The language the template is written in, which may be a fallback for the language asked for.</param>
/// <param name="Manifest">The values the kind accepts.</param>
/// <param name="Subject">The subject, which may use values.</param>
/// <param name="PreviewText">The text mail clients show beside the subject, which may use values; empty when there is none.</param>
/// <param name="Body">The email-ready HTML, which may use values and carry a style element.</param>
public record EmailTemplate(EmailKind Kind, EmailLanguage Language, EmailManifest Manifest, string Subject, string PreviewText, string Body);
