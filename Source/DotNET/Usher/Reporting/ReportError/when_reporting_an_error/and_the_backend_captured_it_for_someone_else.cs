// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.given;
using Cratis.Showrunner.Usher.Reporting.ReportError.given;

namespace Cratis.Showrunner.Usher.Reporting.ReportError.when_reporting_an_error;

public class and_the_backend_captured_it_for_someone_else : an_error_report
{
    void Establish() => _store.Capture(Owner with { Reporter = Reporters.SomeoneElsesIdentity }, _failingCommand, Captured);

    async Task Because() => _result = await _scenario.Execute(ReportOfTheFailingCommand());

    [Fact] void should_succeed() => _result.ShouldBeSuccessful();

    [Fact]
    async Task should_record_the_error_as_the_client_sent_it() =>
        await _scenario.ShouldHaveAppendedEvent<ReportError, ReporterFlaggedError>(_issueId, e =>
            e.Location == IssueEvents.Location &&
            e.ErrorType == "System.NullReferenceException" &&
            e.ErrorMessage == "Object reference not set to an instance of an object." &&
            e.ApplicationVersion == "1.2.3");
}
#endif
