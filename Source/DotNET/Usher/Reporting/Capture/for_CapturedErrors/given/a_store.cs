// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Usher.Reporting.Capture.for_CapturedErrors.given;

/// <summary>
/// A store of captured errors on a clock the specification moves, holding nothing yet.
/// </summary>
public class a_store : Specification
{
    protected static readonly ErrorOwner Owner = new("fjord", "8c2f6a1e-reporter");
    protected static readonly CapturedError Error = new(
        new("Invoicing", "Invoices", "SendInvoice"),
        "System.InvalidOperationException",
        "The invoice is already sent",
        [new("Ledger.Invoicing.SendInvoice.Handle", "SendInvoice.cs", 42)],
        "4.12.1");

    protected UsherOptions _options;
    protected MovableClock _clock;
    protected CapturedErrors _store;
    protected CorrelationId _correlationId;

    void Establish()
    {
        _options = new() { ApplicationName = "Ledger" };
        _clock = new(new DateTimeOffset(2026, 10, 26, 12, 0, 0, TimeSpan.Zero));
        _store = new(Options.Create(_options), _clock);
        _correlationId = Guid.NewGuid();
    }
}
#endif
