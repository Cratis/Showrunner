---
title: Templates
description: Author Herald email manifests and language-specific templates, declare accepted values and follow supported rendering constructs and language rules.
---

An email kind is a name such as `invitation` or `sign-up-declined`: lowercase letters, digits and dashes. The templates folder holds, for each kind, a manifest and one template per language:

```
Emails/
  invitation.json          the values the invitation email accepts
  invitation.en.html       English
  invitation.nb.html       Norwegian
  images/
    logo.png
```

## The manifest

`{kind}.json` declares every value the email accepts. A value is a name in dotted segments; a required value must be given, and an optional value that is not given is empty.

```json
{
  "values": [
    { "name": "invitation.link", "required": true },
    "invitation.expiresAt",
    "inviter.name",
    "inviter.message"
  ]
}
```

An entry can be a plain name for an optional value. A name can not be both a value and a group of values (`inviter` and `inviter.name`), and `look` is reserved for the values Herald provides.

## The template

A template starts with a header between two lines of three dashes, then the email-ready HTML. The header has a `subject` and, optionally, a `preview`, the text mail clients show beside the subject. Both can use values.

```html
---
subject: {{inviter.name}} invited you to Ledger
preview: Accept the invitation before {{date invitation.expiresAt "d MMMM"}}
---
<html>
<head>
  <style>
    .button { background-color: {{look.brandColor}}; color: #ffffff; padding: 12px 24px; text-decoration: none; }
  </style>
</head>
<body>
  <table role="presentation" width="600" align="center"><tr><td>
    <img src="cid:logo.png" alt="Ledger" width="120">
    <p>{{inviter.name}} invited you to Ledger.</p>
    {{#if inviter.message}}<blockquote>{{inviter.message}}</blockquote>{{/if}}
    <a class="button" href="{{invitation.link}}">Accept the invitation</a>
    <p>{{look.footer}}</p>
  </td></tr></table>
</body>
</html>
```

Author the HTML for email clients: a table layout, a fixed width of about 600 pixels, `role="presentation"` on layout tables and an `alt` on every image. Write styles in a `<style>` element with classes; Herald moves the rules into `style` attributes when it renders, because many clients ignore a style element. Media queries, such as `@media (prefers-color-scheme: dark)`, stay in the element for the clients that read them.

## What a template can use

Templates are Handlebars, rendered in strict mode with only three constructs:

| Construct | Meaning |
| --- | --- |
| `{{invitation.link}}` | A declared value, or one of `{{look.brandColor}}` and `{{look.footer}}`. HTML-encoded. |
| `{{#if inviter.message}}…{{else}}…{{/if}}` | Shows a section only when the value is not empty. |
| `{{date invitation.expiresAt "d MMMM"}}` | Formats a date, given as ISO 8601, in the culture of the email's language: `15 November` in English, `15. november` in Norwegian. |

Everything else is refused when the template is read, with a message that names the email and the placeholder: unescaped output (`{{{value}}}`), partials, other helpers, and any value the manifest does not declare. That is the point of the restriction: a template can not switch the encoding off, and a misspelled value is an error and not an empty space.

## Rendering rules

- A value the email does not declare, and a required value that is missing or blank, stop the email before it is queued.
- Every value is HTML-encoded in the body, including quotes inside attributes, so `"` in a link can not end the attribute. The subject and preview text are plain text and are not encoded.
- The subject is collapsed to one line.
- Herald generates the plain-text alternative from the HTML: paragraphs and list items on their own lines, links as `text (address)`, images as their `alt` text.

## Languages

The language comes from the request, or is the default language. If there is no template in it, Herald uses the language without its region (`nb-NO` uses `nb`), then the default language. The language of the template that was used is recorded in the facts, and is the culture dates are formatted in.
