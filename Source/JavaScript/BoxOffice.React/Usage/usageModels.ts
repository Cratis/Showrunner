// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { AllowancePeriod, MeterStanding, OverageBehavior } from '@cratis/showrunner.boxoffice';
import { BoxOfficeLabels, format } from '../Labels/index.js';
import { formatDay, formatQuantity } from '../formatting.js';

/** How a meter stands against what it may use. */
export type UsageLevel = 'ok' | 'warning' | 'atBudget' | 'atAllowance';

/** The share of an allowance used from which a meter is shown as running out. */
export const usageWarningShare = 0.8;

/**
 * What the usage page shows for one meter.
 */
export interface MeterUsageModel {
    /** The meter's standing. */
    readonly standing: MeterStanding;

    /** How much of the allowance is used, from 0 to 100; 0 when there is no allowance. */
    readonly percent: number;

    /** How the meter stands. */
    readonly level: UsageLevel;

    /** What is used against the allowance, in words. */
    readonly usedText: string;

    /** The allowance's period and when it starts over, in words. */
    readonly periodText: string;

    /** What happens beyond the allowance, in words. */
    readonly overageText: string;

    /** The budget, in words; nothing set when there is none. */
    readonly budgetText: string;

    /** The concurrency limit, in words; nothing set when there is none. */
    readonly concurrencyText: string;
}

/**
 * Works out how a meter stands.
 * @param standing The meter's standing from the backend.
 * @returns The {@link UsageLevel}.
 */
export function usageLevel(standing: MeterStanding): UsageLevel {
    if (standing.allowance > 0 && standing.used >= standing.allowance) {
        return 'atAllowance';
    }

    if (standing.budget !== undefined && standing.budget !== null && standing.used >= standing.budget) {
        return 'atBudget';
    }

    return standing.allowance > 0 && standing.used / standing.allowance >= usageWarningShare ? 'warning' : 'ok';
}

/**
 * Lays out how a meter stands for the usage page.
 * @param standing The meter's standing from the backend.
 * @param labels The labels to show.
 * @param locale The locale to format for; the browser's when not given.
 * @returns The {@link MeterUsageModel}.
 */
export function meterUsageModel(standing: MeterStanding, labels: BoxOfficeLabels, locale?: string): MeterUsageModel {
    const hasAllowance = standing.allowance > 0;
    const period = standing.period === AllowancePeriod.month ? labels.usage.month : labels.usage.day;
    const used = formatQuantity(standing.used, locale);

    return {
        standing,
        percent: hasAllowance ? Math.min(100, Math.round((standing.used / standing.allowance) * 100)) : 0,
        level: usageLevel(standing),
        usedText: hasAllowance
            ? format(labels.usage.usedOfAllowance, { used, allowance: formatQuantity(standing.allowance, locale), unit: standing.unit })
            : format(labels.usage.noAllowance, { used, unit: standing.unit }),
        periodText: hasAllowance
            ? `${period}, ${format(labels.usage.resets, { date: formatDay(standing.periodEnds, locale) })}`
            : period,
        overageText: standing.overage === OverageBehavior.park ? labels.usage.park : labels.usage.block,
        budgetText: standing.budget !== undefined && standing.budget !== null
            ? `${formatQuantity(standing.budget, locale)} ${standing.unit}`
            : labels.usage.notSet,
        concurrencyText: !standing.hasConcurrencyLimit
            ? ''
            : standing.concurrencyLimit !== undefined && standing.concurrencyLimit !== null
                ? formatQuantity(standing.concurrencyLimit, locale)
                : labels.usage.notSet
    };
}
