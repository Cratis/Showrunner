// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if DEBUG
using Cratis.Showrunner.GuestList.Contracts.Audiences;
using Cratis.Showrunner.GuestList.Querying;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cratis.Showrunner.GuestList.Availability.given;

/// <summary>
/// A <see cref="GuestListAudienceSource"/> over a guest list a specification fills, with a clock it moves.
/// </summary>
public class a_source_over_a_guest_list : Specification
{
    protected static readonly TenantId Acme = new("acme");
    protected static readonly UserId Alice = new("alice");

    protected IGuestList _guestList;
    protected a_clock _clock;
    protected AudienceChangeFeed _feed;
    protected GuestListAudienceSource _source;

    void Establish()
    {
        _guestList = Substitute.For<IGuestList>();
        Holds(system: [], tenant: [], user: []);
        _clock = new();
        _feed = new(_clock, NullLogger<AudienceChangeFeed>.Instance);
        _source = new(_guestList, _feed);
    }

    /// <summary>
    /// Makes the guest list answer with what each level holds for the tenant <c>acme</c> and its user <c>alice</c>.
    /// </summary>
    /// <param name="system">What the system holds.</param>
    /// <param name="tenant">What the tenant holds.</param>
    /// <param name="user">What the user holds.</param>
    protected void Holds(string[] system, string[] tenant, string[] user)
    {
        _guestList.ForSystem(Arg.Any<CancellationToken>()).Returns(Set(system));
        _guestList.ForTenant(Acme, Arg.Any<CancellationToken>()).Returns(Set(tenant));
        _guestList.ForUser(Acme, Alice, Arg.Any<CancellationToken>()).Returns(Set(user));
    }

    static Task<IReadOnlySet<AudienceName>> Set(string[] audiences) =>
        Task.FromResult<IReadOnlySet<AudienceName>>(audiences.Select(_ => new AudienceName(_)).ToHashSet());
}
#endif
