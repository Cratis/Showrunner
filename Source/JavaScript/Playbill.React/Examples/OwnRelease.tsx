// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Playbill, ReleaseNoteViewProps } from '@cratis/showrunner.playbill.react';

const OwnRelease = ({ release }: ReleaseNoteViewProps) => (
    <article>
        <h2>Version {release.version}</h2>
        {release.sections.map(section => (
            <details key={section.name} open={section.name === 'Added'}>
                <summary>{section.name}</summary>
                <pre>{section.markdown}</pre>
            </details>
        ))}
    </article>
);

export const OwnReleasePage = () => (
    <Playbill release={OwnRelease} />
);
