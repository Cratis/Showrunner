// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Button, IconButton } from '@cratis/components/Common';
import { BugIcon } from '../Icons.js';
import { useUsher } from '../Provider/useUsher.js';
import { reportButtonModel, UsherButtonSize } from './buttonModels.js';

/**
 * Props for {@link ReportButton}.
 */
export interface ReportButtonProps {
    /** `large` shows the icon and the text; `small` shows the icon, with the text as tooltip and accessible name. Defaults to `large`. */
    size?: UsherButtonSize;

    /** The error to report: `ErrorDetails` from `@cratis/showrunner.usher`, or anything thrown. The button reports a problem described by hand when not given. */
    error?: unknown;

    /** Additional class name for the button. */
    className?: string;
}

/**
 * Opens the report a problem dialog, or the report this error dialog when given an error, with where in the
 * application the button is.
 */
export const ReportButton = ({ size = 'large', error, className }: ReportButtonProps) => {
    const usher = useUsher();
    const reportsError = error !== undefined;
    const model = reportButtonModel(size, reportsError, usher.labels);
    const report = () => reportsError ? usher.reportError(error) : usher.reportIssue();

    return size === 'small'
        ? (
            <IconButton
                className={className}
                variant='ghost'
                icon={<BugIcon />}
                aria-label={model.accessibleName}
                tooltip={model.tooltip}
                onClick={report} />
        )
        : (
            <Button
                className={className}
                variant='outline'
                icon={<BugIcon />}
                label={model.text}
                onClick={report} />
        );
};
