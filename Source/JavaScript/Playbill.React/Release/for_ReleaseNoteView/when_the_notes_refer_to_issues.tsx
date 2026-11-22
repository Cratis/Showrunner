// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { cleanup, render } from '@testing-library/react';
import { defaultPlaybillLabels } from '../../Labels/index.js';
import { aRelease } from '../../given/releases.js';
import { ReleaseNoteView } from '../ReleaseNoteView.js';

describe('when the notes refer to issues', () => {
    const release = aRelease({ version: '2.3.0', lead: 'Started with #12.', sections: [{ name: 'Fixed', markdown: '- A crash (#981)' }] });
    const links = (issueRepository?: string) => {
        const { container } = render(<ReleaseNoteView release={release} labels={defaultPlaybillLabels} issueRepository={issueRepository} />);
        return [...container.querySelectorAll('a')].map(link => link.getAttribute('href'));
    };

    afterEach(() => cleanup());

    describe('and the repository is known', () => {
        it('should link them in the sections', () => links('https://github.com/Cratis/Studio').should.include('https://github.com/Cratis/Studio/issues/981'));
        it('should link them in the lead', () => links('https://github.com/Cratis/Studio').should.include('https://github.com/Cratis/Studio/issues/12'));
    });

    describe('and the repository is not known', () => {
        it('should leave them as text', () => links().should.be.empty);
    });
});
