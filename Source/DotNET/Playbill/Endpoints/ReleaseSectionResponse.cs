// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Playbill.Endpoints;

/// <summary>
/// A heading of a release note and what is written under it.
/// </summary>
/// <param name="Name">The heading.</param>
/// <param name="Markdown">The markdown under the heading.</param>
public record ReleaseSectionResponse(string Name, string Markdown);
