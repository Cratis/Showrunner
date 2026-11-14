// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ComponentType } from 'react';
import ReactMarkdown from 'react-markdown';
import { MarqueeLink } from './MarqueeLink.js';
import { safeLinkOrEmpty } from './safeLink.js';

/**
 * Props for {@link MarqueeInlineMarkdown}.
 */
export interface MarqueeInlineMarkdownProps {
    /** The inline Markdown source, such as the title of an announcement. */
    source: string;
}

const inlineElements = ['a', 'em', 'strong', 'code'];

/**
 * Renders the inline Markdown of an announcement title, `Maintenance tonight. [Read more here.](https://status.example.com)`,
 * as React elements inside a `span`: links, emphasis and code only. Block elements, such as paragraphs, headings, lists
 * and code blocks, are not rendered, and their text runs on in the line. It follows the same safety rules as
 * {@link MarqueeMarkdown}: raw HTML is dropped, images are left out, a link opens in a new tab with `noopener noreferrer`,
 * and only `https`, `mailto` and `tel` links are links at all.
 */
export const MarqueeInlineMarkdown: ComponentType<MarqueeInlineMarkdownProps> = ({ source }: MarqueeInlineMarkdownProps) => (
    <span className='marquee-inline-markdown'>
        <ReactMarkdown
            skipHtml
            allowedElements={inlineElements}
            unwrapDisallowed
            urlTransform={safeLinkOrEmpty}
            components={{ a: MarqueeLink }}>
            {source}
        </ReactMarkdown>
    </span>
);
