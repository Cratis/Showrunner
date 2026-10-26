// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Reporting.ReportError;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute.ExceptionExtensions;

namespace Cratis.Showrunner.Usher.Reporting.Capture.for_CaptureFailedCommands.when_a_command_fails;

/// <summary>
/// Any failing command is captured; recording a reported error is used here because it is a command Usher has, made to
/// fail by a fingerprint algorithm that throws.
/// </summary>
public class in_the_command_pipeline : an_issue_command<ReportError.ReportError>
{
    static readonly ErrorOwner Owner = new("fjord", Reporters.Subject);
    CapturedErrors _store;

    void Establish()
    {
        var fingerprints = Substitute.For<IComputeErrorFingerprints>();
        fingerprints
            .Compute(Arg.Any<ApplicationVersion>(), Arg.Any<SliceLocation>(), Arg.Any<ErrorType>(), Arg.Any<ErrorMessage>(), Arg.Any<IEnumerable<StackFrame>>())
            .Throws(new InvalidOperationException("The fingerprint store is unavailable"));
        _scenario.Services.AddSingleton(fingerprints);

        var owners = Substitute.For<ICurrentErrorOwner>();
        owners.TryGet(out Arg.Any<ErrorOwner?>()).Returns(call =>
        {
            call[0] = Owner;
            return true;
        });
        _scenario.Services.AddSingleton(owners);

        _store = new(Microsoft.Extensions.Options.Options.Create(new UsherOptions { ApplicationName = "Ledger", ApplicationVersion = "4.12.1" }), TimeProvider.System);
        _scenario.Services.AddSingleton<ICapturedErrors>(_store);
    }

    bool _captured;
    CapturedError? _error;

    async Task Because()
    {
        _result = await _scenario.Execute(Reports.Error(_issueId));
        _captured = _store.TryGet(Owner, _result.CorrelationId.Value, out _error);
    }

    [Fact] void should_fail_with_its_own_exception() => _result.ExceptionMessages.ShouldContain("The fingerprint store is unavailable");
    [Fact] void should_capture_the_error_for_the_correlation_id_of_the_command() => _captured.ShouldBeTrue();
    [Fact] void should_capture_where_the_command_belongs() => _error!.Location.ShouldEqual(new SliceLocation(ModuleName.NotSet, "Reporting", "ReportError"));
    [Fact] void should_capture_the_error_type() => _error!.ErrorType.Value.ShouldEqual("System.InvalidOperationException");
    [Fact] void should_capture_the_message() => _error!.ErrorMessage.Value.ShouldEqual("The fingerprint store is unavailable");
}
#endif
