// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Notes.given;

namespace Cratis.Showrunner.Notes.Identity.for_NoteGivers;

public class when_reading_the_signed_in_giver : Specification
{
    SignedInGiver _signedIn;
    SignedInGiver _anonymous;
    SignedInGiver _withNameIdentifier;

    void Because()
    {
        _signedIn = new(new FixedPrincipal(Givers.SignedInAs(Givers.UserId)));
        _anonymous = new(new FixedPrincipal(Givers.Anonymous()));
        _withNameIdentifier = new(new FixedPrincipal(new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "abc"), new Claim(ClaimTypes.Name, "Grace")], "Specifications"))));
    }

    [Fact] void should_be_signed_in() => _signedIn.IsSignedIn.ShouldBeTrue();
    [Fact] void should_read_the_subject() => _signedIn.UserId.ShouldEqual(Givers.UserId);
    [Fact] void should_read_the_name() => _signedIn.Giver.DisplayName.ShouldEqual(Givers.DisplayName);
    [Fact] void should_not_be_signed_in_when_anonymous() => _anonymous.IsSignedIn.ShouldBeFalse();
    [Fact] void should_have_no_user_id_when_anonymous() => _anonymous.UserId.ShouldEqual(GiverUserId.NotSet);
    [Fact] void should_fall_back_to_the_name_identifier_claim() => _withNameIdentifier.UserId.ShouldEqual(new GiverUserId("abc"));
    [Fact] void should_fall_back_to_the_name_claim() => _withNameIdentifier.Giver.DisplayName.ShouldEqual(new GiverDisplayName("Grace"));
}
#endif
