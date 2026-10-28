// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { format } from '../Labels/index.js';
import { useUsherContext } from '../Provider/UsherContext.js';
import { formatLocation, LocationParts } from './formatLocation.js';

/**
 * Props for {@link SentWithReport}.
 */
export interface SentWithReportProps {
    /** Where in the application the reporter was. */
    location: LocationParts;

    /** How many recent requests the report carries. */
    correlationIdCount: number;
}

/**
 * Shows the reporter what is sent along with their report: where they were, the application version and how many
 * recent requests.
 */
export const SentWithReport = ({ location, correlationIdCount }: SentWithReportProps) => {
    const { labels, applicationName, applicationVersion } = useUsherContext();
    const parts = [
        formatLocation(location, labels.common.locationSeparator, labels.common.unknownLocation),
        [applicationName, applicationVersion].filter(part => part.length > 0).join(' '),
        correlationIdCount === 1 ? labels.common.oneRecentRequest : format(labels.common.recentRequests, { count: correlationIdCount })
    ].filter(part => part.length > 0);

    return (
        <section className='usher-sent-with-report' aria-label={labels.common.sentWithYourReport}>
            <span className='usher-sent-with-report__heading'>{labels.common.sentWithYourReport}</span>
            <span className='usher-sent-with-report__parts'>{parts.join(' · ')}</span>
        </section>
    );
};
