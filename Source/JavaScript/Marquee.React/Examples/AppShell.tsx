// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Arc } from '@cratis/arc.react';
import { CratisComponentsProvider } from '@cratis/components';
import { Marquee } from '@cratis/showrunner.marquee.react';
import { ReactNode } from 'react';

export const AppShell = ({ children }: { children: ReactNode }) => (
    <CratisComponentsProvider>
        <Arc>
            <Marquee />
            {children}
        </Arc>
    </CratisComponentsProvider>
);
