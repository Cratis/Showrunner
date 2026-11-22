// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Playbill.Seeing.MarkReleaseNotesSeen;

/// <summary>
/// Who is marking release notes as seen, and whether they had seen that far already.
/// </summary>
/// <param name="Viewer">The person.</param>
/// <param name="AlreadySeen">Whether they had already seen the notes of that release.</param>
public record SeenMark(ViewerSubject Viewer, bool AlreadySeen);
