// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Notes.Contracts.Context.for_NoteDialog;

public class when_there_is_no_dialog : Specification
{
    NoteDialog _dialog = default!;

    void Because() => _dialog = NoteDialog.NoDialog;

    [Fact] void should_have_no_title() => _dialog.DialogTitle.ShouldEqual(DialogTitle.NotSet);
    [Fact] void should_have_no_location() => _dialog.Location.ShouldEqual(SliceLocation.NotSet);
}
#endif
