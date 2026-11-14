// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { defaultCueLabels } from './defaultCueLabels.js';
import type { CueLabels } from './CueLabels.js';

/**
 * Overlays the texts an application gives on the English defaults.
 * @param overrides The texts to replace; the rest stay English.
 * @returns The texts to show.
 */
export const resolveLabels = (overrides?: Partial<CueLabels>): CueLabels => ({ ...defaultCueLabels, ...overrides });
