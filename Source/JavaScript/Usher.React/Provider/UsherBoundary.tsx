// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ArcContext } from '@cratis/arc.react';
import { Slice } from '@cratis/arc.react/slices';
import { ReactNode, useContext } from 'react';
import { UsherContext } from './UsherContext.js';

/**
 * Props for {@link UsherBoundary}.
 */
export interface UsherBoundaryProps {
    /** What Usher renders. */
    children?: ReactNode;
}

/**
 * Keeps Usher's own commands and queries out of what a report carries: they run without correlation tracking, and
 * do not change the slice location of the solution's components around them.
 */
export const UsherBoundary = ({ children }: UsherBoundaryProps) => {
    const usher = useContext(UsherContext);
    const arc = useContext(ArcContext);

    return (
        <ArcContext.Provider value={usher.untrackedArc ?? arc}>
            <Slice>{children}</Slice>
        </ArcContext.Provider>
    );
};
