---
title: React components
description: Integrate Showrunner Notes in React with NotesProvider, feedback buttons, contextual dialogs and a component picker, plus users' own feedback history.
---

`@cratis/showrunner.notes.react` is the user interface of Notes: a feedback button for the top bar and for dialogs, the dialogs that send a note, a way to point at a component, and the list of the user's own feedback. It is built on `@cratis/components` and the generated [frontend proxies](./frontend-proxies.md), so every dialog is a Cratis Components dialog bound to Notes' commands and queries.

## Slot Notes into an application

Notes needs the backend, the provider and a button.

**Backend.** Host `Cratis.Showrunner.Notes`; see [Backend](./backend.md).

**Frontend.** Install the package and its peers:

```shell
yarn add @cratis/showrunner.notes.react @cratis/showrunner.notes @cratis/arc @cratis/arc.react @cratis/components @cratis/fundamentals
```

It needs `@cratis/arc.react` 22.155 or later, for the application context and slice markers, and `@cratis/components` 4.39 or later, for the component picker and dialog header actions.

Import the stylesheet once, after the Cratis Components stylesheets:

```css
@import '@cratis/components/tokens';
@import '@cratis/components/styles';
@import '@cratis/components/theme';
@import '@cratis/showrunner.notes.react/styles';
```

Wrap the application in `NotesProvider`, inside `<Arc sliceMarkers>`, the dialog registration and a `CratisComponentsProvider` with a toaster. Pass `notesDialogHeaderActions` to the Components provider to put the button in the header of dialogs:

```tsx
import { Arc } from '@cratis/arc.react';
import { DialogComponents } from '@cratis/arc.react/dialogs';
import { CratisComponentsProvider } from '@cratis/components';
import { BusyIndicatorDialog, ConfirmationDialog } from '@cratis/components/Dialogs';
import { NotesProvider, notesDialogHeaderActions } from '@cratis/showrunner.notes.react';

export const App = () => (
    <CratisComponentsProvider toaster dialogHeaderActions={notesDialogHeaderActions}>
        <Arc sliceMarkers>
            <DialogComponents confirmation={ConfirmationDialog} busyIndicator={BusyIndicatorDialog}>
                <NotesProvider enabled>
                    <Application />
                </NotesProvider>
            </DialogComponents>
        </Arc>
    </CratisComponentsProvider>
);
```

Then drop the button into a top bar:

```tsx
import { NotesButton } from '@cratis/showrunner.notes.react';

<NotesButton size='small' />
```

The application also needs an identity: the `<Arc>` identity provider tells Notes whether the user is signed in.

## What renders

`NotesButton` renders nothing, and so does the header button, unless **both** hold:

- the user is signed in, as Arc's identity says;
- Notes is switched on, as the `Availability` query's `NotesAvailability.isEnabled` says. Until it answers, nothing renders.

## NotesProvider

| Prop | Default | Meaning |
| --- | --- | --- |
| `enabled` | `false` | Turns on the feedback button in dialog headers. It takes effect through `notesDialogHeaderActions` on the Components provider. |
| `labels` | English | The texts to replace; see [Texts and localization](#texts-and-localization). |
| `knownSlices` | none | Every slice the application knows about, for the search of the slices dialog, as `{ module, feature, slice }`. |

The provider renders the dialogs and the component picker once. It must wrap the application, including the place dialogs render, for the header button to reach it.

## NotesButton

A split button. Props: `size` (`'large'` shows icon and text, `'small'` the icon with a tooltip; default `'large'`), `dialog` (a `DialogHeaderContext`, set by the header registration) and `className`.

| Control | What it does | Command |
| --- | --- | --- |
| Main click, **Give feedback** | Opens the dialog for the context. It shows what was captured in plain language, such as *Orders › Editing › Change order, on screen Order details*, a kind (problem, suggestion, question, praise) and a text. | `GiveNoteOnContext` |
| **About specific slices…** | The slices active on the screen, and a search over `knownSlices`. | `GiveNoteOnSlices` |
| **About this screen…** | The screen as the subject. | `GiveNoteOnScreen` |
| **Point at something…** | Starts the Components picker. The picked component stays highlighted. The dialog then offers **Describe it** and **Change it**. | `GiveNoteOnComponent`, `SuggestComponentPropertyChange` |
| **My feedback** | The user's notes with their status, from the `MyNotes` observable query, each of which can be withdrawn. | `WithdrawNote` |

The drop-down is an accessible menu: arrows, Home and End move, Escape closes and gives focus back to the toggle.

### Point at something

While the picker is on, the user clicks a component, or moves with the arrow keys and presses Enter. **Describe it** sends the text. **Change it** shows the component's presentation properties in the Components property editor: each change is previewed on the live component, and goes away when the dialog closes, whether it was sent or cancelled. One `SuggestComponentPropertyChange` is sent per changed property, all with the note id of the picking session. Picking works from inside an open dialog.

A component's slice comes from the closest Arc slice marker (`data-cratis-module`, `data-cratis-feature` and `data-cratis-slice`), which `<Arc sliceMarkers>` renders. Without markers, the focused location is used.

### In dialogs

With `notesDialogHeaderActions` registered and `<NotesProvider enabled>`, a small button appears in the header of every dialog that has a slice location (see `useDialogSliceLocation` in `@cratis/arc.react/dialogs`) and not in dialogs without one. The context it captures is the dialog's location and title.

## What every command carries

Every command gets the full `NoteContext`, captured when the user clicks, before Notes opens a dialog of its own:

| Field | Source |
| --- | --- |
| `location` | The focused location of Arc's `useApplicationContext()`, or the dialog's location |
| `activeSlices` | The slices active on the screen |
| `screen` | The screen of the application context (`id`, `name`, `path`) |
| `route` | `window.location.pathname` |
| `dialog` | The dialog the button is in, or the top open dialog; empty when none |
| `modelReferences` | The `modelReferences` entry the host contributed to the Arc application context; empty when none |
| `correlationId` | A new correlation id for the interaction |

Tell Notes about the screen and the model with Arc's hook:

```tsx
import { useContributeApplicationContext } from '@cratis/arc.react';

useContributeApplicationContext({
    screen: { id: 'order-details', name: 'Order details', path: '/orders/42' },
    values: { modelReferences: { eventModel: 'orders-v3', slice: 'ChangeOrder' } }
});
```

The commands are guarded against firing twice: a dialog's send button is disabled while a send is in flight, and so are the withdraw buttons.

## Open the dialogs from your own code

`useNotes` opens any dialog from a menu or a shortcut of your own:

```tsx
import { useNotes } from '@cratis/showrunner.notes.react';

const notes = useNotes();

notes.giveFeedback();         // the context
notes.giveFeedbackOnSlices(); // specific slices
notes.giveFeedbackOnScreen(); // this screen
notes.pointAtSomething();     // the picker
notes.showMyFeedback();       // my feedback
```

## Accessibility

- Every button has an accessible name. A small button shows only its icon, and its text is its tooltip and accessible name.
- The toggle says it has a menu and whether it is expanded.
- Each feedback dialog opens with focus in its content, not on **Send feedback**, so a held Enter key does not send a note the user has not written.
- The highlight of a picked component is hidden from assistive technology; the dialog names the component.

## Texts and localization

Every text comes from `labels`. Pass only the texts you replace:

```tsx
<NotesProvider labels={{
    button: { giveFeedback: 'Gi tilbakemelding' },
    myFeedback: { statuses: { addressed: 'Løst i {version}' } }
}}>
```

The full set is the `NotesLabels` type, and `defaultNotesLabels` holds the English texts. Placeholders in braces, such as `{version}` and `{count}`, are filled in by the components.

## Exports

| Export | What it is |
| --- | --- |
| `NotesProvider`, `NotesProviderProps` | The provider |
| `NotesButton`, `NotesButtonProps` | The split button |
| `notesDialogHeaderActions`, `showsFeedbackButton` | The `dialogHeaderActions` renderer and its rule |
| `useNotes`, `useNotesContext`, `NotesContext` | Open the dialogs from code |
| `FeedbackDialog`, `SlicesFeedbackDialog`, `ComponentFeedbackDialog`, `MyFeedbackDialog`, `MyFeedbackList` | The dialogs, for an application that opens them with its own `useDialog` |
| `buildNoteContext`, `describeContext`, `modelReferencesOf` | Build and describe a `NoteContext` |
| `defaultNotesLabels`, `resolveLabels`, `NotesLabels`, `NotesLabelOverrides` | Labels |
