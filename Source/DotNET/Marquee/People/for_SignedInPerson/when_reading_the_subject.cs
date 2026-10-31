// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Identity;
using Cratis.Showrunner.Marquee.given;

namespace Cratis.Showrunner.Marquee.People.for_SignedInPerson;

/// <summary>
/// The person is the subject claim of the signed-in principal.
/// </summary>
public class when_reading_the_subject : Specification
{
    SignedInPerson _signedIn;
    SignedInPerson _anonymous;
    SignedInPerson _withoutSubject;

    void Establish()
    {
        _signedIn = new(new FixedPrincipal(Viewers.SignedInAs(Viewers.Me)));
        _anonymous = new(new FixedPrincipal(Viewers.Anonymous));
        _withoutSubject = new(new FixedPrincipal(new ClaimsPrincipal(new ClaimsIdentity([new Claim("name", "somebody")], "Specifications"))));
    }

    [Fact] void should_be_signed_in() => _signedIn.IsSignedIn.ShouldBeTrue();

    [Fact] void should_have_the_subject() => _signedIn.Subject.ShouldEqual(Viewers.Me);

    [Fact] void should_not_be_signed_in_when_the_principal_is_not_authenticated() => _anonymous.IsSignedIn.ShouldBeFalse();

    [Fact] void should_not_be_signed_in_without_a_subject() => _withoutSubject.IsSignedIn.ShouldBeFalse();

    [Fact] void should_be_nobody_without_a_subject() => _withoutSubject.Subject.ShouldEqual(UserSubject.NotSet);
}
#endif
