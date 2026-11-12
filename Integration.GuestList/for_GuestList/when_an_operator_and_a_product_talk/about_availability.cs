// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Choreography.Availability.Audiences;
using Cratis.Showrunner.GuestList.Authoring.Decisions;
using Cratis.Showrunner.GuestList.Integration.given;
using Cratis.Showrunner.Usher.Integration.given;
using ChoreographyAudienceLevel = Cratis.Choreography.Availability.Audiences.AudienceLevel;

namespace Cratis.Showrunner.GuestList.Integration.for_GuestList.when_an_operator_and_a_product_talk;

/// <summary>
/// A product that hands its audiences to the Choreography availability engine. An operator grants audiences in its own event
/// store; the product records them locally and the engine resolves them, tells an open session when one changes, carries the
/// grant for an audience the application does not declare, and keeps answering what it last knew when the operator is gone.
/// </summary>
[Collection(ChronicleCollection.Name)]
public class about_availability : Specification
{
    static readonly EventStoreNamespaceName _tenant = "fjord-logistics";
    static readonly TenantId _tenantId = "fjord-logistics";
    static readonly UserId _user = "ada";

    readonly List<AudienceChange> _heard = [];
    OperatorSide _operator;
    ProductSide _product;
    EffectiveAudiences _initially;
    EffectiveAudiences _afterTheChange;
    EffectiveAudiences _afterTheOperatorWasGone;

    async Task Because()
    {
        var infrastructure = ChronicleInfrastructure.Current;
        _operator = await OperatorSide.Start(infrastructure);
        _product = await ProductSide.Start(infrastructure, _operator.Name, ["Plus", "Preview", "Internal", "Gold"]);
        await _product.In(_tenant);

        await _operator.Decide(
            ProductSide.Product,
            new AudienceGrantDecided(Contracts.Audiences.AudienceLevel.Tenant, _tenantId, UserId.NotSet, "Plus"),
            new AudienceGrantDecided(Contracts.Audiences.AudienceLevel.User, _tenantId, _user, "Preview"),
            new AudienceGrantDecided(Contracts.Audiences.AudienceLevel.System, TenantId.NotSet, UserId.NotSet, "Internal"),
            new AudienceGrantDecided(Contracts.Audiences.AudienceLevel.System, TenantId.NotSet, UserId.NotSet, "Retired"));

        var subject = new AudienceSubject(_tenantId.Value, _user.Value);
        _initially = await Eventually.Get(
            async () =>
            {
                var effective = await _product.Resolver.Resolve(subject);
                return effective.Holds("Plus") && effective.Holds("Preview") && effective.Holds("Internal") && effective.Undeclared.Count > 0 ? effective : null;
            },
            "the engine to resolve the audiences the operator granted");

        // An open session subscribes, then the operator grants another audience: the session is told without a new request.
        using var subscription = _product.Source.OnChanged(change =>
        {
            lock (_heard)
            {
                _heard.Add(change);
            }

            return ValueTask.CompletedTask;
        });
        await _operator.Decide(ProductSide.Product, new AudienceGrantDecided(Contracts.Audiences.AudienceLevel.Tenant, _tenantId, UserId.NotSet, "Gold"));
        _ = await Eventually.Get(
            () =>
            {
                lock (_heard)
                {
                    return Task.FromResult(_heard.Count > 0 ? _heard.ToArray() : null);
                }
            },
            "the open session to be told that the tenant's audiences changed");
        _afterTheChange = await Eventually.Get(
            async () =>
            {
                var effective = await _product.Resolver.Resolve(subject);
                return effective.Holds("Gold") ? effective : null;
            },
            "the engine to resolve the audience granted later");

        // The operator goes away: the engine still answers the last known state, from local state.
        await _operator.DisposeAsync();
        _afterTheOperatorWasGone = await _product.Resolver.Resolve(subject);
    }

    async Task Destroy()
    {
        if (_product is not null)
        {
            await _product.DisposeAsync();
        }
    }

    [Fact] void should_union_the_tenants_the_users_and_the_systems_audiences() => _initially.Matching.Select(_ => _.Value).ShouldContainOnly("Plus", "Preview", "Internal");
    [Fact] void should_carry_the_grant_the_application_does_not_declare() => _initially.Undeclared.Select(_ => _.Audience.Value).ShouldContainOnly("Retired");
    [Fact] void should_never_match_the_undeclared_grant() => _initially.Holds("Retired").ShouldBeFalse();
    [Fact] void should_tell_an_open_session_that_the_tenant_changed() => _heard.ShouldContain(new AudienceChange(ChoreographyAudienceLevel.Tenant, _tenantId.Value));
    [Fact] void should_resolve_the_audience_granted_after_the_session_opened() => _afterTheChange.Holds("Gold").ShouldBeTrue();
    [Fact] void should_keep_answering_what_it_last_knew_when_the_operator_is_gone() => _afterTheOperatorWasGone.Matching.Select(_ => _.Value).ShouldContainOnly("Plus", "Preview", "Internal", "Gold");
}
