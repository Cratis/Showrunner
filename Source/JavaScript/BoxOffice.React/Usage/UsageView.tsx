// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Column, DataTableCore, DataTableStatus } from '@cratis/components/DataTables';
import { ProgressBar, Tag } from '@cratis/components/Display';
import { format } from '../Labels/index.js';
import { useBoxOfficeContext } from '../Provider/BoxOfficeContext.js';
import { MeterUsageModel, UsageLevel } from './usageModels.js';

/**
 * Props for {@link UsageView}.
 */
export interface UsageViewProps {
    /** How each meter stands. */
    meters: readonly MeterUsageModel[];

    /** Whether the usage is still loading. */
    isLoading?: boolean;

    /** Whether the usage could not be loaded because the user may not see it. */
    isUnauthorized?: boolean;

    /** Whether the usage could not be loaded. */
    hasFailed?: boolean;
}

const statusOf = (isLoading?: boolean, isUnauthorized?: boolean, hasFailed?: boolean) => {
    if (isUnauthorized) {
        return DataTableStatus.Unauthorized;
    }

    if (hasFailed) {
        return DataTableStatus.Failed;
    }

    return isLoading ? DataTableStatus.Loading : DataTableStatus.Ready;
};

/**
 * What a tenant has used of each meter in the period it is in, against its allowance and budget.
 */
export const UsageView = ({ meters, isLoading, isUnauthorized, hasFailed }: UsageViewProps) => {
    const { labels } = useBoxOfficeContext();
    const levelText: Record<UsageLevel, string | undefined> = {
        ok: undefined,
        warning: undefined,
        atBudget: labels.usage.atBudget,
        atAllowance: labels.usage.atAllowance
    };
    const showsConcurrency = meters.some(_ => _.standing.hasConcurrencyLimit);

    return (
        <section className='boxoffice-usage' aria-labelledby='boxoffice-usage-title'>
            <header className='boxoffice-page__header'>
                <h2 id='boxoffice-usage-title' className='boxoffice-page__title'>{labels.usage.title}</h2>
            </header>

            <DataTableCore<MeterUsageModel>
                data={[...meters]}
                dataKey='standing.meter'
                status={statusOf(isLoading, isUnauthorized, hasFailed)}
                emptyMessage={labels.usage.empty}
                loadingMessage={labels.common.loading}
                failureMessage={labels.common.failed}
                unauthorizedMessage={labels.common.unauthorized}>
                <Column<MeterUsageModel>
                    header={labels.usage.meterColumn}
                    body={meter => (
                        <span className='boxoffice-usage__meter'>
                            <strong>{meter.standing.name}</strong>
                            <span className='boxoffice-usage__period'>{meter.periodText}</span>
                        </span>
                    )} />
                <Column<MeterUsageModel>
                    header={labels.usage.usedColumn}
                    body={meter => (
                        <span className={`boxoffice-usage__used boxoffice-usage__used--${meter.level}`}>
                            <span>{meter.usedText}</span>
                            {meter.standing.allowance > 0 && (
                                <ProgressBar
                                    value={meter.percent}
                                    showValue={false}
                                    aria-label={format(labels.usage.progress, { meter: meter.standing.name })} />
                            )}
                            {levelText[meter.level] && <Tag severity={meter.level === 'atAllowance' ? 'danger' : 'warn'} value={levelText[meter.level]} />}
                            <span className='boxoffice-usage__overage'>{meter.overageText}</span>
                        </span>
                    )} />
                <Column<MeterUsageModel> header={labels.usage.budgetColumn} body={meter => meter.budgetText} />
                {showsConcurrency && <Column<MeterUsageModel> header={labels.usage.concurrencyColumn} body={meter => meter.concurrencyText} />}
            </DataTableCore>
        </section>
    );
};
