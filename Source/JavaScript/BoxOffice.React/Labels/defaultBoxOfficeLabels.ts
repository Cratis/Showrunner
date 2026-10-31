// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { BoxOfficeLabels } from './BoxOfficeLabels.js';

/**
 * The English labels of the Box Office components.
 */
export const defaultBoxOfficeLabels: BoxOfficeLabels = {
    common: {
        cancel: 'Cancel',
        close: 'Close',
        loading: 'Loading…',
        failed: 'This could not be loaded.',
        unauthorized: 'You are not allowed to see this.',
        portalFailed: 'The customer portal could not be opened. Try again in a moment.',
        noPortal: 'Contact the people who run this product to change your plan.'
    },
    moduleAccess: {
        none: 'Not included',
        preview: 'Preview',
        trial: 'Trial',
        full: 'Included',
        until: 'until {date}'
    },
    trialBanner: {
        running: 'Your trial ends in {days} days.',
        lastDay: 'Your trial ends today.',
        closed: 'Your trial has ended. Your data is kept, and you can read it, but nothing can be changed until you buy.',
        buy: 'Buy'
    },
    suspension: {
        title: 'Your organization is suspended',
        trialEndedWithoutSeats: 'Your trial ended without any seats being bought.',
        paymentOverdue: 'A payment is overdue.',
        administratorDecision: 'The people who run this product suspended it.',
        resolve: 'You can read everything, but nothing can be changed until the suspension is lifted.',
        openPortal: 'Open the customer portal'
    },
    noLicense: {
        noSeatTitle: 'You do not have a seat',
        noSeat: 'Ask an administrator of your organization to give you a seat.',
        overCapacityTitle: 'Your seat is not covered',
        overCapacity: 'Your organization has more people with seats than it has bought. Ask an administrator to buy more seats or release some.',
        trialClosedTitle: 'The trial has ended',
        trialClosed: 'Your organization can read what it has, but nothing can be changed until it buys.',
        suspendedTitle: 'Your organization is suspended',
        suspended: 'Your organization can read what it has, but nothing can be changed until the suspension is lifted.',
        openPortal: 'Open the customer portal'
    },
    requiresModule: {
        title: '{module} is not part of your plan',
        none: 'Your organization does not have {module}.',
        preview: 'Your organization can look at {module}, but not use it yet.',
        try: 'Try it',
        buy: 'Buy'
    },
    seats: {
        title: 'Seats',
        summary: '{held} of {capacity} seats in use',
        overCapacity: 'Your organization has {over} more people with seats than it has bought.',
        noCapacity: 'Your organization has no seats.',
        empty: 'Nobody holds a seat yet.',
        holderColumn: 'Name',
        emailColumn: 'Email',
        assignedColumn: 'Assigned',
        statusColumn: 'Status',
        licensed: 'Licensed',
        overCapacityStatus: 'Not covered',
        assign: 'Assign a seat',
        releaseShort: 'Release',
        release: 'Release the seat of {name}',
        assignTitle: 'Assign a seat',
        assignConfirm: 'Assign',
        name: 'Name',
        email: 'Email',
        identityProvider: 'Identity provider',
        subject: 'Subject at the identity provider',
        releaseTitle: 'Release the seat',
        releaseMessage: '{name} can no longer use the product until they are given a seat again. Release the seat?',
        readOnly: 'Seats cannot be changed while your organization is read-only.'
    },
    usage: {
        title: 'Usage',
        empty: 'This product measures nothing.',
        meterColumn: 'Meter',
        usedColumn: 'Used',
        allowanceColumn: 'Allowance',
        budgetColumn: 'Budget',
        concurrencyColumn: 'At the same time',
        usedOfAllowance: '{used} of {allowance} {unit}',
        noAllowance: '{used} {unit}, with no allowance',
        day: 'per day',
        month: 'per month',
        resets: 'starts over {date}',
        atAllowance: 'Allowance used up',
        atBudget: 'Budget reached',
        block: 'Work beyond the allowance is turned away.',
        park: 'Work beyond the allowance waits for the next period.',
        notSet: 'Not set',
        progress: 'Use of {meter}'
    },
    plan: {
        title: 'Plan and modules',
        modulesTitle: 'Modules',
        offersTitle: 'Plans and offers',
        noModules: 'This product has no modules.',
        noOffers: 'Nothing is on sale right now.',
        includedSeats: '{count} seats included',
        includedModules: 'Includes',
        perMonth: '{price} per month',
        perYear: '{price} per year',
        buy: 'Buy',
        try: 'Try',
        changePlan: 'Change plan',
        onTrial: 'You are on a trial with {days} days left.',
        withSeats: 'Your plan has {seats} seats.'
    }
};
