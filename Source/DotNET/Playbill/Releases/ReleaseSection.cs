// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Playbill.Releases;

/// <summary>
/// One heading of a release note, such as <c>Added</c> or <c>Fixed</c>, with what is written under it.
/// </summary>
/// <param name="Name">The heading.</param>
/// <param name="Markdown">The markdown under the heading, without the heading itself.</param>
public record ReleaseSection(string Name, string Markdown);
