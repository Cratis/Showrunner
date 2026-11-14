// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

const codeMarker = '\u0000';
const codeSpan = /(`+)([\s\S]*?[^`])\1(?!`)/g;
const urlInParentheses = String.raw`\((?:[^()]|\([^()]*\))*\)`;

/**
 * Turns the inline Markdown of an announcement title into the plain text a screen reader, a document title or an
 * `aria-label` should get: a link keeps its text and loses its address, emphasis and code lose their markers, an image
 * keeps its alternative text, and raw HTML tags are dropped.
 * @param source The title as written, possibly with inline Markdown.
 * @returns The text without Markdown, with white space collapsed.
 */
export const markdownToPlainText = (source: string | null | undefined): string => {
    if (!source) {
        return '';
    }

    const codes: string[] = [];
    const withoutCode = source.replace(codeSpan, (_, __, code: string) => {
        codes.push(code.trim());
        return `${codeMarker}${codes.length - 1}${codeMarker}`;
    });

    const plain = withoutCode
        .replace(new RegExp(String.raw`!\[([^\]]*)\]${urlInParentheses}`, 'g'), '$1')
        .replace(new RegExp(String.raw`\[([^\]]*)\]${urlInParentheses}`, 'g'), '$1')
        .replace(/<((?:https?:\/\/|mailto:|tel:)[^>\s]+)>/g, '$1')
        .replace(/<\/?[a-zA-Z][^>]*>/g, '')
        .replace(/(\*\*|__)(?=\S)([\s\S]*?\S)\1/g, '$2')
        .replace(/(\*)(?=\S)([\s\S]*?\S)\1/g, '$2')
        .replace(/(^|[^\p{L}\p{N}_])_(?=\S)([\s\S]*?\S)_(?![\p{L}\p{N}_])/gu, '$1$2')
        .replace(/\\([\\`*_{}[\]()#+\-.!~<>|])/g, '$1')
        .replace(new RegExp(`${codeMarker}(\\d+)${codeMarker}`, 'g'), (_, index: string) => codes[Number(index)]);

    return plain.replace(/\s+/g, ' ').trim();
};

/**
 * Whether there is anything to show in a Markdown text: the message of an announcement is optional, and a message of
 * only white space is the same as none.
 * @param source The Markdown as written, or nothing.
 * @returns True when there is text.
 */
export const hasMarkdown = (source: string | null | undefined): source is string => !!source && source.trim().length > 0;
