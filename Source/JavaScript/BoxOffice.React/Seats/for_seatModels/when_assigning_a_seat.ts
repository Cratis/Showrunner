// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { emptySeatPerson, isValidSeatPerson, personOf } from '../seatModels.js';

describe('when assigning a seat', () => {
    const typed = { identityProvider: ' github ', subject: ' 1001 ', name: ' Ada Lovelace ', email: ' ada@fjord.example.com ' };
    const person = personOf(typed);

    it('should not accept an empty form', () => isValidSeatPerson(emptySeatPerson).should.be.false);
    it('should not accept a form with a field missing', () => isValidSeatPerson({ ...typed, email: '  ' }).should.be.false);
    it('should accept a filled form', () => isValidSeatPerson(typed).should.be.true);
    it('should trim the identity provider', () => person.identityProvider.should.equal('github'));
    it('should trim the subject', () => person.identityProviderSubject.should.equal('1001'));
    it('should trim the name', () => person.name.should.equal('Ada Lovelace'));
    it('should trim the email address', () => person.email.should.equal('ada@fjord.example.com'));
});
