// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Availability.Audiences;
using Cratis.Arc.Availability.Suspensions;
using Cratis.Chronicle.ReadModels;
using Cratis.Showrunner.GuestList.Authoring.Decisions;
using Cratis.Showrunner.GuestList.Integration.given;
using Cratis.Showrunner.Usher.Integration.given;
using ArcAudienceLevel = Cratis.Arc.Availability.Audiences.AudienceLevel;

namespace Cratis.Showrunner.GuestList.Integration.for_GuestList.when_an_operator_and_a_product_talk;

/// <summary>
/// An operator suspends a part for a tenant in its own event store. The decision reaches the running product, an open session
/// is told, the newest decision wins when the operator resumes the part, and the operator's history shows both.
/// </summary>
[Collection(ChronicleCollection.Name)]
public class about_suspension : Specification
{
    static readonly EventStoreNamespaceName _tenant = "fjord-logistics";
    static readonly TenantId _tenantId = "fjord-logistics";
    static readonly Contracts.Suspensions.PartPath _part = "Fleet/Reports";

    readonly List<SuspensionChange> _heard = [];
    OperatorSide _operator;
    ProductSide _product;
    IReadOnlyCollection<PartSuspensionDecision> _afterTheSuspension;
    IReadOnlyCollection<PartSuspensionDecision> _afterTheResume;
    ProductSuspensions _history;

    async Task Because()
    {
        var infrastructure = ChronicleInfrastructure.Current;
        _operator = await OperatorSide.Start(infrastructure);
        _product = await ProductSide.Start(infrastructure, _operator.Name, ["Plus"]);
        await _product.In(_tenant);

        var subject = new AudienceSubject(_tenantId.Value, null);
        using var subscription = _product.Suspensions.OnChanged(change =>
        {
            lock (_heard)
            {
                _heard.Add(change);
            }

            return ValueTask.CompletedTask;
        });

        await _operator.Decide(ProductSide.Product, new PartSuspensionDecided(Contracts.Suspensions.SuspensionLevel.Tenant, _tenantId, UserId.NotSet, _part));
        _afterTheSuspension = await Eventually.Get(
            async () =>
            {
                var decisions = await _product.Suspensions.GetDecisions(subject);
                return decisions.Any(decision => decision.IsSuspended) ? decisions : null;
            },
            "the product to receive the suspension");

        await _operator.Decide(ProductSide.Product, new PartResumeDecided(Contracts.Suspensions.SuspensionLevel.Tenant, _tenantId, UserId.NotSet, _part));
        _afterTheResume = await Eventually.Get(
            async () =>
            {
                var decisions = await _product.Suspensions.GetDecisions(subject);
                return decisions.Any(decision => !decision.IsSuspended) ? decisions : null;
            },
            "the product to receive the resume");

        var store = await _operator.In(EventStoreNamespaceName.Default);
        _history = await store.ReadModels.GetInstanceById<ProductSuspensions>(new ReadModelKey(ProductSide.Product.Value));
    }

    async Task Destroy()
    {
        if (_product is not null)
        {
            await _product.DisposeAsync();
        }

        if (_operator is not null)
        {
            await _operator.DisposeAsync();
        }
    }

    [Fact] void should_reach_the_product_as_a_suspension_of_the_tenant() => _afterTheSuspension.ShouldContain(new PartSuspensionDecision(_part.Value, ArcAudienceLevel.Tenant, true, _afterTheSuspension.Single().DecidedAt));
    [Fact] void should_tell_an_open_session_that_the_tenant_changed() => _heard.ShouldContain(new SuspensionChange(ArcAudienceLevel.Tenant, _tenantId.Value));
    [Fact] void should_carry_the_resume_so_the_engine_keeps_the_newest() => _afterTheResume.OrderBy(decision => decision.DecidedAt).Last().IsSuspended.ShouldBeFalse();
    [Fact] void should_keep_nothing_suspended_for_the_operator() => _history.Suspended.ShouldBeEmpty();
    [Fact] void should_show_the_suspension_and_the_resume_in_the_history() => _history.History.Select(entry => entry.IsSuspended).ShouldEqual([true, false]);
}
