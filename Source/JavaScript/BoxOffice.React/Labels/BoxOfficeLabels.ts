// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

/**
 * Every text the Box Office components show or announce. Placeholders in braces, such as `{days}`, are filled in by
 * the components.
 */
export interface BoxOfficeLabels {
    /** Texts shared by several components. */
    common: {
        /** The cancel button. */
        cancel: string;
        /** The accessible name of a dialog's close button. */
        close: string;
        /** Shown while something loads. */
        loading: string;
        /** Shown when something could not be loaded. */
        failed: string;
        /** Shown when the user may not see something. */
        unauthorized: string;
        /** Shown when the customer portal could not be opened. */
        portalFailed: string;
        /** Shown instead of a portal button when the product has no customer portal. */
        noPortal: string;
    };

    /** What a module's access is called. */
    moduleAccess: {
        /** The tenant has no access. */
        none: string;
        /** The tenant may look at the module but not use it. */
        preview: string;
        /** The tenant has the module on trial. */
        trial: string;
        /** The tenant has the module. */
        full: string;
        /** Says when access ends. `{date}` is the last day. */
        until: string;
    };

    /** The banner that follows a trial. */
    trialBanner: {
        /** A trial that runs. `{days}` is how many days are left. */
        running: string;
        /** A trial that ends today. */
        lastDay: string;
        /** A trial that has closed. */
        closed: string;
        /** The button that opens the customer portal to buy. */
        buy: string;
    };

    /** The banner for a suspended tenant. */
    suspension: {
        /** The title. */
        title: string;
        /** Why, when the trial ended without seats. */
        trialEndedWithoutSeats: string;
        /** Why, when a payment is overdue. */
        paymentOverdue: string;
        /** Why, when an administrator decided it. */
        administratorDecision: string;
        /** What to do about it. */
        resolve: string;
        /** The button that opens the customer portal. */
        openPortal: string;
    };

    /** What a user without a license is told. */
    noLicense: {
        /** The title when the user has no seat. */
        noSeatTitle: string;
        /** What to do when the user has no seat. */
        noSeat: string;
        /** The title when more people hold seats than the tenant has. */
        overCapacityTitle: string;
        /** What to do when more people hold seats than the tenant has. */
        overCapacity: string;
        /** The title when the trial has closed. */
        trialClosedTitle: string;
        /** What to do when the trial has closed. */
        trialClosed: string;
        /** The title when the tenant is suspended. */
        suspendedTitle: string;
        /** What to do when the tenant is suspended. */
        suspended: string;
        /** The button that opens the customer portal. */
        openPortal: string;
    };

    /** What the user sees where a module they do not have would be. */
    requiresModule: {
        /** The title. `{module}` is the module. */
        title: string;
        /** The explanation when the tenant has no access. */
        none: string;
        /** The explanation when the tenant may only preview the module. */
        preview: string;
        /** The button that opens the customer portal to try the module. */
        try: string;
        /** The button that opens the customer portal to buy the module. */
        buy: string;
    };

    /** The seats page. */
    seats: {
        /** The title. */
        title: string;
        /** How many seats are used. `{held}` and `{capacity}` are the numbers. */
        summary: string;
        /** Shown when more people hold seats than the tenant has. `{over}` is how many are over. */
        overCapacity: string;
        /** Shown when the tenant has no seats at all. */
        noCapacity: string;
        /** Shown when nobody holds a seat. */
        empty: string;
        /** The column of names. */
        holderColumn: string;
        /** The column of email addresses. */
        emailColumn: string;
        /** The column of when a seat was assigned. */
        assignedColumn: string;
        /** The column of seat status. */
        statusColumn: string;
        /** A seat that is licensed. */
        licensed: string;
        /** A seat the tenant has no room for. */
        overCapacityStatus: string;
        /** The button that assigns a seat. */
        assign: string;
        /** The text of the button that releases a seat. */
        releaseShort: string;
        /** The accessible name of the button that releases a seat. `{name}` is the person. */
        release: string;
        /** The title of the assign dialog. */
        assignTitle: string;
        /** The assign dialog's confirm button. */
        assignConfirm: string;
        /** The person's name field. */
        name: string;
        /** The person's email field. */
        email: string;
        /** The identity provider field. */
        identityProvider: string;
        /** The person's subject at the identity provider. */
        subject: string;
        /** The title of the confirmation to release a seat. */
        releaseTitle: string;
        /** The message of the confirmation to release a seat. `{name}` is the person. */
        releaseMessage: string;
        /** Said when seats cannot be changed because the tenant is read-only. */
        readOnly: string;
    };

    /** The usage page. */
    usage: {
        /** The title. */
        title: string;
        /** Shown when the product measures nothing. */
        empty: string;
        /** The column of meters. */
        meterColumn: string;
        /** The column of what is used. */
        usedColumn: string;
        /** The column of the allowance. */
        allowanceColumn: string;
        /** The column of the budget. */
        budgetColumn: string;
        /** The column of concurrency limits. */
        concurrencyColumn: string;
        /** How much is used of the allowance. `{used}`, `{allowance}` and `{unit}` are the values. */
        usedOfAllowance: string;
        /** Said when the operator set no allowance. `{used}` and `{unit}` are the values. */
        noAllowance: string;
        /** The period of an allowance that counts per day. */
        day: string;
        /** The period of an allowance that counts per month. */
        month: string;
        /** Says when the period starts over. `{date}` is the day. */
        resets: string;
        /** Said when the allowance is used up. */
        atAllowance: string;
        /** Said when the budget is reached. */
        atBudget: string;
        /** Said of work beyond the allowance that is turned away. */
        block: string;
        /** Said of work beyond the allowance that waits for the next period. */
        park: string;
        /** Shown for a budget or limit nobody set. */
        notSet: string;
        /** The accessible name of a meter's progress bar. `{meter}` is the meter. */
        progress: string;
    };

    /** The plan and modules page. */
    plan: {
        /** The title. */
        title: string;
        /** The heading of the modules. */
        modulesTitle: string;
        /** The heading of the offers. */
        offersTitle: string;
        /** Shown when no module is declared. */
        noModules: string;
        /** Shown when nothing is on sale. */
        noOffers: string;
        /** The seats an offer includes. `{count}` is the number. */
        includedSeats: string;
        /** The modules an offer includes. */
        includedModules: string;
        /** A price per month. `{price}` is the amount with its currency. */
        perMonth: string;
        /** A price per year. `{price}` is the amount with its currency. */
        perYear: string;
        /** The button that opens the customer portal to buy an offer. */
        buy: string;
        /** The button that opens the customer portal to try a module. */
        try: string;
        /** The button that opens the customer portal to change plan. */
        changePlan: string;
        /** Shown for a tenant on a trial. `{days}` is how many days are left. */
        onTrial: string;
        /** Shown for a tenant with seats. `{seats}` is how many. */
        withSeats: string;
    };
}

/**
 * Makes every property of a type optional, all the way down.
 */
export type DeepPartial<T> = {
    [TKey in keyof T]?: T[TKey] extends object ? DeepPartial<T[TKey]> : T[TKey];
};

/**
 * The labels of the Box Office components that a solution replaces, for instance to localize them. Labels left out
 * keep their English defaults.
 */
export type BoxOfficeLabelOverrides = DeepPartial<BoxOfficeLabels>;
