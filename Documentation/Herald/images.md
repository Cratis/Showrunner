---
title: Images
description: Embed images in Herald email templates using cid references, configure the images folder and understand the security boundary that rejects SVG files.
---

Images live in the images folder (`ImagesFolder`). A template refers to an image by its file name with the `cid:` scheme:

```html
<img src="cid:logo.png" alt="Ledger" width="120">
```

Herald finds each image the rendered HTML refers to and embeds it in the message as an inline part. The recipient sees it without anything being hosted, no remote content has to be allowed, and a private or local address can never end up in an email.

| Type | Embedded |
| --- | --- |
| PNG, JPEG, GIF | Yes. |
| SVG | No. Rendering fails with an error that names the file. |
| Anything else | No. Rendering fails. |

An image that is referred to and is not in the folder also fails the rendering, so a broken image is never sent.

## Why SVG is refused

Mail clients do not render SVG: Outlook and Gmail show a broken image. The usual remedy is to convert it to PNG when sending, but a converter means a rasterizing library with native components, which Herald does not take on for every application that sends a welcome email. Export a PNG of the logo, at twice the size you show it at for sharp screens, and refer to that.

File names in `cid:` references use letters, digits, dots, dashes and underscores, and are looked up directly in the images folder: a reference can not point outside it.
