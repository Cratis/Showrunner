// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Button, IconButton } from '@cratis/components/Common';
import { Badge } from '@cratis/components/Display';
import { MyIssues } from '@cratis/showrunner.usher';
import { InboxIcon } from '../Icons.js';
import { waitingCount } from '../Issues/MyIssuesList.js';
import { UsherBoundary } from '../Provider/UsherBoundary.js';
import { useUsher } from '../Provider/useUsher.js';
import { myIssuesButtonModel, UsherButtonSize } from './buttonModels.js';

/**
 * Props for {@link MyIssuesButton}.
 */
export interface MyIssuesButtonProps {
    /** `large` shows the icon and the text; `small` shows the icon, with the text as tooltip and accessible name. Defaults to `large`. */
    size?: UsherButtonSize;

    /** Additional class name for the button. */
    className?: string;
}

/**
 * Opens the my issues side sheet. A badge counts the issues support waits for the user on, and the accessible name
 * says how many.
 */
export const MyIssuesButton = (props: MyIssuesButtonProps) => {
    const usher = useUsher();

    return (
        <UsherBoundary>
            <MyIssuesButtonContent {...props} onOpen={usher.showMyIssues} />
        </UsherBoundary>
    );
};

const MyIssuesButtonContent = ({ size = 'large', className, onOpen }: MyIssuesButtonProps & { onOpen: () => void }) => {
    const usher = useUsher();
    const [result] = MyIssues.use();

    return <MyIssuesButtonView size={size} className={className} waiting={waitingCount(result.data ?? [])} labels={usher.labels} onOpen={onOpen} />;
};

/**
 * Props for {@link MyIssuesButtonView}.
 */
export interface MyIssuesButtonViewProps extends MyIssuesButtonProps {
    /** How many issues support waits for the user on. */
    waiting: number;

    /** The labels to show. */
    labels: ReturnType<typeof useUsher>['labels'];

    /** Opens the my issues side sheet. */
    onOpen: () => void;
}

/**
 * Renders the my issues button for a given number of issues support waits for the user on.
 */
export const MyIssuesButtonView = ({ size = 'large', className, waiting, labels, onOpen }: MyIssuesButtonViewProps) => {
    const model = myIssuesButtonModel(size, waiting, labels);
    const badge = waiting > 0 && (
        <span className='usher-my-issues-button__badge' aria-hidden='true'>
            <Badge value={waiting} severity='warn' size='small' />
        </span>
    );

    return (
        <span className={['usher-my-issues-button', className].filter(Boolean).join(' ')} data-size={size}>
            {size === 'small'
                ? <IconButton variant='ghost' icon={<InboxIcon />} aria-label={model.accessibleName} tooltip={model.tooltip} onClick={onOpen} />
                : <Button variant='outline' icon={<InboxIcon />} label={model.text} aria-label={model.accessibleName} onClick={onOpen} />}
            {badge}
        </span>
    );
};
