// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useAnnouncements } from '@cratis/showrunner.marquee.react';

export const AnnouncementBell = () => {
    const { banners, dialogs, close } = useAnnouncements();

    return (
        <section aria-label='Announcements'>
            <p>{banners.length} standing</p>
            <ul>
                {dialogs.map(announcement => (
                    <li key={announcement.id.toString()}>
                        {announcement.title}
                        <button type='button' onClick={() => void close(announcement)}>Mark as read</button>
                    </li>
                ))}
            </ul>
        </section>
    );
};
