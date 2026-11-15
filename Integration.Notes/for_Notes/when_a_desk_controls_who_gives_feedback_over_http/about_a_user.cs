// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Collecting.Transport;
using Cratis.Showrunner.Notes.Contracts.Access;
using Cratis.Showrunner.Notes.Integration.given;
using Cratis.Showrunner.Notes.Transport;
using Cratis.Showrunner.Usher.Integration.given;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Notes.Integration.for_Notes.when_a_desk_controls_who_gives_feedback_over_http;

/// <summary>
/// An application that runs its own kernel and lets only granted users give feedback hears the desk through the collector:
/// the desk grants a user the right through Chronicle's outbox, the collector records it and serves the list over HTTP, and the
/// application allows the user; the desk revokes it and, within a poll, the application refuses them.
/// </summary>
[Collection(ChronicleCollection.Name)]
public class about_a_user : Specification
{
    const string Secret = "integration-secret-of-the-publication";
    static readonly EventStoreNamespaceName _tenant = "fjord-logistics";
    static readonly FeedbackGiverIdentity _ada = new("github", "1234");
    static readonly FeedbackGiverIdentity _grace = new("github", "5678");

    AccessDesk _desk;
    HttpCollector _collector;
    Application _application;

    bool _grantedBeforeAnyDecision;
    bool _grantedAfterTheGrant;
    bool _otherUserGrantedAfterTheGrant;
    bool _grantedAfterTheRevocation;
    bool _otherUserStillGranted;
    IReadOnlyList<AppendedEvent> _recordedByTheApplication;

    async Task Because()
    {
        var infrastructure = ChronicleInfrastructure.Current;
        _desk = await AccessDesk.Connect(infrastructure);
        _collector = await HttpCollector.Start(infrastructure, _desk.Name, new NotesPublication(_tenant.Value, "Ledger"), "pub-7f3a", Secret);

        // The tenant namespace has to exist in the collecting store for the desk's decisions to be forwarded into it.
        await _collector.In(_tenant);
        _application = await Application.StartOverHttp(infrastructure, _collector.Url, Secret, _tenant.Value);
        var cache = _application.Services.GetRequiredService<GrantedGiversCache>();

        await Eventually.Get(() => Task.FromResult(cache.HasList ? new object() : null), "the application to have fetched the list of granted users");
        _grantedBeforeAnyDecision = cache.IsGranted(FeedbackGiverId.For(_ada));

        await _desk.Grant(_tenant.Value, _ada);
        await Granted(cache, _ada, true);
        _grantedAfterTheGrant = cache.IsGranted(FeedbackGiverId.For(_ada));
        _otherUserGrantedAfterTheGrant = cache.IsGranted(FeedbackGiverId.For(_grace));

        await _desk.Grant(_tenant.Value, _grace);
        await Granted(cache, _grace, true);

        await _desk.Revoke(_tenant.Value, _ada);
        await Granted(cache, _ada, false);
        _grantedAfterTheRevocation = cache.IsGranted(FeedbackGiverId.For(_ada));
        _otherUserStillGranted = cache.IsGranted(FeedbackGiverId.For(_grace));

        // Over HTTP the desk's decisions are not recorded in the application's own event store.
        _recordedByTheApplication = await (await _application.In(_tenant)).EventLog.GetFromSequenceNumber(EventSequenceNumber.First, FeedbackGiverId.For(_ada));
    }

    void Destroy()
    {
        _application?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _collector?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _desk?.Dispose();
    }

    [Fact] void should_refuse_everyone_before_the_desk_decides() => _grantedBeforeAnyDecision.ShouldBeFalse();
    [Fact] void should_allow_the_user_the_desk_granted() => _grantedAfterTheGrant.ShouldBeTrue();
    [Fact] void should_not_allow_a_user_the_desk_did_not_grant() => _otherUserGrantedAfterTheGrant.ShouldBeFalse();
    [Fact] void should_refuse_the_user_the_desk_revoked() => _grantedAfterTheRevocation.ShouldBeFalse();
    [Fact] void should_keep_allowing_the_user_the_desk_still_lets() => _otherUserStillGranted.ShouldBeTrue();
    [Fact] void should_not_record_the_decisions_in_the_applications_own_event_store() => _recordedByTheApplication.ShouldBeEmpty();

    static async Task Granted(GrantedGiversCache cache, FeedbackGiverIdentity giver, bool granted) =>
        await Eventually.Get(
            () => Task.FromResult(cache.IsGranted(FeedbackGiverId.For(giver)) == granted ? new object() : null),
            $"the application to {(granted ? "allow" : "refuse")} {giver.Subject.Value}");
}
