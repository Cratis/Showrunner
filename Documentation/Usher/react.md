---
title: React components
description: The @cratis/showrunner.usher.react package - slot Usher into a React application with UsherProvider and the Usher buttons, open its dialogs from your own menus, replace its texts, and show an issue on a page of your own.
---

`@cratis/showrunner.usher.react` is the user interface of Usher: the report a problem and report this error dialogs, the my issues side sheet with each issue's conversation, and two buttons that open them. It is built on `@cratis/components` and the generated [frontend proxies](./frontend-proxies.md), so every dialog is a Cratis Components dialog bound to Usher's commands and queries.

## Slot Usher into an application

Usher needs three things: the backend, the provider and a button.

**Backend.** Reference `Cratis.Showrunner.Usher` and call `AddUsher`; see [Backend](./backend.md#add-usher-to-an-application).

```csharp
builder.AddCratis();
builder.AddUsher(usher => usher.ApplicationName = "Ledger");
```

**Frontend.** Install the package and its peers:

```shell
yarn add @cratis/showrunner.usher.react @cratis/showrunner.usher @cratis/arc @cratis/arc.react @cratis/components @cratis/fundamentals
```

Import the stylesheet once, after the Cratis Components stylesheets:

```css
@import '@cratis/components/tokens';
@import '@cratis/components/styles';
@import '@cratis/components/theme';
@import '@cratis/showrunner.usher.react/styles';
```

Wrap the application in `UsherProvider`, inside `<Arc>`, the dialog registration and a `CratisComponentsProvider` with a toaster:

```tsx
import { Arc } from '@cratis/arc.react';
import { DialogComponents } from '@cratis/arc.react/dialogs';
import { CratisComponentsProvider } from '@cratis/components';
import { BusyIndicatorDialog, ConfirmationDialog } from '@cratis/components/Dialogs';
import { UsherProvider } from '@cratis/showrunner.usher.react';

export const App = () => (
    <CratisComponentsProvider toaster>
        <Arc>
            <DialogComponents confirmation={ConfirmationDialog} busyIndicator={BusyIndicatorDialog}>
                <UsherProvider applicationName='Ledger' applicationVersion='4.12.1'>
                    <Application />
                </UsherProvider>
            </DialogComponents>
        </Arc>
    </CratisComponentsProvider>
);
```

Then place the buttons where they fit:

```tsx
import { MyIssuesButton, ReportButton } from '@cratis/showrunner.usher.react';

<ReportButton size='small' />           // opens Report a problem
<ReportButton size='large' error={e} /> // on an error surface, opens Report this error
<MyIssuesButton size='small' />         // opens My issues
```

That is all: the dialogs, the side sheet and the toast after a report come from the provider.

## UsherProvider

`UsherProvider` renders the dialogs once and gives the buttons and `useUsher` what they need.

| Prop | Default | Meaning |
| --- | --- | --- |
| `applicationVersion` | empty | The version of the application, sent with every report. |
| `applicationName` | empty | Shown to the reporter with what a report sends. The desk sees the name the backend sets with `AddUsher`. |
| `shareIdentityByDefault` | `false` | Whether the report dialogs start with "Let support know who I am" checked. |
| `labels` | English | The texts to replace; see [Texts and localization](#texts-and-localization). |
| `locale` | the browser's | The locale dates and times are formatted for. |
| `correlationIdCount` | `5` | How many recent correlation ids a report carries. |
| `correlationIdHeader` | `X-Correlation-ID` | The header the backend reads the correlation id from. Change it only when the backend changes Arc's correlation id header. |

## Open the dialogs from your own code

`useUsher` opens any dialog from a menu, a shortcut or an error handler of your own:

```tsx
import { errorDetailsFromCommandResult } from '@cratis/showrunner.usher';
import { useUsher } from '@cratis/showrunner.usher.react';

const usher = useUsher();

usher.reportIssue();                // Report a problem
usher.reportError(error);           // Report this error
usher.showMyIssues();               // My issues
usher.showIssue(issueId);           // My issues, on one issue

const result = await payInvoice.execute();
if (!result.isSuccess) {
    usher.reportError(errorDetailsFromCommandResult(result, payInvoice));
}
```

`reportError` takes the `ErrorDetails` the [error helpers](./frontend-proxies.md#turn-an-error-into-what-a-report-sends) build, or anything thrown, which it turns into `ErrorDetails` itself.

## What a report carries

Every report carries, without the reporter doing anything:

- **Where it happened**: the slice location of the component that opened the dialog, from Arc's slice context (`useSliceContext()`). Wrap a part of a page in Arc's `<Slice>` to say which slice it belongs to. For an error, the location the error carries wins.
- **The version** given to `UsherProvider`.
- **The correlation ids** of the last few Arc commands and queries the application ran. For an error, the error's own correlation ids come first.

The report dialogs show the reporter this before they send, such as *Billing › Invoices › Pay · Ledger 4.12.1 · 3 recent requests*.

### How correlation ids are tracked

`UsherProvider` wraps the HTTP headers Arc sends with every command and snapshot query of the components inside it: each request gets an `X-Correlation-ID` header with a new correlation id, unless the application's own headers already carry one, and Usher keeps the most recent ones. Arc's backend uses the correlation id the request carries, so the ids a report sends are the ids the backend recorded, and Usher finds the error it [captured](./backend.md#error-capture) for a failing command.

Some requests are not tracked:

- **Observable queries** run over Arc's shared connection, which does not use the headers of the component, so the subscription of an observable query is not tracked.
- **Commands and queries created outside React**, with `new` instead of their `use()` hook, and those outside `UsherProvider`, do not get the header. Track their correlation id yourself when you have it: `usher.trackCorrelationId(result.correlationId)`.
- **Usher's own** commands and queries are left out, so a report does not carry the requests Usher made to send it.

## Accessibility

- Every button has an accessible name. A small button shows only its icon, and its text is its tooltip and its accessible name. The my issues button says how many issues wait for the user in its name, such as *My issues, 1 waiting for you*; the badge is hidden from screen readers so the count is not read twice.
- The report dialogs open with focus on their title, not on **Send report**, so a held Enter key does not send a report the reporter has not read. Sharing who you are cannot be undone, so its dialog opens with focus on **Cancel**.
- While access is granted, "Let support know who I am" is checked and disabled, and is described by the notice that says until when support can act as you.
- Revoking access and withdrawing an issue ask for confirmation through the application's confirmation dialog.

## Texts and localization

Every text Usher shows or announces comes from `labels`. Pass only the texts you replace; the rest keep their English defaults:

```tsx
<UsherProvider labels={{
    reportIssue: { title: 'Meld et problem', send: 'Send rapport' },
    myIssuesButton: { label: 'Mine saker', waiting: '{count} venter på deg' }
}}>
```

The full set is the `UsherLabels` type, and `defaultUsherLabels` holds the English texts. Placeholders in braces, such as `{count}` and `{time}`, are filled in by the components. The labels of the Cratis Components themselves, such as the paginator, come from `CratisComponentsProvider`.

## Show an issue on a page of your own

`IssueView` is the issue view of the side sheet on its own, for an application that wants issues in its own layout. It shows the issue's state, the conversation with support, who support knows the reporter is, support's access and what the reporter can do, and it stays current as support works on the issue:

```tsx
import { IssueView } from '@cratis/showrunner.usher.react';

<IssueView issueId={issueId} />
<IssueView issueId={issueId} onBack={() => navigate('/issues')} />
```

It must be rendered inside `UsherProvider`, so that its texts and dialogs work. Without `onBack` it shows no back button.

The dialogs are exported too, for an application that opens them with its own `useDialog`: `ReportIssueDialog`, `ReportErrorDialog` and `MyIssuesDialog`. The report dialogs close with the id of the new issue.
