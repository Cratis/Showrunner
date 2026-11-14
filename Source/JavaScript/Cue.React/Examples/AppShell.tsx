// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Arc } from '@cratis/arc.react';
import { CratisComponentsProvider } from '@cratis/components';
import { CueProvider, createHttpSpeech } from '@cratis/showrunner.cue.react';
import { ReactNode } from 'react';

// Created once, outside any component.
const speech = createHttpSpeech({
    speakUrl: '/api/speech',
    availableUrl: '/api/speech/available'
});

export const AppShell = ({ children }: { children: ReactNode }) => (
    <CratisComponentsProvider>
        <Arc>
            <CueProvider speech={speech}>
                {children}
            </CueProvider>
        </Arc>
    </CratisComponentsProvider>
);
