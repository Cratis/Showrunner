// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Understudies.Catalog;

/// <summary>
/// One file of a backup run.
/// </summary>
/// <param name="Type">What the file holds.</param>
/// <param name="Database">The database it holds, or null for a file that holds no database, such as encrypted keys.</param>
/// <param name="File">The name of the file, beside the manifest.</param>
/// <param name="SizeBytes">The size of the file.</param>
/// <param name="Sha256">The SHA-256 of the file as lowercase hexadecimal, to check a download or a restore.</param>
public record ManifestEntry(BackupType Type, string? Database, string File, long SizeBytes, string Sha256);
