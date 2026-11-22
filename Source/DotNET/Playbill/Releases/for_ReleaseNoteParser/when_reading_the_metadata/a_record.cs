// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Playbill.Releases.for_ReleaseNoteParser.when_reading_the_metadata;

/// <summary>
/// The record is optional and never gets in the way: what cannot be read is simply not known.
/// </summary>
public class a_record : Specification
{
    [Fact] void should_read_when_and_from_which_pull_request() => ReleaseNoteParser.ReadMetadata("{ \"releasedAt\": \"2026-03-02T10:00:00Z\", \"pull\": 12 }")
        .ShouldEqual((new DateTimeOffset(2026, 3, 2, 10, 0, 0, TimeSpan.Zero), "12"));

    [Fact] void should_accept_a_pull_request_written_as_text() => ReleaseNoteParser.ReadMetadata("{ \"pull\": \"12\" }").Pull.ShouldEqual("12");

    [Fact] void should_know_nothing_when_there_is_no_record() => ReleaseNoteParser.ReadMetadata(null).ShouldEqual((null, null));

    [Fact] void should_know_nothing_when_the_record_is_not_json() => ReleaseNoteParser.ReadMetadata("not json").ShouldEqual((null, null));

    [Fact] void should_know_nothing_when_the_record_is_not_an_object() => ReleaseNoteParser.ReadMetadata("[1]").ShouldEqual((null, null));

    [Fact] void should_ignore_a_date_that_is_not_a_date() => ReleaseNoteParser.ReadMetadata("{ \"releasedAt\": \"soon\" }").ReleasedAt.ShouldBeNull();
}
#endif
