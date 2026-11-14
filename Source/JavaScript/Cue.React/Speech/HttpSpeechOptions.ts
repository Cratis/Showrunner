// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Where the audio for a line comes from, for {@link createHttpSpeech}.
 */
export interface HttpSpeechOptions {
    /**
     * The address that is sent a line and answers with audio the browser can play, such as an MP3. A line is sent with
     * POST, as JSON: `{ "text": "..." }` unless {@link body} says otherwise.
     */
    speakUrl: string;

    /**
     * The address that is asked, with GET, whether speech is available, and answers `{ "available": true }`. Left out, speech
     * is taken to be available and a failing line is reported as it happens.
     */
    availableUrl?: string;

    /**
     * Builds what is sent for a line, when the service wants more than the text: the voice, the speed, who is speaking.
     * The voice of the tour is the application's to choose here, never the browser's to name.
     */
    body?: (text: string) => unknown;

    /** The headers sent with every request, such as an API key. */
    headers?: Record<string, string>;

    /** Whether cookies go with the requests. Defaults to `include`, so a service behind the application's own sign-in works. */
    credentials?: RequestCredentials;

    /** How many lines are kept so speaking one again does not ask for it again. Defaults to 24. */
    cacheSize?: number;

    /** The fetch to use. Defaults to the browser's; a specification gives its own. */
    fetch?: typeof fetch;
}
