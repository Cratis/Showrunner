// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Arc.Availability.Audiences;
using Cratis.Arc.Availability.Suspensions;
using Cratis.Showrunner.GuestList.Querying;
using ShowrunnerTenant = Cratis.Showrunner.GuestList.Contracts.Audiences.TenantId;
using ShowrunnerUser = Cratis.Showrunner.GuestList.Contracts.Audiences.UserId;

namespace Cratis.Showrunner.GuestList.Availability;

/// <summary>
/// Represents an <see cref="ISuspensionSource"/> over what the product recorded in its own event store, which is what the
/// availability engine reads to know the parts suspended for a request.
/// </summary>
/// <remarks>
/// <para>
/// The decisions that apply are the system's for a system caller, and the tenant's and the user's the subject names. Each
/// is tagged with the level it is held at and dated by when the operator decided, resumes included, so the engine keeps
/// the newest decision per level and path: a resume newer than a suspension at the same level ends it, and a suspension at
/// any other level still holds.
/// </para>
/// <para>
/// Nothing is asked of the operator. The answer is read from the local read models, so it is the last state the product
/// received and stays so while the link to the operator is down.
/// </para>
/// </remarks>
/// <param name="suspensions">What the product knows about the parts suspended for its tenants, users and the system.</param>
/// <param name="changes">The feed of changes to what the product holds.</param>
public class GuestListSuspensionSource(IPartSuspensions suspensions, AudienceChangeFeed changes) : ISuspensionSource
{
    /// <inheritdoc/>
    public async ValueTask<IReadOnlyCollection<PartSuspensionDecision>> GetDecisions(AudienceSubject subject, CancellationToken cancellationToken = default)
    {
        List<PartSuspensionDecision> decisions = [];
        if (subject.IsSystem)
        {
            decisions.AddRange(Tag(await suspensions.ForSystem(cancellationToken), AudienceLevel.System));
        }

        if (string.IsNullOrWhiteSpace(subject.Tenant))
        {
            return decisions;
        }

        var tenant = new ShowrunnerTenant(subject.Tenant);
        decisions.AddRange(Tag(await suspensions.ForTenant(tenant, cancellationToken), AudienceLevel.Tenant));

        if (!string.IsNullOrWhiteSpace(subject.User))
        {
            decisions.AddRange(Tag(await suspensions.ForUser(tenant, new ShowrunnerUser(subject.User), cancellationToken), AudienceLevel.User));
        }

        return decisions;
    }

    /// <inheritdoc/>
    public IDisposable OnChanged(Func<SuspensionChange, ValueTask> handler) => changes.SubscribeToSuspensions(handler);

    static IEnumerable<PartSuspensionDecision> Tag(IEnumerable<Holding.PartDecision> decided, AudienceLevel level) =>
        decided.Select(decision => new PartSuspensionDecision(decision.Part.Value, level, decision.IsSuspended, decision.Decided));
}
