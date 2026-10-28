// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.Reporting.Capture.for_ExceptionCapture.given;
using Microsoft.Extensions.DependencyInjection;

using ArcSliceLocation = Cratis.Arc.Slices.SliceLocation;

namespace Cratis.Showrunner.Usher.Reporting.Capture.for_CaptureFailedCommands.given;

/// <summary>
/// A command that failed with a thrown exception, in a scope with Usher added, run by the reporter in a tenant.
/// </summary>
public class a_failing_command : Specification
{
    protected static readonly ErrorOwner Owner = new("fjord", new("entra", "8c2f6a1e-reporter"));

    protected IServiceCollection _services;
    protected ICurrentErrorOwner _owners;
    protected CaptureFailedCommands _mapper;
    protected Guid _correlationId;
    protected Exception _exception;
    protected bool _mapped;
    protected CommandResult? _result;

    void Establish()
    {
        _owners = Substitute.For<ICurrentErrorOwner>();
        _owners.TryGet(out Arg.Any<ErrorOwner?>()).Returns(call =>
        {
            call[0] = Owner;
            return true;
        });

        _services = new ServiceCollection();
        _services.AddSingleton(_owners);
        _services.AddUsher(usher =>
        {
            usher.ApplicationName = "Ledger";
            usher.ApplicationVersion = "4.12.1";
        });

        _mapper = new(Microsoft.Extensions.Logging.Abstractions.NullLogger<CaptureFailedCommands>.Instance);
        _correlationId = Guid.NewGuid();
        _exception = Thrower.Thrown();
    }

    /// <summary>
    /// Offers the exception to the mapper as Arc does when the command fails.
    /// </summary>
    /// <param name="services">The command's services.</param>
    protected void Fail(IServiceProvider services)
    {
        var context = new CommandContext(_correlationId, typeof(object), new object(), [], new CommandContextValues(), ServiceProvider: services)
        {
            SliceLocation = new ArcSliceLocation("Invoicing", "Invoices", "SendInvoice")
        };
        _mapped = _mapper.TryMap(context, _exception, out _result);
    }

    /// <summary>
    /// Gets what the store holds for the reporter and the failing command.
    /// </summary>
    /// <param name="services">The command's services.</param>
    /// <returns>The captured error, if any.</returns>
    protected CapturedError? CapturedIn(IServiceProvider services) =>
        services.GetRequiredService<ICapturedErrors>().TryGet(Owner, (CorrelationId)_correlationId, out var error) ? error : null;
}
#endif
