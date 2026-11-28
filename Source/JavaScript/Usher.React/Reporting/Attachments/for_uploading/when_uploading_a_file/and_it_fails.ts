// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { uploadUsherAttachment } from '../../uploading.js';

describe('when uploading a file and it fails', () => {
    const file = new File(['content'], 'too-big.png', { type: 'image/png' });
    let error: unknown;

    beforeEach(async () => {
        vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response(null, { status: 400 })));
        try {
            await uploadUsherAttachment(file);
        } catch (caught) {
            error = caught;
        }
    });
    afterEach(() => vi.unstubAllGlobals());

    it('should reject naming the file that could not be uploaded', () => (error as Error).message.should.contain('too-big.png'));
});
