// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ComponentProps } from 'react';

type LinkProps = ComponentProps<'a'>;

/**
 * A link in an announcement: it opens in a new tab with `noopener noreferrer`. A link that is not safe has no `href`
 * left, because the URL transform emptied it, and is only its text.
 */
export const MarqueeLink = ({ href, title, children }: LinkProps) => href
    ? <a href={href} title={title} target='_blank' rel='noopener noreferrer'>{children}</a>
    : <span>{children}</span>;
