// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Importance } from '@cratis/showrunner.marquee';
import { MarqueeLabels } from '../Labels/index.js';

/**
 * How an importance looks: a modifier the stylesheet colors, and the label of its icon.
 */
export interface ImportancePresentation {
    /** The modifier of the class names, `marquee-banner__item--<modifier>` and `marquee-dialog__importance--<modifier>`. */
    modifier: 'information' | 'warning' | 'critical';

    /** The accessible name of the icon. */
    label: string;
}

/**
 * Chooses how an importance is presented, so the banner and the dialog agree on the color and the icon of a level.
 * @param importance The importance.
 * @param labels The texts to name the icon with.
 * @returns How to present it.
 */
export const importancePresentation = (importance: Importance, labels: MarqueeLabels): ImportancePresentation => {
    switch (importance) {
        case Importance.warning:
            return { modifier: 'warning', label: labels.warning };
        case Importance.critical:
            return { modifier: 'critical', label: labels.critical };
        default:
            return { modifier: 'information', label: labels.information };
    }
};
