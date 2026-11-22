// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Playbill.given;

namespace Cratis.Showrunner.Playbill.Releases.for_ReleaseNotes.when_reading_the_history;

/// <summary>
/// An application built without a ReleaseNotes folder has an empty history, not a failure.
/// </summary>
public class no_files_at_all : Specification
{
    ReleaseNotes _notes;

    void Because() => _notes = ReleaseHistory.Of(new FakeReleaseNoteFiles(), "1.0.0");

    [Fact] void should_have_no_releases() => _notes.All.ShouldBeEmpty();

    [Fact] void should_still_know_the_running_version() => _notes.Current.Value.ShouldEqual("1.0.0");

    [Fact] void should_not_find_a_release() => _notes.Find("1.0.0").ShouldBeNull();
}
#endif
