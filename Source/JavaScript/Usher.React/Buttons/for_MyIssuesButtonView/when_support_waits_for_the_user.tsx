// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CratisComponentsProvider } from '@cratis/components';
import { cleanup, render } from '@testing-library/react';
import sinon from 'sinon';
import { defaultUsherLabels } from '../../Labels/defaultUsherLabels.js';
import { MyIssuesButtonView } from '../MyIssuesButton.js';

describe('when support waits for the user', () => {
    let button: HTMLButtonElement;
    let container: HTMLElement;

    beforeEach(() => {
        const result = render(
            <CratisComponentsProvider>
                <MyIssuesButtonView size='small' waiting={2} labels={defaultUsherLabels} onOpen={sinon.stub()} />
            </CratisComponentsProvider>
        );
        container = result.container;
        button = result.getByRole('button') as HTMLButtonElement;
    });

    afterEach(() => cleanup());

    it('should say how many wait in the name', () => button.getAttribute('aria-label')!.should.equal('My issues, 2 waiting for you'));
    it('should show the count', () => container.querySelector('.usher-my-issues-button__badge')!.textContent!.should.equal('2'));
    it('should hide the count from screen readers, as the name says it', () => container.querySelector('.usher-my-issues-button__badge')!.getAttribute('aria-hidden')!.should.equal('true'));
});
