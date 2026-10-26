// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';
import { StackFrame } from '@cratis/showrunner.usher';
import { format } from '../Labels/index.js';
import { useUsherLabels } from '../Provider/UsherContext.js';

/**
 * Props for {@link TechnicalDetails}.
 */
export interface TechnicalDetailsProps {
    /** The type of the error; empty when not known. */
    errorType: string;

    /** The correlation ids sent with the report. */
    correlationIds: Guid[];

    /** The stack frames of the error, top frame first. */
    stackFrames: StackFrame[];
}

/**
 * The technical details of an error, collapsed until the reporter opens them.
 */
export const TechnicalDetails = ({ errorType, correlationIds, stackFrames }: TechnicalDetailsProps) => {
    const labels = useUsherLabels();

    return (
        <details className='usher-technical-details'>
            <summary className='usher-technical-details__summary'>{labels.reportError.technicalDetails}</summary>
            <dl className='usher-technical-details__list'>
                {errorType.length > 0 && (
                    <>
                        <dt>{labels.reportError.errorType}</dt>
                        <dd><code>{errorType}</code></dd>
                    </>
                )}
                {correlationIds.length > 0 && (
                    <>
                        <dt>{labels.reportError.correlationId}</dt>
                        <dd><code>{correlationIds[0].toString()}</code></dd>
                    </>
                )}
                {stackFrames.length > 0 && (
                    <>
                        <dt>{labels.reportError.stackFrames}</dt>
                        <dd>
                            <ol className='usher-technical-details__frames'>
                                {stackFrames.map((frame, index) => (
                                    <li key={index}><code>{format(labels.reportError.stackFrame, { method: frame.method, file: frame.file, line: frame.line })}</code></li>
                                ))}
                            </ol>
                        </dd>
                    </>
                )}
            </dl>
        </details>
    );
};
