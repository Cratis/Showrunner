# @cratis/showrunner.usher.react

React components for Usher, the Showrunner support part: report a problem, report this error, and the my issues side
sheet with each issue's conversation. Built on `@cratis/components` and the generated proxies in
`@cratis/showrunner.usher`. How to use it: [Documentation/Usher/react.md](../../../Documentation/Usher/react.md).

## Component hierarchy

```
UsherProvider                 provides UsherContext, wraps ArcContext for correlation tracking, renders the dialogs once
├── ReportIssueDialog         CommandDialog<ReportIssue>
│   ├── SentWithReport        what is sent along: location, version, recent requests
│   └── AccessFields          share identity / grant access / duration, with AccessNotice
├── ReportErrorDialog         CommandDialog<ReportError>
│   ├── TechnicalDetails      collapsed <details>: type, correlation id, stack frames
│   ├── SentWithReport
│   └── AccessFields
└── MyIssuesDialog            Dialog placement='end', 720px
    ├── IssueList             DataTableForObservableQuery<MyIssues>
    └── IssueView             also exported on its own
        ├── IssueHeader, StatusStrip, QuestionBanner
        ├── IssueConversationPanel   ChatConversation; AnswerQuestion or AddDetails
        ├── IdentityPanel            ShareIdentityDialog
        ├── AccessPanel              GrantAccessDialog, revoke with confirmation, SupportSessions
        └── IssueActions             ReopenIssueDialog, withdraw with confirmation
ReportButton, MyIssuesButton  open the dialogs through useUsher()
```

## Architecture decisions

- **One provider, dialogs rendered once.** `UsherProvider` owns the `useDialog` wrappers; the buttons and `useUsher()`
  only call into `UsherContext`. The location of the caller is taken with `useSliceContext()` where `useUsher()` is
  called and passed to the dialog, because the dialogs render at the provider, outside the caller's slice scope.
- **Correlation tracking through the HTTP headers callback.** Arc has no global observer of command or query results,
  but Arc React hands each command and snapshot query the `httpHeadersCallback` of the nearest `ArcContext`. The
  provider wraps it to add an `X-Correlation-ID` to every request and remember it (`Correlation/`). Arc's backend uses
  the id a request carries. Observable queries go over Arc's shared hub connection and are not tracked.
- **`UsherBoundary` keeps Usher out of what it reports.** Usher's own components run with the untracked `ArcContext`
  and inside a `<Slice>` boundary, so its requests are not sent with a report and its proxies do not change the slice
  context of the application around them.
- **Granting access forces sharing identity.** `useAccessChoice` follows the form through `onFieldChange` and forces
  `shareIdentity` through `currentValues`. `shareIdentity` is left out of `initialValues`, because the form merges
  `initialValues` over `currentValues`.

## State

Decisions live in pure, specified modules; the components render and wire events:

| Module | Decides |
| --- | --- |
| `Reporting/AccessChoice.ts` | access choice transitions, forced identity, until when access lasts |
| `Reporting/reportValues.ts` | the initial values of a report, error details from what was thrown |
| `Issues/issueStates.ts` | state badges, closed states, status strip step |
| `Issues/MyIssuesList.ts` | waiting count, closed filter, fix released |
| `Issues/conversationMessages.ts` | chat authors and message bodies |
| `Issues/accessStatus.ts` | whether support may sign in, against the clock |
| `Issues/MyIssues/MyIssuesNavigation.ts` | list or issue, show closed |
| `Buttons/buttonModels.ts` | text, tooltip and accessible name of the buttons |
| `Labels/` | English defaults, deep overrides, placeholders |

## CSS conventions

One stylesheet, `styles.css`, exported as `@cratis/showrunner.usher.react/styles`; the build has no CSS pipeline, so
components do not import CSS. Classes are prefixed `usher-` with BEM-like element names, and every color, border and
radius comes from the `--cratis-*` tokens. State is styled through `data-*` attributes (`data-highlighted`,
`data-reached`, `data-current`).

## Extending

- A new text: add it to `UsherLabels` and `defaultUsherLabels`; every component reads labels from `UsherContext`.
- A new conversation entry kind: add its sentence to `ConversationEventLabels` and map it in `conversationMessages.ts`.
- A new dialog: render its `useDialog` wrapper in `UsherProvider` and add an opener to `UsherContextValue` and `useUsher`.
