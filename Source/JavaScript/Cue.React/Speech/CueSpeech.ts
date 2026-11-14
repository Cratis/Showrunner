// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * The voice-over the tour plays through: what a text-to-speech service looks like to the tour. An application gives the
 * tour one through {@link CueProvider}, either built for a service with {@link createHttpSpeech} or its own.
 *
 * The tour asks for a line to be spoken when its step begins, asks for the lines to come ahead of time, and watches
 * the state to learn when a line has finished: a line has finished once it was heard preparing or speaking and neither
 * is true any more. A voice that cannot be had is reported as an error and the tour carries on with its subtitles.
 */
export interface CueSpeech {
    /** Whether a text-to-speech service is available at all. */
    isAvailable: boolean;
    /** Whether a line is being prepared: from asking for it until audio starts playing. */
    isPreparing: boolean;
    /** Whether audio is playing. */
    isSpeaking: boolean;
    /** Whether the last line failed. Cleared when the next one starts. */
    hasError: boolean;
    /** Speaks a line, first silencing whatever was still being said. A line asked for ahead of time with {@link prefetch} starts at once. */
    speak(text: string): Promise<void>;
    /** Asks for a line ahead of time, so speaking it later does not wait for the service. */
    prefetch(text: string): void;
    /** Silences whatever is being said. */
    stop(): void;
}
