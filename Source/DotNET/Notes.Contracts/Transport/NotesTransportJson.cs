// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using Cratis.Json;

namespace Cratis.Showrunner.Notes.Contracts.Transport;

/// <summary>
/// The JSON options both ends of the HTTP transport use, so the content of a contract event round-trips: camelCase
/// members, concepts as their underlying value and enumerations as their names.
/// </summary>
public static class NotesTransportJson
{
    /// <summary>
    /// Gets the options. They are shared and must not be changed.
    /// </summary>
    public static JsonSerializerOptions Options { get; } = Create();

    static JsonSerializerOptions Create()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new ConceptAsJsonConverterFactory());
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
