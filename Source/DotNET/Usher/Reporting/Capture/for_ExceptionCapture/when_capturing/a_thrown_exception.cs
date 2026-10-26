// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.Reporting.Capture.for_ExceptionCapture.given;

namespace Cratis.Showrunner.Usher.Reporting.Capture.for_ExceptionCapture.when_capturing;

public class a_thrown_exception : Specification
{
    static readonly SliceLocation Location = new("Invoicing", "Invoices", "SendInvoice");
    CapturedError _error;

    void Because() => _error = ExceptionCapture.Capture(Thrower.Thrown(), Location, "4.12.1", 50);

    [Fact] void should_have_the_location() => _error.Location.ShouldEqual(Location);
    [Fact] void should_have_the_full_type_name() => _error.ErrorType.Value.ShouldEqual("System.InvalidOperationException");
    [Fact] void should_have_the_message() => _error.ErrorMessage.Value.ShouldEqual(Thrower.Message);
    [Fact] void should_have_the_version() => _error.ApplicationVersion.Value.ShouldEqual("4.12.1");
    [Fact] void should_have_the_throwing_method_as_the_top_frame() => _error.StackFrames.First().Method.Value.ShouldEqual($"{typeof(Thrower).FullName}.Throw");
    [Fact] void should_have_the_file_of_the_top_frame() => _error.StackFrames.First().File.Value.ShouldMatch(file => file.EndsWith("Thrower.cs", StringComparison.Ordinal));
    [Fact] void should_have_the_line_of_the_top_frame() => _error.StackFrames.First().Line.Value.ShouldBeGreaterThan(0);
    [Fact] void should_have_the_catching_method_below() => _error.StackFrames.Skip(1).First().Method.Value.ShouldEqual($"{typeof(Thrower).FullName}.Thrown");
}
#endif
