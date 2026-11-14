// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Cue.given;

namespace Cratis.Showrunner.Cue.Viewers.for_SignedInViewer.when_reading_the_subject;

/// <summary>
/// The subject of a person signed in with a <c>sub</c> claim is that claim.
/// </summary>
public class a_person_with_a_sub_claim : Specification
{
    SignedInViewer _viewer;

    void Establish() => _viewer = new(new FixedPrincipal(People.SignedInAs(People.Me)));

    [Fact] void should_be_signed_in() => _viewer.IsSignedIn.ShouldBeTrue();

    [Fact] void should_have_the_subject_of_the_claim() => _viewer.Subject.ShouldEqual(People.Me);
}
#endif
