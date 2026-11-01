# @cratis/showrunner.notes.react

React components for Notes, the Showrunner product feedback part: a feedback button with a context drop-down, the
feedback dialogs, the component picker and the user's own feedback. Built on `@cratis/components` and the generated
proxies in `@cratis/showrunner.notes`. How to use it: [Documentation/Notes/react.md](../../../Documentation/Notes/react.md).

## Component hierarchy

```
NotesProvider                 provides NotesContext, renders the dialogs and the component picker once
├── FeedbackDialog            CommandDialog<GiveNoteOnContext | GiveNoteOnScreen>
├── SlicesFeedbackDialog      CommandDialog<GiveNoteOnSlices>, with SliceChooser
├── ComponentFeedbackDialog   highlight; Describe it = CommandDialog<GiveNoteOnComponent>,
│                             Change it = Dialog + PresentationPropertyEditor (preview), SuggestComponentPropertyChange per property
└── MyFeedbackDialog          Dialog placement='end', MyNotes observable query, WithdrawNote
NotesButton                   split button; opens the dialogs through NotesContext
notesDialogHeaderActions      renderer for CratisComponentsProvider's dialogHeaderActions
```

## Architecture decisions

- **The context is captured at the click.** `NotesProvider` reads Arc's application context store when the user clicks,
  before Notes opens a dialog of its own, so a Notes dialog never ends up as the "open dialog" of its own note.
- **The provider cannot register the dialog header action itself.** `dialogHeaderActions` is a prop of the host's
  `CratisComponentsProvider`, so Notes exports a renderer, `notesDialogHeaderActions`, and `<NotesProvider enabled>`
  is what makes it show.
- **A component's slice comes from the picker's `resolve`.** `resolveComponentContext` reads the closest Arc slice
  marker; without markers the focused location is used.
- **Previews belong to the property editor.** `PresentationPropertyEditor preview` reverts when it unmounts, which is
  when the dialog closes, whether it was sent or cancelled.
- **Visibility is one rule.** `isNotesAvailable(isSignedIn, isEnabled)`: the user is signed in and `NotesAvailability`
  says Notes is on.

## State

| Module | Decides |
| --- | --- |
| `Context/noteContext.ts` | the `NoteContext` of a click, the plain-language sentence |
| `Context/componentContext.ts` | the slice of a picked component, the component a note carries |
| `Feedback/sliceChoice.ts` | choosing and searching slices |
| `Feedback/propertySuggestions.ts` | one suggestion per changed property |
| `MyFeedback/noteStates.ts` | status and target text, what can be withdrawn |
| `Buttons/menuItems.ts` | drop-down items and keyboard movement |
| `Provider/notesVisibility.ts` | whether anything renders |
| `Labels/` | English defaults, deep overrides, placeholders |

## CSS conventions

One stylesheet, `styles.css`, exported as `@cratis/showrunner.notes.react/styles`. Classes are prefixed `notes-`, and
every color, border and radius comes from the `--cratis-*` tokens.
