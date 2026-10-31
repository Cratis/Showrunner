// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Message } from '@cratis/components/Display';
import { PortalIntent } from '@cratis/showrunner.boxoffice';
import { useEntitlementsContext } from '../Entitlements/EntitlementsContext.js';
import { PortalButton } from '../Portal/PortalButton.js';
import { useBoxOfficeContext } from '../Provider/BoxOfficeContext.js';
import { noLicenseModel } from './bannerModels.js';

/**
 * Tells a user who cannot use the product why, and what to do about it: the tenant is suspended, its trial ended, the
 * user has no seat, or the tenant has more people with seats than it has bought. Nothing is shown to a user with a
 * license. Render it where the application's content would be, or above it.
 */
export const NoLicense = () => {
    const entitlements = useEntitlementsContext();
    const { labels, seat } = useBoxOfficeContext();
    const model = noLicenseModel(entitlements, seat, labels);

    if (!model) {
        return null;
    }

    return (
        <div className='boxoffice-no-license' data-reason={model.reason}>
            <Message severity={model.reason === 'noSeat' || model.reason === 'overCapacity' ? 'warn' : 'error'}>
                <div className='boxoffice-banner__text'>
                    <strong>{model.title}</strong>
                    <span>{model.text}</span>
                </div>
            </Message>
            {model.reason !== 'noSeat' && <PortalButton intent={PortalIntent.buy} label={labels.noLicense.openPortal} variant='outline' />}
        </div>
    );
};
