// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// What Direct took from a batch.
/// </summary>
/// <param name="Watermark">The highest sequence Direct has durably taken - the instance may drop every entry up to it.</param>
/// <param name="Accepted">How many entries were new.</param>
/// <param name="Duplicates">How many had already been taken.</param>
/// <param name="Rejected">How many could not be read or recorded and were skipped - they will not be asked for again.</param>
/// <param name="Gaps">The gaps this batch revealed.</param>
public record WatchdogEventBatchReceipt(long Watermark, int Accepted, int Duplicates, int Rejected, IReadOnlyList<WatchdogSequenceGap> Gaps);
