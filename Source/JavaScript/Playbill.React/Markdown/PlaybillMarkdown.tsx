// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ComponentProps, ComponentType } from 'react';
import ReactMarkdown from 'react-markdown';
import { resolveAssetUrl } from './assetUrl.js';
import { PlaybillLink } from './PlaybillLink.js';
import { safeLinkOrEmpty } from './safeLink.js';

/**
 * Props for the component that renders the Markdown of a release note. Give your own to the `markdown` slot of
 * {@link Playbill}, {@link PlaybillDialog} or {@link ReleaseNoteView}.
 */
export interface PlaybillMarkdownProps {
    /** The Markdown source. */
    source: string;
    /** Where the files of the release are served from, so an image written as `hero.png` can be loaded. */
    assetBase?: string;
}

type ImageProps = ComponentProps<'img'>;

/**
 * Renders the Markdown of a release note as React elements, never as injected HTML: raw HTML in the source is dropped,
 * a link opens in a new tab with `noopener noreferrer`, and only `https`, `mailto` and `tel` links are links at all.
 * Images are the ones captured with the release, loaded from its own folder, and `https` images the capture could not
 * download; every other image is left out.
 */
export const PlaybillMarkdown: ComponentType<PlaybillMarkdownProps> = ({ source, assetBase }: PlaybillMarkdownProps) => (
    <div className='playbill-markdown'>
        <ReactMarkdown
            skipHtml
            urlTransform={(url, key) => key === 'src' ? resolveAssetUrl(url, assetBase) : safeLinkOrEmpty(url)}
            components={{
                a: PlaybillLink,
                img: ({ src, alt }: ImageProps) => src ? <img src={src} alt={alt ?? ''} loading='lazy' /> : <></>
            }}>
            {source}
        </ReactMarkdown>
    </div>
);
