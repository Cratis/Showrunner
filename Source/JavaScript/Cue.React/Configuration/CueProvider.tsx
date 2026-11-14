// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import React, { type ReactNode, createContext, useContext, useMemo } from 'react';
import { type CueLabels } from '../Labels/CueLabels.js';
import { resolveLabels } from '../Labels/resolveLabels.js';
import { CueStageBoundary } from './CueStageBoundary.js';
import type { CueConfiguration } from './CueConfiguration.js';
import type { ComponentType } from 'react';
import type { CueSpeech } from '../Speech/CueSpeech.js';
import { silentSpeech } from '../Speech/silentSpeech.js';

/** The configuration as the tour reads it: everything is there. */
export interface ResolvedCueConfiguration {
    /** The hook that gives a tour its voice; a silent one when the tours play without a voice-over. */
    useSpeech: () => CueSpeech;
    /** Unlocks audio playback from a viewer's gesture; does nothing when there is no voice-over. */
    unlockAudio: () => void;
    /** Every text of the tour's chrome. */
    labels: CueLabels;
    /** What guards the stage of a tour. */
    stageBoundary: ComponentType<{ children: ReactNode }>;
}

// A tour with no voice-over plays silently: there is no service, so nothing is ever said.
const useSilentSpeech = (): CueSpeech => silentSpeech;

const defaults: ResolvedCueConfiguration = {
    useSpeech: useSilentSpeech,
    unlockAudio: () => undefined,
    labels: resolveLabels(),
    stageBoundary: CueStageBoundary
};

const CueConfigurationContext = createContext<ResolvedCueConfiguration>(defaults);

/**
 * Props for {@link CueProvider}.
 */
export interface CueProviderProps extends CueConfiguration {
    /** What the configuration applies to: the tours. */
    children: ReactNode;
}

/**
 * Tells the tours below it how to speak, what to call their buttons and how to guard their stage. Put it once near the
 * root of the application; without it a tour plays silently, in English.
 */
export const CueProvider = ({ speech, labels, stageBoundary, children }: CueProviderProps) => {
    const value = useMemo<ResolvedCueConfiguration>(() => ({
        useSpeech: speech?.useSpeech ?? useSilentSpeech,
        unlockAudio: () => speech?.unlock?.(),
        labels: resolveLabels(labels),
        stageBoundary: stageBoundary ?? CueStageBoundary
    }), [speech, labels, stageBoundary]);

    return <CueConfigurationContext.Provider value={value}>{children}</CueConfigurationContext.Provider>;
};

/**
 * Gets the configuration of the nearest {@link CueProvider}, or the defaults when there is none.
 * @returns The {@link ResolvedCueConfiguration}.
 */
export const useCueConfiguration = (): ResolvedCueConfiguration => useContext(CueConfigurationContext);
