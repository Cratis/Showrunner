// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ArcContext } from '@cratis/arc.react';
import { CratisComponentsProvider } from '@cratis/components';
import { Guid } from '@cratis/fundamentals';
import { cleanup, render } from '@testing-library/react';
import { useContext } from 'react';
import { UsherBoundary } from '../UsherBoundary.js';
import { UsherProvider } from '../UsherProvider.js';
import { Usher, useUsher } from '../useUsher.js';

interface Observed {
    usher?: Usher;
    applicationHeaders?: Record<string, string>;
    usherHeaders?: Record<string, string>;
}

const Application = ({ observed }: { observed: Observed }) => {
    const arc = useContext(ArcContext);
    observed.usher = useUsher();
    observed.applicationHeaders = arc.httpHeadersCallback?.() as Record<string, string>;
    return null;
};

const UsherInternals = ({ observed }: { observed: Observed }) => {
    const arc = useContext(ArcContext);
    observed.usherHeaders = arc.httpHeadersCallback?.() as Record<string, string>;
    return null;
};

describe('when the application sends requests', () => {
    const observed: Observed = {};

    beforeEach(() => {
        render(
            <CratisComponentsProvider>
                <UsherProvider labels={{ myIssuesButton: { label: 'Mine saker' } }}>
                    <Application observed={observed} />
                    <UsherBoundary>
                        <UsherInternals observed={observed} />
                    </UsherBoundary>
                </UsherProvider>
            </CratisComponentsProvider>
        );
    });

    afterEach(() => cleanup());

    it('should give the request a correlation id', () => Guid.isGuid(observed.applicationHeaders!['x-correlation-id']).should.be.true);
    it('should track the correlation id', () => observed.usher!.recentCorrelationIds().map(id => id.toString()).should.include(observed.applicationHeaders!['x-correlation-id']));
    it('should not track the requests of usher itself', () => (observed.usherHeaders!['x-correlation-id'] === undefined).should.be.true);
    it('should use the labels given', () => observed.usher!.labels.myIssuesButton.label.should.equal('Mine saker'));
    it('should keep the defaults of the labels left out', () => observed.usher!.labels.reportIssue.title.should.equal('Report a problem'));
});
