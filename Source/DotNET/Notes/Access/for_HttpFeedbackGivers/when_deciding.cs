// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Arc.Validation;
using Cratis.Monads;
using Cratis.Showrunner.Notes.Contracts.Access;
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Identity;
using Cratis.Showrunner.Notes.Transport;
using Cratis.Showrunner.Notes.Transport.given;
using Microsoft.Extensions.Options;

namespace Cratis.Showrunner.Notes.Access.for_HttpFeedbackGivers;

/// <summary>
/// Over the HTTP transport the decision is read from the list the collector serves, and everything the user sees is what the
/// same-kernel transport shows: a permit for a granted user, the one refusal for everyone else, and availability to match.
/// </summary>
public class when_deciding : Specification
{
    RecordingCollector _collector;
    GrantedGiversCache _cache;
    FeedbackAccess _access;
    Result<FeedbackPermit, ValidationResult> _before;
    bool _mayBefore;
    Result<FeedbackPermit, ValidationResult> _granted;
    bool _mayGranted;
    Result<FeedbackPermit, ValidationResult> _revoked;
    bool _mayRevoked;

    async Task Because()
    {
        var options = Options.Create(new NotesOptions { Transport = NotesTransport.Http, AccessMode = FeedbackAccessMode.GrantedUsers });
        _collector = new();
        _cache = new(_collector, options, new SteppedTime());
        _access = new(options, new SignedInGiver(new FixedPrincipal(Givers.SignedInAs(Givers.UserId))), new HttpFeedbackGivers(_cache));

        _before = await _access.Permit();
        _mayBefore = await _access.MayGiveFeedback();

        _collector.Grants = new(1, [FeedbackGiverId.For(Givers.Identity).Value]);
        await _cache.Refresh();
        _granted = await _access.Permit();
        _mayGranted = await _access.MayGiveFeedback();

        _collector.Grants = new(2, []);
        await _cache.Refresh();
        _revoked = await _access.Permit();
        _mayRevoked = await _access.MayGiveFeedback();
    }

    [Fact] void should_refuse_before_any_list_was_fetched() => _before.IsSuccess.ShouldBeFalse();
    [Fact] void should_refuse_with_the_same_words_as_in_the_same_kernel() => ((ValidationResult)_before).Message.ShouldEqual(FeedbackAccess.Refusal);
    [Fact] void should_not_advise_showing_it_before_any_list_was_fetched() => _mayBefore.ShouldBeFalse();
    [Fact] void should_permit_the_granted_user() => _granted.IsSuccess.ShouldBeTrue();
    [Fact] void should_give_the_note_as_the_signed_in_user() => ((FeedbackPermit)_granted).Giver.ShouldEqual(Givers.Giver);
    [Fact] void should_advise_showing_it_to_the_granted_user() => _mayGranted.ShouldBeTrue();
    [Fact] void should_refuse_the_revoked_user_with_the_same_words() => ((ValidationResult)_revoked).Message.ShouldEqual(FeedbackAccess.Refusal);
    [Fact] void should_not_advise_showing_it_to_the_revoked_user() => _mayRevoked.ShouldBeFalse();
}
#endif
