// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Guid } from '@cratis/fundamentals';

/**
 * What a report dialog closes with once the report is sent.
 */
export interface ReportSent {
    /** The issue the report opened. */
    readonly issueId: Guid;
}
