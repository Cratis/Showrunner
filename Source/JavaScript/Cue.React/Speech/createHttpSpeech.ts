// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useCallback, useEffect, useRef, useState } from 'react';
import type { CueSpeech } from './CueSpeech.js';
import type { CueSpeechConfiguration } from './CueSpeechConfiguration.js';
import type { HttpSpeechOptions } from './HttpSpeechOptions.js';
import { createSpeechClips } from './speechClips.js';

// A tenth of a second of silence. Playing it from a gesture is what lets a browser that blocks sound until the viewer
// has done something allow the lines that follow.
const SILENT_CLIP = 'data:audio/wav;base64,UklGRkQDAABXQVZFZm10IBAAAAABAAEAQB8AAEAfAAABAAgAZGF0YSADAACAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgA==';

/**
 * Gives a tour its voice-over from a text-to-speech service that is reached over HTTP: each line is sent to the service,
 * which answers with audio, and the audio is played one line at a time.
 * @param options Where the audio comes from.
 * @returns The configuration to give {@link CueProvider}.
 * @remarks
 * Create it once, outside any component, and keep it. An application whose service streams, mixes voices or plays through
 * its own audio pipeline gives the tour a {@link CueSpeechConfiguration} of its own instead.
 */
export function createHttpSpeech(options: HttpSpeechOptions): CueSpeechConfiguration {
    const clips = createSpeechClips(options);
    let availability: Promise<boolean> | undefined;
    let element: HTMLAudioElement | undefined;

    const audio = (): HTMLAudioElement => element ??= new Audio();

    // Whether a service is there is the same answer for every tour, so it is asked once.
    const isAvailable = (): Promise<boolean> => {
        if (!options.availableUrl) {
            return Promise.resolve(true);
        }

        availability ??= (options.fetch ?? fetch)(options.availableUrl, { credentials: options.credentials ?? 'include', headers: options.headers })
            .then(response => (response.ok ? response.json() : { available: false }))
            .then(payload => Boolean((payload as { available?: boolean }).available))
            .catch(() => {
                availability = undefined;
                return false;
            });
        return availability;
    };

    const useSpeech = (): CueSpeech => {
        const [available, setAvailable] = useState(!options.availableUrl);
        const [isPreparing, setIsPreparing] = useState(false);
        const [isSpeaking, setIsSpeaking] = useState(false);
        const [hasError, setHasError] = useState(false);
        const request = useRef(0);
        const finish = useRef<(() => void) | null>(null);

        useEffect(() => {
            let current = true;
            void isAvailable().then(value => {
                if (current) setAvailable(value);
            });
            return () => { current = false; };
        }, []);

        const stop = useCallback(() => {
            request.current++;
            finish.current?.();
            finish.current = null;
            setIsPreparing(false);
            setIsSpeaking(false);
        }, []);

        useEffect(() => stop, [stop]);

        const speak = useCallback(async (text: string) => {
            stop();
            const mine = request.current;
            setHasError(false);
            setIsPreparing(true);
            let url: string | undefined;
            try {
                const clip = await clips.obtain(text);
                if (mine !== request.current) return;

                const player = audio();
                url = URL.createObjectURL(clip);
                const source = url;
                await new Promise<void>((resolve, reject) => {
                    const done = () => {
                        player.onplaying = null;
                        player.onended = null;
                        player.onerror = null;
                        player.pause();
                        URL.revokeObjectURL(source);
                        resolve();
                    };
                    finish.current = done;
                    player.onplaying = () => {
                        if (mine !== request.current) return;
                        setIsPreparing(false);
                        setIsSpeaking(true);
                    };
                    player.onended = () => {
                        if (mine === request.current) {
                            finish.current = null;
                            setIsSpeaking(false);
                        }
                        done();
                    };
                    player.onerror = () => reject(new Error('The line could not be played.'));
                    player.src = source;
                    player.play().catch(reject);
                });
            } catch {
                if (url) URL.revokeObjectURL(url);
                if (mine === request.current) {
                    setIsPreparing(false);
                    setIsSpeaking(false);
                    setHasError(true);
                }
            }
        }, [stop]);

        const prefetch = useCallback((text: string) => clips.prefetch(text), []);

        return { isAvailable: available, isPreparing, isSpeaking, hasError, speak, prefetch, stop };
    };

    const unlock = () => {
        const player = audio();
        if (player.paused && !player.src) {
            player.src = SILENT_CLIP;
            player.play().catch(() => undefined);
        }
    };

    return { useSpeech, unlock };
}
