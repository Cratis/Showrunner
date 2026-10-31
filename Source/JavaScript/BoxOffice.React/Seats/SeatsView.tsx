// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { Button } from '@cratis/components/Common';
import { Column, DataTableCore, DataTableStatus } from '@cratis/components/DataTables';
import { Message, Tag } from '@cratis/components/Display';
import { SeatHolding } from '@cratis/showrunner.boxoffice';
import { formatDay } from '../formatting.js';
import { format } from '../Labels/index.js';
import { useBoxOfficeContext } from '../Provider/BoxOfficeContext.js';
import { SeatRow, SeatSummary } from './seatModels.js';

/**
 * Props for {@link SeatsView}.
 */
export interface SeatsViewProps {
    /** The seats, oldest first. */
    rows: readonly SeatRow[];

    /** What to summarize. */
    summary: SeatSummary;

    /** Whether the tenant is read-only, so seats cannot be changed. */
    isReadOnly: boolean;

    /** Whether the seats are still loading. */
    isLoading?: boolean;

    /** Whether the seats could not be loaded because the user may not see them. */
    isUnauthorized?: boolean;

    /** Whether the seats could not be loaded. */
    hasFailed?: boolean;

    /** Called when the user wants to assign a seat. */
    onAssign: () => void;

    /** Called when the user wants to release a seat. */
    onRelease: (holding: SeatHolding) => void;
}

const statusOf = (isLoading?: boolean, isUnauthorized?: boolean, hasFailed?: boolean) => {
    if (isUnauthorized) {
        return DataTableStatus.Unauthorized;
    }

    if (hasFailed) {
        return DataTableStatus.Failed;
    }

    return isLoading ? DataTableStatus.Loading : DataTableStatus.Ready;
};

/**
 * The seats of the tenant: how many are used, who holds them, and buttons to assign and release.
 */
export const SeatsView = ({ rows, summary, isReadOnly, isLoading, isUnauthorized, hasFailed, onAssign, onRelease }: SeatsViewProps) => {
    const { labels, locale } = useBoxOfficeContext();

    return (
        <section className='boxoffice-seats' aria-labelledby='boxoffice-seats-title'>
            <header className='boxoffice-page__header'>
                <h2 id='boxoffice-seats-title' className='boxoffice-page__title'>{labels.seats.title}</h2>
                <Button label={labels.seats.assign} disabled={isReadOnly || summary.capacity === 0} onClick={onAssign} />
            </header>

            <p className='boxoffice-seats__summary'>
                {summary.capacity === 0
                    ? labels.seats.noCapacity
                    : format(labels.seats.summary, { held: summary.held, capacity: summary.capacity })}
            </p>
            {summary.over > 0 && <Message severity='warn'>{format(labels.seats.overCapacity, { over: summary.over })}</Message>}
            {isReadOnly && <Message severity='info'>{labels.seats.readOnly}</Message>}

            <DataTableCore<SeatRow>
                data={[...rows]}
                dataKey='holding.holder.identityProviderSubject'
                status={statusOf(isLoading, isUnauthorized, hasFailed)}
                emptyMessage={labels.seats.empty}
                loadingMessage={labels.common.loading}
                failureMessage={labels.common.failed}
                unauthorizedMessage={labels.common.unauthorized}>
                <Column<SeatRow> header={labels.seats.holderColumn} body={row => row.holding.holder.name} />
                <Column<SeatRow> header={labels.seats.emailColumn} body={row => row.holding.holder.email} />
                <Column<SeatRow> header={labels.seats.assignedColumn} body={row => formatDay(row.holding.assignedAt, locale)} />
                <Column<SeatRow>
                    header={labels.seats.statusColumn}
                    body={row => (
                        <Tag
                            severity={row.coverage === 'licensed' ? 'success' : 'warn'}
                            value={row.coverage === 'licensed' ? labels.seats.licensed : labels.seats.overCapacityStatus} />
                    )} />
                <Column<SeatRow>
                    header=''
                    body={row => (
                        <Button
                            variant='ghost'
                            tone='critical'
                            size='small'
                            label={labels.seats.releaseShort}
                            aria-label={format(labels.seats.release, { name: row.holding.holder.name })}
                            disabled={isReadOnly}
                            onClick={() => onRelease(row.holding)} />
                    )} />
            </DataTableCore>
        </section>
    );
};
