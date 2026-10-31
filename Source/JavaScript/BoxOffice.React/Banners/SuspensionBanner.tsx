// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Message } from '@cratis/components/Display';
import { CurrentStanding, PortalIntent, ReadOnlyReason, SuspensionCause } from '@cratis/showrunner.boxoffice';
import { useEntitlementsContext } from '../Entitlements/EntitlementsContext.js';
import { PortalButton } from '../Portal/PortalButton.js';
import { useBoxOfficeLabels } from '../Provider/BoxOfficeContext.js';
import { suspensionReason } from './bannerModels.js';

/**
 * Props for {@link SuspensionBannerView}.
 */
export interface SuspensionBannerViewProps {
    /** The cause the operator gave; the reason is left out when not known. */
    cause?: SuspensionCause;
}

/**
 * What the suspension banner shows, for the cause it is given.
 */
export const SuspensionBannerView = ({ cause }: SuspensionBannerViewProps) => {
    const labels = useBoxOfficeLabels();
    const reason = suspensionReason(cause, labels);

    return (
        <div className='boxoffice-banner'>
            <Message severity='error'>
                <div className='boxoffice-banner__text'>
                    <strong>{labels.suspension.title}</strong>
                    {reason && <span>{reason}</span>}
                    <span>{labels.suspension.resolve}</span>
                </div>
            </Message>
            <PortalButton intent={PortalIntent.buy} label={labels.suspension.openPortal} size='small' variant='outline' />
        </div>
    );
};

/**
 * Tells the user that the operator suspended the tenant and why, and that everything is read-only until it is lifted.
 * Nothing is shown when the tenant is not suspended. Render it once, above the application's pages.
 */
export const SuspensionBanner = () => {
    const entitlements = useEntitlementsContext();
    const isSuspended = entitlements.readOnlyReason === ReadOnlyReason.tenantSuspended;
    const [standing] = CurrentStanding.when(isSuspended).use();

    if (!isSuspended) {
        return null;
    }

    return <SuspensionBannerView cause={standing.isSuccess ? standing.data?.cause : undefined} />;
};
