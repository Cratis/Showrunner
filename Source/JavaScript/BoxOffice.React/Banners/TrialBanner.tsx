// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Message } from '@cratis/components/Display';
import { PortalIntent } from '@cratis/showrunner.boxoffice';
import { useEntitlementsContext } from '../Entitlements/EntitlementsContext.js';
import { PortalButton } from '../Portal/PortalButton.js';
import { useBoxOfficeLabels } from '../Provider/BoxOfficeContext.js';
import { trialBannerModel } from './bannerModels.js';

/**
 * Tells the user how long the tenant's trial runs, or that it has closed, with a button to buy. Nothing is shown when
 * the tenant is not on a trial. Render it once, above the application's pages.
 */
export const TrialBanner = () => {
    const entitlements = useEntitlementsContext();
    const labels = useBoxOfficeLabels();
    const model = trialBannerModel(entitlements, labels);

    if (!model) {
        return null;
    }

    return (
        <div className='boxoffice-banner'>
            <Message severity={model.severity}>{model.text}</Message>
            <PortalButton intent={PortalIntent.buy} label={labels.trialBanner.buy} size='small' />
        </div>
    );
};
