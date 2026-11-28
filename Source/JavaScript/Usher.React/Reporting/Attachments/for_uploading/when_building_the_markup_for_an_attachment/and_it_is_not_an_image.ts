// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { uploadMarkupFor } from '../../uploading.js';

describe('when building the markup for an attachment and it is not an image', () => {
    const markup = uploadMarkupFor('crash.log', 'https://support.example.com/usher-attachments/abc.log', false);

    it('should build a link carrying the file name and address', () =>
        markup.should.equal('[crash.log](https://support.example.com/usher-attachments/abc.log)'));
});
