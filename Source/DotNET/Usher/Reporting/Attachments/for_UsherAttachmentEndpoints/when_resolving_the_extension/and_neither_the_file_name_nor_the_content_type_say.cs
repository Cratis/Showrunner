// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Usher.Reporting.Attachments.for_UsherAttachmentEndpoints.when_resolving_the_extension;

public class and_neither_the_file_name_nor_the_content_type_say : Specification
{
    string _extension;

    void Because() => _extension = UsherAttachmentEndpoints.ExtensionFor(string.Empty, "application/octet-stream");

    [Fact] void should_be_empty() => _extension.ShouldBeEmpty();
}
#endif
