---
title: Customize the release-notes experience
description: Restyle Playbill components with tokens and part classes, replace parts, change texts, build your own surface with hooks or read history elsewhere.
---
Playbill draws with Cratis Components and takes every color from the `--cratis-*` tokens, so your theme and dark mode apply with no work. Everything past that is a choice, from a few custom properties to drawing the whole thing yourself.

## Style it

Set any of these custom properties on an ancestor of the components. Each falls back to the Cratis token, so you set only what you want to change:

| Property | Controls |
| --- | --- |
| `--playbill-accent` | The current version, the selected release, the unread tag and the badge count. |
| `--playbill-accent-text` | The text on top of the accent. |
| `--playbill-surface` | The hover background of a release and of the badge. |
| `--playbill-border` | The lines between releases. |
| `--playbill-text` | The text. |
| `--playbill-muted` | Dates, summaries and secondary text. |
| `--playbill-radius` | The corners of tags, releases and the badge. |
| `--playbill-added`, `--playbill-changed`, `--playbill-fixed`, `--playbill-removed`, `--playbill-security`, `--playbill-deprecated`, `--playbill-other` | The marker beside each section. |

```css
.release-notes {
    --playbill-accent: #7c3aed;
    --playbill-radius: 0.75rem;
    --playbill-security: #b91c1c;
}
```

Every part also has a stable class, so you can style it directly: `playbill`, `playbill__list`, `playbill-item`, `playbill-release`, `playbill-release__version`, `playbill-release__lead`, `playbill-section`, `playbill-section--added` and the other sections, `playbill-tag`, `playbill-markdown`, `playbill-dialog`, `playbill-badge` and `playbill-badge__count`. The selected release carries `playbill-item--selected` and `aria-current`, and an unread badge carries `playbill-badge--unread`.

## Change the texts

Every text is a label. Give the ones you want to change, and the rest stay English. `{count}` in a text is replaced with the number:

```tsx
import { Playbill } from '@cratis/showrunner.playbill.react';

export const LocalizedPlaybill = () => (
    <Playbill
        locale='nb-NO'
        labels={{
            releases: 'Utgivelser',
            released: 'Utgitt',
            unread: 'Ny',
            current: 'Gjeldende',
            added: 'Nytt',
            changed: 'Endret',
            fixed: 'Rettet',
            loading: 'Henter utgivelsesnotater…'
        }} />
);
```

`locale` sets the language of the dates. A release date is always read in UTC, so every person sees the same day wherever they are. The labels cover the headings of the six standard sections; a note that uses another heading shows it as it was written.

## Replace a part

The page and the dialog take slots. A slot is a component that gets what the default one gets.

`release` replaces how one release is drawn. It gets the release with its sections, the folder its files are served from, the labels, and whether the release is unread or current:

```tsx
import { Playbill, ReleaseNoteViewProps } from '@cratis/showrunner.playbill.react';

const OwnRelease = ({ release }: ReleaseNoteViewProps) => (
    <article>
        <h2>Version {release.version}</h2>
        {release.sections.map(section => (
            <details key={section.name} open={section.name === 'Added'}>
                <summary>{section.name}</summary>
                <pre>{section.markdown}</pre>
            </details>
        ))}
    </article>
);

export const OwnReleasePage = () => (
    <Playbill release={OwnRelease} />
);
```

`markdown` replaces the renderer the default release view uses:

```tsx
import { Playbill, PlaybillMarkdownProps } from '@cratis/showrunner.playbill.react';

const PlainText = ({ source }: PlaybillMarkdownProps) => (
    <pre>{source}</pre>
);

export const PlainTextNotes = () => (
    <Playbill markdown={PlainText} />
);
```

The default renderer never injects HTML. Raw HTML in a note is dropped, links open in a new tab with `noopener noreferrer`, and only `https`, `mailto` and `tel` links are links. An image is shown only when it is a file captured with the release, loaded from its own folder, or an `https` image the capture could not download. If you write your own renderer, keep those rules: a note is data from a pull request, not markup you trust.

## Build your own surface

`usePlaybill` gives the history and what the person has seen. It is what the components use:

```tsx
import { usePlaybill } from '@cratis/showrunner.playbill.react';

export const OwnSurface = () => {
    const { releases, unread, unreadVersions, markSeen } = usePlaybill();
    const newest = releases[0];

    return (
        <aside>
            <strong>{unread} new</strong>
            {newest && <p>{newest.version}: {newest.summary}</p>}
            <button type='button' onClick={() => void markSeen(unreadVersions[0])} disabled={unread === 0}>
                Mark as read
            </button>
        </aside>
    );
};
```

`usePlaybill` is `useReleases` and `usePlaybillStatus` together. Use `useReleases` alone where there is no Arc context or no signed-in person, such as a public page on an application with `AllowAnonymous`. `useRelease(version)` and `useReleaseNotes(versions)` read the notes themselves. All three components also read and record what the person has seen, so they need the backend package and Arc's context; the hooks that only read the history need neither.

## Link the issues

Release notes mention the issues they deliver, as `#123` or `owner/repo#123`. Give `issues` the address of the repository the bare form belongs to and Playbill turns both into links; without it they stay text:

```tsx
<Playbill issues='https://github.com/Cratis/Studio' />
```

`issues` is accepted by `Playbill`, `PlaybillDialog` and `PlaybillHistoryDialog`. A product whose issues moved to another repository can give a function that answers for each version, so an old release keeps pointing at the repository its numbers belong to:

```tsx
<Playbill issues={version => version < '2.0.0' ? 'https://github.com/Cratis/Old' : 'https://github.com/Cratis/New'} />
```

A custom `release` receives the answer as `issueRepository`.

## Read from another endpoint

By default the components read `/.cratis/playbill/v1` of the page's own origin. `PlaybillProvider` changes where, or how the request is made, for example to add a header:

```tsx
import { Playbill, PlaybillProvider } from '@cratis/showrunner.playbill.react';

const withTenantHeader: typeof fetch = (input, init) => fetch(input, {
    ...init,
    headers: { ...init?.headers, 'X-Tenant': 'acme' }
});

export const OwnEndpoint = () => (
    <PlaybillProvider basePath='/api/acme/.cratis/playbill/v1' fetcher={withTenantHeader}>
        <Playbill />
    </PlaybillProvider>
);
```

Give it a `PlaybillClient` of your own to take over completely.
