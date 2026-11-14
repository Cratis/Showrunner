// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.GuestList.Holding;

/// <summary>
/// Where the product records the audiences it is given. Each level has its own stream, so a decision at one level can never
/// touch another level: the tenant's stream and a stream per user are in the tenant's own namespace, and the system's is in
/// the namespace the operator published it in, the product's own.
/// </summary>
public static class GuestListStreams
{
    /// <summary>
    /// The start of the event source of the audiences one named system holds, in the product's own namespace. It is
    /// followed by the client id of the system.
    /// </summary>
    public const string NamedSystemPrefix = "system:";

    /// <summary>
    /// The event source of the audiences the tenant holds, in the tenant's namespace.
    /// </summary>
    public static readonly EventSourceId Tenant = new("tenant");

    /// <summary>
    /// The event source of the audiences the system holds, in the product's own namespace.
    /// </summary>
    public static readonly EventSourceId System = new("system");

    /// <summary>
    /// Gets the event source of the audiences one named system holds, in the product's own namespace. It is its own stream, so
    /// a decision about one system never touches another system or the audiences every system holds.
    /// </summary>
    /// <param name="client">The client id of the system.</param>
    /// <returns>The event source.</returns>
    public static EventSourceId NamedSystem(SystemClientId client) => new($"{NamedSystemPrefix}{client.Value}");

    /// <summary>
    /// Gets the event source of the audiences a user holds, in the namespace of the user's tenant.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>The event source.</returns>
    public static EventSourceId User(UserId user) => new($"user:{user.Value}");

    /// <summary>
    /// Gets the event source an audience decision is recorded under.
    /// </summary>
    /// <param name="level">The level of the decision.</param>
    /// <param name="user">The user, for <see cref="AudienceLevel.User"/>.</param>
    /// <param name="client">The client id of the system, for <see cref="AudienceLevel.NamedSystem"/>.</param>
    /// <returns>The event source.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The level is not one of <see cref="AudienceLevel"/>.</exception>
    public static EventSourceId For(AudienceLevel level, UserId user, SystemClientId? client = null) => level switch
    {
        AudienceLevel.Tenant => Tenant,
        AudienceLevel.User => User(user),
        AudienceLevel.System => System,
        AudienceLevel.NamedSystem => NamedSystem(client ?? SystemClientId.NotSet),
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Not an audience level.")
    };
}
