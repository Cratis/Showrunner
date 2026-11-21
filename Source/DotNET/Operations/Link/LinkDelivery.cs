// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Cratis.Showrunner.Operations.Link.Protocol;

namespace Cratis.Showrunner.Operations.Link;

/// <summary>
/// Drains the outbox to Direct's inbox in sequence, one batch at a time, and moves the watermark to what Direct durably took.
/// A redelivery is harmless - Direct deduplicates by sequence - so a crash anywhere only means a batch is sent again.
/// </summary>
/// <param name="client">The Direct client.</param>
/// <param name="store">The state store.</param>
internal sealed class LinkDelivery(IDirectClient client, ILinkStore store)
{
    /// <summary>
    /// How many batches one flush sends at most.
    /// </summary>
    internal const int MaximumBatchesPerFlush = 20;

    /// <summary>
    /// Delivers what the outbox holds, then saves it.
    /// </summary>
    /// <param name="context">The pass.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the outbox is saved.</returns>
    internal async Task Flush(LinkContext context, CancellationToken cancellationToken)
    {
        if (context.Connected && context.Enrolled)
        {
            await Drain(context, cancellationToken);
        }

        await store.SaveOutbox(context.Outbox.ToSnapshot(), cancellationToken);
    }

    static async Task<bool> Baseline(LinkContext context, IDirectClient client, CancellationToken cancellationToken)
    {
        // An outbox that never talked to Direct asks for Direct's watermark first, with an empty batch: when Direct is
        // ahead, earlier state was lost and what is held now is renumbered above it.
        var answer = await client.Events(context.Credentials, new WatchdogEventBatch([]), cancellationToken);
        if (!context.Succeeded(answer, "reading the outbox watermark") || answer.Value is null)
        {
            return false;
        }

        context.Outbox.Rebase(answer.Value.Watermark);
        return true;
    }

    async Task Drain(LinkContext context, CancellationToken cancellationToken)
    {
        if (!context.Outbox.Baselined && !await Baseline(context, client, cancellationToken))
        {
            return;
        }

        for (var batch = 0; batch < MaximumBatchesPerFlush; batch++)
        {
            var pending = context.Outbox.Pending(WatchdogProtocol.MaximumBatchSize);
            if (pending.Count == 0)
            {
                return;
            }

            var answer = await client.Events(context.Credentials, new WatchdogEventBatch(pending), cancellationToken);
            if (!context.Succeeded(answer, "delivering events") || answer.Value is not { } receipt)
            {
                return;
            }

            var before = context.Outbox.Depth;
            if (receipt.Watermark > context.Outbox.LastAssigned)
            {
                context.Outbox.Rebase(receipt.Watermark);
            }
            else
            {
                context.Outbox.Acknowledge(receipt.Watermark);
            }

            context.Delivered += Math.Max(before - context.Outbox.Depth, 0);
            if (receipt.Gaps is { Count: > 0 } gaps)
            {
                context.Problems.Add($"Direct recorded {gaps.Count} gap(s) in the outbox sequence");
            }

            if (context.Outbox.Depth >= before)
            {
                context.Problems.Add("Direct did not advance its watermark; the rest of the outbox stays queued");
                return;
            }
        }
    }
}
