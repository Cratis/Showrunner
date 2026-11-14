// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Cue.given;

namespace Cratis.Showrunner.Cue.Viewers.for_SignedInViewer.when_reading_the_subject;

/// <summary>
/// Without a <c>sub</c> claim the name identifier is the subject.
/// </summary>
public class a_person_with_a_name_identifier_claim : Specification
{
    SignedInViewer _viewer;

    void Establish() => _viewer = new(new FixedPrincipal(new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "person-7")], "Specifications"))));

    [Fact] void should_be_signed_in() => _viewer.IsSignedIn.ShouldBeTrue();

    [Fact] void should_have_the_name_identifier_as_subject() => _viewer.Subject.Value.ShouldEqual("person-7");
}
#endif
