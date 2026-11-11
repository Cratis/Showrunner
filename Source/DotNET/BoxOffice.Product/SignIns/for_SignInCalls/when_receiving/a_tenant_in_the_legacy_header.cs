// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using System.Collections.Immutable;
using Cratis.Chronicle.EventSequences;
using Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.given;

namespace Cratis.Showrunner.BoxOffice.Product.SignIns.for_SignInCalls.when_receiving;

public class a_tenant_in_the_legacy_header : a_sign_in_callback
{
    int _legacyOnly;
    int _legacyMatchesBody;
    int _legacyDiffersFromBody;
    int _outboxBeforeAcceptedCalls;
    int _headerWins;
    IImmutableList<AppendedEvent> _afterLegacyOnly;

    async Task Because()
    {
        var differs = Call(Body(), tenantHeader: null);
        differs.Request.Headers[SignInHeaders.LegacyTenant] = "somebody-elses";
        _legacyDiffersFromBody = await Answer(differs);
        await ReadOutbox();
        _outboxBeforeAcceptedCalls = _outbox.Count;

        var only = Call(Body(tenant: null));
        only.Request.Headers[SignInHeaders.LegacyTenant] = "fjord-logistics";
        _legacyOnly = await Answer(only);
        await ReadOutbox();
        _afterLegacyOnly = _outbox;

        var matches = Call(Body());
        matches.Request.Headers[SignInHeaders.LegacyTenant] = "fjord-logistics";
        _legacyMatchesBody = await Answer(matches);

        var wins = Call(Body(tenant: null), tenantHeader: "fjord-logistics");
        wins.Request.Headers[SignInHeaders.LegacyTenant] = "somebody-elses";
        _headerWins = await Answer(wins);
        await ReadOutbox();
    }

    [Fact] void should_refuse_one_that_differs_from_the_body() => _legacyDiffersFromBody.ShouldEqual(StatusCodes.Status400BadRequest);
    [Fact] void should_tell_the_operator_nothing_for_the_refusal() => _outboxBeforeAcceptedCalls.ShouldEqual(0);
    [Fact] void should_accept_it_on_its_own() => _legacyOnly.ShouldEqual(StatusCodes.Status204NoContent);
    [Fact] void should_tell_the_operator_in_that_tenant() => ((SeatHolderActive)_afterLegacyOnly.Single().Content).TenantId.Value.ShouldEqual("fjord-logistics");
    [Fact] void should_accept_one_that_matches_the_body() => _legacyMatchesBody.ShouldEqual(StatusCodes.Status204NoContent);
    [Fact] void should_let_the_current_header_win_over_the_legacy_one() => _headerWins.ShouldEqual(StatusCodes.Status204NoContent);
}
#endif
