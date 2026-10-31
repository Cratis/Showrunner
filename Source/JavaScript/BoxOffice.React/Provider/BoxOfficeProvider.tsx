// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ArcContext } from '@cratis/arc.react';
import { toast } from '@cratis/components/Notifications';
import { Guid } from '@cratis/fundamentals';
import { CurrentEntitlements, EntitlementSummary, Mine, PortalIntent, SeatStanding } from '@cratis/showrunner.boxoffice';
import { ReactNode, useContext, useMemo } from 'react';
import { entitlementsOf } from '../Entitlements/entitlementModel.js';
import { EntitlementsContext } from '../Entitlements/EntitlementsContext.js';
import { BoxOfficeLabelOverrides, resolveLabels } from '../Labels/index.js';
import { linkToPortal, openInNewTab } from '../Portal/portalLinks.js';
import { BoxOfficeContext, BoxOfficeContextValue } from './BoxOfficeContext.js';

/**
 * Props for {@link BoxOfficeProvider}.
 */
export interface BoxOfficeProviderProps {
    /** The labels that replace the English defaults, for instance to localize them. */
    labels?: BoxOfficeLabelOverrides;

    /** The locale to format dates and numbers for; the browser's when not given. */
    locale?: string;

    /**
     * The entitlements to use instead of asking the backend. For stories, specifications and previews; leave it out in
     * the application.
     */
    entitlements?: EntitlementSummary;

    /**
     * Where the user stands with the tenant's seats, to use instead of asking the backend. For stories,
     * specifications and previews; leave it out in the application.
     */
    seat?: SeatStanding;

    /** Opens a link to the customer portal. Defaults to opening it in a new tab. */
    openLink?: (url: string) => void;

    /**
     * Gets the link that opens the customer portal, instead of asking the backend for a signed one. For stories and
     * specifications; leave it out in the application.
     */
    portalLink?: (intent: PortalIntent, offer?: Guid) => Promise<string | undefined>;

    /** The application. */
    children?: ReactNode;
}

/**
 * Slots Box Office into an application: asks the backend what the signed-in user's tenant may do and where the user
 * stands with its seats, and gives {@link useEntitlements}, {@link RequiresModule}, the banners and the pages what they
 * need. Render it inside `<Arc>` and a `CratisComponentsProvider` with a toaster, once, around the shell.
 */
export const BoxOfficeProvider = ({ labels, locale, entitlements: preloaded, seat: preloadedSeat, openLink = openInNewTab, portalLink, children }: BoxOfficeProviderProps) => {
    const arc = useContext(ArcContext);
    const resolvedLabels = useMemo(() => resolveLabels(labels), [labels]);
    const [summaryResult, refreshSummary] = CurrentEntitlements.when(preloaded === undefined).use();
    const [seatResult, refreshSeat] = Mine.when(preloadedSeat === undefined).use();

    const summary = preloaded ?? (summaryResult.isSuccess && summaryResult.data?.modules ? summaryResult.data : undefined);
    const seat = preloadedSeat ?? (seatResult.isSuccess && seatResult.data?.status !== undefined ? seatResult.data : undefined);
    const entitlements = useMemo(() => entitlementsOf(summary), [summary]);

    const value = useMemo<BoxOfficeContextValue>(() => ({
        isProvided: true,
        labels: resolvedLabels,
        locale,
        seat,
        openPortal: async (intent: PortalIntent, offer?: Guid) => {
            try {
                const url = await (portalLink ? portalLink(intent, offer) : linkToPortal(arc, intent, offer));
                if (url) {
                    openLink(url);
                    return;
                }
            } catch {
                /* The toast below says what went wrong; the details are of no use to the user. */
            }

            toast.error({ title: resolvedLabels.common.portalFailed });
        },
        refresh: () => {
            void refreshSummary();
            void refreshSeat();
        }
    }), [resolvedLabels, locale, seat, arc, openLink, portalLink, refreshSummary, refreshSeat]);

    return (
        <BoxOfficeContext.Provider value={value}>
            <EntitlementsContext.Provider value={entitlements}>
                {children}
            </EntitlementsContext.Provider>
        </BoxOfficeContext.Provider>
    );
};
