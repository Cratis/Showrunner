// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CheckboxField, DropdownField, ToggleSwitchField } from '@cratis/components/CommandForm';
import { AccessDuration } from '@cratis/showrunner.usher';
import { useId } from 'react';
import { useUsherLabels } from '../Provider/UsherContext.js';
import { AccessChoice } from './AccessChoice.js';
import { AccessNotice } from './AccessNotice.js';
import { durationOptions } from './durationOptions.js';

/**
 * The command values {@link AccessFields} bind to.
 */
export interface IdentityAndAccess {
    /** Whether to let support know who the reporter is. */
    shareIdentity: boolean;
    /** Whether to let support sign in as the reporter. */
    grantAccess: boolean;
    /** For how long. */
    accessDuration: AccessDuration;
}

/**
 * Props for {@link AccessFields}.
 */
export interface AccessFieldsProps {
    /** The reporter's {@link AccessChoice}, as the form follows it. */
    choice: AccessChoice;
}

/**
 * The identity and access choices of a report: let support know who the reporter is, and let support sign in as
 * them for a while. Granting access always shares who the reporter is, so the checkbox is checked and disabled while
 * access is granted, and the notice says until when support can act as the reporter.
 */
export const AccessFields = <TCommand extends IdentityAndAccess>({ choice }: AccessFieldsProps) => {
    const labels = useUsherLabels();
    const shareDescriptionId = useId();
    const grantDescriptionId = useId();
    const noticeId = useId();

    return (
        <div className='usher-access-fields'>
            <div className='usher-access-fields__choice'>
                <CheckboxField<TCommand>
                    value={command => command.shareIdentity}
                    label={labels.access.shareIdentity}
                    aria-describedby={choice.grantAccess ? `${shareDescriptionId} ${noticeId}` : shareDescriptionId}
                    pt={{ input: { disabled: choice.grantAccess } }} />
                <p id={shareDescriptionId} className='usher-access-fields__description'>{labels.access.shareIdentityDescription}</p>
            </div>
            <div className='usher-access-fields__choice'>
                <ToggleSwitchField<TCommand>
                    value={command => command.grantAccess}
                    label={labels.access.grantAccess}
                    aria-describedby={grantDescriptionId} />
                <p id={grantDescriptionId} className='usher-access-fields__description'>{labels.access.grantAccessDescription}</p>
            </div>
            {choice.grantAccess && (
                <div className='usher-access-fields__duration'>
                    <DropdownField<TCommand>
                        value={command => command.accessDuration}
                        title={labels.access.duration}
                        aria-describedby={noticeId}
                        options={durationOptions(labels)}
                        optionValue='value'
                        optionLabel='label' />
                    <AccessNotice id={noticeId} duration={choice.accessDuration} />
                </div>
            )}
        </div>
    );
};
