// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Marquee, MarqueeBannerProps } from '@cratis/showrunner.marquee.react';

const OneLineBanner = ({ announcements, labels, markdown: Markdown }: MarqueeBannerProps) => (
    <div role='status' aria-live='polite' aria-label={labels.banners}>
        {announcements.map(announcement => (
            <p key={announcement.id.toString()}>
                <strong>{announcement.title}</strong>
                {Markdown && <Markdown source={announcement.body} compact />}
            </p>
        ))}
    </div>
);

export const MarqueeWithOneLineBanner = () => <Marquee banner={OneLineBanner} />;
