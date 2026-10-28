// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Reporting.Capture;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Usher.Reporting.ReportError.given;

/// <summary>
/// A report of an error by the reporter in a tenant, with a store of errors the backend captured, holding the error of a
/// failing command that differs in every part from what the client sends.
/// </summary>
public class an_error_report : an_issue_command<ReportError>
{
    protected static readonly ErrorOwner Owner = new("fjord", Reporters.Identity);
    protected static readonly CapturedError Captured = new(
        new("Invoicing", "Invoices", "PayInvoice"),
        "Ledger.Invoicing.InvoiceAlreadyPaid",
        "Invoice 1042 is already paid",
        [new("Ledger.Invoicing.PayInvoice.Handle", "/src/Invoicing/PayInvoice.cs", 57)],
        "4.12.1");

    protected CapturedErrors _store;
    protected CorrelationId _failingCommand;

    void Establish()
    {
        var owners = Substitute.For<ICurrentErrorOwner>();
        owners.TryGet(out Arg.Any<ErrorOwner?>()).Returns(call =>
        {
            call[0] = Owner;
            return true;
        });
        _scenario.Services.AddSingleton(owners);

        _store = new(Options.Create(new UsherOptions { ApplicationName = "Ledger" }), TimeProvider.System);
        _scenario.Services.AddSingleton<ICapturedErrors>(_store);
        _failingCommand = Guid.NewGuid();
    }

    /// <summary>
    /// Reports the error the client saw, naming a command run just before and the failing command.
    /// </summary>
    /// <returns>The command.</returns>
    protected ReportError ReportOfTheFailingCommand() => Reports.Error(_issueId) with { CorrelationIds = [Guid.NewGuid(), _failingCommand] };
}
#endif
