// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Message, Tag } from '@cratis/components/Display';
import { Offer, PortalIntent } from '@cratis/showrunner.boxoffice';
import { Entitlements } from '../Entitlements/entitlementModel.js';
import { PortalButton } from '../Portal/PortalButton.js';
import { useBoxOfficeContext } from '../Provider/BoxOfficeContext.js';
import { moduleRows, offerRows, planSummary } from './planModels.js';

/**
 * Props for {@link PlanView}.
 */
export interface PlanViewProps {
    /** What the tenant may do. */
    entitlements: Entitlements;

    /** The offers on sale. */
    offers: readonly Offer[];

    /** Whether the offers are still loading. */
    isLoading?: boolean;

    /** Whether the offers could not be loaded. */
    hasFailed?: boolean;
}

/**
 * The plan and modules of the tenant: where it stands, which modules it has, and what it can buy, with buttons that
 * open the customer portal.
 */
export const PlanView = ({ entitlements, offers, isLoading, hasFailed }: PlanViewProps) => {
    const { labels, locale } = useBoxOfficeContext();
    const modules = moduleRows(entitlements, labels, locale);
    const offerList = offerRows(offers, entitlements, labels, locale);
    const summary = planSummary(entitlements, labels);

    return (
        <section className='boxoffice-plan' aria-labelledby='boxoffice-plan-title'>
            <header className='boxoffice-page__header'>
                <h2 id='boxoffice-plan-title' className='boxoffice-page__title'>{labels.plan.title}</h2>
            </header>
            {summary && <p className='boxoffice-plan__summary'>{summary}</p>}
            {!entitlements.isLoading && !entitlements.canOpenPortal && <Message severity='info'>{labels.common.noPortal}</Message>}

            <h3 className='boxoffice-plan__heading'>{labels.plan.modulesTitle}</h3>
            {modules.length === 0
                ? <p className='boxoffice-plan__empty'>{entitlements.isLoading ? labels.common.loading : labels.plan.noModules}</p>
                : (
                    <ul className='boxoffice-plan__modules'>
                        {modules.map(module => (
                            <li key={module.standing.module} className='boxoffice-plan__module'>
                                <span className='boxoffice-plan__name'>{module.standing.name}</span>
                                <Tag severity={module.severity} value={module.accessText} />
                                {module.untilText && <span className='boxoffice-plan__until'>{module.untilText}</span>}
                                {module.canTry && <PortalButton intent={PortalIntent.try} label={labels.plan.try} variant='outline' size='small' />}
                            </li>
                        ))}
                    </ul>
                )}

            <h3 className='boxoffice-plan__heading'>{labels.plan.offersTitle}</h3>
            {hasFailed && <Message severity='error'>{labels.common.failed}</Message>}
            {!hasFailed && offerList.length === 0 && <p className='boxoffice-plan__empty'>{isLoading ? labels.common.loading : labels.plan.noOffers}</p>}
            <ul className='boxoffice-plan__offers'>
                {offerList.map(row => (
                    <li key={row.offer.id.toString()} className='boxoffice-plan__offer'>
                        <div className='boxoffice-plan__offer-text'>
                            <strong>{row.offer.name}</strong>
                            {row.seatsText && <span>{row.seatsText}</span>}
                            {row.moduleNames.length > 0 && <span>{labels.plan.includedModules}: {row.moduleNames.join(', ')}</span>}
                            {row.prices.map(price => <span key={price} className='boxoffice-plan__price'>{price}</span>)}
                        </div>
                        <PortalButton intent={row.intent} offer={row.offer.id} label={row.buttonLabel} />
                    </li>
                ))}
            </ul>
        </section>
    );
};
