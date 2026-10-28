// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { formatLocation } from '../formatLocation.js';

describe('when formatting a location', () => {
    it('should join the parts', () => formatLocation({ module: 'Billing', feature: 'Invoices', slice: 'Pay' }, ' › ', 'Unknown').should.equal('Billing › Invoices › Pay'));
    it('should leave out empty parts', () => formatLocation({ module: 'Billing', feature: '', slice: 'Pay' }, ' › ', 'Unknown').should.equal('Billing › Pay'));
    it('should say it is unknown when no part is known', () => formatLocation({ module: '', feature: '', slice: '' }, ' › ', 'Unknown').should.equal('Unknown'));
    it('should say it is unknown when there is none', () => formatLocation(undefined, ' › ', 'Unknown').should.equal('Unknown'));
});
