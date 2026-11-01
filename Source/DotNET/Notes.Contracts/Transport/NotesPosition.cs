// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.Notes.Contracts.Transport;

/// <summary>
/// The answer to which note the collector stored last.
/// </summary>
/// <param name="SequenceNumber">The highest outbox sequence number the collector stored for the publication; absent when it stored none.</param>
public record NotesPosition(ulong? SequenceNumber);
