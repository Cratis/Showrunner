// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
namespace Cratis.Showrunner.Usher.Reporting.Attachments.for_UsherAttachmentEndpoints.when_checking_whether_an_extension_is_served_inline;

public class and_it_is_an_allow_listed_image : Specification
{
    bool _isInline;

    void Because() => _isInline = UsherAttachmentEndpoints.IsAllowListedInlineExtension(".png");

    [Fact] void should_be_served_inline() => _isInline.ShouldBeTrue();
}
#endif
