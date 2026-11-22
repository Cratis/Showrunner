// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { defaultPlaybillLabels } from './defaultPlaybillLabels.js';
import { PlaybillLabels } from './PlaybillLabels.js';

/**
 * Overlays the texts a product gives on the English defaults.
 * @param overrides The texts to replace; the rest stay English.
 * @returns The texts to show.
 */
export const resolveLabels = (overrides?: Partial<PlaybillLabels>): PlaybillLabels => ({ ...defaultPlaybillLabels, ...overrides });

/**
 * Puts a number into a text that has `{count}`.
 * @param text The text.
 * @param count The number.
 * @returns The text with the number in it.
 */
export const withCount = (text: string, count: number): string => text.split('{count}').join(count.toString());
