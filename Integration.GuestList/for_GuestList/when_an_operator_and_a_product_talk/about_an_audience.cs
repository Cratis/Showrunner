// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.GuestList.Authoring.Decisions;
using Cratis.Showrunner.GuestList.Holding;
using Cratis.Showrunner.GuestList.Integration.given;
using Cratis.Showrunner.GuestList.Querying;
using Cratis.Showrunner.Usher.Integration.given;

namespace Cratis.Showrunner.GuestList.Integration.for_GuestList.when_an_operator_and_a_product_talk;

/// <summary>
/// An operator grants audiences at the three levels, withdraws one, and decides about a tenant and a product the product
/// side does not have. The product records the decisions in the tenant's own namespace and answers from what it recorded,
/// also after the operator is gone: the last known state stays.
/// </summary>
[Collection(ChronicleCollection.Name)]
public class about_an_audience : Specification
{
    static readonly EventStoreNamespaceName _tenant = "fjord-logistics";
    static readonly TenantId _tenantId = "fjord-logistics";
    static readonly UserId _user = "ada";

    OperatorSide _operator;
    ProductSide _product;
    IReadOnlySet<AudienceName> _forTheTenant;
    IReadOnlySet<AudienceName> _forTheUser;
    IReadOnlySet<AudienceName> _forAnotherUser;
    IReadOnlySet<AudienceName> _forTheSystem;
    IReadOnlySet<AudienceName> _forCrm;
    IReadOnlySet<AudienceName> _forReports;
    IReadOnlySet<AudienceName> _afterTheOperatorWasGone;
    IReadOnlyList<GrantedAudience> _granted;
    IEnumerable<UndeclaredGrant> _undeclared;
    IReadOnlyList<AppendedEvent> _recordedForTheTenant;
    IReadOnlyList<AppendedEvent> _recordedForTheProduct;
    IReadOnlyList<AppendedEvent> _recordedForAnotherTenant;

    async Task Because()
    {
        var infrastructure = ChronicleInfrastructure.Current;

        // The operator first, so its store name is known to the product; the product has the tenant's namespace, which is
        // where it records what is decided about the tenant.
        _operator = await OperatorSide.Start(infrastructure);
        _product = await ProductSide.Start(infrastructure, _operator.Name);
        await _product.In(_tenant);

        await _operator.Decide(
            ProductSide.Product,
            new AudienceGrantDecided(AudienceLevel.Tenant, _tenantId, UserId.NotSet, "Plus", SystemClientId.NotSet, SystemKind.None),
            new AudienceGrantDecided(AudienceLevel.Tenant, _tenantId, UserId.NotSet, "Beta", SystemClientId.NotSet, SystemKind.None),
            new AudienceGrantDecided(AudienceLevel.User, _tenantId, _user, "Preview", SystemClientId.NotSet, SystemKind.None),
            new AudienceGrantDecided(AudienceLevel.User, _tenantId, _user, "Plus", SystemClientId.NotSet, SystemKind.None),
            new AudienceGrantDecided(AudienceLevel.System, TenantId.NotSet, UserId.NotSet, "Internal", SystemClientId.NotSet, SystemKind.None),
            new AudienceGrantDecided(AudienceLevel.System, TenantId.NotSet, UserId.NotSet, "Retired", SystemClientId.NotSet, SystemKind.None),
            new AudienceGrantDecided(AudienceLevel.NamedSystem, TenantId.NotSet, UserId.NotSet, "Plus", "crm-sync", SystemKind.Mcp),
            new AudienceGrantDecided(AudienceLevel.NamedSystem, TenantId.NotSet, UserId.NotSet, "Preview", "reports-mcp", SystemKind.Api),

            // A tenant the product does not have is never recorded there, and a decision for another product is not
            // delivered to this one.
            new AudienceGrantDecided(AudienceLevel.Tenant, "initech", UserId.NotSet, "Plus", SystemClientId.NotSet, SystemKind.None),
            new AudienceWithdrawalDecided(AudienceLevel.Tenant, _tenantId, UserId.NotSet, "Beta", SystemClientId.NotSet, SystemKind.None));
        await _operator.Decide("studio", new AudienceGrantDecided(AudienceLevel.System, TenantId.NotSet, UserId.NotSet, "Other", SystemClientId.NotSet, SystemKind.None));

        // Waits for the last decision, which is the withdrawal: everything before it has then been recorded.
        _forTheTenant = await Eventually.Get(
            async () =>
            {
                var audiences = await _product.GuestList.ForTenant(_tenantId);
                return audiences.Contains("Plus") && !audiences.Contains("Beta") && (await _product.GuestList.ForSystem()).Contains("Retired") ? audiences : null;
            },
            "the tenant's audiences to reflect the withdrawal of Beta");

        _forTheUser = await _product.GuestList.For(_tenantId, _user);
        _forAnotherUser = await _product.GuestList.For(_tenantId, "grace");
        _forTheSystem = await _product.GuestList.ForSystem();
        _forCrm = await _product.GuestList.ForSystemCaller("crm-sync");
        _forReports = await _product.GuestList.ForSystemCaller("reports-mcp");
        _granted = await _product.GuestList.Granted(_tenantId);
        _undeclared = UndeclaredGrant.Among(_granted, ["Plus", "Preview", "Internal"]);
        _recordedForTheTenant = await (await _product.In(_tenant)).EventLog.GetFromSequenceNumber(EventSequenceNumber.First);
        _recordedForTheProduct = await (await _product.In(EventStoreNamespaceName.Default)).EventLog.GetFromSequenceNumber(EventSequenceNumber.First);
        _recordedForAnotherTenant = await (await _product.In("initech")).EventLog.GetFromSequenceNumber(EventSequenceNumber.First);

        // The operator goes away: the product still answers what it last knew.
        await _operator.DisposeAsync();
        _afterTheOperatorWasGone = await _product.GuestList.For(_tenantId, _user);
    }

    static IEnumerable<string> Names(IEnumerable<AudienceName> audiences) => audiences.Select(_ => _.Value);

    async Task Destroy()
    {
        if (_product is not null)
        {
            await _product.DisposeAsync();
        }
    }

    [Fact] void should_grant_the_tenant_what_was_granted_at_its_level() => Names(_forTheTenant).ShouldContainOnly("Plus");
    [Fact] void should_add_the_tenants_users_and_the_systems_audiences_for_a_user() => Names(_forTheUser).ShouldContainOnly("Plus", "Preview", "Internal", "Retired");
    [Fact] void should_leave_a_users_audiences_with_that_user() => Names(_forAnotherUser).ShouldContainOnly("Plus", "Internal", "Retired");
    [Fact] void should_grant_the_system_what_was_granted_at_its_level() => Names(_forTheSystem).ShouldContainOnly("Internal", "Retired");
    [Fact] void should_give_a_named_system_what_every_system_holds_and_its_own() => Names(_forCrm).ShouldContainOnly("Internal", "Retired", "Plus");
    [Fact] void should_not_give_a_named_system_what_another_system_holds() => Names(_forReports).ShouldContainOnly("Internal", "Retired", "Preview");
    [Fact] void should_list_every_grant_that_applies_to_the_tenant() => _granted.Count.ShouldEqual(5);
    [Fact] void should_report_the_grant_the_application_does_not_declare() => _undeclared.Select(_ => _.Audience.Value).ShouldContainOnly("Retired");
    [Fact] void should_record_the_decisions_about_the_tenant_in_its_own_namespace() => _recordedForTheTenant.Count(_ => _.Content is AudienceGrantRecorded or AudienceWithdrawalRecorded).ShouldEqual(5);
    [Fact] void should_record_the_decisions_about_the_system_in_the_products_own_namespace() => _recordedForTheProduct.Count(_ => _.Content is AudienceGrantRecorded { Level: AudienceLevel.System }).ShouldEqual(2);
    [Fact] void should_not_record_a_decision_for_a_tenant_the_product_does_not_have() => _recordedForAnotherTenant.ShouldBeEmpty();
    [Fact] void should_not_receive_a_decision_for_another_product() => _recordedForTheProduct.Any(_ => _.Content is AudienceGrantRecorded recorded && string.Equals(recorded.Audience.Value, "Other", StringComparison.Ordinal)).ShouldBeFalse();
    [Fact] void should_keep_answering_what_it_last_knew_when_the_operator_is_gone() => Names(_afterTheOperatorWasGone).ShouldContainOnly("Plus", "Preview", "Internal", "Retired");
}
