// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Cue.given;

namespace Cratis.Showrunner.Cue.Viewers.for_SignedInViewer.when_reading_the_subject;

/// <summary>
/// An anonymous principal is nobody.
/// </summary>
public class nobody_signed_in : Specification
{
    SignedInViewer _viewer;

    void Establish() => _viewer = new(new FixedPrincipal(People.Anonymous));

    [Fact] void should_not_be_signed_in() => _viewer.IsSignedIn.ShouldBeFalse();

    [Fact] void should_have_no_subject() => _viewer.Subject.ShouldEqual(ViewerSubject.NotSet);
}
#endif
