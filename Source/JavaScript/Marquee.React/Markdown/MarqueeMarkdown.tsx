// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ComponentType } from 'react';
import ReactMarkdown from 'react-markdown';
import { MarqueeLink } from './MarqueeLink.js';
import { safeLinkOrEmpty } from './safeLink.js';

/**
 * Props for the component that renders the Markdown body of an announcement. Give your own to the `markdown` slot of
 * {@link Marquee}.
 */
export interface MarqueeMarkdownProps {
    /** The Markdown source. */
    source: string;

    /** Whether to render inline, as the banner does: paragraphs run together and take no space between them. */
    compact?: boolean;
}

/**
 * Renders the Markdown of an announcement as React elements, never as injected HTML: raw HTML in the source is
 * dropped, images are left out, a link opens in a new tab with `noopener noreferrer`, and only `https`, `mailto`
 * and `tel` links are links at all.
 */
export const MarqueeMarkdown: ComponentType<MarqueeMarkdownProps> = ({ source, compact = false }: MarqueeMarkdownProps) => (
    <div className={compact ? 'marquee-markdown marquee-markdown--compact' : 'marquee-markdown'}>
        <ReactMarkdown
            skipHtml
            disallowedElements={['img']}
            urlTransform={safeLinkOrEmpty}
            components={{ a: MarqueeLink }}>
            {source}
        </ReactMarkdown>
    </div>
);
