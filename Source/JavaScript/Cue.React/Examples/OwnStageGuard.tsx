// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { CueProvider } from '@cratis/showrunner.cue.react';
import { Component, ReactNode } from 'react';

class ReportingGuard extends Component<{ children: ReactNode }, { failed: boolean }> {
    state = { failed: false };

    static getDerivedStateFromError() {
        return { failed: true };
    }

    componentDidCatch(error: Error) {
        console.error('The tour could not be drawn.', error);
    }

    render() {
        return this.state.failed ? <p>The tour could not be drawn.</p> : this.props.children;
    }
}

export const GuardedCue = ({ children }: { children: ReactNode }) => (
    <CueProvider stageBoundary={ReportingGuard}>{children}</CueProvider>
);
