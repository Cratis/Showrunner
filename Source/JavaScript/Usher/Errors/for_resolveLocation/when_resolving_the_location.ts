// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { SliceLocation as ArcSliceLocation } from '@cratis/arc/reflection';
import { resolveLocation } from '../resolveLocation';

class PayInvoice {
    static readonly sliceLocation = new ArcSliceLocation('Invoicing', 'Invoices', 'PayInvoice');
}

class Undeclared {
}

const fromTheComponentTree = new ArcSliceLocation('Invoicing', 'Invoices', '');

describe('when resolving the location', () => {
    it('should use the location the proxy declares', () => resolveLocation(new PayInvoice(), fromTheComponentTree).should.equal(PayInvoice.sliceLocation));
    it('should use the given location when the proxy declares none', () => resolveLocation(new Undeclared(), fromTheComponentTree).should.equal(fromTheComponentTree));
    it('should use the given location without a command or query', () => resolveLocation(undefined, fromTheComponentTree).should.equal(fromTheComponentTree));
    it('should be empty when nothing is known', () => resolveLocation().isEmpty.should.be.true);
});
