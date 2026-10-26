// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { AccessDuration } from '@cratis/showrunner.usher';
import { UsherLabels } from '../Labels/index.js';

/**
 * One choice of how long support may sign in as the reporter.
 */
export interface DurationOption {
    [key: string]: unknown;

    /** The {@link AccessDuration}. */
    value: AccessDuration;

    /** What the reporter sees. */
    label: string;
}

/**
 * Gets the choices of how long support may sign in as the reporter.
 * @param labels The labels to show.
 * @returns The {@link DurationOption} for every {@link AccessDuration}, shortest first.
 */
export function durationOptions(labels: UsherLabels): DurationOption[] {
    return [
        { value: AccessDuration.oneHour, label: labels.access.oneHour },
        { value: AccessDuration.oneDay, label: labels.access.oneDay },
        { value: AccessDuration.sevenDays, label: labels.access.sevenDays }
    ];
}
