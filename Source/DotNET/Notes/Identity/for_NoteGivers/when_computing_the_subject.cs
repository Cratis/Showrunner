// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Chronicle;
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Identity.for_NoteGivers;

/// <summary>
/// The compliance subject is the person, not the note, and a one-way hash: it travels unencrypted with every event.
/// </summary>
public class when_computing_the_subject : Specification
{
    Subject _subject;
    Subject _again;
    Subject _someoneElses;

    void Because()
    {
        _subject = NoteGivers.For(Givers.UserId);
        _again = NoteGivers.For(Givers.UserId);
        _someoneElses = NoteGivers.For(Givers.SomeoneElse);
    }

    [Fact] void should_be_the_same_for_the_same_person() => _subject.ShouldEqual(_again);
    [Fact] void should_differ_between_people() => _subject.ShouldNotEqual(_someoneElses);
    [Fact] void should_be_prefixed() => _subject.Value.StartsWith("notes-giver-", StringComparison.Ordinal).ShouldBeTrue();
    [Fact] void should_not_contain_the_user_id() => _subject.Value.ShouldNotContain(Givers.UserId.Value);
}
#endif
