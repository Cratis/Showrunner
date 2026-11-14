// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Security.Claims;
using Cratis.Showrunner.Cue.given;

namespace Cratis.Showrunner.Cue.Viewers.for_SignedInViewer.when_reading_the_subject;

/// <summary>
/// A claim on an identity that was never authenticated does not sign anyone in.
/// </summary>
public class a_claim_without_authentication : Specification
{
    SignedInViewer _viewer;

    void Establish() => _viewer = new(new FixedPrincipal(new ClaimsPrincipal(new ClaimsIdentity([new Claim("sub", "person-1")]))));

    [Fact] void should_not_be_signed_in() => _viewer.IsSignedIn.ShouldBeFalse();
}
#endif
