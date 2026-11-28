// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { summarizeAttachmentsInText } from '../summarizeAttachmentsInText.js';

describe('when the text carries an image tag', () => {
    const result = summarizeAttachmentsInText('Before <img width="640" alt="screenshot.png" src="https://support.example.com/usher-attachments/abc.png" /> after');

    it('should replace the tag with a marker carrying the file name', () => result.should.equal('Before 📎 screenshot.png after'));
});
