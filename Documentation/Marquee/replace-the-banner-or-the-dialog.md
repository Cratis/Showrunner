---
title: Replace the banner or the dialog
description: Customize Marquee announcements with banner, dialog and Markdown renderer slots, or use useAnnouncements to draw your own announcement surface.
---

`<Marquee />` is made of three parts you can replace and one hook you can use instead. The windows, the order of dialogs, the acknowledgement and the safety of links stay in Marquee either way.

| You want | Use |
| --- | --- |
| Another banner | The `banner` slot |
| Another dialog | The `dialog` slot |
| Another Markdown renderer, for both | The `markdown` slot |
| A surface that is neither, such as a bell or an inbox | `useAnnouncements` |

## Replace the banner

The component you give is rendered once, with every banner to show, the most important first. It is rendered with an empty list when there is none, so it can keep a live region in the page that is announced when the first one arrives. It also gets the resolved `labels` and the Markdown renderer:

```tsx
import { hasMarkdown, Marquee, MarqueeBannerProps, MarqueeInlineMarkdown } from '@cratis/showrunner.marquee.react';

const OneLineBanner = ({ announcements, labels, markdown: Markdown }: MarqueeBannerProps) => (
    <div role='status' aria-live='polite' aria-label={labels.banners}>
        {announcements.map(announcement => (
            <p key={announcement.id.toString()}>
                <strong><MarqueeInlineMarkdown source={announcement.title} /></strong>
                {Markdown && hasMarkdown(announcement.body) && <Markdown source={announcement.body} compact />}
            </p>
        ))}
    </div>
);

export const MarqueeWithOneLineBanner = () => <Marquee banner={OneLineBanner} />;
```

A banner can not be closed, so there is nothing to call.

The title is inline Markdown, so draw it with `MarqueeInlineMarkdown`, which follows the same safety rules as the Markdown renderer, and a message is optional, so check `hasMarkdown(announcement.body)` before you draw it. The `markdown` slot renders the message only; the title always goes through `MarqueeInlineMarkdown`. The default banner shows the title only, with a button to expand the message inline and a tooltip with it on hover and focus; the button is not drawn when there is no message. Its texts are the `expand` and `collapse` labels.

## Replace the dialog

The component you give is rendered with one announcement at a time. Call `onClose` when the person is done: Marquee acknowledges a `DialogOnce` through the command and opens the next. `hasMore` says whether another follows, so the button can offer *Next* instead of *Got it*:

```tsx
import { hasMarkdown, markdownToPlainText, Marquee, MarqueeDialogProps, MarqueeInlineMarkdown } from '@cratis/showrunner.marquee.react';

const SheetDialog = ({ announcement, hasMore, onClose, labels, markdown: Markdown }: MarqueeDialogProps) => (
    <dialog open aria-label={markdownToPlainText(announcement.title)}>
        <h2><MarqueeInlineMarkdown source={announcement.title} /></h2>
        {Markdown && hasMarkdown(announcement.body) && <Markdown source={announcement.body} />}
        <button type='button' onClick={onClose}>{hasMore ? labels.next : labels.acknowledge}</button>
    </dialog>
);

export const MarqueeWithSheetDialog = () => <Marquee dialog={SheetDialog} />;
```

A dialog you draw is yours to make accessible: trap focus, close on Escape and name it. The default dialog is the Cratis Components `Dialog`, which does all three.

A title can be Markdown, but a name, an `aria-label` or a document title is plain text: use `markdownToPlainText(announcement.title)`, which keeps the text of a link and drops the rest. The default dialog does this for its heading, and shows a title that uses Markdown a second time with its links at the top of the content. It shows the message only when there is one.

## Replace the Markdown renderer

The default renderer is safe by construction: raw HTML is dropped, images are left out and only `https`, `mailto` and `tel` links are links. A renderer you give replaces that, so keep the same care. When the safe thing is to show the text as it was written, say so:

```tsx
import { Marquee, MarqueeMarkdownProps } from '@cratis/showrunner.marquee.react';

const PlainText = ({ source }: MarqueeMarkdownProps) => <span>{source}</span>;

export const MarqueeWithPlainText = () => <Marquee markdown={PlainText} />;
```

If you keep the default renderer for your own surface, `MarqueeMarkdown`, `MarqueeInlineMarkdown` (links, emphasis and code only, for titles) and `isSafeLink` are exported.

## Draw any surface with `useAnnouncements`

`useAnnouncements` observes the announcements, applies the windows against the clock every 60 seconds and hands you what is to be shown. It must be used inside `<Arc>`. Do not also mount `<Marquee />` for the same announcements, or the person sees them twice:

```tsx
import { markdownToPlainText, useAnnouncements } from '@cratis/showrunner.marquee.react';

export const AnnouncementBell = () => {
    const { banners, dialogs, close } = useAnnouncements();

    return (
        <section aria-label='Announcements'>
            <p>{banners.length} standing</p>
            <ul>
                {dialogs.map(announcement => (
                    <li key={announcement.id.toString()}>
                        {markdownToPlainText(announcement.title)}
                        <button type='button' onClick={() => void close(announcement)}>Mark as read</button>
                    </li>
                ))}
            </ul>
        </section>
    );
};
```

`close` acknowledges a `DialogOnce` through the command, so it is gone on every device, and closes a `DialogEverySession` for this session only. A closed dialog is not offered again in the session even if the announcements are observed again. If sending the acknowledgement fails, the dialog is still closed for the session and returns in the next.

The hook also takes `checkInterval`, in milliseconds.
