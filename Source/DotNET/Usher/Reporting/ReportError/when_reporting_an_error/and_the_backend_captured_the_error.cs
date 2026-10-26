// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Reporting.ReportError.given;

namespace Cratis.Showrunner.Usher.Reporting.ReportError.when_reporting_an_error;

public class and_the_backend_captured_the_error : an_error_report
{
    void Establish() => _store.Capture(Owner, _failingCommand, Captured);

    async Task Because() => _result = await _scenario.Execute(ReportOfTheFailingCommand());

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_record_the_error_as_the_backend_saw_it() =>
        await _scenario.ShouldHaveAppendedEvent<ReportError, ReporterFlaggedError>(_issueId, e =>
            e.Location == Captured.Location &&
            e.ErrorType == Captured.ErrorType &&
            e.ErrorMessage == Captured.ErrorMessage &&
            e.StackFrames.SequenceEqual(Captured.StackFrames) &&
            e.ApplicationVersion == Captured.ApplicationVersion);

    [Fact]
    async Task should_keep_the_correlation_ids_the_client_sent() =>
        await _scenario.ShouldHaveAppendedEvent<ReportError, ReporterFlaggedError>(_issueId, e => e.CorrelationIds.Count() == 2 && e.CorrelationIds.Last() == _failingCommand);

    [Fact]
    async Task should_fingerprint_the_error_the_backend_saw() =>
        await _scenario.ShouldHaveAppendedEvent<ReportError, ReporterFlaggedError>(_issueId, e => e.Fingerprint == new ErrorFingerprints().Compute(
            Captured.ApplicationVersion,
            Captured.Location,
            Captured.ErrorType,
            Captured.ErrorMessage,
            Captured.StackFrames));
}
#endif
