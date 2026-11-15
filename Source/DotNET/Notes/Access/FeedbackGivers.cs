// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Chronicle.ReadModels;
using Cratis.Showrunner.Notes.Contracts.Access;
using Microsoft.Extensions.DependencyInjection;

namespace Cratis.Showrunner.Notes.Access;

/// <summary>
/// Represents an implementation of <see cref="IFeedbackGivers"/> that reads <see cref="FeedbackGiver"/> from the current
/// tenant.
/// </summary>
/// <remarks>
/// The command-aware <see cref="IDecisionReads"/> is resolved when a decision needs it, not when this is constructed: it is
/// not available outside a command, and an application that lets everyone give feedback never asks.
/// </remarks>
/// <param name="readModels">The <see cref="IReadModels"/> of the current tenant.</param>
/// <param name="services">The <see cref="IServiceProvider"/> to resolve the decision reads from.</param>
public class FeedbackGivers(IReadModels readModels, IServiceProvider services) : IFeedbackGivers
{
    /// <inheritdoc/>
    public async Task<FeedbackGiver?> Current(FeedbackGiverId id) =>
        await readModels.GetInstanceById<FeedbackGiver>(Key(id));

    /// <inheritdoc/>
    public async Task<FeedbackGiver?> ForDecision(FeedbackGiverId id, CancellationToken cancellationToken = default)
    {
        var read = await services.GetRequiredService<IDecisionReads>().Get<FeedbackGiver>(Key(id), cancellationToken);
        return read.Instance;
    }

    static ReadModelKey Key(FeedbackGiverId id) => id.Value.ToString("D");
}
