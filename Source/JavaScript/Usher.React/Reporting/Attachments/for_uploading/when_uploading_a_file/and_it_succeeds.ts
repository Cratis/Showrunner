// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { uploadUsherAttachment, usherAttachmentUploadPath } from '../../uploading.js';

describe('when uploading a file and it succeeds', () => {
    const file = new File(['content'], 'screenshot.png', { type: 'image/png' });
    const fetchStub = vi.fn().mockImplementation(() =>
        Promise.resolve(new Response(JSON.stringify({ url: 'https://support.example.com/usher-attachments/abc.png', isImage: true }), { status: 200 })));
    let markup: string;

    beforeEach(async () => {
        vi.stubGlobal('fetch', fetchStub);
        markup = await uploadUsherAttachment(file);
    });

    afterEach(() => vi.unstubAllGlobals());

    it('should post to the attachment upload endpoint', () => (fetchStub.mock.calls[0][0] as string).should.equal(usherAttachmentUploadPath));
    it('should carry the original file name in a header', () => (fetchStub.mock.calls[0][1].headers as Record<string, string>)['X-File-Name'].should.equal('screenshot.png'));
    it('should resolve to the markup for the uploaded attachment', () => markup.should.equal('<img width="640" alt="screenshot.png" src="https://support.example.com/usher-attachments/abc.png" />'));
});
