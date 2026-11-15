// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.Notes.Collecting.Transport.given;
using Cratis.Showrunner.Notes.Contracts.Access;
using Cratis.Showrunner.Notes.Contracts.Transport;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Net.Http.Headers;

namespace Cratis.Showrunner.Notes.Collecting.Transport.for_NoteCollectingCalls;

/// <summary>
/// An application that presents its publication's secret reads the users the desk lets give feedback, with an entity tag: the
/// answer is complete the first time and empty-handed when it names the version it already has, and it moves on the moment the
/// desk decides something.
/// </summary>
public class when_asking_for_the_access : note_collecting_calls
{
    static readonly FeedbackGiverIdentity Ada = DeskDecisions.User("1234");
    static readonly FeedbackGiverIdentity Grace = DeskDecisions.User("5678");

    DefaultHttpContext _first;
    IResult _firstResult;
    DefaultHttpContext _unchanged;
    IResult _unchangedResult;
    DefaultHttpContext _afterRevoking;
    IResult _afterRevokingResult;

    async Task Because()
    {
        await DeskDecisions.Grant(_tenantStore.EventLog, Ada, 0);
        await DeskDecisions.Grant(_tenantStore.EventLog, Grace, 1);

        _first = Request("Bearer " + Secret);
        _firstResult = await _calls.Access(PublicationName, _first);

        _unchanged = Request("Bearer " + Secret);
        _unchanged.Request.Headers.IfNoneMatch = _first.Response.Headers[HeaderNames.ETag];
        _unchangedResult = await _calls.Access(PublicationName, _unchanged);

        await DeskDecisions.Revoke(_tenantStore.EventLog, Ada, 2);
        _afterRevoking = Request("Bearer " + Secret);
        _afterRevoking.Request.Headers.IfNoneMatch = _first.Response.Headers[HeaderNames.ETag];
        _afterRevokingResult = await _calls.Access(PublicationName, _afterRevoking);
    }

    static FeedbackAccessGrants Grants(IResult result) => ((JsonHttpResult<FeedbackAccessGrants>)result).Value!;

    [Fact] void should_answer_ok() => StatusOf(_firstResult).ShouldEqual(200);
    [Fact] void should_name_the_granted_users() => Grants(_firstResult).Givers.ShouldContainOnly(FeedbackGiverId.For(Ada).Value, FeedbackGiverId.For(Grace).Value);
    [Fact] void should_name_no_identity_provider_or_subject() => Grants(_firstResult).Givers.ShouldEachConformTo(giver => giver != Guid.Empty);
    [Fact] void should_tag_the_answer_with_its_version() => _first.Response.Headers[HeaderNames.ETag].ToString().ShouldEqual($"\"{Grants(_firstResult).Version}\"");
    [Fact] void should_ask_not_to_be_served_from_a_cache() => _first.Response.Headers[HeaderNames.CacheControl].ToString().ShouldEqual("no-cache");
    [Fact] void should_answer_not_modified_to_the_version_it_has() => StatusOf(_unchangedResult).ShouldEqual(304);
    [Fact] void should_repeat_the_tag_when_not_modified() => _unchanged.Response.Headers[HeaderNames.ETag].ToString().ShouldEqual(_first.Response.Headers[HeaderNames.ETag].ToString());
    [Fact] void should_answer_the_new_list_when_the_desk_decided_something() => StatusOf(_afterRevokingResult).ShouldEqual(200);
    [Fact] void should_no_longer_name_the_revoked_user() => Grants(_afterRevokingResult).Givers.ShouldContainOnly(FeedbackGiverId.For(Grace).Value);
    [Fact] void should_move_the_version_on() => Grants(_afterRevokingResult).Version.ShouldBeGreaterThan(Grants(_firstResult).Version);
}
#endif
