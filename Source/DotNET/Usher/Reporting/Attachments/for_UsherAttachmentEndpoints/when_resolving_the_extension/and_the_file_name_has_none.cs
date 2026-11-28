// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Usher.Reporting.Attachments.for_UsherAttachmentEndpoints.when_resolving_the_extension;

public class and_the_file_name_has_none : Specification
{
    string _extension;

    void Because() => _extension = UsherAttachmentEndpoints.ExtensionFor(string.Empty, "image/jpeg");

    [Fact] void should_fall_back_to_the_content_type() => _extension.ShouldEqual(".jpg");
}
#endif
