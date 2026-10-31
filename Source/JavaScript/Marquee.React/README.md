# @cratis/showrunner.marquee.react

The Marquee drop-in React component, the Showrunner announcements part: `<Marquee />` shows a product's announcements as
banners at the top and dialogs one at a time. Built on `@cratis/components` and the generated proxies in
`@cratis/showrunner.marquee`. How to use it: [Documentation/Marquee](../../../Documentation/Marquee/index.md).

## Component hierarchy

```
Marquee                      useAnnouncements, then the banner and one dialog
├── MarqueeBanner            role='status' live region, one row for each banner
│   └── MarqueeMarkdown      compact
└── MarqueeDialog            Cratis Components Dialog, one announcement at a time
    └── MarqueeMarkdown
```

`banner`, `dialog` and `markdown` are slots: a product replaces any of them. `useAnnouncements` gives a product the
announcements to draw a surface of its own.

## Architecture decisions

- **Decisions live in pure, specified modules; the components render and wire events.**

  | Module | Decides |
  | --- | --- |
  | `Selection/announcementSelection.ts` | which announcements are inside their window, which dialogs are still due, in which order |
  | `Selection/importancePresentation.ts` | the modifier and the icon name of an importance |
  | `Markdown/safeLink.ts` | which links may be followed |
  | `Labels/` | English defaults and overrides |

- **The backend does not apply the window.** The query returns announcements scheduled for later, and
  `useAnnouncements` compares each window with the clock every 60 seconds. The start is inclusive, the end exclusive.
- **A dialog closed is remembered in state, not in storage.** A dialog is closed for the session in the hook, so it is
  not offered again if the observable query sends its list again. `DialogOnce` is also acknowledged through the
  command, which is what keeps it from coming back on any device. A failed acknowledgement does not keep the dialog
  open; it returns in the next session.
- **Markdown is rendered as elements, never as HTML.** `react-markdown` with `skipHtml`, images disallowed, and every
  link passed through `isSafeLink` (`https`, `mailto`, `tel`). A link opens in a new tab with `noopener noreferrer`.
- **The banner region is always rendered**, because a live region that is added with its content is not reliably
  announced. It takes no space while it is empty.
- **Dialog focus lands on the title** (`DialogInitialFocus.Content`), so a key held when the dialog opened does not
  acknowledge it unread.
- **No MVVM.** The component keeps no state of its own beyond the clock and the closed ids, in one hook.

## Examples and stories

`Examples/` holds the code shown in the documentation. It is compiled with the package, and a specification fails when a
documentation page no longer contains an example exactly as it builds. `yarn storybook` runs the stories of the banner
and the dialog, one for each importance and display.
