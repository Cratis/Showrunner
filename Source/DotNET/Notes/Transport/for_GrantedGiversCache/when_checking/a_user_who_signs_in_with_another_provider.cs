// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Contracts.Access;
using Cratis.Showrunner.Notes.given;
using Cratis.Showrunner.Notes.Transport.given;

namespace Cratis.Showrunner.Notes.Transport.for_GrantedGiversCache.when_checking;

/// <summary>
/// A grant names the user by identity provider and subject, so the same subject at another provider is not granted.
/// </summary>
public class a_user_who_signs_in_with_another_provider : a_granted_givers_cache
{
    async Task Because()
    {
        Serve(4, TheGiver);
        await _cache.Refresh();
    }

    [Fact] void should_not_grant_the_same_subject_at_another_provider() => _cache.IsGranted(FeedbackGiverId.For(new(new("aad"), Givers.Identity.Subject))).ShouldBeFalse();
}
#endif
