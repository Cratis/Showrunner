// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Usher.Reporting.Attachments.for_UsherAttachmentEndpoints.when_resolving_the_extension;

public class and_the_file_name_has_one : Specification
{
    string _extension;

    void Because() => _extension = UsherAttachmentEndpoints.ExtensionFor("screenshot.PNG", "application/octet-stream");

    [Fact] void should_use_the_file_names_own_extension_lowercased() => _extension.ShouldEqual(".png");
}
#endif
