// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cratis.Showrunner.Operations.Link.Protocol;

/// <summary>
/// The serialization both ends of the Direct watchdog protocol use - web defaults (camelCase, case-insensitive reads) with
/// enums as strings, so an added enum member never silently shifts a number on the wire.
/// </summary>
/// <remarks>
/// Re-declared from Direct's <c>Operations.Contracts</c> (see <c>Documentation/Operations/index.md</c>) - the shape and the
/// options must stay identical, because the signed configuration is verified over this serialization.
/// </remarks>
public static class WatchdogJson
{
    /// <summary>
    /// Gets the options.
    /// </summary>
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        options.MakeReadOnly(populateMissingResolver: true);
        return options;
    }
}
