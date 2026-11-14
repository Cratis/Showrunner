// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import type { HttpSpeechOptions } from './HttpSpeechOptions.js';

const DEFAULT_CACHE_SIZE = 24;

/** The audio of the lines a tour speaks, asked for once each. */
export interface SpeechClips {
    /** Gets the audio of a line, asking the service for it unless it already has it or is already asking. */
    obtain(text: string): Promise<Blob>;

    /** Starts asking for the audio of a line, without waiting for it. */
    prefetch(text: string): void;
}

/**
 * Creates the clips of a speech service.
 * @param options Where the audio comes from.
 * @returns The {@link SpeechClips}.
 * @remarks
 * A line that failed is forgotten, so asking for it again asks the service again. The oldest lines are dropped once the
 * cache is full.
 */
export function createSpeechClips(options: HttpSpeechOptions): SpeechClips {
    const clips = new Map<string, Promise<Blob>>();
    const limit = options.cacheSize ?? DEFAULT_CACHE_SIZE;

    const request = async (text: string): Promise<Blob> => {
        const send = options.fetch ?? fetch;
        const response = await send(options.speakUrl, {
            method: 'POST',
            credentials: options.credentials ?? 'include',
            headers: { 'Content-Type': 'application/json', ...options.headers },
            body: JSON.stringify(options.body ? options.body(text) : { text })
        });
        if (!response.ok) {
            throw new Error(`The speech service answered ${response.status}.`);
        }
        return response.blob();
    };

    const obtain = (text: string): Promise<Blob> => {
        const known = clips.get(text);
        if (known) {
            return known;
        }

        const clip = request(text);
        clips.set(text, clip);
        clip.catch(() => {
            if (clips.get(text) === clip) {
                clips.delete(text);
            }
        });
        while (clips.size > limit) {
            clips.delete(clips.keys().next().value as string);
        }
        return clip;
    };

    return {
        obtain,
        prefetch: text => { obtain(text).catch(() => undefined); }
    };
}
