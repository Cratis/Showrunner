// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Usher.Reporting.Capture.for_ExceptionCapture;

public class when_getting_the_file_name_of_a_frame : Specification
{
    [Fact] void should_drop_the_folders_of_a_unix_path() => ExceptionCapture.FileNameOf("/home/build/src/Invoicing/PayInvoice.cs").Value.ShouldEqual("PayInvoice.cs");
    [Fact] void should_drop_the_folders_of_a_windows_path() => ExceptionCapture.FileNameOf(@"C:\agent\_work\src\Invoicing\PayInvoice.cs").Value.ShouldEqual("PayInvoice.cs");
    [Fact] void should_keep_a_bare_file_name() => ExceptionCapture.FileNameOf("PayInvoice.cs").Value.ShouldEqual("PayInvoice.cs");
}
#endif
