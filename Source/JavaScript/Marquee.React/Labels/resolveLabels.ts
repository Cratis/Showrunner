// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { defaultMarqueeLabels } from './defaultMarqueeLabels.js';
import { MarqueeLabels } from './MarqueeLabels.js';

/**
 * Overlays the texts a product gives on the English defaults.
 * @param overrides The texts to replace; the rest stay English.
 * @returns The texts to show.
 */
export const resolveLabels = (overrides?: Partial<MarqueeLabels>): MarqueeLabels => ({ ...defaultMarqueeLabels, ...overrides });
