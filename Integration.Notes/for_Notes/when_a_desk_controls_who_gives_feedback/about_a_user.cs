// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Notes.Access;
using Cratis.Showrunner.Notes.Access.Receiving;
using Cratis.Showrunner.Notes.Contracts.Access;
using Cratis.Showrunner.Notes.Integration.given;
using Cratis.Showrunner.Usher.Integration.given;

namespace Cratis.Showrunner.Notes.Integration.for_Notes.when_a_desk_controls_who_gives_feedback;

/// <summary>
/// An application that lets only granted users give feedback hears the desk's decisions through Chronicle's outbox and
/// inbox: the desk grants a user the right, revokes it and grants it again, and the application's state for that user - which
/// is what the commands decide on - follows each decision. A user the desk never decided about, and a decision for a tenant
/// the application does not have, change nothing.
/// </summary>
[Collection(ChronicleCollection.Name)]
public class about_a_user : Specification
{
    static readonly EventStoreNamespaceName _tenant = "fjord-logistics";
    static readonly FeedbackGiverIdentity _ada = new("github", "1234");
    static readonly FeedbackGiverIdentity _grace = new("github", "5678");
    static readonly FeedbackGiverIdentity _someoneElsewhere = new("github", "9012");

    AccessDesk _desk;
    Application _application;

    FeedbackGiver _beforeAnyDecision;
    FeedbackGiver _granted;
    AppendedEvent _grant;
    FeedbackGiver _revoked;
    FeedbackGiver _grantedAgain;
    FeedbackGiver _neverDecided;
    IReadOnlyList<AppendedEvent> _everythingRecordedForAda;
    IReadOnlyList<AppendedEvent> _unknownTenantEvents;

    async Task Because()
    {
        var infrastructure = ChronicleInfrastructure.Current;
        _desk = await AccessDesk.Connect(infrastructure);
        _application = await Application.Start(infrastructure, new EventStoreName($"Studio-{Guid.NewGuid():N}"), _desk.Name);
        var tenant = await _application.In(_tenant);

        _beforeAnyDecision = await StateOf(tenant, _ada);

        await _desk.Grant(_tenant.Value, _ada);
        _granted = await WaitForState(tenant, _ada, granted: true);
        _grant = await _application.WaitFor<DeskGrantedFeedbackAccess>(_tenant, _ada);

        await _desk.Revoke(_tenant.Value, _ada);
        _revoked = await WaitForState(tenant, _ada, granted: false);

        await _desk.Grant(_tenant.Value, _ada);
        _grantedAgain = await WaitForState(tenant, _ada, granted: true);

        // Another user is decided about last, so by the time it shows, the decisions about the first user are all in.
        await _desk.Grant(_tenant.Value, _grace);
        await WaitForState(tenant, _grace, granted: true);

        // A decision for a tenant the application does not have is never recorded.
        await _desk.Grant("somewhere-else", _someoneElsewhere);
        await _desk.Grant(_tenant.Value, new FeedbackGiverIdentity("github", "3456"));
        await WaitForState(tenant, new FeedbackGiverIdentity("github", "3456"), granted: true);

        _neverDecided = await StateOf(tenant, new FeedbackGiverIdentity("github", "7777"));
        _everythingRecordedForAda = await tenant.EventLog.GetFromSequenceNumber(EventSequenceNumber.First, FeedbackGiverId.For(_ada));
        _unknownTenantEvents = await (await _application.In(EventStoreNamespaceName.Default)).EventLog.GetFromSequenceNumber(EventSequenceNumber.First, FeedbackGiverId.For(_someoneElsewhere));
    }

    void Destroy()
    {
        _application?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _desk?.Dispose();
    }

    [Fact] void should_not_let_a_user_give_feedback_before_the_desk_decides() => _beforeAnyDecision.ShouldBeNull();
    [Fact] void should_let_the_user_give_feedback_when_the_desk_grants_it() => _granted.IsGranted.ShouldBeTrue();
    [Fact] void should_record_the_grant_for_the_user_with_the_user_as_the_subject() => _grant.Context.Subject.ShouldEqual(FeedbackGiverId.SubjectOf(_ada));
    [Fact] void should_record_who_granted_it() => ((DeskGrantedFeedbackAccess)_grant.Content).GrantedBy.Value.ShouldEqual("desk-operator-1");
    [Fact] void should_refuse_the_user_when_the_desk_revokes_it() => _revoked.IsGranted.ShouldBeFalse();
    [Fact] void should_let_the_user_give_feedback_again_when_the_desk_grants_it_again() => _grantedAgain.IsGranted.ShouldBeTrue();
    [Fact] void should_record_each_decision_once() => _everythingRecordedForAda.Count.ShouldEqual(3);
    [Fact] void should_not_let_a_user_the_desk_never_decided_about_give_feedback() => _neverDecided.ShouldBeNull();
    [Fact] void should_not_record_a_decision_for_a_tenant_the_application_does_not_have() => _unknownTenantEvents.ShouldBeEmpty();

    static async Task<FeedbackGiver> StateOf(IEventStore tenant, FeedbackGiverIdentity giver) =>
        await tenant.ReadModels.GetInstanceById<FeedbackGiver>(FeedbackGiverId.For(giver).Value.ToString("D"));

    static async Task<FeedbackGiver> WaitForState(IEventStore tenant, FeedbackGiverIdentity giver, bool granted) =>
        await Eventually.Get(
            async () => await StateOf(tenant, giver) is { } state && state.IsGranted == granted ? state : null,
            $"the feedback state of {giver.IdentityProvider.Value}:{giver.Subject.Value} to be {(granted ? "granted" : "revoked")}");
}
