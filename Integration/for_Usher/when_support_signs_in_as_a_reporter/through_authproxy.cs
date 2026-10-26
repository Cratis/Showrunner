// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Text.Json;
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Sessions;
using Cratis.Showrunner.Usher.Contracts.Conversation;
using Cratis.Showrunner.Usher.Contracts.Identity;
using Cratis.Showrunner.Usher.Conversation.AddDetails;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;
using Cratis.Showrunner.Usher.Integration.given;
using Cratis.Showrunner.Usher.Reporting.ReportIssue;
using Cratis.Showrunner.Usher.Viewing;

namespace Cratis.Showrunner.Usher.Integration.for_Usher.when_support_signs_in_as_a_reporter;

/// <summary>
/// Support signs in as a reporter through AuthProxy's on-behalf session, acts on the issue, and the reporter revokes the
/// grant. AuthProxy is played by <see cref="AuthProxy"/>, which calls the application exactly as AuthProxy does: the
/// same paths, headers and signed tokens.
/// </summary>
[Collection(ChronicleCollection.Name)]
public class through_authproxy : Specification
{
    static readonly EventStoreNamespaceName _tenant = "fjord-logistics";
    static readonly IdentityProviderSubject _reporterSubject = "8c2f6a1e-reporter";
    static readonly Reporter _reporter = new("entra", _reporterSubject, "Ada Lovelace", "ada@example.com");
    static readonly ForwardedUser _actor = new("entra", "support-person", "grace@support.example.com", ["support"], [new("name", "Grace Hopper")]);

    AuthProxy _authProxy;
    Desk _desk;
    WebApplicationWithUsher _application;
    IssueId _issueId;
    OnBehalfSession _session;
    OnBehalfSession _sessionOnAnotherIssue;

    HttpStatusCode _grantCheckStatus;
    JsonElement _liveGrant;
    HttpStatusCode _startedStatus;
    HttpStatusCode _startedWithoutGrantStatus;
    HttpStatusCode _replayedStatus;
    HttpStatusCode _wrongTenantStatus;
    JsonElement _addedDetails;
    HttpStatusCode _forgedWithoutAssertion;
    HttpStatusCode _forgedWithAnotherActor;
    JsonElement _revoked;
    JsonElement _grantAfterRevoke;
    HttpStatusCode _firstEndedStatus;
    HttpStatusCode _secondEndedStatus;
    AppendedEvent _signedIn;
    AppendedEvent _detailsAdded;
    IReadOnlyList<AppendedEvent> _sessionEnds;
    int _detailsAddedCount;
    IReadOnlyList<AppendedEvent> _eventsOnAnotherIssue;
    IssueConversation _conversation;

    async Task Because()
    {
        var infrastructure = ChronicleInfrastructure.Current;
        _authProxy = new();
        _issueId = IssueId.New();
        var anotherIssue = IssueId.New();
        _session = new("session-one", _tenant.Value, _issueId.Value.ToString(), _reporterSubject.Value, _actor, DateTimeOffset.UtcNow);
        _sessionOnAnotherIssue = _session with { SessionId = "session-two", Issue = anotherIssue.Value.ToString() };

        _desk = await Desk.Connect(infrastructure);
        await _desk.Serve(_tenant);
        _application = await WebApplicationWithUsher.Start(infrastructure, _desk.Name, _authProxy);
        var baseUrl = WebApplicationWithUsher.BaseUrl;

        // The reporter reported two issues, shared who they are and granted access on the first only.
        var tenant = await _application.In(_tenant);
        var validUntil = DateTimeOffset.UtcNow.AddHours(1);
        foreach (var issue in new[] { _issueId, anotherIssue })
        {
            await tenant.EventLog.Append(issue, new ReporterDescribedIssue(new("Invoicing", "Invoices", "SendInvoice"), [], "The invoice is not sent", "I pressed send.", "Ledger", "1.2.3", _reporterSubject));
            await tenant.EventLog.Append(issue, new ReporterSharedIdentity(_reporter));
        }

        await tenant.EventLog.Append(_issueId, new ReporterGrantedAccess(validUntil));

        // AuthProxy checks the grant, then announces the session.
        var grantCheck = _authProxy.GrantCheck(_session, baseUrl);
        using (var response = await _application.Send(grantCheck))
        {
            _grantCheckStatus = response.StatusCode;
            _liveGrant = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.Clone();
        }

        using (var replayed = await _application.Send(grantCheck))
        {
            _replayedStatus = replayed.StatusCode;
        }

        using (var wrongTenant = await _application.Send(_authProxy.GrantCheck(_session, baseUrl) with { Tenant = "another-tenant" }))
        {
            _wrongTenantStatus = wrongTenant.StatusCode;
        }

        using (var started = await _application.Send(_authProxy.Started(_session, baseUrl, validUntil)))
        {
            _startedStatus = started.StatusCode;
        }

        using (var startedWithoutGrant = await _application.Send(_authProxy.Started(_sessionOnAnotherIssue, baseUrl, validUntil)))
        {
            _startedWithoutGrantStatus = startedWithoutGrant.StatusCode;
        }

        // Support acts as the reporter: AuthProxy forwards the represented user as the principal the grant check
        // answered, and the actor beside it.
        var represented = JsonSerializer.Deserialize<ForwardedUser>(_liveGrant.GetProperty("principal").GetRawText(), JsonSerializerOptions.Web)!;
        represented = represented with { UserRoles = [.. represented.UserRoles, "anonymous", "authenticated"] };
        var actorHeader = AuthProxy.Encode(_actor);
        var addDetails = new { issueId = _issueId.Value, details = "Pressing send twice sends nothing." };
        (_, _addedDetails) = await _application.Execute("/api/conversation/add-details", addDetails, _tenant, represented, _ =>
        {
            _.Request.Headers.Add("x-cratis-on-behalf-actor", actorHeader);
            _.Request.Headers.Add("x-cratis-on-behalf-assertion", _authProxy.Forward(_session, _.PrincipalHeader, actorHeader));
        });

        // A caller that reached the application some other way forges the forward: without AuthProxy's assertion,
        // and with an assertion for other identity headers.
        (_forgedWithoutAssertion, _) = await _application.Execute("/api/conversation/add-details", addDetails, _tenant, represented, _ =>
            _.Request.Headers.Add("x-cratis-on-behalf-actor", actorHeader));
        (_forgedWithAnotherActor, _) = await _application.Execute("/api/conversation/add-details", addDetails, _tenant, represented, _ =>
        {
            _.Request.Headers.Add("x-cratis-on-behalf-actor", AuthProxy.Encode(_actor with { UserId = "somebody-else" }));
            _.Request.Headers.Add("x-cratis-on-behalf-assertion", _authProxy.Forward(_session, _.PrincipalHeader, actorHeader));
        });

        _signedIn = await WaitFor<SupportSignedInAsReporter>(tenant, _issueId);
        _detailsAdded = await WaitFor<ReporterAddedDetails>(tenant, _issueId);
        _conversation = await Eventually.Get(
            async () => (await tenant.ReadModels.GetInstanceById<IssueConversation>(_issueId.Value.ToString())) is { } issue && issue.Entries.Any(_ => _.Kind == ConversationEntryKind.DetailsAdded) ? issue : null,
            "the conversation with the details support added");

        // The reporter revokes the grant themselves.
        (_, _revoked) = await _application.Execute("/api/access/revoke-access", new { issueId = _issueId.Value }, _tenant, Principal(_reporter));
        await WaitFor<SupportSessionEnded>(tenant, _issueId);

        // AuthProxy's next grant check is not live, and two of its replicas report the end.
        using (var afterRevoke = await _application.Send(_authProxy.GrantCheck(_session, baseUrl)))
        {
            _grantAfterRevoke = JsonDocument.Parse(await afterRevoke.Content.ReadAsStringAsync()).RootElement.Clone();
        }

        using (var first = await _application.Send(_authProxy.Ended(_session, baseUrl, "grant-not-live")))
        {
            _firstEndedStatus = first.StatusCode;
        }

        using (var second = await _application.Send(_authProxy.Ended(_session, baseUrl, "grant-not-live")))
        {
            _secondEndedStatus = second.StatusCode;
        }

        var events = await tenant.EventLog.GetFromSequenceNumber(EventSequenceNumber.First, _issueId);
        _sessionEnds = [.. events.Where(_ => _.Content is SupportSessionEnded)];
        _detailsAddedCount = events.Count(_ => _.Content is ReporterAddedDetails);
        _eventsOnAnotherIssue = [.. await tenant.EventLog.GetFromSequenceNumber(EventSequenceNumber.First, anotherIssue)];
    }

    void Destroy()
    {
        _application?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _desk?.Dispose();
        _authProxy?.Dispose();
    }

    [Fact] void should_answer_the_grant_check() => _grantCheckStatus.ShouldEqual(HttpStatusCode.OK);
    [Fact] void should_answer_that_the_grant_is_live() => _liveGrant.GetProperty("live").GetBoolean().ShouldBeTrue();
    [Fact] void should_represent_exactly_the_user_asked_about() => _liveGrant.GetProperty("principal").GetProperty("userId").GetString().ShouldEqual(_reporterSubject.Value);
    [Fact] void should_answer_until_when_the_grant_lasts() => _liveGrant.GetProperty("validUntil").GetDateTimeOffset().ShouldBeGreaterThan(DateTimeOffset.UtcNow);
    [Fact] void should_refuse_a_replayed_token() => _replayedStatus.ShouldEqual(HttpStatusCode.Unauthorized);
    [Fact] void should_refuse_a_tenant_header_other_than_the_tokens() => _wrongTenantStatus.ShouldEqual(HttpStatusCode.BadRequest);
    [Fact] void should_acknowledge_the_start() => ((int)_startedStatus).ShouldBeInRange(200, 299);
    [Fact] void should_refuse_a_start_without_a_live_grant() => _startedWithoutGrantStatus.ShouldEqual(HttpStatusCode.Forbidden);
    [Fact] void should_not_record_a_session_without_a_live_grant() => _eventsOnAnotherIssue.ShouldNotContain(_ => _.Content is SupportSignedInAsReporter);
    [Fact] void should_record_who_signed_in() => ((SupportSignedInAsReporter)_signedIn.Content).ShouldEqual(new SupportSignedInAsReporter("session-one", new(_actor.UserDetails, SupportAuthorKind.Person)));
    [Fact] void should_record_the_start_as_caused_by_the_actor() => _signedIn.Context.CausedBy.Subject.ShouldEqual(_actor.UserId);
    [Fact] void should_record_the_start_on_behalf_of_the_reporter() => _signedIn.Context.CausedBy.OnBehalfOf!.Subject.ShouldEqual(_reporterSubject.Value);
    [Fact] void should_execute_the_command_as_the_reporter() => _addedDetails.GetProperty("isSuccess").GetBoolean().ShouldBeTrue();
    [Fact] void should_refuse_a_forward_without_authproxys_assertion() => _forgedWithoutAssertion.ShouldEqual(HttpStatusCode.Unauthorized);
    [Fact] void should_refuse_a_forward_with_identities_the_assertion_is_not_for() => _forgedWithAnotherActor.ShouldEqual(HttpStatusCode.Unauthorized);
    [Fact] void should_record_only_what_authproxy_vouched_for() => _detailsAddedCount.ShouldEqual(1);
    [Fact] void should_record_what_support_did_as_caused_by_the_actor() => _detailsAdded.Context.CausedBy.Subject.ShouldEqual(_actor.UserId);
    [Fact] void should_record_what_support_did_on_behalf_of_the_reporter() => _detailsAdded.Context.CausedBy.OnBehalfOf!.Subject.ShouldEqual(_reporterSubject.Value);
    [Fact] void should_show_the_reporter_who_did_it() => ((string)Added().ActedBy).ShouldEqual(_actor.UserDetails);
    [Fact] void should_show_the_reporter_the_session_it_was_done_in() => ((string)Added().Session).ShouldEqual("session-one");
    [Fact] void should_let_the_reporter_revoke() => _revoked.GetProperty("isSuccess").GetBoolean().ShouldBeTrue();
    [Fact] void should_answer_not_live_after_the_revocation() => _grantAfterRevoke.GetProperty("live").GetBoolean().ShouldBeFalse();
    [Fact] void should_acknowledge_each_end() => (_firstEndedStatus, _secondEndedStatus).ShouldEqual((HttpStatusCode.NoContent, HttpStatusCode.NoContent));
    [Fact] void should_record_the_end_once() => _sessionEnds.Count.ShouldEqual(1);

    static ForwardedUser Principal(Reporter reporter) => new(
        reporter.IdentityProvider,
        reporter.IdentityProviderSubject,
        reporter.Name,
        ["anonymous", "authenticated"],
        [new("name", reporter.Name), new("email", reporter.Email)]);

    static Task<AppendedEvent> WaitFor<TEvent>(IEventStore store, IssueId issueId) =>
        Eventually.Get(
            async () => (await store.EventLog.GetFromSequenceNumber(EventSequenceNumber.First, issueId)).FirstOrDefault(_ => _.Content is TEvent),
            $"{typeof(TEvent).Name} on {issueId}");

    ConversationEntry Added() => _conversation.Entries.Single(_ => _.Kind == ConversationEntryKind.DetailsAdded);
}
