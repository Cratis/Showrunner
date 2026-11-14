// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import React, { type ComponentProps } from 'react';
import ReactMarkdown from 'react-markdown';
import { safeUrl } from './safeUrl.js';

/**
 * Props for {@link CueMarkdown}.
 */
export interface CueMarkdownProps {
    /** The Markdown source of a chapter. */
    source: string;
}

const Link = ({ href, title, children }: ComponentProps<'a'>) => href
    ? <a href={href} title={title} target='_blank' rel='noopener noreferrer'>{children}</a>
    : <span>{children}</span>;

/**
 * Renders a chapter's Markdown as React elements, never as injected HTML: raw HTML in the source is dropped, a link opens
 * in a new tab with `noopener noreferrer`, and an address that would run script is not an address at all. Screenshots and
 * animations are kept, since a written chapter shows what the narration points at.
 */
export const CueMarkdown = ({ source }: CueMarkdownProps) => (
    <div className='cue-markdown'>
        <ReactMarkdown skipHtml urlTransform={safeUrl} components={{ a: Link }}>
            {source}
        </ReactMarkdown>
    </div>
);
