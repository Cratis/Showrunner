// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CueProvider } from '@cratis/showrunner.cue.react';
import { ReactNode } from 'react';

export const LocalizedCue = ({ children }: { children: ReactNode }) => (
    <CueProvider
        labels={{
            close: 'Lukk omvisningen',
            play: 'Spill av',
            pause: 'Pause',
            previousChapter: 'Forrige kapittel',
            nextChapter: 'Neste kapittel',
            chapters: 'Kapitler'
        }}>
        {children}
    </CueProvider>
);
