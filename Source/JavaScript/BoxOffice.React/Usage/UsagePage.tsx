// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CurrentUsage } from '@cratis/showrunner.boxoffice';
import { useBoxOfficeContext } from '../Provider/BoxOfficeContext.js';
import { meterUsageModel } from './usageModels.js';
import { UsageView } from './UsageView.js';

/**
 * The usage page: what the tenant has used of each meter in the period it is in, against its allowance and budget. For
 * the people who maintain the product; the backend refuses everyone else. Render it inside a
 * {@link BoxOfficeProvider}.
 */
export const UsagePage = () => {
    const { labels, locale } = useBoxOfficeContext();
    const [usage] = CurrentUsage.use();
    const meters = usage.isSuccess ? (usage.data ?? []).map(standing => meterUsageModel(standing, labels, locale)) : [];

    return (
        <UsageView
            meters={meters}
            isLoading={usage.isPerforming && !usage.isSuccess}
            isUnauthorized={!usage.isAuthorized}
            hasFailed={!usage.isPerforming && usage.isAuthorized && !usage.isSuccess} />
    );
};
