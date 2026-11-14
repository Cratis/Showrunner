// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import sinon from 'sinon';

/**
 * Stands in for a text-to-speech service reached over HTTP: every line asked for is recorded, and answered with a clip, or
 * refused when the service is set to refuse.
 */
export class aSpeechService {
    readonly requests: { url: string; init: RequestInit }[] = [];
    refuses = false;
    readonly fetch: typeof fetch;

    constructor() {
        this.fetch = sinon.fake(((url: string, init: RequestInit) => {
            this.requests.push({ url, init });
            return Promise.resolve({
                ok: !this.refuses,
                status: this.refuses ? 503 : 200,
                blob: () => Promise.resolve(new Blob(['audio'])),
                json: () => Promise.resolve({ available: !this.refuses })
            });
        }) as never) as never;
    }

    /** The lines that were sent, in order. */
    get lines(): string[] {
        return this.requests.filter(request => request.init.method === 'POST').map(request => (JSON.parse(request.init.body as string) as { text: string }).text);
    }
}
