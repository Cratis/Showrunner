// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { PlaybillHistoryDialog } from '@cratis/showrunner.playbill.react';
import { useState } from 'react';

export const WhatsNewMenuEntry = () => {
    const [open, setOpen] = useState(false);

    return (
        <>
            <button type='button' onClick={() => setOpen(true)}>What's new</button>
            <PlaybillHistoryDialog open={open} onClose={() => setOpen(false)} />
        </>
    );
};
