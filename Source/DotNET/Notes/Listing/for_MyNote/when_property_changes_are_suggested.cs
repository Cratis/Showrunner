// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Listing.for_MyNote;

/// <summary>
/// The suggestions of a picking session open the note, one per property; the text about the component follows.
/// </summary>
public class when_property_changes_are_suggested : a_read_model<MyNote>
{
    MyNote _suggested;
    MyNote _written;

    async Task Because()
    {
        await Happened(NoteEvents.Suggested(), NoteEvents.Suggested() with { Property = "color", SuggestedValue = "red" }, NoteEvents.Suggested() with { SuggestedValue = "Send now" });
        _suggested = _scenario.Instance!;
        await Happened(new Giving.GiveNoteOnComponent.GiverGaveNoteOnComponent(NoteEvents.Application, Givers.Giver, NoteEvents.Context, NoteEvents.Component, NoteKind.Suggestion, "Make it stand out."));
        _written = _scenario.Instance!;
    }

    [Fact] void should_be_about_a_component() => _suggested.Target.ShouldEqual(NoteTarget.Component);
    [Fact] void should_name_the_component() => _suggested.Component.ShouldEqual(NoteEvents.Component.ComponentName);
    [Fact] void should_keep_one_change_per_property() => _suggested.PropertyChanges.Select(_ => _.Property).ShouldContainOnly(new ComponentProperty("label"), new ComponentProperty("color"));
    [Fact] void should_keep_the_latest_suggestion_for_a_property() => _suggested.PropertyChanges.Single(_ => _.Property == "label").SuggestedValue.ShouldEqual(new PropertyValue("Send now"));
    [Fact] void should_have_no_text_until_it_is_written() => _suggested.Text.ShouldEqual(MyNoteText.NotSet);
    [Fact] void should_have_the_text_once_written() => _written.Text.ShouldEqual(new MyNoteText("Make it stand out."));
    [Fact] void should_keep_the_changes_once_the_text_is_written() => _written.PropertyChanges.Count().ShouldEqual(2);
}
#endif
