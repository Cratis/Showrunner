// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import React, { Component, type ErrorInfo, type ReactNode } from 'react';

interface CueStageBoundaryState {
    failed: boolean;
}

/**
 * The guard of a tour's stage when the application brings none: a panel that says the tour could not be drawn, with a way
 * to try again.
 */
export class CueStageBoundary extends Component<{ children: ReactNode }, CueStageBoundaryState> {
    state: CueStageBoundaryState = { failed: false };

    static getDerivedStateFromError(): CueStageBoundaryState {
        return { failed: true };
    }

    componentDidCatch(error: Error, info: ErrorInfo) {
        console.error('The guided tour could not be drawn.', error, info.componentStack);
    }

    render() {
        if (!this.state.failed) {
            return this.props.children;
        }

        return (
            <div className='cue-stage-failed' role='alert'>
                <p>The guided tour could not be drawn.</p>
                <button type='button' onClick={() => this.setState({ failed: false })}>Try again</button>
            </div>
        );
    }
}
