// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// A qualified issue reference (`owner/repo#123`) after the start of a line, whitespace or an opening parenthesis.
const qualifiedIssuePattern = /(^|[\s(])([A-Za-z\d][A-Za-z\d-]*\/[A-Za-z\d._-]+)#(\d+)\b/g;

// A bare issue reference (`#123`) in the same places. A heading never matches, because `#` is followed by a space, and a
// reference inside a link or an address is preceded by a character that does not match.
const bareIssuePattern = /(^|[\s(])#(\d+)\b/g;

/**
 * Turns the issue references in a note into links: `#123` to an issue of the repository the notes belong to, and
 * `owner/repo#123` to that repository's.
 * @param markdown - The Markdown of the note.
 * @param repository - The address of the repository `#123` belongs to, such as `https://github.com/Cratis/Studio`.
 * @returns The Markdown with the references as links.
 */
export const linkIssueReferences = (markdown: string, repository: string): string => {
    const home = repository.replace(/\/+$/, '');
    return markdown
        .replace(qualifiedIssuePattern, (_match, prefix: string, name: string, number: string) =>
            `${prefix}[${name}#${number}](https://github.com/${name}/issues/${number})`)
        .replace(bareIssuePattern, (_match, prefix: string, number: string) =>
            `${prefix}[#${number}](${home}/issues/${number})`);
};
