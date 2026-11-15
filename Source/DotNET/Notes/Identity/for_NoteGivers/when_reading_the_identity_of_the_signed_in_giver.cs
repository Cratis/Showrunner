// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Arc.Identity;
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Identity.for_NoteGivers;

/// <summary>
/// A desk names a user by identity provider and subject, so the application tells who is signed in the same way.
/// </summary>
public class when_reading_the_identity_of_the_signed_in_giver : Specification
{
    SignedInGiver _signedIn;
    SignedInGiver _anonymous;
    SignedInGiver _withoutProvider;

    void Because()
    {
        _signedIn = new(new FixedPrincipal(Givers.SignedInAs(Givers.UserId)));
        _anonymous = new(new FixedPrincipal(Givers.Anonymous()));
        _withoutProvider = new(new FixedPrincipal(new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "abc")], "Specifications"))));
    }

    [Fact] void should_read_the_provider() => _signedIn.Identity.IdentityProvider.ShouldEqual(Givers.IdentityProvider);
    [Fact] void should_read_the_subject() => _signedIn.Identity.Subject.Value.ShouldEqual(Givers.UserId.Value);
    [Fact] void should_name_nobody_when_anonymous() => _anonymous.Identity.Subject.Value.ShouldBeEmpty();
    [Fact] void should_have_no_provider_when_the_principal_names_none() => _withoutProvider.Identity.IdentityProvider.Value.ShouldBeEmpty();
    [Fact] void should_still_read_the_subject_when_the_principal_names_no_provider() => _withoutProvider.Identity.Subject.Value.ShouldEqual("abc");
}
#endif
