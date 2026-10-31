// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CratisComponentsProvider } from '@cratis/components';
import { EntitlementSummary, SeatStanding } from '@cratis/showrunner.boxoffice';
import { ReactNode } from 'react';
import { BoxOfficeProvider } from '../Provider/BoxOfficeProvider.js';
import { summaryOf } from './boxOfficeData.js';

/**
 * Props for {@link BoxOfficeHarness}.
 */
export interface BoxOfficeHarnessProps {
    /** What the tenant may do; a licensed tenant when not given. */
    entitlements?: EntitlementSummary;

    /** Where the user stands with the seats. */
    seat?: SeatStanding;

    /** Opens a link; nothing happens when not given. */
    openLink?: (url: string) => void;

    /** Gets the link to the customer portal; a fixed link when not given. */
    portalLink?: Parameters<typeof BoxOfficeProvider>[0]['portalLink'];

    /** What to show. */
    children?: ReactNode;
}

/**
 * Renders Box Office components with the entitlements and seat given instead of asking a backend, for specifications and
 * stories.
 */
export const BoxOfficeHarness = ({
    entitlements,
    seat,
    openLink = () => undefined,
    portalLink = () => Promise.resolve('https://portal.example.com/box-office?signed'),
    children
}: BoxOfficeHarnessProps) => (
    <CratisComponentsProvider>
        <BoxOfficeProvider
            entitlements={entitlements ?? summaryOf()}
            seat={seat}
            locale='en-US'
            openLink={openLink}
            portalLink={portalLink}>
            {children}
        </BoxOfficeProvider>
    </CratisComponentsProvider>
);
