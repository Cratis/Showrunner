// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Seeing.MarkReleaseNotesSeen.when_marking;

/// <summary>
/// A version the application has no notes for is refused.
/// </summary>
public class a_release_without_notes : a_marking_viewer
{
    async Task Because() => _result = await _scenario.Execute(new MarkReleaseNotesSeen("9.9.9"));

    [Fact] void should_not_succeed() => _result.IsSuccess.ShouldBeFalse();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
