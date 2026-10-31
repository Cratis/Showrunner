// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { OnSale } from '@cratis/showrunner.boxoffice';
import { useEntitlementsContext } from '../Entitlements/EntitlementsContext.js';
import { PlanView } from './PlanView.js';

/**
 * The plan and modules page: where the tenant stands with its plan, which modules it has, and the offers the operator
 * sells, with buttons that open the customer portal to buy, try or change plan. Render it inside a
 * {@link BoxOfficeProvider}.
 */
export const PlanPage = () => {
    const entitlements = useEntitlementsContext();
    const [offers] = OnSale.use();

    return (
        <PlanView
            entitlements={entitlements}
            offers={offers.isSuccess ? (offers.data ?? []) : []}
            isLoading={offers.isPerforming && !offers.isSuccess}
            hasFailed={!offers.isPerforming && !offers.isSuccess} />
    );
};
