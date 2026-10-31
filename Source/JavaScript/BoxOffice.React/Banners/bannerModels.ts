// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { ReadOnlyReason, SeatStanding, SeatStatus, SuspensionCause } from '@cratis/showrunner.boxoffice';
import { Entitlements } from '../Entitlements/entitlementModel.js';
import { BoxOfficeLabels, format } from '../Labels/index.js';

/** How loud a banner is. */
export type BannerSeverity = 'info' | 'warn' | 'error';

/**
 * What the trial banner says.
 */
export interface TrialBannerModel {
    /** How loud the banner is. */
    readonly severity: BannerSeverity;

    /** What it says. */
    readonly text: string;
}

/** How many days before a trial ends the banner turns into a warning. */
export const trialWarningDays = 7;

/**
 * Works out what the trial banner says.
 * @param entitlements What the tenant may do.
 * @param labels The labels to show.
 * @returns What to say; undefined when the tenant is not on a trial, or the trial is not what makes it read-only.
 */
export function trialBannerModel(entitlements: Entitlements, labels: BoxOfficeLabels): TrialBannerModel | undefined {
    if (entitlements.state === 'trialClosed') {
        return { severity: 'error', text: labels.trialBanner.closed };
    }

    if (entitlements.state !== 'trial' || !entitlements.trial) {
        return undefined;
    }

    const { daysLeft } = entitlements.trial;
    if (daysLeft <= 0) {
        return { severity: 'warn', text: labels.trialBanner.lastDay };
    }

    return {
        severity: daysLeft <= trialWarningDays ? 'warn' : 'info',
        text: format(labels.trialBanner.running, { days: daysLeft })
    };
}

/**
 * Says why the operator suspended a tenant.
 * @param cause The cause the operator gave.
 * @param labels The labels to show.
 * @returns The reason, in words.
 */
export function suspensionReason(cause: SuspensionCause | undefined, labels: BoxOfficeLabels): string {
    switch (cause) {
        case SuspensionCause.trialEndedWithoutSeats:
            return labels.suspension.trialEndedWithoutSeats;
        case SuspensionCause.paymentOverdue:
            return labels.suspension.paymentOverdue;
        case SuspensionCause.administratorDecision:
            return labels.suspension.administratorDecision;
        default:
            return '';
    }
}

/** Why a user is told they have no license. */
export type NoLicenseReason = 'suspended' | 'trialClosed' | 'noSeat' | 'overCapacity';

/**
 * What a user without a license is told.
 */
export interface NoLicenseModel {
    /** Why the user has no license. */
    readonly reason: NoLicenseReason;

    /** The title. */
    readonly title: string;

    /** What to do about it. */
    readonly text: string;
}

/**
 * Works out whether the user has a license, and what to tell them if not. The tenant being read-only comes first,
 * because it holds for everyone in it.
 * @param entitlements What the tenant may do.
 * @param seat Where the user stands with the tenant's seats; nothing while it loads.
 * @param labels The labels to show.
 * @returns What to tell the user; undefined when they have a license, or it is not known yet.
 */
export function noLicenseModel(entitlements: Entitlements, seat: SeatStanding | undefined, labels: BoxOfficeLabels): NoLicenseModel | undefined {
    if (entitlements.isLoading) {
        return undefined;
    }

    if (entitlements.readOnlyReason === ReadOnlyReason.tenantSuspended) {
        return { reason: 'suspended', title: labels.noLicense.suspendedTitle, text: labels.noLicense.suspended };
    }

    if (entitlements.readOnlyReason === ReadOnlyReason.trialClosed) {
        return { reason: 'trialClosed', title: labels.noLicense.trialClosedTitle, text: labels.noLicense.trialClosed };
    }

    if (seat?.status === SeatStatus.noSeat) {
        return { reason: 'noSeat', title: labels.noLicense.noSeatTitle, text: labels.noLicense.noSeat };
    }

    if (seat?.status === SeatStatus.overCapacity) {
        return { reason: 'overCapacity', title: labels.noLicense.overCapacityTitle, text: labels.noLicense.overCapacity };
    }

    return undefined;
}
