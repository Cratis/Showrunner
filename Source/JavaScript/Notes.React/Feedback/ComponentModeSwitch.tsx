// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ToggleGroup } from '@cratis/components/Common';
import { useNotesLabels } from '../Provider/NotesContext.js';

/**
 * How feedback on a component is given.
 */
export type ComponentFeedbackMode = 'describe' | 'change';

/**
 * Props for {@link ComponentModeSwitch}.
 */
export interface ComponentModeSwitchProps {
    /** The mode. */
    mode: ComponentFeedbackMode;

    /** Called with the mode the user chooses. */
    onChange: (mode: ComponentFeedbackMode) => void;
}

/**
 * Switches between describing a component and changing it.
 */
export const ComponentModeSwitch = ({ mode, onChange }: ComponentModeSwitchProps) => {
    const labels = useNotesLabels();

    return (
        <ToggleGroup
            aria-label={labels.component.mode}
            value={mode}
            options={[
                { value: 'describe', label: labels.component.describe },
                { value: 'change', label: labels.component.change }
            ]}
            onChange={value => onChange(value as ComponentFeedbackMode)} />
    );
};
