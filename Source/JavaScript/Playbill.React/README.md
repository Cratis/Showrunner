# @cratis/showrunner.playbill.react

The Playbill release-notes experience for React, the Showrunner release-notes part: `<Playbill />` is a releases page,
`<PlaybillDialog />` opens by itself with what is new, and `<PlaybillBadge />` is the unread count for a header. Built on
`@cratis/components` and the generated proxies in `@cratis/showrunner.playbill`. How to use it:
[Documentation/Playbill](../../../Documentation/Playbill/index.md).

## Component hierarchy

```
Playbill                     usePlaybill, then the list and the selected release
├── nav.playbill__list       one button for each release: version, tags, date and summary
└── ReleaseNoteView          one release: header, lead and a section for each heading
    └── PlaybillMarkdown     the lead and each section

PlaybillDialog               Cratis Components Dialog, the notes of every unread release
└── ReleaseNoteView          one for each release, one heading level down

PlaybillBadge                a button with the unread count and an accessible name that says it in words
```

`release` and `markdown` are slots: a product replaces either. `usePlaybill` gives a product the releases and the status
to draw a surface of its own.

## Architecture decisions

- **Reading the history and remembering what was seen are separate.** The history comes over plain HTTP from the
  endpoints `MapPlaybill` maps (`Client/PlaybillClient.ts`, reached through `PlaybillProvider`), so it needs no Arc and
  works for a public page. What a person has seen is the generated command and observable query, so it needs Arc.
  `useReleases` is the first, `usePlaybillStatus` the second, `usePlaybill` both.
- **Decisions live in pure, specified modules; the components render and wire events.**
  | Module | Decides |
  | --- | --- |
  | `Markdown/assetUrl.ts` | where an image of a note is loaded from, and which are refused |
  | `Markdown/safeLink.ts` | which links may be followed |
  | `Release/sectionPresentation.ts` | the title and the style of a section, known or not |
  | `Release/formatReleaseDate.ts` | the date of a release, read in UTC so everybody sees the same day |
  | `Labels/` | English defaults, overrides and the number in a text |
- **A note is data from a pull request, not markup.** `react-markdown` with `skipHtml`; only `https`, `mailto` and `tel`
  links, opened in a new tab with `noopener noreferrer`; an image only when it is a file captured with the release, loaded
  from its own folder, or an `https` image the capture could not download. A relative path that climbs out of the folder
  is not an image.
- **Seeing is recorded when it was read, not when it was shown.** The dialog records the newest release it showed when it
  is closed. The page records a release when it is shown, and can be told not to. A failure to record never keeps a
  person from reading on; the release is simply unread again next time.
- **A release draws no frame of its own,** so the page, the dialog or a product's surface can hold it. Its heading level is
  a prop, so the heading structure stays right wherever it sits.
- **Styling is tokens, custom properties and stable classes.** Every color falls back to a `--cratis-*` token, and a
  `--playbill-*` property overrides it. A specification fails when a part the components draw has no style.
- **No MVVM.** The components keep no state beyond the selected release and the dismissed dialog, in the component.

## Examples

`Examples/` holds the code shown in the documentation. It is compiled with the package, and a specification fails when a
documentation page no longer contains an example exactly as it builds.
