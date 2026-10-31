// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ForProduct } from '@cratis/showrunner.marquee.authoring';

export const AnnouncementList = () => {
    const [announcements] = ForProduct.use({ product: 'ledger' });

    return (
        <ul>
            {announcements.data.map(announcement => (
                <li key={announcement.id.toString()}>{announcement.title}</li>
            ))}
        </ul>
    );
};
