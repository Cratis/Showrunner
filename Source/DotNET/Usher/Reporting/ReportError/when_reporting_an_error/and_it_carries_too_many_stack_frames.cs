// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.given;

namespace Cratis.Showrunner.Usher.Reporting.ReportError.when_reporting_an_error;

public class and_it_carries_too_many_stack_frames : an_issue_command<ReportError>
{
    async Task Because() => _result = await _scenario.Execute(Reports.Error(_issueId) with { StackFrames = [.. Enumerable.Range(0, ReportLimits.StackFrames + 1).Select(_ => new StackFrame("Ledger.Invoicing.Send", "SendInvoice.cs", 42))] });

    [Fact] void should_not_succeed() => _result.ShouldNotBeSuccessful();
    [Fact] void should_have_validation_errors() => _result.ShouldHaveValidationErrors();
    [Fact] void should_not_append_anything() => _scenario.AppendedEvents.ShouldBeEmpty();
}
#endif
