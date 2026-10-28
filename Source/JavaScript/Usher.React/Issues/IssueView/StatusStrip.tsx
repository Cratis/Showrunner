// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { IssueState } from '@cratis/showrunner.usher';
import { useUsherLabels } from '../../Provider/UsherContext.js';
import { stateBadgeFor, statusStepOf, statusSteps } from '../issueStates.js';

/**
 * Props for {@link StatusStrip}.
 */
export interface StatusStripProps {
    /** The state the issue is in. */
    state: IssueState;
}

/**
 * Shows how far along an issue is, from reported to resolved. The step reached is marked as the current step.
 */
export const StatusStrip = ({ state }: StatusStripProps) => {
    const labels = useUsherLabels();
    const reached = statusStepOf(state);
    const closedOtherwise = state === IssueState.wontFix || state === IssueState.withdrawn;

    return (
        <ol className='usher-status-strip'>
            {statusSteps.map((step, index) => {
                const isCurrent = index === reached;
                const label = isCurrent && closedOtherwise ? stateBadgeFor(state, labels).label : stateBadgeFor(step, labels).label;
                return (
                    <li
                        key={step}
                        className='usher-status-strip__step'
                        data-reached={index <= reached || undefined}
                        data-current={isCurrent || undefined}
                        aria-current={isCurrent ? 'step' : undefined}>
                        {label}
                    </li>
                );
            })}
        </ol>
    );
};
