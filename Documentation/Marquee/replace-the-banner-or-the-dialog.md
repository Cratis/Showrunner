---
title: Replace the banner or the dialog
description: How to draw announcements yourself - give Marquee your own banner, dialog or Markdown renderer through its slots, or take the announcements with useAnnouncements and draw any surface.
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
import { Marquee, MarqueeBannerProps } from '@cratis/showrunner.marquee.react';

const OneLineBanner = ({ announcements, labels, markdown: Markdown }: MarqueeBannerProps) => (
    <div role='status' aria-live='polite' aria-label={labels.banners}>
        {announcements.map(announcement => (
            <p key={announcement.id.toString()}>
                <strong>{announcement.title}</strong>
                {Markdown && <Markdown source={announcement.body} compact />}
            </p>
        ))}
    </div>
);

export const MarqueeWithOneLineBanner = () => <Marquee banner={OneLineBanner} />;
```

A banner can not be closed, so there is nothing to call.

## Replace the dialog

The component you give is rendered with one announcement at a time. Call `onClose` when the person is done: Marquee acknowledges a `DialogOnce` through the command and opens the next. `hasMore` says whether another follows, so the button can offer *Next* instead of *Got it*:

```tsx
import { Marquee, MarqueeDialogProps } from '@cratis/showrunner.marquee.react';

const SheetDialog = ({ announcement, hasMore, onClose, labels, markdown: Markdown }: MarqueeDialogProps) => (
    <dialog open aria-label={announcement.title}>
        <h2>{announcement.title}</h2>
        {Markdown && <Markdown source={announcement.body} />}
        <button type='button' onClick={onClose}>{hasMore ? labels.next : labels.acknowledge}</button>
    </dialog>
);

export const MarqueeWithSheetDialog = () => <Marquee dialog={SheetDialog} />;
```

A dialog you draw is yours to make accessible: trap focus, close on Escape and name it. The default dialog is the Cratis Components `Dialog`, which does all three.

## Replace the Markdown renderer

The default renderer is safe by construction: raw HTML is dropped, images are left out and only `https`, `mailto` and `tel` links are links. A renderer you give replaces that, so keep the same care. When the safe thing is to show the text as it was written, say so:

```tsx
import { Marquee, MarqueeMarkdownProps } from '@cratis/showrunner.marquee.react';

const PlainText = ({ source }: MarqueeMarkdownProps) => <span>{source}</span>;

export const MarqueeWithPlainText = () => <Marquee markdown={PlainText} />;
```

If you keep the default renderer for your own surface, `MarqueeMarkdown` and `isSafeLink` are exported.

## Draw any surface with `useAnnouncements`

`useAnnouncements` observes the announcements, applies the windows against the clock every 60 seconds and hands you what is to be shown. It must be used inside `<Arc>`. Do not also mount `<Marquee />` for the same announcements, or the person sees them twice:

```tsx
import { useAnnouncements } from '@cratis/showrunner.marquee.react';

export const AnnouncementBell = () => {
    const { banners, dialogs, close } = useAnnouncements();

    return (
        <section aria-label='Announcements'>
            <p>{banners.length} standing</p>
            <ul>
                {dialogs.map(announcement => (
                    <li key={announcement.id.toString()}>
                        {announcement.title}
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
