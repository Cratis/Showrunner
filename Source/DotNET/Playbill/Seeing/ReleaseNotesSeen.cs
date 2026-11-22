// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Playbill.Seeing;

/// <summary>
/// A person saw the release notes up to a version, and will not be told about those releases again. Appended under the
/// person's own stream, with the person as compliance subject.
/// </summary>
/// <param name="Version">The newest release the person has seen the notes of.</param>
/// <param name="Viewer">Who saw them.</param>
[EventType]
public record ReleaseNotesSeen(ReleaseVersion Version, ViewerSubject Viewer);
