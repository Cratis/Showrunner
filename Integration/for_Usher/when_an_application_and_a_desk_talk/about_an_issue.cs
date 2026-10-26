// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using Cratis.Showrunner.Usher.Access.GrantAccess;
using Cratis.Showrunner.Usher.Access.Lapsing;
using Cratis.Showrunner.Usher.Contracts.Access;
using Cratis.Showrunner.Usher.Contracts.Identity;
using Cratis.Showrunner.Usher.Contracts.Lifecycle;
using Cratis.Showrunner.Usher.Contracts.Reporting;
using Cratis.Showrunner.Usher.Contracts.WorkItems;
using Cratis.Showrunner.Usher.Identity.ShareIdentity;
using Cratis.Showrunner.Usher.Integration.given;
using Cratis.Showrunner.Usher.Receiving;
using Cratis.Showrunner.Usher.Reporting.ReportIssue;

namespace Cratis.Showrunner.Usher.Integration.for_Usher.when_an_application_and_a_desk_talk;

/// <summary>
/// A reporter in a tenant reports an issue, shares who they are and grants access for a short while; the desk reads it,
/// accepts the issue and ships a fix, and also answers about an issue that does not exist in the tenant.
/// </summary>
[Collection(ChronicleCollection.Name)]
public class about_an_issue : Specification
{
    static readonly EventStoreNamespaceName _tenant = "fjord-logistics";
    static readonly IdentityProviderSubject _reporterSubject = "8c2f6a1e-reporter";
    static readonly Reporter _reporter = new("entra", _reporterSubject, "Ada Lovelace", "ada@example.com");

    Desk _desk;
    Application _application;
    IssueId _issueId;
    IssueId _unknownIssueId;
    ValidUntil _validUntil;

    AppendedEvent _issueReported;
    AppendedEvent _attributed;
    AppendedEvent _accessGranted;
    AppendedEvent _accessExpired;
    AppendedEvent _lapsed;
    AppendedEvent _acceptedByDesk;
    AppendedEvent _accepted;
    AppendedEvent _shippedFix;
    IReadOnlyList<AppendedEvent> _unknownIssueEvents;
    IReadOnlyList<AppendedEvent> _receivedByDesk;
    IReadOnlyList<AppendedEvent> _defaultNamespaceEvents;

    async Task Because()
    {
        var infrastructure = ChronicleInfrastructure.Current;
        _issueId = IssueId.New();
        _unknownIssueId = IssueId.New();

        _desk = await Desk.Connect(infrastructure);
        await _desk.Serve(_tenant);
        _application = await Application.Start(infrastructure, _desk.Name);
        await _desk.SubscribeTo(_application.Name);

        // The reporter's facts, recorded in the tenant's namespace as Usher's commands record them.
        var tenant = await _application.In(_tenant);
        _validUntil = DateTimeOffset.UtcNow.AddSeconds(5);
        await tenant.EventLog.Append(_issueId, new ReporterDescribedIssue(
            new("Invoicing", "Invoices", "SendInvoice"),
            [],
            "The invoice is not sent",
            "I pressed send and nothing happened.",
            "Ledger",
            "1.2.3",
            _reporterSubject));
        await tenant.EventLog.Append(_issueId, new ReporterSharedIdentity(_reporter));
        await tenant.EventLog.Append(_issueId, new ReporterGrantedAccess(_validUntil));

        _issueReported = await _desk.WaitFor<IssueReported>(_application.Name, _tenant, _issueId);
        _attributed = await _desk.WaitFor<IssueAttributedToReporter>(_application.Name, _tenant, _issueId);
        _accessGranted = await _desk.WaitFor<AccessGranted>(_application.Name, _tenant, _issueId);

        // The desk answers from its own default namespace, naming the tenant in the payload.
        await _desk.Publish(_unknownIssueId, new IssueTriaged(_tenant.Value));
        _acceptedByDesk = await _desk.Publish(_issueId, new IssueAccepted(_tenant.Value));
        await _desk.Publish(_issueId, new IssueFixedInRelease(_tenant.Value, "4.12.3", "Cratis/Ledger#42"));

        _accepted = await _application.WaitFor<SupportAcceptedIssue>(_tenant, _issueId);
        _shippedFix = await _application.WaitFor<ReleaseShippedFix>(_tenant, _issueId);
        _lapsed = await _application.WaitFor<AccessGrantLapsed>(_tenant, _issueId);
        _accessExpired = await _desk.WaitFor<AccessExpired>(_application.Name, _tenant, _issueId);

        _unknownIssueEvents = await tenant.EventLog.GetFromSequenceNumber(EventSequenceNumber.First, _unknownIssueId);
        _receivedByDesk = await _desk.ReceivedFrom(_application.Name, _tenant, _issueId);
        _defaultNamespaceEvents = await (await _application.In(EventStoreNamespaceName.Default)).EventLog.GetFromSequenceNumber(EventSequenceNumber.First, _issueId);
    }

    void Destroy()
    {
        _application?.DisposeAsync().AsTask().GetAwaiter().GetResult();
        _desk?.Dispose();
    }

    [Fact] void should_publish_the_report_to_the_desk_in_the_tenants_namespace() => _issueReported.Context.Namespace.ShouldEqual(_tenant);
    [Fact] void should_name_the_tenant_in_the_report() => ((IssueReported)_issueReported.Content).TenantId.ShouldEqual(new TenantId(_tenant.Value));
    [Fact] void should_carry_the_summary_in_the_report() => ((IssueReported)_issueReported.Content).Summary.Value.ShouldEqual("The invoice is not sent");
    [Fact] void should_not_publish_the_reporter_subject_with_the_report() => JsonSerializer.Serialize(_issueReported.Content).ShouldNotContain(_reporterSubject.Value);
    [Fact] void should_publish_who_the_reporter_is_once_shared() => ((IssueAttributedToReporter)_attributed.Content).Reporter.ShouldEqual(_reporter);
    [Fact] void should_publish_the_grant() => ((AccessGranted)_accessGranted.Content).ValidUntil.Value.ToUnixTimeMilliseconds().ShouldEqual(_validUntil.Value.ToUnixTimeMilliseconds());
    [Fact] void should_publish_each_fact_once() => _receivedByDesk.GroupBy(_ => _.Content.GetType()).ShouldEachConformTo(_ => _.Count() == 1);
    [Fact] void should_record_the_desk_accepting_the_issue_in_the_tenant() => _accepted.Context.Namespace.ShouldEqual(_tenant);
    [Fact] void should_keep_when_the_desk_accepted_the_issue() => _accepted.Context.Occurred.ShouldEqual(_acceptedByDesk.Context.Occurred);
    [Fact] void should_record_the_release_that_shipped_the_fix() => _shippedFix.Content.ShouldEqual(new ReleaseShippedFix("4.12.3", "Cratis/Ledger#42"));
    [Fact] void should_ignore_the_desk_answering_about_an_unknown_issue() => _unknownIssueEvents.ShouldBeEmpty();
    [Fact] void should_not_record_anything_in_the_default_namespace() => _defaultNamespaceEvents.ShouldBeEmpty();
    [Fact] void should_lapse_the_grant_when_its_time_passes() => ((AccessGrantLapsed)_lapsed.Content).ValidUntil.Value.ToUnixTimeMilliseconds().ShouldEqual(_validUntil.Value.ToUnixTimeMilliseconds());
    [Fact] void should_lapse_it_after_its_end() => _lapsed.Context.Occurred.ShouldBeGreaterThanOrEqual(_validUntil.Value.AddMilliseconds(-1));
    [Fact] void should_publish_the_lapse_as_access_expired() => ((AccessExpired)_accessExpired.Content).TenantId.ShouldEqual(new TenantId(_tenant.Value));
}
