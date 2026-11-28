// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

const imageTag = /<img\b[^>]*\balt="([^"]*)"[^>]*\/?>/gi;
const markdownLink = /\[([^\]]*)\]\([^)]*\)/g;

/**
 * Replaces an attached image or file embedded as markdown/HTML with a short plain-text marker.
 *
 * {@link ChatConversation} from `@cratis/components/Chat`, which the issue conversation uses, renders a message's
 * body as plain text - it has no markdown or HTML hook. Without this, a reporter who attached a screenshot would
 * see the literal `<img ... />` tag when looking back at their own issue. This is a display-only transform for
 * that one view: the stored text is left untouched everywhere else, including the support desk, which does render
 * the real image.
 * @param text The text as stored, possibly carrying an embedded attachment.
 * @returns The text with every embedded attachment replaced by a marker; unchanged when there is none.
 */
export function summarizeAttachmentsInText(text: string): string {
    return text
        .replace(imageTag, (_, name: string) => `📎 ${name}`)
        .replace(markdownLink, (_, name: string) => `📎 ${name}`);
}
