// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { uploadMarkupFor } from '../../uploading.js';

describe('when building the markup for an attachment and it is an image', () => {
    const markup = uploadMarkupFor('screenshot.png', 'https://support.example.com/usher-attachments/abc.png', true);

    it('should build an image tag carrying the file name and address', () =>
        markup.should.equal('<img width="640" alt="screenshot.png" src="https://support.example.com/usher-attachments/abc.png" />'));
});
