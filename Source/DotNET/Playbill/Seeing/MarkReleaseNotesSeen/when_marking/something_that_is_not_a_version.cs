// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Seeing.MarkReleaseNotesSeen.when_marking;

/// <summary>
/// Something that is not a version is refused by the validator.
/// </summary>
public class something_that_is_not_a_version : a_marking_viewer
{
    async Task Because() => _result = await _scenario.Execute(new MarkReleaseNotesSeen("latest"));

    [Fact] void should_not_succeed() => _result.IsSuccess.ShouldBeFalse();

    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
