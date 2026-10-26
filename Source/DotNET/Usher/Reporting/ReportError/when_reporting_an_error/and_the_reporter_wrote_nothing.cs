// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Reporting.ReportError.when_reporting_an_error;

public class and_the_reporter_wrote_nothing : an_issue_command<ReportError>
{
    async Task Because() => _result = await _scenario.Execute(Reports.Error(_issueId));

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();
    [Fact] void should_only_append_that_the_reporter_flagged_the_error() =>
        _scenario.AppendedEvents.Select(_ => _.Event.Content.GetType()).ShouldEqual([typeof(ReporterFlaggedError)]);

    [Fact]
    async Task should_flag_the_error_as_reported() =>
        await _scenario.ShouldHaveAppendedEvent<ReportError, ReporterFlaggedError>(_issueId, e =>
            e.ErrorMessage == "Object reference not set to an instance of an object." &&
            e.ErrorType == "System.NullReferenceException" &&
            e.StackFrames.Single() == new StackFrame("Ledger.Invoicing.Send", "SendInvoice.cs", 42) &&
            e.Location == IssueEvents.Location &&
            e.ApplicationName == "Ledger" &&
            e.ApplicationVersion == "1.2.3" &&
            e.ReporterSubject == Reporters.Subject);

    [Fact]
    async Task should_not_have_a_fingerprint_until_fingerprinting_is_built() =>
        await _scenario.ShouldHaveAppendedEvent<ReportError, ReporterFlaggedError>(_issueId, e => e.Fingerprint == ErrorFingerprint.NotSet);
}
#endif
